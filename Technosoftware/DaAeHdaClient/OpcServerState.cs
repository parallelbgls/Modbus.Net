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

namespace Technosoftware.DaAeHdaClient
{
	/// <summary>
	/// The set of possible server states.
	/// </summary>
	public enum OpcServerState
	{
		/// <summary>
		/// The server state is not known.
		/// </summary>
		Unknown,

		/// <summary>
		/// The server is running normally. This is the usual state for a server 
		/// </summary>
		Operational,

		/// <summary>
        /// The server is not operational due to a fault. The server is no longer functioning. The recovery procedure from this situation is vendor specific. An error code of E_FAIL should generally be returned from any other server method.
		/// </summary>
        Faulted,

		/// <summary>
		/// The server is running but has no configuration information loaded and thus cannot function normally. Note this state implies that the server needs configuration information in order to function. Servers which do not require configuration information should not return this state.
		/// </summary>
        NeedsConfiguration,

		/// <summary>
		/// The server has been temporarily suspended via some vendor specific method and is not getting or sending data. Note that Quality will be returned as OPC_QUALITY_OUT_OF_SERVICE.
		/// </summary>
        OutOfService,

		/// <summary>
        /// The server is in Diagnostics Mode. The outputs are disconnected from the real hardware but the server will otherwise behave normally. Inputs may be real or may be simulated depending on the vendor implementation. Quality will generally be returned normally.
		/// </summary>
        Diagnostics,

        /// <summary>
        /// The server is not operational. The outputs are disconnected from the real hardware but the server will otherwise behave normally. Inputs may be real or may be simulated depending on the vendor implementation. Quality will generally be returned normally.
        /// </summary>
        NotConnected,

        /// <summary>
        /// The server is not operational because it is starting up.
        /// </summary>
        Initializing,

        /// <summary>
        /// The server is operational but it is shutting down and aborting all of its client contexts.
        /// </summary>
        Aborting,

        /// <summary>
        /// The server is not operational, but the reason is not known.
        /// </summary>
        NotOperational,
	}
}
