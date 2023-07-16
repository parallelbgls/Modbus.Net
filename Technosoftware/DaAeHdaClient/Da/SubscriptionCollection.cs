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
#endregion

namespace Technosoftware.DaAeHdaClient.Da
{
    /// <summary>A collection of subscriptions.</summary>
    [Serializable]
    public class TsCDaSubscriptionCollection : ICollection, ICloneable, IList
    {
        ///////////////////////////////////////////////////////////////////////
        #region Fields

        private ArrayList _subscriptions = new ArrayList();

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Constructors, Destructor, Initialization

        /// <summary>
        /// Initializes object with the default values.
        /// </summary>
        public TsCDaSubscriptionCollection() { }

        /// <summary>
        /// Initializes object with the specified SubscriptionCollection object.
        /// </summary>
        public TsCDaSubscriptionCollection(TsCDaSubscriptionCollection subscriptions)
        {
            if (subscriptions != null)
            {
                foreach (TsCDaSubscription subscription in subscriptions)
                {
                    Add(subscription);
                }
            }
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Properties

        /// <summary>
        ///  Gets the item at the specified index.
        /// </summary>
        public TsCDaSubscription this[int index]
        {
            get => (TsCDaSubscription)_subscriptions[index];
            set => _subscriptions[index] = value;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region ICloneable Members

        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public virtual object Clone()
        {
            var clone = (TsCDaSubscriptionCollection)MemberwiseClone();

            clone._subscriptions = new ArrayList();

            foreach (TsCDaSubscription subscription in _subscriptions)
            {
                clone._subscriptions.Add(subscription.Clone());
            }

            return clone;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region ICollection Members

        /// <summary>
        /// Indicates whether access to the ICollection is synchronized (thread-safe).
        /// </summary>
        public bool IsSynchronized => false;

        /// <summary>
		/// Gets the number of objects in the collection.
		/// </summary>
		public int Count => (_subscriptions != null) ? _subscriptions.Count : 0;

        /// <summary>
		/// Copies the objects to an Array, starting at a the specified index.
		/// </summary>
		/// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
		/// <param name="index">The zero-based index in the Array at which copying begins.</param>
		public void CopyTo(Array array, int index)
        {
            if (_subscriptions != null)
            {
                _subscriptions.CopyTo(array, index);
            }
        }

        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public void CopyTo(TsCDaSubscription[] array, int index)
        {
            CopyTo((Array)array, index);
        }

        /// <summary>
        /// Indicates whether access to the ICollection is synchronized (thread-safe).
        /// </summary>
        public object SyncRoot => this;

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that can iterate through a collection.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return _subscriptions.GetEnumerator();
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region IList Members

        /// <summary>
        /// Gets a value indicating whether the IList is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		object IList.this[int index]
        {
            get => _subscriptions[index];

            set
            {
                if (!typeof(TsCDaSubscription).IsInstanceOfType(value))
                {
                    throw new ArgumentException("May only add Subscription objects into the collection.");
                }

                _subscriptions[index] = value;
            }
        }

        /// <summary>
        /// Removes the IList subscription at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the subscription to remove.</param>
        public void RemoveAt(int index)
        {
            _subscriptions.RemoveAt(index);
        }

        /// <summary>
        /// Inserts an subscription to the IList at the specified position.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The Object to insert into the IList. </param>
        public void Insert(int index, object value)
        {
            if (!typeof(TsCDaSubscription).IsInstanceOfType(value))
            {
                throw new ArgumentException("May only add Subscription objects into the collection.");
            }

            _subscriptions.Insert(index, value);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the IList.
        /// </summary>
        /// <param name="value">The Object to remove from the IList.</param>
        public void Remove(object value)
        {
            _subscriptions.Remove(value);
        }

        /// <summary>
        /// Determines whether the IList contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>true if the Object is found in the IList; otherwise, false.</returns>
        public bool Contains(object value)
        {
            return _subscriptions.Contains(value);
        }

        /// <summary>
        /// Removes all subscriptions from the IList.
        /// </summary>
        public void Clear()
        {
            _subscriptions.Clear();
        }

        /// <summary>
        /// Determines the index of a specific subscription in the IList.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        public int IndexOf(object value)
        {
            return _subscriptions.IndexOf(value);
        }

        /// <summary>
        /// Adds an subscription to the IList.
        /// </summary>
        /// <param name="value">The Object to add to the IList. </param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(object value)
        {
            if (!typeof(TsCDaSubscription).IsInstanceOfType(value))
            {
                throw new ArgumentException("May only add Subscription objects into the collection.");
            }

            return _subscriptions.Add(value);
        }

        /// <summary>
        /// Indicates whether the IList has a fixed size.
        /// </summary>
        public bool IsFixedSize => false;

        /// <summary>
		/// Inserts an subscription to the IList at the specified position.
		/// </summary>
		/// <param name="index">The zero-based index at which value should be inserted.</param>
		/// <param name="value">The Object to insert into the IList. </param>
		public void Insert(int index, TsCDaSubscription value)
        {
            Insert(index, (object)value);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the IList.
        /// </summary>
        /// <param name="value">The Object to remove from the IList.</param>
        public void Remove(TsCDaSubscription value)
        {
            Remove((object)value);
        }

        /// <summary>
        /// Determines whether the IList contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>true if the Object is found in the IList; otherwise, false.</returns>
        public bool Contains(TsCDaSubscription value)
        {
            return Contains((object)value);
        }

        /// <summary>
        /// Determines the index of a specific subscription in the IList.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        public int IndexOf(TsCDaSubscription value)
        {
            return IndexOf((object)value);
        }

        /// <summary>
        /// Adds an subscription to the IList.
        /// </summary>
        /// <param name="value">The Object to add to the IList. </param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(TsCDaSubscription value)
        {
            return Add((object)value);
        }

        #endregion
    }
}
