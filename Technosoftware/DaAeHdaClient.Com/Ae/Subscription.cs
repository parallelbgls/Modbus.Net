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
using System.Runtime.InteropServices;
using Technosoftware.DaAeHdaClient.Ae;
using Technosoftware.DaAeHdaClient.Utilities;
using Technosoftware.OpcRcw.Ae;

#endregion

namespace Technosoftware.DaAeHdaClient.Com.Ae
{
    /// <summary>
    /// A .NET wrapper for a COM server that implements the AE subscription interfaces.
    /// </summary>
    [Serializable]
    internal class Subscription : ITsCAeSubscription
    {
        #region Constructors
        /// <summary>
        /// Initializes the object with the specified URL and COM server.
        /// </summary>
        internal Subscription(TsCAeSubscriptionState state, object subscription)
        {
            subscription_ = subscription;
            clientHandle_ = OpcConvert.Clone(state.ClientHandle);
            supportsAe11_ = true;
            callback_ = new Callback(state.ClientHandle);

            // check if the V1.1 interfaces are supported.
            try
            {
                var server = (IOPCEventSubscriptionMgt2)subscription_;
            }
            catch
            {
                supportsAe11_ = false;
            }
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
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
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

        #region Technosoftware.DaAeHdaClient.ISubscription Members
        /// <summary>
        /// An event to receive data change updates.
        /// </summary>
        public event TsCAeDataChangedEventHandler DataChangedEvent
        {
            add { lock (this) { Advise(); callback_.DataChangedEvent += value; } }
            remove { lock (this) { callback_.DataChangedEvent -= value; Unadvise(); } }
        }

        //======================================================================
        // State Management

        /// <summary>
        /// Returns the current state of the subscription.
        /// </summary>
        /// <returns>The current state of the subscription.</returns>
        public TsCAeSubscriptionState GetState()
        {
            lock (this)
            {
                // verify state and arguments.
                if (subscription_ == null) throw new NotConnectedException();

                // initialize arguments.
                int pbActive;
                int pdwBufferTime;
                int pdwMaxSize;
                var pdwKeepAliveTime = 0;

                // invoke COM method.
                try
                {
                    ((IOPCEventSubscriptionMgt)subscription_).GetState(
                        out pbActive,
                        out pdwBufferTime,
                        out pdwMaxSize,
                        out _);
                }
                catch (Exception e)
                {
                    throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException("IOPCEventSubscriptionMgt.GetState", e);
                }

                // get keep alive.
                if (supportsAe11_)
                {
                    try
                    {
                        ((IOPCEventSubscriptionMgt2)subscription_).GetKeepAlive(out pdwKeepAliveTime);
                    }
                    catch (Exception e)
                    {
                        throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException("IOPCEventSubscriptionMgt2.GetKeepAlive", e);
                    }
                }

                // build results 
                var state = new TsCAeSubscriptionState
                {
                    Active = pbActive != 0,
                    ClientHandle = clientHandle_,
                    BufferTime = pdwBufferTime,
                    MaxSize = pdwMaxSize,
                    KeepAlive = pdwKeepAliveTime
                };

                // return results.
                return state;
            }
        }

        /// <summary>
        /// Changes the state of a subscription.
        /// </summary>
        /// <param name="masks">A bit mask that indicates which elements of the subscription state are changing.</param>
        /// <param name="state">The new subscription state.</param>
        /// <returns>The actual subscription state after applying the changes.</returns>
        public TsCAeSubscriptionState ModifyState(int masks, TsCAeSubscriptionState state)
        {
            lock (this)
            {
                // verify state and arguments.
                if (subscription_ == null) throw new NotConnectedException();

                // initialize arguments.
                var active = (state.Active) ? 1 : 0;

                var hActive = GCHandle.Alloc(active, GCHandleType.Pinned);
                var hBufferTime = GCHandle.Alloc(state.BufferTime, GCHandleType.Pinned);
                var hMaxSize = GCHandle.Alloc(state.MaxSize, GCHandleType.Pinned);

                var pbActive = ((masks & (int)TsCAeStateMask.Active) != 0) ? hActive.AddrOfPinnedObject() : IntPtr.Zero;
                var pdwBufferTime = ((masks & (int)TsCAeStateMask.BufferTime) != 0) ? hBufferTime.AddrOfPinnedObject() : IntPtr.Zero;
                var pdwMaxSize = ((masks & (int)TsCAeStateMask.MaxSize) != 0) ? hMaxSize.AddrOfPinnedObject() : IntPtr.Zero;

                var phClientSubscription = 0;

                // invoke COM method.
                try
                {
                    ((IOPCEventSubscriptionMgt)subscription_).SetState(
                        pbActive,
                        pdwBufferTime,
                        pdwMaxSize,
                        phClientSubscription,
                        out _,
                        out _);
                }
                catch (Exception e)
                {
                    throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException("IOPCEventSubscriptionMgt.SetState", e);
                }
                finally
                {
                    if (hActive.IsAllocated) hActive.Free();
                    if (hBufferTime.IsAllocated) hBufferTime.Free();
                    if (hMaxSize.IsAllocated) hMaxSize.Free();
                }

                // update keep alive.
                if (((masks & (int)TsCAeStateMask.KeepAlive) != 0) && supportsAe11_)
                {
                    try
                    {
                        ((IOPCEventSubscriptionMgt2)subscription_).SetKeepAlive(
                            state.KeepAlive,
                            out _);
                    }
                    catch (Exception e)
                    {
                        throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException("IOPCEventSubscriptionMgt2.SetKeepAlive", e);
                    }
                }

                // return current state.
                return GetState();
            }
        }

        //======================================================================
        // Filter Management

        /// <summary>
        /// Returns the current filters for the subscription.
        /// </summary>
        /// <returns>The current filters for the subscription.</returns>
        public TsCAeSubscriptionFilters GetFilters()
        {
            lock (this)
            {
                // verify state and arguments.
                if (subscription_ == null) throw new NotConnectedException();

                // initialize arguments.
                int pdwEventType;
                int pdwNumCategories;
                IntPtr ppidEventCategories;
                int pdwLowSeverity;
                int pdwHighSeverity;
                int pdwNumAreas;
                IntPtr ppsAreaList;
                int pdwNumSources;
                IntPtr ppsSourceList;

                // invoke COM method.
                try
                {
                    ((IOPCEventSubscriptionMgt)subscription_).GetFilter(
                        out pdwEventType,
                        out pdwNumCategories,
                        out ppidEventCategories,
                        out pdwLowSeverity,
                        out pdwHighSeverity,
                        out pdwNumAreas,
                        out ppsAreaList,
                        out pdwNumSources,
                        out ppsSourceList);
                }
                catch (Exception e)
                {
                    throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException("IOPCEventSubscriptionMgt.GetFilter", e);
                }

                // marshal results 
                var categoryIDs = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref ppidEventCategories, pdwNumCategories, true);
                var areaIDs = Technosoftware.DaAeHdaClient.Com.Interop.GetUnicodeStrings(ref ppsAreaList, pdwNumAreas, true);
                var sourceIDs = Technosoftware.DaAeHdaClient.Com.Interop.GetUnicodeStrings(ref ppsSourceList, pdwNumSources, true);

                // build results.
                var filters = new TsCAeSubscriptionFilters
                {
                    EventTypes = pdwEventType,
                    LowSeverity = pdwLowSeverity,
                    HighSeverity = pdwHighSeverity
                };


                filters.Categories.AddRange(categoryIDs);
                filters.Areas.AddRange(areaIDs);
                filters.Sources.AddRange(sourceIDs);

                // return results.
                return filters;
            }
        }

