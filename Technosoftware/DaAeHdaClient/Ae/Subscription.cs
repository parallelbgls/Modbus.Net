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

namespace Technosoftware.DaAeHdaClient.Ae
{
    /// <summary>
    /// An in-process object which provides access to AE subscription objects.
    /// </summary>
    [Serializable]
    public class TsCAeSubscription : ITsCAeSubscription, ISerializable, ICloneable
    {
        #region CategoryCollection Class
        /// <summary>
        /// Contains a read-only collection category ids.
        /// </summary>
        public class CategoryCollection : OpcReadOnlyCollection
        {
            #region Constructors, Destructor, Initialization
            /// <summary>
            /// Creates an empty collection.
            /// </summary>
            internal CategoryCollection() : base(new int[0]) { }

            /// <summary>
            /// Creates a collection containing the list of category ids.
            /// </summary>
            internal CategoryCollection(int[] categoryIDs) : base(categoryIDs) { }
            #endregion

            #region Public Methods
            /// <summary>
            /// An indexer for the collection.
            /// </summary>
            public new int this[int index] => (int)Array.GetValue(index);

            /// <summary>
			/// Returns a copy of the collection as an array.
			/// </summary>
			public new int[] ToArray()
            {
                return (int[])OpcConvert.Clone(Array);
            }
            #endregion
        }
        #endregion

        #region StringCollection Class
        /// <summary>
        /// Contains a read-only collection of strings.
        /// </summary>
        public class StringCollection : OpcReadOnlyCollection
        {
            #region Constructors, Destructor, Initialization
            /// <summary>
            /// Creates an empty collection.
            /// </summary>
            internal StringCollection() : base(new string[0]) { }

            /// <summary>
            /// Creates a collection containing the specified strings.
            /// </summary>
            internal StringCollection(string[] strings) : base(strings) { }
            #endregion

            #region Public Methods
            /// <summary>
            /// An indexer for the collection.
            /// </summary>
            public new string this[int index] => (string)Array.GetValue(index);

            /// <summary>
			/// Returns a copy of the collection as an array.
			/// </summary>
			public new string[] ToArray()
            {
                return (string[])OpcConvert.Clone(Array);
            }
            #endregion
        }
        #endregion

        #region AttributeDictionary Class
        /// <summary>
        /// Contains a read-only dictionary of attribute lists indexed by category id.
        /// </summary>
        [Serializable]
        public class AttributeDictionary : OpcReadOnlyDictionary
        {
            #region Constructors, Destructor, Initialization
            /// <summary>
            /// Creates an empty collection.
            /// </summary>
            internal AttributeDictionary() : base(null) { }

            /// <summary>
            /// Constructs an dictionary from a set of category ids.
            /// </summary>
            internal AttributeDictionary(Hashtable dictionary) : base(dictionary) { }
            #endregion

            #region Public Methods
            /// <summary>
            /// Gets or sets the attribute collection for the specified category. 
            /// </summary>
            public AttributeCollection this[int categoryId] => (AttributeCollection)base[categoryId];

            /// <summary>
			/// Adds or replaces the set of attributes associated with the category.
			/// </summary>
			internal void Update(int categoryId, int[] attributeIDs)
            {
                Dictionary[categoryId] = new AttributeCollection(attributeIDs);
            }
            #endregion

            #region ISerializable Members
            /// <summary>
            /// Constructs an object by deserializing it from a stream.
            /// </summary>
            protected AttributeDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }
            #endregion
        }
        #endregion

        #region AttributeCollection Class
        /// <summary>
        /// Contains a read-only collection attribute ids.
        /// </summary>
        [Serializable]
        public class AttributeCollection : OpcReadOnlyCollection
        {
            #region Constructors, Destructor, Initialization
            /// <summary>
            /// Creates an empty collection.
            /// </summary>
            internal AttributeCollection() : base(new int[0]) { }

            /// <summary>
            /// Creates a collection containing the specified attribute ids.
            /// </summary>
            internal AttributeCollection(int[] attributeIDs) : base(attributeIDs) { }
            #endregion

            #region Public Methods
            /// <summary>
            /// An indexer for the collection.
            /// </summary>
            public new int this[int index] => (int)Array.GetValue(index);

            /// <summary>
			/// Returns a copy of the collection as an array.
			/// </summary>
			public new int[] ToArray()
            {
                return (int[])OpcConvert.Clone(Array);
            }
            #endregion

            #region ISerializable Members
            /// <summary>
            /// Constructs an object by deserializing it from a stream.
            /// </summary>
            protected AttributeCollection(SerializationInfo info, StreamingContext context) : base(info, context) { }
            #endregion
        }
        #endregion

        #region Names Class
        /// <summary>
        /// A set of names for fields used in serialization.
        /// </summary>
        private class Names
        {
            internal const string State = "ST";
            internal const string Filters = "FT";
            internal const string Attributes = "AT";
        }
        #endregion

        #region Fields
        private bool disposed_;
        private TsCAeServer server_;
        private ITsCAeSubscription subscription_;

        // state
        private TsCAeSubscriptionState state_;
        private string name_;

