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
    /// Stores the state of a browse operation.
    /// </summary>
    [Serializable]
    public class TsCDaBrowsePosition : IOpcBrowsePosition
    {
        #region Fields
        private TsCDaBrowseFilters browseFilters_;
        private OpcItem itemId_;
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Saves the parameters for an incomplete browse information.
        /// </summary>
        public TsCDaBrowsePosition(OpcItem itemId, TsCDaBrowseFilters filters)
        {
            if (filters == null) throw new ArgumentNullException(nameof(filters));

            itemId_ = (OpcItem)itemId?.Clone();
            browseFilters_ = (TsCDaBrowseFilters)filters.Clone();
        }

        /// <summary>
        /// Releases unmanaged resources held by the object.
        /// </summary>
        public virtual void Dispose()
        {
            // does nothing.
        }
        #endregion

        #region Properties
        /// <summary>
        /// The item identifier of the branch being browsed.
        /// </summary>
        public OpcItem ItemID => itemId_;

        /// <summary>
        /// The filters applied during the browse operation.
        /// </summary>
        public TsCDaBrowseFilters Filters => (TsCDaBrowseFilters)browseFilters_.Clone();

        /// <summary>
        /// The maximum number of elements that may be returned in a single browse.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public int MaxElementsReturned
        {
            get => browseFilters_.MaxElementsReturned;
            set => browseFilters_.MaxElementsReturned = value;
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public virtual object Clone()
        {
            return (TsCDaBrowsePosition)MemberwiseClone();
        }
        #endregion
    }
}
