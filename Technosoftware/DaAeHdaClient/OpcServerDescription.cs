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
    /// Information about an OPC Server
    /// </summary>
	public class OpcServerDescription
    {
        /// <summary>
		/// The server types supported by this server. Standard types are defined 
		/// by the ServerType class.
		/// </summary>
        public uint ServerTypes { get; set; }

        /// <summary>
		/// <para>Name of the server software vendor.  </para> 
		/// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// <para>Namespace for types defined by this vendor.  This may or 
        /// may not be the same as the VendorName. Null or empty if not used.</para> 
        /// </summary>
        public string VendorNamespace { get; set; }

        /// <summary>
        /// Name of the server software. 
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// <para>Namespace for server-specific types. Null or empty if not used.</para> 
        /// <para>This name is typically a concatentation of the VendorNamespace 
        /// and the ServerName (separated by a '/' character) 
        /// (e.g "MyVendorNamespace/MyServer").</para>
        /// </summary>
        public string ServerNamespace { get; set; }

        /// <summary>
        /// <para>The HostName of the machine in which the server resides (runs).  The 
        /// HostName is used as part of the object path in InstanceIds of the 
        /// server's objects.</para> 
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
		/// <para>The name of the system that contains the objects accessible 
		/// through the server. Null or empty if the server provides access 
		/// to objects from more than one system. </para> 
		/// </summary>
        public string SystemName { get; set; }

        /// <summary>
		/// Detailed information about the server.
		/// Set to null if the ServerDescription is being 
		/// accessed without a client context.
		/// </summary>
        public OpcServerDetail ServerDetails { get; set; }
    }
}
