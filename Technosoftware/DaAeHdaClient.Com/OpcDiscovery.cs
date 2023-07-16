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
using System.Collections.Generic;
#endregion

namespace Technosoftware.DaAeHdaClient.Com
{
    /// <summary>Provides methods for discover (search) of OPC Servers.</summary>
	public class OpcDiscovery
    {
        #region Fields
        private static ServerEnumerator discovery_;
        private static string hostName_;
        private bool disposed_;
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// The finalizer implementation.
        /// </summary>
        ~OpcDiscovery()
        {
            Dispose(false);
        }

        /// <summary>
        /// Implements <see cref="IDisposable.Dispose"/>.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">If true managed and unmanaged resources can be disposed. If false only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposed_)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    discovery_?.Dispose();
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.
            }
            disposed_ = true;
        }
        #endregion

        #region Public Methods (Host related)
        /// <summary>
        /// Returns a list of host names which could contain OPC servers.
        /// </summary>
        /// <returns>List of available network host names.</returns>
        public static List<string> GetHostNames()
        {
            return ComUtils.EnumComputers();
        }
        #endregion

        #region Public Methods (Returns a list of OpcServer)
        /// <summary>
        /// Returns a list of servers that support the specified specification.
        /// </summary>
        /// <param name="specification">Unique identifier for one OPC specification.</param>
        /// <returns>Returns a list of found OPC servers.</returns>
        public static List<OpcServer> GetServers(OpcSpecification specification)
        {
            var identity = new OpcUserIdentity("", "");
            return GetServers(specification, null, identity);
        }

        /// <summary>
        /// Returns a list of servers that support the specified specification.
        /// </summary>
        /// <param name="specification">Unique identifier for one OPC specification.</param>
        /// <param name="discoveryServerUrl">The URL of the discovery server to be used.</param>
        /// <returns>Returns a list of found OPC servers.</returns>
        public static List<OpcServer> GetServers(OpcSpecification specification, string discoveryServerUrl)
        {
            var identity = new OpcUserIdentity("", "");
            return GetServers(specification, discoveryServerUrl, identity);
        }

        /// <summary>
        /// Returns a list of servers that support the specified specification.
        /// </summary>
        /// <param name="specification">Unique identifier for one OPC specification.</param>
        /// <param name="discoveryServerUrl">The URL of the discovery server to be used.</param>
        /// <param name="identity">The user identity to use when discovering the servers.</param>
        /// <returns>Returns a list of found OPC servers.</returns>
        public static List<OpcServer> GetServers(OpcSpecification specification, string discoveryServerUrl, OpcUserIdentity identity)
        {
            var serverList = new List<OpcServer>();

            var discovery = specification == OpcSpecification.OPC_AE_10 || (specification == OpcSpecification.OPC_DA_20 ||
                specification == OpcSpecification.OPC_DA_30) || specification == OpcSpecification.OPC_HDA_10;

            if (discovery)
            {
                if (discovery_ == null || hostName_ != discoveryServerUrl)
                {
                    discovery_?.Dispose();
                    hostName_ = discoveryServerUrl;
                    discovery_ = new ServerEnumerator();
                }

                var servers = discovery_.GetAvailableServers(specification);

                if (servers != null)
                {
                    foreach (var server in servers)
                    {
                        serverList.Add(server);
                    }
                }
            }

            return serverList;
        }
        #endregion

        #region Public Methods (Returns OpcServer object for a specific URL)
        /// <summary>
        /// Creates a server object for the specified URL.
        /// </summary>
        /// <param name="url">The OpcUrl of the OPC server.</param>
        /// <returns>The OpcServer object.</returns>
        public static OpcServer GetServer(OpcUrl url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            OpcServer server = null;

            // create an unconnected server object for COM based servers.

            // DA
            if (string.CompareOrdinal(url.Scheme, OpcUrlScheme.DA) == 0)
            {
                server = new Technosoftware.DaAeHdaClient.Da.TsCDaServer(new Factory(), url);
            }

            // AE
            else if (string.CompareOrdinal(url.Scheme, OpcUrlScheme.AE) == 0)
            {
                server = new Technosoftware.DaAeHdaClient.Ae.TsCAeServer(new Factory(), url);
            }

            // HDA
            else if (string.CompareOrdinal(url.Scheme, OpcUrlScheme.HDA) == 0)
            {
                server = new Technosoftware.DaAeHdaClient.Hda.TsCHdaServer(new Factory(), url);
            }

            // Other specifications not supported yet.
            if (server == null)
            {
                throw new NotSupportedException(url.Scheme);
            }

            return server;
        }
        #endregion

    }
}
