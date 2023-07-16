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
#endregion

namespace Technosoftware.DaAeHdaClient.Da
{
    /// <summary>
    /// Contains a description of an element in the server address space.
    /// </summary>
    [Serializable]
    public class TsCDaBrowseElement : ICloneable
    {
        #region Fields
        private TsCDaItemProperty[] itemProperties_ = new TsCDaItemProperty[0];
        #endregion

        #region Properties
        /// <summary>
        /// A descriptive name for element that is unique within a branch.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The primary identifier for the element within the server namespace.
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// An secondary identifier for the element within the server namespace.
        /// </summary>
        public string ItemPath { get; set; }

        /// <summary>
        /// Whether the element refers to an item with data that can be accessed.
        /// </summary>
        public bool IsItem { get; set; }

        /// <summary>
        /// Whether the element has children.
        /// </summary>
        public bool HasChildren { get; set; }

        /// <summary>
        /// The set of properties for the element.
        /// </summary>
        public TsCDaItemProperty[] Properties
        {
            get => itemProperties_;
            set => itemProperties_ = value;
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public virtual object Clone()
        {
            var clone = (TsCDaBrowseElement)MemberwiseClone();
            clone.itemProperties_ = (TsCDaItemProperty[])OpcConvert.Clone(itemProperties_);
            return clone;
        }
        #endregion
    };
}
