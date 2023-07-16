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
    /// <summary>
    /// A list of properties for a single item.
    /// </summary>
    [Serializable]
    public class TsCDaItemPropertyCollection : ArrayList, IOpcResult
    {
        #region Fields
        private OpcResult result_ = OpcResult.S_OK;
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes the object with its default values.
        /// </summary>
        public TsCDaItemPropertyCollection()
        {
        }

        /// <summary>
        /// Initializes the object with the specified item identifier.
        /// </summary>
        public TsCDaItemPropertyCollection(OpcItem itemId)
        {
            if (itemId != null)
            {
                ItemName = itemId.ItemName;
                ItemPath = itemId.ItemPath;
            }
        }

        /// <summary>
        /// Initializes the object with the specified item identifier and result.
        /// </summary>
        public TsCDaItemPropertyCollection(OpcItem itemId, OpcResult result)
        {
            if (itemId != null)
            {
                ItemName = itemId.ItemName;
                ItemPath = itemId.ItemPath;
            }

            result_ = result;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The primary identifier for the item within the server namespace.
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// An secondary identifier for the item within the server namespace.
        /// </summary>
        public string ItemPath { get; set; }

        /// <summary>
        /// Accesses the items at the specified index.
        /// </summary>
        public new TsCDaItemProperty this[int index]
        {
            get => (TsCDaItemProperty)base[index];
            set => base[index] = value;
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

        #region ICollection Members
        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public void CopyTo(TsCDaItemProperty[] array, int index)
        {
            CopyTo((Array)array, index);
        }
        #endregion

        #region IList Members
        /// <summary>
        /// Inserts an item to the IList at the specified position.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The Object to insert into the IList. </param>
        public void Insert(int index, TsCDaItemProperty value)
        {
            Insert(index, (object)value);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the IList.
        /// </summary>
        /// <param name="value">The Object to remove from the IList.</param>
        public void Remove(TsCDaItemProperty value)
        {
            Remove((object)value);
        }

        /// <summary>
        /// Determines whether the IList contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>true if the Object is found in the IList; otherwise, false.</returns>
        public bool Contains(TsCDaItemProperty value)
        {
            return Contains((object)value);
        }

        /// <summary>
        /// Determines the index of a specific item in the IList.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        public int IndexOf(TsCDaItemProperty value)
        {
            return IndexOf((object)value);
        }

        /// <summary>
        /// Adds an item to the IList.
        /// </summary>
        /// <param name="value">The Object to add to the IList. </param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(TsCDaItemProperty value)
        {
            return Add((object)value);
        }
        #endregion
    }
}
