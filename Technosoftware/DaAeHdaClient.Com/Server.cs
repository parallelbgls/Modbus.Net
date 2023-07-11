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
using Technosoftware.DaAeHdaClient.Com.Utilities;
using Technosoftware.DaAeHdaClient.Utilities;
using Technosoftware.OpcRcw.Comn;
#endregion

namespace Technosoftware.DaAeHdaClient.Com
{
    /// <summary>
    /// An in-process wrapper for a remote OPC COM server (not thread safe).
    /// </summary>
	internal class Server : IOpcServer
    {
        #region Fields

        /// <summary>
        /// The COM server wrapped by the object.
        /// </summary>
        protected object server_;

        /// <summary>
        /// The URL containing host, prog id and clsid information for The remote server.
        /// </summary>
        protected OpcUrl url_;

        /// <summary>
        /// A connect point with the COM server.
        /// </summary>
        private ConnectionPoint connection_;

        /// <summary>
        /// The internal object that implements the IOPCShutdown interface.
        /// </summary>
        private Callback callback_;

        /// <summary>
        /// The synchronization object for server access
        /// </summary>
        private static volatile object lock_ = new object();

        private int outstandingCalls_;

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the object.
        /// </summary>
        internal Server()
        {
            url_ = null;
            server_ = null;
            callback_ = new Callback(this);
        }

        /// <summary>
        /// Initializes the object with the specifed COM server.
        /// </summary>
        internal Server(OpcUrl url, object server)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            url_ = (OpcUrl)url.Clone();
            server_ = server;
            callback_ = new Callback(this);
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// The finalizer.
        /// </summary>
        ~Server()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged resources held by the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
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
            if (!disposed_)
            {
                lock (this)
                {
                    if (disposing)
                    {
                        // Free other state (managed objects).

                        // close callback connections.
                        if (connection_ != null)
                        {
                            connection_.Dispose();
                            connection_ = null;
                        }
                    }

                    DisableDCOMCallCancellation();

                    // Free your own state (unmanaged objects).
                    // Set large fields to null.                

                    // release server.
                    Interop.ReleaseServer(server_);
                    server_ = null;
                }

                disposed_ = true;
            }
        }

        private bool disposed_;
        #endregion

        #region Public Methods