        /// <summary>
        /// Sets the current filters for the subscription.
        /// </summary>
        /// <param name="filters">The new filters to use for the subscription.</param>
        public void SetFilters(TsCAeSubscriptionFilters filters)
        {
            lock (this)
            {
                // verify state and arguments.
                if (subscription_ == null) throw new NotConnectedException();

                // invoke COM method.
                try
                {
                    ((IOPCEventSubscriptionMgt)subscription_).SetFilter(
                        filters.EventTypes,
                        filters.Categories.Count,
                        filters.Categories.ToArray(),
                        filters.LowSeverity,
                        filters.HighSeverity,
                        filters.Areas.Count,
                        filters.Areas.ToArray(),
                        filters.Sources.Count,
                        filters.Sources.ToArray());
                }
                catch (Exception e)
                {
                    throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException("IOPCEventSubscriptionMgt.SetFilter", e);
                }
            }
        }

        //======================================================================
        // Attribute Management

        /// <summary>
        /// Returns the set of attributes to return with event notifications.
        /// </summary>
        /// <returns>The set of attributes to returned with event notifications.</returns>
        public int[] GetReturnedAttributes(int eventCategory)
        {
            lock (this)
            {
                // verify state and arguments.
                if (subscription_ == null) throw new NotConnectedException();

                // initialize arguments.
                int pdwCount;
                IntPtr ppidAttributeIDs;

                // invoke COM method.
                try
                {
                    ((IOPCEventSubscriptionMgt)subscription_).GetReturnedAttributes(
                        eventCategory,
                        out pdwCount,
                        out ppidAttributeIDs);
                }
                catch (Exception e)
                {
                    throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException("IOPCEventSubscriptionMgt.GetReturnedAttributes", e);
                }

                // marshal results 
                var attributeIDs = Technosoftware.DaAeHdaClient.Com.Interop.GetInt32s(ref ppidAttributeIDs, pdwCount, true);

                // return results.
                return attributeIDs;
            }
        }

