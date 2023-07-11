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

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// A writeable collection class which can be used to expose arrays as properties of classes.
    /// </summary>
    [Serializable]
    public class OpcWriteableCollection : ICollection, IList, ICloneable, ISerializable
    {
        #region Fields
        private ArrayList _array;
        private Type _elementType;
        #endregion

        #region Public Interface
        /// <summary>
        /// An indexer for the collection.
        /// </summary>
        public virtual object this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        /// <summary>
        /// Returns a copy of the collection as an array.
        /// </summary>
        public virtual Array ToArray()
        {
            return _array.ToArray(_elementType);
        }

        /// <summary>
        /// Adds a list of values to the collection.
        /// </summary>
        public virtual void AddRange(ICollection collection)
        {
            if (collection != null)
            {
                foreach (var element in collection)
                {
                    ValidateElement(element);
                }

                _array.AddRange(collection);
            }
        }
        #endregion

        #region Protected Interface
        /// <summary>
        /// Creates a collection that wraps the specified array instance.
        /// </summary>
        protected OpcWriteableCollection(ICollection array, Type elementType)
        {
            // copy array.
            if (array != null)
            {
                _array = new ArrayList(array);
            }
            else
            {
                _array = new ArrayList();
            }

            // set default element type.
            _elementType = typeof(object);

            // verify that current contents of the array are the correct type.
            if (elementType != null)
            {
                foreach (var element in _array)
                {
                    ValidateElement(element);
                }

                _elementType = elementType;
            }
        }

        /// <summary>
        /// The array instance exposed by the collection.
        /// </summary>
        protected virtual ArrayList Array
        {
            get => _array;

            set
            {
                _array = value;

                if (_array == null)
                {
                    _array = new ArrayList();
                }
            }
        }

        /// <summary>
        /// The type of objects allowed in the collection.
        /// </summary>
        protected virtual Type ElementType
        {
            get => _elementType;

            set
            {
                // verify that current contents of the array are the correct type.
                foreach (var element in _array)
                {
                    ValidateElement(element);
                }

                _elementType = value;
            }
        }

        /// <summary>
        /// Throws an exception if the element is not valid for the collection.
        /// </summary>
        protected virtual void ValidateElement(object element)
        {
            if (element == null)
            {
                throw new ArgumentException(string.Format(INVALID_VALUE, element));
            }

            if (!_elementType.IsInstanceOfType(element))
            {
                throw new ArgumentException(string.Format(INVALID_TYPE, element.GetType()));
            }
        }

        /// <remarks/>
        protected const string INVALID_VALUE = "The value '{0}' cannot be added to the collection.";
        /// <remarks/>
        protected const string INVALID_TYPE = "A value with type '{0}' cannot be added to the collection.";
        #endregion

        #region ISerializable Members
        /// <summary>
        /// A set of names for fields used in serialization.
        /// </summary>
        private class Names
        {
            internal const string COUNT = "CT";
            internal const string ELEMENT = "EL";
            internal const string ELEMENT_TYPE = "ET";
        }

        /// <summary>
        /// Contructs a server by de-serializing its OpcUrl from the stream.
        /// </summary>
        protected OpcWriteableCollection(SerializationInfo info, StreamingContext context)
        {
            _elementType = (Type)info.GetValue(Names.ELEMENT_TYPE, typeof(Type));

            var count = (int)info.GetValue(Names.COUNT, typeof(int));

            _array = new ArrayList(count);

            for (var ii = 0; ii < count; ii++)
            {
                _array.Add(info.GetValue(Names.ELEMENT + ii.ToString(), typeof(object)));
            }
        }

        /// <summary>
        /// Serializes a server into a stream.
        /// </summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(Names.ELEMENT_TYPE, _elementType);
            info.AddValue(Names.COUNT, _array.Count);

            for (var ii = 0; ii < _array.Count; ii++)
            {
                info.AddValue(Names.ELEMENT + ii.ToString(), _array[ii]);
            }
        }
        #endregion

        #region ICollection Members
        /// <summary>
        /// Indicates whether access to the ICollection is synchronized (thread-safe).
        /// </summary>
        public virtual bool IsSynchronized => false;

        /// <summary>
        /// Gets the number of objects in the collection.
        /// </summary>
        public virtual int Count => _array.Count;

        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public virtual void CopyTo(Array array, int index)
        {
            if (_array != null)
            {
                _array.CopyTo(array, index);
            }
        }

        /// <summary>
        /// Indicates whether access to the ICollection is synchronized (thread-safe).
        /// </summary>
        public virtual object SyncRoot => this;

        #endregion

        #region IEnumerable Members
        /// <summary>
        /// Returns an enumerator that can iterate through a collection.
        /// </summary>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return _array.GetEnumerator();
        }
        #endregion

        #region IList Members
        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public virtual bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = value;
        }

        /// <summary>
        /// Removes the IList item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public virtual void RemoveAt(int index)
        {
            _array.RemoveAt(index);
        }

        /// <summary>
        /// Inserts an item to the IList at the specified position.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The Object to insert into the IList. </param>
        public virtual void Insert(int index, object value)
        {
            ValidateElement(value);
            _array.Insert(index, value);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the IList.
        /// </summary>
        /// <param name="value">The Object to remove from the IList.</param>
        public virtual void Remove(object value)
        {
            _array.Remove(value);
        }

        /// <summary>
        /// Determines whether the IList contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>true if the Object is found in the IList; otherwise, false.</returns>
        public virtual bool Contains(object value)
        {
            return _array.Contains(value);
        }

        /// <summary>
        /// Removes all items from the IList.
        /// </summary>
        public virtual void Clear()
        {
            _array.Clear();
        }

        /// <summary>
        /// Determines the index of a specific item in the IList.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        public virtual int IndexOf(object value)
        {
            return _array.IndexOf(value);
        }

        /// <summary>
        /// Adds an item to the IList.
        /// </summary>
        /// <param name="value">The Object to add to the IList. </param>
        /// <returns>The position into which the new element was inserted.</returns>
        public virtual int Add(object value)
        {
            ValidateElement(value);
            return _array.Add(value);
        }

        /// <summary>
        /// Indicates whether the IList has a fixed size.
        /// </summary>
        public virtual bool IsFixedSize => false;

        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the collection.
        /// </summary>
        public virtual object Clone()
        {
            var clone = (OpcWriteableCollection)MemberwiseClone();

            clone._array = new ArrayList();

            for (var ii = 0; ii < _array.Count; ii++)
            {
                clone.Add(OpcConvert.Clone(_array[ii]));
            }

            return clone;
        }
        #endregion
    }
}
