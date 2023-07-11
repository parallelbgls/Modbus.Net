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
    /// A collection of attribute filters used when browsing the server address space.
    /// </summary>
    [Serializable]
    public class TsCHdaBrowseFilterCollection : OpcItem, ICollection
    {
        #region Fields
        private TsCHdaBrowseFilter[] browseFilters_ = new TsCHdaBrowseFilter[0];
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Creates an empty collection.
        /// </summary>
        public TsCHdaBrowseFilterCollection()
        {
            // do nothing.
        }

        /// <summary>
        /// Initializes the object with any BrowseFilter contained in the collection.
        /// </summary>
        /// <param name="collection">A collection containing browse filters.</param>
        public TsCHdaBrowseFilterCollection(ICollection collection)
        {
            Init(collection);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the browse filter at the specified index.
        /// </summary>
        public TsCHdaBrowseFilter this[int index]
        {
            get => browseFilters_[index];
            set => browseFilters_[index] = value;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the browse filter for the specified attribute id.
        /// </summary>
        public TsCHdaBrowseFilter Find(int id)
        {
            foreach (var filter in browseFilters_)
            {
                if (filter.AttributeID == id)
                {
                    return filter;
                }
            }

            return null;
        }

        /// <summary>
        /// Initializes the object with any attribute values contained in the collection.
        /// </summary>
        /// <param name="collection">A collection containing attribute values.</param>
        public void Init(ICollection collection)
        {
            Clear();

            if (collection != null)
            {
                var values = new ArrayList(collection.Count);

                foreach (var value in collection)
                {
                    if (value.GetType() == typeof(TsCHdaBrowseFilter))
                    {
                        values.Add(OpcConvert.Clone(value));
                    }
                }

                browseFilters_ = (TsCHdaBrowseFilter[])values.ToArray(typeof(TsCHdaBrowseFilter));
            }
        }

        /// <summary>
        /// Removes all attribute values in the collection.
        /// </summary>
        public void Clear()
        {
            browseFilters_ = new TsCHdaBrowseFilter[0];
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public override object Clone()
        {
            return new TsCHdaBrowseFilterCollection(this);
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
        public int Count => browseFilters_?.Length ?? 0;

        /// <summary>
        /// Copies the objects in to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public void CopyTo(Array array, int index)
        {
            browseFilters_?.CopyTo(array, index);
        }

        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public void CopyTo(TsCHdaBrowseFilter[] array, int index)
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
            return browseFilters_.GetEnumerator();
        }
        #endregion
    }
}