        // filters
        private TsCAeSubscriptionFilters subscriptionFilters_ = new TsCAeSubscriptionFilters();
        private CategoryCollection categories_ = new CategoryCollection();
        private StringCollection areas_ = new StringCollection();
        private StringCollection sources_ = new StringCollection();

        // returned attributes
        private AttributeDictionary attributes_ = new AttributeDictionary();
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes object with default values.
        /// </summary>
        public TsCAeSubscription(TsCAeServer server, ITsCAeSubscription subscription, TsCAeSubscriptionState state)
        {
            server_ = server ?? throw new ArgumentNullException(nameof(server));
            subscription_ = subscription ?? throw new ArgumentNullException(nameof(subscription));
            state_ = (TsCAeSubscriptionState)state.Clone();
            name_ = state.Name;
        }

        /// <summary>
        /// The finalizer implementation.
        /// </summary>
        ~TsCAeSubscription()
        {
            Dispose(false);
        }

        /// <summary>
        /// 
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
                    if (subscription_ != null)
                    {
                        server_.SubscriptionDisposed(this);
                        subscription_.Dispose();
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
        /// The server that the subscription object belongs to.
        /// </summary>
        public TsCAeServer Server => server_;

        /// <summary>
		/// A descriptive name for the subscription.
		/// </summary>
		public string Name => state_.Name;

        /// <summary>
		/// A unique identifier for the subscription assigned by the client.
		/// </summary>
		public object ClientHandle => state_.ClientHandle;

        /// <summary>
		/// Whether the subscription is monitoring for events to send to the client.
		/// </summary>
		public bool Active => state_.Active;

        /// <summary>
		/// The maximum rate at which the server send event notifications.
		/// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
		/// the time format (UTC or local   time).
		/// </summary>
		public int BufferTime => state_.BufferTime;

        /// <summary>
		/// The requested maximum number of events that will be sent in a single callback.
		/// </summary>
		public int MaxSize => state_.MaxSize;

        /// <summary>
		/// The maximum period between updates sent to the client.
		/// </summary>
		public int KeepAlive => state_.KeepAlive;

        /// <summary>
		/// A mask indicating which event types should be sent to the client.
		/// </summary>
		public int EventTypes => subscriptionFilters_.EventTypes;

        /// <summary>
		/// The highest severity for the events that should be sent to the client.
		/// </summary>
        // ReSharper disable once UnusedMember.Global
        public int HighSeverity => subscriptionFilters_.HighSeverity;

        /// <summary>
		/// The lowest severity for the events that should be sent to the client.
		/// </summary>
        // ReSharper disable once UnusedMember.Global
        public int LowSeverity => subscriptionFilters_.LowSeverity;

        /// <summary>
		/// The event category ids monitored by this subscription.
		/// </summary>
		public CategoryCollection Categories => categories_;

        /// <summary>
		/// A list of full-qualified ids for process areas of interest - only events or conditions in these areas will be reported.
		/// </summary>
		public StringCollection Areas => areas_;

        /// <summary>
		/// A list of full-qualified ids for sources of interest - only events or conditions from these sources will be reported.
		/// </summary>
		public StringCollection Sources => sources_;

        /// <summary>
		/// The list of attributes returned for each event category.
		/// </summary>
		public AttributeDictionary Attributes => attributes_;
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns a writable copy of the current attributes.
        /// </summary>
        public TsCAeAttributeDictionary GetAttributes()
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.AlarmsConditions);
            var attributes = new TsCAeAttributeDictionary();

