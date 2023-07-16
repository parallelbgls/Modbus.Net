#region Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved
// Web: https://www.technosoftware.com 
// 
// The source code in this file is covered under a dual-license scenario:
//   - Owner of a purchased license: SCLA 1.0
//   - GPL V3: everybody else
//
// SCLA license terms accompanied with this source code.
// See SCLA 1.0: https://technosoftware.com/license/Source_Code_License_Agreement.pdf
//
// GNU General Public License as published by the Free Software Foundation;
// version 3 of the License are accompanied with this source code.
// See https://technosoftware.com/license/GPLv3License.txt
//
// This source code is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved
#region Using Directives
using System;
using System.Collections;
using System.Runtime.InteropServices;
using Technosoftware.DaAeHdaClient.Com.Da;
using Technosoftware.DaAeHdaClient.Com.Utilities;
using Technosoftware.DaAeHdaClient.Da;
using Technosoftware.DaAeHdaClient.Utilities;
using Technosoftware.OpcRcw.Comn;
using Technosoftware.OpcRcw.Da;

#endregion

namespace Technosoftware.DaAeHdaClient.Com.Da20
{
    /// <summary>
    /// An in-process wrapper for a remote OPC Data Access 2.0X server.
    /// </summary>
    internal class Server : Da.Server
    {

        #region Constructors
        /// <summary>
        /// The default constructor for the object.
        /// </summary>
        internal Server() { }

        /// <summary>
        /// Initializes the object with the specified COM server.
        /// </summary>
        internal Server(OpcUrl url, object unknown) : base(url, unknown) { }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// This must be called explicitly by clients to ensure the COM server is released.
        /// </summary>
        public new void Dispose()
        {
            lock (this)
            {
                if (subscription_ != null)
                {
                    var methodName = "IOPCServer.RemoveGroup";
                    try
                    {
                        var server = BeginComCall<IOPCServer>(methodName, true);
                        server.RemoveGroup(groupHandle_, 0);
                    }
                    catch (Exception e)
                    {
                        ComCallError(methodName, e);
                    }
                    finally
                    {
                        EndComCall(methodName);
                    }

                    Utilities.Interop.ReleaseServer(subscription_);
                    subscription_ = null;
                    groupHandle_ = 0;

                    base.Dispose();
                }
            }
        }
        #endregion

        //======================================================================
        // Connection Management

        /// <summary>
        /// Connects to the server with the specified URL and credentials.
        /// </summary>
        public override void Initialize(OpcUrl url, OpcConnectData connectData)
        {
            lock (this)
            {
                // connect to server.
                base.Initialize(url, connectData);

                separators_ = null;
                var methodName = "IOPCCommon.GetLocaleID";

                // create a global subscription required for various item level operations.
                var localeID = 0;
                try
                {
                    // get the default locale for the server.
                    var server = BeginComCall<IOPCCommon>(methodName, true);
                    server.GetLocaleID(out localeID);
                }
                catch (Exception e)
                {
                    Uninitialize();
                    ComCallError(methodName, e);
                    throw Utilities.Interop.CreateException(methodName, e);
                }
                finally
                {
                    EndComCall(methodName);
                }

                // create a global subscription required for various item level operations.
                methodName = "IOPCServer.AddGroup";
                try
                {
                    // add the subscription.
                    var iid = typeof(IOPCItemMgt).GUID;

                    var server = BeginComCall<IOPCServer>(methodName, true);
                    ((IOPCServer)server).AddGroup(
                        "",
                        1,
                        500,
                        0,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        localeID,
                        out groupHandle_,
                        out var revisedUpdateRate,
                        ref iid,
                        out subscription_);

                    if (DCOMCallWatchdog.IsCancelled)
                    {
                        throw new Exception($"{methodName} call was cancelled due to response timeout");
                    }
                }
                catch (Exception e)
                {
                    Uninitialize();
                    ComCallError(methodName, e);
                    throw Utilities.Interop.CreateException(methodName, e);
                }
                finally
                {
                    EndComCall(methodName);
                }
            }
        }

        //======================================================================
        // Private Members

        /// <summary>
        /// A global subscription used for various item level operations. 
        /// </summary>
        private object subscription_ = null;

        /// <summary>
        /// The server handle for the global subscription.
        /// </summary>
        private int groupHandle_ = 0;

        /// <summary>
        /// True if BROWSE_TO is supported; otherwise false.
        /// </summary>
        private bool browseToSupported_ = true;

        /// <summary>
        /// A list of seperators used in the browse paths.
        /// </summary>
        private char[] separators_ = null;
        private readonly object separatorsLock_ = new();

        //======================================================================
        // Read

