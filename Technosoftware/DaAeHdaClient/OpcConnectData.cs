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
using System.Runtime.Serialization;
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// Contains protocol dependent connection and authentication information.
    /// </summary>
    [Serializable]
    public class OpcConnectData : ISerializable, ICredentials
    {
        #region Public Properties
        /// <summary>
        /// The credentials to submit to the proxy server for authentication.
        /// </summary>
        public OpcUserIdentity UserIdentity { get; set; }

        /// <summary>
        /// The license key used to connect to the server.
        /// </summary>
        public string LicenseKey { get; set; }

        /// <summary>
        /// Always uses the DA20 interfaces even if DA3.0 is supported.
        /// </summary>
        bool ForceDa20Usage { get; set; }

        /// <summary>
        /// Use DCOM connect level security (may be needed for backward compatibility).
        /// </summary>
        public bool UseConnectSecurity { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
		/// Returns a UserIdentity object that is associated with the specified URI, and authentication type.
        /// </summary>
        public NetworkCredential GetCredential(Uri uri, string authenticationType)
        {
            if (UserIdentity != null)
            {
                return new NetworkCredential(UserIdentity.Username, UserIdentity.Password, UserIdentity.Domain);
            }

            return null;
        }

        /// <summary>
        /// Returns the web proxy object to use when connecting to the server.
        /// </summary>
        public IWebProxy GetProxy()
        {
            if (proxy_ != null)
            {
                return proxy_;
            }
            else
            {
                return new WebProxy();
            }
        }

        /// <summary>
        /// Sets the web proxy object.
        /// </summary>
        public void SetProxy(WebProxy proxy)
        {
            proxy_ = proxy;
        }

        /// <summary>
        /// Initializes an instance with the specified credentials.
        /// </summary>
		public OpcConnectData(OpcUserIdentity userIdentity)
        {
            UserIdentity = userIdentity;
            proxy_ = null;
        }

        /// <summary>
        /// Initializes an instance with the specified credentials and web proxy.
        /// </summary>
		public OpcConnectData(OpcUserIdentity userIdentity, WebProxy proxy)
        {
            UserIdentity = userIdentity;
            proxy_ = proxy;
        }
        #endregion

        #region ISerializable Members
        /// <summary>
        /// A set of names for fields used in serialization.
        /// </summary>
        private class Names
        {
            internal const string UserName = "UN";
            internal const string Password = "PW";
            internal const string Domain = "DO";
            internal const string ProxyUri = "PU";
            internal const string LicenseKey = "LK";
        }

        /// <summary>
        /// Construct the object by de-serializing from the stream.
        /// </summary>
        protected OpcConnectData(SerializationInfo info, StreamingContext context)
        {
            var username = info.GetString(Names.UserName);
            var password = info.GetString(Names.Password);
            var domain = info.GetString(Names.Domain);
            var proxyUri = info.GetString(Names.ProxyUri);
            info.GetString(Names.LicenseKey);

            if (domain != null)
            {
                UserIdentity = new OpcUserIdentity("", "");
            }
            else
            {
                UserIdentity = new OpcUserIdentity(username, password);
            }

            if (proxyUri != null)
            {
                proxy_ = new WebProxy(proxyUri);
            }
            else
            {
                proxy_ = null;
            }
        }

        /// <summary>
        /// Serializes a server into a stream.
        /// </summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (UserIdentity != null)
            {
                info.AddValue(Names.UserName, UserIdentity.Username);
                info.AddValue(Names.Password, UserIdentity.Password);
                info.AddValue(Names.Domain, UserIdentity.Domain);
            }
            else
            {
                info.AddValue(Names.UserName, null);
                info.AddValue(Names.Password, null);
                info.AddValue(Names.Domain, null);
            }

            if (proxy_ != null)
            {
                info.AddValue(Names.ProxyUri, proxy_.Address);
            }
            else
            {
                info.AddValue(Names.ProxyUri, null);
            }
        }
        #endregion

        #region Private Fields
        private WebProxy proxy_;
        #endregion
    }
}
