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
    /// Filters applied by the server before returning item results.
    /// </summary>
    [Flags]
    public enum TsCDaResultFilter
    {
        /// <summary>
        /// Include the ItemName in the ItemIdentifier if bit is set.
        /// </summary>
        ItemName = 0x01,

        /// <summary>
        /// Include the ItemPath in the ItemIdentifier if bit is set.
        /// </summary>
        ItemPath = 0x02,

        /// <summary>
        /// Include the ClientHandle in the ItemIdentifier if bit is set.
        /// </summary>
        ClientHandle = 0x04,

        /// <summary>
        /// Include the Timestamp in the ItemValue if bit is set.
        /// </summary>
        ItemTime = 0x08,

        /// <summary>
        /// Include verbose, localized error text with result if bit is set. 
        /// </summary>
        ErrorText = 0x10,

        /// <summary>
        /// Include additional diagnostic information with result if bit is set.
        /// </summary>
        DiagnosticInfo = 0x20,

        /// <summary>
        /// Include the ItemName and Timestamp if bit is set.
        /// </summary>
        Minimal = ItemName | ItemTime,

        /// <summary>
        /// Include all information in the results if bit is set.
        /// </summary>
        All = 0x3F
    }
}
