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
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>A base class for an in-process object used to access OPC servers.</summary>
    [Serializable]
    public class OpcServer : IOpcServer, ISerializable, ICloneable
    {
        #region Fields

        /// <summary>
        /// The remote server object.
        /// </summary>
        internal IOpcServer Server;

        /// <summary>
        /// The OpcUrl that describes the network location of the server.
        /// </summary>
        private OpcUrl opcUrl_;

        /// <summary>
        /// The factory used to instantiate the remote server.
        /// </summary>
        protected IOpcFactory Factory;

        /// <summary>
        /// The last set of credentials used to connect successfully to the server.
        /// </summary>
        private OpcConnectData connectData_;

        /// <summary>
        /// A short name for the server.
        /// </summary>
        private string serverName_;

        /// <summary>
        /// A short name for the server assigned by the client
        /// </summary>
        private string clientName_;

        /// <summary>
        /// The default locale used by the server.
        /// </summary>
        private string locale_;

        /// <summary>
        /// The set of locales supported by the remote server.
        /// </summary>
        private string[] supportedLocales_;

        /// <summary>
        /// The resource manager used to access localized resources.
        /// </summary>
        protected ResourceManager ResourceManager;
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes the object.
        /// </summary>
        public OpcServer()
        {
            Factory = null;
            Server = null;
            opcUrl_ = null;
            serverName_ = null;
            supportedLocales_ = null;
            ResourceManager = new ResourceManager("Technosoftware.DaAeHdaClient.Resources.Strings", Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Initializes the object with a factory and a default OpcUrl.
        /// </summary>
        /// <param name="factory">The OpcFactory used to connect to remote servers.</param>
        /// <param name="url">The network address of a remote server.</param>
		public OpcServer(OpcFactory factory, OpcUrl url)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            Factory = (IOpcFactory)factory.Clone();
            Server = null;
            opcUrl_ = null;
            serverName_ = null;
            supportedLocales_ = null;
            ResourceManager = new ResourceManager("Technosoftware.DaAeHdaClient.Resources.Strings", Assembly.GetExecutingAssembly());

            if (url != null) SetUrl(url);
        }

        /// <summary>
        /// This must be called explicitly by clients to ensure the remote server is released.
        /// </summary>
        public virtual void Dispose()
        {
            if (Factory != null)
            {
                Factory.Dispose();
                Factory = null;
            }

            if (Server != null)
            {
                try { Disconnect(); }
                catch
                {
                    // ignored
                }

                Server = null;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Information about an OPC Server
        /// </summary>
        public OpcServerDescription ServerDescription { get; set; }

        /// <summary>
        /// List of supported OPC specifications
        /// </summary>
        public IList<OpcSpecification> SupportedSpecifications { get; set; }

        /// <summary>
        /// Can be used to force OPC DA 2.0 even if OPC DA 3.0 server features are available
        /// </summary>
        public bool ForceDa20Usage { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Finds the best matching locale given a set of supported locales.
        /// </summary>
        public static string FindBestLocale(string requestedLocale, string[] supportedLocales)
        {
            try
            {
                // check for direct match with requested locale.
                foreach (var supportedLocale in supportedLocales)
                {
                    if (supportedLocale == requestedLocale)
                    {
                        return requestedLocale;
                    }
                }

                // try to find match for parent culture.
                var requestedCulture = new CultureInfo(requestedLocale);

                foreach (var supportedLocale in supportedLocales)
                {
                    try
                    {
                        var supportedCulture = new CultureInfo(supportedLocale);

                        if (requestedCulture.Parent.Name == supportedCulture.Name)
                        {
                            return supportedCulture.Name;
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                // return default locale.     
                return (supportedLocales.Length > 0) ? supportedLocales[0] : "";
            }
            catch
            {
                // return default locale on any error.    
                return (supportedLocales != null && supportedLocales.Length > 0) ? supportedLocales[0] : "";
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Updates the OpcUrl for the server.
        /// </summary>
        private void SetUrl(OpcUrl url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            //  cannot change the OpcUrl if the remote server is already instantiated.
            if (Server != null) throw new OpcResultException(new OpcResult(OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The server is already connected.");

            //  copy the url.
            opcUrl_ = (OpcUrl)url.Clone();

            //  construct a name for the server.
            var name = "";

            //  use the host name as a base.
            if (opcUrl_.HostName != null)
            {
                name = opcUrl_.HostName.ToLower();

                // suppress localhost and loopback as explicit hostnames.
                if (name == "localhost" || name == "127.0.0.1")
                {
                    name = "";
                }
            }

            //  append the port.
            if (opcUrl_.Port != 0)
            {
                name += $".{opcUrl_.Port}";
            }

            //  add a separator.
            if (name != "") { name += "."; }

            //  use the prog id as the name.
            if (opcUrl_.Scheme != OpcUrlScheme.HTTP)
            {
                var progId = opcUrl_.Path;

                var index = progId.LastIndexOf('/');

                if (index != -1)
                {
                    progId = progId.Substring(0, index);
                }

                name += progId;
            }

            // use full path without the extension as the name.
            else
            {
                var path = opcUrl_.Path;

                // strip the file extension.
                var index = path.LastIndexOf('.');

                if (index != -1)
                {
                    path = path.Substring(0, index);
                }

                // replace slashes with dashes.
                while (path.IndexOf('/') != -1)
                {
                    path = path.Replace('/', '-');
                }

                name += path;
            }

            //  save the generated name in case the server name is not already set
            if (string.IsNullOrEmpty(serverName_))
            {
                serverName_ = name;
            }
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Returns a localized string with the specified name.
        /// </summary>
        protected string GetString(string name)
        {
            //  create a culture object.
            CultureInfo culture;

            try { culture = new CultureInfo(Locale); }
            catch { culture = new CultureInfo(""); }

            //  lookup resource string.
            try { return ResourceManager.GetString(name, culture); }
            catch { return null; }
        }
        #endregion

        #region ISerializable Members
        /// <summary>
        /// A   set of names for fields used in serialization.
        /// </summary>
        private class Names
        {
            internal const string Name = "Name";
            internal const string Url = "Url";
            internal const string Factory = "Factory";
        }

        /// <summary>
        /// Construct a server by de-serializing its OpcUrl from the stream.
        /// </summary>
        internal OpcServer(SerializationInfo info, StreamingContext context)
        {
            serverName_ = info.GetString(Names.Name);
            opcUrl_ = (OpcUrl)info.GetValue(Names.Url, typeof(OpcUrl));
            Factory = (IOpcFactory)info.GetValue(Names.Factory, typeof(IOpcFactory));
        }

        /// <summary>
        /// Serializes a server into a stream.
        /// </summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(Names.Name, serverName_);
            info.AddValue(Names.Url, opcUrl_);
            info.AddValue(Names.Factory, Factory);
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Returns an unconnected copy of the server with the same OpcUrl. 
        /// </summary>
        public virtual object Clone()
        {
            //  do a memberwise clone.
            var clone = (OpcServer)MemberwiseClone();

            //  place clone in disconnected state.
            clone.Server = null;
            clone.supportedLocales_ = null;
            clone.locale_ = null;
            clone.ResourceManager = new ResourceManager("Technosoftware.DaAeHdaClient.Resources.Strings", Assembly.GetExecutingAssembly());

            //  return clone.
            return clone;
        }
        #endregion

        #region IOpcServer Members
        /// <summary>
        /// A short descriptive name for the server.
        /// </summary>
        public virtual string ServerName
        {
            get => serverName_;
            set => serverName_ = value;
        }

        /// <summary>
        /// A short descriptive name for the server assigned by the client.
        /// </summary>
        public virtual string ClientName
        {
            get => clientName_;
            set
            {
                clientName_ = value;
                Server?.SetClientName(value);
            }
        }
        /// <summary>
        /// The OpcUrl that describes the network location of the server.
        /// </summary>
        public virtual OpcUrl Url
        {
            get => (OpcUrl)opcUrl_?.Clone();
            set => SetUrl(value);
        }

        /// <summary>
        /// The default of locale used by the remote server.
        /// </summary>
        public virtual string Locale => locale_;

        /// <summary>
        /// The set of locales supported by the remote server.
        /// </summary>
        public virtual string[] SupportedLocales => (string[])supportedLocales_?.Clone();

        /// <summary>
        /// Whether the remote server is currently connected.
        /// </summary>
        public virtual bool IsConnected => (Server != null);

        /// <summary>
        /// Allows the client to optionally register a client name with the server. This is included primarily for debugging purposes. The recommended behavior is that the client set his Node name and EXE name here.
        /// </summary>
        public virtual void SetClientName(string clientName)
        {
            ClientName = clientName;
        }

        /// <summary>
        /// Establishes a physical connection to the remote server.
        /// </summary>
        public virtual void Connect()
        {
            Connect(opcUrl_, null);
        }

        /// <summary>Establishes a physical connection to the remote server.</summary>
        /// <exception cref="OpcResultException" caption="OpcResultException Class">If an OPC specific error occur this exception is raised. The Result field includes then the OPC specific code.</exception>
        /// <param name="url">Name of the server. The usual form is http:://xxx/yyy, e.g. http://localhost//TsOpcXSampleServer/Service.asmx.</param>
        public virtual void Connect(string url)
        {
            Factory = null;
            var opcUrl = new OpcUrl(url);
            Connect(opcUrl, null);
        }

        /// <summary>
        /// Establishes a physical connection to the remote server.
        /// </summary>
        /// <param name="connectData">Any protocol configuration or user authentication information.</param>
        public virtual void Connect(OpcConnectData connectData)
        {
            Connect(opcUrl_, connectData);
        }

        /// <summary>
        /// Establishes a physical connection to the remote server identified by a OpcUrl.
        /// </summary>
        /// <param name="url">The network address of the remote server.</param>
        /// <param name="connectData">Any protocol configuration or user authentication information.</param>
        public virtual void Connect(OpcUrl url, OpcConnectData connectData)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
            if (Server != null) throw new OpcResultException(new OpcResult(OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The server is already connected.");

            //  save url.
            SetUrl(url);

            try
            {
                Factory.ForceDa20Usage = ForceDa20Usage;

                // instantiate the server object.
                Server = Factory.CreateInstance(url, connectData);
                if (Server == null) throw new OpcResultException(new OpcResult(OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "A connection to the server could not be established.");

                // save the connect data.
                connectData_ = connectData;

                try
                {
                    // cache the supported locales.
                    GetSupportedLocales();

                    // update the default locale.
                    SetLocale(locale_);
                }
                catch
                {
                    // ignored
                }
            }
            catch (Exception)
            {
                if (Server != null)
                {
                    try { Disconnect(); }
                    catch
                    {
                        // ignored
                    }
                }

                throw;
            }
        }

        /// <summary>
        /// Disconnects from the server and releases all network resources.
        /// </summary>
        public virtual void Disconnect()
        {
            if (Server == null) throw new OpcResultException(new OpcResult(OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The server is not currently connected.");

            //  dispose of the remote server object.
            Server.Dispose();
            Server = null;
        }

        /// <summary>
        /// Creates a new instance of a server object with the same factory and url.
        /// </summary>
        /// <remarks>This method does not copy the value of any properties.</remarks>
        /// <returns>An unconnected duplicate instance of the server object.</returns>
        public virtual OpcServer Duplicate()
        {
            var instance = (OpcServer)Activator.CreateInstance(GetType(), Factory, opcUrl_);

            //  preserve the credentials.
            instance.connectData_ = connectData_;

            //  preserve the locale.
            instance.locale_ = locale_;

            return instance;
        }

        /// <summary>
        /// An event to receive server shutdown notifications.
        /// </summary>
        public virtual event OpcServerShutdownEventHandler ServerShutdownEvent
        {
            add => Server.ServerShutdownEvent += value;
            remove => Server.ServerShutdownEvent -= value;
        }

        /// <summary>
        /// The locale used in any error messages or results returned to the client.
        /// </summary>
        /// <returns>The locale name in the format "[languagecode]-[country/regioncode]".</returns>
        public virtual string GetLocale()
        {
            if (Server == null) throw new NotConnectedException();

            // cache the current locale.
            locale_ = Server.GetLocale();

            // return the cached value.
            return locale_;
        }

        /// <summary>
        /// Sets the locale used in any error messages or results returned to the client.
        /// </summary>
        /// <param name="locale">The locale name in the format "[languagecode]-[country/regioncode]".</param>
        /// <returns>A locale that the server supports and is the best match for the requested locale.</returns>
        public virtual string SetLocale(string locale)
        {
            if (Server == null) throw new NotConnectedException();

            try
            {
                // set the requested locale on the server.
                locale_ = Server.SetLocale(locale);
            }
            catch
            {
                // find a best match and check if the server supports it.
                var revisedLocale = FindBestLocale(locale, supportedLocales_);

                if (revisedLocale != locale)
                {
                    Server.SetLocale(revisedLocale);
                }

                // cache the revised locale.
                locale_ = revisedLocale;
            }

            // return actual local used.
            return locale_;
        }

        /// <summary>
        /// Returns the locales supported by the server
        /// </summary>
        /// <remarks>The first element in the array must be the default locale for the server.</remarks>
        /// <returns>An array of locales with the format "[languagecode]-[country/regioncode]".</returns>
        public virtual string[] GetSupportedLocales()
        {
            if (Server == null) throw new OpcResultException(new OpcResult(OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The server is not currently connected.");

            //  cache supported locales.
            supportedLocales_ = Server.GetSupportedLocales();

            //  return copy of cached locales. 
            return SupportedLocales;
        }

        /// <summary>
        /// Returns the localized text for the specified result code.
        /// </summary>
        /// <param name="locale">The locale name in the format "[languagecode]-[country/regioncode]".</param>
        /// <param name="resultId">The result code identifier.</param>
        /// <returns>A message localized for the best match for the requested locale.</returns>
        public virtual string GetErrorText(string locale, OpcResult resultId)
        {
            if (Server == null) throw new OpcResultException(OpcResult.E_FAIL, "The server is not currently connected.");

            return Server.GetErrorText(locale ?? locale_, resultId);
        }
        #endregion
    }

    //=============================================================================
    // Exceptions

    /// <summary>
    /// Raised if an operation cannot be executed because the server is not connected.
    /// </summary>
    [Serializable]
    public class AlreadyConnectedException : ApplicationException
    {
        private const string Default = "The remote server is already connected.";
        /// <remarks/>
        public AlreadyConnectedException() : base(Default) { }
        /// <remarks/>
        public AlreadyConnectedException(string message) : base(Default + "\r\n" + message) { }
        /// <remarks/>
        public AlreadyConnectedException(Exception e) : base(Default, e) { }
        /// <remarks/>
        public AlreadyConnectedException(string message, Exception innerException) : base(Default + "\r\n" + message, innerException) { }
        /// <remarks/>
        protected AlreadyConnectedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Raised if an operation cannot be executed because the server is not connected.
    /// </summary>
    [Serializable]
    public class NotConnectedException : ApplicationException
    {
        private const string Default = "The remote server is not currently connected.";
        /// <remarks/>
        public NotConnectedException() : base(Default) { }
        /// <remarks/>
        public NotConnectedException(string message) : base(Default + "\r\n" + message) { }
        /// <remarks/>
        public NotConnectedException(Exception e) : base(Default, e) { }
        /// <remarks/>
        public NotConnectedException(string message, Exception innerException) : base(Default + "\r\n" + message, innerException) { }
        /// <remarks/>
        protected NotConnectedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Raised if an operation cannot be executed because the server is not reachable.
    /// </summary>
    [Serializable]
    public class ConnectFailedException : OpcResultException
    {
        private const string Default = "Could not connect to server.";
        /// <remarks/>
        public ConnectFailedException() : base(OpcResult.E_ACCESS_DENIED, Default) { }
        /// <remarks/>
        public ConnectFailedException(string message) : base(OpcResult.E_NETWORK_ERROR, Default + "\r\n" + message) { }
        /// <remarks/>
        public ConnectFailedException(Exception e) : base(OpcResult.E_NETWORK_ERROR, Default, e) { }
        /// <remarks/>
        public ConnectFailedException(string message, Exception innerException) : base(OpcResult.E_NETWORK_ERROR, Default + "\r\n" + message, innerException) { }
        /// <remarks/>
        protected ConnectFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Raised if an operation cannot be executed because the server refuses access.
    /// </summary>
    [Serializable]
    public class AccessDeniedException : OpcResultException
    {
        private const string Default = "The server refused the connection.";
        /// <remarks/>
        public AccessDeniedException() : base(OpcResult.E_ACCESS_DENIED, Default) { }
        /// <remarks/>
        public AccessDeniedException(string message) : base(OpcResult.E_ACCESS_DENIED, Default + "\r\n" + message) { }
        /// <remarks/>
        public AccessDeniedException(Exception e) : base(OpcResult.E_ACCESS_DENIED, Default, e) { }
        /// <remarks/>
        public AccessDeniedException(string message, Exception innerException) : base(OpcResult.E_NETWORK_ERROR, Default + "\r\n" + message, innerException) { }
        /// <remarks/>
        protected AccessDeniedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Raised an remote operation by the server timed out
    /// </summary>
    public class ServerTimeoutException : OpcResultException
    {
        private const string Default = "The server did not respond within the specified timeout period.";
        /// <remarks/>
        public ServerTimeoutException() : base(OpcResult.E_TIMEDOUT, Default) { }
        /// <remarks/>
        public ServerTimeoutException(string message) : base(OpcResult.E_TIMEDOUT, Default + "\r\n" + message) { }
        /// <remarks/>
        public ServerTimeoutException(Exception e) : base(OpcResult.E_TIMEDOUT, Default, e) { }
        /// <remarks/>
        public ServerTimeoutException(string message, Exception innerException) : base(OpcResult.E_TIMEDOUT, Default + "\r\n" + message, innerException) { }
        /// <remarks/>
        protected ServerTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Raised an remote operation by the server returned unusable or invalid results.
    /// </summary>
    [Serializable]
    public class InvalidResponseException : ApplicationException
    {
        private const string Default = "The response from the server was invalid or incomplete.";
        /// <remarks/>
        public InvalidResponseException() : base(Default) { }
        /// <remarks/>
        public InvalidResponseException(string message) : base(Default + "\r\n" + message) { }
        /// <remarks/>
        public InvalidResponseException(Exception e) : base(Default, e) { }
        /// <remarks/>
        public InvalidResponseException(string message, Exception innerException) : base(Default + "\r\n" + message, innerException) { }
        /// <remarks/>
        protected InvalidResponseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Raised if the browse position is not valid.
    /// </summary>
    [Serializable]
    public class BrowseCannotContinueException : ApplicationException
    {
        private const string Default = "The browse operation cannot continue.";
        /// <remarks/>
        public BrowseCannotContinueException() : base(Default) { }
        /// <remarks/>
        public BrowseCannotContinueException(string message) : base(Default + "\r\n" + message) { }
        /// <remarks/>
        public BrowseCannotContinueException(string message, Exception innerException) : base(Default + "\r\n" + message, innerException) { }
        /// <remarks/>
        protected BrowseCannotContinueException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Raised if the browse position is not valid.
    /// </summary>
    [Serializable]
    public class BadInternalErrorException : ApplicationException
    {
        private const string Default = "License required! You can't use this feature.";
        /// <remarks/>
        public BadInternalErrorException() : base(Default) { }
        /// <remarks/>
        public BadInternalErrorException(string message) : base(message) { }
        /// <remarks/>
        public BadInternalErrorException(string message, Exception innerException) : base(message, innerException) { }
        /// <remarks/>
        protected BadInternalErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Exception that is raise when a DCOM call is cancelled due to timeout
    /// </summary>
    /// 
    [Serializable]
    public class DCOMCallCancelledException : ApplicationException
    {
        private const string Default = "The current pending DCOM call was cancelled";
        /// <remarks/>
        public DCOMCallCancelledException() : base(Default) { }
        /// <remarks/>
        public DCOMCallCancelledException(string message) : base(message) { }
        /// <remarks/>
        public DCOMCallCancelledException(string message, Exception innerException) : base(message, innerException) { }
        /// <remarks/>
        protected DCOMCallCancelledException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    }

}
