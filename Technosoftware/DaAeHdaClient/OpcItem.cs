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
using System.Text;
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// A unique item identifier.
    /// </summary>
    [Serializable]
    public class OpcItem : ICloneable
    {
        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes the object with default values.
        /// </summary>
        public OpcItem() { }

        /// <summary>
        /// Initializes the object with the specified item name.
        /// </summary>
        public OpcItem(string itemName)
        {
            ItemPath = null;
            ItemName = itemName;
        }

        /// <summary>
        /// Initializes the object with the specified item path and item name.
        /// </summary>
        public OpcItem(string itemPath, string itemName)
        {
            ItemPath = itemPath;
            ItemName = itemName;
        }

        /// <summary>
        /// Initializes the object with the specified item identifier.
        /// </summary>
        public OpcItem(OpcItem itemId)
        {
            if (itemId != null)
            {
                ItemPath = itemId.ItemPath;
                ItemName = itemId.ItemName;
                ClientHandle = itemId.ClientHandle;
                ServerHandle = itemId.ServerHandle;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// The primary identifier for an item within the server namespace.
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// An secondary identifier for an item within the server namespace.
        /// </summary>
        public string ItemPath { get; set; }

        /// <summary>
        /// A unique item identifier assigned by the client.
        /// </summary>
        public object ClientHandle { get; set; }

        /// <summary>
        /// A unique item identifier assigned by the server.
        /// </summary>
        public object ServerHandle { get; set; }

        /// <summary>
        /// Create a string that can be used as index in a hash table for the item.
        /// </summary>
        public string Key =>
            new StringBuilder(64)
                .Append(ItemName ?? "null")
                .Append(Environment.NewLine)
                .Append(ItemPath ?? "null")
                .ToString();
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a shallow copy of the object.
        /// </summary>
        public virtual object Clone() { return MemberwiseClone(); }
        #endregion
    }
}