        /// <summary>
        /// Reads the values for the specified items.
        /// </summary>
        public override TsCDaItemValueResult[] Read(TsCDaItem[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            // check if nothing to do.
            if (items.Length == 0)
            {
                return Array.Empty<TsCDaItemValueResult>();
            }

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // create temporary items.
                var temporaryItems = AddItems(items);
                var results = new TsCDaItemValueResult[items.Length];

                try
                {
                    // construct return values.
                    var cacheItems = new ArrayList(items.Length);
                    var cacheResults = new ArrayList(items.Length);
                    var deviceItems = new ArrayList(items.Length);
                    var deviceResults = new ArrayList(items.Length);

                    for (var ii = 0; ii < items.Length; ii++)
                    {
                        results[ii] = new TsCDaItemValueResult(temporaryItems[ii]);

                        if (temporaryItems[ii].Result.Failed())
                        {
                            results[ii].Result = temporaryItems[ii].Result;
                            results[ii].DiagnosticInfo = temporaryItems[ii].DiagnosticInfo;
                            continue;
                        }

                        if (items[ii].MaxAgeSpecified && (items[ii].MaxAge < 0 || items[ii].MaxAge == int.MaxValue))
                        {
                            cacheItems.Add(items[ii]);
                            cacheResults.Add(results[ii]);
                        }
                        else
                        {
                            deviceItems.Add(items[ii]);
                            deviceResults.Add(results[ii]);
                        }
                    }

                    // read values from the cache.
                    if (cacheResults.Count > 0)
                    {
                        var methodName = "IOPCItemMgt.SetActiveState";
                        // items must be active for cache reads.
                        try
                        {
                            // create list of server handles.
                            var serverHandles = new int[cacheResults.Count];

                            for (var ii = 0; ii < cacheResults.Count; ii++)
                            {
                                serverHandles[ii] = (int)((TsCDaItemValueResult)cacheResults[ii]).ServerHandle;
                            }

                            var pErrors = IntPtr.Zero;

                            var subscription = BeginComCall<IOPCItemMgt>(subscription_, methodName, true);
                            subscription.SetActiveState(
                                cacheResults.Count,
                                serverHandles,
                                1,
                                out pErrors);

                            // free error array.
                            Marshal.FreeCoTaskMem(pErrors);

                            if (DCOMCallWatchdog.IsCancelled)
                            {
                                throw new Exception($"{methodName} call was cancelled due to response timeout");
                            }
                        }
                        catch (Exception e)
                        {
                            ComCallError(methodName, e);
                            throw Utilities.Interop.CreateException(methodName, e);
                        }
                        finally
                        {
                            EndComCall(methodName);
                        }

                        // read the values.
                        ReadValues(
                            (TsCDaItem[])cacheItems.ToArray(typeof(TsCDaItem)),
                            (TsCDaItemValueResult[])cacheResults.ToArray(typeof(TsCDaItemValueResult)),
                            true);
                    }

                    // read values from the device.
                    if (deviceResults.Count > 0)
                    {
                        ReadValues(
                            (TsCDaItem[])deviceItems.ToArray(typeof(TsCDaItem)),
                            (TsCDaItemValueResult[])deviceResults.ToArray(typeof(TsCDaItemValueResult)),
                            false);
                    }
                }

                // remove temporary items after read.
                finally
                {
                    RemoveItems(temporaryItems);
                }

                // return results.
                return results;
            }
        }

        //======================================================================
        // Write

        /// <summary>
        /// Write the values for the specified items.
        /// </summary>
        public override OpcItemResult[] Write(TsCDaItemValue[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            // check if nothing to do.
            if (items.Length == 0)
            {
                return Array.Empty<OpcItemResult>();
            }

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // create item objects to add temporary items.
                var groupItems = new TsCDaItem[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    groupItems[ii] = new TsCDaItem(items[ii]);
                }

                // create temporary items.
                var results = AddItems(groupItems);

                try
                {
                    // construct list of valid items to write.
                    var writeItems = new ArrayList(items.Length);
                    var writeValues = new ArrayList(items.Length);

                    for (var ii = 0; ii < items.Length; ii++)
                    {
                        if (results[ii].Result.Failed())
                        {
                            continue;
                        }

                        if (items[ii].QualitySpecified || items[ii].TimestampSpecified)
                        {
                            results[ii].Result = OpcResult.Da.E_NO_WRITEQT;
                            results[ii].DiagnosticInfo = null;
                            continue;
                        }

                        writeItems.Add(results[ii]);
                        writeValues.Add(items[ii]);
                    }

                    // read values from the cache.
                    if (writeItems.Count > 0)
                    {
                        // initialize input parameters.
                        var serverHandles = new int[writeItems.Count];
                        var values = new object[writeItems.Count];

                        for (var ii = 0; ii < serverHandles.Length; ii++)
                        {
                            serverHandles[ii] = (int)((OpcItemResult)writeItems[ii]).ServerHandle;
                            values[ii] = Utilities.Interop.GetVARIANT(((TsCDaItemValue)writeValues[ii]).Value);
                        }

                        var pErrors = IntPtr.Zero;

                        // write item values.
                        var methodName = "IOPCSyncIO.Write";
                        try
                        {
                            var subscription = BeginComCall<IOPCSyncIO>(subscription_, methodName, true);
                            subscription.Write(
                                writeItems.Count,
                                serverHandles,
                                values,
                                out pErrors);

                            if (DCOMCallWatchdog.IsCancelled)
                            {
                                throw new Exception($"{methodName} call was cancelled due to response timeout");
                            }
                        }
                        catch (Exception e)
                        {
                            ComCallError(methodName, e);
                            throw Utilities.Interop.CreateException(methodName, e);
                        }
                        finally
                        {
                            EndComCall(methodName);
                        }

                        // unmarshal results.
                        var errors = Utilities.Interop.GetInt32s(ref pErrors, writeItems.Count, true);

                        for (var ii = 0; ii < writeItems.Count; ii++)
                        {
                            var result = (OpcItemResult)writeItems[ii];

                            result.Result = Utilities.Interop.GetResultId(errors[ii]);
                            result.DiagnosticInfo = null;

                            // convert COM code to unified DA code.
                            if (errors[ii] == Result.E_BADRIGHTS) { results[ii].Result = new OpcResult(OpcResult.Da.E_READONLY, Result.E_BADRIGHTS); }
                        }
                    }
                }

                // remove temporary items
                finally
                {
                    RemoveItems(results);
                }

                // return results.
                return results;
            }
        }

