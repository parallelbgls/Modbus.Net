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
using System.Xml;
using System.Net;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Collections.Generic;

#endregion

namespace Technosoftware.DaAeHdaClient.Com
{
    /// <summary>
    /// The default class used to instantiate server objects.
    /// </summary>
    [Serializable]
    public class Factory : OpcFactory
    {
        //======================================================================
        // Construction

        /// <summary>
        /// Initializes an instance for use for in process objects.
        /// </summary>
        public Factory() : base(null)
        {
            // do nothing.
        }

        //======================================================================
        // ISerializable

        /// <summary>
        /// Contructs a server by de-serializing its URL from the stream.
        /// </summary>
        protected Factory(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // do nothing.
        }

        //======================================================================
        // IOpcFactory

        /// <summary>
        /// Creates a new instance of the server.
        /// </summary>
		public override IOpcServer CreateInstance(OpcUrl url, OpcConnectData connectData)
        {
            var comServer = Factory.Connect(url, connectData);

            if (comServer == null)
            {
                return null;
            }

            Server server = null;
            Type interfaceType = null;

            try
            {
                if (string.IsNullOrEmpty(url.Scheme))
                {
                    throw new NotSupportedException(string.Format("The URL scheme '{0}' is not supported.", url.Scheme));
                }

                // DA
                else if (url.Scheme == OpcUrlScheme.DA)
                {
                    // Verify that it is a DA server.
                    if (!typeof(OpcRcw.Da.IOPCServer).IsInstanceOfType(comServer))
                    {
                        interfaceType = typeof(OpcRcw.Da.IOPCServer);
                        throw new NotSupportedException();
                    }

                    // DA 3.00
                    if (!ForceDa20Usage && typeof(OpcRcw.Da.IOPCBrowse).IsInstanceOfType(comServer) && typeof(OpcRcw.Da.IOPCItemIO).IsInstanceOfType(comServer))
                    {
                        server = new Technosoftware.DaAeHdaClient.Com.Da.Server(url, comServer);
                    }

                    // DA 2.XX
                    else if (typeof(OpcRcw.Da.IOPCItemProperties).IsInstanceOfType(comServer))
                    {
                        server = new Technosoftware.DaAeHdaClient.Com.Da20.Server(url, comServer);
                    }

                    else
                    {
                        interfaceType = typeof(OpcRcw.Da.IOPCItemProperties);
                        throw new NotSupportedException();
                    }
                }

                // AE
                else if (url.Scheme == OpcUrlScheme.AE)
                {
                    // Verify that it is a AE server.
                    if (!typeof(OpcRcw.Ae.IOPCEventServer).IsInstanceOfType(comServer))
                    {
                        interfaceType = typeof(OpcRcw.Ae.IOPCEventServer);
                        throw new NotSupportedException();
                    }

                    server = new Technosoftware.DaAeHdaClient.Com.Ae.Server(url, comServer);
                }

                // HDA
                else if (url.Scheme == OpcUrlScheme.HDA)
                {
                    // Verify that it is a HDA server.
                    if (!typeof(OpcRcw.Hda.IOPCHDA_Server).IsInstanceOfType(comServer))
                    {
                        interfaceType = typeof(OpcRcw.Hda.IOPCHDA_Server);
                        throw new NotSupportedException();
                    }

                    server = new Technosoftware.DaAeHdaClient.Com.Hda.Server(url, comServer);
                }

                // All other specifications not supported yet.
                else
                {
                    throw new NotSupportedException(string.Format("The URL scheme '{0}' is not supported.", url.Scheme));
                }
            }
            catch (NotSupportedException)
            {
                Utilities.Interop.ReleaseServer(server);
                server = null;

                if (interfaceType != null)
                {
                    var message = new StringBuilder();

                    message.AppendFormat("The COM server does not support the interface ");
                    message.AppendFormat("'{0}'.", interfaceType.FullName);
                    message.Append("\r\n\r\nThis problem could be caused by:\r\n");
                    message.Append("- incorrectly installed proxy/stubs.\r\n");
                    message.Append("- problems with the DCOM security settings.\r\n");
                    message.Append("- a personal firewall (sometimes activated by default).\r\n");

                    throw new NotSupportedException(message.ToString());
                }

                throw;
            }
            catch (Exception)
            {
                Utilities.Interop.ReleaseServer(server);
                throw;
            }

            // initialize the wrapper object.
            if (server != null)
            {
                server.Initialize(url, connectData);
            }

            SupportedSpecifications = new List<OpcSpecification>();
            if (server is Com.Da20.Server)
            {
                SupportedSpecifications.Add(OpcSpecification.OPC_DA_20);
            }
            else if (server is Com.Da.Server)
            {
                SupportedSpecifications.Add(OpcSpecification.OPC_DA_30);
                SupportedSpecifications.Add(OpcSpecification.OPC_DA_20);
            }
            else if (server is Com.Ae.Server)
            {
                SupportedSpecifications.Add(OpcSpecification.OPC_AE_10);
            }
            else if (server is Com.Hda.Server)
            {
                SupportedSpecifications.Add(OpcSpecification.OPC_HDA_10);
            }

            return server;
        }

        /// <summary>
        /// Connects to the specified COM server.
        /// </summary>
        public static object Connect(OpcUrl url, OpcConnectData connectData)
        {
            // parse path to find prog id and clsid.
            var progID = url.Path;
            string clsid = null;

            var index = url.Path.IndexOf('/');

            if (index >= 0)
            {
                progID = url.Path.Substring(0, index);
                clsid = url.Path.Substring(index + 1);
            }

            // look up prog id if clsid not specified in the url.
            Guid guid;

            if (clsid == null)
            {
                // use OpcEnum to lookup the prog id.
                guid = new ServerEnumerator().CLSIDFromProgID(progID, url.HostName, connectData);

                // check if prog id is actually a clsid string.
                if (guid == Guid.Empty)
                {
                    try
                    {
                        guid = new Guid(progID);
                    }
                    catch
                    {
                        throw new OpcResultException(new OpcResult((int)OpcResult.CO_E_CLASSSTRING.Code, OpcResult.FuncCallType.SysFuncCall, null), string.Format("Could not connect to server {0}", progID));
                    }
                }
            }

            // convert clsid string to a guid.
            else
            {
                try
                {
                    guid = new Guid(clsid);
                }
                catch
                {
                    throw new OpcResultException(new OpcResult((int)OpcResult.CO_E_CLASSSTRING.Code, OpcResult.FuncCallType.SysFuncCall, null), string.Format("Could not connect to server {0}", progID));
                }
            }

            // get the credentials.
            var credentials = (connectData != null) ? connectData.UserIdentity : null;

            // instantiate the server using CoCreateInstanceEx.
            if (connectData == null || connectData.LicenseKey == null)
            {
                try
                {
                    return Utilities.Interop.CreateInstance(guid, url.HostName, credentials);
                }
                catch (Exception e)
                {
                    throw new OpcResultException(OpcResult.CO_E_CLASSSTRING, e.Message, e);
                }
            }

            // instantiate the server using IClassFactory2.
            else
            {
                try
                {
                    return null;
                    //return Technosoftware.DaAeHdaClient.Utilities.Interop.CreateInstanceWithLicenseKey(guid, url.HostName, credentials, connectData.LicenseKey);
                }
                catch (Exception e)
                {
                    throw new OpcResultException(OpcResult.CO_E_CLASSSTRING, e.Message, e);
                }
            }
        }
    }
}
