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
using Technosoftware.DaAeHdaClient.Com.Da;
using Technosoftware.DaAeHdaClient.Com.Utilities;
using Technosoftware.DaAeHdaClient.Da;
using Technosoftware.OpcRcw.Da;
#endregion

namespace Technosoftware.DaAeHdaClient.Com.Da20
{
    /// <summary>
    /// An in-process wrapper for a remote OPC Data Access 2.0X subscription.
    /// </summary>
    internal class Subscription : Technosoftware.DaAeHdaClient.Com.Da.Subscription
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of a subscription.
        /// </summary>
        internal Subscription(object subscription, TsCDaSubscriptionState state, int filters) :
            base(subscription, state, filters)
        {
        }
        #endregion

        #region ISubscription Members
        /// <summary>
        /// Returns the current state of the subscription.
        /// </summary>
        /// <returns>The current state of the subscription.</returns>
        public override TsCDaSubscriptionState GetState()
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

                state.KeepAlive = 0;

                return state;
            }
        }

        /// <summary>
        /// Tells the server to send an data change update for all subscription items containing the cached values. 
        /// </summary>
        public override void Refresh()
        {
            if (subscription_ == null) throw new NotConnectedException();
            lock (lock_)
            {
                var methodName = "IOPCAsyncIO2.Refresh2";
                try
                {
                    var cancelID = 0;
                    var subscription = BeginComCall<IOPCAsyncIO2>(methodName, true);
                    subscription.Refresh2(OPCDATASOURCE.OPC_DS_CACHE, ++_counter, out cancelID);

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
        /// Sets whether data change callbacks are enabled.
        /// </summary>
        public override void SetEnabled(bool enabled)
        {
            if (subscription_ == null) throw new NotConnectedException();
            lock (lock_)
            {
                var methodName = "IOPCAsyncIO2.SetEnable";
                try
                {
                    var subscription = BeginComCall<IOPCAsyncIO2>(methodName, true);
                    subscription.SetEnable((enabled) ? 1 : 0);

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
        /// Gets whether data change callbacks are enabled.
        /// </summary>
        public override bool GetEnabled()
        {
            if (subscription_ == null) throw new NotConnectedException();
            lock (lock_)
            {
                var methodName = "IOPCAsyncIO2.GetEnable";
                try
                {
                    var enabled = 0;
                    var subscription = BeginComCall<IOPCAsyncIO2>(methodName, true);
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
                    throw Utilities.Interop.CreateException(methodName, e);
                }
                finally
                {
                    EndComCall(methodName);
                }
            }
        }
        #endregion

        #region Private and Protected Members
        /// <summary>
        /// Reads a set of items using DA2.0 interfaces.
        /// </summary>
        protected override TsCDaItemValueResult[] Read(OpcItem[] itemIDs, TsCDaItem[] items)
        {
            if (subscription_ == null) throw new NotConnectedException();
            // create result list.
            var results = new TsCDaItemValueResult[itemIDs.Length];

            // separate into cache reads and device reads.
            var cacheReads = new ArrayList();
            var deviceReads = new ArrayList();

            for (var ii = 0; ii < itemIDs.Length; ii++)
            {
                results[ii] = new TsCDaItemValueResult(itemIDs[ii]);

                if (items[ii].MaxAgeSpecified && (items[ii].MaxAge < 0 || items[ii].MaxAge == int.MaxValue))
                {
                    cacheReads.Add(results[ii]);
                }
                else
                {
                    deviceReads.Add(results[ii]);
                }
            }

            // read items from cache.
            if (cacheReads.Count > 0)
            {
                Read((TsCDaItemValueResult[])cacheReads.ToArray(typeof(TsCDaItemValueResult)), true);
            }

            // read items from device.
            if (deviceReads.Count > 0)
            {
                Read((TsCDaItemValueResult[])deviceReads.ToArray(typeof(TsCDaItemValueResult)), false);
            }

            // return results.
            return results;
        }

        /// <summary>
        /// Reads a set of values.
        /// </summary>
        private void Read(TsCDaItemValueResult[] items, bool cache)
        {
            if (items.Length == 0) return;

            // marshal input parameters.
            var serverHandles = new int[items.Length];

            for (var ii = 0; ii < items.Length; ii++)
            {
                serverHandles[ii] = (int)items[ii].ServerHandle;
            }

            // initialize output parameters.
            var pValues = IntPtr.Zero;
            var pErrors = IntPtr.Zero;

            var methodName = "IOPCSyncIO.Read";
            try
            {
                var subscription = BeginComCall<IOPCSyncIO>(methodName, true);
                subscription.Read(
                    (cache) ? OPCDATASOURCE.OPC_DS_CACHE : OPCDATASOURCE.OPC_DS_DEVICE,
                    items.Length,
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
            }

            // unmarshal output parameters.
            var values = Technosoftware.DaAeHdaClient.Com.Da.Interop.GetItemValues(ref pValues, items.Length, true);
            var errors = Utilities.Interop.GetInt32s(ref pErrors, items.Length, true);

            // construct results list.
            for (var ii = 0; ii < items.Length; ii++)
            {
                items[ii].Result = Utilities.Interop.GetResultId(errors[ii]);
                items[ii].DiagnosticInfo = null;

                // convert COM code to unified DA code.
                if (errors[ii] == Result.E_BADRIGHTS) { items[ii].Result = new OpcResult(OpcResult.Da.E_WRITEONLY, Result.E_BADRIGHTS); }

                if (items[ii].Result.Succeeded())
                {
                    items[ii].Value = values[ii].Value;
                    items[ii].Quality = values[ii].Quality;
                    items[ii].QualitySpecified = values[ii].QualitySpecified;
                    items[ii].Timestamp = values[ii].Timestamp;
                    items[ii].TimestampSpecified = values[ii].TimestampSpecified;
                }
            }
        }

        /// <summary>
        /// Writes a set of items using DA2.0 interfaces.
        /// </summary>
        protected override OpcItemResult[] Write(OpcItem[] itemIDs, TsCDaItemValue[] items)
        {
            if (subscription_ == null) throw new NotConnectedException();
            // create result list.
            var results = new OpcItemResult[itemIDs.Length];

            // construct list of valid items to write.
            var writeItems = new ArrayList(itemIDs.Length);
            var writeValues = new ArrayList(itemIDs.Length);

            for (var ii = 0; ii < items.Length; ii++)
            {
                results[ii] = new OpcItemResult(itemIDs[ii]);

                if (items[ii].QualitySpecified || items[ii].TimestampSpecified)
                {
                    results[ii].Result = OpcResult.Da.E_NO_WRITEQT;
                    results[ii].DiagnosticInfo = null;
                    continue;
                }

                writeItems.Add(results[ii]);
                writeValues.Add(items[ii]);
            }

            // check if there is nothing to do.
            if (writeItems.Count == 0)
            {
                return results;
            }

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
                var subscription = BeginComCall<IOPCSyncIO>(methodName, true);
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

            // return results.
            return results;
        }

        /// <summary>
        /// Begins an asynchronous read of a set of items using DA2.0 interfaces.
        /// </summary>
        protected override OpcItemResult[] BeginRead(
            OpcItem[] itemIDs,
            TsCDaItem[] items,
            int requestID,
            out int cancelID)
        {
            var methodName = "IOPCAsyncIO2.Read";
            try
            {
                // marshal input parameters.
                var serverHandles = new int[itemIDs.Length];

                for (var ii = 0; ii < itemIDs.Length; ii++)
                {
                    serverHandles[ii] = (int)itemIDs[ii].ServerHandle;
                }

                // initialize output parameters.
                var pErrors = IntPtr.Zero;

                var subscription = BeginComCall<IOPCAsyncIO2>(methodName, true);
                subscription.Read(
                    itemIDs.Length,
                    serverHandles,
                    requestID,
                    out cancelID,
                    out pErrors);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new Exception($"{methodName} call was cancelled due to response timeout");
                }

                // unmarshal output parameters.
                var errors = Utilities.Interop.GetInt32s(ref pErrors, itemIDs.Length, true);

                // create item results.
                var results = new OpcItemResult[itemIDs.Length];

                for (var ii = 0; ii < itemIDs.Length; ii++)
                {
                    results[ii] = new OpcItemResult(itemIDs[ii]);
                    results[ii].Result = Utilities.Interop.GetResultId(errors[ii]);
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
                throw Utilities.Interop.CreateException(methodName, e);
            }
            finally
            {
                EndComCall(methodName);
            }
        }

        /// <summary>
        /// Begins an asynchronous write for a set of items using DA2.0 interfaces.
        /// </summary>
        protected override OpcItemResult[] BeginWrite(
            OpcItem[] itemIDs,
            TsCDaItemValue[] items,
            int requestID,
            out int cancelID)
        {
            cancelID = 0;

            var validItems = new ArrayList();
            var validValues = new ArrayList();

            // construct initial result list.
            var results = new OpcItemResult[itemIDs.Length];

            for (var ii = 0; ii < itemIDs.Length; ii++)
            {
                results[ii] = new OpcItemResult(itemIDs[ii]);

                results[ii].Result = OpcResult.S_OK;
                results[ii].DiagnosticInfo = null;

                if (items[ii].QualitySpecified || items[ii].TimestampSpecified)
                {
                    results[ii].Result = OpcResult.Da.E_NO_WRITEQT;
                    results[ii].DiagnosticInfo = null;
                    continue;
                }

                validItems.Add(results[ii]);
                validValues.Add(Utilities.Interop.GetVARIANT(items[ii].Value));
            }

            // check if any valid items exist.
            if (validItems.Count == 0)
            {
                return results;
            }

            var methodName = "IOPCAsyncIO2.Write";
            try
            {
                // initialize input parameters.
                var serverHandles = new int[validItems.Count];

                for (var ii = 0; ii < validItems.Count; ii++)
                {
                    serverHandles[ii] = (int)((OpcItemResult)validItems[ii]).ServerHandle;
                }

                // write to sever.
                var pErrors = IntPtr.Zero;

                var subscription = BeginComCall<IOPCAsyncIO2>(methodName, true);
                subscription.Write(
                    validItems.Count,
                    serverHandles,
                    (object[])validValues.ToArray(typeof(object)),
                    requestID,
                    out cancelID,
                    out pErrors);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new Exception($"{methodName} call was cancelled due to response timeout");
                }

                // unmarshal results.
                var errors = Utilities.Interop.GetInt32s(ref pErrors, validItems.Count, true);

                // create result list.
                for (var ii = 0; ii < validItems.Count; ii++)
                {
                    var result = (OpcItemResult)validItems[ii];

                    result.Result = Utilities.Interop.GetResultId(errors[ii]);
                    result.DiagnosticInfo = null;

                    // convert COM code to unified DA code.
                    if (errors[ii] == Result.E_BADRIGHTS) { results[ii].Result = new OpcResult(OpcResult.Da.E_READONLY, Result.E_BADRIGHTS); }
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

            // return results.
            return results;
        }
        #endregion
    }
}