        //======================================================================
        // Browse

        /// <summary>
        /// Fetches child elements of the specified branch which match the filter criteria.
        /// </summary>
        public override TsCDaBrowseElement[] Browse(
            OpcItem itemId,
            TsCDaBrowseFilters filters,
            out TsCDaBrowsePosition position)
        {
            if (filters == null) throw new ArgumentNullException(nameof(filters));

            position = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                BrowsePosition pos = null;

                var elements = new ArrayList();

                // search for child branches.
                if (filters.BrowseFilter != TsCDaBrowseFilter.Item)
                {
                    var branches = GetElements(elements.Count, itemId, filters, true, ref pos);

                    if (branches != null)
                    {
                        elements.AddRange(branches);
                    }

                    position = pos;

                    // return current set if browse halted.
                    if (position != null)
                    {
                        return (TsCDaBrowseElement[])elements.ToArray(typeof(TsCDaBrowseElement));
                    }
                }

                // search for child items.
                if (filters.BrowseFilter != TsCDaBrowseFilter.Branch)
                {
                    var items = GetElements(elements.Count, itemId, filters, false, ref pos);

                    if (items != null)
                    {
                        elements.AddRange(items);
                    }

                    position = pos;
                }

                // return the elements.
                return (TsCDaBrowseElement[])elements.ToArray(typeof(TsCDaBrowseElement));
            }
        }

        //======================================================================
        // BrowseNext