            var enumerator = attributes_.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Key != null)
                {
                    var categoryId = (int)enumerator.Key;
                    var attributeIDs = (AttributeCollection)enumerator.Value;

                    attributes.Add(categoryId, attributeIDs.ToArray());
                }
            }

            return attributes;
        }
        #endregion

        #region ISerializable Members
        /// <summary>
        /// Constructs a server by de-serializing its OpcUrl from the stream.
        /// </summary>
        protected TsCAeSubscription(SerializationInfo info, StreamingContext context)
        {
            state_ = (TsCAeSubscriptionState)info.GetValue(Names.State, typeof(TsCAeSubscriptionState));
            subscriptionFilters_ = (TsCAeSubscriptionFilters)info.GetValue(Names.Filters, typeof(TsCAeSubscriptionFilters));
            attributes_ = (AttributeDictionary)info.GetValue(Names.Attributes, typeof(AttributeDictionary));

            name_ = state_.Name;

            categories_ = new CategoryCollection(subscriptionFilters_.Categories.ToArray());
            areas_ = new StringCollection(subscriptionFilters_.Areas.ToArray());
            sources_ = new StringCollection(subscriptionFilters_.Sources.ToArray());
        }

        /// <summary>
        /// Serializes a server into a stream.
        /// </summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(Names.State, state_);
            info.AddValue(Names.Filters, subscriptionFilters_);
            info.AddValue(Names.Attributes, attributes_);
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Returns an unconnected copy of the subscription with the same items.
        /// </summary>
        public virtual object Clone()
        {
            // do a memberwise clone.
            var clone = (TsCAeSubscription)MemberwiseClone();

            /*
			// place clone in disconnected state.
			clone.server       = null;
			clone.subscription = null;
			clone.state        = (SubscriptionState)state.Clone();

			// clear server handles.
			clone.state.ServerHandle = null;

			// always make cloned subscriptions inactive.
			clone.state.Active = false;

			// clone items.
			if (clone.items != null)
			{
				ArrayList items = new ArrayList();

				foreach (Item item in clone.items)
				{
					items.Add(item.Clone());
				}

				clone.items = (Item[])items.ToArray(typeof(Item));
			}
			*/

            // return clone.
            return clone;
        }
        #endregion

        #region ISubscription Members
        /// <summary>
        /// An event to receive data change updates.
        /// </summary>
        public event TsCAeDataChangedEventHandler DataChangedEvent
        {
            add => subscription_.DataChangedEvent += value;
            remove => subscription_.DataChangedEvent -= value;
        }

        /// <summary>
        /// Returns the current state of the subscription.
        /// </summary>
        /// <returns>The current state of the subscription.</returns>
        public TsCAeSubscriptionState GetState()
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.AlarmsConditions);
            if (subscription_ == null) throw new NotConnectedException();

            state_ = subscription_.GetState();
            state_.Name = name_;

            return (TsCAeSubscriptionState)state_.Clone();
        }

        /// <summary>
        /// Changes the state of a subscription.
        /// </summary>
        /// <param name="masks">A bit mask that indicates which elements of the subscription state are changing.</param>
        /// <param name="state">The new subscription state.</param>
        /// <returns>The actual subscription state after applying the changes.</returns>
        public TsCAeSubscriptionState ModifyState(int masks, TsCAeSubscriptionState state)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.AlarmsConditions);
            if (subscription_ == null) throw new NotConnectedException();

            state_ = subscription_.ModifyState(masks, state);

            if ((masks & (int)TsCAeStateMask.Name) != 0)
            {
                state_.Name = name_ = state.Name;
            }
            else
            {
                state_.Name = name_;
            }

            return (TsCAeSubscriptionState)state_.Clone();
        }

        /// <summary>
        /// Returns the current filters for the subscription.
        /// </summary>
        /// <returns>The current filters for the subscription.</returns>
        public TsCAeSubscriptionFilters GetFilters()
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.AlarmsConditions);
            if (subscription_ == null) throw new NotConnectedException();

            subscriptionFilters_ = subscription_.GetFilters();
            categories_ = new CategoryCollection(subscriptionFilters_.Categories.ToArray());
            areas_ = new StringCollection(subscriptionFilters_.Areas.ToArray());
            sources_ = new StringCollection(subscriptionFilters_.Sources.ToArray());

            return (TsCAeSubscriptionFilters)subscriptionFilters_.Clone();
        }

        /// <summary>
        /// Sets the current filters for the subscription.
        /// </summary>
        /// <param name="filters">The new filters to use for the subscription.</param>
        public void SetFilters(TsCAeSubscriptionFilters filters)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.AlarmsConditions);
            if (subscription_ == null) throw new NotConnectedException();

            subscription_.SetFilters(filters);

            GetFilters();
        }

        /// <summary>
        /// Returns the set of attributes to return with event notifications.
        /// </summary>
        /// <returns>The set of attributes to returned with event notifications.</returns>
        public int[] GetReturnedAttributes(int eventCategory)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.AlarmsConditions);
            if (subscription_ == null) throw new NotConnectedException();

            var attributeIDs = subscription_.GetReturnedAttributes(eventCategory);

            attributes_.Update(eventCategory, (int[])OpcConvert.Clone(attributeIDs));

            return attributeIDs;
        }

        /// <summary>
        /// Selects the set of attributes to return with event notifications.
        /// </summary>
        /// <param name="eventCategory">The specific event category for which the attributes apply.</param>
        /// <param name="attributeIDs">The list of attribute ids to return.</param>
        public void SelectReturnedAttributes(int eventCategory, int[] attributeIDs)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.AlarmsConditions);
            if (subscription_ == null) throw new NotConnectedException();

            subscription_.SelectReturnedAttributes(eventCategory, attributeIDs);

            attributes_.Update(eventCategory, (int[])OpcConvert.Clone(attributeIDs));
        }

        /// <summary>
        /// Force a refresh for all active conditions and inactive, unacknowledged conditions whose event notifications match the filter of the event subscription.
        /// </summary>
        public void Refresh()
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.AlarmsConditions);
            if (subscription_ == null) throw new NotConnectedException();

            subscription_.Refresh();
        }

        /// <summary>
        /// Cancels an outstanding refresh request.
        /// </summary>
        public void CancelRefresh()
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.AlarmsConditions);
            if (subscription_ == null) throw new NotConnectedException();

            subscription_.CancelRefresh();
        }
        #endregion

        #region Internal Properties
        /// <summary>
        /// The current state.
        /// </summary>
        internal TsCAeSubscriptionState State => state_;

        /// <summary>
		/// The current filters.
		/// </summary>
		internal TsCAeSubscriptionFilters Filters => subscriptionFilters_;

        #endregion
    }
}
