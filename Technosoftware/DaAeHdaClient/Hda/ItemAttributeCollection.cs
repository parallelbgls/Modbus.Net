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
    /// A collection of item attribute values passed to write or returned from a read operation.
    /// </summary>
    [Serializable]
    public class TsCHdaItemAttributeCollection : OpcItem, IOpcResult, ITsCHdaActualTime, IList
    {
        #region Fields
        private DateTime startTime_ = DateTime.MinValue;
        private DateTime endTime_ = DateTime.MinValue;
        private ArrayList attributes_ = new ArrayList();
        private OpcResult result_ = OpcResult.S_OK;
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes object with the default values.
        /// </summary>
        public TsCHdaItemAttributeCollection() { }

        /// <summary>
        /// Initializes object with the specified ItemIdentifier object.
        /// </summary>
        public TsCHdaItemAttributeCollection(OpcItem item) : base(item) { }

        /// <summary>
        /// Initializes object with the specified ItemAttributeCollection object.
        /// </summary>
        public TsCHdaItemAttributeCollection(TsCHdaItemAttributeCollection item)
            : base(item)
        {
            attributes_ = new ArrayList(item.attributes_.Count);

            foreach (TsCHdaAttributeValueCollection value in item.attributes_)
            {
                if (value != null)
                {
                    attributes_.Add(value.Clone());
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accessor for elements in the collection.
        /// </summary>
        public TsCHdaAttributeValueCollection this[int index]
        {
            get => (TsCHdaAttributeValueCollection)attributes_[index];
            set => attributes_[index] = value;
        }
        #endregion

        #region IOpcResult Members
        /// <summary>
        /// The error id for the result of an operation on an item.
        /// </summary>
        public OpcResult Result
        {
            get => result_;
            set => result_ = value;
        }

        /// <summary>
        /// Vendor specific diagnostic information (not the localized error text).
        /// </summary>
        public string DiagnosticInfo { get; set; }
        #endregion

        #region IActualTime Members
        /// <summary>
        /// The actual start time used by a server while processing a request.
        /// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
        /// the time format (UTC or local time).
        /// </summary>
        public DateTime StartTime
        {
            get => startTime_;
            set => startTime_ = value;
        }

        /// <summary>
        /// The actual end time used by a server while processing a request.
        /// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
        /// the time format (UTC or local time).
        /// </summary>
        public DateTime EndTime
        {
            get => endTime_;
            set => endTime_ = value;
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public override object Clone()
        {
            var collection = (TsCHdaItemAttributeCollection)base.Clone();

            collection.attributes_ = new ArrayList(attributes_.Count);

            foreach (TsCHdaAttributeValueCollection value in attributes_)
            {
                collection.attributes_.Add(value.Clone());
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
		public int Count => attributes_?.Count ?? 0;

        /// <summary>
		/// Copies the objects to an Array, starting at a the specified index.
		/// </summary>
		/// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
		/// <param name="index">The zero-based index in the Array at which copying begins.</param>
		public void CopyTo(Array array, int index)
        {
            attributes_?.CopyTo(array, index);
        }

        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public void CopyTo(TsCHdaAttributeValueCollection[] array, int index)
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
            return attributes_.GetEnumerator();
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
            get => attributes_[index];

            set
            {
                if (!(value is TsCHdaAttributeValueCollection))
                {
                    throw new ArgumentException("May only add AttributeValueCollection objects into the collection.");
                }

                attributes_[index] = value;
            }
        }

        /// <summary>
        /// Removes the IList item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            attributes_.RemoveAt(index);
        }

        /// <summary>
        /// Inserts an item to the IList at the specified position.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The Object to insert into the IList. </param>
        public void Insert(int index, object value)
        {
            if (!(value is TsCHdaAttributeValueCollection))
            {
                throw new ArgumentException("May only add AttributeValueCollection objects into the collection.");
            }

            attributes_.Insert(index, value);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the IList.
        /// </summary>
        /// <param name="value">The Object to remove from the IList.</param>
        public void Remove(object value)
        {
            attributes_.Remove(value);
        }

        /// <summary>
        /// Determines whether the IList contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>true if the Object is found in the IList; otherwise, false.</returns>
        public bool Contains(object value)
        {
            return attributes_.Contains(value);
        }

        /// <summary>
        /// Removes all items from the IList.
        /// </summary>
        public void Clear()
        {
            attributes_.Clear();
        }

        /// <summary>
        /// Determines the index of a specific item in the IList.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        public int IndexOf(object value)
        {
            return attributes_.IndexOf(value);
        }

        /// <summary>
        /// Adds an item to the IList.
        /// </summary>
        /// <param name="value">The Object to add to the IList. </param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(object value)
        {
            if (!(value is TsCHdaAttributeValueCollection))
            {
                throw new ArgumentException("May only add AttributeValueCollection objects into the collection.");
            }

            return attributes_.Add(value);
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
		public void Insert(int index, TsCHdaAttributeValueCollection value)
        {
            Insert(index, (object)value);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the IList.
        /// </summary>
        /// <param name="value">The Object to remove from the IList.</param>
        public void Remove(TsCHdaAttributeValueCollection value)
        {
            Remove((object)value);
        }

        /// <summary>
        /// Determines whether the IList contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>true if the Object is found in the IList; otherwise, false.</returns>
        public bool Contains(TsCHdaAttributeValueCollection value)
        {
            return Contains((object)value);
        }

        /// <summary>
        /// Determines the index of a specific item in the IList.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        public int IndexOf(TsCHdaAttributeValueCollection value)
        {
            return IndexOf((object)value);
        }

        /// <summary>
        /// Adds an item to the IList.
        /// </summary>
        /// <param name="value">The Object to add to the IList. </param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(TsCHdaAttributeValueCollection value)
        {
            return Add((object)value);
        }
        #endregion
    }
}
