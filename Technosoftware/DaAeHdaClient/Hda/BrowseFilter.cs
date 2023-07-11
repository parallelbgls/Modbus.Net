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

namespace Technosoftware.DaAeHdaClient.Hda
{
    /// <summary>
    /// Defines a filter to apply to an item attribute when browsing.
    /// </summary>
    [Serializable]
    public class TsCHdaBrowseFilter : ICloneable
    {
        #region Fields
        private TsCHdaOperator filterOperator_ = TsCHdaOperator.Equal;
        #endregion

        #region Properties
        /// <summary>
        /// The attribute id to use when filtering.
        /// </summary>
        public int AttributeID { get; set; }

        /// <summary>
        /// The operator to use when testing if the filter condition is met.
        /// </summary>
        public TsCHdaOperator Operator
        {
            get => filterOperator_;
            set => filterOperator_ = value;
        }

        /// <summary>
        /// The value of the filter. The '*' and '?' wildcard characters are permitted. 
        /// </summary>
        public object FilterValue { get; set; }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep-copy of the object.
        /// </summary>
        public virtual object Clone()
        {
            var filter = (TsCHdaBrowseFilter)MemberwiseClone();
            filter.FilterValue = OpcConvert.Clone(FilterValue);
            return filter;
        }
        #endregion
    }
}
