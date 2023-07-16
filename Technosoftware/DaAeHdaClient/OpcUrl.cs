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
using System.Net;
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// Contains information required to connect to the OPC server.
    /// </summary>
    [Serializable]
    public class OpcUrl : ICloneable
    {
        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes an empty instance.
        /// </summary>
        public OpcUrl()
        {
            Scheme = OpcUrlScheme.HTTP;
            HostName = "localhost";
            Port = 0;
            Path = null;
        }

        /// <summary>
        /// Initializes an instance by providing OPC specification, OPC URL scheme and an URL string.
        /// </summary>
        /// <param name="specification">A description of an interface version defined by an OPC specification.</param>
        /// <param name="scheme">The scheme (protocol) for the URL</param>
        /// <param name="url">The URL of the OPC server.</param>
        public OpcUrl(OpcSpecification specification, string scheme, string url)
        {
            Specification = specification;
            HostName = "localhost";
            Port = 0;
            Path = null;

            ParseUrl(url);

            Scheme = scheme;
        }

        /// <summary>
        /// Initializes an instance by parsing an URL string.
        /// </summary>
        /// <param name="url">The URL of the OPC server.</param>
        public OpcUrl(string url)
        {
            Scheme = OpcUrlScheme.HTTP;
            HostName = "localhost";
            Port = 0;
            Path = null;

            ParseUrl(url);
        }
        #endregion

        #region Properties
        /// <summary>
        /// The supported OPC specification.
        /// </summary>
        public OpcSpecification Specification { get; set; }

        /// <summary>
        /// The scheme (protocol) for the URL
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// The host name for the URL.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// The port name for the URL (0 means default for protocol).
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The path for the URL.
        /// </summary>
        public string Path { get; set; }
        #endregion

        #region Object Method Overrides
        /// <summary>
        /// Returns a URL string for the object.
        /// </summary>
        public override string ToString()
        {
            var hostName = string.IsNullOrEmpty(HostName) ? "localhost" : HostName;

            if (Port > 0)
            {
                return $"{Scheme}://{hostName}:{Port}/{Path}";
            }
            else
            {
                return $"{Scheme}://{hostName}/{Path}";
            }
        }

        /// <summary>
        /// Compares the object to either another URL object or a URL string.
        /// </summary>
        public override bool Equals(object target)
        {
            OpcUrl url;

            if (target != null && target.GetType() == typeof(OpcUrl))
            {
                url = (OpcUrl)target;
            }
            else
            {
                url = null;
            }

            if (target != null && target.GetType() == typeof(string))
            {
                url = new OpcUrl((string)target);
            }

            if (url == null) return false;
            if (url.Path != Path) return false;
            if (url.Scheme != Scheme) return false;
            if (url.HostName != HostName) return false;
            if (url.Port != Port) return false;

            return true;
        }

        /// <summary>
        /// Returns a hash code for the object.
        /// </summary>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Returns a deep copy of the object.
        /// </summary>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }
        #endregion

        #region Private Methods
        private void ParseUrl(string url)
        {

            var buffer = url;

            // extract the scheme (default is http).
            var index = buffer.IndexOf("://", StringComparison.Ordinal);

            if (index >= 0)
            {
                Scheme = buffer.Substring(0, index);
                buffer = buffer.Substring(index + 3);
            }

            index = buffer.IndexOfAny(new[] { '/' });

            if (index < 0)
            {
                Path = buffer;
                return;
            }

            var hostPortString = buffer.Substring(0, index);

            IPAddress.TryParse(hostPortString, out var address);

            if (address != null && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                if (hostPortString.Contains("]"))
                {
                    HostName = hostPortString.Substring(0, hostPortString.IndexOf("]", StringComparison.Ordinal) + 1);
                    if (hostPortString.Substring(hostPortString.IndexOf(']')).Contains(":"))
                    {
                        var portString = hostPortString.Substring(hostPortString.LastIndexOf(':') + 1);
                        if (portString != "")
                        {
                            try
                            {
                                Port = Convert.ToUInt16(portString);
                            }
                            catch
                            {
                                Port = 0;
                            }
                        }
                        else
                        {
                            Port = 0;
                        }
                    }
                    else
                    {
                        Port = 0;
                    }

                    Path = buffer.Substring(index + 1);
                }
                else
                {
                    HostName = $"[{hostPortString}]";
                    Port = 0;
                }

                Path = buffer.Substring(index + 1);
            }
            else
            {

                // extract the hostname (default is localhost).
                index = buffer.IndexOfAny(new[] { ':', '/' });

                if (index < 0)
                {
                    Path = buffer;
                    return;
                }

                HostName = buffer.Substring(0, index);

                // extract the port number (default is 0).
                if (buffer[index] == ':')
                {
                    buffer = buffer.Substring(index + 1);
                    index = buffer.IndexOf("/", StringComparison.Ordinal);

                    string port;

                    if (index >= 0)
                    {
                        port = buffer.Substring(0, index);
                        buffer = buffer.Substring(index + 1);
                    }
                    else
                    {
                        port = buffer;
                        buffer = "";
                    }

                    try
                    {
                        Port = Convert.ToUInt16(port);
                    }
                    catch
                    {
                        Port = 0;
                    }
                }
                else
                {
                    buffer = buffer.Substring(index + 1);
                }

                // extract the path.
                Path = buffer;

                // In case the specification is not set, we try to find it out based on the Scheme
                if (Specification.Id == null)
                {
                    if (Scheme == OpcUrlScheme.DA)
                    {
                        Specification = OpcSpecification.OPC_DA_20;
                        return;
                    }
                    if (Scheme == OpcUrlScheme.AE)
                    {
                        Specification = OpcSpecification.OPC_AE_10;
                        return;
                    }
                    if (Scheme == OpcUrlScheme.HDA)
                    {
                        Specification = OpcSpecification.OPC_HDA_10;
                        return;
                    }
                    if (Scheme == OpcUrlScheme.HTTP)
                    {
                        Specification = OpcSpecification.XML_DA_10;
                    }
                }
            }
        }
        #endregion
    }
}
