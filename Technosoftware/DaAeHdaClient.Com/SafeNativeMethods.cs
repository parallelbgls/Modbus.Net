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
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using Technosoftware.DaAeHdaClient.Utilities;
#endregion

#pragma warning disable 618

namespace Technosoftware.DaAeHdaClient.Com
{
    /// <summary>
    /// Exposes WIN32 and COM API functions.
    /// </summary>
    [SuppressUnmanagedCodeSecurityAttribute]
    internal static class SafeNativeMethods
    {
        #region NetApi Function Declarations
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct SERVER_INFO_100
        {
            public uint sv100_platform_id;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string sv100_name;
        }

        internal const uint LEVEL_SERVER_INFO_100 = 100;
        internal const uint LEVEL_SERVER_INFO_101 = 101;

        internal const int MAX_PREFERRED_LENGTH = -1;

        internal const uint SV_TYPE_WORKSTATION = 0x00000001;
        internal const uint SV_TYPE_SERVER = 0x00000002;

        [DllImport("Netapi32.dll")]
        internal static extern int NetServerEnum(
            IntPtr servername,
            uint level,
            out IntPtr bufptr,
            int prefmaxlen,
            out int entriesread,
            out int totalentries,
            uint servertype,
            IntPtr domain,
            IntPtr resume_handle);

        [DllImport("Netapi32.dll")]
        internal static extern int NetApiBufferFree(IntPtr buffer);

        /// <summary>
        /// Enumerates computers on the local network.
        /// </summary>
        public static List<string> EnumComputers()
        {
            IntPtr pInfo;
            int entriesRead;
            int totalEntries;
            var result = NetServerEnum(
                IntPtr.Zero,
                LEVEL_SERVER_INFO_100,
                out pInfo,
                MAX_PREFERRED_LENGTH,
                out entriesRead,
                out totalEntries,
                SV_TYPE_WORKSTATION | SV_TYPE_SERVER,
                IntPtr.Zero,
                IntPtr.Zero);

            if (result != 0)
            {
                throw new ApplicationException("NetApi Error = " + string.Format("0x{0:X8}", result));
            }

            var computers = new List<string>();

            var pos = pInfo;

            for (var ii = 0; ii < entriesRead; ii++)
            {
                var info = (SERVER_INFO_100)Marshal.PtrToStructure(pos, typeof(SERVER_INFO_100));

                computers.Add(info.sv100_name);

                pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(SERVER_INFO_100)));
            }

            NetApiBufferFree(pInfo);

