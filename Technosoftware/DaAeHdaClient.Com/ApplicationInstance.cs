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

namespace Technosoftware.DaAeHdaClient.Com
{
    /// <summary>
    /// Manages the license to enable the different product versions.
    /// </summary>
    public partial class ApplicationInstance
    {
        #region Nested Enums
        /// <summary>
        /// The possible authentication levels.
        /// </summary>
        [Flags]
        public enum AuthenticationLevel : uint
        {
            /// <summary>
            /// Tells DCOM to choose the authentication level using its normal security blanket negotiation algorithm.
            /// </summary>
            Default = 0,

            /// <summary>
            /// Performs no authentication.
            /// </summary>
            None = 1,

            /// <summary>
            /// Authenticates the credentials of the client only when the client establishes a relationship with the server. Datagram transports always use Packet instead.
            /// </summary>
            Connect = 2,

            /// <summary>
            /// Authenticates only at the beginning of each remote procedure call when the server receives the request. Datagram transports use Packet instead.
            /// </summary>
            Call = 3,

            /// <summary>
            /// Authenticates that all data received is from the expected client.
            /// </summary>
            Packet = 4,

            /// <summary>
            /// Authenticates and verifies that none of the data transferred between client and server has been modified.
            /// </summary>
            Integrity = 5,

            /// <summary>
            /// Authenticates all previous levels and encrypts the argument value of each remote procedure call.
            /// </summary>
            Privacy = 6,
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initializes COM security. This should be called directly at the beginning of an application and can only be called once.
        /// </summary>
        /// <param name="authenticationLevel">The default authentication level for the process. Both servers and clients use this parameter when they call CoInitializeSecurity. With the Windows Update KB5004442 a higher authentication level of Integrity must be used.</param>
        public static void InitializeSecurity(AuthenticationLevel authenticationLevel)
        {
            if (!InitializeSecurityCalled)
            {
                Com.Interop.InitializeSecurity((uint)authenticationLevel);
                InitializeSecurityCalled = true;
            }
        }
        #endregion

        #region Internal Fields
        internal static bool InitializeSecurityCalled;
        #endregion
    }
}
