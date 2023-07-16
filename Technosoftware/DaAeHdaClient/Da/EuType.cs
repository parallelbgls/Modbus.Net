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

namespace Technosoftware.DaAeHdaClient.Da
{
    /// <summary><para>Defines possible item engineering unit types</para></summary>
    public enum TsDaEuType
    {
        /// <summary>No engineering unit information available</summary>
        NoEnum = 0x01,
        /// <summary>
        /// Analog engineering unit - will contain a SAFE ARRAY of exactly two doubles
        /// (VT_ARRAY | VT_R8) corresponding to the LOW and HI EU range.
        /// </summary>
        Analog = 0x02,
        /// <summary>
        /// Enumerated engineering unit - will contain a SAFE ARRAY of strings (VT_ARRAY |
        /// VT_BSTR) which contains a list of strings (Example: “OPEN”, “CLOSE”, “IN TRANSIT”,
        /// etc.) corresponding to sequential numeric values (0, 1, 2, etc.)
        /// </summary>
        Enumerated = 0x03
    }
}
