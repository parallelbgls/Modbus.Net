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
    /// Defines possible HDA quality codes.
    /// </summary>
    [Flags]
    public enum TsCHdaQuality
    {
        /// <summary>
        /// More than one piece of data that may be hidden exists at same timestamp.
        /// </summary>
        ExtraData = 0x00010000,

        /// <summary>
        /// Interpolated data value.
        /// </summary>
        Interpolated = 0x00020000,

        /// <summary>
        /// Raw data
        /// </summary>
        Raw = 0x00040000,

        /// <summary>
        /// Calculated data value, as would be returned from a ReadProcessed call.
        /// </summary>
        Calculated = 0x00080000,

        /// <summary>
        /// No data found to provide upper or lower bound value. 
        /// </summary>
        NoBound = 0x00100000,

        /// <summary>
        /// Bad No data collected. Archiving not active (for item or all items).
        /// </summary>
        NoData = 0x00200000,

        /// <summary>
        /// Collection started/stopped/lost.
        /// </summary>
        DataLost = 0x00400000,

        /// <summary>
        /// Scaling or conversion error. 
        /// </summary>
        Conversion = 0x00800000,

        /// <summary>
        /// Aggregate value is for an incomplete interval. 
        /// </summary>
        Partial = 0x01000000
    }
}
