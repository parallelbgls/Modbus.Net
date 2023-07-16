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
    /// <summary>
    ///     <para>Defines the possible limit status bits.</para>
    ///     <para>The Limit Field is valid regardless of the Quality and Substatus. In some
    ///     cases such as Sensor Failure it can provide useful diagnostic information.</para>
    /// </summary>
    public enum TsDaLimitBits
    {
        /// <summary>The value is free to move up or down</summary>
        None = 0x0,
        /// <summary>The value has ‘pegged’ at some lower limit</summary>
        Low = 0x1,
        /// <summary>The value has ‘pegged’ at some high limit</summary>
        High = 0x2,
        /// <summary>The value is a constant and cannot move</summary>
        Constant = 0x3
    }
}