            return computers;
        }
        #endregion

        #region OLE32 Function/Interface Declarations
        internal const int MAX_MESSAGE_LENGTH = 1024;

        internal const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        internal const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;

        [DllImport("Kernel32.dll")]
        internal static extern int FormatMessageW(
            int dwFlags,
            IntPtr lpSource,
            int dwMessageId,
            int dwLanguageId,
            IntPtr lpBuffer,
            int nSize,
            IntPtr Arguments);

        [DllImport("Kernel32.dll")]
        internal static extern int GetSystemDefaultLangID();

        [DllImport("Kernel32.dll")]
        internal static extern int GetUserDefaultLangID();

        /// <summary>
        /// The WIN32 system default locale.
        /// </summary>
        public const int LOCALE_SYSTEM_DEFAULT = 0x800;

        /// <summary>
        /// The WIN32 user default locale.
        /// </summary>
        public const int LOCALE_USER_DEFAULT = 0x400;

        /// <summary>
        /// The base for the WIN32 FILETIME structure.
        /// </summary>
        internal static readonly DateTime FILETIME_BaseTime = new DateTime(1601, 1, 1);

        /// <summary>
        /// WIN32 GUID struct declaration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct GUID
        {
            public int Data1;
            public short Data2;
            public short Data3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data4;
        }

        /// <summary>
        /// The size, in bytes, of a VARIANT structure.
        /// </summary>
        internal static int VARIANT_SIZE => (IntPtr.Size > 4) ? 0x18 : 0x10;

        [DllImport("OleAut32.dll")]
        internal static extern int VariantChangeTypeEx(
            IntPtr pvargDest,
            IntPtr pvarSrc,
            int lcid,
            ushort wFlags,
            short vt);

        /// <summary>
        /// Intializes a pointer to a VARIANT.
        /// </summary>
        [DllImport("oleaut32.dll")]
        internal static extern void VariantInit(IntPtr pVariant);

        /// <summary>
        /// Frees all memory referenced by a VARIANT stored in unmanaged memory.
        /// </summary>
        [DllImport("oleaut32.dll")]
        internal static extern int VariantClear(IntPtr pVariant);

        internal const int DISP_E_TYPEMISMATCH = -0x7FFDFFFB; // 0x80020005
        internal const int DISP_E_OVERFLOW = -0x7FFDFFF6; // 0x8002000A

        internal const int VARIANT_NOVALUEPROP = 0x01;
        internal const int VARIANT_ALPHABOOL = 0x02; // For VT_BOOL to VT_BSTR conversions convert to "True"/"False" instead of

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct SOLE_AUTHENTICATION_SERVICE
        {
            public uint dwAuthnSvc;
            public uint dwAuthzSvc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pPrincipalName;
            public int hr;
        }

        internal const uint RPC_C_AUTHN_NONE = 0;
        internal const uint RPC_C_AUTHN_DCE_PRIVATE = 1;
        internal const uint RPC_C_AUTHN_DCE_PUBLIC = 2;
        internal const uint RPC_C_AUTHN_DEC_PUBLIC = 4;
        internal const uint RPC_C_AUTHN_GSS_NEGOTIATE = 9;
        internal const uint RPC_C_AUTHN_WINNT = 10;
        internal const uint RPC_C_AUTHN_GSS_SCHANNEL = 14;
        internal const uint RPC_C_AUTHN_GSS_KERBEROS = 16;
        internal const uint RPC_C_AUTHN_DPA = 17;
        internal const uint RPC_C_AUTHN_MSN = 18;
        internal const uint RPC_C_AUTHN_DIGEST = 21;
        internal const uint RPC_C_AUTHN_MQ = 100;
        internal const uint RPC_C_AUTHN_DEFAULT = 0xFFFFFFFF;

        internal const uint RPC_C_AUTHZ_NONE = 0;
        internal const uint RPC_C_AUTHZ_NAME = 1;
        internal const uint RPC_C_AUTHZ_DCE = 2;
        internal const uint RPC_C_AUTHZ_DEFAULT = 0xffffffff;

        internal const uint RPC_C_AUTHN_LEVEL_DEFAULT = 0;
        internal const uint RPC_C_AUTHN_LEVEL_NONE = 1;
        internal const uint RPC_C_AUTHN_LEVEL_CONNECT = 2;
        internal const uint RPC_C_AUTHN_LEVEL_CALL = 3;
        internal const uint RPC_C_AUTHN_LEVEL_PKT = 4;
        internal const uint RPC_C_AUTHN_LEVEL_PKT_INTEGRITY = 5;
        internal const uint RPC_C_AUTHN_LEVEL_PKT_PRIVACY = 6;

        internal const uint RPC_C_IMP_LEVEL_ANONYMOUS = 1;
        internal const uint RPC_C_IMP_LEVEL_IDENTIFY = 2;
        internal const uint RPC_C_IMP_LEVEL_IMPERSONATE = 3;
        internal const uint RPC_C_IMP_LEVEL_DELEGATE = 4;

        internal const uint EOAC_NONE = 0x00;
        internal const uint EOAC_MUTUAL_AUTH = 0x01;
        internal const uint EOAC_CLOAKING = 0x10;
        internal const uint EOAC_STATIC_CLOAKING = 0x20;
        internal const uint EOAC_DYNAMIC_CLOAKING = 0x40;
        internal const uint EOAC_SECURE_REFS = 0x02;
        internal const uint EOAC_ACCESS_CONTROL = 0x04;
        internal const uint EOAC_APPID = 0x08;

        [DllImport("ole32.dll")]
        internal static extern int CoInitializeSecurity(
            IntPtr pSecDesc,
            int cAuthSvc,
            SOLE_AUTHENTICATION_SERVICE[] asAuthSvc,
            IntPtr pReserved1,
            uint dwAuthnLevel,
            uint dwImpLevel,
            IntPtr pAuthList,
            uint dwCapabilities,
            IntPtr pReserved3);

        [DllImport("ole32.dll")]
        private static extern int CoQueryProxyBlanket(
            [MarshalAs(UnmanagedType.IUnknown)]
            object pProxy,
            ref uint pAuthnSvc,
            ref uint pAuthzSvc,
            [MarshalAs(UnmanagedType.LPWStr)]
            ref string pServerPrincName,
            ref uint pAuthnLevel,
            ref uint pImpLevel,
            ref IntPtr pAuthInfo,
            ref uint pCapabilities);

        [DllImport("ole32.dll")]
        private static extern int CoSetProxyBlanket(
            [MarshalAs(UnmanagedType.IUnknown)]
            object pProxy,
            uint pAuthnSvc,
            uint pAuthzSvc,
            IntPtr pServerPrincName,
            uint pAuthnLevel,
            uint pImpLevel,
            IntPtr pAuthInfo,
            uint pCapabilities);

        private static readonly IntPtr COLE_DEFAULT_PRINCIPAL = new IntPtr(-1);
        private static readonly IntPtr COLE_DEFAULT_AUTHINFO = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct COSERVERINFO
        {
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pwszName;
            public IntPtr pAuthInfo;
            public uint dwReserved2;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct COAUTHINFO
        {
            public uint dwAuthnSvc;
            public uint dwAuthzSvc;
            public IntPtr pwszServerPrincName;
            public uint dwAuthnLevel;
            public uint dwImpersonationLevel;
            public IntPtr pAuthIdentityData;
            public uint dwCapabilities;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct COAUTHIDENTITY
        {
            public IntPtr User;
            public uint UserLength;
            public IntPtr Domain;
            public uint DomainLength;
            public IntPtr Password;
            public uint PasswordLength;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MULTI_QI
        {
            public IntPtr iid;
            [MarshalAs(UnmanagedType.IUnknown)]
            public object pItf;
            public uint hr;
        }

        internal const uint CLSCTX_INPROC_SERVER = 0x1;
        internal const uint CLSCTX_INPROC_HANDLER = 0x2;
        internal const uint CLSCTX_LOCAL_SERVER = 0x4;
        internal const uint CLSCTX_REMOTE_SERVER = 0x10;
        internal const uint CLSCTX_DISABLE_AAA = 0x8000;

        private static readonly Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

        internal const uint SEC_WINNT_AUTH_IDENTITY_ANSI = 0x1;
        internal const uint SEC_WINNT_AUTH_IDENTITY_UNICODE = 0x2;

        [DllImport("ole32.dll")]
        private static extern int CoCreateInstanceEx(
            ref Guid clsid,
            [MarshalAs(UnmanagedType.IUnknown)]
            object punkOuter,
            uint dwClsCtx,
            [In]
            ref COSERVERINFO pServerInfo,
            uint dwCount,
            [In, Out]
            MULTI_QI[] pResults);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct LICINFO
        {
            public int cbLicInfo;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fRuntimeKeyAvail;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fLicVerified;
        }

        [ComImport]
        [GuidAttribute("00000001-0000-0000-C000-000000000046")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IClassFactory
        {
            void CreateInstance(
                [MarshalAs(UnmanagedType.IUnknown)]
                object punkOuter,
                [MarshalAs(UnmanagedType.LPStruct)]
                Guid riid,
                [MarshalAs(UnmanagedType.Interface)]
                [Out] out object ppvObject);

            void LockServer(
                [MarshalAs(UnmanagedType.Bool)]
                bool fLock);
        }

        [ComImport]
        [GuidAttribute("B196B28F-BAB4-101A-B69C-00AA00341D07")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IClassFactory2
        {
            void CreateInstance(
                [MarshalAs(UnmanagedType.IUnknown)]
                object punkOuter,
                [MarshalAs(UnmanagedType.LPStruct)]
                Guid riid,
                [MarshalAs(UnmanagedType.Interface)]
                [Out] out object ppvObject);

            void LockServer(
                [MarshalAs(UnmanagedType.Bool)]
                bool fLock);

            void GetLicInfo(
                [In, Out] ref LICINFO pLicInfo);

            void RequestLicKey(
                int dwReserved,
                [MarshalAs(UnmanagedType.BStr)]
                string pbstrKey);

            void CreateInstanceLic(
                [MarshalAs(UnmanagedType.IUnknown)]
                object punkOuter,
                [MarshalAs(UnmanagedType.IUnknown)]
                object punkReserved,
                [MarshalAs(UnmanagedType.LPStruct)]
                Guid riid,
                [MarshalAs(UnmanagedType.BStr)]
                string bstrKey,
                [MarshalAs(UnmanagedType.IUnknown)]
                [Out] out object ppvObject);
        }

        [DllImport("ole32.dll")]
        private static extern int CoGetClassObject(
            [MarshalAs(UnmanagedType.LPStruct)]
            Guid clsid,
            uint dwClsContext,
            [In] ref COSERVERINFO pServerInfo,
            [MarshalAs(UnmanagedType.LPStruct)]
            Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)]
            [Out] out object ppv);

        internal const int LOGON32_PROVIDER_DEFAULT = 0;
        internal const int LOGON32_LOGON_INTERACTIVE = 2;
        internal const int LOGON32_LOGON_NETWORK = 3;

        internal const int SECURITY_ANONYMOUS = 0;
        internal const int SECURITY_IDENTIFICATION = 1;
        internal const int SECURITY_IMPERSONATION = 2;
        internal const int SECURITY_DELEGATION = 3;

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private extern static bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private extern static bool DuplicateToken(
            IntPtr ExistingTokenHandle,
            int SECURITY_IMPERSONATION_LEVEL,
            ref IntPtr DuplicateTokenHandle);
        #endregion

        #region ServerInfo Class
        /// <summary>
        /// A class used to allocate and deallocate the elements of a COSERVERINFO structure.
        /// </summary>
        class ServerInfo
        {
            #region Public Interface
            /// <summary>
            /// Allocates a COSERVERINFO structure.
            /// </summary>
            public COSERVERINFO Allocate(string hostName, OpcUserIdentity identity)
            {
                // initialize server info structure.
                var serverInfo = new COSERVERINFO();

                serverInfo.pwszName = hostName;
                serverInfo.pAuthInfo = IntPtr.Zero;
                serverInfo.dwReserved1 = 0;
                serverInfo.dwReserved2 = 0;

                // no authentication for default identity
                if (OpcUserIdentity.IsDefault(identity))
                {
                    return serverInfo;
                }

                m_hUserName = GCHandle.Alloc(identity.Username, GCHandleType.Pinned);
                m_hPassword = GCHandle.Alloc(identity.Password, GCHandleType.Pinned);
                m_hDomain = GCHandle.Alloc(identity.Domain, GCHandleType.Pinned);

                m_hIdentity = new GCHandle();

                // create identity structure.
                var authIdentity = new COAUTHIDENTITY();

                authIdentity.User = m_hUserName.AddrOfPinnedObject();
                authIdentity.UserLength = (uint)((identity.Username != null) ? identity.Username.Length : 0);
                authIdentity.Password = m_hPassword.AddrOfPinnedObject();
                authIdentity.PasswordLength = (uint)((identity.Password != null) ? identity.Password.Length : 0);
                authIdentity.Domain = m_hDomain.AddrOfPinnedObject();
                authIdentity.DomainLength = (uint)((identity.Domain != null) ? identity.Domain.Length : 0);
                authIdentity.Flags = SEC_WINNT_AUTH_IDENTITY_UNICODE;

                m_hIdentity = GCHandle.Alloc(authIdentity, GCHandleType.Pinned);

                // create authorization info structure.
                var authInfo = new COAUTHINFO();

                authInfo.dwAuthnSvc = RPC_C_AUTHN_WINNT;
                authInfo.dwAuthzSvc = RPC_C_AUTHZ_NONE;
                authInfo.pwszServerPrincName = IntPtr.Zero;
                authInfo.dwAuthnLevel = RPC_C_AUTHN_LEVEL_CONNECT;
                authInfo.dwImpersonationLevel = RPC_C_IMP_LEVEL_IMPERSONATE;
                authInfo.pAuthIdentityData = m_hIdentity.AddrOfPinnedObject();
                authInfo.dwCapabilities = EOAC_NONE; // EOAC_DYNAMIC_CLOAKING;

                m_hAuthInfo = GCHandle.Alloc(authInfo, GCHandleType.Pinned);

                // update server info structure.
                serverInfo.pAuthInfo = m_hAuthInfo.AddrOfPinnedObject();

                return serverInfo;
            }

            /// <summary>
            /// Deallocated memory allocated when the COSERVERINFO structure was created.
            /// </summary>
            public void Deallocate()
            {
                if (m_hUserName.IsAllocated) m_hUserName.Free();
                if (m_hPassword.IsAllocated) m_hPassword.Free();
                if (m_hDomain.IsAllocated) m_hDomain.Free();
                if (m_hIdentity.IsAllocated) m_hIdentity.Free();
                if (m_hAuthInfo.IsAllocated) m_hAuthInfo.Free();
            }
            #endregion

            #region Private Members
            private GCHandle m_hUserName;
            private GCHandle m_hPassword;
            private GCHandle m_hDomain;
            private GCHandle m_hIdentity;
            private GCHandle m_hAuthInfo;
            #endregion
        }
        #endregion

        #region Initialization Functions
        /// <summary>
        /// Initializes COM security.
        /// </summary>
        public static void InitializeSecurity()
        {
            var error = CoInitializeSecurity(
                IntPtr.Zero,
                -1,
                null,
                IntPtr.Zero,
                RPC_C_AUTHN_LEVEL_CONNECT,
                RPC_C_IMP_LEVEL_IMPERSONATE,
                IntPtr.Zero,
                EOAC_DYNAMIC_CLOAKING,
                IntPtr.Zero);

            // this call will fail in the debugger if the 
            // 'Debug | Enable Visual Studio Hosting Process'  
            // option is checked in the project properties. 
            if (error != 0)
            {
                // throw new ExternalException("CoInitializeSecurity: " + GetSystemMessage(error), error);
            }
        }

        /// <summary>
        /// Determines if the host is the local host.
        /// </summary>
        private static bool IsLocalHost(string hostName)
        {
            // lookup requested host.
            var requestedHost = Dns.GetHostEntry(hostName);

            if (requestedHost == null || requestedHost.AddressList == null)
            {
                return true;
            }

            // check for loopback.
            for (var ii = 0; ii < requestedHost.AddressList.Length; ii++)
            {
                var requestedIP = requestedHost.AddressList[ii];

                if (requestedIP == null || requestedIP.Equals(IPAddress.Loopback))
                {
                    return true;
                }
            }

            // lookup local host.
            var localHost = Dns.GetHostEntry(Dns.GetHostName());

            if (localHost == null || localHost.AddressList == null)
            {
                return false;
            }

            // check for localhost.
            for (var ii = 0; ii < requestedHost.AddressList.Length; ii++)
            {
                var requestedIP = requestedHost.AddressList[ii];

                for (var jj = 0; jj < localHost.AddressList.Length; jj++)
                {
                    if (requestedIP.Equals(localHost.AddressList[jj]))
                    {
                        return true;
                    }
                }
            }

            // must be remote.
            return false;
        }

        // COM impersonation is a nice feature but variations between behavoirs on different
        // windows platforms make it virtually impossible to support. This code is left here 
        // in case it becomes a critical requirement in the future.
#if COM_IMPERSONATION_SUPPORT
        /// <summary>
        /// Returns the WindowsIdentity associated with a UserIdentity.
        /// </summary>
        public static WindowsPrincipal GetPrincipalFromUserIdentity(UserIdentity user)
        {
            if (UserIdentity.IsDefault(user))
            {
                return null;
            }

            // validate the credentials.
		    IntPtr token = IntPtr.Zero;

		    bool result = LogonUser(
                user.Username,
                user.Domain,
                user.Password, 
			    LOGON32_LOGON_NETWORK, 
			    LOGON32_PROVIDER_DEFAULT,
			    ref token);

		    if (!result)
		    {                    
                throw ServiceResultException.Create(
                    StatusCodes.BadIdentityTokenRejected, 
                    "Could not logon as user '{0}'. Reason: {1}.", 
                    user.Username, 
                    GetSystemMessage(Marshal.GetLastWin32Error(), LOCALE_SYSTEM_DEFAULT));
		    }
            
            try
            {
                // create the windows identity.
                WindowsIdentity identity = new WindowsIdentity(token);

                // validate the identity.
                identity.Impersonate();

                // return a principal.
                return new WindowsPrincipal(identity);
            }
            finally
            {                    
			    CloseHandle(token);
            }
        }

        /// <summary>
        /// Sets the security settings for the proxy.
        /// </summary>
        public static void SetProxySecurity(object server, UserIdentity user)
        {
            // allocate the 
            GCHandle hUserName = GCHandle.Alloc(user.Username, GCHandleType.Pinned);
            GCHandle hPassword = GCHandle.Alloc(user.Password, GCHandleType.Pinned);
            GCHandle hDomain   = GCHandle.Alloc(user.Domain,   GCHandleType.Pinned);

            GCHandle hIdentity = new GCHandle();

            // create identity structure.
            COAUTHIDENTITY authIdentity = new COAUTHIDENTITY();

            authIdentity.User           = hUserName.AddrOfPinnedObject();
            authIdentity.UserLength     = (uint)((user.Username != null) ? user.Username.Length : 0);
            authIdentity.Password       = hPassword.AddrOfPinnedObject();
            authIdentity.PasswordLength = (uint)((user.Password != null) ? user.Password.Length : 0);
            authIdentity.Domain         = hDomain.AddrOfPinnedObject();
            authIdentity.DomainLength   = (uint)((user.Domain != null) ? user.Domain.Length : 0);
            authIdentity.Flags          = SEC_WINNT_AUTH_IDENTITY_UNICODE;

            hIdentity = GCHandle.Alloc(authIdentity, GCHandleType.Pinned);
            
            try
            {
                SetProxySecurity(server, hIdentity.AddrOfPinnedObject());
            }
            finally
            {
                hUserName.Free();
                hPassword.Free();
                hDomain.Free();
                hIdentity.Free();
            }
        }

        /// <summary>
        /// Sets the security settings for the proxy.
        /// </summary>
        public static void SetProxySecurity(object server, IntPtr pAuthInfo)
        {
            // get the existing proxy settings.
            uint pAuthnSvc = 0;
            uint pAuthzSvc = 0;
            string pServerPrincName = "";
            uint pAuthnLevel = 0;
            uint pImpLevel = 0;
            IntPtr pAuthInfo2 = IntPtr.Zero;
            uint pCapabilities = 0;

            CoQueryProxyBlanket(
                server,
                ref pAuthnSvc,
                ref pAuthzSvc,
                ref pServerPrincName,
                ref pAuthnLevel,
                ref pImpLevel,
                ref pAuthInfo2,
                ref pCapabilities);

            pAuthnSvc = RPC_C_AUTHN_WINNT;
            pAuthzSvc = RPC_C_AUTHZ_NONE;
            pAuthnLevel = RPC_C_AUTHN_LEVEL_CONNECT;
            pImpLevel = RPC_C_IMP_LEVEL_IMPERSONATE;
            pCapabilities = EOAC_DYNAMIC_CLOAKING;
            
            // update proxy security settings.
            CoSetProxyBlanket(
                server,
                pAuthnSvc,
                pAuthzSvc,
                COLE_DEFAULT_PRINCIPAL,
                pAuthnLevel,
                pImpLevel,
                pAuthInfo,
                pCapabilities);
        }
		
		/// <summary>
		/// Creates an instance of a COM server using the specified license key.
		/// </summary>
		public static object CreateInstance2(Guid clsid, string hostName, UserIdentity identity)
		{
            // validate the host name before proceeding (exception thrown if host is not valid).
            bool isLocalHost = IsLocalHost(hostName);

            // allocate the connection info.
			ServerInfo    serverInfo   = new ServerInfo();
			COSERVERINFO  coserverInfo = serverInfo.Allocate(hostName, identity);
			object        instance     = null; 
			IClassFactory factory      = null; 
			
			try
			{
                // create the factory.
                object server = null;

				CoGetClassObject(
					clsid,
					(isLocalHost)?CLSCTX_LOCAL_SERVER:CLSCTX_REMOTE_SERVER,
					ref coserverInfo,
					IID_IUnknown,
					out server);

                // SetProxySecurity(server, coserverInfo.pAuthInfo);
                
                factory = (IClassFactory)server;

                // check for valid factory.
                if (factory == null)
                {
                    throw ServiceResultException.Create(StatusCodes.BadCommunicationError, "Could not load IClassFactory for COM server '{0}' on host '{1}'.", clsid, hostName);
                }

                // SetProxySecurity(factory, coserverInfo.pAuthInfo);

			    factory.CreateInstance(null, IID_IUnknown, out instance);

                // SetProxySecurity(instance, coserverInfo.pAuthInfo);
			}
			finally
			{
				serverInfo.Deallocate();
			}

            return instance;
		}	

		/// <summary>
		/// Creates an instance of a COM server using the specified license key.
		/// </summary>
		public static object CreateInstanceWithLicenseKey(Guid clsid, string hostName, UserIdentity identity, string licenseKey)
		{
			ServerInfo     serverInfo   = new ServerInfo();
			COSERVERINFO   coserverInfo = serverInfo.Allocate(hostName, identity);
			object         instance     = null; 
			IClassFactory2 factory      = null; 

			try
			{
				// check whether connecting locally or remotely.
				uint clsctx = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER;

				if (hostName != null && hostName.Length > 0)
				{
					clsctx = CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER;
				}

				// get the class factory.
				object server = null;

				CoGetClassObject(
					clsid,
					clsctx,
					ref coserverInfo,
					typeof(IClassFactory2).GUID,
					out server);

                // SetProxySecurity(server, coserverInfo.pAuthInfo);
                
                factory = (IClassFactory2)server;

                // check for valid factory.
                if (factory == null)
                {
                    throw ServiceResultException.Create(StatusCodes.BadCommunicationError, "Could not load IClassFactory2 for COM server '{0}' on host '{1}'.", clsid, hostName);
                }

                // SetProxySecurity(factory, coserverInfo.pAuthInfo);

				// create instance.
				factory.CreateInstanceLic(
					null,
					null,
					IID_IUnknown,
					licenseKey,
                    out instance);

                // SetProxySecurity(instance, coserverInfo.pAuthInfo);
			}
			finally
			{
				serverInfo.Deallocate();
                SafeNativeMethods.ReleaseServer(factory);
			}
			  
			return instance;
		}	
#endif
        #endregion

        #region Conversion Functions

        /// <summary>
        /// Tests if the specified string matches the specified pattern.
        /// </summary>
        public static bool Match(string target, string pattern, bool caseSensitive)
        {
            // an empty pattern always matches.
            if (pattern == null || pattern.Length == 0)
            {
                return true;
            }

            // an empty string never matches.
            if (target == null || target.Length == 0)
            {
                return false;
            }

            // check for exact match
            if (caseSensitive)
            {
                if (target == pattern)
                {
                    return true;
                }
            }
            else
            {
                if (target.ToLower() == pattern.ToLower())
                {
                    return true;
                }
            }

            char c;
            char p;
            char l;

            var pIndex = 0;
            var tIndex = 0;

            while (tIndex < target.Length && pIndex < pattern.Length)
            {
                p = ConvertCase(pattern[pIndex++], caseSensitive);

                if (pIndex > pattern.Length)
                {
                    return (tIndex >= target.Length); // if end of string true
                }

                switch (p)
                {
                    // match zero or more char.
                    case '*':
                        {
                            while (pIndex < pattern.Length && pattern[pIndex] == '*')
                            {
                                pIndex++;
                            }

                            while (tIndex < target.Length)
                            {
                                if (Match(target.Substring(tIndex++), pattern.Substring(pIndex), caseSensitive))
                                {
                                    return true;
                                }
                            }

                            return Match(target, pattern.Substring(pIndex), caseSensitive);
                        }

                    // match any one char.
                    case '?':
                        {
                            // check if end of string when looking for a single character.
                            if (tIndex >= target.Length)
                            {
                                return false;
                            }

                            // check if end of pattern and still string data left.
                            if (pIndex >= pattern.Length && tIndex < target.Length - 1)
                            {
                                return false;
                            }

                            tIndex++;
                            break;
                        }

                    // match char set 
                    case '[':
                        {
                            c = ConvertCase(target[tIndex++], caseSensitive);

                            if (tIndex > target.Length)
                            {
                                return false; // syntax 
                            }

                            l = '\0';

                            // match a char if NOT in set []
                            if (pattern[pIndex] == '!')
                            {
                                ++pIndex;

                                p = ConvertCase(pattern[pIndex++], caseSensitive);

                                while (pIndex < pattern.Length)
                                {
                                    if (p == ']') // if end of char set, then 
                                    {
                                        break; // no match found 
                                    }

                                    if (p == '-')
                                    {
                                        // check a range of chars? 
                                        p = ConvertCase(pattern[pIndex], caseSensitive);

                                        // get high limit of range 
                                        if (pIndex > pattern.Length || p == ']')
                                        {
                                            return false; // syntax 
                                        }

                                        if (c >= l && c <= p)
                                        {
                                            return false; // if in range, return false
                                        }
                                    }

                                    l = p;

                                    if (c == p) // if char matches this element 
                                    {
                                        return false; // return false 
                                    }

                                    p = ConvertCase(pattern[pIndex++], caseSensitive);
                                }
                            }

                            // match if char is in set []
                            else
                            {
                                p = ConvertCase(pattern[pIndex++], caseSensitive);

                                while (pIndex < pattern.Length)
                                {
                                    if (p == ']') // if end of char set, then no match found 
                                    {
                                        return false;
                                    }

                                    if (p == '-')
                                    {
                                        // check a range of chars? 
                                        p = ConvertCase(pattern[pIndex], caseSensitive);

                                        // get high limit of range 
                                        if (pIndex > pattern.Length || p == ']')
                                        {
                                            return false; // syntax 
                                        }

                                        if (c >= l && c <= p)
                                        {
                                            break; // if in range, move on 
                                        }
                                    }

                                    l = p;

                                    if (c == p) // if char matches this element move on 
                                    {
                                        break;
                                    }

                                    p = ConvertCase(pattern[pIndex++], caseSensitive);
                                }

                                while (pIndex < pattern.Length && p != ']') // got a match in char set skip to end of set
                                {
                                    p = pattern[pIndex++];
                                }
                            }

                            break;
                        }

                    // match digit.
                    case '#':
                        {
                            c = target[tIndex++];

                            if (!char.IsDigit(c))
                            {
                                return false; // not a digit
                            }

                            break;
                        }

                    // match exact char.
                    default:
                        {
                            c = ConvertCase(target[tIndex++], caseSensitive);

                            if (c != p) // check for exact char
                            {
                                return false; // not a match
                            }

                            // check if end of pattern and still string data left.
                            if (pIndex >= pattern.Length && tIndex < target.Length - 1)
                            {
                                return false;
                            }

                            break;
                        }
                }
            }

            return true;
        }

        // ConvertCase
        private static char ConvertCase(char c, bool caseSensitive)
        {
            return (caseSensitive) ? c : char.ToUpper(c);
        }

        /// <summary>
        /// Unmarshals and frees an array of HRESULTs.
        /// </summary>
        public static int[] GetStatusCodes(ref IntPtr pArray, int size, bool deallocate)
        {
            if (pArray == IntPtr.Zero || size <= 0)
            {
                return null;
            }

            // unmarshal HRESULT array.
            var output = new int[size];
            Marshal.Copy(pArray, output, 0, size);

            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pArray);
                pArray = IntPtr.Zero;
            }

            return output;
        }

        /// <summary>
        /// Unmarshals and frees an array of 32 bit integers.
        /// </summary>
        public static int[] GetInt32s(ref IntPtr pArray, int size, bool deallocate)
        {
            if (pArray == IntPtr.Zero || size <= 0)
            {
                return null;
            }

            var array = new int[size];
            Marshal.Copy(pArray, array, 0, size);

            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pArray);
                pArray = IntPtr.Zero;
            }

            return array;
        }

        /// <summary>
        /// Unmarshals and frees an array of 32 bit integers.
        /// </summary>
        public static int[] GetUInt32s(ref IntPtr pArray, int size, bool deallocate)
        {
            if (pArray == IntPtr.Zero || size <= 0)
            {
                return null;
            }

            var array = new int[size];
            Marshal.Copy(pArray, array, 0, size);

            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pArray);
                pArray = IntPtr.Zero;
            }

            return array;
        }


        /// <summary>
        /// Allocates and marshals an array of 32 bit integers.
        /// </summary>
        public static IntPtr GetInt32s(int[] input)
        {
            var output = IntPtr.Zero;

            if (input != null)
            {
                output = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * input.Length);
                Marshal.Copy(input, 0, output, input.Length);
            }

            return output;
        }

        /// <summary>
        /// Unmarshals and frees a array of 16 bit integers.
        /// </summary>
        public static short[] GetInt16s(ref IntPtr pArray, int size, bool deallocate)
        {
            if (pArray == IntPtr.Zero || size <= 0)
            {
                return null;
            }

            var array = new short[size];
            Marshal.Copy(pArray, array, 0, size);

            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pArray);
                pArray = IntPtr.Zero;
            }

            return array;
        }

        /// <summary>
        /// Allocates and marshals an array of 16 bit integers.
        /// </summary>
        public static IntPtr GetInt16s(short[] input)
        {
            var output = IntPtr.Zero;

            if (input != null)
            {
                output = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(short)) * input.Length);
                Marshal.Copy(input, 0, output, input.Length);
            }

            return output;
        }

        /// <summary>
        /// Marshals an array of strings into a unmanaged memory buffer
        /// </summary>
        /// <param name="values">The array of strings to marshal</param>
        /// <returns>The pointer to the unmanaged memory buffer</returns>
        public static IntPtr GetUnicodeStrings(string[] values)
        {
            var size = (values != null) ? values.Length : 0;

            if (size <= 0)
            {
                return IntPtr.Zero;
            }

            var pointers = new int[size];

            for (var ii = 0; ii < size; ii++)
            {
                pointers[ii] = (int)Marshal.StringToCoTaskMemUni(values[ii]);
            }

            var pValues = Marshal.AllocCoTaskMem(values.Length * Marshal.SizeOf(typeof(IntPtr)));
            Marshal.Copy(pointers, 0, pValues, size);

            return pValues;
        }

        /// <summary>
        /// Unmarshals and frees a array of unicode strings.
        /// </summary>
        public static string[] GetUnicodeStrings(ref IntPtr pArray, int size, bool deallocate)
        {
            if (pArray == IntPtr.Zero || size <= 0)
            {
                return null;
            }

            var pointers = new IntPtr[size];
            Marshal.Copy(pArray, pointers, 0, size);

            var strings = new string[size];

            for (var ii = 0; ii < size; ii++)
            {
                var pString = pointers[ii];
                strings[ii] = Marshal.PtrToStringUni(pString);
                if (deallocate) Marshal.FreeCoTaskMem(pString);
            }

            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pArray);
                pArray = IntPtr.Zero;
            }

            return strings;
        }

        /// <summary>
        /// Marshals a DateTime as a WIN32 FILETIME.
        /// </summary>
        /// <param name="datetime">The DateTime object to marshal</param>
        /// <returns>The WIN32 FILETIME</returns>
        public static System.Runtime.InteropServices.ComTypes.FILETIME GetFILETIME(DateTime datetime)
        {
            System.Runtime.InteropServices.ComTypes.FILETIME filetime;

            if (datetime <= FILETIME_BaseTime)
            {
                filetime.dwHighDateTime = 0;
                filetime.dwLowDateTime = 0;
                return filetime;
            }
            // adjust for WIN32 FILETIME base.
            var ticks = datetime.Subtract(new TimeSpan(FILETIME_BaseTime.Ticks)).Ticks;

            filetime.dwHighDateTime = (int)((ticks >> 32) & 0xFFFFFFFF);
            filetime.dwLowDateTime = (int)(ticks & 0xFFFFFFFF);

            return filetime;
        }

        /// <summary>
        /// Unmarshals an array of FILETIMEs
        /// </summary>
        public static System.Runtime.InteropServices.ComTypes.FILETIME[] GetFILETIMEs(ref IntPtr pArray, int size)
        {
            if (pArray == IntPtr.Zero || size <= 0)
            {
                return null;
            }

            var values = new System.Runtime.InteropServices.ComTypes.FILETIME[size];

            var pos = pArray;

            for (var ii = 0; ii < size; ii++)
            {
                try
                {
                    values[ii] = (System.Runtime.InteropServices.ComTypes.FILETIME)Marshal.PtrToStructure(pos, typeof(System.Runtime.InteropServices.ComTypes.FILETIME));
                }
                catch (Exception)
                {
                }

                pos = (IntPtr)(pos.ToInt64() + 8);
            }


            Marshal.FreeCoTaskMem(pArray);
            pArray = IntPtr.Zero;

            return values;
        }

        /// <summary>
        /// Unmarshals a WIN32 FILETIME from a pointer.
        /// </summary>
        /// <param name="pFiletime">A pointer to a FILETIME structure.</param>
        /// <returns>A DateTime object.</returns>
        public static DateTime GetDateTime(IntPtr pFiletime)
        {
            if (pFiletime == IntPtr.Zero)
            {
                return DateTime.MinValue;
            }

            return GetDateTime((System.Runtime.InteropServices.ComTypes.FILETIME)Marshal.PtrToStructure(pFiletime, typeof(System.Runtime.InteropServices.ComTypes.FILETIME)));
        }

        /// <summary>
        /// Unmarshals a WIN32 FILETIME.
        /// </summary>
        public static DateTime GetDateTime(System.Runtime.InteropServices.ComTypes.FILETIME filetime)
        {
            // check for invalid value.
            if (filetime.dwHighDateTime < 0)
            {
                return DateTime.MinValue;
            }

            // convert FILETIME structure to a 64 bit integer.
            var buffer = (long)filetime.dwHighDateTime;

            if (buffer < 0)
            {
                buffer += ((long)uint.MaxValue + 1);
            }

            var ticks = (buffer << 32);

            buffer = (long)filetime.dwLowDateTime;

            if (buffer < 0)
            {
                buffer += ((long)uint.MaxValue + 1);
            }

            ticks += buffer;

            // check for invalid value.
            if (ticks == 0)
            {
                return DateTime.MinValue;
            }

            // adjust for WIN32 FILETIME base.			
            return FILETIME_BaseTime.Add(new TimeSpan(ticks));
        }

        /// <summary>
        /// Marshals an array of DateTimes into an unmanaged array of FILETIMEs
        /// </summary>
        /// <param name="datetimes">The array of DateTimes to marshal</param>
        /// <returns>The IntPtr array of FILETIMEs</returns>
        public static IntPtr GetFILETIMEs(DateTime[] datetimes)
        {
            var count = (datetimes != null) ? datetimes.Length : 0;

            if (count <= 0)
            {
                return IntPtr.Zero;
            }

            var pFiletimes = Marshal.AllocCoTaskMem(count * Marshal.SizeOf(typeof(System.Runtime.InteropServices.ComTypes.FILETIME)));

            var pos = pFiletimes;

            for (var ii = 0; ii < count; ii++)
            {
                Marshal.StructureToPtr(GetFILETIME(datetimes[ii]), pos, false);
                pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(System.Runtime.InteropServices.ComTypes.FILETIME)));
            }

            return pFiletimes;
        }

        /// <summary>
        /// Unmarshals an array of WIN32 FILETIMEs as DateTimes.
        /// </summary>
        public static DateTime[] GetDateTimes(ref IntPtr pArray, int size, bool deallocate)
        {
            if (pArray == IntPtr.Zero || size <= 0)
            {
                return null;
            }

            var datetimes = new DateTime[size];

            var pos = pArray;

            for (var ii = 0; ii < size; ii++)
            {
                datetimes[ii] = GetDateTime(pos);
                pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(System.Runtime.InteropServices.ComTypes.FILETIME)));
            }

            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pArray);
                pArray = IntPtr.Zero;
            }

            return datetimes;
        }

        /// <summary>
        /// Unmarshals an array of WIN32 GUIDs as Guid.
        /// </summary>
        public static Guid[] GetGUIDs(ref IntPtr pInput, int size, bool deallocate)
        {
            if (pInput == IntPtr.Zero || size <= 0)
            {
                return null;
            }

            var guids = new Guid[size];

            var pos = pInput;

            for (var ii = 0; ii < size; ii++)
            {
                var input = (GUID)Marshal.PtrToStructure(pInput, typeof(GUID));

                guids[ii] = new Guid(input.Data1, input.Data2, input.Data3, input.Data4);

                pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(GUID)));
            }

            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pInput);
                pInput = IntPtr.Zero;
            }

            return guids;
        }


        /// <summary>
        /// Converts a LCID to a Locale string.
        /// </summary>
        public static string GetLocale(int input)
        {
            try
            {
                if (input == LOCALE_SYSTEM_DEFAULT || input == LOCALE_USER_DEFAULT || input == 0)
                {
                    return CultureInfo.InvariantCulture.Name;
                }

                return new CultureInfo(input).Name;
            }
            catch (Exception e)
            {
                throw new OpcResultException(OpcResult.E_FAIL, "Unrecognized locale provided.", e);
            }
        }

        /// <summary>
        /// Converts a Locale string to a LCID.
        /// </summary>
        public static int GetLocale(string input)
        {
            // check for the default culture.
            if (input == null || input == "")
            {
                return 0;
            }

            CultureInfo locale;
            try { locale = new CultureInfo(input); }
            catch { locale = CultureInfo.CurrentCulture; }

            return locale.LCID;
        }

        /// <summary>
        /// Converts the VARTYPE to a SystemType
        /// </summary>
        public static Type GetSystemType(short input)
        {
            var isArray = (((short)VarEnum.VT_ARRAY & input) != 0);
            var varType = (VarEnum)Enum.ToObject(typeof(VarEnum), (input & ~(short)VarEnum.VT_ARRAY));

            if (!isArray)
            {
                switch (varType)
                {
                    case VarEnum.VT_I1: return typeof(sbyte);
                    case VarEnum.VT_UI1: return typeof(byte);
                    case VarEnum.VT_I2: return typeof(short);
                    case VarEnum.VT_UI2: return typeof(ushort);
                    case VarEnum.VT_I4: return typeof(int);
                    case VarEnum.VT_UI4: return typeof(uint);
                    case VarEnum.VT_I8: return typeof(long);
                    case VarEnum.VT_UI8: return typeof(ulong);
                    case VarEnum.VT_R4: return typeof(float);
                    case VarEnum.VT_R8: return typeof(double);
                    case VarEnum.VT_BOOL: return typeof(bool);
                    case VarEnum.VT_DATE: return typeof(DateTime);
                    case VarEnum.VT_BSTR: return typeof(string);
                    case VarEnum.VT_CY: return typeof(decimal);
                    case VarEnum.VT_EMPTY: return typeof(object);
                    case VarEnum.VT_VARIANT: return typeof(object);
                }
            }
            else
            {
                switch (varType)
                {
                    case VarEnum.VT_I1: return typeof(sbyte[]);
                    case VarEnum.VT_UI1: return typeof(byte[]);
                    case VarEnum.VT_I2: return typeof(short[]);
                    case VarEnum.VT_UI2: return typeof(ushort[]);
                    case VarEnum.VT_I4: return typeof(int[]);
                    case VarEnum.VT_UI4: return typeof(uint[]);
                    case VarEnum.VT_I8: return typeof(long[]);
                    case VarEnum.VT_UI8: return typeof(ulong[]);
                    case VarEnum.VT_R4: return typeof(float[]);
                    case VarEnum.VT_R8: return typeof(double[]);
                    case VarEnum.VT_BOOL: return typeof(bool[]);
                    case VarEnum.VT_DATE: return typeof(DateTime[]);
                    case VarEnum.VT_BSTR: return typeof(string[]);
                    case VarEnum.VT_CY: return typeof(decimal[]);
                    case VarEnum.VT_EMPTY: return typeof(object[]);
                    case VarEnum.VT_VARIANT: return typeof(object[]);
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the VARTYPE for the value.
        /// </summary>
        public static VarEnum GetVarType(object input)
        {
            if (input == null)
            {
                return VarEnum.VT_EMPTY;
            }

            return GetVarType(input.GetType());
        }

        /// <summary>
        /// Converts the system type to a VARTYPE.
        /// </summary>
        public static VarEnum GetVarType(Type type)
        {
            if (type == null)
            {
                return VarEnum.VT_EMPTY;
            }

            if (type == null) return VarEnum.VT_EMPTY;
            if (type == typeof(sbyte)) return VarEnum.VT_I1;
            if (type == typeof(byte)) return VarEnum.VT_UI1;
            if (type == typeof(short)) return VarEnum.VT_I2;
            if (type == typeof(ushort)) return VarEnum.VT_UI2;
            if (type == typeof(int)) return VarEnum.VT_I4;
            if (type == typeof(uint)) return VarEnum.VT_UI4;
            if (type == typeof(long)) return VarEnum.VT_I8;
            if (type == typeof(ulong)) return VarEnum.VT_UI8;
            if (type == typeof(float)) return VarEnum.VT_R4;
            if (type == typeof(double)) return VarEnum.VT_R8;
            if (type == typeof(decimal)) return VarEnum.VT_CY;
            if (type == typeof(bool)) return VarEnum.VT_BOOL;
            if (type == typeof(DateTime)) return VarEnum.VT_DATE;
            if (type == typeof(string)) return VarEnum.VT_BSTR;
            if (type == typeof(sbyte[])) return VarEnum.VT_ARRAY | VarEnum.VT_I1;
            if (type == typeof(byte[])) return VarEnum.VT_ARRAY | VarEnum.VT_UI1;
            if (type == typeof(short[])) return VarEnum.VT_ARRAY | VarEnum.VT_I2;
            if (type == typeof(ushort[])) return VarEnum.VT_ARRAY | VarEnum.VT_UI2;
            if (type == typeof(int[])) return VarEnum.VT_ARRAY | VarEnum.VT_I4;
            if (type == typeof(uint[])) return VarEnum.VT_ARRAY | VarEnum.VT_UI4;
            if (type == typeof(long[])) return VarEnum.VT_ARRAY | VarEnum.VT_I8;
            if (type == typeof(ulong[])) return VarEnum.VT_ARRAY | VarEnum.VT_UI8;
            if (type == typeof(float[])) return VarEnum.VT_ARRAY | VarEnum.VT_R4;
            if (type == typeof(double[])) return VarEnum.VT_ARRAY | VarEnum.VT_R8;
            if (type == typeof(decimal[])) return VarEnum.VT_ARRAY | VarEnum.VT_CY;
            if (type == typeof(bool[])) return VarEnum.VT_ARRAY | VarEnum.VT_BOOL;
            if (type == typeof(DateTime[])) return VarEnum.VT_ARRAY | VarEnum.VT_DATE;
            if (type == typeof(string[])) return VarEnum.VT_ARRAY | VarEnum.VT_BSTR;
            if (type == typeof(object[])) return VarEnum.VT_ARRAY | VarEnum.VT_VARIANT;

            return VarEnum.VT_EMPTY;
        }

        /// <summary>
        /// Returns true is the object is a valid COM type.
        /// </summary>
        public static bool IsValidComType(object input)
        {
            var array = input as Array;

            if (array != null)
            {
                foreach (var value in array)
                {
                    if (!IsValidComType(value))
                    {
                        return false;
                    }
                }

                return true;
            }

            return GetVarType(input) != VarEnum.VT_EMPTY;
        }

        /// <summary>
        /// Returns the symbolic name for the specified error.
        /// </summary>
        public static string GetErrorText(Type type, int error)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                if (error == (int)field.GetValue(type))
                {
                    return field.Name;
                }
            }

            return string.Format("0x{0:X8}", error);
        }

        /// <summary>
        /// Gets the error code for the exception.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="defaultCode">The default code.</param>
        /// <returns>The error code</returns>
        /// <remarks>This method ignores the exception but makes it possible to keep track of ignored exceptions.</remarks>
        public static int GetErrorCode(Exception e, int defaultCode)
        {
            return defaultCode;
        }

        /// <summary>
        /// Releases the server if it is a true COM server.
        /// </summary>
        public static void ReleaseServer(object server)
        {
            if (server != null && server.GetType().IsCOMObject)
            {
                Marshal.ReleaseComObject(server);
            }
        }

        /// <summary>
        /// Retrieves the system message text for the specified error.
        /// </summary>
        public static string GetSystemMessage(int error, int localeId)
        {
            int langId;
            switch (localeId)
            {
                case LOCALE_SYSTEM_DEFAULT:
                    {
                        langId = GetSystemDefaultLangID();
                        break;
                    }

                case LOCALE_USER_DEFAULT:
                    {
                        langId = GetUserDefaultLangID();
                        break;
                    }

                default:
                    {
                        langId = (0xFFFF & localeId);
                        break;
                    }
            }

            var buffer = Marshal.AllocCoTaskMem(MAX_MESSAGE_LENGTH);

            var result = FormatMessageW(
                (int)FORMAT_MESSAGE_FROM_SYSTEM,
                IntPtr.Zero,
                error,
                langId,
                buffer,
                MAX_MESSAGE_LENGTH - 1,
                IntPtr.Zero);

            if (result > 0)
            {
                var msg = Marshal.PtrToStringUni(buffer);
                Marshal.FreeCoTaskMem(buffer);

                if (msg != null && msg.Length > 0)
                {
                    return msg.Trim();
                }
            }

            return string.Format("0x{0:X8}", error);
        }

        /// <summary>
        /// Converts an exception to an exception that returns a COM error code.
        /// </summary>
        public static Exception CreateComException(Exception e, int errorId)
        {
            return new COMException(e.Message, errorId);
        }

        /// <summary>
        /// Creates a COM exception.
        /// </summary>
        public static Exception CreateComException(string message, int errorId)
        {
            return new COMException(message, errorId);
        }

        /// <summary>
        /// Converts an exception to an exception that returns a COM error code.
        /// </summary>
        public static Exception CreateComException(int errorId)
        {
            return new COMException(string.Format("0x{0:X8}", errorId), errorId);
        }

        /// <summary>
        /// Checks if the error is an RPC error.
        /// </summary>
        public static bool IsRpcError(Exception e)
        {
            var error = Marshal.GetHRForException(e);

            // Assume that any 0x8007 is a fatal communication error.
            // May need to update this check if the assumption proves to be incorrect.
            if ((error & 0xFFFF0000) == 0x80070000)
            {
                error &= 0xFFFF;

                //  check the RPC error range define in WinError.h
                if (error >= 1700 && error < 1918)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the error for the exception is one of the recognized errors.
        /// </summary>
        public static bool IsUnknownError(Exception e, params int[] knownErrors)
        {
            var error = Marshal.GetHRForException(e);

            if (knownErrors != null)
            {
                for (var ii = 0; ii < knownErrors.Length; ii++)
                {
                    if (knownErrors[ii] == error)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        #endregion

        #region Utility Functions
        /// <summary>
        /// Compares a string locale to a WIN32 localeId
        /// </summary>
        public static bool CompareLocales(int localeId, string locale, bool ignoreRegion)
        {
            // parse locale.
            CultureInfo culture;
            try
            {
                culture = new CultureInfo(locale);
            }
            catch (Exception)
            {
                return false;
            }

            // only match the language portion of the locale id.
            if (ignoreRegion)
            {
                if ((localeId & culture.LCID & 0x3FF) == (localeId & 0x3FF))
                {
                    return true;
                }
            }

            // check for exact match.
            else
            {
                if (localeId == culture.LCID)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reports an unexpected exception during a COM operation. 
        /// </summary>
        public static void TraceComError(Exception e, string format, params object[] args)
        {
            var message = Utils.Format(format, args);

            var code = Marshal.GetHRForException(e);

            var error = code.ToString();

            if (error == null)
            {
                Utils.Trace(e, message);
                return;
            }

            Utils.Trace(e, "{0:X}: {1}", error, message);
        }
        #endregion
    }
}
