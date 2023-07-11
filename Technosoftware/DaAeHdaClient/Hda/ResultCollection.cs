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

namespace Technosoftware.DaAeHdaClient.Hda
{
    /// <summary>
    /// A collection of results passed to write or returned from an insert, replace or delete operation.
    /// </summary>
    [Serializable]
    public class TsCHdaResultCollection : OpcItem, ICollection, ICloneable, IList
    {
        #region Fields
        private ArrayList results_ = new ArrayList();
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes object with the default values.
        /// </summary>
        public TsCHdaResultCollection() { }

        /// <summary>
        /// Initializes object with the specified ItemIdentifier object.
        /// </summary>
        public TsCHdaResultCollection(OpcItem item) : base(item) { }

        /// <summary>
        /// Initializes object with the specified ResultCollection object.
        /// </summary>
        public TsCHdaResultCollection(TsCHdaResultCollection item)
            : base(item)
        {
            results_ = new ArrayList(item.results_.Count);

            foreach (TsCHdaResult value in item.results_)
            {
                results_.Add(value.Clone());
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accessor for elements in the collection.
        /// </summary>
        public TsCHdaResult this[int index]
        {
            get => (TsCHdaResult)results_[index];
            set => results_[index] = value;
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public override object Clone()
        {
            var collection = (TsCHdaResultCollection)base.Clone();

            collection.results_ = new ArrayList(results_.Count);

            foreach (TsCHdaResultCollection value in results_)
            {
                collection.results_.Add(value.Clone());
            }

            return collection;
        }
        #endregion

        #region ICollection Members
        /// <summary>
        /// Indicates whether access to the ICollection is synchronized (thread-safe).
        /// </summary>
        public bool IsSynchronized => false;

        /// <summary>
		/// Gets the number of objects in the collection.
		/// </summary>
		public int Count => results_?.Count ?? 0;

        /// <summary>
		/// Copies the objects to an Array, starting at a the specified index.
		/// </summary>
		/// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
		/// <param name="index">The zero-based index in the Array at which copying begins.</param>
		public void CopyTo(Array array, int index)
        {
            results_?.CopyTo(array, index);
        }

        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public void CopyTo(TsCHdaResult[] array, int index)
        {
            CopyTo((Array)array, index);
        }

        /// <summary>
        /// Indicates whether access to the ICollection is synchronized (thread-safe).
        /// </summary>
        public object SyncRoot => this;
        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Returns an enumerator that can iterate through a collection.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return results_.GetEnumerator();
        }
        #endregion

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
            get => results_[index];

            set
            {
                if (!(value is TsCHdaResult))
                {
                    throw new ArgumentException("May only add Result objects into the collection.");
                }

                results_[index] = value;
            }
        }

        /// <summary>
        /// Removes the IList item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            results_.RemoveAt(index);
        }

        /// <summary>
        /// Inserts an item to the IList at the specified position.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The Object to insert into the IList. </param>
        public void Insert(int index, object value)
        {
            if (!(value is TsCHdaResult))
            {
                throw new ArgumentException("May only add Result objects into the collection.");
            }

            results_.Insert(index, value);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the IList.
        /// </summary>
        /// <param name="value">The Object to remove from the IList.</param>
        public void Remove(object value)
        {
            results_.Remove(value);
        }

        /// <summary>
        /// Determines whether the IList contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>true if the Object is found in the IList; otherwise, false.</returns>
        public bool Contains(object value)
        {
            return results_.Contains(value);
        }

        /// <summary>
        /// Removes all items from the IList.
        /// </summary>
        public void Clear()
        {
            results_.Clear();
        }

        /// <summary>
        /// Determines the index of a specific item in the IList.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        public int IndexOf(object value)
        {
            return results_.IndexOf(value);
        }

        /// <summary>
        /// Adds an item to the IList.
        /// </summary>
        /// <param name="value">The Object to add to the IList. </param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(object value)
        {
            if (!(value is TsCHdaResult))
            {
                throw new ArgumentException("May only add Result objects into the collection.");
            }

            return results_.Add(value);
        }

        /// <summary>
        /// Indicates whether the IList has a fixed size.
        /// </summary>
        public bool IsFixedSize => false;

        /// <summary>
		/// Inserts an item to the IList at the specified position.
		/// </summary>
		/// <param name="index">The zero-based index at which value should be inserted.</param>
		/// <param name="value">The Object to insert into the IList. </param>
		public void Insert(int index, TsCHdaResult value)
        {
            Insert(index, (object)value);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the IList.
        /// </summary>
        /// <param name="value">The Object to remove from the IList.</param>
        public void Remove(TsCHdaResult value)
        {
            Remove((object)value);
        }

        /// <summary>
        /// Determines whether the IList contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>true if the Object is found in the IList; otherwise, false.</returns>
        public bool Contains(TsCHdaResult value)
        {
            return Contains((object)value);
        }

        /// <summary>
        /// Determines the index of a specific item in the IList.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        public int IndexOf(TsCHdaResult value)
        {
            return IndexOf((object)value);
        }

        /// <summary>
        /// Adds an item to the IList.
        /// </summary>
        /// <param name="value">The Object to add to the IList. </param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(TsCHdaResult value)
        {
            return Add((object)value);
        }
        #endregion
    }
}
