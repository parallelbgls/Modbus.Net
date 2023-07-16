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
    /// This interface is used to discover OPC servers on the network.
    /// </summary>
    public interface IOpcDiscovery : IDisposable
    {
        /// <summary>
        /// Returns a list of host names which could contain OPC servers.
        /// </summary>
        /// <returns>A array of strings that are valid network host names.</returns>
        string[] EnumerateHosts();

        /// <summary>
        /// Returns a list of servers that support an OPC specification.
        /// </summary>
        /// <param name="specification">A unique identifier for an OPC specification.</param>
        /// <returns>An array of unconnected OPC server obejcts on the local machine.</returns>
        OpcServer[] GetAvailableServers(OpcSpecification specification);

        /// <summary>
        /// Returns a list of servers that support an OPC specification on remote machine.
        /// </summary>
        /// <param name="specification">A unique identifier for an OPC specification.</param>
        /// <param name="host">The network host name of the machine to search for servers.</param>
        /// <param name="connectData">Any necessary user authentication or protocol configuration information.</param>
        /// <returns>An array of unconnected OPC server objects.</returns>
        OpcServer[] GetAvailableServers(OpcSpecification specification, string host, OpcConnectData connectData);
    }
}
