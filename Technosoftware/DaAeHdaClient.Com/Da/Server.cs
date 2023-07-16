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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using Technosoftware.DaAeHdaClient.Com.Utilities;
using Technosoftware.DaAeHdaClient.Da;
using Technosoftware.OpcRcw.Da;

#endregion

namespace Technosoftware.DaAeHdaClient.Com.Da
{
    /// <summary>
    /// A .NET wrapper for a COM server that implements the DA server interfaces.
    /// </summary>
    internal class Server : Com.Server, ITsDaServer
    {
        #region Fields

        /// <summary>
        /// The default result filters for the server.
        /// </summary>
        private int filters_ = (int)TsCDaResultFilter.All | (int)TsCDaResultFilter.ClientHandle;

        /// <summary>
        /// A table of active subscriptions for the server.
        /// </summary>
        private readonly Hashtable subscriptions_ = new Hashtable();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the object.
        /// </summary>
        internal Server() { }

        /// <summary>
        /// Initializes the object with the specified COM server.
        /// </summary>
        internal Server(OpcUrl url, object server)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            url_ = (OpcUrl)url.Clone();
            server_ = server;
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">If true managed and unmanaged resources can be disposed. If false only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposed_)
            {
                lock (this)
                {
                    if (disposing)
                    {
                        // Release managed resources.
                        if (server_ != null)
                        {
                            // release all groups.
                            foreach (Subscription subscription in subscriptions_.Values)
                            {
                                var methodName = "IOPCServer.RemoveGroup";

                                // remove subscription from server.
                                try
                                {
                                    var state = subscription.GetState();
                                    if (state != null)
                                    {
                                        var server = BeginComCall<IOPCServer>(methodName, true);
                                        server?.RemoveGroup((int)state.ServerHandle, 0);
                                    }
                                }
                                catch
                                {
                                    // Ignore error during Dispose
                                }
                                finally
                                {
                                    EndComCall(methodName);
                                }
                                // dispose of the subscription object (disconnects all subscription connections).
                                subscription.Dispose();
                            }

                            // clear subscription table.
                            subscriptions_.Clear();
                        }
                    }

                    // Release unmanaged resources.
                    // Set large fields to null.

                    if (server_ != null)
                    {
                        // release the COM server.
                        Technosoftware.DaAeHdaClient.Com.Interop.ReleaseServer(server_);
                        server_ = null;
                    }
                }

                // Call Dispose on your base class.
                disposed_ = true;
            }

            base.Dispose(disposing);
        }

        private bool disposed_;
        #endregion