        /// <summary>
        /// Selects the set of attributes to return with event notifications.
        /// </summary>
        /// <param name="eventCategory">The specific event category for which the attributes apply.</param>
        /// <param name="attributeIDs">The list of attribute ids to return.</param>
        public void SelectReturnedAttributes(int eventCategory, int[] attributeIDs)
        {
            lock (this)
            {
                // verify state and arguments.
                if (subscription_ == null) throw new NotConnectedException();

                // invoke COM method.
                try
                {
                    ((IOPCEventSubscriptionMgt)subscription_).SelectReturnedAttributes(
                        eventCategory,
                        attributeIDs?.Length ?? 0,
                        attributeIDs ?? Array.Empty<int>());
                }
                catch (Exception e)
                {
                    throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException("IOPCEventSubscriptionMgt.SelectReturnedAttributes", e);
                }
            }
        }

        //======================================================================
        // Refresh

        /// <summary>
        /// Force a refresh for all active conditions and inactive, unacknowledged conditions whose event notifications match the filter of the event subscription.
        /// </summary>
        public void Refresh()
        {
            lock (this)
            {
                // verify state and arguments.
                if (subscription_ == null) throw new NotConnectedException();

                // invoke COM method.
                try
                {
                    ((IOPCEventSubscriptionMgt)subscription_).Refresh(0);
                }
                catch (Exception e)
                {
                    throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException("IOPCEventSubscriptionMgt.Refresh", e);
                }
            }
        }

        /// <summary>
        /// Cancels an outstanding refresh request.
        /// </summary>
        public void CancelRefresh()
        {
            lock (this)
            {
                // verify state and arguments.
                if (subscription_ == null) throw new NotConnectedException();

                // invoke COM method.
                try
                {
                    ((IOPCEventSubscriptionMgt)subscription_).CancelRefresh(0);
                }
                catch (Exception e)
                {
                    throw Technosoftware.DaAeHdaClient.Com.Interop.CreateException("IOPCEventSubscriptionMgt.CancelRefresh", e);
                }
            }
        }
        #endregion

        #region IOPCEventSink Members
        /// <summary>
        /// A class that implements the IOPCEventSink interface.
        /// </summary>
        private class Callback : IOPCEventSink
        {
            /// <summary>
            /// Initializes the object with the containing subscription object.
            /// </summary>
            public Callback(object clientHandle)
            {
                clientHandle_ = clientHandle;
            }

            /// <summary>
            /// Raised when data changed callbacks arrive.
            /// </summary>
            public event TsCAeDataChangedEventHandler DataChangedEvent
            {
                add { lock (this) { DataChangedEventHandler += value; } }
                remove { lock (this) { DataChangedEventHandler -= value; } }
            }

            /// <summary>
            /// Called when a data changed event is received.
            /// </summary>
			public void OnEvent(
                int hClientSubscription,
                int bRefresh,
                int bLastRefresh,
                int dwCount,
                ONEVENTSTRUCT[] pEvents)
            {
                LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.AlarmsConditions, true);
                try
                {
                    lock (this)
                    {
                        // do nothing if no connections.
                        if (DataChangedEventHandler == null) return;

                        // un marshal item values.
                        var notifications = Interop.GetEventNotifications(pEvents);

                        foreach (var notification in notifications)
                        {
                            notification.ClientHandle = clientHandle_;
                        }

                        if (!LicenseHandler.IsExpired)
                        {
                            // invoke the callback.
                            DataChangedEventHandler?.Invoke(notifications, bRefresh != 0, bLastRefresh != 0);
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.Trace(e, "Exception '{0}' in event handler.", e.Message);
                }
            }

            #region Private Members
            private object clientHandle_;
            private event TsCAeDataChangedEventHandler DataChangedEventHandler;
            #endregion
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
                connection_ = new ConnectionPoint(subscription_, typeof(IOPCEventSink).GUID);
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

        #region Private Members
        private object subscription_;
        private object clientHandle_;
        private bool supportsAe11_ = true;
        private ConnectionPoint connection_;
        private Callback callback_;

        /// <summary>
        /// The synchronization object for subscription access
        /// </summary>
        private static volatile object lock_ = new object();

        private bool disposed_;

        #endregion
    }
}
