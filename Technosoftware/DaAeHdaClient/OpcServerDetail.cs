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

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// Detailed information about the server.
    /// Set to null if the ServerDescription is being accessed without a client context.
    /// </summary>
    public class OpcServerDetail
    {
        /// <summary>
        /// The time the server was last started.
        /// </summary>
        public DateTime StartTime;

        /// <summary>
        /// The build number of the server.
        /// </summary>
        public string BuildNumber;

        /// <summary>
        /// The version of the server.
        /// </summary>
        public string Version;

        /// <summary>
        /// Vendor-specific information about the server.
        /// </summary>
        public string VendorInfo;
    }
}