        #region Technosoftware.DaAeHdaClient.Com.Server Overrides
        /// <summary>
        /// Returns the localized text for the specified result code.
        /// </summary>
        /// <param name="locale">The locale name in the format "[languagecode]-[country/regioncode]".</param>
        /// <param name="resultId">The result code identifier.</param>
        /// <returns>A message localized for the best match for the requested locale.</returns>
        public override string GetErrorText(string locale, OpcResult resultId)
        {
            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();
                var methodName = "IOPCServer.GetErrorString";

                // invoke COM method.
                try
                {
                    var server = BeginComCall<IOPCServer>(methodName, true);

                    (server).GetErrorString(
                        resultId.Code,
                        Technosoftware.DaAeHdaClient.Com.Interop.GetLocale(locale),
                        out var errorText);

                    if (DCOMCallWatchdog.IsCancelled)
                    {
                        throw new Exception($"{methodName} call was cancelled due to response timeout");
                    }

                    return errorText;
                }
                catch (Exception e)
                {
                    ComCallError(methodName, e);
                    throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException("IOPCServer.GetErrorString", e);
                }
                finally
                {
                    EndComCall(methodName);
                }
            }
        }
        #endregion

        #region Technosoftware.DaAeHdaClient.IOpcServer Members
        /// <summary>
        /// Returns the filters applied by the server to any item results returned to the client.
        /// </summary>
        /// <returns>A bit mask indicating which fields should be returned in any item results.</returns>
        public int GetResultFilters()
        {
            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();
                return filters_;
            }
        }

        /// <summary>
        /// Sets the filters applied by the server to any item results returned to the client.
        /// </summary>
        /// <param name="filters">A bit mask indicating which fields should be returned in any item results.</param>
        public void SetResultFilters(int filters)
        {
            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();
                filters_ = filters;
            }
        }

        /// <summary>
        /// Returns the current server status.
        /// </summary>
        /// <returns>The current server status.</returns>
        public OpcServerStatus GetServerStatus()
        {
            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();
                var methodName = "IOPCServer.GetStatus";

                // initialize arguments.
                IntPtr pStatus;

                // invoke COM method.
                try
                {
                    var server = BeginComCall<IOPCServer>(methodName, true);
                    (server).GetStatus(out pStatus);

                    if (DCOMCallWatchdog.IsCancelled)
                    {
                        throw new Exception($"{methodName} call was cancelled due to response timeout");
                    }
                }
                catch (Exception e)
                {
                    ComCallError(methodName, e);
                    throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException(methodName, e);
                }
                finally
                {
                    EndComCall(methodName);
                }

                // return status.
                return Interop.GetServerStatus(ref pStatus, true);
            }
        }

        /// <summary>
        /// Reads the current values for a set of items. 
        /// </summary>
        /// <param name="items">The set of items to read.</param>
        /// <returns>The results of the read operation for each item.</returns>
        public virtual TsCDaItemValueResult[] Read(TsCDaItem[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            lock (this)
            {
                var methodName = "IOPCItemIO.Read";
                if (server_ == null) throw new NotConnectedException();

                var count = items.Length;
                if (count == 0) throw new ArgumentOutOfRangeException(nameof(items.Length), @"0");

                // initialize arguments.
                var itemIDs = new string[count];
                var maxAges = new int[count];

                for (var ii = 0; ii < count; ii++)
                {
                    itemIDs[ii] = items[ii].ItemName;
                    maxAges[ii] = (items[ii].MaxAgeSpecified) ? items[ii].MaxAge : 0;
                }

                var pValues = IntPtr.Zero;
                var pQualities = IntPtr.Zero;
                var pTimestamps = IntPtr.Zero;
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    var server = BeginComCall<IOPCItemIO>(methodName, true);
                    server.Read(
                         count,
                         itemIDs,
                         maxAges,
                         out pValues,
                         out pQualities,
                         out pTimestamps,
                         out pErrors);

                    if (DCOMCallWatchdog.IsCancelled)
                    {
                        throw new Exception($"{methodName} call was cancelled due to response timeout");
                    }

                }
                catch (Exception e)
                {
                    ComCallError(methodName, e);
                    throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException(methodName, e);
                }
                finally
                {
                    EndComCall(methodName);
                }

                // unmarshal results.
                var values = Technosoftware.DaAeHdaClient.Com.Interop.GetVARIANTs(ref pValues, count, true);
                var qualities = Technosoftware.DaAeHdaClient.Com.Interop.GetInt16s(ref pQualities, count, true);
                var timestamps = Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIMEs(ref pTimestamps, count, true);
                var errors = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref pErrors, count, true);

                // pre-fetch the current locale to use for data conversions.
                var locale = GetLocale();

                // construct result array.
                var results = new TsCDaItemValueResult[count];

                for (var ii = 0; ii < results.Length; ii++)
                {
                    results[ii] = new TsCDaItemValueResult(items[ii]);

                    results[ii].ServerHandle = null;
                    results[ii].Value = values[ii];
                    results[ii].Quality = new TsCDaQuality(qualities[ii]);
                    results[ii].QualitySpecified = true;
                    results[ii].Timestamp = timestamps[ii];
                    results[ii].TimestampSpecified = timestamps[ii] != DateTime.MinValue;
                    results[ii].Result = Utilities.Interop.GetResultId(errors[ii]);
                    results[ii].DiagnosticInfo = null;

                    // convert COM code to unified DA code.
                    if (errors[ii] == Result.E_BADRIGHTS) { results[ii].Result = new OpcResult(OpcResult.Da.E_WRITEONLY, Result.E_BADRIGHTS); }

                    // convert the data type since the server does not support the feature.
                    if (results[ii].Value != null && items[ii].ReqType != null)
                    {
                        try
                        {
                            results[ii].Value = ChangeType(values[ii], items[ii].ReqType, locale);
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

                    // apply request options.
                    if ((filters_ & (int)TsCDaResultFilter.ItemName) == 0) results[ii].ItemName = null;
                    if ((filters_ & (int)TsCDaResultFilter.ItemPath) == 0) results[ii].ItemPath = null;
                    if ((filters_ & (int)TsCDaResultFilter.ClientHandle) == 0) results[ii].ClientHandle = null;

                    if ((filters_ & (int)TsCDaResultFilter.ItemTime) == 0)
                    {
                        results[ii].Timestamp = DateTime.MinValue;
                        results[ii].TimestampSpecified = false;
                    }
                }

                // return results.
                return results;
            }
        }

        /// <summary>
        /// Writes the value, quality and timestamp for a set of items.
        /// </summary>
        /// <param name="items">The set of item values to write.</param>
        /// <returns>The results of the write operation for each item.</returns>
        public virtual OpcItemResult[] Write(TsCDaItemValue[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();
                var methodName = "IOPCItemIO.WriteVQT";

                var count = items.Length;
                if (count == 0) throw new ArgumentOutOfRangeException("items.Length", "0");

                // initialize arguments.
                var itemIDs = new string[count];

                for (var ii = 0; ii < count; ii++)
                {
                    itemIDs[ii] = items[ii].ItemName;
                }

                var values = Interop.GetOPCITEMVQTs(items);

                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    var server = BeginComCall<IOPCItemIO>(methodName, true);
                    server.WriteVQT(
                        count,
                        itemIDs,
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
                    throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException(methodName, e);
                }
                finally
                {
                    EndComCall(methodName);
                }

                // unmarshal results.
                var errors = Utilities.Interop.GetInt32s(ref pErrors, count, true);

                // construct result array.
                var results = new OpcItemResult[count];

                for (var ii = 0; ii < count; ii++)
                {
                    results[ii] = new OpcItemResult(items[ii]);

                    results[ii].ServerHandle = null;
                    results[ii].Result = Utilities.Interop.GetResultId(errors[ii]);
                    results[ii].DiagnosticInfo = null;

                    // convert COM code to unified DA code.
                    if (errors[ii] == Result.E_BADRIGHTS) { results[ii].Result = new OpcResult(OpcResult.Da.E_READONLY, Result.E_BADRIGHTS); }

                    // apply request options.
                    if ((filters_ & (int)TsCDaResultFilter.ItemName) == 0) results[ii].ItemName = null;
                    if ((filters_ & (int)TsCDaResultFilter.ItemPath) == 0) results[ii].ItemPath = null;
                    if ((filters_ & (int)TsCDaResultFilter.ClientHandle) == 0) results[ii].ClientHandle = null;
                }

                // return results.
                return results;
            }
        }

        /// <summary>
        /// Creates a new subscription.
        /// </summary>
        /// <param name="state">The initial state of the subscription.</param>
        /// <returns>The new subscription object.</returns>
        public ITsCDaSubscription CreateSubscription(TsCDaSubscriptionState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();
                var methodName = "IOPCServer.AddGroup";

                // copy the subscription state.
                var result = (TsCDaSubscriptionState)state.Clone();

                // initialize arguments.
                var iid = typeof(IOPCItemMgt).GUID;
                object group = null;

                var serverHandle = 0;
                var revisedUpdateRate = 0;

                var hDeadband = GCHandle.Alloc(result.Deadband, GCHandleType.Pinned);

                // invoke COM method.
                try
                {
                    var server = BeginComCall<IOPCServer>(methodName, true);
                    server.AddGroup(
                        (result.Name != null) ? result.Name : "",
                        (result.Active) ? 1 : 0,
                        result.UpdateRate,
                        0,
                        IntPtr.Zero,
                        hDeadband.AddrOfPinnedObject(),
                        Technosoftware.DaAeHdaClient.Com.Interop.GetLocale(result.Locale),
                        out serverHandle,
                        out revisedUpdateRate,
                        ref iid,
                        out group);

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
                    if (hDeadband.IsAllocated)
                    {
                        hDeadband.Free();
                    }
                    EndComCall(methodName);
                }

                if (group == null) throw new OpcResultException(OpcResult.E_FAIL, "The subscription  was not created.");

                methodName = "IOPCGroupStateMgt2.SetKeepAlive";

                // set the keep alive rate if requested.
                try
                {
                    var keepAlive = 0;
                    var comObject = BeginComCall<IOPCGroupStateMgt2>(group, methodName, true);
                    comObject.SetKeepAlive(result.KeepAlive, out keepAlive);

                    if (DCOMCallWatchdog.IsCancelled)
                    {
                        throw new Exception($"{methodName} call was cancelled due to response timeout");
                    }

                    result.KeepAlive = keepAlive;
                }
                catch (Exception e1)
                {
                    result.KeepAlive = 0;
                    ComCallError(methodName, e1);
                }
                finally
                {
                    EndComCall(methodName);
                }

                // save server handle.
                result.ServerHandle = serverHandle;

                // set the revised update rate.
                if (revisedUpdateRate > result.UpdateRate)
                {
                    result.UpdateRate = revisedUpdateRate;
                }

                // create the subscription object.
                var subscription = CreateSubscription(group, result, filters_);

                // index by server handle.
                subscriptions_[serverHandle] = subscription;

                // return subscription.
                return subscription;
            }
        }

        /// <summary>
        /// Cancels a subscription and releases all resources allocated for it.
        /// </summary>
        /// <param name="subscription">The subscription to cancel.</param>
        public void CancelSubscription(ITsCDaSubscription subscription)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();
                var methodName = "IOPCServer.RemoveGroup";

                // validate argument.
                if (!typeof(Subscription).IsInstanceOfType(subscription))
                {
                    throw new ArgumentException("Incorrect object type.", nameof(subscription));
                }

                // get the subscription state.
                var state = subscription.GetState();

                if (!subscriptions_.ContainsKey(state.ServerHandle))
                {
                    throw new ArgumentException("Handle not found.", nameof(subscription));
                }

                subscriptions_.Remove(state.ServerHandle);

                // release all subscription resources.
                subscription.Dispose();

                // invoke COM method.
                try
                {
                    var server = BeginComCall<IOPCServer>(methodName, true);
                    server.RemoveGroup((int)state.ServerHandle, 0);

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
            }
        }

        /// <summary>
        /// Fetches the children of a branch that meet the filter criteria.
        /// </summary>
        /// <param name="itemId">The identifier of branch which is the target of the search.</param>
        /// <param name="filters">The filters to use to limit the set of child elements returned.</param>
        /// <param name="position">An object used to continue a browse that could not be completed.</param>
        /// <returns>The set of elements found.</returns>
        public virtual TsCDaBrowseElement[] Browse(
            OpcItem itemId,
            TsCDaBrowseFilters filters,
            out TsCDaBrowsePosition position)
        {
            if (filters == null) throw new ArgumentNullException(nameof(filters));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();
                var methodName = "IOPCBrowse.Browse";

                position = null;

                // initialize arguments.
                var count = 0;
                var moreElements = 0;

                var pContinuationPoint = IntPtr.Zero;
                var pElements = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    var server = BeginComCall<IOPCBrowse>(methodName, true);
                    server.Browse(
                             (itemId != null && itemId.ItemName != null) ? itemId.ItemName : "",
                         ref pContinuationPoint,
                         filters.MaxElementsReturned,
                             Interop.GetBrowseFilter(filters.BrowseFilter),
                             (filters.ElementNameFilter != null) ? filters.ElementNameFilter : "",
                             (filters.VendorFilter != null) ? filters.VendorFilter : "",
                             (filters.ReturnAllProperties) ? 1 : 0,
                             (filters.ReturnPropertyValues) ? 1 : 0,
                             (filters.PropertyIDs != null) ? filters.PropertyIDs.Length : 0,
                             Interop.GetPropertyIDs(filters.PropertyIDs),
                         out moreElements,
                         out count,
                         out pElements);

                    if (DCOMCallWatchdog.IsCancelled)
                    {
                        throw new Exception($"{methodName} call was cancelled due to response timeout");
                    }
                }
                catch (Exception e)
                {
                    ComCallError(methodName, e);
                    throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException(methodName, e);
                }
                finally
                {
                    EndComCall(methodName);
                }

                // unmarshal results.
                var elements = Interop.GetBrowseElements(ref pElements, count, true);

                var continuationPoint = Marshal.PtrToStringUni(pContinuationPoint);
                Marshal.FreeCoTaskMem(pContinuationPoint);

                // check if more results exist.
                if (moreElements != 0 || (continuationPoint != null && continuationPoint != ""))
                {
                    // allocate new browse position object.
                    position = new BrowsePosition(itemId, filters, continuationPoint);
                }

                // process results.
                ProcessResults(elements, filters.PropertyIDs);

                return elements;
            }
        }

        /// <summary>
        /// Continues a browse operation with previously specified search criteria.
        /// </summary>
        /// <param name="position">An object containing the browse operation state information.</param>
        /// <returns>The set of elements found.</returns>
        public virtual TsCDaBrowseElement[] BrowseNext(ref TsCDaBrowsePosition position)
        {
            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();
                var methodName = "IOPCBrowse.Browse";

                // check for valid position object.
                if (position == null || position.GetType() != typeof(BrowsePosition))
                {
                    throw new BrowseCannotContinueException();
                }

                var pos = (BrowsePosition)position;

                // check for valid continuation point.
                if (pos == null || pos.ContinuationPoint == null || pos.ContinuationPoint == "")
                {
                    throw new BrowseCannotContinueException();
                }

                // initialize arguments.
                var count = 0;
                var moreElements = 0;

                var itemID = ((BrowsePosition)position).ItemID;
                var filters = ((BrowsePosition)position).Filters;

                var pContinuationPoint = Marshal.StringToCoTaskMemUni(pos.ContinuationPoint);
                var pElements = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    var server = BeginComCall<IOPCBrowse>(methodName, true);
                    server.Browse(
                        (itemID != null && itemID.ItemName != null) ? itemID.ItemName : "",
                        ref pContinuationPoint,
                        filters.MaxElementsReturned,
                            Interop.GetBrowseFilter(filters.BrowseFilter),
                            (filters.ElementNameFilter != null) ? filters.ElementNameFilter : "",
                            (filters.VendorFilter != null) ? filters.VendorFilter : "",
                            (filters.ReturnAllProperties) ? 1 : 0,
                            (filters.ReturnPropertyValues) ? 1 : 0,
                            (filters.PropertyIDs != null) ? filters.PropertyIDs.Length : 0,
                            Interop.GetPropertyIDs(filters.PropertyIDs),
                        out moreElements,
                        out count,
                        out pElements);

                    if (DCOMCallWatchdog.IsCancelled)
                    {
                        throw new Exception($"{methodName} call was cancelled due to response timeout");
                    }
                }
                catch (Exception e)
                {
                    ComCallError(methodName, e);
                    throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException(methodName, e);
                }
                finally
                {
                    EndComCall(methodName);
                }

                // unmarshal results.
                var elements = Interop.GetBrowseElements(ref pElements, count, true);

                pos.ContinuationPoint = Marshal.PtrToStringUni(pContinuationPoint);
                Marshal.FreeCoTaskMem(pContinuationPoint);

                // check if more no results exist.
                if (moreElements == 0 && (pos.ContinuationPoint == null || pos.ContinuationPoint == ""))
                {
                    position = null;
                }

                // process results.
                ProcessResults(elements, filters.PropertyIDs);

                return elements;
            }
        }

        /// <summary>
        /// Returns the item properties for a set of items.
        /// </summary>
        /// <param name="itemIds">A list of item identifiers.</param>
        /// <param name="propertyIDs">A list of properties to fetch for each item.</param>
        /// <param name="returnValues">Whether the property values should be returned with the properties.</param>
        /// <returns>A list of properties for each item.</returns>
        public virtual TsCDaItemPropertyCollection[] GetProperties(
            OpcItem[] itemIds,
            TsDaPropertyID[] propertyIDs,
            bool returnValues)
        {
            if (itemIds == null) throw new ArgumentNullException(nameof(itemIds));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();
                var methodName = "IOPCBrowse.GetProperties";

                // initialize arguments.
                var pItemIDs = new string[itemIds.Length];

                for (var ii = 0; ii < itemIds.Length; ii++)
                {
                    pItemIDs[ii] = itemIds[ii].ItemName;
                }

                var pPropertyLists = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    var server = BeginComCall<IOPCBrowse>(methodName, true);
                    server.GetProperties(
                          itemIds.Length,
                          pItemIDs,
                          (returnValues) ? 1 : 0,
                          (propertyIDs != null) ? propertyIDs.Length : 0,
                          Interop.GetPropertyIDs(propertyIDs),
                          out pPropertyLists);

                    if (DCOMCallWatchdog.IsCancelled)
                    {
                        throw new Exception($"{methodName} call was cancelled due to response timeout");
                    }
                }
                catch (Exception e)
                {
                    ComCallError(methodName, e);
                    throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException(methodName, e);
                }
                finally
                {
                    EndComCall(methodName);
                }

                // unmarshal results.
                var resultLists = Interop.GetItemPropertyCollections(ref pPropertyLists, itemIds.Length, true);

                // replace integer codes with qnames passed in.
                if (propertyIDs != null && propertyIDs.Length > 0)
                {
                    foreach (var resultList in resultLists)
                    {
                        for (var ii = 0; ii < resultList.Count; ii++)
                        {
                            resultList[ii].ID = propertyIDs[ii];
                        }
                    }
                }

                // return the results.
                return resultLists;
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Converts a value to the specified type using the specified locale.
        /// </summary>
        protected object ChangeType(object source, Type type, string locale)
        {
            var culture = Thread.CurrentThread.CurrentCulture;

            // override the current thread culture to ensure conversions happen correctly.
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(locale);
            }
            catch
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            }

            try
            {
                var result = OpcConvert.ChangeType(source, type);

                // check for overflow converting to float.
                if (typeof(float) == type)
                {
                    if (float.IsInfinity(Convert.ToSingle(result)))
                    {
                        throw new OverflowException();
                    }
                }

                return result;
            }

            // restore the current thread culture after conversion.
            finally
            {
                Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        /// <summary>
        /// Creates a new instance of a subscription.
        /// </summary>
        protected virtual Subscription CreateSubscription(
            object group,
            TsCDaSubscriptionState state,
            int filters)
        {
            return new Subscription(group, state, filters);
        }

        /// <summary>
        /// Updates the properties to convert COM values to OPC .NET API results.
        /// </summary>
        private void ProcessResults(TsCDaBrowseElement[] elements, TsDaPropertyID[] propertyIds)
        {
            // check for null.
            if (elements == null)
            {
                return;
            }

            // process each element.
            foreach (var element in elements)
            {
                // check if no properties.
                if (element.Properties == null)
                {
                    continue;
                }

                // process each property.
                foreach (var property in element.Properties)
                {
                    // replace the property ids which on contain the codes with the proper qualified names passed in.
                    if (propertyIds != null)
                    {
                        foreach (var propertyId in propertyIds)
                        {
                            if (property.ID.Code == propertyId.Code)
                            {
                                property.ID = propertyId;
                                break;
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
