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
using System.Runtime.Serialization;
#endregion

namespace Technosoftware.DaAeHdaClient.Da
{
    /// <summary>
    /// An in-process object used to access subscriptions on OPC Data Access servers.
    /// </summary>
    [Serializable]
    public class TsCDaSubscription : ITsCDaSubscription, ISerializable, ICloneable
    {
        #region Names Class

        /// <summary>
        /// A set of names for fields used in serialization.
        /// </summary>
        private class Names
        {
            internal const string State = "State";
            internal const string Filters = "Filters";
            internal const string Items = "Items";
        }

        #endregion

        #region Fields

        private bool disposed_;

        /// <summary>
        /// The containing server object.
        /// </summary>
        private TsCDaServer server_;

        /// <summary>
        /// The remote subscription object.
        /// </summary>
        internal ITsCDaSubscription Subscription;

        /// <summary>
        /// The local copy of the subscription state.
        /// </summary>
        private TsCDaSubscriptionState subscriptionState_ = new TsCDaSubscriptionState();

        /// <summary>
        /// The local copy of all subscription items.
        /// </summary>
        private TsCDaItem[] daItems_;

        /// <summary>
        /// Whether data callbacks are enabled.
        /// </summary>
        private bool enabled_ = true;

        /// <summary>
        /// The local copy of the result filters.
        /// </summary>
        private int filters_ = (int)TsCDaResultFilter.All | (int)TsCDaResultFilter.ClientHandle;
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes object with default values.
        /// </summary>
        public TsCDaSubscription(TsCDaServer server, ITsCDaSubscription subscription)
        {
            server_ = server ?? throw new ArgumentNullException(nameof(server));
            Subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));

