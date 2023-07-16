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

#endregion

namespace Technosoftware.DaAeHdaClient.Hda
{
    /// <summary>
    /// The set of possible operators to use when applying an item attribute filter.
    /// </summary>
    public enum TsCHdaOperator
    {
        /// <summary>
        /// The attribute value is equal (or matches) to the filter.
        /// </summary>
        Equal = 1,

        /// <summary>
        /// The attribute value is less than the filter.
        /// </summary>
        Less,

        /// <summary>
        /// The attribute value is less than or equal to the filter.
        /// </summary>
        LessEqual,

        /// <summary>
        /// The attribute value is greater than the filter.
        /// </summary>
        Greater,

        /// <summary>
        /// The attribute value is greater than or equal to the filter.
        /// </summary>
        GreaterEqual,

        /// <summary>
        /// The attribute value is not equal (or does not match)to the filter.
        /// </summary>
        NotEqual
    }
}
