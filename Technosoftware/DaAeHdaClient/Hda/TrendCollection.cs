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
    /// A collection of items.
    /// </summary>
    [Serializable]
    public class TsCHdaTrendCollection : ICloneable, IList
    {
        #region Fields
        private ArrayList trends_ = new ArrayList();
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes object with the default values.
        /// </summary>
        public TsCHdaTrendCollection() { }

        /// <summary>
        /// Initializes object with the specified TrendValueCollection object.
        /// </summary>
        public TsCHdaTrendCollection(TsCHdaTrendCollection items)
        {
            if (items != null)
            {
                foreach (TsCHdaTrend item in items)
                {
                    Add(item);
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the trend at the specified index.
        /// </summary>
        public TsCHdaTrend this[int index] => (TsCHdaTrend)trends_[index];

        /// <summary>
		/// Gets the first trend with the specified name.
		/// </summary>
		public TsCHdaTrend this[string name]
        {
            get
            {
                foreach (TsCHdaTrend trend in trends_)
                {
                    if (trend.Name == name)
                    {
                        return trend;
                    }
                }

                return null;
            }
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public virtual object Clone()
        {
            var clone = (TsCHdaTrendCollection)MemberwiseClone();

            clone.trends_ = new ArrayList();

            foreach (TsCHdaTrend trend in trends_)
            {
                clone.trends_.Add(trend.Clone());
            }

            return clone;
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
		public int Count => trends_?.Count ?? 0;

        /// <summary>
		/// Copies the objects to an Array, starting at a the specified index.
		/// </summary>
		/// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
		/// <param name="index">The zero-based index in the Array at which copying begins.</param>
		public void CopyTo(Array array, int index)
        {
            trends_?.CopyTo(array, index);
        }

        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public void CopyTo(TsCHdaTrend[] array, int index)
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
            return trends_.GetEnumerator();
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
            get => trends_[index];

            set
            {
                if (!(value is TsCHdaTrend))
                {
                    throw new ArgumentException("May only add Trend objects into the collection.");
                }

                trends_[index] = value;
            }
        }

        /// <summary>
        /// Removes the IList item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            trends_.RemoveAt(index);
        }

        /// <summary>
        /// Inserts an item to the IList at the specified position.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The Object to insert into the IList. </param>
        public void Insert(int index, object value)
        {
            if (!(value is TsCHdaTrend))
            {
                throw new ArgumentException("May only add Trend objects into the collection.");
            }

            trends_.Insert(index, value);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the IList.
        /// </summary>
        /// <param name="value">The Object to remove from the IList.</param>
        public void Remove(object value)
        {
            trends_.Remove(value);
        }

        /// <summary>
        /// Determines whether the IList contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>true if the Object is found in the IList; otherwise, false.</returns>
        public bool Contains(object value)
        {
            return trends_.Contains(value);
        }

        /// <summary>
        /// Removes all items from the IList.
        /// </summary>
        public void Clear()
        {
            trends_.Clear();
        }

        /// <summary>
        /// Determines the index of a specific item in the IList.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        public int IndexOf(object value)
        {
            return trends_.IndexOf(value);
        }

        /// <summary>
        /// Adds an item to the IList.
        /// </summary>
        /// <param name="value">The Object to add to the IList. </param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(object value)
        {
            if (!(value is TsCHdaTrend))
            {
                throw new ArgumentException("May only add Trend objects into the collection.");
            }

            return trends_.Add(value);
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
		public void Insert(int index, TsCHdaTrend value)
        {
            Insert(index, (object)value);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the IList.
        /// </summary>
        /// <param name="value">The Object to remove from the IList.</param>
        public void Remove(TsCHdaTrend value)
        {
            Remove((object)value);
        }

        /// <summary>
        /// Determines whether the IList contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>true if the Object is found in the IList; otherwise, false.</returns>
        public bool Contains(TsCHdaTrend value)
        {
            return Contains((object)value);
        }

        /// <summary>
        /// Determines the index of a specific item in the IList.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        public int IndexOf(TsCHdaTrend value)
        {
            return IndexOf((object)value);
        }

        /// <summary>
        /// Adds an item to the IList.
        /// </summary>
        /// <param name="value">The Object to add to the IList. </param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(TsCHdaTrend value)
        {
            return Add((object)value);
        }
        #endregion
    }
}
