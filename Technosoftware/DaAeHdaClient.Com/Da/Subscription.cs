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
using Technosoftware.DaAeHdaClient.Com.Utilities;
using Technosoftware.DaAeHdaClient.Da;
using Technosoftware.DaAeHdaClient.Utilities;
using Technosoftware.OpcRcw.Da;
#endregion

namespace Technosoftware.DaAeHdaClient.Com.Da
{
    /// <summary>
    /// A .NET wrapper for a COM server that implements the DA subscription interfaces.
    /// </summary>
    internal class Subscription : ITsCDaSubscription
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of a subscription.
        /// </summary>
        internal Subscription(object subscription, TsCDaSubscriptionState state, int filters)
        {
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (state == null) throw new ArgumentNullException(nameof(state));

            subscription_ = subscription;
            name_ = state.Name;
            _handle = state.ClientHandle;
            _filters = filters;
            callback_ = new Callback(state.ClientHandle, _filters, items_);
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// The finalizer.
        /// </summary>
        ~Subscription()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged resources held by the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed_)
            {
                lock (lock_)
                {
                    if (disposing)
                    {
                        // Free other state (managed objects).

                        if (subscription_ != null)
                        {
                            // close all connections.
                            if (connection_ != null)
                            {
                                try
                                {
                                    connection_.Dispose();
                                }
                                catch
                                {
                                    // Ignore. COM Server probably no longer connected
                                }
                                connection_ = null;
                            }
                        }
                    }

                    // Free your own state (unmanaged objects).
                    // Set large fields to null.

                    if (subscription_ != null)
                    {
                        // release subscription object.
                        try
                        {
                            Technosoftware.DaAeHdaClient.Com.Interop.ReleaseServer(subscription_);
                        }
                        catch
                        {
                            // Ignore. COM Server probably no longer connected
                        }
                        subscription_ = null;
                    }
                }

                disposed_ = true;
            }
        }
        #endregion

        #region Private Members
        /// <summary>
        /// The COM server for the subscription object.
        /// </summary>
        protected object subscription_;

        /// <summary>
        /// A connect point with the COM server.
        /// </summary>
        protected ConnectionPoint connection_;

        /// <summary>
        /// The internal object that implements the IOPCDataCallback interface.
        /// </summary>
        private Callback callback_;

        /// <summary>
        /// The name of the subscription on the server.
        /// </summary>
        protected string name_;

        /// <summary>
        /// A handle assigned by the client for the subscription.
        /// </summary>
        protected object _handle;

        /// <summary>
        /// The default result filters for the subscription.
        /// </summary>
        protected int _filters = (int)TsCDaResultFilter.Minimal;

        /// <summary>
        /// A table of all item identifers which are indexed by internal handle.
        /// </summary>
        private ItemTable items_ = new ItemTable();

        /// <summary>
        /// A counter used to assign unique internal client handles.
        /// </summary>
        protected int _counter;

        /// <summary>
        /// The synchronization object for subscription access
        /// </summary>
        protected object lock_ = new object();

        private int outstandingCalls_;

        private bool disposed_;
        #endregion

        #region ISubscription Members
        /// <summary>
        /// An event to receive data change updates.
        /// </summary>
        public event TsCDaDataChangedEventHandler DataChangedEvent
        {
            add { lock (lock_) { callback_.DataChangedEvent += value; Advise(); } }
            remove { lock (lock_) { callback_.DataChangedEvent -= value; Unadvise(); } }
        }

        //======================================================================
        // Result Filters

        /// <summary>
        /// Returns the filters applied by the server to any item results returned to the client.
        /// </summary>
        /// <returns>A bit mask indicating which fields should be returned in any item results.</returns>
        public int GetResultFilters()
        {
            lock (lock_) { return _filters; }
        }

        /// <summary>
        /// Sets the filters applied by the server to any item results returned to the client.
        /// </summary>
        /// <param name="filters">A bit mask indicating which fields should be returned in any item results.</param>
        public void SetResultFilters(int filters)
        {
            lock (lock_)
            {
                _filters = filters;

                // update the callback object.
                callback_.SetFilters(_handle, _filters);
            }
        }

        //======================================================================
        // State Management

        /// <summary>
        /// Returns the current state of the subscription.
        /// </summary>
        /// <returns>The current state of the subscription.</returns>
        public virtual TsCDaSubscriptionState GetState()
        {
            if (subscription_ == null) throw new NotConnectedException();
            lock (lock_)
            {
                var methodName = "IOPCGroupStateMgt.GetState";
                var state = new TsCDaSubscriptionState { ClientHandle = _handle };

                string name = null;

                try
                {
                    var active = 0;
                    var updateRate = 0;
                    float deadband = 0;
                    var timebias = 0;
                    var localeID = 0;
                    var clientHandle = 0;
                    var serverHandle = 0;
              
                    var subscription = BeginComCall<IOPCGroupStateMgt>(methodName, true);
                    subscription.GetState(
                        out updateRate,
                        out active,
                        out name,
                        out timebias,
                        out deadband,
                        out localeID,
                        out clientHandle,
                        out serverHandle);

                    if (DCOMCallWatchdog.IsCancelled)
                    {
                        throw new Exception($"{methodName} call was cancelled due to response timeout");
                    }

                    state.Name = name;
                    state.ServerHandle = serverHandle;
                    state.Active = active != 0;
                    state.UpdateRate = updateRate;
                    state.TimeBias = timebias;
                    state.Deadband = deadband;
                    state.Locale = Technosoftware.DaAeHdaClient.Com.Interop.GetLocale(localeID);

                    // cache the name separately.
                    name_ = state.Name;
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

                if (name != null)
                {
                    methodName = "IOPCGroupStateMgt2.GetKeepAlive";
                    try
                    {                
                        var keepAlive = 0;
                        var subscription = BeginComCall<IOPCGroupStateMgt2>(methodName, true);
                        subscription.GetKeepAlive(out keepAlive);

                        if (DCOMCallWatchdog.IsCancelled)
                        {
                            throw new Exception($"{methodName} call was cancelled due to response timeout");
                        }

                        state.KeepAlive = keepAlive;
                    }
                    catch (Exception e)
                    {
                        ComCallError(methodName, e);
                        state.KeepAlive = 0;
                    }
                    finally
                    {
                        EndComCall(methodName);
                    }
                }

                return state;
            }
        }

        /// <summary>
        /// Changes the state of a subscription.
        /// </summary>
        /// <param name="masks">A bit mask that indicates which elements of the subscription state are changing.</param>
        /// <param name="state">The new subscription state.</param>
        /// <returns>The actual subscption state after applying the changes.</returns>
        public TsCDaSubscriptionState ModifyState(int masks, TsCDaSubscriptionState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (subscription_ == null) throw new NotConnectedException();

            lock (lock_)
            {               
                var methodName = "IOPCGroupStateMgt.SetName";
                // update the subscription name.
                if ((masks & (int)TsCDaStateMask.Name) != 0 && state.Name != name_)
                {
                    try
                    {
                        var subscription = BeginComCall<IOPCGroupStateMgt>(methodName, true);
                        subscription.SetName(state.Name);

                        if (DCOMCallWatchdog.IsCancelled)
                        {
                            throw new Exception($"{methodName} call was cancelled due to response timeout");
                        }

                        name_ = state.Name;
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
                }

                // update the client handle.
                if ((masks & (int)TsCDaStateMask.ClientHandle) != 0)
                {
                    _handle = state.ClientHandle;

                    // update the callback object.
                    callback_.SetFilters(_handle, _filters);
                }

                // update the subscription state.
                var active = (state.Active) ? 1 : 0;
                var localeID = ((masks & (int)TsCDaStateMask.Locale) != 0) ? Technosoftware.DaAeHdaClient.Com.Interop.GetLocale(state.Locale) : 0;

                var hActive = GCHandle.Alloc(active, GCHandleType.Pinned);
                var hLocale = GCHandle.Alloc(localeID, GCHandleType.Pinned);
                var hUpdateRate = GCHandle.Alloc(state.UpdateRate, GCHandleType.Pinned);
                var hDeadband = GCHandle.Alloc(state.Deadband, GCHandleType.Pinned);

                var updateRate = 0;

                methodName = "IOPCGroupStateMgt.SetState";
                try
                {                
                    var subscription = BeginComCall<IOPCGroupStateMgt>(methodName, true);
                    subscription.SetState(
                        ((masks & (int)TsCDaStateMask.UpdateRate) != 0) ? hUpdateRate.AddrOfPinnedObject() : IntPtr.Zero,
                        out updateRate,
                        ((masks & (int)TsCDaStateMask.Active) != 0) ? hActive.AddrOfPinnedObject() : IntPtr.Zero,
                        IntPtr.Zero,
                        ((masks & (int)TsCDaStateMask.Deadband) != 0) ? hDeadband.AddrOfPinnedObject() : IntPtr.Zero,
                        ((masks & (int)TsCDaStateMask.Locale) != 0) ? hLocale.AddrOfPinnedObject() : IntPtr.Zero,
                        IntPtr.Zero);

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
                    if (hActive.IsAllocated) hActive.Free();
                    if (hLocale.IsAllocated) hLocale.Free();
                    if (hUpdateRate.IsAllocated) hUpdateRate.Free();
                    if (hDeadband.IsAllocated) hDeadband.Free();
                    EndComCall(methodName);
                }

                // set keep alive, if supported.
                if ((masks & (int)TsCDaStateMask.KeepAlive) != 0)
                {
                    var keepAlive = 0;

                    methodName = "IOPCGroupStateMgt2.SetKeepAlive";
                    try
                    {           
                        var subscription = BeginComCall<IOPCGroupStateMgt2>(methodName, true);
                        subscription.SetKeepAlive(state.KeepAlive, out keepAlive);

                        if (DCOMCallWatchdog.IsCancelled)
                        {
                            throw new Exception($"{methodName} call was cancelled due to response timeout");
                        }
                    }
                    catch (Exception e)
                    {
                        ComCallError(methodName, e);
                        state.KeepAlive = 0;
                    }
                    finally
                    {
                        EndComCall(methodName);
                    }
                }

                // return the current state.
                return GetState();
            }
        }

        /// <summary>
        /// Adds items to the subscription.
        /// </summary>
        /// <param name="items">The set of items to add to the subscription.</param>
        /// <returns>The results of the add item operation for each item.</returns>
        public TsCDaItemResult[] AddItems(TsCDaItem[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (subscription_ == null) throw new NotConnectedException();

            // check if nothing to do.
            if (items.Length == 0)
            {
                return new TsCDaItemResult[0];
            }

            lock (lock_)
            {
                // marshal input parameters.
                var count = items.Length;

                var definitions = Interop.GetOPCITEMDEFs(items);
                TsCDaItemResult[] results = null;

                lock (items_)
                {
                    for (var ii = 0; ii < count; ii++)
                    {
                        definitions[ii].hClient = ++_counter;
                    }

                    // initialize output parameters.
                    var pResults = IntPtr.Zero;
                    var pErrors = IntPtr.Zero;

                    var methodName = "IOPCItemMgt.AddItems";
                    try
                    {          
                        var subscription = BeginComCall<IOPCItemMgt>(methodName, true);
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
                        throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException(methodName, e);
                    }
                    finally
                    {
                        EndComCall(methodName);
                    }

                    // unmarshal output parameters.
                    var serverHandles = Interop.GetItemResults(ref pResults, count, true);
                    var errors = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref pErrors, count, true);

                    // construct result list.
                    results = new TsCDaItemResult[count];

                    for (var ii = 0; ii < count; ii++)
                    {
                        // create a new Result.
                        results[ii] = new TsCDaItemResult(items[ii]);

                        // save server handles.
                        results[ii].ServerHandle = serverHandles[ii];
                        results[ii].ClientHandle = definitions[ii].hClient;

                        // items created active by default.
                        if (!results[ii].ActiveSpecified)
                        {
                            results[ii].Active = true;
                            results[ii].ActiveSpecified = true;
                        }

                        // update result id.
                        results[ii].Result = Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(errors[ii]);
                        results[ii].DiagnosticInfo = null;

                        // add new item table.
                        if (results[ii].Result.Succeeded())
                        {
                            // save client handle.
                            results[ii].ClientHandle = items[ii].ClientHandle;

                            items_[definitions[ii].hClient] = new OpcItem(results[ii]);

                            // restore internal handle.
                            results[ii].ClientHandle = definitions[ii].hClient;
                        }
                    }
                }

                // set non-critical item parameters - these methods all update the item result objects. 
                UpdateDeadbands(results);
                UpdateSamplingRates(results);
                SetEnableBuffering(results);

                lock (items_)
                {
                    var filteredResults = (TsCDaItemResult[])items_.ApplyFilters(_filters, results);

                    // need to return the client handle for failed items.
                    if ((_filters & (int)TsCDaResultFilter.ClientHandle) != 0)
                    {
                        for (var ii = 0; ii < count; ii++)
                        {
                            if (filteredResults[ii].Result.Failed())
                            {
                                filteredResults[ii].ClientHandle = items[ii].ClientHandle;
                            }
                        }
                    }

                    return filteredResults;
                }
            }
        }

        /// <summary>
        /// Modifies the state of items in the subscription
        /// </summary>
        /// <param name="masks">Specifies which item state parameters are being modified.</param>
        /// <param name="items">The new state for each item.</param>
        /// <returns>The results of the modify item operation for each item.</returns>
        public TsCDaItemResult[] ModifyItems(int masks, TsCDaItem[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (subscription_ == null) throw new NotConnectedException();

            // check if nothing to do.
            if (items.Length == 0)
            {
                return new TsCDaItemResult[0];
            }

            lock (lock_)
            {
                // initialize result list.
                TsCDaItemResult[] results = null;

                lock (items_)
                {
                    results = items_.CreateItems(items);
                }

                if ((masks & (int)TsCDaStateMask.ReqType) != 0) SetReqTypes(results);
                if ((masks & (int)TsCDaStateMask.Active) != 0) UpdateActive(results);
                if ((masks & (int)TsCDaStateMask.Deadband) != 0) UpdateDeadbands(results);
                if ((masks & (int)TsCDaStateMask.SamplingRate) != 0) UpdateSamplingRates(results);
                if ((masks & (int)TsCDaStateMask.EnableBuffering) != 0) SetEnableBuffering(results);

                // return results.
                lock (items_)
                {
                    return (TsCDaItemResult[])items_.ApplyFilters(_filters, results);
                }
            }
        }

        /// <summary>
        /// Removes items from the subscription.
        /// </summary>
        /// <param name="items">The identifiers (i.e. server handles) for the items being removed.</param>
        /// <returns>The results of the remove item operation for each item.</returns>
        public OpcItemResult[] RemoveItems(OpcItem[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (subscription_ == null) throw new NotConnectedException();

            // check if nothing to do.
            if (items.Length == 0)
            {
                return new OpcItemResult[0];
            }

            lock (lock_)
            {
                // get item ids.
                OpcItem[] itemIDs = null;

                lock (items_)
                {
                    itemIDs = items_.GetItemIDs(items);
                }

                // fetch server handles.
                var serverHandles = new int[itemIDs.Length];

                for (var ii = 0; ii < itemIDs.Length; ii++)
                {
                    serverHandles[ii] = (int)itemIDs[ii].ServerHandle;
                }

                // initialize output parameters.
                var pErrors = IntPtr.Zero;

                var methodName = "IOPCItemMgt.RemoveItems";
                try
                {         
                    var subscription = BeginComCall<IOPCItemMgt>(methodName, true);
                    subscription.RemoveItems(itemIDs.Length, serverHandles, out pErrors);

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

                // unmarshal output parameters.
                var errors = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref pErrors, itemIDs.Length, true);

                // process results.
                var results = new OpcItemResult[itemIDs.Length];

                var itemsToRemove = new ArrayList(itemIDs.Length);

                for (var ii = 0; ii < itemIDs.Length; ii++)
                {
                    results[ii] = new OpcItemResult(itemIDs[ii]);

                    results[ii].Result = Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(errors[ii]);
                    results[ii].DiagnosticInfo = null;

                    // flag item for removal from local list.
                    if (results[ii].Result.Succeeded())
                    {
                        itemsToRemove.Add(results[ii].ClientHandle);
                    }
                }

                // apply filter to results.
                lock (items_)
                {
                    results = (OpcItemResult[])items_.ApplyFilters(_filters, results);

                    // remove item from local list.
                    foreach (int clientHandle in itemsToRemove)
                    {
                        items_[clientHandle] = null;
                    }

                    return results;
                }
            }
        }

        /// <summary>
        /// Reads the values for a set of items in the subscription.
        /// </summary>
        /// <param name="items">The identifiers (i.e. server handles) for the items being read.</param>
        /// <returns>The value for each of items.</returns>
        public TsCDaItemValueResult[] Read(TsCDaItem[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (subscription_ == null) throw new NotConnectedException();

            // check if nothing to do.
            if (items.Length == 0)
            {
                return new TsCDaItemValueResult[0];
            }

            lock (lock_)
            {
                // get item ids.
                OpcItem[] itemIDs = null;

                lock (items_)
                {
                    itemIDs = items_.GetItemIDs(items);
                }

                // read from the server.
                var results = Read(itemIDs, items);

                // return results.
                lock (items_)
                {
                    return (TsCDaItemValueResult[])items_.ApplyFilters(_filters, results);
                }
            }
        }

        /// <summary>
        /// Writes the value, quality and timestamp for a set of items in the subscription.
        /// </summary>
        /// <param name="items">The item values to write.</param>
        /// <returns>The results of the write operation for each item.</returns>
        public OpcItemResult[] Write(TsCDaItemValue[] items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (subscription_ == null) throw new NotConnectedException();

            // check if nothing to do.
            if (items.Length == 0)
            {
                return new OpcItemResult[0];
            }

            lock (lock_)
            {
                // get item ids.
                OpcItem[] itemIDs = null;

                lock (items_)
                {
                    itemIDs = items_.GetItemIDs(items);
                }

                // write to the server.
                var results = Write(itemIDs, items);

                // return results.
                lock (items_)
                {
                    return (OpcItemResult[])items_.ApplyFilters(_filters, results);
                }
            }
        }

        //======================================================================
        // Asynchronous I/O

        /// <summary>
        /// Begins an asynchronous read operation for a set of items.
        /// </summary>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] Read(
            TsCDaItem[] items,
            object requestHandle,
            TsCDaReadCompleteEventHandler callback,
            out IOpcRequest request)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            if (subscription_ == null) throw new NotConnectedException();

            request = null;

            // check if nothing to do.
            if (items.Length == 0)
            {
                return new OpcItemResult[0];
            }

            lock (lock_)
            {
                // ensure a callback connection is established with the server.
                if (connection_ == null)
                {
                    Advise();
                }

                // get item ids.
                OpcItem[] itemIDs = null;

                lock (items_)
                {
                    itemIDs = items_.GetItemIDs(items);
                }

                // create request object.
                var internalRequest = new Request(
                    this,
                    requestHandle,
                    _filters,
                    _counter++,
                    callback);

                // register request with callback object.
                callback_.BeginRequest(internalRequest);
                request = internalRequest;

                // begin read request.
                OpcItemResult[] results = null;
                var cancelID = 0;

                try
                {
                    results = BeginRead(itemIDs, items, internalRequest.RequestID, out cancelID);
                }
                catch (Exception)
                {
                    callback_.EndRequest(internalRequest);
                    throw;
                }

                // apply request options.
                lock (items_)
                {
                    items_.ApplyFilters(_filters | (int)TsCDaResultFilter.ClientHandle, results);
                }

                lock (internalRequest)
                {
                    // check if all results have already arrived - this invokes the callback if this is the case.
                    if (internalRequest.BeginRead(cancelID, results))
                    {
                        callback_.EndRequest(internalRequest);
                        request = null;
                    }
                }

                // return initial results.
                return results;
            }
        }

        /// <summary>
        /// Begins an asynchronous write operation for a set of items.
        /// </summary>
        /// <param name="items">The set of item values to write (must include the server handle).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] Write(
            TsCDaItemValue[] items,
            object requestHandle,
            TsCDaWriteCompleteEventHandler callback,
            out IOpcRequest request)
        {

            if (items == null) throw new ArgumentNullException(nameof(items));
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            if (subscription_ == null) throw new NotConnectedException();

            request = null;

            // check if nothing to do.
            if (items.Length == 0)
            {
                return new OpcItemResult[0];
            }

            lock (lock_)
            {
                // ensure a callback connection is established with the server.
                if (connection_ == null)
                {
                    Advise();
                }

                // get item ids.
                OpcItem[] itemIDs = null;

                lock (items_)
                {
                    itemIDs = items_.GetItemIDs(items);
                }

                // create request object.
                var internalRequest = new Request(
                    this,
                    requestHandle,
                    _filters,
                    _counter++,
                    callback);

                // register request with callback object.
                callback_.BeginRequest(internalRequest);
                request = internalRequest;

                // begin write request.
                OpcItemResult[] results = null;
                var cancelID = 0;

                try
                {
                    results = BeginWrite(itemIDs, items, internalRequest.RequestID, out cancelID);
                }
                catch (Exception)
                {
                    callback_.EndRequest(internalRequest);
                    throw;
                }

                // apply request options.
                lock (items_)
                {
                    items_.ApplyFilters(_filters | (int)TsCDaResultFilter.ClientHandle, results);
                }

                lock (internalRequest)
                {
                    // check if all results have already arrived - this invokes the callback if this is the case.
                    if (internalRequest.BeginWrite(cancelID, results))
                    {
                        callback_.EndRequest(internalRequest);
                        request = null;
                    }
                }

                // return initial results.
                return results;
            }
        }

        /// <summary>
        /// Cancels an asynchronous read or write operation.
        /// </summary>
        /// <param name="request">The object returned from the BeginRead or BeginWrite request.</param>
        /// <param name="callback">The function to invoke when the cancel completes.</param>
        public void Cancel(IOpcRequest request, TsCDaCancelCompleteEventHandler callback)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            lock (lock_)
            {
                lock (request)
                {
                    // check if request can still be cancelled.
                    if (!callback_.CancelRequest((Request)request))
                    {
                        return;
                    }

                    // update the callback.
                    ((Request)request).Callback = callback;

                    // send a cancel request to the server.
                    var methodName = "IOPCAsyncIO2.Cancel2";
                    try
                    {
                        var subscription = BeginComCall<IOPCAsyncIO2>(methodName, true);
                        subscription.Cancel2(((Request)request).CancelID);

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
                }
            }
        }

        /// <summary>
        /// Causes the server to send a data changed notification for all active items. 
        /// </summary>
        public virtual void Refresh()
        {
            if (subscription_ == null) throw new NotConnectedException();
            lock (lock_)
            {
                var methodName = "IOPCAsyncIO3.RefreshMaxAge";
                try
                {
                    var cancelID = 0;
                    var subscription = BeginComCall<IOPCAsyncIO3>(methodName, true);
                    subscription.RefreshMaxAge(int.MaxValue, ++_counter, out cancelID);

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
            }
        }

        /// <summary>
        /// Causes the server to send a data changed notification for all active items. 
        /// </summary>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public virtual void Refresh(
            object requestHandle,
            out IOpcRequest request)
        {
            if (subscription_ == null) throw new NotConnectedException();
            lock (lock_)
            {
                // ensure a callback connection is established with the server.
                if (connection_ == null)
                {
                    Advise();
                }

                // create request object.
                var internalRequest = new Request(
                    this,
                    requestHandle,
                    _filters,
                    _counter++,
                    null);

                var cancelID = 0;

                var methodName = "IOPCAsyncIO3.RefreshMaxAge";
                try
                {
                    var subscription = BeginComCall<IOPCAsyncIO3>(methodName, true);
                    subscription.RefreshMaxAge(0, (int)internalRequest.RequestID, out cancelID);

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

                request = internalRequest;

                // save the cancel id.
                lock (request)
                {
                    internalRequest.BeginRefresh(cancelID);
                }
            }
        }

        /// <summary>
        /// Enables or disables data change notifications from the server.
        /// </summary>
        /// <param name="enabled">Whether data change notifications are enabled.</param>
        public virtual void SetEnabled(bool enabled)
        {
            if (subscription_ == null) throw new NotConnectedException();
            lock (lock_)
            {
                var methodName = "IOPCAsyncIO3.SetEnable";
                try
                {
                    var subscription = BeginComCall<IOPCAsyncIO3>(methodName, true);
                    subscription.SetEnable((enabled) ? 1 : 0);

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
            }
        }

        /// <summary>
        /// Checks whether data change notifications from the server are enabled.
        /// </summary>
        /// <returns>Whether data change notifications are enabled.</returns>
        public virtual bool GetEnabled()
        {
            if (subscription_ == null) throw new NotConnectedException();
            lock (lock_)
            {
                var methodName = "IOPCAsyncIO3.GetEnable";
                try
                {
                    var enabled = 0;
                    var subscription = BeginComCall<IOPCAsyncIO3>(methodName, true);
                    subscription.GetEnable(out enabled);

                    if (DCOMCallWatchdog.IsCancelled)
                    {
                        throw new Exception($"{methodName} call was cancelled due to response timeout");
                    }

                    return enabled != 0;
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
            }
        }
        #endregion

        #region COM Call Tracing
        /// <summary>
        /// Must be called before any COM call.
        /// </summary>
        /// <typeparam name="T">The interface to used when making the call.</typeparam>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="isRequiredInterface">if set to <c>true</c> interface is an required interface and and exception is thrown on error.</param>
        /// <returns></returns>
        protected T BeginComCall<T>(string methodName, bool isRequiredInterface) where T : class
        {
            Utils.Trace(Utils.TraceMasks.ExternalSystem, "{0} called.", methodName);

            lock (lock_)
            {
                outstandingCalls_++;

                if (subscription_ == null)
                {
                    if (isRequiredInterface)
                    {
                        throw new NotConnectedException();
                    }
                }

                var comObject = subscription_ as T;

                if (comObject == null)
                {
                    if (isRequiredInterface)
                    {
                        throw new NotSupportedException(Utils.Format("OPC Interface '{0}' is a required interface but not supported by the server.", typeof(T).Name));
                    }
                    else
                    {
                        Utils.Trace(Utils.TraceMasks.ExternalSystem, "OPC Interface '{0}' is not supported by server but it is an optional one.", typeof(T).Name);
                    }
                }

                DCOMCallWatchdog.Set();

                return comObject;
            }
        }

        /// <summary>
        /// Must called if a COM call returns an unexpected exception.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="e">The exception.</param>
        /// <remarks>Note that some COM calls are expected to return errors.</remarks>
        protected void ComCallError(string methodName, Exception e)
        {
            SafeNativeMethods.TraceComError(e, methodName);
        }

        /// <summary>
        /// Must be called in the finally block after making a COM call.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        protected void EndComCall(string methodName)
        {
            Utils.Trace(Utils.TraceMasks.ExternalSystem, "{0} completed.", methodName);

            lock (lock_)
            {
                outstandingCalls_--;

                DCOMCallWatchdog.Reset();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Reads a set of items using DA3.0 interfaces.
        /// </summary>
        protected virtual TsCDaItemValueResult[] Read(OpcItem[] itemIDs, TsCDaItem[] items)
        {
            var methodName = "IOPCSyncIO2.ReadMaxAge";
            try
            {
                // marshal input parameters.
                var serverHandles = new int[itemIDs.Length];
                var maxAges = new int[itemIDs.Length];

                for (var ii = 0; ii < itemIDs.Length; ii++)
                {
                    serverHandles[ii] = (int)itemIDs[ii].ServerHandle;
                    maxAges[ii] = (items[ii].MaxAgeSpecified) ? items[ii].MaxAge : 0;
                }

                // initialize output parameters.
                var pValues = IntPtr.Zero;
                var pQualities = IntPtr.Zero;
                var pTimestamps = IntPtr.Zero;
                var pErrors = IntPtr.Zero;

                var subscription = BeginComCall<IOPCSyncIO2>(methodName, true);
                subscription.ReadMaxAge(
                    itemIDs.Length,
                    serverHandles,
                    maxAges,
                    out pValues,
                    out pQualities,
                    out pTimestamps,
                    out pErrors);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new Exception($"{methodName} call was cancelled due to response timeout");
                }

                // unmarshal output parameters.
                var values = Com.Interop.GetVARIANTs(ref pValues, itemIDs.Length, true);
                var qualities = Technosoftware.DaAeHdaClient.Com.Interop.GetInt16s(ref pQualities, itemIDs.Length, true);
                var timestamps = Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIMEs(ref pTimestamps, itemIDs.Length, true);
                var errors = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref pErrors, itemIDs.Length, true);

                // create item results.
                var results = new TsCDaItemValueResult[itemIDs.Length];

                for (var ii = 0; ii < itemIDs.Length; ii++)
                {
                    results[ii] = new TsCDaItemValueResult(itemIDs[ii]);

                    results[ii].Value = values[ii];
                    results[ii].Quality = new TsCDaQuality(qualities[ii]);
                    results[ii].QualitySpecified = values[ii] != null;
                    results[ii].Timestamp = timestamps[ii];
                    results[ii].TimestampSpecified = values[ii] != null;
                    results[ii].Result = Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(errors[ii]);
                    results[ii].DiagnosticInfo = null;

                    // convert COM code to unified DA code.
                    if (errors[ii] == Result.E_BADRIGHTS) { results[ii].Result = new OpcResult(OpcResult.Da.E_WRITEONLY, Result.E_BADRIGHTS); }
                }

                // return results.
                return results;
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
        }

        /// <summary>
        /// Writes a set of items using DA3.0 interfaces.
        /// </summary>
        protected virtual OpcItemResult[] Write(OpcItem[] itemIDs, TsCDaItemValue[] items)
        {
            var methodName = "IOPCSyncIO2.WriteVQT";
            try
            {
                // initialize input parameters.
                var serverHandles = new int[itemIDs.Length];

                for (var ii = 0; ii < itemIDs.Length; ii++)
                {
                    serverHandles[ii] = (int)itemIDs[ii].ServerHandle;
                }

                var values = Interop.GetOPCITEMVQTs(items);

                // write to sever.
                var pErrors = IntPtr.Zero;

                var subscription = BeginComCall<IOPCSyncIO2>(methodName, true);
                subscription.WriteVQT(
                    itemIDs.Length,
                    serverHandles,
                    values,
                    out pErrors);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new Exception($"{methodName} call was cancelled due to response timeout");
                }

                // unmarshal results.
                var errors = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref pErrors, itemIDs.Length, true);

                // create result list.
                var results = new OpcItemResult[itemIDs.Length];

                for (var ii = 0; ii < itemIDs.Length; ii++)
                {
                    results[ii] = new OpcItemResult(itemIDs[ii]);

                    results[ii].Result = Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(errors[ii]);
                    results[ii].DiagnosticInfo = null;

                    // convert COM code to unified DA code.
                    if (errors[ii] == Result.E_BADRIGHTS) { results[ii].Result = new OpcResult(OpcResult.Da.E_READONLY, Result.E_BADRIGHTS); }
                }

                // return results.
                return results;
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
        }

        /// <summary>
        /// Begins an asynchronous read of a set of items using DA3.0 interfaces.
        /// </summary>
        protected virtual OpcItemResult[] BeginRead(
            OpcItem[] itemIDs,
            TsCDaItem[] items,
            int requestID,
            out int cancelID)
        {
            var methodName = "IOPCAsyncIO3.ReadMaxAge";
            try
            {
                // marshal input parameters.
                var serverHandles = new int[itemIDs.Length];
                var maxAges = new int[itemIDs.Length];

                for (var ii = 0; ii < itemIDs.Length; ii++)
                {
                    serverHandles[ii] = (int)itemIDs[ii].ServerHandle;
                    maxAges[ii] = (items[ii].MaxAgeSpecified) ? items[ii].MaxAge : 0;
                }

                // initialize output parameters.
                var pErrors = IntPtr.Zero;

                var subscription = BeginComCall<IOPCAsyncIO3>(methodName, true);
                subscription.ReadMaxAge(
                    itemIDs.Length,
                    serverHandles,
                    maxAges,
                    requestID,
                    out cancelID,
                    out pErrors);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new Exception($"{methodName} call was cancelled due to response timeout");
                }

                // unmarshal output parameters.
                var errors = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref pErrors, itemIDs.Length, true);

                // create item results.
                var results = new OpcItemResult[itemIDs.Length];

                for (var ii = 0; ii < itemIDs.Length; ii++)
                {
                    results[ii] = new OpcItemResult(itemIDs[ii]);

                    results[ii].Result = Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(errors[ii]);
                    results[ii].DiagnosticInfo = null;

                    // convert COM code to unified DA code.
                    if (errors[ii] == Result.E_BADRIGHTS) { results[ii].Result = new OpcResult(OpcResult.Da.E_WRITEONLY, Result.E_BADRIGHTS); }
                }

                // return results.
                return results;
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
        }

        /// <summary>
        /// Begins an asynchronous write for a set of items using DA3.0 interfaces.
        /// </summary>
        protected virtual OpcItemResult[] BeginWrite(
            OpcItem[] itemIDs,
            TsCDaItemValue[] items,
            int requestID,
            out int cancelID)
        {
            var methodName = "IOPCAsyncIO3.WriteVQT";
            try
            {
                // initialize input parameters.
                var serverHandles = new int[itemIDs.Length];

                for (var ii = 0; ii < itemIDs.Length; ii++)
                {
                    serverHandles[ii] = (int)itemIDs[ii].ServerHandle;
                }

                var values = Interop.GetOPCITEMVQTs(items);

                // write to sever.
                var pErrors = IntPtr.Zero;

                var subscription = BeginComCall<IOPCAsyncIO3>(methodName, true);
                subscription.WriteVQT(
                    itemIDs.Length,
                    serverHandles,
                    values,
                    requestID,
                    out cancelID,
                    out pErrors);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new Exception($"{methodName} call was cancelled due to response timeout");
                }

                // unmarshal results.
                var errors = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref pErrors, itemIDs.Length, true);

                // create result list.
                var results = new OpcItemResult[itemIDs.Length];

                for (var ii = 0; ii < itemIDs.Length; ii++)
                {
                    results[ii] = new OpcItemResult(itemIDs[ii]);

                    results[ii].Result = Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(errors[ii]);
                    results[ii].DiagnosticInfo = null;

                    // convert COM code to unified DA code.
                    if (errors[ii] == Result.E_BADRIGHTS) { results[ii].Result = new OpcResult(OpcResult.Da.E_READONLY, Result.E_BADRIGHTS); }
                }

                // return results.
                return results;
            }
            catch (Exception e)
            {
                ComCallError(methodName, e);
                throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException("IOPCAsyncIO3.WriteVQT()", e);
            }
            finally
            {
                EndComCall(methodName);
            }
        }

        /// <summary>
        /// Sets the requested data type for the specified items.
        /// </summary>
        private void SetReqTypes(TsCDaItemResult[] items)
        {
            // check if there is nothing to do.
            if (items == null || items.Length == 0) return;

            // clients must explicitly set the ReqType to typeof(object) in order to set it to VT_EMPTY.
            var changedItems = new ArrayList();

            foreach (var item in items)
            {
                if (item.Result.Succeeded())
                {
                    if (item.ReqType != null) changedItems.Add(item);
                }
            }

            // check if there is nothing to do.
            if (changedItems.Count == 0) return;

            // invoke method.
            var methodName = "IOPCItemMgt.SetDatatypes";
            try
            {
                // initialize input parameters.
                var handles = new int[changedItems.Count];
                var datatypes = new short[changedItems.Count];

                for (var ii = 0; ii < changedItems.Count; ii++)
                {
                    var item = (TsCDaItemResult)changedItems[ii];
                    handles[ii] = Convert.ToInt32(item.ServerHandle);
                    datatypes[ii] = (short)Technosoftware.DaAeHdaClient.Com.Interop.GetType(item.ReqType);
                }

                // initialize output parameters.
                var pErrors = IntPtr.Zero;

                var subscription = BeginComCall<IOPCItemMgt>(methodName, true);
                subscription.SetDatatypes(
                    changedItems.Count,
                    handles,
                    datatypes,
                    out pErrors);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new Exception($"{methodName} call was cancelled due to response timeout");
                }

                // check for individual item errors.
                var errors = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref pErrors, handles.Length, true);

                for (var ii = 0; ii < errors.Length; ii++)
                {
                    if (Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(errors[ii]).Failed())
                    {
                        var item = (TsCDaItemResult)changedItems[ii];
                        item.Result = OpcResult.Da.E_BADTYPE;
                        item.DiagnosticInfo = null;
                    }
                }
            }

            // treat any general failure to mean the item is deactivated.
            catch (Exception e)
            {
                for (var ii = 0; ii < changedItems.Count; ii++)
                {
                    var item = (TsCDaItemResult)changedItems[ii];
                    item.Result = OpcResult.Da.E_BADTYPE;
                    item.DiagnosticInfo = null;
                }
                ComCallError(methodName, e);
            }
            finally
            {
                EndComCall(methodName);
            }
        }

        /// <summary>
        /// Sets the active state for the specified items.
        /// </summary>
        private void SetActive(TsCDaItemResult[] items, bool active)
        {
            // check if there is nothing to do.
            if (items == null || items.Length == 0) return;

            // invoke method.
            var methodName = "IOPCItemMgt.SetActiveState";
            try
            {
                // initialize input parameters.
                var handles = new int[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    handles[ii] = Convert.ToInt32(items[ii].ServerHandle);
                }

                // initialize output parameters.
                var pErrors = IntPtr.Zero;

                var subscription = BeginComCall<IOPCItemMgt>(methodName, true);
                subscription.SetActiveState(
                    items.Length,
                    handles,
                    (active) ? 1 : 0,
                    out pErrors);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new Exception($"{methodName} call was cancelled due to response timeout");
                }

                // check for individual item errors.
                var errors = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref pErrors, handles.Length, true);

                for (var ii = 0; ii < errors.Length; ii++)
                {
                    if (Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(errors[ii]).Failed())
                    {
                        items[ii].Active = false;
                        items[ii].ActiveSpecified = true;
                    }
                }
            }

            // treat any general failure to mean the item is deactivated.
            catch (Exception e)
            {
                for (var ii = 0; ii < items.Length; ii++)
                {
                    items[ii].Active = false;
                    items[ii].ActiveSpecified = true;
                    ComCallError(methodName, e);
                }
            }
            finally
            {
                EndComCall(methodName);
            }
        }

        /// <summary>
        /// Update the active state for the specified items.
        /// </summary>
        private void UpdateActive(TsCDaItemResult[] items)
        {
            if (items == null || items.Length == 0) return;

            // seperate items in two groups depending on whether the deadband is being set or cleared.
            var activatedItems = new ArrayList();
            var deactivatedItems = new ArrayList();

            foreach (var item in items)
            {
                if (item.Result.Succeeded() && item.ActiveSpecified)
                {
                    if (item.Active)
                    {
                        activatedItems.Add(item);
                    }
                    else
                    {
                        deactivatedItems.Add(item);
                    }
                }
            }

            // activate items.
            SetActive((TsCDaItemResult[])activatedItems.ToArray(typeof(TsCDaItemResult)), true);

            // de-activate items.
            SetActive((TsCDaItemResult[])deactivatedItems.ToArray(typeof(TsCDaItemResult)), false);
        }

        /// <summary>
        /// Sets the deadbands for the specified items.
        /// </summary>
        private void SetDeadbands(TsCDaItemResult[] items)
        {
            // check if there is nothing to do.
            if (items == null || items.Length == 0) return;

            // invoke method.
            var methodName = "IOPCItemDeadbandMgt.SetItemDeadband";
            try
            {
                // initialize input parameters.
                var handles = new int[items.Length];
                var deadbands = new float[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    handles[ii] = Convert.ToInt32(items[ii].ServerHandle);
                    deadbands[ii] = items[ii].Deadband;
                }

                // initialize output parameters.
                var pErrors = IntPtr.Zero;

                var subscription = BeginComCall<IOPCItemDeadbandMgt>(methodName, true);
                subscription.SetItemDeadband(
                    handles.Length,
                    handles,
                    deadbands,
                    out pErrors);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new Exception($"{methodName} call was cancelled due to response timeout");
                }

                // check for individual item errors.
                var errors = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref pErrors, handles.Length, true);

                for (var ii = 0; ii < errors.Length; ii++)
                {
                    if (Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(errors[ii]).Failed())
                    {
                        items[ii].Deadband = 0;
                        items[ii].DeadbandSpecified = false;
                    }
                }
            }

            // treat any general failure as an indication that deadband is not supported.
            catch (Exception e)
            {
                for (var ii = 0; ii < items.Length; ii++)
                {
                    items[ii].Deadband = 0;
                    items[ii].DeadbandSpecified = false;
                }
                ComCallError(methodName, e);
            }
            finally
            {
                EndComCall(methodName);
            }
        }

        /// <summary>
        /// Clears the deadbands for the specified items.
        /// </summary>
        private void ClearDeadbands(TsCDaItemResult[] items)
        {
            // check if there is nothing to do.
            if (items == null || items.Length == 0) return;

            // invoke method.
            var methodName = "IOPCItemDeadbandMgt.ClearItemDeadband";
            try
            {
                // initialize input parameters.
                var handles = new int[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    handles[ii] = Convert.ToInt32(items[ii].ServerHandle);
                }

                // initialize output parameters.
                var pErrors = IntPtr.Zero;

                var subscription = BeginComCall<IOPCItemDeadbandMgt>(methodName, true);
                subscription.ClearItemDeadband(
                    handles.Length,
                    handles,
                    out pErrors);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new Exception($"{methodName} call was cancelled due to response timeout");
                }

                // check for individual item errors.
                var errors = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref pErrors, handles.Length, true);

                for (var ii = 0; ii < errors.Length; ii++)
                {
                    if (Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(errors[ii]).Failed())
                    {
                        items[ii].Deadband = 0;
                        items[ii].DeadbandSpecified = false;
                    }
                }
            }

            // treat any general failure as an indication that deadband is not supported.
            catch (Exception e)
            {
                for (var ii = 0; ii < items.Length; ii++)
                {
                    items[ii].Deadband = 0;
                    items[ii].DeadbandSpecified = false;
                }
                ComCallError(methodName, e);
            }
            finally
            {
                EndComCall(methodName);
            }
        }

        /// <summary>
        /// Update the deadbands for the specified items.
        /// </summary>
        private void UpdateDeadbands(TsCDaItemResult[] items)
        {
            if (items == null || items.Length == 0) return;

            // seperate items in two groups depending on whether the deadband is being set or cleared.
            var changedItems = new ArrayList();
            var clearedItems = new ArrayList();

            foreach (var item in items)
            {
                if (item.Result.Succeeded())
                {
                    if (item.DeadbandSpecified)
                    {
                        changedItems.Add(item);
                    }
                    else
                    {
                        clearedItems.Add(item);
                    }
                }
            }

            // set deadbands.
            SetDeadbands((TsCDaItemResult[])changedItems.ToArray(typeof(TsCDaItemResult)));

            // clear deadbands.
            ClearDeadbands((TsCDaItemResult[])clearedItems.ToArray(typeof(TsCDaItemResult)));
        }

        /// <summary>
        /// Sets the sampling rates for the specified items.
        /// </summary>
        private void SetSamplingRates(TsCDaItemResult[] items)
        {
            // check if there is nothing to do.
            if (items == null || items.Length == 0) return;

            // invoke method.
            var methodName = "IOPCItemSamplingMgt.SetItemSamplingRate";
            try
            {
                // initialize input parameters.
                var handles = new int[items.Length];
                var samplingRate = new int[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    handles[ii] = Convert.ToInt32(items[ii].ServerHandle);
                    samplingRate[ii] = items[ii].SamplingRate;
                }

                // initialize output parameters.
                var pResults = IntPtr.Zero;
                var pErrors = IntPtr.Zero;

                var subscription = BeginComCall<IOPCItemSamplingMgt>(methodName, false);
                if (subscription != null)
                {
                    subscription.SetItemSamplingRate(
                    handles.Length,
                    handles,
                    samplingRate,
                    out pResults,
                    out pErrors);

                    if (DCOMCallWatchdog.IsCancelled)
                    {
                        throw new Exception($"{methodName} call was cancelled due to response timeout");
                    }

                    // check for individual item errors.
                    var results = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref pResults, handles.Length, true);
                    var errors = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref pErrors, handles.Length, true);

                    for (var ii = 0; ii < errors.Length; ii++)
                    {
                        if (items[ii].SamplingRate != results[ii])
                        {
                            items[ii].SamplingRate = results[ii];
                            items[ii].SamplingRateSpecified = true;
                            continue;
                        }

                        if (Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(errors[ii]).Failed())
                        {
                            items[ii].SamplingRate = 0;
                            items[ii].SamplingRateSpecified = false;
                            continue;
                        }
                    }
                }
            }

            // treat any general failure as an indication that sampling rate is not supported.
            catch (Exception e)
            {
                for (var ii = 0; ii < items.Length; ii++)
                {
                    items[ii].SamplingRate = 0;
                    items[ii].SamplingRateSpecified = false;
                }
                ComCallError(methodName, e);
            }
            finally
            {
                EndComCall(methodName);
            }
        }

        /// <summary>
        /// Clears the sampling rates for the specified items.
        /// </summary>
        private void ClearSamplingRates(TsCDaItemResult[] items)
        {
            // check if there is nothing to do.
            if (items == null || items.Length == 0) return;

            // invoke method.
            var methodName = "IOPCItemSamplingMgt.ClearItemSamplingRate";
            try
            {
                // initialize input parameters.
                var handles = new int[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    handles[ii] = Convert.ToInt32(items[ii].ServerHandle);
                }

                // initialize output parameters.
                var pErrors = IntPtr.Zero;

                var subscription = BeginComCall<IOPCItemSamplingMgt>(methodName, false);
                if (subscription != null)
                {
                    subscription.ClearItemSamplingRate(
                    handles.Length,
                    handles,
                    out pErrors);

                    if (DCOMCallWatchdog.IsCancelled)
                    {
                        throw new Exception($"{methodName} call was cancelled due to response timeout");
                    }

                    // check for individual item errors.
                    var errors = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref pErrors, handles.Length, true);

                    for (var ii = 0; ii < errors.Length; ii++)
                    {
                        if (Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(errors[ii]).Failed())
                        {
                            items[ii].SamplingRate = 0;
                            items[ii].SamplingRateSpecified = false;
                        }
                    }
                }
            }

            // treat any general failure as an indication that sampling rate is not supported.
            catch (Exception e)
            {
                for (var ii = 0; ii < items.Length; ii++)
                {
                    items[ii].SamplingRate = 0;
                    items[ii].SamplingRateSpecified = false;
                }
                ComCallError(methodName, e);
            }
            finally
            {
                EndComCall(methodName);
            }
        }

        /// <summary>
        /// Update the sampling rates for the specified items.
        /// </summary>
        private void UpdateSamplingRates(TsCDaItemResult[] items)
        {
            if (items == null || items.Length == 0) return;

            // seperate items in two groups depending on whether the sampling rate is being set or cleared.
            var changedItems = new ArrayList();
            var clearedItems = new ArrayList();

            foreach (var item in items)
            {
                if (item.Result.Succeeded())
                {
                    if (item.SamplingRateSpecified)
                    {
                        changedItems.Add(item);
                    }
                    else
                    {
                        clearedItems.Add(item);
                    }
                }
            }

            // set sampling rates.
            SetSamplingRates((TsCDaItemResult[])changedItems.ToArray(typeof(TsCDaItemResult)));

            // clear sampling rates.
            ClearSamplingRates((TsCDaItemResult[])clearedItems.ToArray(typeof(TsCDaItemResult)));
        }

        /// <summary>
        /// Sets the enable buffering flags.
        /// </summary>
        private void SetEnableBuffering(TsCDaItemResult[] items)
        {
            // check if there is nothing to do.
            if (items == null || items.Length == 0) return;

            var changedItems = new ArrayList();

            foreach (var item in items)
            {
                if (item.Result.Succeeded())
                {
                    changedItems.Add(item);
                }
            }

            // check if there is nothing to do.
            if (changedItems.Count == 0) return;

            // invoke method.
            var methodName = "IOPCItemSamplingMgt.SetItemBufferEnable";
            try
            {
                // initialize input parameters.
                var handles = new int[changedItems.Count];
                var enabled = new int[changedItems.Count];

                for (var ii = 0; ii < changedItems.Count; ii++)
                {
                    var item = (TsCDaItemResult)changedItems[ii];
                    handles[ii] = Convert.ToInt32(item.ServerHandle);
                    enabled[ii] = (item.EnableBufferingSpecified && item.EnableBuffering) ? 1 : 0;
                }

                // initialize output parameters.
                var pErrors = IntPtr.Zero;

                var subscription = BeginComCall<IOPCItemSamplingMgt>(methodName, false);
                if (subscription != null)
                {
                    subscription.SetItemBufferEnable(
                    handles.Length,
                    handles,
                    enabled,
                    out pErrors);

                    if (DCOMCallWatchdog.IsCancelled)
                    {
                        throw new Exception($"{methodName} call was cancelled due to response timeout");
                    }

                    // check for individual item errors.
                    var errors = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref pErrors, handles.Length, true);

                    for (var ii = 0; ii < errors.Length; ii++)
                    {
                        var item = (TsCDaItemResult)changedItems[ii];

                        if (Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(errors[ii]).Failed())
                        {
                            item.EnableBuffering = false;
                            item.EnableBufferingSpecified = true;
                        }
                    }
                }
            }

            // treat any general failure as an indication that enable buffering is not supported.
            catch (Exception e)
            {
                foreach (TsCDaItemResult item in changedItems)
                {
                    item.EnableBuffering = false;
                    item.EnableBufferingSpecified = true;
                }
                ComCallError(methodName, e);
            }
            finally
            {
                EndComCall(methodName);
            }
        }

        /// <summary>
        /// Establishes a connection point callback with the COM server.
        /// </summary>
        private void Advise()
        {
            if (connection_ == null)
            {
                connection_ = new ConnectionPoint(subscription_, typeof(IOPCDataCallback).GUID);
                connection_.Advise(callback_);
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
        #endregion

        #region ItemTable Class
        /// <summary>
        /// A table of item identifiers indexed by internal handle.
        /// </summary>
        private class ItemTable
        {
            /// <summary>
            /// Looks up an item identifier for the specified internal handle.
            /// </summary>
            public OpcItem this[object handle]
            {
                get
                {
                    if (handle != null)
                    {
                        return (OpcItem)items_[handle];
                    }

                    return null;
                }

                set
                {
                    if (handle != null)
                    {
                        if (value == null)
                        {
                            items_.Remove(handle);
                            return;
                        }

                        items_[handle] = value;
                    }
                }
            }

            /// <summary>
            /// Returns a server handle that must be treated as invalid by the server,
            /// </summary>
            /// <returns></returns>
            private int GetInvalidHandle()
            {
                var invalidHandle = 0;

                foreach (OpcItem item in items_.Values)
                {
                    if (item.ServerHandle != null && item.ServerHandle.GetType() == typeof(int))
                    {
                        if (invalidHandle < (int)item.ServerHandle)
                        {
                            invalidHandle = (int)item.ServerHandle + 1;
                        }
                    }
                }

                return invalidHandle;
            }

            /// <summary>
            /// Copies a set of items an substitutes the client and server handles for use by the server.
            /// </summary>
            public OpcItem[] GetItemIDs(OpcItem[] items)
            {
                // create an invalid server handle.
                var invalidHandle = GetInvalidHandle();

                // copy the items.
                var itemIDs = new OpcItem[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    // lookup server handle.
                    var itemID = this[items[ii].ServerHandle];

                    // copy the item id.
                    if (itemID != null)
                    {
                        itemIDs[ii] = (OpcItem)itemID.Clone();
                    }

                    else
                    {
                        itemIDs[ii] = new OpcItem();
                        itemIDs[ii].ServerHandle = invalidHandle;
                    }

                    // store the internal handle as the client handle.
                    itemIDs[ii].ClientHandle = items[ii].ServerHandle;
                }

                // return the item copies.
                return itemIDs;
            }

            /// <summary>
            /// Creates a item result list from a set of items and sets the handles for use by the server.
            /// </summary>
            public TsCDaItemResult[] CreateItems(TsCDaItem[] items)
            {
                if (items == null) { return null; }

                var results = new TsCDaItemResult[items.Length];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    // initialize result with the item
                    results[ii] = new TsCDaItemResult((TsCDaItem)items[ii]);

                    // lookup the cached identifier.
                    var itemID = this[items[ii].ServerHandle];

                    if (itemID != null)
                    {
                        results[ii].ItemName = itemID.ItemName;
                        results[ii].ItemPath = itemID.ItemName;
                        results[ii].ServerHandle = itemID.ServerHandle;

                        // update the client handle.
                        itemID.ClientHandle = items[ii].ClientHandle;
                    }

                    // check if handle not found.
                    if (results[ii].ServerHandle == null)
                    {
                        results[ii].Result = OpcResult.Da.E_INVALIDHANDLE;
                        results[ii].DiagnosticInfo = null;
                        continue;
                    }

                    // replace client handle with internal handle.
                    results[ii].ClientHandle = items[ii].ServerHandle;
                }

                return results;
            }

            /// <summary>
            /// Updates a result list based on the request options and sets the handles for use by the client.
            /// </summary>
            public OpcItem[] ApplyFilters(int filters, OpcItem[] results)
            {
                if (results == null) { return null; }

                foreach (var result in results)
                {
                    var itemID = this[result.ClientHandle];

                    if (itemID != null)
                    {
                        result.ItemName = ((filters & (int)TsCDaResultFilter.ItemName) != 0) ? itemID.ItemName : null;
                        result.ItemPath = ((filters & (int)TsCDaResultFilter.ItemPath) != 0) ? itemID.ItemPath : null;
                        result.ServerHandle = result.ClientHandle;
                        result.ClientHandle = ((filters & (int)TsCDaResultFilter.ClientHandle) != 0) ? itemID.ClientHandle : null;
                    }

                    if ((filters & (int)TsCDaResultFilter.ItemTime) == 0)
                    {
                        if (result.GetType() == typeof(TsCDaItemValueResult))
                        {
                            ((TsCDaItemValueResult)result).Timestamp = DateTime.MinValue;
                            ((TsCDaItemValueResult)result).TimestampSpecified = false;
                        }
                    }
                }

                return results;
            }

            /// <summary>
            /// The table of known item identifiers.
            /// </summary>
            private Hashtable items_ = new Hashtable();
        }
        #endregion

        #region IOPCDataCallback Members
        /// <summary>
        /// A class that implements the IOPCDataCallback interface.
        /// </summary>
        private class Callback : IOPCDataCallback
        {
            /// <summary>
            /// Initializes the object with the containing subscription object.
            /// </summary>
            public Callback(object handle, int filters, ItemTable items)
            {
                handle_ = handle;
                filters_ = filters;
                items_ = items;
            }

            /// <summary>
            /// Updates the result filters and subscription handle.
            /// </summary>
            public void SetFilters(object handle, int filters)
            {
                lock (lock_)
                {
                    handle_ = handle;
                    filters_ = filters;
                }
            }

            /// <summary>
            /// Adds an asynchrounous request.
            /// </summary>
            public void BeginRequest(Request request)
            {
                lock (lock_)
                {
                    requests_[request.RequestID] = request;
                }
            }

            /// <summary>
            /// Returns true is an asynchrounous request can be cancelled.
            /// </summary>
            public bool CancelRequest(Request request)
            {
                lock (lock_)
                {
                    return requests_.ContainsKey(request.RequestID);
                }
            }

            /// <summary>
            /// Remvoes an asynchrounous request.
            /// </summary>
            public void EndRequest(Request request)
            {
                lock (lock_)
                {
                    requests_.Remove(request.RequestID);
                }
            }

            /// <summary>
            /// The handle to return with any callbacks. 
            /// </summary>
            private object handle_;

            /// <summary>
            /// The current request options for the subscription.
            /// </summary>
            private int filters_ = (int)TsCDaResultFilter.Minimal;

            /// <summary>
            /// A table of item identifiers indexed by internal handle.
            /// </summary>
            private ItemTable items_;

            /// <summary>
            /// A table of autstanding asynchronous requests.
            /// </summary>
            private Hashtable requests_ = new Hashtable();
            private object lock_ = new object();

            /// <summary>
            /// Raised when data changed callbacks arrive.
            /// </summary>
            public event TsCDaDataChangedEventHandler DataChangedEvent
            {
                add { lock (lock_) { _dataChangedEvent += value; } }
                remove { lock (lock_) { _dataChangedEvent -= value; } }
            }
            /// <remarks/>
            private event TsCDaDataChangedEventHandler _dataChangedEvent = null;

            /// <summary>
            /// Called when a data changed event is received.
            /// </summary>
            public void OnDataChange(
                int dwTransid,
                int hGroup,
                int hrMasterquality,
                int hrMastererror,
                int dwCount,
                int[] phClientItems,
                object[] pvValues,
                short[] pwQualities,
                OpcRcw.Da.FILETIME[] pftTimeStamps,
                int[] pErrors)
            {
                LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess, true);
                try
                {
                    Request request = null;

                    lock (lock_)
                    {
                        // check for an outstanding request.
                        if (dwTransid != 0)
                        {
                            request = (Request)requests_[dwTransid];


                            if (request != null)
                            {
                                // remove the request.
                                requests_.Remove(dwTransid);
                            }
                        }

                        // do nothing if no connections.
                        if (_dataChangedEvent == null) return;

                        // unmarshal item values.
                        var values = UnmarshalValues(
                            dwCount,
                            phClientItems,
                            pvValues,
                            pwQualities,
                            pftTimeStamps,
                            pErrors);

                        // apply request options.
                        lock (items_)
                        {
                            items_.ApplyFilters(filters_ | (int)TsCDaResultFilter.ClientHandle, values);
                        }

                        if (_dataChangedEvent != null)
                        {
                            if (!LicenseHandler.IsExpired)
                            {
                                // invoke the callback.
                                _dataChangedEvent(handle_, (request != null) ? request.Handle : null, values);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    var stack = e.StackTrace;
                }
            }

            // sends read complete notifications.
            public void OnReadComplete(
                int dwTransid,
                int hGroup,
                int hrMasterquality,
                int hrMastererror,
                int dwCount,
                int[] phClientItems,
                object[] pvValues,
                short[] pwQualities,
                OpcRcw.Da.FILETIME[] pftTimeStamps,
                int[] pErrors)
            {
                try
                {
                    Request request = null;
                    TsCDaItemValueResult[] values = null;

                    lock (lock_)
                    {
                        // do nothing if no outstanding requests.
                        request = (Request)requests_[dwTransid];

                        if (request == null)
                        {
                            return;
                        }

                        // remove the request.
                        requests_.Remove(dwTransid);

                        // unmarshal item values.
                        values = UnmarshalValues(
                            dwCount,
                            phClientItems,
                            pvValues,
                            pwQualities,
                            pftTimeStamps,
                            pErrors);

                        // apply request options.
                        lock (items_)
                        {
                            items_.ApplyFilters(filters_ | (int)TsCDaResultFilter.ClientHandle, values);
                        }
                    }

                    // end the request.
                    lock (request)
                    {
                        request.EndRequest(values);
                    }
                }
                catch (Exception e)
                {
                    var stack = e.StackTrace;
                }
            }

            // handles asynchronous write complete events.
            public void OnWriteComplete(
                int dwTransid,
                int hGroup,
                int hrMastererror,
                int dwCount,
                int[] phClientItems,
                int[] pErrors)
            {
                try
                {
                    Request request = null;
                    OpcItemResult[] results = null;

                    lock (lock_)
                    {
                        // do nothing if no outstanding requests.
                        request = (Request)requests_[dwTransid];

                        if (request == null)
                        {
                            return;
                        }

                        // remove the request.
                        requests_.Remove(dwTransid);

                        // contruct the item results.
                        results = new OpcItemResult[dwCount];

                        for (var ii = 0; ii < results.Length; ii++)
                        {
                            // lookup the external client handle.
                            var itemID = (OpcItem)items_[phClientItems[ii]];

                            results[ii] = new OpcItemResult(itemID);
                            results[ii].ClientHandle = phClientItems[ii];
                            results[ii].Result = Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(pErrors[ii]);
                            results[ii].DiagnosticInfo = null;

                            // convert COM code to unified DA code.
                            if (pErrors[ii] == Result.E_BADRIGHTS) { results[ii].Result = new OpcResult(OpcResult.Da.E_READONLY, Result.E_BADRIGHTS); }
                        }

                        // apply request options.
                        lock (items_)
                        {
                            items_.ApplyFilters(filters_ | (int)TsCDaResultFilter.ClientHandle, results);
                        }
                    }

                    // end the request.
                    lock (request)
                    {
                        request.EndRequest(results);
                    }
                }
                catch (Exception e)
                {
                    var stack = e.StackTrace;
                }
            }

            // handles asynchronous request cancel events.
            public void OnCancelComplete(
                int dwTransid,
                int hGroup)
            {
                try
                {
                    Request request = null;

                    lock (lock_)
                    {
                        // do nothing if no outstanding requests.
                        request = (Request)requests_[dwTransid];

                        if (request == null)
                        {
                            return;
                        }

                        // remove the request.
                        requests_.Remove(dwTransid);
                    }

                    // end the request.
                    lock (request)
                    {
                        request.EndRequest();
                    }
                }
                catch (Exception e)
                {
                    var stack = e.StackTrace;
                }
            }

            /// <summary>
            /// Creates an array of item value result objects from the callback data.
            /// </summary>
            private TsCDaItemValueResult[] UnmarshalValues(
                int dwCount,
                int[] phClientItems,
                object[] pvValues,
                short[] pwQualities,
                OpcRcw.Da.FILETIME[] pftTimeStamps,
                int[] pErrors)
            {
                // contruct the item value results.
                var values = new TsCDaItemValueResult[dwCount];

                for (var ii = 0; ii < values.Length; ii++)
                {
                    // lookup the external client handle.
                    var itemID = (OpcItem)items_[phClientItems[ii]];

                    values[ii] = new TsCDaItemValueResult(itemID);
                    values[ii].ClientHandle = phClientItems[ii];
                    values[ii].Value = pvValues[ii];
                    values[ii].Quality = new TsCDaQuality(pwQualities[ii]);
                    values[ii].QualitySpecified = true;
                    values[ii].Timestamp = Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIME(Interop.Convert(pftTimeStamps[ii]));
                    values[ii].TimestampSpecified = values[ii].Timestamp != DateTime.MinValue;
                    values[ii].Result = Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(pErrors[ii]);
                    values[ii].DiagnosticInfo = null;

                    // convert COM code to unified DA code.
                    if (pErrors[ii] == Result.E_BADRIGHTS) { values[ii].Result = new OpcResult(OpcResult.Da.E_WRITEONLY, Result.E_BADRIGHTS); }
                }

                // return results
                return values;
            }
        }
        #endregion
    }

    #region Request Class
    /// <summary>
    /// Contains the state of an asynchronous request to a COM server.
    /// </summary>
    [Serializable]
    internal class Request : TsCDaRequest
    {
        /// <summary>
        /// The unique id assigned by the subscription.
        /// </summary>
        internal int RequestID = 0;

        /// <summary>
        /// The unique id assigned by the server.
        /// </summary>
        internal int CancelID;

        /// <summary>
        /// The callback used when the request completes.
        /// </summary>
        internal Delegate Callback;

        /// <summary>
        /// The result filters to use for the request.
        /// </summary>
        internal int Filters;

        /// <summary>
        /// The set of initial results.
        /// </summary>
        internal OpcItem[] InitialResults;

        /// <summary>
        /// Initializes the object with a subscription and a unique id.
        /// </summary>
        public Request(
            ITsCDaSubscription subscription,
            object clientHandle,
            int filters,
            int requestID,
            Delegate callback)
            :
            base(subscription, clientHandle)
        {
            Filters = filters;
            RequestID = requestID;
            Callback = callback;
            CancelID = 0;
            InitialResults = null;
        }

        /// <summary>
        /// Begins a read request by storing the initial results.
        /// </summary>
        public bool BeginRead(int cancelID, OpcItemResult[] results)
        {
            CancelID = cancelID;

            // check if results have already arrived.
            if (InitialResults != null)
            {
                if (InitialResults.GetType() == typeof(TsCDaItemValueResult[]))
                {
                    var values = (TsCDaItemValueResult[])InitialResults;
                    InitialResults = results;
                    EndRequest(values);
                    return true;
                }
            }

            // check that at least one valid item existed.
            foreach (var result in results)
            {
                if (result.Result.Succeeded())
                {
                    InitialResults = results;
                    return false;
                }
            }

            // request complete - all items had errors.
            return true;
        }

        /// <summary>
        /// Begins a write request by storing the initial results.
        /// </summary>
        public bool BeginWrite(int cancelID, OpcItemResult[] results)
        {
            CancelID = cancelID;

            // check if results have already arrived.
            if (InitialResults != null)
            {
                if (InitialResults.GetType() == typeof(OpcItemResult[]))
                {
                    var callbackResults = (OpcItemResult[])InitialResults;
                    InitialResults = results;
                    EndRequest(callbackResults);
                    return true;
                }
            }

            // check that at least one valid item existed.
            foreach (var result in results)
            {
                if (result.Result.Succeeded())
                {
                    InitialResults = results;
                    return false;
                }
            }

            // apply filters.       
            for (var ii = 0; ii < results.Length; ii++)
            {
                if ((Filters & (int)TsCDaResultFilter.ItemName) == 0) results[ii].ItemName = null;
                if ((Filters & (int)TsCDaResultFilter.ItemPath) == 0) results[ii].ItemPath = null;
                if ((Filters & (int)TsCDaResultFilter.ClientHandle) == 0) results[ii].ClientHandle = null;
            }


            // invoke callback.
            ((TsCDaWriteCompleteEventHandler)Callback)(Handle, results);

            return true;
        }

        /// <summary>
        /// Begins a refersh request by saving the cancel id.
        /// </summary>
        public bool BeginRefresh(int cancelID)
        {
            // save cancel id.
            CancelID = cancelID;

            // request not complete.
            return false;
        }

        /// <summary>
        /// Completes a read request by processing the values and invoking the callback.
        /// </summary>
        public void EndRequest()
        {
            // check for cancelled request.
            if (typeof(TsCDaCancelCompleteEventHandler).IsInstanceOfType(Callback))
            {
                ((TsCDaCancelCompleteEventHandler)Callback)(Handle);
                return;
            }
        }

        /// <summary>
        /// Completes a read request by processing the values and invoking the callback.
        /// </summary>
        public void EndRequest(TsCDaItemValueResult[] results)
        {
            // check if the begin request has not completed yet.
            if (InitialResults == null)
            {
                InitialResults = results;
                return;
            }

            // check for cancelled request.
            if (typeof(TsCDaCancelCompleteEventHandler).IsInstanceOfType(Callback))
            {
                ((TsCDaCancelCompleteEventHandler)Callback)(Handle);
                return;
            }

            // apply filters.
            for (var ii = 0; ii < results.Length; ii++)
            {
                if ((Filters & (int)TsCDaResultFilter.ItemName) == 0) results[ii].ItemName = null;
                if ((Filters & (int)TsCDaResultFilter.ItemPath) == 0) results[ii].ItemPath = null;
                if ((Filters & (int)TsCDaResultFilter.ClientHandle) == 0) results[ii].ClientHandle = null;

                if ((Filters & (int)TsCDaResultFilter.ItemTime) == 0)
                {
                    results[ii].Timestamp = DateTime.MinValue;
                    results[ii].TimestampSpecified = false;
                }
            }

            // invoke callback.
            if (typeof(TsCDaReadCompleteEventHandler).IsInstanceOfType(Callback))
            {
                ((TsCDaReadCompleteEventHandler)Callback)(Handle, results);
            }
        }

        /// <summary>
        /// Completes a write request by processing the values and invoking the callback.
        /// </summary>
        public void EndRequest(OpcItemResult[] callbackResults)
        {
            // check if the begin request has not completed yet.
            if (InitialResults == null)
            {
                InitialResults = callbackResults;
                return;
            }

            // check for cancelled request.
            if (Callback != null && Callback.GetType() == typeof(TsCDaCancelCompleteEventHandler))
            {
                ((TsCDaCancelCompleteEventHandler)Callback)(Handle);
                return;
            }

            // update initial results with callback results.
            var results = (OpcItemResult[])InitialResults;

            // insert matching value by checking client handle.
            var index = 0;

            for (var ii = 0; ii < results.Length; ii++)
            {
                while (index < callbackResults.Length)
                {
                    // the initial results have the internal handle stores as the server handle.
                    if (callbackResults[ii].ServerHandle.Equals(results[index].ServerHandle))
                    {
                        results[index++] = callbackResults[ii];
                        break;
                    }

                    index++;
                }
            }

            // apply filters.
            for (var ii = 0; ii < results.Length; ii++)
            {
                if ((Filters & (int)TsCDaResultFilter.ItemName) == 0) results[ii].ItemName = null;
                if ((Filters & (int)TsCDaResultFilter.ItemPath) == 0) results[ii].ItemPath = null;
                if ((Filters & (int)TsCDaResultFilter.ClientHandle) == 0) results[ii].ClientHandle = null;
            }

            // invoke callback.
            if (Callback != null && Callback.GetType() == typeof(TsCDaWriteCompleteEventHandler))
            {
                ((TsCDaWriteCompleteEventHandler)Callback)(Handle, results);
            }
        }
    }
    #endregion
}
