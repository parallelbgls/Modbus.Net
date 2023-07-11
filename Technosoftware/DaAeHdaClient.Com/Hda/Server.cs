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

using Technosoftware.DaAeHdaClient.Hda;
using Technosoftware.OpcRcw.Hda;

#endregion

#pragma warning disable CS0618  

namespace Technosoftware.DaAeHdaClient.Com.Hda
{
    /// <summary>
    /// An in-process wrapper for a remote OPC COM-HDA server (thread-safe).
    /// </summary>
    internal class Server : Technosoftware.DaAeHdaClient.Com.Server, ITsCHdaServer
    {
        #region Constructor
        //======================================================================
        // Construction

        /// <summary>
        /// Initializes the object.
        /// </summary>
        internal Server() { }

        /// <summary>
        /// Initializes the object with the specifed COM server.
        /// </summary>
        internal Server(OpcUrl url, object server)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            url_ = (OpcUrl)url.Clone();
            server_ = server;

            // establish the callback.
            Advise();
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
                lock (lock_)
                {
                    if (disposing)
                    {
                        // Release managed resources.
                        // close the callback.
                        Unadvise();
                    }

                    // Release unmanaged resources.
                    // Set large fields to null.

                    disposed_ = true;
                }
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Server Info
        //======================================================================
        // GetStatus

