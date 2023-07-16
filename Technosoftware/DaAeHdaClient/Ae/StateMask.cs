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

namespace Technosoftware.DaAeHdaClient.Ae
{
    /// <summary>
    /// Defines masks to be used when modifying the subscription or item state.
    /// </summary>
    [Flags]
    public enum TsCAeStateMask
    {
        /// <summary>
        /// A name assigned to subscription.
        /// </summary>
        Name = 0x0001,

        /// <summary>
        /// The client assigned handle for the item or subscription.
        /// </summary>
        ClientHandle = 0x0002,

        /// <summary>
        /// Whether the subscription is active.
        /// </summary>
        Active = 0x0004,

        /// <summary>
        /// The maximum rate at which the server send event notifications.
        /// </summary>
        BufferTime = 0x0008,

        /// <summary>
        /// The requested maximum number of events that will be sent in a single callback.
        /// </summary>
        MaxSize = 0x0010,

        /// <summary>
        /// The maximum period between updates sent to the client.
        /// </summary>
        KeepAlive = 0x0020,

        /// <summary>
        /// All fields are valid.
        /// </summary>
        All = 0xFFFF
    }
}
