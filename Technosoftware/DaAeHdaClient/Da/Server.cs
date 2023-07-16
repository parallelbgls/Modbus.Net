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
using System.Collections.Generic;
using System.Runtime.Serialization;
#endregion

namespace Technosoftware.DaAeHdaClient.Da
{

    /// <summary>
    /// This class is the main interface to access an OPC Data Access server.
    /// </summary>
    [Serializable]
    public class TsCDaServer : OpcServer, ITsDaServer
    {
        #region Names Class
        /// <summary>A set of names for fields used in serialization.</summary>
        private class Names
        {
            internal const string Filters = "Filters";
            internal const string Subscriptions = "Subscription";
        }
        #endregion

        #region Fields
        /// <summary>
        /// A list of subscriptions for the server.
        /// </summary>
        private TsCDaSubscriptionCollection subscriptions_ = new TsCDaSubscriptionCollection();

        /// <summary>
        /// The local copy of the result filters.
        /// </summary>
        private int filters_ = (int)TsCDaResultFilter.All | (int)TsCDaResultFilter.ClientHandle;
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes the object.
        /// </summary>
        public TsCDaServer()

        {
        }

        /// <summary>
        /// Initializes the object with a factory and a default OpcUrl.
        /// </summary>
        /// <param name="factory">The OpcFactory used to connect to remote servers.</param>
        /// <param name="url">The network address of a remote server.</param>
        public TsCDaServer(OpcFactory factory, OpcUrl url)
            :
            base(factory, url)
        {
        }

        /// <summary>
        /// Constructs a server by de-serializing its OpcUrl from the stream.
        /// </summary>
        protected TsCDaServer(SerializationInfo info, StreamingContext context)
            :
            base(info, context)
        {
            filters_ = (int)info.GetValue(Names.Filters, typeof(int));

            var subscriptions = (TsCDaSubscription[])info.GetValue(Names.Subscriptions, typeof(TsCDaSubscription[]));

            if (subscriptions != null)
            {
                Array.ForEach(subscriptions, subscription => subscriptions_.Add(subscription));
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns an array of all subscriptions for the server.
        /// </summary>
        public TsCDaSubscriptionCollection Subscriptions => subscriptions_;

        /// <summary>
		/// The current result filters applied by the server.
		/// </summary>
		public int Filters => filters_;
        #endregion

        #region Class properties serialization helpers
        /// <summary>Serializes a server into a stream.</summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(Names.Filters, filters_);

            TsCDaSubscription[] subscriptions = null;

            if (subscriptions_.Count > 0)
            {
                subscriptions = new TsCDaSubscription[subscriptions_.Count];

                for (var ii = 0; ii < subscriptions.Length; ii++)
                {
                    subscriptions[ii] = subscriptions_[ii];
                }
            }

            info.AddValue(Names.Subscriptions, subscriptions);
        }
        #endregion

        #region Public Methods
        /// <summary>Returns an unconnected copy of the server with the same OpcUrl.</summary>
        public override object Clone()
        {
            // clone the base object.
            var clone = (TsCDaServer)base.Clone();

            // clone subscriptions.
            if (clone.subscriptions_ != null)
            {
                var subscriptions = new TsCDaSubscriptionCollection();

                foreach (TsCDaSubscription subscription in clone.subscriptions_)
                {
                    subscriptions.Add(subscription.Clone());
                }

                clone.subscriptions_ = subscriptions;
            }

            // return clone.
            return clone;
        }

        /// <summary>Connects to the server with the specified OpcUrl and credentials.</summary>
        /// <exception caption="OpcResultException Class" cref="OpcResultException">If an OPC specific error occur this exception is raised. The Result field includes then the OPC specific code.</exception>
        /// <param name="url">The network address of the remote server.</param>
        /// <param name="connectData">Any protocol configuration or user authentication information.</param>
        public override void Connect(OpcUrl url, OpcConnectData connectData)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            // connect to server.
            base.Connect(url, connectData);

            // all done if no subscriptions.
            if (subscriptions_ == null)
            {
                return;
            }

            // create subscriptions (should only happen if server has been deserialized).
            var subscriptions = new TsCDaSubscriptionCollection();

            foreach (TsCDaSubscription template in subscriptions_)
            {
                // create subscription for template.
                try
                {
                    subscriptions.Add(EstablishSubscription(template));
                }
                catch
                {
                    // Ignore exceptions here
                }
            }

            // save new set of subscriptions.
            subscriptions_ = subscriptions;
        }

