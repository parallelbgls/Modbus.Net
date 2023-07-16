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
    /// An offset component of a relative time.
    /// </summary>
    [Serializable]
    public struct TsCHdaTimeOffset
    {
        #region Properties
        /// <summary>
        /// A signed value indicated the magnitude of the time offset.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The time interval to use when applying the offset.
        /// </summary>
        public TsCHdaRelativeTime Type { get; set; }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Converts a offset type to a string token.
        /// </summary>
        /// <param name="offsetType">The offset type value to convert.</param>
        /// <returns>The string token representing the offset type.</returns>
        internal static string OffsetTypeToString(TsCHdaRelativeTime offsetType)
        {
            switch (offsetType)
            {
                case TsCHdaRelativeTime.Second: { return "S"; }
                case TsCHdaRelativeTime.Minute: { return "M"; }
                case TsCHdaRelativeTime.Hour: { return "H"; }
                case TsCHdaRelativeTime.Day: { return "D"; }
                case TsCHdaRelativeTime.Week: { return "W"; }
                case TsCHdaRelativeTime.Month: { return "MO"; }
                case TsCHdaRelativeTime.Year: { return "Y"; }
            }

            throw new ArgumentOutOfRangeException(nameof(offsetType), offsetType.ToString(), @"Invalid value for relative time offset type.");
        }
        #endregion
    }
}
