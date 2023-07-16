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
using System.Collections;
using System.Runtime.InteropServices;
using Technosoftware.DaAeHdaClient.Ae;
using Technosoftware.DaAeHdaClient.Da;
using Technosoftware.DaAeHdaClient.Hda;
using Technosoftware.OpcRcw.Comn;
#endregion

namespace Technosoftware.DaAeHdaClient.Com
{
    /// <summary>
    /// A unique identifier for the result of an operation of an item.
    /// </summary>
    public class ServerEnumerator : IOpcDiscovery
    {
        //======================================================================
        // IDisposable

        /// <summary>
        /// Frees all unmanaged resources
        /// </summary>
        public void Dispose() { }

        //======================================================================
        // IDiscovery

        /// <summary>
        /// Enumerates hosts that may be accessed for server discovery.
        /// </summary>
        public string[] EnumerateHosts()
        {
            return Interop.EnumComputers();
        }

        /// <summary>
        /// Returns a list of servers that support the specified interface specification.
        /// </summary>
        public OpcServer[] GetAvailableServers(OpcSpecification specification)
        {
            return GetAvailableServers(specification, null, null);
        }

        /// <summary>
        /// Returns a list of servers that support the specified specification on the specified host.
        /// </summary>
        public OpcServer[] GetAvailableServers(OpcSpecification specification, string host, OpcConnectData connectData)
        {
            lock (this)
            {
                var credentials = (connectData != null) ? connectData.GetCredential(null, null) : null;

                // connect to the server.				
                m_server = (IOPCServerList2)Interop.CreateInstance(CLSID, host, credentials, connectData?.UseConnectSecurity ?? false);
                m_host = host;

                try
                {
                    var servers = new ArrayList();

                    // convert the interface version to a guid.
                    var catid = new Guid(specification.Id);

                    // get list of servers in the specified specification.
                    IOPCEnumGUID enumerator = null;

                    m_server.EnumClassesOfCategories(
                        1,
                        new Guid[] { catid },
                        0,
                        null,
                        out enumerator);

                    // read clsids.
                    var clsids = ReadClasses(enumerator);

                    // release enumerator object.					
                    Interop.ReleaseServer(enumerator);
                    enumerator = null;

                    // fetch class descriptions.
                    foreach (var clsid in clsids)
                    {
                        var factory = new Factory();

                        try
                        {
                            var url = CreateUrl(specification, clsid);

                            OpcServer server = null;

                            if (specification == OpcSpecification.OPC_DA_30)
                            {
                                server = new TsCDaServer(factory, url);
                            }

                            else if (specification == OpcSpecification.OPC_DA_20)
                            {
                                server = new TsCDaServer(factory, url);
                            }

                            else if (specification == OpcSpecification.OPC_AE_10)
                            {
                                server = new TsCAeServer(factory, url);
                            }

                            else if (specification == OpcSpecification.OPC_HDA_10)
                            {
                                server = new TsCHdaServer(factory, url);
                            }

                            servers.Add(server);
                        }
                        catch (Exception)
                        {
                            // ignore bad clsids.
                        }
                    }

                    return (OpcServer[])servers.ToArray(typeof(OpcServer));
                }
                finally
                {
                    // free the server.
                    Interop.ReleaseServer(m_server);
                    m_server = null;
                }
            }
        }

        /// <summary>
        /// Looks up the CLSID for the specified prog id on a remote host.
        /// </summary>
        public Guid CLSIDFromProgID(string progID, string host, OpcConnectData connectData)
        {
            lock (this)
            {
                var credentials = (connectData != null) ? connectData.GetCredential(null, null) : null;

                // connect to the server.		
                m_server = (IOPCServerList2)Interop.CreateInstance(CLSID, host, credentials, connectData?.UseConnectSecurity ?? false);
                m_host = host;

                // lookup prog id.
                Guid clsid;

                try
                {
                    m_server.CLSIDFromProgID(progID, out clsid);
                }
                catch
                {
                    clsid = Guid.Empty;
                }
                finally
                {
                    Interop.ReleaseServer(m_server);
                    m_server = null;
                }

                // return empty guid if prog id not found.
                return clsid;
            }
        }

        //======================================================================
        // Private Members

        /// <summary>
        /// The server enumerator COM server.
        /// </summary>
        private IOPCServerList2 m_server = null;

        /// <summary>
        /// The host where the servers are being enumerated.
        /// </summary>
        private string m_host = null;

        /// <summary>
        /// The ProgID for the OPC Server Enumerator.
        /// </summary>
        private const string ProgID = "OPC.ServerList.1";

        /// <summary>
        /// The CLSID for the OPC Server Enumerator.
        /// </summary>
        private static readonly Guid CLSID = new Guid("13486D51-4821-11D2-A494-3CB306C10000");

        //======================================================================
        // Private Methods

        /// <summary>
        /// Reads the guids from the enumerator.
        /// </summary>
        private Guid[] ReadClasses(IOPCEnumGUID enumerator)
        {
            var guids = new ArrayList();
            var count = 10;

            // create buffer.
            var buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(Guid)) * count);

            try
            {

                int fetched;
                do
                {
                    try
                    {
                        enumerator.Next(count, buffer, out fetched);

                        var pPos = buffer;

                        for (var ii = 0; ii < fetched; ii++)
                        {
                            var guid = (Guid)Marshal.PtrToStructure(pPos, typeof(Guid));
                            guids.Add(guid);
                            pPos = (IntPtr)(pPos.ToInt64() + Marshal.SizeOf(typeof(Guid)));
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
                while (fetched > 0);

                return (Guid[])guids.ToArray(typeof(Guid));
            }
            finally
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }

        /// <summary>
        /// Reads the server details from the enumerator.
        /// </summary>
        OpcUrl CreateUrl(OpcSpecification specification, Guid clsid)
        {
            // initialize the server url.
            var url = new OpcUrl();

            url.HostName = m_host;
            url.Port = 0;
            url.Path = null;

            if (specification == OpcSpecification.OPC_DA_30) { url.Scheme = OpcUrlScheme.DA; }
            else if (specification == OpcSpecification.OPC_DA_20) { url.Scheme = OpcUrlScheme.DA; }
            else if (specification == OpcSpecification.OPC_DA_10) { url.Scheme = OpcUrlScheme.DA; }
            else if (specification == OpcSpecification.OPC_AE_10) { url.Scheme = OpcUrlScheme.AE; }
            else if (specification == OpcSpecification.OPC_HDA_10) { url.Scheme = OpcUrlScheme.HDA; }

            try
            {
                // fetch class details from the enumerator.
                string progID = null;
                string description = null;
                string verIndProgID = null;

                m_server.GetClassDetails(
                    ref clsid,
                    out progID,
                    out description,
                    out verIndProgID);

                // create the server URL path.
                if (verIndProgID != null)
                {
                    url.Path = string.Format("{0}/{1}", verIndProgID, "{" + clsid.ToString() + "}");
                }
                else if (progID != null)
                {
                    url.Path = string.Format("{0}/{1}", progID, "{" + clsid.ToString() + "}");
                }
            }
            catch (Exception)
            {
                // bad value in registry.
            }
            finally
            {
                // default to the clsid if the prog is not known.
                if (url.Path == null)
                {
                    url.Path = string.Format("{0}", "{" + clsid.ToString() + "}");
                }
            }

            // return the server url.
            return url;
        }
    }
}
