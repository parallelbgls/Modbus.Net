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
    /// A read only collection class which can be used to expose arrays as properties of classes.
    /// </summary>
    [Serializable]
    public class OpcReadOnlyCollection : ICollection, ICloneable, ISerializable
    {
        #region Fields
        private Array array_;
        #endregion

        #region Public Methods
        /// <summary>
        /// An indexer for the collection.
        /// </summary>
        public virtual object this[int index] => array_.GetValue(index);

        /// <summary>
        /// Returns a copy of the collection as an array.
        /// </summary>
        public virtual Array ToArray()
        {
            return (Array)OpcConvert.Clone(array_);
        }
        #endregion

        #region Protected Interface
        /// <summary>
        ///Creates a collection that wraps the specified array instance.
        /// </summary>
		protected OpcReadOnlyCollection(Array array)
        {
            Array = array;
        }

        /// <summary>
        /// The array instance exposed by the collection.
        /// </summary>
        protected virtual Array Array
        {
            get => array_;

            set
            {
                array_ = value;

                if (array_ == null)
                {
                    array_ = new object[0];
                }
            }
        }
        #endregion

        #region ISerializable Members
        /// <summary>
        /// A set of names for fields used in serialization.
        /// </summary>
        private class Names
        {
            internal const string Array = "AR";
        }

        /// <summary>
        /// Construct a server by de-serializing its OpcUrl from the stream.
        /// </summary>
        protected OpcReadOnlyCollection(SerializationInfo info, StreamingContext context)
        {
            array_ = (Array)info.GetValue(Names.Array, typeof(Array));
        }

        /// <summary>
        /// Serializes a server into a stream.
        /// </summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(Names.Array, array_);
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
        public virtual int Count => array_.Length;

        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public virtual void CopyTo(Array array, int index)
        {
            if (array_ != null)
            {
                array_.CopyTo(array, index);
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
        public virtual IEnumerator GetEnumerator()
        {
            return array_.GetEnumerator();
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the collection.
        /// </summary>
        public virtual object Clone()
        {
            var clone = (OpcReadOnlyCollection)MemberwiseClone();

            var array = new ArrayList(array_.Length);

            // clone the elements and determine the element type.
            Type elementType = null;

            for (var ii = 0; ii < array_.Length; ii++)
            {
                var element = array_.GetValue(ii);

                if (elementType == null)
                {
                    elementType = element.GetType();
                }
                else if (elementType != typeof(object))
                {
                    while (!(elementType is null) && !elementType.IsInstanceOfType(element))
                    {
                        elementType = elementType.BaseType;
                    }
                }

                array.Add(OpcConvert.Clone(element));
            }

            // convert array list to an array.
            clone.Array = array.ToArray(elementType);

            return clone;
        }
        #endregion
    }
}
