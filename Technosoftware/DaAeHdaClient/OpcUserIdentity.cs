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
using System.Text;
using System.Security.Cryptography;
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// The user identity to use when connecting to the OPC server.
    /// </summary>
    public class OpcUserIdentity
    {
        ///////////////////////////////////////////////////////////////////////
        #region Fields

        private RSAParameters _rsaParams;

        private readonly byte[] _domainName;
        private readonly bool _domainNameValid;
        private readonly bool _usernameValid;
        private readonly byte[] _username;
        private readonly bool _passwordValid;
        private readonly byte[] _password;
        private readonly bool _clientCertificateNameValid;
        private readonly byte[] _clientCertificateName;
        private readonly bool _serverCertificateNameValid;
        private readonly byte[] _serverCertificateName;

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Constructors, Destructor, Initialization

        /// <summary>
        /// Sets the username and password (extracts the domain from the username if a '\' is present).
        /// </summary>
        /// <param name="username">The user name</param>
        /// <param name="password">The password</param>
        public OpcUserIdentity(string username, string password)
        {
            var ByteConverter = new UnicodeEncoding();
            using (var RSA = new RSACryptoServiceProvider())
            {
                string domainName = null;
                _rsaParams = RSA.ExportParameters(true);
                RSA.ImportParameters(_rsaParams);
                if (!string.IsNullOrEmpty(username))
                {
                    var index = username.IndexOf('\\');

                    if (index != -1)
                    {
                        domainName = username.Substring(0, index);
                        username = username.Substring(index + 1);
                    }
                    _usernameValid = true;
                    var userIdBytes = ByteConverter.GetBytes(username);
                    _username = RSA.Encrypt(userIdBytes, false);
                }

                if (!string.IsNullOrEmpty(password))
                {
                    _passwordValid = true;
                    var userKeyBytes = ByteConverter.GetBytes(password);
                    _password = RSA.Encrypt(userKeyBytes, false);
                }

                if (!string.IsNullOrEmpty(domainName))
                {
                    _domainNameValid = true;
                    var domainBytes = ByteConverter.GetBytes(domainName);
                    _domainName = RSA.Encrypt(domainBytes, false);
                }

            }
        }

        /// <summary>
        /// Sets the username and password.
        /// </summary>
        /// <param name="domainName">The windows domain name</param>
        /// <param name="userName">The user name</param>
        /// <param name="password">The password</param>
        public OpcUserIdentity(string domainName, string userName, string password)
            : this(userName, password)
        {
            var ByteConverter = new UnicodeEncoding();
            using (var RSA = new RSACryptoServiceProvider())
            {
                RSA.ImportParameters(_rsaParams);
                if (!string.IsNullOrEmpty(domainName))
                {
                    _domainNameValid = true;
                    var domainBytes = ByteConverter.GetBytes(domainName);
                    _domainName = RSA.Encrypt(domainBytes, false);
                }
            }
        }

        /// <summary>
        /// Sets the username and password.
        /// </summary>
        /// <param name="domainName">The windows domain name</param>
        /// <param name="userName">The user name</param>
        /// <param name="password">The password</param>
        /// <param name="clientCertificateName">The Client Certificate name</param>
        /// <param name="serverCertificateName">The Server Certificate name</param>
        public OpcUserIdentity(string domainName, string userName, string password, string clientCertificateName, string serverCertificateName)
            : this(domainName, userName, password)
        {
            var ByteConverter = new UnicodeEncoding();
            using (var RSA = new RSACryptoServiceProvider())
            {
                RSA.ImportParameters(_rsaParams);
                if (!string.IsNullOrEmpty(clientCertificateName))
                {
                    _clientCertificateNameValid = true;
                    var clientCertificateBytes = ByteConverter.GetBytes(clientCertificateName);
                    _clientCertificateName = RSA.Encrypt(clientCertificateBytes, false);
                }
                if (!string.IsNullOrEmpty(serverCertificateName))
                {
                    _serverCertificateNameValid = true;
                    var serverCertificateBytes = ByteConverter.GetBytes(serverCertificateName);
                    _serverCertificateName = RSA.Encrypt(serverCertificateBytes, false);
                }
            }
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Properties

        /// <summary>
        /// The windows domain name.
        /// </summary>
        public string Domain
        {
            get
            {
                if (_domainNameValid)
                {
                    try
                    {
                        using (var RSA = new RSACryptoServiceProvider())
                        {
                            RSA.ImportParameters(_rsaParams);
                            var domainNameBytes = RSA.Decrypt(_domainName, false);
                            var ByteConverter = new UnicodeEncoding();
                            return ByteConverter.GetString(domainNameBytes);
                        }
                    }
                    catch (Exception)
                    {
                        throw new OpcResultException(OpcResult.E_FAIL, "The user info object has been corrupted.");
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// The user name. 
        /// </summary>
        public string Username
        {
            get
            {
                if (_usernameValid)
                {
                    try
                    {
                        using (var RSA = new RSACryptoServiceProvider())
                        {
                            RSA.ImportParameters(_rsaParams);
                            var userNameBytes = RSA.Decrypt(_username, false);
                            var ByteConverter = new UnicodeEncoding();
                            return ByteConverter.GetString(userNameBytes);
                        }
                    }
                    catch (Exception)
                    {
                        throw new OpcResultException(OpcResult.E_FAIL, "The user info object has been corrupted.");
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// The password. 
        /// </summary>
        public string Password
        {
            get
            {
                if (_passwordValid)
                {
                    try
                    {
                        using (var RSA = new RSACryptoServiceProvider())
                        {
                            RSA.ImportParameters(_rsaParams);
                            var passwordBytes = RSA.Decrypt(_password, false);
                            var ByteConverter = new UnicodeEncoding();
                            return ByteConverter.GetString(passwordBytes);
                        }
                    }
                    catch (Exception)
                    {
                        throw new OpcResultException(OpcResult.E_FAIL, "The user info object has been corrupted.");
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets or sets a license key to use when activating the server.
        /// </summary>
        public string LicenseKey { get; set; }

        /// <summary>
        /// The Client Certificate name for certificate mode authentication of the server access. 
        /// </summary>
        public string ClientCertificateName
        {
            get
            {
                if (_clientCertificateNameValid)
                {
                    try
                    {
                        using (var RSA = new RSACryptoServiceProvider())
                        {
                            RSA.ImportParameters(_rsaParams);
                            var clientCertificateBytes = RSA.Decrypt(_clientCertificateName, false);
                            var ByteConverter = new UnicodeEncoding();
                            return ByteConverter.GetString(clientCertificateBytes);
                        }
                    }
                    catch (Exception)
                    {
                        throw new OpcResultException(OpcResult.E_FAIL, "The user info object has been corrupted.");
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// The Server Certificate name for certificate mode authentication of the server access.
        /// </summary>
        public string ServerCertificateName
        {
            get
            {
                if (_serverCertificateNameValid)
                {
                    try
                    {
                        using (var RSA = new RSACryptoServiceProvider())
                        {
                            RSA.ImportParameters(_rsaParams);
                            var serverCertificateBytes = RSA.Decrypt(_serverCertificateName, false);
                            var ByteConverter = new UnicodeEncoding();
                            return ByteConverter.GetString(serverCertificateBytes);
                        }
                    }
                    catch (Exception)
                    {
                        throw new OpcResultException(OpcResult.E_FAIL, "The user info object has been corrupted.");
                    }
                }
                return null;
            }
        }
 
        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Public Methods

        /// <summary>
        /// Whether the identity represents an the default identity.
        /// </summary>
        public static bool IsDefault(OpcUserIdentity identity)
        {
            if (identity != null)
            {
                return (!identity._usernameValid);
            }

            return true;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Public Methods (Comparison Operators)

        /// <summary>
        /// Determines if the object is equal to the specified value.
        /// </summary>
        /// <param name="target">The OpcUserIdentity to compare with</param>
        /// <returns>True if the objects are equal; otherwise false.</returns>
        public override bool Equals(object target)
        {
            object identity = target as OpcUserIdentity;

            if (identity == null)
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Converts the object to a string used for display.
        /// </summary>
        public override string ToString()
        {
            if (_domainNameValid)
            {
                return string.Format("{0}\\{1}", Domain, Username);
            }

            return Username;
        }

        /// <summary>
        /// Returns a suitable hash code for the result.
        /// </summary>
        public override int GetHashCode()
        {
            if (_usernameValid)
            {
                return Username.GetHashCode();
            }

            return 0;
        }

        /// <summary>
        /// Returns true if the objects are equal.
        /// </summary>
        /// <param name="a">The first OpcUserIdentity to compare.</param>
        /// <param name="b">The second OpcUserIdentity to compare.</param>
        /// <returns>True if the objects are equal; otherwise false.</returns>
        public static bool operator ==(OpcUserIdentity a, OpcUserIdentity b)
        {
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
            }

            return a.Equals(b);
        }

        /// <summary>
        /// Returns true if the objects are not equal.
        /// </summary>
        /// <param name="a">The first OpcUserIdentity to compare.</param>
        /// <param name="b">The second OpcUserIdentity to compare.</param>
        /// <returns>True if the objects are not equal; otherwise false.</returns>
        public static bool operator !=(OpcUserIdentity a, OpcUserIdentity b)
        {
            if (ReferenceEquals(a, null))
            {
                return !ReferenceEquals(b, null);
            }

            return !a.Equals(b);
        }

        #endregion
    }
}