        /// <summary>Disconnects from the server and releases all network resources.</summary>
        public override void Disconnect()
        {
            if (Server == null) throw new NotConnectedException();

            // dispose of all subscriptions first.
            if (subscriptions_ != null)
            {
                foreach (TsCDaSubscription subscription in subscriptions_)
                {
                    subscription.Dispose();
                }

                subscriptions_ = null;
            }

            // disconnect from server.
            base.Disconnect();
        }

        /// <summary>Returns the filters applied by the server to any item results returned to the client.</summary>
        /// <returns>A bit mask indicating which fields should be returned in any item results.</returns>
        public int GetResultFilters()
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            if (Server == null) throw new NotConnectedException();

            // update local cache.
            filters_ = ((ITsDaServer)Server).GetResultFilters();

            // return filters.
            return filters_;
        }

        /// <summary>Sets the filters applied by the server to any item results returned to the client.</summary>
        /// <param name="filters">A bit mask indicating which fields should be returned in any item results.</param>
        public void SetResultFilters(int filters)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            if (Server == null) throw new NotConnectedException();

            // set filters on server.
            ((ITsDaServer)Server).SetResultFilters(filters);

            // cache updated filters.
            filters_ = filters;
        }

        /// <summary>Returns the current server status.</summary>
        /// <returns>The current server status.</returns>
        public OpcServerStatus GetServerStatus()
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            if (Server == null) throw new NotConnectedException();

            var status = ((ITsDaServer)Server).GetServerStatus();

            if (status != null)
            {
                if (status.StatusInfo == null)
                {
                    status.StatusInfo = GetString($"serverState.{status.ServerState}");
                }
            }
            else
            {
                throw new NotConnectedException();
            }

            return status;
        }

        /// <summary>Reads the current values for a set of items.</summary>
        /// <returns>The results of the read operation for each item.</returns>
        /// <requirements>OPC XML-DA Server or OPC Data Access Server V3.x</requirements>
        /// <param name="items">The set of items to read.</param>
        public TsCDaItemValueResult[] Read(TsCDaItem[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            if (Server == null) throw new NotConnectedException();

            return ((ITsDaServer)Server).Read(items);
        }

        /// <summary>Writes the value, quality and timestamp for a set of items.</summary>
        /// <returns>The results of the write operation for each item.</returns>
        /// <requirements>OPC XML-DA Server or OPC Data Access Server V3.x</requirements>
        /// <param name="items">The set of item values to write.</param>
        public OpcItemResult[] Write(TsCDaItemValue[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            if (Server == null) throw new NotConnectedException();

            return ((ITsDaServer)Server).Write(items);
        }

        /// <summary>
        /// Creates a new subscription.
        /// </summary>
        /// <returns>The new subscription object.</returns>
        /// <requirements>OPC XML-DA Server or OPC Data Access Server V2.x / V3.x</requirements>
        /// <param name="state">The initial state of the subscription.</param>
        public virtual ITsCDaSubscription CreateSubscription(TsCDaSubscriptionState state)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (Server == null) throw new NotConnectedException();

            // create subscription on server.
            var subscription = ((ITsDaServer)Server).CreateSubscription(state);

            // set filters.
            subscription.SetResultFilters(filters_);

            // append new subscription to existing list.
            var subscriptions = new TsCDaSubscriptionCollection();

            if (subscriptions_ != null)
            {
                foreach (TsCDaSubscription value in subscriptions_)
                {
                    subscriptions.Add(value);
                }
            }

            subscriptions.Add(CreateSubscription(subscription));

            // save new subscription list.
            subscriptions_ = subscriptions;

            // return new subscription.
            return subscriptions_[subscriptions_.Count - 1];
        }

        /// <summary>
        /// Creates a new instance of the appropriate subscription object.
        /// </summary>
        /// <param name="subscription">The remote subscription object.</param>
        protected virtual TsCDaSubscription CreateSubscription(ITsCDaSubscription subscription)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            return new TsCDaSubscription(this, subscription);
        }

        /// <summary>Cancels a subscription and releases all resources allocated for it.</summary>
        /// <requirements>OPC XML-DA Server or OPC Data Access Server V2.x / V3.x</requirements>
        /// <param name="subscription">The subscription to cancel.</param>
        public virtual void CancelSubscription(ITsCDaSubscription subscription)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            if (subscription == null) throw new ArgumentNullException(nameof(subscription));
            if (Server == null) throw new NotConnectedException();

            // validate argument.
            if (!typeof(TsCDaSubscription).IsInstanceOfType(subscription))
            {
                throw new ArgumentException(@"Incorrect object type.", nameof(subscription));
            }

            if (!Equals(((TsCDaSubscription)subscription).Server))
            {
                throw new ArgumentException(@"Server subscription.", nameof(subscription));
            }

            // search for subscription in list of subscriptions.
            var subscriptions = new TsCDaSubscriptionCollection();

            foreach (TsCDaSubscription current in subscriptions_)
            {
                if (!subscription.Equals(current))
                {
                    subscriptions.Add(current);
                }
            }

            // check if subscription was not found.
            if (subscriptions.Count == subscriptions_.Count)
            {
                throw new ArgumentException(@"Subscription not found.", nameof(subscription));
            }

            // remove subscription from list of subscriptions.
            subscriptions_ = subscriptions;

            // cancel subscription on server.
            ((ITsDaServer)Server).CancelSubscription(((TsCDaSubscription)subscription).Subscription);
        }

        /// <summary>Fetches all the children of the root branch that meet the filter criteria.</summary>
        /// <returns>The set of elements found.</returns>
        /// <requirements>OPC Data Access Server V2.x / V3.x</requirements>
        /// <param name="filters">The filters to use to limit the set of child elements returned.</param>
        private TsCDaBrowseElement[] Browse(
            TsCDaBrowseFilters filters)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            if (Server == null) throw new NotConnectedException();
            TsCDaBrowsePosition position;
            var elementsList = new List<TsCDaBrowseElement>();

            var elements = ((ITsDaServer)Server).Browse(null, filters, out position);

            if (elements != null)
            {
                Browse(elements, filters, ref elementsList);
            }

            return elementsList.ToArray();
        }

        private void Browse(TsCDaBrowseElement[] elements, TsCDaBrowseFilters filters, ref List<TsCDaBrowseElement> elementsList)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            TsCDaBrowsePosition position;

            foreach (var element in elements)
            {
                if (element.HasChildren)
                {
                    var itemId = new OpcItem(element.ItemPath, element.ItemName);

                    var childElements = ((ITsDaServer)Server).Browse(itemId, filters, out position);
                    if (childElements != null)
                    {
                        Browse(childElements, filters, ref elementsList);
                    }

                }
                else
                {
                    elementsList.Add(element);
                }
            }
        }

        /// <summary>Fetches the children of a branch that meet the filter criteria.</summary>
        /// <returns>The set of elements found.</returns>
        /// <requirements>OPC XML-DA Server or OPC Data Access Server V2.x / V3.x</requirements>
        /// <param name="itemId">The identifier of branch which is the target of the search.</param>
        /// <param name="filters">The filters to use to limit the set of child elements returned.</param>
        /// <param name="position">An object used to continue a browse that could not be completed.</param>
        public TsCDaBrowseElement[] Browse(
            OpcItem itemId,
            TsCDaBrowseFilters filters,
            out TsCDaBrowsePosition position)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsDaServer)Server).Browse(itemId, filters, out position);
        }

        /// <summary>Continues a browse operation with previously specified search criteria.</summary>
        /// <returns>The set of elements found.</returns>
        /// <requirements>OPC XML-DA Server or OPC Data Access Server V2.x / V3.x</requirements>
        /// <param name="position">An object containing the browse operation state information.</param>
        public TsCDaBrowseElement[] BrowseNext(ref TsCDaBrowsePosition position)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsDaServer)Server).BrowseNext(ref position);
        }

        /// <summary>Returns the item properties for a set of items.</summary>
        /// <param name="itemIds">A list of item identifiers.</param>
        /// <param name="propertyIDs">A list of properties to fetch for each item.</param>
        /// <param name="returnValues">Whether the property values should be returned with the properties.</param>
        /// <returns>A list of properties for each item.</returns>
        public TsCDaItemPropertyCollection[] GetProperties(
            OpcItem[] itemIds,
            TsDaPropertyID[] propertyIDs,
            bool returnValues)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.DataAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsDaServer)Server).GetProperties(itemIds, propertyIDs, returnValues);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Establishes a subscription based on the template provided.
        /// </summary>
        private TsCDaSubscription EstablishSubscription(TsCDaSubscription template)
        {
            // create subscription.
            var subscription = new TsCDaSubscription(this, ((ITsDaServer)Server).CreateSubscription(template.State));

            // set filters.
            subscription.SetResultFilters(template.Filters);

            // add items.
            try
            {
                subscription.AddItems(template.Items);
            }
            catch
            {
                subscription.Dispose();
                subscription = null;
            }

            // return new subscription.
            return subscription;
        }
        #endregion

    }
}