        /// <summary>
        /// Returns the current server status.
        /// </summary>
        /// <returns>The current server status.</returns>
        public OpcServerStatus GetServerStatus()
        {
            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // initialize arguments.
                var pStatus = IntPtr.Zero;

                var wStatus = OPCHDA_SERVERSTATUS.OPCHDA_INDETERMINATE;

                var pftCurrentTime = IntPtr.Zero;
                var pftStartTime = IntPtr.Zero;
                short wMajorVersion = 0;
                short wMinorVersion = 0;
                short wBuildNumber = 0;
                var dwMaxReturnValues = 0;
                string szStatusString = null;
                string szVendorInfo = null;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_Server)server_).GetHistorianStatus(
                        out wStatus,
                        out pftCurrentTime,
                        out pftStartTime,
                        out wMajorVersion,
                        out wMinorVersion,
                        out wBuildNumber,
                        out dwMaxReturnValues,
                        out szStatusString,
                        out szVendorInfo);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_Server.GetHistorianStatus", e);
                }

                // unmarshal return parameters and free memory.
                var status = new OpcServerStatus();

                status.VendorInfo = szVendorInfo;
                status.ProductVersion = string.Format("{0}.{1}.{2}", wMajorVersion, wMinorVersion, wBuildNumber);
                switch (wStatus)
                {
                    case OPCHDA_SERVERSTATUS.OPCHDA_DOWN:
                        status.ServerState = OpcServerState.NotOperational;
                        break;
                    case OPCHDA_SERVERSTATUS.OPCHDA_INDETERMINATE:
                        status.ServerState = OpcServerState.Unknown;
                        break;
                    case OPCHDA_SERVERSTATUS.OPCHDA_UP:
                        status.ServerState = OpcServerState.Operational;
                        break;
                    default:
                        status.ServerState = OpcServerState.Unknown;
                        break;
                }
                status.ServerState = (OpcServerState)wStatus;
                status.StatusInfo = szStatusString;
                status.StartTime = DateTime.MinValue;
                status.CurrentTime = DateTime.MinValue;
                status.MaxReturnValues = dwMaxReturnValues;

                if (pftStartTime != IntPtr.Zero)
                {
                    status.StartTime = Utilities.Interop.GetDateTime(pftStartTime);
                    Marshal.FreeCoTaskMem(pftStartTime);
                }

                if (pftCurrentTime != IntPtr.Zero)
                {
                    status.CurrentTime = Utilities.Interop.GetDateTime(pftCurrentTime);
                    Marshal.FreeCoTaskMem(pftCurrentTime);
                }

                return status;
            }
        }

        //======================================================================
        // GetAttributes

        /// <summary>
        /// Returns the item attributes supported by the server.
        /// </summary>
        /// <returns>The a set of item attributes and their descriptions.</returns>
        public TsCHdaAttribute[] GetAttributes()
        {
            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // initialize arguments.
                var count = 0;

                var pIDs = IntPtr.Zero;
                var pNames = IntPtr.Zero;
                var pDescriptions = IntPtr.Zero;
                var pDataTypes = IntPtr.Zero;

                try
                {
                    ((IOPCHDA_Server)server_).GetItemAttributes(
                        out count,
                        out pIDs,
                        out pNames,
                        out pDescriptions,
                        out pDataTypes);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_Server.GetItemAttributes", e);
                }

                // check if no attributes supported.
                if (count == 0)
                {
                    return new TsCHdaAttribute[0];
                }

                // unmarshal return parameters and free memory.
                var ids = Utilities.Interop.GetInt32s(ref pIDs, count, true);
                var names = Utilities.Interop.GetUnicodeStrings(ref pNames, count, true);
                var descriptions = Utilities.Interop.GetUnicodeStrings(ref pDescriptions, count, true);
                var datatypes = Utilities.Interop.GetInt16s(ref pDataTypes, count, true);

                // verify return parameters.
                if (ids == null || names == null || descriptions == null || datatypes == null)
                {
                    throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The response from the server was invalid or incomplete");
                }

                var attributes = new TsCHdaAttribute[count];

                for (var ii = 0; ii < count; ii++)
                {
                    attributes[ii] = new TsCHdaAttribute();

                    attributes[ii].ID = ids[ii];
                    attributes[ii].Name = names[ii];
                    attributes[ii].Description = descriptions[ii];
                    attributes[ii].DataType = Utilities.Interop.GetType((VarEnum)Enum.ToObject(typeof(VarEnum), datatypes[ii]));
                }

                // return results.
                return attributes;
            }
        }

        //======================================================================
        // GetAggregates

        /// <summary>
        /// Returns the aggregates supported by the server.
        /// </summary>
        /// <returns>The a set of aggregates and their descriptions.</returns>
        public TsCHdaAggregate[] GetAggregates()
        {
            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // initialize arguments.
                var count = 0;

                var pIDs = IntPtr.Zero;
                var pNames = IntPtr.Zero;
                var pDescriptions = IntPtr.Zero;

                try
                {
                    ((IOPCHDA_Server)server_).GetAggregates(
                        out count,
                        out pIDs,
                        out pNames,
                        out pDescriptions);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_Server.GetAggregates", e);
                }

                // check if no aggregates supported.
                if (count == 0)
                {
                    return new TsCHdaAggregate[0];
                }

                // unmarshal return parameters and free memory.
                var ids = Utilities.Interop.GetInt32s(ref pIDs, count, true);
                var names = Utilities.Interop.GetUnicodeStrings(ref pNames, count, true);
                var descriptions = Utilities.Interop.GetUnicodeStrings(ref pDescriptions, count, true);

                // verify return parameters.
                if (ids == null || names == null || descriptions == null)
                {
                    throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The response from the server was invalid or incomplete");
                }

                var aggregates = new TsCHdaAggregate[count];

                for (var ii = 0; ii < count; ii++)
                {
                    aggregates[ii] = new TsCHdaAggregate();

                    aggregates[ii].Id = ids[ii];
                    aggregates[ii].Name = names[ii];
                    aggregates[ii].Description = descriptions[ii];
                }

                // return results.
                return aggregates;
            }
        }

        //======================================================================
        // CreateBrowser

        /// <summary>
        /// Creates a object used to browse the server address space.
        /// </summary>
        /// <param name="filters">The set of attribute filters to use when browsing.</param>
        /// <param name="results">A result code for each individual filter.</param>
        /// <returns>A browser object that must be released by calling Dispose().</returns>
        public ITsCHdaBrowser CreateBrowser(TsCHdaBrowseFilter[] filters, out OpcResult[] results)
        {
            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // initialize arguments.
                var count = (filters != null) ? filters.Length : 0;

                // marshal input parameters.
                var ids = new int[count];
                var values = new object[count];
                var operators = new OPCHDA_OPERATORCODES[count];

                for (var ii = 0; ii < count; ii++)
                {
                    ids[ii] = filters[ii].AttributeID;
                    operators[ii] = (OPCHDA_OPERATORCODES)Enum.ToObject(typeof(OPCHDA_OPERATORCODES), filters[ii].Operator);
                    values[ii] = Utilities.Interop.GetVARIANT(filters[ii].FilterValue);
                }

                // initialize output parameners
                IOPCHDA_Browser pBrowser = null;
                var pErrors = IntPtr.Zero;

                // call COM server.
                try
                {
                    ((IOPCHDA_Server)server_).CreateBrowse(
                        count,
                        ids,
                        operators,
                        values,
                        out pBrowser,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_Server.CreateBrowse", e);
                }

                // unmarshal return parameters and free memory.
                var errors = Utilities.Interop.GetInt32s(ref pErrors, count, true);

                // verify return parameters.
                if ((count > 0 && errors == null) || pBrowser == null)
                {
                    throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
                }

                results = new OpcResult[count];

                for (var ii = 0; ii < count; ii++)
                {
                    results[ii] = Utilities.Interop.GetResultId(errors[ii]);
                }

                // return browser.
                return new Browser(this, pBrowser, filters, results);
            }
        }
        #endregion

        #region Item Management
        //======================================================================
        // CreateItems

        /// <summary>
        /// Creates a set of items.
        /// </summary>
        /// <param name="items">The identifiers for the items to create.</param>
        /// <returns>The results for each item containing the server handle and result code.</returns>
        public OpcItemResult[] CreateItems(OpcItem[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                // initialize input parameters.
                var itemIDs = new string[items.Length];
                var clientHandles = new int[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    if (items[ii] != null)
                    {
                        itemIDs[ii] = items[ii].ItemName;
                        clientHandles[ii] = CreateHandle();
                    }
                }

                // initialize output arguments.
                var pServerHandles = IntPtr.Zero;
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_Server)server_).GetItemHandles(
                        items.Length,
                        itemIDs,
                        clientHandles,
                        out pServerHandles,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_Server.GetItemHandles", e);
                }

                // unmarshal return parameters and free memory.
                var serverHandles = Utilities.Interop.GetInt32s(ref pServerHandles, items.Length, true);
                var errors = Utilities.Interop.GetInt32s(ref pErrors, items.Length, true);

                // verify return parameters.
                if (serverHandles == null || errors == null)
                {
                    throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
                }

                var results = new OpcItemResult[items.Length];

                for (var ii = 0; ii < results.Length; ii++)
                {
                    results[ii] = new OpcItemResult(items[ii]);
                    results[ii].Result = Utilities.Interop.GetResultId(errors[ii]);

                    if (results[ii].Result.Succeeded())
                    {
                        // cache item id locally to store remote server handle/local client handle mapping.
                        var itemID = new OpcItem();

                        itemID.ItemName = items[ii].ItemName;
                        itemID.ItemPath = items[ii].ItemPath;
                        itemID.ServerHandle = serverHandles[ii];
                        itemID.ClientHandle = items[ii].ClientHandle;

                        items_.Add(clientHandles[ii], itemID);

                        // return correct handles in result.
                        results[ii].ServerHandle = clientHandles[ii];
                        results[ii].ClientHandle = items[ii].ClientHandle;
                    }
                }

                return results;
            }
        }

        //======================================================================
        // ReleaseItems

        /// <summary>
        /// Releases a set of previously created items.
        /// </summary>
        /// <param name="items">The server handles for the items to release.</param>
        /// <returns>The results for each item containing the result code.</returns>
        public OpcItemResult[] ReleaseItems(OpcItem[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                // initialize input parameters.
                var serverHandles = GetServerHandles(items);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_Server)server_).ReleaseItemHandles(
                        items.Length,
                        serverHandles,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_Server.ReleaseItemHandles", e);
                }

                // unmarshal return parameters and free memory.
                var errors = Utilities.Interop.GetInt32s(ref pErrors, items.Length, true);

                // verify return parameters.
                if (errors == null)
                {
                    throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
                }

                var results = new OpcItemResult[items.Length];

                for (var ii = 0; ii < results.Length; ii++)
                {
                    results[ii] = new OpcItemResult(items[ii]);
                    results[ii].Result = Utilities.Interop.GetResultId(errors[ii]);

                    if (results[ii].Result.Succeeded() && items[ii].ServerHandle != null)
                    {
                        // lookup locally cached item id.
                        var itemID = (OpcItem)items_[items[ii].ServerHandle];

                        // remove the locally cached item.
                        if (itemID != null)
                        {
                            results[ii].ItemName = itemID.ItemName;
                            results[ii].ItemPath = itemID.ItemPath;
                            results[ii].ClientHandle = itemID.ClientHandle;

                            items_.Remove(items[ii].ServerHandle);
                        }
                    }
                }

                // return results.
                return results;
            }
        }

        //======================================================================
        // ValidateItems

        /// <summary>
        /// Validates a set of items.
        /// </summary>
        /// <param name="items">The identifiers for the items to validate.</param>
        /// <returns>The results for each item containing the result code.</returns>
        public OpcItemResult[] ValidateItems(OpcItem[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                // initialize input parameters.
                var itemIDs = new string[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    if (items[ii] != null)
                    {
                        itemIDs[ii] = items[ii].ItemName;
                    }
                }

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_Server)server_).ValidateItemIDs(
                        items.Length,
                        itemIDs,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_Server.ValidateItemIDs", e);
                }

                // unmarshal return parameters and free memory.
                var errors = Utilities.Interop.GetInt32s(ref pErrors, items.Length, true);

                // verify return parameters.
                if (errors == null)
                {
                    throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
                }

                var results = new OpcItemResult[items.Length];

                for (var ii = 0; ii < results.Length; ii++)
                {
                    results[ii] = new OpcItemResult(items[ii]);
                    results[ii].Result = Utilities.Interop.GetResultId(errors[ii]);
                }

                return results;
            }
        }
        #endregion

        #region Read Raw
        //======================================================================
        // ReadRaw

        /// <summary>
        /// Reads raw (unprocessed) data from the historian database for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="maxValues">The number of values to be read for each item.</param>
        /// <param name="includeBounds">Whether the bounding item values should be returned.</param>
        /// <param name="items">The set of items to read (must include the server handle).</param>
        /// <returns>A set of values, qualities and timestamps within the requested time range for each item.</returns>
        public TsCHdaItemValueCollection[] ReadRaw(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            int maxValues,
            bool includeBounds,
            OpcItem[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new TsCHdaItemValueCollection[0];
                }

                // initialize input parameters.
                var serverHandles = GetServerHandles(items);

                var pStartTime = Interop.GetTime(startTime);
                var pEndTime = Interop.GetTime(endTime);

                // initialize output arguments.
                var pValues = IntPtr.Zero;
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_SyncRead)server_).ReadRaw(
                        ref pStartTime,
                        ref pEndTime,
                        maxValues,
                        (includeBounds) ? 1 : 0,
                        serverHandles.Length,
                        serverHandles,
                        out pValues,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_SyncRead.ReadRaw", e);
                }

                // unmarhal modified item structures.
                var results = Interop.GetItemValueCollections(ref pValues, items.Length, true);

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // store actual items in result.
                UpdateActualTimes(results, pStartTime, pEndTime);

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Sends an asynchronous request to read raw data from the historian database for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="maxValues">The number of values to be read for each item.</param>
        /// <param name="includeBounds">Whether the bounding item values should be returned.</param>
        /// <param name="items">The set of items to read (must include the server handle).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] ReadRaw(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            int maxValues,
            bool includeBounds,
            OpcItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            request = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                var internalRequest = callback_.CreateRequest(requestHandle, callback);

                // initialize input parameters.
                var requestID = internalRequest.RequestID;
                var cancelID = 0;

                var serverHandles = GetServerHandles(items);

                var pStartTime = Interop.GetTime(startTime);
                var pEndTime = Interop.GetTime(endTime);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_AsyncRead)server_).ReadRaw(
                        internalRequest.RequestID,
                        ref pStartTime,
                        ref pEndTime,
                        maxValues,
                        (includeBounds) ? 1 : 0,
                        serverHandles.Length,
                        serverHandles,
                        out cancelID,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_AsyncRead.ReadRaw", e);
                }

                // create result objects.
                var results = new OpcItemResult[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    results[ii] = new OpcItemResult();
                }

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // check if request has already completed.
                if (internalRequest.Update(cancelID, results))
                {
                    // discard the request.
                    request = null;
                    callback_.CancelRequest(internalRequest, (TsCHdaCancelCompleteEventHandler)null);

                    // return results.
                    return results;
                }

                // store actual items in request object.
                UpdateActualTimes(new ITsCHdaActualTime[] { internalRequest }, pStartTime, pEndTime);

                // return request object.
                request = internalRequest;

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Requests that the server periodically send notifications when new data becomes available for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="updateInterval">The frequency, in seconds, that the server should check for new data.</param>
        /// <param name="items">The set of items to read (must include the server handle).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] AdviseRaw(
            TsCHdaTime startTime,
            decimal updateInterval,
            OpcItem[] items,
            object requestHandle,
            TsCHdaDataUpdateEventHandler callback,
            out IOpcRequest request)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            request = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                var internalRequest = callback_.CreateRequest(requestHandle, callback);

                // initialize input parameters.
                var requestID = internalRequest.RequestID;
                var cancelID = 0;

                var serverHandles = GetServerHandles(items);

                var pStartTime = Interop.GetTime(startTime);
                var ftUpdateInterval = Interop.GetFILETIME(updateInterval);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_AsyncRead)server_).AdviseRaw(
                        internalRequest.RequestID,
                        ref pStartTime,
                        ftUpdateInterval,
                        serverHandles.Length,
                        serverHandles,
                        out cancelID,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_AsyncRead.AdviseRaw", e);
                }

                // create result objects.
                var results = new OpcItemResult[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    results[ii] = new OpcItemResult();
                }

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // send callbacks for any data that has already arrived.
                internalRequest.Update(cancelID, results);

                // return request object.
                request = internalRequest;

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Begins the playback raw data from the historian database for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="maxValues">The number of values to be read for each item.</param>      
        /// <param name="updateInterval">The frequency, in seconds, that the server send data.</param>
        /// <param name="playbackDuration">The duration, in seconds, of the timespan returned with each update.</param>
        /// <param name="items">The set of items to read (must include the server handle).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] PlaybackRaw(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            int maxValues,
            decimal updateInterval,
            decimal playbackDuration,
            OpcItem[] items,
            object requestHandle,
            TsCHdaDataUpdateEventHandler callback,
            out IOpcRequest request)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            request = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                var internalRequest = callback_.CreateRequest(requestHandle, callback);

                // initialize input parameters.
                var requestID = internalRequest.RequestID;
                var cancelID = 0;

                var serverHandles = GetServerHandles(items);

                var pStartTime = Interop.GetTime(startTime);
                var pEndTime = Interop.GetTime(endTime);
                var ftUpdateInterval = Interop.GetFILETIME(updateInterval);
                var ftUpdateDuration = Interop.GetFILETIME(playbackDuration);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_Playback)server_).ReadRawWithUpdate(
                        internalRequest.RequestID,
                        ref pStartTime,
                        ref pEndTime,
                        maxValues,
                        ftUpdateDuration,
                        ftUpdateInterval,
                        serverHandles.Length,
                        serverHandles,
                        out cancelID,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_Playback.ReadRawWithUpdate", e);
                }

                // create result objects.
                var results = new OpcItemResult[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    results[ii] = new OpcItemResult();
                }

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // send callbacks for any data that has already arrived.
                internalRequest.Update(cancelID, results);

                // return request object.
                request = internalRequest;

                // completed successfully.
                return results;
            }
        }

        #endregion

        #region Read Processed

        /// <summary>
        /// Reads processed data from the historian database for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="resampleInterval">The interval between returned values.</param>
        /// <param name="items">The set of items to read (must include the server handle).</param>
        /// <returns>A set of values, qualities and timestamps within the requested time range for each item.</returns>
        public TsCHdaItemValueCollection[] ReadProcessed(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            decimal resampleInterval,
            TsCHdaItem[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new TsCHdaItemValueCollection[0];
                }

                // initialize input parameters.
                var serverHandles = GetServerHandles(items);
                var aggregateIDs = GetAggregateIDs(items);

                var pStartTime = Interop.GetTime(startTime);
                var pEndTime = Interop.GetTime(endTime);
                var ftResampleInterval = Interop.GetFILETIME(resampleInterval);

                // initialize output arguments.
                var pValues = IntPtr.Zero;
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_SyncRead)server_).ReadProcessed(
                        ref pStartTime,
                        ref pEndTime,
                        ftResampleInterval,
                        serverHandles.Length,
                        serverHandles,
                        aggregateIDs,
                        out pValues,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_SyncRead.ReadProcessed", e);
                }

                // unmarhal modified item structures.
                var results = Interop.GetItemValueCollections(ref pValues, items.Length, true);

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // store actual items in result.
                UpdateActualTimes(results, pStartTime, pEndTime);

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Sends an asynchronous request to read processed data from the historian database for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="resampleInterval">The interval between returned values.</param>
        /// <param name="items">The set of items to read (must include the server handle).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] ReadProcessed(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            decimal resampleInterval,
            TsCHdaItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            request = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                var internalRequest = callback_.CreateRequest(requestHandle, callback);

                // initialize input parameters.
                var requestID = internalRequest.RequestID;
                var cancelID = 0;

                var serverHandles = GetServerHandles(items);
                var aggregateIDs = GetAggregateIDs(items);

                var pStartTime = Interop.GetTime(startTime);
                var pEndTime = Interop.GetTime(endTime);
                var ftResampleInterval = Interop.GetFILETIME(resampleInterval);


                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_AsyncRead)server_).ReadProcessed(
                        internalRequest.RequestID,
                        ref pStartTime,
                        ref pEndTime,
                        ftResampleInterval,
                        serverHandles.Length,
                        serverHandles,
                        aggregateIDs,
                        out cancelID,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_AsyncRead.ReadProcessed", e);
                }

                // create result objects.
                var results = new OpcItemResult[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    results[ii] = new OpcItemResult();
                }

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // check if request has already completed.
                if (internalRequest.Update(cancelID, results))
                {
                    // discard the request.
                    request = null;
                    callback_.CancelRequest(internalRequest, (TsCHdaCancelCompleteEventHandler)null);

                    // return results.
                    return results;
                }

                // store actual items in request object.
                UpdateActualTimes(new ITsCHdaActualTime[] { internalRequest }, pStartTime, pEndTime);

                // return request object.
                request = internalRequest;

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Requests that the server periodically send notifications when new data becomes available for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="resampleInterval">The interval between returned values.</param>
        /// <param name="numberOfIntervals">The number of resample intervals that the server should return in each callback.</param>
        /// <param name="items">The set of items to read (must include the server handle).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] AdviseProcessed(
            TsCHdaTime startTime,
            decimal resampleInterval,
            int numberOfIntervals,
            TsCHdaItem[] items,
            object requestHandle,
            TsCHdaDataUpdateEventHandler callback,
            out IOpcRequest request)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            request = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                var internalRequest = callback_.CreateRequest(requestHandle, callback);

                // initialize input parameters.
                var requestID = internalRequest.RequestID;
                var cancelID = 0;

                var serverHandles = GetServerHandles(items);
                var aggregateIDs = GetAggregateIDs(items);

                var pStartTime = Interop.GetTime(startTime);
                var ftResampleInterval = Interop.GetFILETIME(resampleInterval);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_AsyncRead)server_).AdviseProcessed(
                        internalRequest.RequestID,
                        ref pStartTime,
                        ftResampleInterval,
                        serverHandles.Length,
                        serverHandles,
                        aggregateIDs,
                        numberOfIntervals,
                        out cancelID,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_AsyncRead.AdviseProcessed", e);
                }

                // create result objects.
                var results = new OpcItemResult[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    results[ii] = new OpcItemResult();
                }

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // send callbacks for any data that has already arrived.
                internalRequest.Update(cancelID, results);

                // return request object.
                request = internalRequest;

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Begins the playback of processed data from the historian database for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="resampleInterval">The interval between returned values.</param>
        /// <param name="numberOfIntervals">The number of resample intervals that the server should return in each callback.</param>
        /// <param name="updateInterval">The frequency, in seconds, that the server send data.</param>
        /// <param name="items">The set of items to read (must include the server handle).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] PlaybackProcessed(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            decimal resampleInterval,
            int numberOfIntervals,
            decimal updateInterval,
            TsCHdaItem[] items,
            object requestHandle,
            TsCHdaDataUpdateEventHandler callback,
            out IOpcRequest request)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            request = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                var internalRequest = callback_.CreateRequest(requestHandle, callback);

                // initialize input parameters.
                var requestID = internalRequest.RequestID;
                var cancelID = 0;

                var serverHandles = GetServerHandles(items);
                var aggregateIDs = GetAggregateIDs(items);

                var pStartTime = Interop.GetTime(startTime);
                var pEndTime = Interop.GetTime(endTime);
                var ftResampleInterval = Interop.GetFILETIME(resampleInterval);
                var ftUpdateInterval = Interop.GetFILETIME(updateInterval);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_Playback)server_).ReadProcessedWithUpdate(
                        internalRequest.RequestID,
                        ref pStartTime,
                        ref pEndTime,
                        ftResampleInterval,
                        numberOfIntervals,
                        ftUpdateInterval,
                        serverHandles.Length,
                        serverHandles,
                        aggregateIDs,
                        out cancelID,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_Playback.ReadProcessedWithUpdate", e);
                }

                // create result objects.
                var results = new OpcItemResult[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    results[ii] = new OpcItemResult();
                }

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // send callbacks for any data that has already arrived.
                internalRequest.Update(cancelID, results);

                // return request object.
                request = internalRequest;

                // completed successfully.
                return results;
            }
        }

        #endregion

        #region Read At Time

        /// <summary>
        /// Reads data from the historian database for a set of items at specific times.
        /// </summary>
        /// <param name="timestamps">The set of timestamps to use when reading items values.</param>
        /// <param name="items">The set of items to read (must include the server handle).</param>
        /// <returns>A set of values, qualities and timestamps within the requested time range for each item.</returns>
        public TsCHdaItemValueCollection[] ReadAtTime(DateTime[] timestamps, OpcItem[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new TsCHdaItemValueCollection[0];
                }

                // initialize input parameters.
                var serverHandles = GetServerHandles(items);
                var ftTimestamps = Interop.GetFILETIMEs(timestamps);

                // initialize output arguments.
                var pValues = IntPtr.Zero;
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_SyncRead)server_).ReadAtTime(
                        ftTimestamps.Length,
                        ftTimestamps,
                        serverHandles.Length,
                        serverHandles,
                        out pValues,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_SyncRead.ReadAtTime", e);
                }

                // unmarhal modified item structures.
                var results = Interop.GetItemValueCollections(ref pValues, items.Length, true);

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Sends an asynchronous request to read item values at specific times.
        /// </summary>
        /// <param name="timestamps">The set of timestamps to use when reading items values.</param>
        /// <param name="items">The set of items to read (must include the server handle).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] ReadAtTime(
            DateTime[] timestamps,
            OpcItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            request = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                var internalRequest = callback_.CreateRequest(requestHandle, callback);

                // initialize input parameters.
                var requestID = internalRequest.RequestID;
                var cancelID = 0;

                var serverHandles = GetServerHandles(items);
                var ftTimestamps = Interop.GetFILETIMEs(timestamps);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_AsyncRead)server_).ReadAtTime(
                        internalRequest.RequestID,
                        ftTimestamps.Length,
                        ftTimestamps,
                        serverHandles.Length,
                        serverHandles,
                        out cancelID,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_AsyncRead.ReadAtTime", e);
                }

                // create result objects.
                var results = new OpcItemResult[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    results[ii] = new OpcItemResult();
                }

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // check if request has already completed.
                if (internalRequest.Update(cancelID, results))
                {
                    // discard the request.
                    request = null;
                    callback_.CancelRequest(internalRequest, (TsCHdaCancelCompleteEventHandler)null);

                    // return results.
                    return results;
                }

                // return request object.
                request = internalRequest;

                // completed successfully.
                return results;
            }
        }

        #endregion

        #region Read Modified

        /// <summary>
        /// Reads item values that have been deleted or replaced.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="maxValues">The number of values to be read for each item.</param>
        /// <param name="items">The set of items to read (must include the server handle).</param>
        /// <returns>A set of values, qualities and timestamps within the requested time range for each item.</returns>
        public TsCHdaModifiedValueCollection[] ReadModified(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            int maxValues,
            OpcItem[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new TsCHdaModifiedValueCollection[0];
                }

                // initialize input parameters.
                var serverHandles = GetServerHandles(items);

                var pStartTime = Interop.GetTime(startTime);
                var pEndTime = Interop.GetTime(endTime);

                // initialize output arguments.
                var pValues = IntPtr.Zero;
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_SyncRead)server_).ReadModified(
                        ref pStartTime,
                        ref pEndTime,
                        maxValues,
                        serverHandles.Length,
                        serverHandles,
                        out pValues,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_SyncRead.ReadModified", e);
                }

                // unmarhal modified item structures.
                var results = Interop.GetModifiedValueCollections(ref pValues, items.Length, true);

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // store actual items in result.
                UpdateActualTimes(results, pStartTime, pEndTime);

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Sends an asynchronous request to read item values that have been deleted or replaced.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="maxValues">The number of values to be read for each item.</param>
        /// <param name="items">The set of items to read (must include the server handle).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] ReadModified(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            int maxValues,
            OpcItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            request = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                var internalRequest = callback_.CreateRequest(requestHandle, callback);

                // initialize input parameters.
                var requestID = internalRequest.RequestID;
                var cancelID = 0;

                var serverHandles = GetServerHandles(items);

                var pStartTime = Interop.GetTime(startTime);
                var pEndTime = Interop.GetTime(endTime);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_AsyncRead)server_).ReadModified(
                        internalRequest.RequestID,
                        ref pStartTime,
                        ref pEndTime,
                        maxValues,
                        serverHandles.Length,
                        serverHandles,
                        out cancelID,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_AsyncRead.ReadModified", e);
                }

                // create result objects.
                var results = new OpcItemResult[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    results[ii] = new OpcItemResult();
                }

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // check if request has already completed.
                if (internalRequest.Update(cancelID, results))
                {
                    // discard the request.
                    request = null;
                    callback_.CancelRequest(internalRequest, (TsCHdaCancelCompleteEventHandler)null);

                    // return results.
                    return results;
                }

                // store actual items in request object.
                UpdateActualTimes(new ITsCHdaActualTime[] { internalRequest }, pStartTime, pEndTime);

                // return request object.
                request = internalRequest;

                // completed successfully.
                return results;
            }
        }

        #endregion

        #region Read Attributes

        /// <summary>
        /// Reads the current or historical values for the attributes of an item.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="item">The item to read (must include the server handle).</param>
        /// <param name="attributeIDs">The attributes to read.</param>
        /// <returns>A set of attribute values for each requested attribute.</returns>
        public TsCHdaItemAttributeCollection ReadAttributes(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem item,
            int[] attributeIDs)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (attributeIDs == null) throw new ArgumentNullException(nameof(attributeIDs));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (attributeIDs.Length == 0)
                {
                    return new TsCHdaItemAttributeCollection(item);
                }

                // initialize input parameters.
                var serverHandles = GetServerHandles(new OpcItem[] { item });

                var pStartTime = Interop.GetTime(startTime);
                var pEndTime = Interop.GetTime(endTime);

                // initialize output arguments.
                var pValues = IntPtr.Zero;
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_SyncRead)server_).ReadAttribute(
                        ref pStartTime,
                        ref pEndTime,
                        serverHandles[0],
                        attributeIDs.Length,
                        attributeIDs,
                        out pValues,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_SyncRead.ReadAttribute", e);
                }

                // unmarhal item attribute structures.
                var attributes = Interop.GetAttributeValueCollections(ref pValues, attributeIDs.Length, true);

                // create item level result collection. 
                var result = UpdateResults(item, attributes, ref pErrors);

                // store actual items in result.
                UpdateActualTimes(new ITsCHdaActualTime[] { result }, pStartTime, pEndTime);

                // completed successfully.
                return result;
            }
        }

        /// <summary>
        /// Sends an asynchronous request to read the attributes of an item.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="item">The item to read (must include the server handle).</param>
        /// <param name="attributeIDs">The attributes to read.</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the attribute ids.</returns>
        public TsCHdaResultCollection ReadAttributes(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem item,
            int[] attributeIDs,
            object requestHandle,
            TsCHdaReadAttributesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (attributeIDs == null) throw new ArgumentNullException(nameof(attributeIDs));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            request = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (attributeIDs.Length == 0)
                {
                    return new TsCHdaResultCollection();
                }

                var internalRequest = callback_.CreateRequest(requestHandle, callback);

                // initialize input parameters.
                var requestID = internalRequest.RequestID;
                var cancelID = 0;

                var serverHandles = GetServerHandles(new OpcItem[] { item });

                var pStartTime = Interop.GetTime(startTime);
                var pEndTime = Interop.GetTime(endTime);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_AsyncRead)server_).ReadAttribute(
                        internalRequest.RequestID,
                        ref pStartTime,
                        ref pEndTime,
                        serverHandles[0],
                        attributeIDs.Length,
                        attributeIDs,
                        out cancelID,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_AsyncRead.ReadAttribute", e);
                }

                // create result objects.
                var results = new TsCHdaResultCollection(item);

                // update result with error code and info from the item argument.
                UpdateResult(item, results, 0);

                // unmarshal return parameters and free memory.
                var errors = Utilities.Interop.GetInt32s(ref pErrors, attributeIDs.Length, true);

                // verify return parameters.
                if (errors == null)
                {
                    throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
                }

                // add results for each attribute.
                foreach (var error in errors)
                {
                    var result = new TsCHdaResult(Utilities.Interop.GetResultId(error));
                    results.Add(result);
                }

                // check if request has already completed.
                if (internalRequest.Update(cancelID, new TsCHdaResultCollection[] { results }))
                {
                    // discard the request.
                    request = null;
                    callback_.CancelRequest(internalRequest, (TsCHdaCancelCompleteEventHandler)null);

                    // return results.
                    return results;
                }

                // store actual items in request object.
                UpdateActualTimes(new ITsCHdaActualTime[] { internalRequest }, pStartTime, pEndTime);

                // return request object.
                request = internalRequest;

                // completed successfully.
                return results;
            }
        }

        #endregion

        #region Annotations

        /// <summary>
        /// Reads any annotations for an item within the a time interval.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="items">The set of items to read (must include the server handle).</param>
        /// <returns>A set of annotations within the requested time range for each item.</returns>
        public TsCHdaAnnotationValueCollection[] ReadAnnotations(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new TsCHdaAnnotationValueCollection[0];
                }

                // initialize input parameters.
                var serverHandles = GetServerHandles(items);

                var pStartTime = Interop.GetTime(startTime);
                var pEndTime = Interop.GetTime(endTime);

                // initialize output arguments.
                var pValues = IntPtr.Zero;
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_SyncAnnotations)server_).Read(
                        ref pStartTime,
                        ref pEndTime,
                        serverHandles.Length,
                        serverHandles,
                        out pValues,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_SyncAnnotations.Read", e);
                }

                // unmarhal modified item structures.
                var results = Interop.GetAnnotationValueCollections(ref pValues, items.Length, true);

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // store actual items in result.
                UpdateActualTimes(results, pStartTime, pEndTime);

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Sends an asynchronous request to read the annotations for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="items">The set of items to read (must include the server handle).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] ReadAnnotations(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem[] items,
            object requestHandle,
            TsCHdaReadAnnotationsCompleteEventHandler callback,
            out IOpcRequest request)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            request = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                var internalRequest = callback_.CreateRequest(requestHandle, callback);

                // initialize input parameters.
                var requestID = internalRequest.RequestID;
                var cancelID = 0;

                var serverHandles = GetServerHandles(items);

                var pStartTime = Interop.GetTime(startTime);
                var pEndTime = Interop.GetTime(endTime);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_AsyncAnnotations)server_).Read(
                        internalRequest.RequestID,
                        ref pStartTime,
                        ref pEndTime,
                        serverHandles.Length,
                        serverHandles,
                        out cancelID,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_AsyncAnnotations.Read", e);
                }

                // create result objects.
                var results = new OpcItemResult[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    results[ii] = new OpcItemResult();
                }

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // check if request has already completed.
                if (internalRequest.Update(cancelID, results))
                {
                    // discard the request.
                    request = null;
                    callback_.CancelRequest(internalRequest, (TsCHdaCancelCompleteEventHandler)null);

                    // return results.
                    return results;
                }

                // store actual items in request object.
                UpdateActualTimes(new ITsCHdaActualTime[] { internalRequest }, pStartTime, pEndTime);

                // return request object.
                request = internalRequest;

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Inserts annotations for one or more items.
        /// </summary>
        /// <param name="items">A list of annotations to add for each item (must include the server handle).</param>
        /// <returns>The results of the insert operation for each annotation set.</returns>
        public TsCHdaResultCollection[] InsertAnnotations(TsCHdaAnnotationValueCollection[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new TsCHdaResultCollection[0];
                }

                // create empty set of result collections.
                var results = CreateResultCollections(items);

                // initialize input parameters.
                int[] serverHandles = null;
                OPCHDA_ANNOTATION[] pAnnotations = null;
                OPCHDA_FILETIME[] pTimestamps = null;

                // flatten out list of collections into a set of single arrays.
                var count = MarshalAnnotatations(
                    items,
                    ref serverHandles,
                    ref pTimestamps,
                    ref pAnnotations);

                // handle trivial case.
                if (count == 0)
                {
                    return results;
                }

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_SyncAnnotations)server_).Insert(
                        serverHandles.Length,
                        serverHandles,
                        pTimestamps,
                        pAnnotations,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_SyncAnnotations.Insert", e);
                }

                // free memory allocated for input arguments.
                for (var ii = 0; ii < pAnnotations.Length; ii++)
                {
                    Utilities.Interop.GetDateTimes(ref pAnnotations[ii].ftTimeStamps, 1, true);
                    Utilities.Interop.GetUnicodeStrings(ref pAnnotations[ii].szAnnotation, 1, true);
                    Utilities.Interop.GetDateTimes(ref pAnnotations[ii].ftAnnotationTime, 1, true);
                    Utilities.Interop.GetUnicodeStrings(ref pAnnotations[ii].szUser, 1, true);
                }

                // unmarshal return parameters and free memory.
                UpdateResults(items, results, count, ref pErrors);

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Sends an asynchronous request to inserts annotations for one or more items.
        /// </summary>
        /// <param name="items">A list of annotations to add for each item (must include the server handle).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] InsertAnnotations(
            TsCHdaAnnotationValueCollection[] items,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            request = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                // create empty set of result collections.
                var results = CreateResultCollections(items);

                // initialize input parameters.
                int[] serverHandles = null;
                OPCHDA_ANNOTATION[] pAnnotations = null;
                OPCHDA_FILETIME[] pTimestamps = null;

                // flatten out list of collections into a set of single arrays.
                var count = MarshalAnnotatations(
                    items,
                    ref serverHandles,
                    ref pTimestamps,
                    ref pAnnotations);

                // handle trivial case.
                if (count == 0)
                {
                    return GetIdentifiedResults(results);
                }

                // create request.
                var internalRequest = callback_.CreateRequest(requestHandle, callback);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                var cancelID = 0;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_AsyncAnnotations)server_).Insert(
                        internalRequest.RequestID,
                        serverHandles.Length,
                        serverHandles,
                        pTimestamps,
                        pAnnotations,
                        out cancelID,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_AsyncAnnotations.Insert", e);
                }

                // free memory allocated for input arguments.
                for (var ii = 0; ii < pAnnotations.Length; ii++)
                {
                    Utilities.Interop.GetDateTimes(ref pAnnotations[ii].ftTimeStamps, 1, true);
                    Utilities.Interop.GetUnicodeStrings(ref pAnnotations[ii].szAnnotation, 1, true);
                    Utilities.Interop.GetDateTimes(ref pAnnotations[ii].ftAnnotationTime, 1, true);
                    Utilities.Interop.GetUnicodeStrings(ref pAnnotations[ii].szUser, 1, true);
                }

                // unmarshal return parameters and free memory.
                UpdateResults(items, results, count, ref pErrors);

                // check if request has already completed.
                if (internalRequest.Update(cancelID, results))
                {
                    // discard the request.
                    request = null;
                    callback_.CancelRequest(internalRequest, (TsCHdaCancelCompleteEventHandler)null);

                    // return results.
                    return GetIdentifiedResults(results);
                }

                // return request object.
                request = internalRequest;

                // completed successfully.
                return GetIdentifiedResults(results);
            }
        }

        #endregion

        #region Insert/Replace

        /// <summary>
        /// Inserts the values into the history database for one or more items. 
        /// </summary>
        /// <param name="items">The set of values to insert.</param>
        /// <param name="replace">Whether existing values should be replaced.</param>
        /// <returns></returns>
        public TsCHdaResultCollection[] Insert(TsCHdaItemValueCollection[] items, bool replace)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new TsCHdaResultCollection[0];
                }

                // create empty set of result collections.
                var results = CreateResultCollections(items);

                // initialize input parameters.
                int[] serverHandles = null;
                object[] values = null;
                int[] qualities = null;
                DateTime[] timestamps = null;

                // flatten out list of collections into a set of single arrays.
                var count = MarshalValues(
                    items,
                    ref serverHandles,
                    ref values,
                    ref qualities,
                    ref timestamps);

                // handle trivial case.
                if (count == 0)
                {
                    return results;
                }

                var ftTimestamps = Interop.GetFILETIMEs(timestamps);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                if (replace)
                {
                    try
                    {
                        ((IOPCHDA_SyncUpdate)server_).InsertReplace(
                            serverHandles.Length,
                            serverHandles,
                            ftTimestamps,
                            values,
                            qualities,
                            out pErrors);
                    }
                    catch (Exception e)
                    {
                        throw Utilities.Interop.CreateException("IOPCHDA_SyncUpdate.InsertReplace", e);
                    }
                }
                else
                {
                    try
                    {
                        ((IOPCHDA_SyncUpdate)server_).Insert(
                            serverHandles.Length,
                            serverHandles,
                            ftTimestamps,
                            values,
                            qualities,
                            out pErrors);
                    }
                    catch (Exception e)
                    {
                        throw Utilities.Interop.CreateException("IOPCHDA_SyncUpdate.Insert", e);
                    }
                }

                // unmarshal return parameters and free memory.
                UpdateResults(items, results, count, ref pErrors);

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Sends an asynchronous request to inserts values for one or more items.
        /// </summary>
        /// <param name="items">The set of values to insert.</param>
        /// <param name="replace">Whether existing values should be replaced.</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] Insert(
            TsCHdaItemValueCollection[] items,
            bool replace,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            request = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                // create empty set of result collections.
                var results = CreateResultCollections(items);

                // initialize input parameters.
                int[] serverHandles = null;
                object[] values = null;
                int[] qualities = null;
                DateTime[] timestamps = null;

                // flatten out list of collections into a set of single arrays.
                var count = MarshalValues(
                    items,
                    ref serverHandles,
                    ref values,
                    ref qualities,
                    ref timestamps);

                // handle trivial case.
                if (count == 0)
                {
                    return GetIdentifiedResults(results);
                }

                var ftTimestamps = Interop.GetFILETIMEs(timestamps);

                // create request.
                var internalRequest = callback_.CreateRequest(requestHandle, callback);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                var cancelID = 0;

                // invoke COM method.
                if (replace)
                {
                    try
                    {
                        ((IOPCHDA_AsyncUpdate)server_).InsertReplace(
                            internalRequest.RequestID,
                            serverHandles.Length,
                            serverHandles,
                            ftTimestamps,
                            values,
                            qualities,
                            out cancelID,
                            out pErrors);
                    }
                    catch (Exception e)
                    {
                        throw Utilities.Interop.CreateException("IOPCHDA_AsyncUpdate.InsertReplace", e);
                    }
                }
                else
                {
                    try
                    {
                        ((IOPCHDA_AsyncUpdate)server_).Insert(
                            internalRequest.RequestID,
                            serverHandles.Length,
                            serverHandles,
                            ftTimestamps,
                            values,
                            qualities,
                            out cancelID,
                            out pErrors);
                    }
                    catch (Exception e)
                    {
                        throw Utilities.Interop.CreateException("IOPCHDA_AsyncUpdate.Insert", e);
                    }
                }

                // unmarshal return parameters and free memory.
                UpdateResults(items, results, count, ref pErrors);

                // check if request has already completed.
                if (internalRequest.Update(cancelID, results))
                {
                    // discard the request.
                    request = null;
                    callback_.CancelRequest(internalRequest, (TsCHdaCancelCompleteEventHandler)null);

                    // return results.
                    return GetIdentifiedResults(results);
                }

                // return request object.
                request = internalRequest;

                // completed successfully.
                return GetIdentifiedResults(results);
            }
        }

        /// <summary>
        /// Replace the values into the history database for one or more items. 
        /// </summary>
        /// <param name="items">The set of values to replace.</param>
        /// <returns></returns>
        public TsCHdaResultCollection[] Replace(TsCHdaItemValueCollection[] items)
        {
            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new TsCHdaResultCollection[0];
                }

                // create empty set of result collections.
                var results = CreateResultCollections(items);

                // initialize input parameters.
                int[] serverHandles = null;
                object[] values = null;
                int[] qualities = null;
                DateTime[] timestamps = null;

                // flatten out list of collections into a set of single arrays.
                var count = MarshalValues(
                    items,
                    ref serverHandles,
                    ref values,
                    ref qualities,
                    ref timestamps);

                // handle trivial case.
                if (count == 0)
                {
                    return results;
                }

                var ftTimestamps = Interop.GetFILETIMEs(timestamps);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_SyncUpdate)server_).Replace(
                        serverHandles.Length,
                        serverHandles,
                        ftTimestamps,
                        values,
                        qualities,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_SyncUpdate.Replace", e);
                }

                // unmarshal return parameters and free memory.
                UpdateResults(items, results, count, ref pErrors);

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Sends an asynchronous request to replace values for one or more items.
        /// </summary>
        /// <param name="items">The set of values to replace.</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] Replace(
            TsCHdaItemValueCollection[] items,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            request = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                // create empty set of result collections.
                var results = CreateResultCollections(items);

                // initialize input parameters.
                int[] serverHandles = null;
                object[] values = null;
                int[] qualities = null;
                DateTime[] timestamps = null;

                // flatten out list of collections into a set of single arrays.
                var count = MarshalValues(
                    items,
                    ref serverHandles,
                    ref values,
                    ref qualities,
                    ref timestamps);

                // handle trivial case.
                if (count == 0)
                {
                    return GetIdentifiedResults(results);
                }

                var ftTimestamps = Interop.GetFILETIMEs(timestamps);

                // create request.
                var internalRequest = callback_.CreateRequest(requestHandle, callback);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                var cancelID = 0;

                try
                {
                    ((IOPCHDA_AsyncUpdate)server_).Replace(
                        internalRequest.RequestID,
                        serverHandles.Length,
                        serverHandles,
                        ftTimestamps,
                        values,
                        qualities,
                        out cancelID,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_AsyncUpdate.Replace", e);
                }

                // unmarshal return parameters and free memory.
                UpdateResults(items, results, count, ref pErrors);

                // check if request has already completed.
                if (internalRequest.Update(cancelID, results))
                {
                    // discard the request.
                    request = null;
                    callback_.CancelRequest(internalRequest, (TsCHdaCancelCompleteEventHandler)null);

                    // return results.
                    return GetIdentifiedResults(results);
                }

                // return request object.
                request = internalRequest;

                // completed successfully.
                return GetIdentifiedResults(results);
            }
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes the values with the specified time domain for one or more items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to delete.</param>
        /// <param name="endTime">The end of the history period to be delete.</param>
        /// <param name="items">The set of items to delete (must include the server handle).</param>
        /// <returns>The results of the delete operation for each item.</returns>
        public OpcItemResult[] Delete(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                // initialize input parameters.
                var serverHandles = GetServerHandles(items);

                var pStartTime = Interop.GetTime(startTime);
                var pEndTime = Interop.GetTime(endTime);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_SyncUpdate)server_).DeleteRaw(
                        ref pStartTime,
                        ref pEndTime,
                        serverHandles.Length,
                        serverHandles,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_SyncUpdate.DeleteRaw", e);
                }

                // create result objects.
                var results = new OpcItemResult[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    results[ii] = new OpcItemResult();
                }

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Sends an asynchronous request to delete values for one or more items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to delete.</param>
        /// <param name="endTime">The end of the history period to be delete.</param>
        /// <param name="items">The set of items to delete (must include the server handle).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] Delete(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem[] items,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            request = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                var internalRequest = callback_.CreateRequest(requestHandle, callback);

                // initialize input parameters.
                var requestID = internalRequest.RequestID;
                var cancelID = 0;

                var serverHandles = GetServerHandles(items);

                var pStartTime = Interop.GetTime(startTime);
                var pEndTime = Interop.GetTime(endTime);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_AsyncUpdate)server_).DeleteRaw(
                        internalRequest.RequestID,
                        ref pStartTime,
                        ref pEndTime,
                        serverHandles.Length,
                        serverHandles,
                        out cancelID,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_AsyncUpdate.DeleteRaw", e);
                }

                // create result objects.
                var results = new OpcItemResult[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    results[ii] = new OpcItemResult();
                }

                // update result with error code and info from the item argument.
                UpdateResults(items, results, ref pErrors);

                // check if request has already completed.
                if (internalRequest.Update(cancelID, results))
                {
                    // discard the request.
                    request = null;
                    callback_.CancelRequest(internalRequest, (TsCHdaCancelCompleteEventHandler)null);

                    // return results.
                    return results;
                }

                // store actual items in request object.
                UpdateActualTimes(new ITsCHdaActualTime[] { internalRequest }, pStartTime, pEndTime);

                // return request object.
                request = internalRequest;

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Deletes the values at the specified times for one or more items. 
        /// </summary>
        /// <param name="items">The set of timestamps to delete for one or more items.</param>
        /// <returns>The results of the operation for each timestamp.</returns>
        public TsCHdaResultCollection[] DeleteAtTime(TsCHdaItemTimeCollection[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new TsCHdaResultCollection[0];
                }

                // create empty set of result collections.
                var results = CreateResultCollections(items);

                // initialize input parameters.
                int[] serverHandles = null;
                DateTime[] timestamps = null;

                // flatten out list of collections into a set of single arrays.
                var count = MarshalTimestamps(
                    items,
                    ref serverHandles,
                    ref timestamps);

                // handle trivial case.
                if (count == 0)
                {
                    return results;
                }

                var ftTimestamps = Interop.GetFILETIMEs(timestamps);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                // invoke COM method.
                try
                {
                    ((IOPCHDA_SyncUpdate)server_).DeleteAtTime(
                        serverHandles.Length,
                        serverHandles,
                        ftTimestamps,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_SyncUpdate.DeleteAtTime", e);
                }

                // unmarshal return parameters and free memory.
                UpdateResults(items, results, count, ref pErrors);

                // completed successfully.
                return results;
            }
        }

        /// <summary>
        /// Sends an asynchronous request to delete values for one or more items at a specified times.
        /// </summary>
        /// <param name="items">The set of timestamps to delete for one or more items.</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] DeleteAtTime(
            TsCHdaItemTimeCollection[] items,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            request = null;

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                // handle trivial case.
                if (items.Length == 0)
                {
                    return new OpcItemResult[0];
                }

                // create empty set of result collections.
                var results = CreateResultCollections(items);

                // initialize input parameters.
                int[] serverHandles = null;
                DateTime[] timestamps = null;

                // flatten out list of collections into a set of single arrays.
                var count = MarshalTimestamps(
                    items,
                    ref serverHandles,
                    ref timestamps);

                // handle trivial case.
                if (count == 0)
                {
                    return GetIdentifiedResults(results);
                }

                var ftTimestamps = Interop.GetFILETIMEs(timestamps);

                // create request.
                var internalRequest = callback_.CreateRequest(requestHandle, callback);

                // initialize output arguments.
                var pErrors = IntPtr.Zero;

                var cancelID = 0;

                try
                {
                    ((IOPCHDA_AsyncUpdate)server_).DeleteAtTime(
                        internalRequest.RequestID,
                        serverHandles.Length,
                        serverHandles,
                        ftTimestamps,
                        out cancelID,
                        out pErrors);
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCHDA_AsyncUpdate.DeleteAtTime", e);
                }

                // unmarshal return parameters and free memory.
                UpdateResults(items, results, count, ref pErrors);

                // check if request has already completed.
                if (internalRequest.Update(cancelID, results))
                {
                    // discard the request.
                    request = null;
                    callback_.CancelRequest(internalRequest, (TsCHdaCancelCompleteEventHandler)null);

                    // return results.
                    return GetIdentifiedResults(results);
                }

                // return request object.
                request = internalRequest;

                // completed successfully.
                return GetIdentifiedResults(results);
            }
        }

        #endregion

        #region Cancel
        //======================================================================
        // CancelRequest

        /// <summary>
        /// Cancels an asynchronous request.
        /// </summary>
        /// <param name="request">The state object for the request to cancel.</param>
        public void CancelRequest(IOpcRequest request)
        {
            CancelRequest(request, (TsCHdaCancelCompleteEventHandler)null);
        }

        /// <summary>
        /// Cancels an asynchronous request.
        /// </summary>
        /// <param name="request">The state object for the request to cancel.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        public void CancelRequest(IOpcRequest request, TsCHdaCancelCompleteEventHandler callback)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                var internalRequest = (Request)request;

                // register the cancel request callback.
                callback_.CancelRequest(internalRequest, callback);

                // invoke COM method.
                try
                {
                    ((IOPCHDA_AsyncRead)server_).Cancel(internalRequest.CancelID);
                }
                catch (Exception e)
                {
                    // a return code of E_FAIL indicates the request does not exist or can't be cancelled.
                    if (OpcResult.E_FAIL.Code != Marshal.GetHRForException(e))
                    {
                        throw Utilities.Interop.CreateException("IOPCHDA_AsyncRead.Cancel", e);
                    }
                }
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Establishes a connection point callback with the COM server.
        /// </summary>
        private void Advise()
        {
            if (connection_ == null)
            {
                try
                {
                    connection_ = new ConnectionPoint(server_, typeof(IOPCHDA_DataCallback).GUID);
                    connection_.Advise(callback_);
                }
                catch
                {
                    connection_ = null;
                }
            }
        }

        /// <summary>
        /// Closes a connection point callback with the COM server.
        /// </summary>
        private void Unadvise()
        {
            if (connection_ != null)
            {
                if (connection_.Unadvise() == 0)
                {
                    connection_.Dispose();
                    connection_ = null;
                }
            }
        }

        /// <summary>
        /// Creates a unique handle for an item.
        /// </summary>
        private int CreateHandle()
        {
            return nextHandle_++;
        }

        /// <summary>
        /// Finds an invalid server handle.
        /// </summary>
        private int GetInvalidHandle()
        {
            var max = 0;

            foreach (OpcItem item in items_.Values)
            {
                var handle = (int)item.ServerHandle;

                if (max < handle)
                {
                    max = handle;
                }
            }

            return max + 1;
        }

        /// <summary>
        /// Gets the total count for multiple collections.
        /// </summary>
        private int GetCount(ICollection[] collections)
        {
            var count = 0;

            if (collections != null)
            {
                foreach (var collection in collections)
                {
                    if (collection != null)
                    {
                        count += collection.Count;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Initializes a set of result collections from a set of item ids.
        /// </summary>
        TsCHdaResultCollection[] CreateResultCollections(OpcItem[] items)
        {
            TsCHdaResultCollection[] results = null;

            if (items != null)
            {
                results = new TsCHdaResultCollection[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    results[ii] = new TsCHdaResultCollection();

                    if (items[ii] != null)
                    {
                        UpdateResult(items[ii], results[ii], 0);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Returns an array of item server handles.
        /// </summary>
        private int[] GetServerHandles(OpcItem[] items)
        {
            // use this if the client passes an unrecognized server handle.
            var invalidHandle = GetInvalidHandle();

            // create server handle array.
            var serverHandles = new int[items.Length];

            for (var ii = 0; ii < items.Length; ii++)
            {
                serverHandles[ii] = invalidHandle;

                if (items[ii] != null && items[ii].ServerHandle != null)
                {
                    // lookup cached handle.
                    var item = (OpcItem)items_[items[ii].ServerHandle];

                    if (item != null)
                    {
                        serverHandles[ii] = (int)item.ServerHandle;
                    }
                }
            }

            // return handles.
            return serverHandles;
        }

        /// <summary>
        /// Returns an array of item aggregate ids.
        /// </summary>
        private int[] GetAggregateIDs(TsCHdaItem[] items)
        {
            var aggregateIDs = new int[items.Length];

            for (var ii = 0; ii < items.Length; ii++)
            {
                aggregateIDs[ii] = 0;

                if (items[ii].Aggregate != TsCHdaAggregateID.NoAggregate)
                {
                    aggregateIDs[ii] = items[ii].Aggregate;
                }
            }

            return aggregateIDs;
        }

        /// <summary>
        /// Updates the result with locally cached item information.
        /// </summary>
        void UpdateResult(OpcItem item, OpcItem result, int error)
        {
            result.ItemName = item.ItemName;
            result.ItemPath = item.ItemPath;
            result.ClientHandle = item.ClientHandle;
            result.ServerHandle = item.ServerHandle;

            if (error >= 0 && item.ServerHandle != null)
            {
                // lookup locally cached item id.
                var itemID = (OpcItem)items_[item.ServerHandle];

                // update result with locally cached information.
                if (itemID != null)
                {
                    result.ItemName = itemID.ItemName;
                    result.ItemPath = itemID.ItemPath;
                    result.ClientHandle = itemID.ClientHandle;
                }
            }
        }

        /// <summary>
        /// Adds the actual start/end times to a result collection. 
        /// </summary>
        void UpdateActualTimes(
            ITsCHdaActualTime[] results,
            OPCHDA_TIME pStartTime,
            OPCHDA_TIME pEndTime)
        {
            // unmarshal actual times from input arguments.
			var startTime = Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIME(Interop.Convert(pStartTime.ftTime));
			var endTime   = Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIME(Interop.Convert(pEndTime.ftTime));

            foreach (var result in results)
            {
                result.StartTime = startTime;
                result.EndTime = endTime;
            }
        }

        /// <summary>
        /// Updates the attribute value objects before returing them to the client.
        /// </summary>
        TsCHdaItemAttributeCollection UpdateResults(
            OpcItem item,
            TsCHdaAttributeValueCollection[] attributes,
            ref IntPtr pErrors)
        {
            // unmarshal return parameters and free memory.
            var errors = Utilities.Interop.GetInt32s(ref pErrors, attributes.Length, true);

            // verify return parameters.
            if (attributes == null || errors == null)
            {
                throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
            }

            // set attribute level errors.
            for (var ii = 0; ii < attributes.Length; ii++)
            {
                attributes[ii].Result = Utilities.Interop.GetResultId(errors[ii]);
            }

            // create item level collection. 
            var result = new TsCHdaItemAttributeCollection();

            foreach (var attribute in attributes)
            {
                result.Add(attribute);
            }

            // add locally cached item information.
            UpdateResult(item, result, 0);

            // all done.
            return result;
        }

        /// <summary>
        /// Updates the annotation value objects before returing them to the client.
        /// </summary>
        void UpdateResults(
            OpcItem[] items,
            OpcItem[] results,
            ref IntPtr pErrors)
        {
            // unmarshal return parameters and free memory.
            var errors = Utilities.Interop.GetInt32s(ref pErrors, items.Length, true);

            // verify return parameters.
            if (results == null || errors == null)
            {
                throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
            }

            for (var ii = 0; ii < results.Length; ii++)
            {
                // get cached item information.
                UpdateResult(items[ii], results[ii], errors[ii]);

                // lookup the error code.
                if (typeof(IOpcResult).IsInstanceOfType(results[ii]))
                {
                    ((IOpcResult)results[ii]).Result = Utilities.Interop.GetResultId(errors[ii]);
                }
            }
        }

        /// <summary>
        /// Unmarshals the errors array and updates the result objects.
        /// </summary>
        void UpdateResults(ICollection[] items, TsCHdaResultCollection[] results, int count, ref IntPtr pErrors)
        {
            // unmarshal return parameters and free memory.
            var errors = Utilities.Interop.GetInt32s(ref pErrors, count, true);

            // verify return parameters.
            if (errors == null)
            {
                throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
            }

            // create result object and lookup error code.
            var index = 0;

            for (var ii = 0; ii < items.Length; ii++)
            {
                for (var jj = 0; jj < items[ii].Count; jj++)
                {
                    if (index >= count)
                    {
                        break;
                    }

                    var result = new TsCHdaResult(Utilities.Interop.GetResultId(errors[index++]));
                    results[ii].Add(result);
                }
            }
        }

        /// <summary>
        /// Flattens a set of item value collections into an set of single arrays.
        /// </summary>
        private int MarshalValues(
            TsCHdaItemValueCollection[] items,
            ref int[] handles,
            ref object[] values,
            ref int[] qualities,
            ref DateTime[] timestamps)
        {

            // determine the total length.
            var count = GetCount(items);

            // flatten out list of collections into a set of single arrays. 
            handles = new int[count];
            timestamps = new DateTime[count];
            values = new object[count];
            qualities = new int[count];

            // initialize input parameters.
            var serverHandles = GetServerHandles(items);

            var index = 0;

            for (var ii = 0; ii < items.Length; ii++)
            {
                foreach (TsCHdaItemValue value in items[ii])
                {
                    handles[index] = serverHandles[ii];
                    timestamps[index] = value.Timestamp;
                    values[index] = Utilities.Interop.GetVARIANT(value.Value);
                    qualities[index] = value.Quality.GetCode();

                    index++;
                }
            }

            // return the total count.
            return count;
        }

        /// <summary>
        /// Flattens a set of item time collections into an set of single arrays.
        /// </summary>
        private int MarshalTimestamps(
            TsCHdaItemTimeCollection[] items,
            ref int[] handles,
            ref DateTime[] timestamps)
        {

            // determine the total length.
            var count = GetCount(items);

            // flatten out list of collections into a set of single arrays. 
            handles = new int[count];
            timestamps = new DateTime[count];

            // initialize input parameters.
            var serverHandles = GetServerHandles(items);

            var index = 0;

            for (var ii = 0; ii < items.Length; ii++)
            {
                foreach (DateTime value in items[ii])
                {
                    handles[index] = serverHandles[ii];
                    timestamps[index] = value;

                    index++;
                }
            }

            // return the total count.
            return count;
        }

        /// <summary>
        /// Marshals a set of annotation collections into an set of arrays.
        /// </summary>
        private int MarshalAnnotatations(
            TsCHdaAnnotationValueCollection[] items,
            ref int[] serverHandles,
            ref OPCHDA_FILETIME[] ftTimestamps,
            ref OPCHDA_ANNOTATION[] annotations)
        {
            // determine the total length.
            var count = GetCount(items);

            // fetch item server handles.
            var remoteHandles = GetServerHandles(items);

            // allocate input arrays.
            serverHandles = new int[count];
            annotations = new OPCHDA_ANNOTATION[count];

            var timestamps = new DateTime[count];

            // flatten array of collections into a single array.
            var index = 0;

            for (var ii = 0; ii < items.Length; ii++)
            {
                for (var jj = 0; jj < items[ii].Count; jj++)
                {
                    serverHandles[index] = remoteHandles[ii];
                    timestamps[index] = items[ii][jj].Timestamp;

                    annotations[index] = new OPCHDA_ANNOTATION();

                    annotations[index].dwNumValues = 1;
                    annotations[index].ftTimeStamps = Utilities.Interop.GetFILETIMEs(new DateTime[] { timestamps[jj] });
                    annotations[index].szAnnotation = Utilities.Interop.GetUnicodeStrings(new string[] { items[ii][jj].Annotation });
                    annotations[index].ftAnnotationTime = Utilities.Interop.GetFILETIMEs(new DateTime[] { items[ii][jj].CreationTime });
                    annotations[index].szUser = Utilities.Interop.GetUnicodeStrings(new string[] { items[ii][jj].User });

                    index++;
                }
            }

            ftTimestamps = Interop.GetFILETIMEs(timestamps);

            // return the total number of annotations.
            return count;
        }

        /// <summary>
        /// Collapses a set of result collections into a single result code.
        /// </summary>
        private OpcItemResult[] GetIdentifiedResults(TsCHdaResultCollection[] results)
        {
            // handle trival case.
            if (results == null || results.Length == 0)
            {
                return new OpcItemResult[0];
            }

            // fetch the results from each collection.
            var items = new OpcItemResult[results.Length];

            for (var ii = 0; ii < results.Length; ii++)
            {
                items[ii] = new OpcItemResult(results[ii]);

                // check if data actually exists.
                if (results[ii] == null || results[ii].Count == 0)
                {
                    items[ii].Result = OpcResult.Hda.S_NODATA;
                    continue;
                }

                // start with the first result code.
                var resultID = results[ii][0].Result;

                foreach (TsCHdaResult result in results[ii])
                {
                    // all result in the collection should have the same error. 
                    if (resultID.Code != result.Result.Code)
                    {
                        resultID = OpcResult.E_FAIL;
                        break;
                    }
                }
            }

            // all done.
            return items;
        }
        #endregion

        #region Private Members
        private static int nextHandle_ = 1;
        private Hashtable items_ = new Hashtable();
        private DataCallback callback_ = new DataCallback();
        private ConnectionPoint connection_;

        /// <summary>
        /// The synchronization object for subscription access
        /// </summary>
        private static volatile object lock_ = new object();
        
        private bool disposed_ = false;
        #endregion
    }
}
