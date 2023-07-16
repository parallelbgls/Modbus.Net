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
    /// Defines masks to be used when modifying the subscription or item state.
    /// </summary>
    [Flags]
    public enum TsCDaStateMask
    {
        /// <summary>
        /// The name of the subscription.
        /// </summary>
        Name = 0x0001,

        /// <summary>
        /// The client assigned handle for the item or subscription.
        /// </summary>
        ClientHandle = 0x0002,

        /// <summary>
        /// The locale to use for results returned to the client from the subscription.
        /// </summary>
        Locale = 0x0004,

        /// <summary>
        /// Whether the item or subscription is active.
        /// </summary>
        Active = 0x0008,

        /// <summary>
        /// The maximum rate at which data update notifications are sent.
        /// </summary>
        UpdateRate = 0x0010,

        /// <summary>
        /// The longest period between data update notifications.<br/>
        /// <strong>Note:</strong> This feature is only supported with OPC Data Access 3.0
        /// Servers.
        /// </summary>
        KeepAlive = 0x0020,

        /// <summary>
        /// The requested data type for the item.
        /// </summary>
        ReqType = 0x0040,

        /// <summary>
        /// The deadband for the item or subscription.
        /// </summary>
        Deadband = 0x0080,

        /// <summary>
        /// The rate at which the server should check for changes to an item value.
        /// </summary>
        SamplingRate = 0x0100,

        /// <summary>
        /// Whether the server should buffer multiple changes to a single item.
        /// </summary>
        EnableBuffering = 0x0200,

        /// <summary>
        /// All fields are valid.
        /// </summary>
        All = 0xFFFF
    }
}