            GetResultFilters();
            GetState();
        }

        /// <summary>
        /// Constructs a server by de-serializing its OpcUrl from the stream.
        /// </summary>
        protected TsCDaSubscription(SerializationInfo info, StreamingContext context)
        {
            subscriptionState_ = (TsCDaSubscriptionState)info.GetValue(Names.State, typeof(TsCDaSubscriptionState));
            filters_ = (int)info.GetValue(Names.Filters, typeof(int));
            daItems_ = (TsCDaItem[])info.GetValue(Names.Items, typeof(TsCDaItem[]));
        }

        /// <summary>
        /// The finalizer implementation.
        /// </summary>
        ~TsCDaSubscription()
        {
            Dispose(false);
        }

        /// <summary>
        /// This must be called explicitly by clients to ensure the remote server is released.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
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
            // Check to see if Dispose has already been called.
            if (!disposed_)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    if (Subscription != null)
                    {
                        Subscription.Dispose();

                        server_ = null;
                        Subscription = null;
                        daItems_ = null;
                    }
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.
            }
            disposed_ = true;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The server that the subscription is attached to.
        /// </summary>
        public TsCDaServer Server => server_;

        /// <summary>
        /// The name assigned to the subscription by the client.
        /// </summary>
        public string Name => subscriptionState_.Name;

        /// <summary>
        /// The handle assigned to the subscription by the client.
        /// </summary>
        public object ClientHandle => subscriptionState_.ClientHandle;

        /// <summary>
        /// The handle assigned to the subscription by the server.
        /// </summary>
        public object ServerHandle => subscriptionState_.ServerHandle;

        /// <summary>
        /// Whether the subscription is active.
        /// </summary>
        public bool Active => subscriptionState_.Active;

        /// <summary>
        /// Whether data callbacks are enabled.
        /// </summary>
        public bool Enabled => enabled_;

        /// <summary>
        /// The current locale used by the subscription.
        /// </summary>
        public string Locale => subscriptionState_.Locale;

        /// <summary>
        /// The current result filters applied by the subscription.
        /// </summary>
        public int Filters => filters_;

        /// <summary>
        /// Returns a copy of the current subscription state.
        /// </summary>
        public TsCDaSubscriptionState State => (TsCDaSubscriptionState)subscriptionState_.Clone();

        /// <summary>
        /// The items belonging to the subscription.
        /// </summary>
        public TsCDaItem[] Items
        {
            get
            {
                if (daItems_ == null) return new TsCDaItem[0];
                var items = new TsCDaItem[daItems_.Length];
                for (var ii = 0; ii < daItems_.Length; ii++) items[ii] = (TsCDaItem)daItems_[ii].Clone();
                return items;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Serializes a server into a stream.
        /// </summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(Names.State, subscriptionState_);
            info.AddValue(Names.Filters, filters_);
            info.AddValue(Names.Items, daItems_);
        }

        /// <summary>
        /// Returns an unconnected copy of the subscription with the same items.
        /// </summary>
        public virtual object Clone()
        {
            // do a memberwise clone.
            var clone = (TsCDaSubscription)MemberwiseClone();

            // place clone in disconnected state.
            clone.server_ = null;
            clone.Subscription = null;
            clone.subscriptionState_ = (TsCDaSubscriptionState)subscriptionState_.Clone();

            // clear server handles.
            clone.subscriptionState_.ServerHandle = null;

            // always make cloned subscriptions inactive.
            clone.subscriptionState_.Active = false;

            // clone items.
            if (clone.daItems_ != null)
            {
                var items = new ArrayList();

                Array.ForEach(clone.daItems_, item => items.Add(item.Clone()));

                clone.daItems_ = (TsCDaItem[])items.ToArray(typeof(TsCDaItem));
            }

            // return clone.
            return clone;
        }

        /// <summary>
        /// Gets default result filters for the server.
        /// </summary>
        public int GetResultFilters()
        {
            filters_ = Subscription.GetResultFilters();
            return filters_;
        }

        /// <summary>
        /// Sets default result filters for the server.
        /// </summary>
        public void SetResultFilters(int filters)
        {
            Subscription.SetResultFilters(filters);
            filters_ = filters;
        }

        /// <summary>
        /// Returns the current subscription state.
        /// </summary>
        public TsCDaSubscriptionState GetState()
        {
            subscriptionState_ = Subscription.GetState();
            return subscriptionState_;
        }

        /// <summary>
        /// Updates the current subscription state.
        /// </summary>
        public TsCDaSubscriptionState ModifyState(int masks, TsCDaSubscriptionState state)
        {
            subscriptionState_ = Subscription.ModifyState(masks, state);
            return subscriptionState_;
        }

        /// <summary>
        /// Adds items to the subscription.
        /// </summary>
        public virtual TsCDaItemResult[] AddItems(TsCDaItem[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            if (items == null) throw new ArgumentNullException(nameof(items));

            // check if there is nothing to do.
            if (items.Length == 0)
            {
                return new TsCDaItemResult[0];
            }

            // add items.
            var results = Subscription.AddItems(items);

            if (results == null || results.Length == 0)
            {
                throw new OpcResultException(new OpcResult(OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
            }

            // update locale item list.
            var itemList = new ArrayList();
            if (daItems_ != null) itemList.AddRange(daItems_);

            for (var ii = 0; ii < results.Length; ii++)
            {
                // check for failure.
                if (results[ii].Result.Failed())
                {
                    continue;
                }

                // create locale copy of the item.
                // item name, item path and client handle may not be returned by server.
                var item = new TsCDaItem(results[ii]) { ItemName = items[ii].ItemName, ItemPath = items[ii].ItemPath, ClientHandle = items[ii].ClientHandle };

                itemList.Add(item);
            }

            // save the new item list.
            daItems_ = (TsCDaItem[])itemList.ToArray(typeof(TsCDaItem));

            // update the local state.
            GetState();

            // return results.
            return results;
        }

        /// <summary>
        /// Modifies items that are already part of the subscription.
        /// </summary>
        public virtual TsCDaItemResult[] ModifyItems(int masks, TsCDaItem[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            if (items == null) throw new ArgumentNullException(nameof(items));

            // check if there is nothing to do.
            if (items.Length == 0)
            {
                return new TsCDaItemResult[0];
            }

            // modify items.
            var results = Subscription.ModifyItems(masks, items);

            if (results == null || results.Length == 0)
            {
                throw new OpcResultException(new OpcResult(OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
            }

            // update local item - modify item success means all fields were updated successfully.
            for (var ii = 0; ii < results.Length; ii++)
            {
                // check for failure.
                if (results[ii].Result.Failed())
                {
                    continue;
                }

                // search local item list.
                for (var jj = 0; jj < daItems_.Length; jj++)
                {
                    if (daItems_[jj].ServerHandle.Equals(items[ii].ServerHandle))
                    {
                        // update locale copy of the item.
                        // item name, item path and client handle may not be returned by server.
                        var item = new TsCDaItem(results[ii]) { ItemName = daItems_[jj].ItemName, ItemPath = daItems_[jj].ItemPath, ClientHandle = daItems_[jj].ClientHandle };

                        daItems_[jj] = item;
                        break;
                    }
                }
            }

            // update the local state.
            GetState();

            // return results.
            return results;
        }

        /// <summary>
        /// Removes items from a subscription.
        /// </summary>
        public virtual OpcItemResult[] RemoveItems(OpcItem[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            if (items == null) throw new ArgumentNullException(nameof(items));

            // check if there is nothing to do.
            if (items.Length == 0)
            {
                return new OpcItemResult[0];
            }

            // remove items from server.
            var results = Subscription.RemoveItems(items);

            if (results == null || results.Length == 0)
            {
                throw new OpcResultException(new OpcResult(OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
            }

            // remove items from local list if successful.
            var itemList = new ArrayList();

            foreach (var item in daItems_)
            {
                var removed = false;

                for (var ii = 0; ii < results.Length; ii++)
                {
                    if (item.ServerHandle.Equals(items[ii].ServerHandle))
                    {
                        removed = results[ii].Result.Succeeded();
                        break;
                    }
                }

                if (!removed) itemList.Add(item);
            }

            // update local list.
            daItems_ = (TsCDaItem[])itemList.ToArray(typeof(TsCDaItem));

            // update the local state.
            GetState();

            // return results.
            return results;
        }

        /// <summary>
        /// Reads a set of subscription items.
        /// </summary>
        public TsCDaItemValueResult[] Read(TsCDaItem[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            return Subscription.Read(items);
        }

        /// <summary>
        /// Writes a set of subscription items.
        /// </summary>
        public OpcItemResult[] Write(TsCDaItemValue[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            return Subscription.Write(items);
        }

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
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            return Subscription.Read(items, requestHandle, callback, out request);
        }

        /// <summary>
        /// Begins an asynchronous write operation for a set of items.
        /// </summary>
        /// <param name="items">The set of item values to write (must include the item name).</param>
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
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            return Subscription.Write(items, requestHandle, callback, out request);
        }

        /// <summary>
        /// Cancels an asynchronous request.
        /// </summary>
        public void Cancel(IOpcRequest request, TsCDaCancelCompleteEventHandler callback)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            Subscription.Cancel(request, callback);
        }

        /// <summary>
        /// Tells the server to send an data change update for all subscription items. 
        /// </summary>
        public void Refresh() { Subscription.Refresh(); }

        /// <summary>
        /// Causes the server to send a data changed notification for all active items. 
        /// </summary>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public void Refresh(
            object requestHandle,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            Subscription.Refresh(requestHandle, out request);
        }

        /// <summary>
        /// Sets whether data change callbacks are enabled.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            Subscription.SetEnabled(enabled);
            enabled_ = enabled;
        }

        /// <summary>
        /// Gets whether data change callbacks are enabled.
        /// </summary>
        public bool GetEnabled()
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            enabled_ = Subscription.GetEnabled();
            return enabled_;
        }
        #endregion

        #region ISubscription
        /// <summary>
        /// An event to receive data change updates.
        /// </summary>
        public event TsCDaDataChangedEventHandler DataChangedEvent
        {
            add => Subscription.DataChangedEvent += value;
            remove => Subscription.DataChangedEvent -= value;
        }
        #endregion
    }
}
