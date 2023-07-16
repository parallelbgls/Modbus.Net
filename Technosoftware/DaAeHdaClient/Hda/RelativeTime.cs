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
    /// Possible base or offset types for relative times.
    /// </summary>
    public enum TsCHdaRelativeTime
    {
        /// <summary>
        /// Start from the current time.
        /// </summary>
        Now,

        /// <summary>
        /// The start of the current second or an offset in seconds.
        /// </summary>
        Second,

        /// <summary>
        /// The start of the current minutes or an offset in minutes.
        /// </summary>
        Minute,

        /// <summary>
        /// The start of the current hour or an offset in hours.
        /// </summary>
        Hour,

        /// <summary>
        /// The start of the current day or an offset in days.
        /// </summary>
        Day,

        /// <summary>
        /// The start of the current week or an offset in weeks.
        /// </summary>
        Week,

        /// <summary>
        /// The start of the current month or an offset in months.
        /// </summary>
        Month,

        /// <summary>
        /// The start of the current year or an offset in years.
        /// </summary>
        Year
    }
}