        /// <summary>
        /// Connects to the server with the specified URL and credentials.
        /// </summary>
        public virtual void Initialize(OpcUrl url, OpcConnectData connectData)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            lock (lock_)
            {
                // re-connect only if the url has changed or has not been initialized.
                if (url_ == null || !url_.Equals(url))
                {
                    // release the current server.
                    if (server_ != null)
                    {
                        Uninitialize();
                    }

                    // instantiate a new server.
                    server_ = (IOPCCommon)Factory.Connect(url, connectData);
                }

                // save url.
                url_ = (OpcUrl)url.Clone();
            }
        }

        /// <summary>
        /// Releases The remote server.
        /// </summary>
        public virtual void Uninitialize()
        {
            lock (lock_)
            {
                Dispose();
            }
        }

        /// <summary>
        /// Allows the client to optionally register a client name with the server. This is included primarily for debugging purposes. The recommended behavior is that the client set his Node name and EXE name here.
        /// </summary>
        public virtual void SetClientName(string clientName)
        {
            try
            {
                ((IOPCCommon)server_).SetClientName(clientName);
            }
            catch (Exception e)
            {
                throw Utilities.Interop.CreateException("IOPCCommon.SetClientName", e);
            }
        }

        /// <summary>
        /// Allows cancellation control of DCOM callbacks to the server - by default DCOM calls will wait the default DCOM timeout
        /// to fail - this method allows for tigher control of the timeout to wait. Note that DOCM calls can only be controlled
        /// on a COM Single Threaded Apartment thread - use [STAThread] attribute on your application entry point or use Thread SetThreadApartment
        /// before the thread the server is operating on is created to STA.
        /// </summary>
        /// <param name="timeout">The DCOM call timeout - uses the default timeout if not specified</param>
        public void EnableDCOMCallCancellation(TimeSpan timeout = default)
        {
            DCOMCallWatchdog.Enable(timeout);
        }

        /// <summary>
        /// Disables cancellation control of DCOM calls to the server
        /// </summary>
        public void DisableDCOMCallCancellation()
        {
            DCOMCallWatchdog.Disable();
        }

        #endregion

        #region IOpcServer Members

        /// <summary>
        /// An event to receive server shutdown notifications.
        /// </summary>
        public virtual event OpcServerShutdownEventHandler ServerShutdownEvent
        {
            add
            {
                lock (lock_)
                {
                    try
                    {
                        Advise();
                        callback_.ServerShutdown += value;
                    }
                    catch
                    {
                        // shutdown not supported.
                    }
                }
            }

            remove
            {
                lock (lock_)
                {
                    callback_.ServerShutdown -= value;
                    Unadvise();
                }
            }
        }

        /// <summary>
        /// The locale used in any error messages or results returned to the client.
        /// </summary>
        /// <returns>The locale name in the format "[languagecode]-[country/regioncode]".</returns>
        public virtual string GetLocale()
        {
            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                try
                {
                    ((IOPCCommon)server_).GetLocaleID(out var localeId);
                    return Interop.GetLocale(localeId);
                }
                catch (Exception e)
                {
                    throw Interop.CreateException("IOPCCommon.GetLocaleID", e);
                }
            }
        }

        /// <summary>
		/// Sets the locale used in any error messages or results returned to the client.
		/// </summary>
		/// <param name="locale">The locale name in the format "[languagecode]-[country/regioncode]".</param>
		/// <returns>A locale that the server supports and is the best match for the requested locale.</returns>
		public virtual string SetLocale(string locale)
        {
            lock (this)
            {
                if (server_ == null) throw new NotConnectedException();

                var lcid = Interop.GetLocale(locale);

                try
                {
                    ((IOPCCommon)server_).SetLocaleID(lcid);
                }
                catch (Exception e)
                {
                    if (lcid != 0)
                    {
                        throw Interop.CreateException("IOPCCommon.SetLocaleID", e);
                    }

                    // use LOCALE_SYSTEM_DEFAULT if the server does not support the Neutral LCID.
                    try { ((IOPCCommon)server_).SetLocaleID(0x800); }
                    catch { }
                }

                return GetLocale();
            }
        }

        /// <summary>
        /// Returns the locales supported by the server
        /// </summary>
        /// <remarks>The first element in the array must be the default locale for the server.</remarks>
        /// <returns>An array of locales with the format "[languagecode]-[country/regioncode]".</returns>
        public virtual string[] GetSupportedLocales()
        {
            lock (lock_)
            {
                if (server_ == null) throw new NotConnectedException();

                try
                {
                    var count = 0;
                    var pLocaleIDs = IntPtr.Zero;

                    ((IOPCCommon)server_).QueryAvailableLocaleIDs(out count, out pLocaleIDs);

                    var localeIDs = Interop.GetInt32s(ref pLocaleIDs, count, true);

                    if (localeIDs != null)
                    {
                        var locales = new ArrayList();

                        foreach (var localeID in localeIDs)
                        {
                            try { locales.Add(Interop.GetLocale(localeID)); }
                            catch { }
                        }

                        return (string[])locales.ToArray(typeof(string));
                    }

                    return null;
                }
                catch
                {
                    //throw Interop.CreateException("IOPCCommon.QueryAvailableLocaleIDs", e);
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns the localized text for the specified result code.
        /// </summary>
        /// <param name="locale">The locale name in the format "[languagecode]-[country/regioncode]".</param>
        /// <param name="resultId">The result code identifier.</param>
        /// <returns>A message localized for the best match for the requested locale.</returns>
        public virtual string GetErrorText(string locale, OpcResult resultId)
        {
            lock (lock_)
            {
                if (server_ == null) throw new NotConnectedException();

                try
                {
                    var currentLocale = GetLocale();

                    if (currentLocale != locale)
                    {
                        SetLocale(locale);
                    }

                    ((IOPCCommon)server_).GetErrorString(resultId.Code, out var errorText);

                    if (currentLocale != locale)
                    {
                        SetLocale(currentLocale);
                    }

                    return errorText;
                }
                catch (Exception e)
                {
                    throw Utilities.Interop.CreateException("IOPCServer.GetErrorString", e);
                }
            }
        }
        #endregion

        #region Protected Members
        /// <summary>
        /// Releases all references to the server.
        /// </summary>
        protected virtual void ReleaseServer()
        {
            lock (lock_)
            {
                SafeNativeMethods.ReleaseServer(server_);
                server_ = null;
            }
        }

        /// <summary>
        /// Checks if the server supports the specified interface.
        /// </summary>
        /// <typeparam name="T">The interface to check.</typeparam>
        /// <returns>True if the server supports the interface.</returns>
        protected bool SupportsInterface<T>() where T : class
        {
            lock (lock_)
            {
                return server_ is T;
            }
        }
        #endregion       

        #region COM Call Tracing
        /// <summary>
        /// Must be called before any COM call.
        /// </summary>
        /// <typeparam name="T">The interface to used when making the call.</typeparam>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="isRequiredInterface">if set to <c>true</c> interface is an required interface and and exception is thrown on error.</param>
        /// <returns></returns>
        protected T BeginComCall<T>(string methodName, bool isRequiredInterface) where T : class
        {
            return BeginComCall<T>(server_, methodName, isRequiredInterface);
        }

        /// <summary>
        /// Must be called before any COM call.
        /// </summary>
        /// <typeparam name="T">The interface to used when making the call.</typeparam>
        /// <param name="parent">Parent COM object</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="isRequiredInterface">if set to <c>true</c> interface is an required interface and and exception is thrown on error.</param>
        /// <returns></returns>
        protected T BeginComCall<T>(object parent, string methodName, bool isRequiredInterface) where T : class
        {
            Utils.Trace(Utils.TraceMasks.ExternalSystem, "{0} called.", methodName);

            lock (lock_)
            {
                outstandingCalls_++;

                if (parent == null)
                {
                    if (isRequiredInterface)
                    {
                        throw new NotConnectedException();
                    }
                }

                var comObject = parent as T;

                if (comObject == null)
                {
                    if (isRequiredInterface)
                    {
                        throw new NotSupportedException(Utils.Format("OPC Interface '{0}' is a required interface but not supported by the server.", typeof(T).Name));
                    }
                    else
                    {
                        Utils.Trace(Utils.TraceMasks.ExternalSystem, "OPC Interface '{0}' is not supported by server but it is only an optional one.", typeof(T).Name);
                    }
                }

                DCOMCallWatchdog.Set();

                return comObject;
            }
        }

        /// <summary>
        /// Must called if a COM call returns an unexpected exception.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="e">The exception.</param>
        /// <remarks>Note that some COM calls are expected to return errors.</remarks>
        protected void ComCallError(string methodName, Exception e)
        {
            SafeNativeMethods.TraceComError(e, methodName);
        }

        /// <summary>
        /// Must be called in the finally block after making a COM call.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        protected void EndComCall(string methodName)
        {
            Utils.Trace(Utils.TraceMasks.ExternalSystem, "{0} completed.", methodName);

            lock (lock_)
            {
                outstandingCalls_--;

                DCOMCallWatchdog.Reset();
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Establishes a connection point callback with the COM server.
        /// </summary>
        private void Advise()
        {
            if (connection_ == null)
            {
                connection_ = new ConnectionPoint(server_, typeof(IOPCShutdown).GUID);
                connection_.Advise(callback_);
            }
        }

        /// <summary>
        /// Closes a connection point callback with the COM server.
        /// </summary>
        private void Unadvise()
        {
            if (connection_ != null)
            {
                if (connection_.Unadvise() == 0)
                {
                    connection_.Dispose();
                    connection_ = null;
                }
            }
        }

        /// <summary>
        /// A class that implements the IOPCShutdown interface.
        /// </summary>
        private class Callback : IOPCShutdown
        {
            /// <summary>
            /// Initializes the object with the containing subscription object.
            /// </summary>
            public Callback(Server server)
            {
                m_server = server;
            }

            /// <summary>
            /// An event to receive server shutdown notificiations.
            /// </summary>
            public event OpcServerShutdownEventHandler ServerShutdown
            {
                add { lock (lock_) { m_serverShutdown += value; } }
                remove { lock (lock_) { m_serverShutdown -= value; } }
            }

            /// <summary>
            /// A table of item identifiers indexed by internal handle.
            /// </summary>
            private Server m_server = null;

            /// <summary>
            /// Raised when data changed callbacks arrive.
            /// </summary>
            private event OpcServerShutdownEventHandler m_serverShutdown = null;

            /// <summary>
            /// Called when a shutdown event is received.
            /// </summary>
            public void ShutdownRequest(string reason)
            {
                try
                {
                    lock (lock_)
                    {
                        if (m_serverShutdown != null)
                        {
                            m_serverShutdown(reason);
                        }
                    }
                }
                catch (Exception e)
                {
                    var stack = e.StackTrace;
                }
            }
        }

        #endregion
    }
}