        /// <summary>
        /// Continues a browse operation with previously specified search criteria.
        /// </summary>
		public override TsCDaBrowseElement[] BrowseNext(ref TsCDaBrowsePosition position)
        {
            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // check for valid browse position object.
                if (position == null && position.GetType() != typeof(BrowsePosition))
                {
                    throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
                }

                var pos = (BrowsePosition)position;

                var itemID = pos.ItemID;
                var filters = pos.Filters;

                var elements = new ArrayList();

                // search for child branches.
                if (pos.IsBranch)
                {
                    var branches = GetElements(elements.Count, itemID, filters, true, ref pos);

                    if (branches != null)
                    {
                        elements.AddRange(branches);
                    }

                    position = pos;

                    // return current set if browse halted.
                    if (position != null)
                    {
                        return (TsCDaBrowseElement[])elements.ToArray(typeof(TsCDaBrowseElement));
                    }
                }

                // search for child items.
                if (filters.BrowseFilter != TsCDaBrowseFilter.Branch)
                {
                    var items = GetElements(elements.Count, itemID, filters, false, ref pos);

                    if (items != null)
                    {
                        elements.AddRange(items);
                    }

                    position = pos;
                }

                // return the elements.
                return (TsCDaBrowseElement[])elements.ToArray(typeof(TsCDaBrowseElement));
            }
        }

        //======================================================================
        // GetProperties

        /// <summary>
        /// Returns the specified properties for a set of items.
        /// </summary>
        public override TsCDaItemPropertyCollection[] GetProperties(
            OpcItem[] itemIds,
            TsDaPropertyID[] propertyIDs,
            bool returnValues)
        {
            if (itemIds == null) throw new ArgumentNullException(nameof(itemIds));

            // check for trival case.
            if (itemIds.Length == 0)
            {
                return Array.Empty<TsCDaItemPropertyCollection>();
            }

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // initialize list of property lists.
                var propertyLists = new TsCDaItemPropertyCollection[itemIds.Length];

                for (var ii = 0; ii < itemIds.Length; ii++)
                {
                    propertyLists[ii] = new TsCDaItemPropertyCollection
                    {
                        ItemName = itemIds[ii].ItemName,
                        ItemPath = itemIds[ii].ItemPath
                    };

                    // fetch properties for item.
                    try
                    {
                        var properties = GetProperties(itemIds[ii].ItemName, propertyIDs, returnValues);

                        if (properties != null)
                        {
                            propertyLists[ii].AddRange(properties);
                        }

                        propertyLists[ii].Result = OpcResult.S_OK;
                    }
                    catch (OpcResultException e)
                    {
                        propertyLists[ii].Result = e.Result;
                    }
                    catch (Exception e)
                    {
                        propertyLists[ii].Result = new OpcResult(Marshal.GetHRForException(e));
                    }
                }

                // return property lists.
                return propertyLists;
            }
        }

        //======================================================================
        // Private Methods

        /// <summary>
        /// Adds a set of temporary items used for a read/write operation.
        /// </summary>
        private OpcItemResult[] AddItems(TsCDaItem[] items)
        {
            // add items to subscription.
            var count = items.Length;

            var definitions = Technosoftware.DaAeHdaClient.Com.Da.Interop.GetOPCITEMDEFs(items);

            // ensure all items are created as inactive.
            for (var ii = 0; ii < definitions.Length; ii++)
            {
                definitions[ii].bActive = 0;
            }

            // initialize output parameters.
            var pResults = IntPtr.Zero;
            var pErrors = IntPtr.Zero;

            // get the default current for the server.
            ((IOPCCommon)server_).GetLocaleID(out var localeID);

            var hLocale = GCHandle.Alloc(localeID, GCHandleType.Pinned);

            var methodName = "IOPCGroupStateMgt.SetState";
            try
            {
                // ensure the current locale is correct.
                var subscription = BeginComCall<IOPCGroupStateMgt>(subscription_, methodName, true);
                ((IOPCGroupStateMgt)subscription).SetState(
                    IntPtr.Zero,
                    out var updateRate,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    hLocale.AddrOfPinnedObject(),
                    IntPtr.Zero);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new Exception($"{methodName} call was cancelled due to response timeout");
                }
            }
            catch (Exception e)
            {
                ComCallError(methodName, e);
                throw Utilities.Interop.CreateException(methodName, e);
            }
            finally
            {
                if (hLocale.IsAllocated) hLocale.Free();
                EndComCall(methodName);
            }

            // add items to subscription.
            methodName = "IOPCItemMgt.AddItems";
            try
            {
                var subscription = BeginComCall<IOPCItemMgt>(subscription_, methodName, true);
                subscription.AddItems(
                    count,
                    definitions,
                    out pResults,
                    out pErrors);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new Exception($"{methodName} call was cancelled due to response timeout");
                }
            }
            catch (Exception e)
            {
                ComCallError(methodName, e);
                throw Utilities.Interop.CreateException(methodName, e);
            }
            finally
            {
                EndComCall(methodName);
                if (hLocale.IsAllocated) hLocale.Free();
            }

            // unmarshal output parameters.
            var serverHandles = Technosoftware.DaAeHdaClient.Com.Da.Interop.GetItemResults(ref pResults, count, true);
            var errors = Utilities.Interop.GetInt32s(ref pErrors, count, true);

            // create results list.
            var results = new OpcItemResult[count];

            for (var ii = 0; ii < count; ii++)
            {
                results[ii] = new OpcItemResult(items[ii])
                {
                    ServerHandle = null,
                    Result = Utilities.Interop.GetResultId(errors[ii]),
                    DiagnosticInfo = null
                };

                if (results[ii].Result.Succeeded())
                {
                    results[ii].ServerHandle = serverHandles[ii];
                }
            }

            // return results.
            return results;
        }

        /// <summary>
        /// Removes a set of temporary items used for a read/write operation.
        /// </summary>
        private void RemoveItems(OpcItemResult[] items)
        {
            try
            {
                // contruct array of valid server handles.
                var handles = new ArrayList(items.Length);

                foreach (var item in items)
                {
                    if (item.Result.Succeeded() && item.ServerHandle.GetType() == typeof(int))
                    {
                        handles.Add((int)item.ServerHandle);
                    }
                }

                // check if nothing to do.
                if (handles.Count == 0)
                {
                    return;
                }

                // remove items from server.
                var pErrors = IntPtr.Zero;

                var methodName = "IOPCItemMgt.RemoveItems";
                try
                {
                    var subscription = BeginComCall<IOPCItemMgt>(subscription_, methodName, true);
                    ((IOPCItemMgt)subscription).RemoveItems(
                        handles.Count,
                        (int[])handles.ToArray(typeof(int)),
                        out pErrors);

                    if (DCOMCallWatchdog.IsCancelled)
                    {
                        throw new Exception($"{methodName} call was cancelled due to response timeout");
                    }

                }
                catch (Exception e)
                {
                    ComCallError(methodName, e);
                    throw Utilities.Interop.CreateException(methodName, e);
                }
                finally
                {
                    EndComCall(methodName);
                    // free returned error array.
                    Utilities.Interop.GetInt32s(ref pErrors, handles.Count, true);
                }

            }
            catch
            {
                // ignore errors.
            }
        }

        /// <summary>
        /// Reads a set of values.
        /// </summary>
        private void ReadValues(TsCDaItem[] items, TsCDaItemValueResult[] results, bool cache)
        {
            if (items.Length == 0 || results.Length == 0) return;

            // marshal input parameters.
            var serverHandles = new int[results.Length];

            for (var ii = 0; ii < results.Length; ii++)
            {
                serverHandles[ii] = Convert.ToInt32(results[ii].ServerHandle);
            }

            // initialize output parameters.
            var pValues = IntPtr.Zero;
            var pErrors = IntPtr.Zero;

            var methodName = "IOPCSyncIO.Read";
            try
            {
                var subscription = BeginComCall<IOPCSyncIO>(subscription_, methodName, true);
                subscription.Read(
                    (cache) ? OPCDATASOURCE.OPC_DS_CACHE : OPCDATASOURCE.OPC_DS_DEVICE,
                    results.Length,
                    serverHandles,
                    out pValues,
                    out pErrors);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new Exception($"{methodName} call was cancelled due to response timeout");
                }
            }
            catch (Exception e)
            {
                ComCallError(methodName, e);
                throw Utilities.Interop.CreateException(methodName, e);
            }
            finally
            {
                EndComCall(methodName);
                // free returned error array.
            }

            // unmarshal output parameters.
            var values = Technosoftware.DaAeHdaClient.Com.Da.Interop.GetItemValues(ref pValues, results.Length, true);
            var errors = Utilities.Interop.GetInt32s(ref pErrors, results.Length, true);

            // pre-fetch the current locale to use for data conversions.
            GetLocale();

            // construct results list.
            for (var ii = 0; ii < results.Length; ii++)
            {
                results[ii].Result = Utilities.Interop.GetResultId(errors[ii]);
                results[ii].DiagnosticInfo = null;

                if (results[ii].Result.Succeeded())
                {
                    results[ii].Value = values[ii].Value;
                    results[ii].Quality = values[ii].Quality;
                    results[ii].QualitySpecified = values[ii].QualitySpecified;
                    results[ii].Timestamp = values[ii].Timestamp;
                    results[ii].TimestampSpecified = values[ii].TimestampSpecified;
                }

                // convert COM code to unified DA code.
                if (errors[ii] == Result.E_BADRIGHTS) { results[ii].Result = new OpcResult(OpcResult.Da.E_WRITEONLY, Result.E_BADRIGHTS); }

                // convert the data type since the server does not support the feature.
                if (results[ii].Value != null && items[ii].ReqType != null)
                {
                    try
                    {
                        results[ii].Value = ChangeType(results[ii].Value, items[ii].ReqType, "en-US");
                    }
                    catch (Exception e)
                    {
                        results[ii].Value = null;
                        results[ii].Quality = TsCDaQuality.Bad;
                        results[ii].QualitySpecified = true;
                        results[ii].Timestamp = DateTime.MinValue;
                        results[ii].TimestampSpecified = false;

                        if (e.GetType() == typeof(OverflowException))
                        {
                            results[ii].Result = Utilities.Interop.GetResultId(Result.E_RANGE);
                        }
                        else
                        {
                            results[ii].Result = Utilities.Interop.GetResultId(Result.E_BADTYPE);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the set of available properties for the item.
        /// </summary>
        private TsCDaItemProperty[] GetAvailableProperties(string itemID)
        {
            // validate argument.
            if (itemID == null || itemID.Length == 0)
            {
                throw new OpcResultException(OpcResult.Da.E_INVALID_ITEM_NAME);
            }

            // query for available properties.
            var count = 0;

            var pPropertyIDs = IntPtr.Zero;
            var pDescriptions = IntPtr.Zero;
            var pDataTypes = IntPtr.Zero;

            var methodName = "IOPCItemProperties.QueryAvailableProperties";
            try
            {
                var server = BeginComCall<IOPCItemProperties>(methodName, true);
                server.QueryAvailableProperties(
                    itemID,
                    out count,
                    out pPropertyIDs,
                    out pDescriptions,
                    out pDataTypes);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new Exception($"{methodName} call was cancelled due to response timeout");
                }

            }
            catch (Exception e)
            {
                ComCallError(methodName, e);
                throw new OpcResultException(OpcResult.Da.E_UNKNOWN_ITEM_NAME);
            }
            finally
            {
                EndComCall(methodName);
                // free returned error array.                
            }

            // unmarshal results.
            var propertyIDs = Utilities.Interop.GetInt32s(ref pPropertyIDs, count, true);
            var datatypes = Utilities.Interop.GetInt16s(ref pDataTypes, count, true);
            var descriptions = Utilities.Interop.GetUnicodeStrings(ref pDescriptions, count, true);

            // check for error condition.
            if (count == 0)
            {
                return null;
            }

            // initialize property objects.
            var properties = new TsCDaItemProperty[count];

            for (var ii = 0; ii < count; ii++)
            {
                properties[ii] = new TsCDaItemProperty
                {
                    ID = Technosoftware.DaAeHdaClient.Com.Da.Interop.GetPropertyID(propertyIDs[ii]),
                    Description = descriptions[ii],
                    DataType = Utilities.Interop.GetType((VarEnum)datatypes[ii]),
                    ItemName = null,
                    ItemPath = null,
                    Result = OpcResult.S_OK,
                    Value = null
                };
            }

            // return property list.
            return properties;
        }

        /// <summary>
        /// Fetches the property item id for the specified set of properties.
        /// </summary>
        private void GetItemIDs(string itemID, TsCDaItemProperty[] properties)
        {
            try
            {
                // create input arguments;
                var propertyIDs = new int[properties.Length];

                for (var ii = 0; ii < properties.Length; ii++)
                {
                    propertyIDs[ii] = properties[ii].ID.Code;
                }

                // lookup item ids.
                var pItemIDs = IntPtr.Zero;
                var pErrors = IntPtr.Zero;

                ((IOPCItemProperties)server_).LookupItemIDs(
                    itemID,
                    properties.Length,
                    propertyIDs,
                    out pItemIDs,
                    out pErrors);

                // unmarshal results.
                var itemIDs = Utilities.Interop.GetUnicodeStrings(ref pItemIDs, properties.Length, true);
                var errors = Utilities.Interop.GetInt32s(ref pErrors, properties.Length, true);

                // update property objects.
                for (var ii = 0; ii < properties.Length; ii++)
                {
                    properties[ii].ItemName = null;
                    properties[ii].ItemPath = null;

                    if (errors[ii] >= 0)
                    {
                        properties[ii].ItemName = itemIDs[ii];
                    }
                }
            }
            catch (Exception)
            {
                // set item ids to null for all properties.
                foreach (var property in properties)
                {
                    property.ItemName = null;
                    property.ItemPath = null;
                }
            }
        }

        /// <summary>
        /// Fetches the property values for the specified set of properties.
        /// </summary>
        private void GetValues(string itemID, TsCDaItemProperty[] properties)
        {
            try
            {
                // create input arguments;
                var propertyIDs = new int[properties.Length];

                for (var ii = 0; ii < properties.Length; ii++)
                {
                    propertyIDs[ii] = properties[ii].ID.Code;
                }

                // lookup item ids.
                var pValues = IntPtr.Zero;
                var pErrors = IntPtr.Zero;

                ((IOPCItemProperties)server_).GetItemProperties(
                    itemID,
                    properties.Length,
                    propertyIDs,
                    out pValues,
                    out pErrors);

                // unmarshal results.
                var values = Interop.GetVARIANTs(ref pValues, properties.Length, true);
                var errors = Utilities.Interop.GetInt32s(ref pErrors, properties.Length, true);

                // update property objects.
                for (var ii = 0; ii < properties.Length; ii++)
                {
                    properties[ii].Value = null;

                    // ignore value for invalid properties.
                    if (!properties[ii].Result.Succeeded())
                    {
                        continue;
                    }

                    properties[ii].Result = Utilities.Interop.GetResultId(errors[ii]);

                    // substitute property reult code.
                    if (errors[ii] == Result.E_BADRIGHTS)
                    {
                        properties[ii].Result = new OpcResult(OpcResult.Da.E_WRITEONLY, Result.E_BADRIGHTS);
                    }

                    if (properties[ii].Result.Succeeded())
                    {
                        properties[ii].Value = Technosoftware.DaAeHdaClient.Com.Da.Interop.UnmarshalPropertyValue(properties[ii].ID, values[ii]);
                    }
                }
            }
            catch (Exception e)
            {
                // set general error code as the result for each property.
                var result = new OpcResult(Marshal.GetHRForException(e));

                foreach (var property in properties)
                {
                    property.Value = null;
                    property.Result = result;
                }
            }
        }

        /// <summary>
        /// Gets the specified properties for the specified item.
        /// </summary>
        private TsCDaItemProperty[] GetProperties(string itemID, TsDaPropertyID[] propertyIDs, bool returnValues)
        {
            TsCDaItemProperty[] properties;

            // return all available properties.
            if (propertyIDs == null)
            {
                properties = GetAvailableProperties(itemID);
            }

            // return on the selected properties.
            else
            {
                // get available properties.
                var availableProperties = GetAvailableProperties(itemID);

                // initialize result list.
                properties = new TsCDaItemProperty[propertyIDs.Length];

                for (var ii = 0; ii < propertyIDs.Length; ii++)
                {
                    // search available property list for specified property.
                    foreach (var property in availableProperties)
                    {
                        if (property.ID == propertyIDs[ii])
                        {
                            properties[ii] = (TsCDaItemProperty)property.Clone();
                            properties[ii].ID = propertyIDs[ii];
                            break;
                        }
                    }

                    // property not valid for the item.
                    if (properties[ii] == null)
                    {
                        properties[ii] = new TsCDaItemProperty
                        {
                            ID = propertyIDs[ii],
                            Result = OpcResult.Da.E_INVALID_PID
                        };
                    }
                }
            }

            // fill in missing fields in property objects.
            if (properties != null)
            {
                GetItemIDs(itemID, properties);

                if (returnValues)
                {
                    GetValues(itemID, properties);
                }
            }

            // return property list.
            return properties;
        }


        /// <summary>
        /// Returns an enumerator for the children of the specified branch.
        /// </summary>
        private EnumString GetEnumerator(string itemID, TsCDaBrowseFilters filters, bool branches, bool flat)
        {
            var browser = (IOPCBrowseServerAddressSpace)server_;

            if (!flat)
            {
                if (itemID == null)
                {
                    if (browseToSupported_)
                    {
                        // move to the root of the hierarchial address spaces.
                        try
                        {
                            var id = string.Empty;
                            browser.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_TO, id);
                        }
                        catch (Exception e)
                        {
                            var message = string.Format("ChangeBrowsePosition to root with BROWSE_TO={0} failed with error {1}. BROWSE_TO not supported.", string.Empty, e.Message);
                            Utils.Trace(e, message);
                            browseToSupported_ = false;
                        }
                    }
                    if (!browseToSupported_)
                    {
                        // browse to root.
                        while (true)
                        {
                            try
                            {
                                browser.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_UP, string.Empty);
                            }
                            catch (Exception)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // move to the specified branch for hierarchial address spaces.
                    var id = itemID ?? "";
                    if (browseToSupported_)
                    {
                        try
                        {
                            browser.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_TO, id);
                        }
                        catch (Exception)
                        {
                            browseToSupported_ = false;
                        }
                    }
                    if (!browseToSupported_)
                    {
                        // try to browse down instead.
                        try
                        {
                            browser.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_DOWN, id);
                        }
                        catch (Exception)
                        {

                            // browse to root.
                            while (true)
                            {
                                try
                                {
                                    browser.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_UP, string.Empty);
                                }
                                catch (Exception)
                                {
                                    break;
                                }
                            }

                            // parse the browse path.
                            string[] paths = null;

                            lock (separatorsLock_)
                            {
                                if (separators_ != null)
                                {
                                    paths = id.Split(separators_);
                                }
                                else
                                {
                                    paths = id.Split(separators_);
                                }
                            }

                            // browse to correct location.
                            for (var ii = 0; ii < paths.Length; ii++)
                            {
                                if (paths[ii] == null || paths[ii].Length == 0)
                                {
                                    continue;
                                }

                                try
                                {
                                    browser.ChangeBrowsePosition(OPCBROWSEDIRECTION.OPC_BROWSE_DOWN, paths[ii]);
                                }
                                catch (Exception)
                                {
                                    throw new OpcResultException(OpcResult.Da.E_UNKNOWN_ITEM_NAME);
                                }
                            }
                        }
                    }
                }
            }

            try
            {
                // create the enumerator.
                var browseType = (branches) ? OPCBROWSETYPE.OPC_BRANCH : OPCBROWSETYPE.OPC_LEAF;

                if (flat)
                {
                    browseType = OPCBROWSETYPE.OPC_FLAT;
                }

                browser.BrowseOPCItemIDs(
                    browseType,
                    filters.ElementNameFilter ?? "",
                    (short)VarEnum.VT_EMPTY,
                    0,
                    out var enumerator);

                // return the enumerator.
                return new EnumString(enumerator);
            }
            catch (Exception)
            {
                throw new OpcResultException(OpcResult.Da.E_UNKNOWN_ITEM_NAME);
            }
        }


        /// <summary>
        /// Detects the separators used in the item id.
        /// </summary>
        private void DetectAndSaveSeparators(string browseName, string itemID)
        {
            if (!itemID.EndsWith(browseName))
            {
                return;
            }

            if (string.Compare(itemID, browseName, true) == 0)
            {
                return;
            }

            var separator = itemID[itemID.Length - browseName.Length - 1];

            lock (separatorsLock_)
            {
                var index = -1;

                if (separators_ != null)
                {
                    for (var ii = 0; ii < separators_.Length; ii++)
                    {
                        if (separators_[ii] == separator)
                        {
                            index = ii;
                            break;
                        }
                    }

                    if (index == -1)
                    {
                        var separators = new char[separators_.Length + 1];
                        Array.Copy(separators_, separators, separators_.Length);
                        separators_ = separators;
                    }
                }

                if (index == -1)
                {
                    separators_ ??= new char[1];

                    separators_[separators_.Length - 1] = separator;
                }
            }
        }

        /// <summary>
        /// Reads a single value from the enumerator and returns a browse element.
        /// </summary>
        private TsCDaBrowseElement GetElement(
                OpcItem itemID,
                string name,
                TsCDaBrowseFilters filters,
                bool isBranch)
        {
            if (name == null)
            {
                return null;
            }

            var element = new TsCDaBrowseElement
            {
                Name = name,
                HasChildren = isBranch,
                ItemPath = null
            };

            // get item id.
            try
            {
                ((IOPCBrowseServerAddressSpace)server_).GetItemID(element.Name, out var itemName);
                element.ItemName = itemName;

                // detect separator.
                if (element.ItemName != null)
                {
                    DetectAndSaveSeparators(element.Name, element.ItemName);
                }
            }

            // this is an error that should not occur.
            catch
            {
                element.ItemName = name;
            }

            // check if element is an actual item or just a branch.
            try
            {
                var definition = new OPCITEMDEF
                {
                    szItemID = element.ItemName,
                    szAccessPath = null,
                    hClient = 0,
                    bActive = 0,
                    vtRequestedDataType = (short)VarEnum.VT_EMPTY,
                    dwBlobSize = 0,
                    pBlob = IntPtr.Zero
                };

                var pResults = IntPtr.Zero;
                var pErrors = IntPtr.Zero;

                // validate item.
                ((IOPCItemMgt)subscription_).ValidateItems(
                    1,
                    new OPCITEMDEF[] { definition },
                    0,
                    out pResults,
                    out pErrors);

                // free results.
                Technosoftware.DaAeHdaClient.Com.Da.Interop.GetItemResults(ref pResults, 1, true);

                var errors = Utilities.Interop.GetInt32s(ref pErrors, 1, true);

                // can only be an item if validation succeeded.
                element.IsItem = (errors[0] >= 0);
            }

            // this is an error that should not occur - must be a branch.
            catch
            {
                element.IsItem = false;
                // Because ABB Real-TPI server always return ItemName == null we use Name instead to fix browsing problem
                element.ItemName = element.Name;
            }


            // fetch item properties.
            try
            {
                if (filters.ReturnAllProperties)
                {
                    element.Properties = GetProperties(element.ItemName, null, filters.ReturnPropertyValues);
                }
                else if (filters.PropertyIDs != null)
                {
                    element.Properties = GetProperties(element.ItemName, filters.PropertyIDs, filters.ReturnPropertyValues);
                }
            }

            // return no properties if an error fetching properties occurred.
            catch
            {
                element.Properties = null;
            }

            // return new element.
            return element;
        }

        /// <summary>
        /// Returns a list of child elements that meet the filter criteria.
        /// </summary>
        private TsCDaBrowseElement[] GetElements(
            int elementsFound,
            OpcItem itemID,
            TsCDaBrowseFilters filters,
            bool branches,
            ref BrowsePosition position)
        {
            // get the enumerator.
            EnumString enumerator;
            if (position == null)
            {
                var browser = (IOPCBrowseServerAddressSpace)server_;

                // check the server address space type.
                OPCNAMESPACETYPE namespaceType;
                try
                {
                    browser.QueryOrganization(out namespaceType);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCBrowseServerAddressSpace.QueryOrganization", e);
                }

                // return an empty list if requesting branches for a flat address space.
                if (namespaceType == OPCNAMESPACETYPE.OPC_NS_FLAT)
                {
                    if (branches)
                    {
                        return Array.Empty<TsCDaBrowseElement>();
                    }

                    // check that root is browsed for flat address spaces.
                    if (itemID != null && itemID.ItemName != null && itemID.ItemName.Length > 0)
                    {
                        throw new OpcResultException(OpcResult.Da.E_UNKNOWN_ITEM_NAME);
                    }
                }

                // get the enumerator.
                enumerator = GetEnumerator(
                         itemID?.ItemName,
                         filters,
                         branches,
                         namespaceType == OPCNAMESPACETYPE.OPC_NS_FLAT);
            }
            else
            {
                enumerator = position.Enumerator;
            }

            var elements = new ArrayList();
            var start = 0;
            string[] names = null;

            // get cached name list.
            if (position != null)
            {
                start = position.Index;
                names = position.Names;
                position = null;
            }

            do
            {
                if (names != null)
                {
                    for (var ii = start; ii < names.Length; ii++)
                    {
                        // check if max returned elements is exceeded.
                        if (filters.MaxElementsReturned != 0 && filters.MaxElementsReturned == elements.Count + elementsFound)
                        {
                            position = new BrowsePosition(itemID, filters, enumerator, branches)
                            {
                                Names = names,
                                Index = ii
                            };
                            break;
                        }

                        // read elements one at a time.
                        // get next element.
                        var element = GetElement(itemID, names[ii], filters, branches);
                        if (element == null)
                        {
                            break;
                        }

                        // add element.
                        elements.Add(element);
                    }
                }

                // check if browse halted.
                if (position != null)
                {
                    break;
                }

                // fetch next element name.
                names = enumerator.Next(10);
                start = 0;
            }
            while (names != null && names.Length > 0);

            // free enumerator.
            if (position == null)
            {
                enumerator.Dispose();
            }

            // return list of elements.
            return (TsCDaBrowseElement[])elements.ToArray(typeof(TsCDaBrowseElement));
        }

        //======================================================================
        // Private Methods

        /// <summary>
        /// Creates a new instance of a subscription.
        /// </summary>
        protected override Technosoftware.DaAeHdaClient.Com.Da.Subscription CreateSubscription(
            object group,
            TsCDaSubscriptionState state,
            int filters)
        {
            return new Subscription(group, state, filters);
        }
    }

    /// <summary>
    /// Implements an object that handles multi-step browse operations for DA2.05 servers.
    /// </summary>
    [Serializable]
    internal class BrowsePosition : TsCDaBrowsePosition
    {
        /// <summary>
        /// The enumerator for a browse operation.
        /// </summary>
        internal EnumString Enumerator = null;

        /// <summary>
        /// Whether the current enumerator returns branches or leaves.
        /// </summary>
        internal bool IsBranch = true;

        /// <summary>
        /// The pre-fetched set of names.
        /// </summary>
        internal string[] Names = null;

        /// <summary>
        /// The current index in the pre-fetched names.
        /// </summary>
        internal int Index = 0;

        /// <summary>
        /// Initializes a browse position 
        /// </summary>
        internal BrowsePosition(
            OpcItem itemID,
            TsCDaBrowseFilters filters,
            EnumString enumerator,
            bool isBranch)
            :
            base(itemID, filters)
        {
            Enumerator = enumerator;
            IsBranch = isBranch;
        }

        /// <summary>
        /// Releases unmanaged resources held by the object.
        /// </summary>
        public override void Dispose()
        {
            if (Enumerator != null)
            {
                Enumerator.Dispose();
                Enumerator = null;
            }
        }

        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public override object Clone()
        {
            var clone = (BrowsePosition)MemberwiseClone();
            clone.Enumerator = Enumerator.Clone();
            return clone;
        }
    }
}
