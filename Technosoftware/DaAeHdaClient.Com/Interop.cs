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
using System.Globalization;
using System.Runtime.InteropServices;

using Technosoftware.DaAeHdaClient.Da;
using Technosoftware.DaAeHdaClient.Com.Utilities;
#endregion

#pragma warning disable 0618

namespace Technosoftware.DaAeHdaClient.Com
{
    /// <summary>
    /// Exposes WIN32 and COM API functions.
    /// </summary>
    public class Interop
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SERVER_INFO_100
        {
            public uint sv100_platform_id;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string sv100_name;
        }

        private const uint LEVEL_SERVER_INFO_100 = 100;
        private const uint LEVEL_SERVER_INFO_101 = 101;

        private const int MAX_PREFERRED_LENGTH = -1;

        private const uint SV_TYPE_WORKSTATION = 0x00000001;
        private const uint SV_TYPE_SERVER = 0x00000002;

        [DllImport("Netapi32.dll")]
        private static extern int NetServerEnum(
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
        private static extern int NetApiBufferFree(IntPtr buffer);

        /// <summary>
        /// Enumerates computers on the local network.
        /// </summary>
        public static string[] EnumComputers()
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
                throw new ApplicationException("NetApi Error = " + string.Format("0x{0,0:X}", result));
            }

            var computers = new string[entriesRead];

            var pos = pInfo;

            for (var ii = 0; ii < entriesRead; ii++)
            {
                var info = (SERVER_INFO_100)Marshal.PtrToStructure(pos, typeof(SERVER_INFO_100));

                computers[ii] = info.sv100_name;

                pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(SERVER_INFO_100)));
            }

            NetApiBufferFree(pInfo);

            return computers;
        }

        private const int MAX_MESSAGE_LENGTH = 1024;

        private const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        private const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;

        [DllImport("Kernel32.dll")]
        private static extern int FormatMessageW(
            int dwFlags,
            IntPtr lpSource,
            int dwMessageId,
            int dwLanguageId,
            IntPtr lpBuffer,
            int nSize,
            IntPtr Arguments);

        /// <summary>
        /// Retrieves the system message text for the specified error.
        /// </summary>
        public static string GetSystemMessage(int error)
        {
            var buffer = Marshal.AllocCoTaskMem(MAX_MESSAGE_LENGTH);

            var result = FormatMessageW(
                (int)(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_FROM_SYSTEM),
                IntPtr.Zero,
                error,
                0,
                buffer,
                MAX_MESSAGE_LENGTH - 1,
                IntPtr.Zero);

            var msg = Marshal.PtrToStringUni(buffer);
            Marshal.FreeCoTaskMem(buffer);

            if (msg != null && msg.Length > 0)
            {
                return msg;
            }

            return string.Format("0x{0,0:X}", error);
        }

        private const int MAX_COMPUTERNAME_LENGTH = 31;

        [DllImport("Kernel32.dll")]
        private static extern int GetComputerNameW(IntPtr lpBuffer, ref int lpnSize);

        /// <summary>
        /// Retrieves the name of the local computer.
        /// </summary>
        public static string GetComputerName()
        {
            string name = null;
            var size = MAX_COMPUTERNAME_LENGTH + 1;

            var pName = Marshal.AllocCoTaskMem(size * 2);

            if (GetComputerNameW(pName, ref size) != 0)
            {
                name = Marshal.PtrToStringUni(pName, size);
            }

            Marshal.FreeCoTaskMem(pName);

            return name;
        }

        #region OLE32 Function/Interface Declarations
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SOLE_AUTHENTICATION_SERVICE
        {
            public uint dwAuthnSvc;
            public uint dwAuthzSvc;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pPrincipalName;
            public int hr;
        }

        private const uint RPC_C_AUTHN_NONE = 0;
        private const uint RPC_C_AUTHN_DCE_PRIVATE = 1;
        private const uint RPC_C_AUTHN_DCE_PUBLIC = 2;
        private const uint RPC_C_AUTHN_DEC_PUBLIC = 4;
        private const uint RPC_C_AUTHN_GSS_NEGOTIATE = 9;
        private const uint RPC_C_AUTHN_WINNT = 10;
        private const uint RPC_C_AUTHN_GSS_SCHANNEL = 14;
        private const uint RPC_C_AUTHN_GSS_KERBEROS = 16;
        private const uint RPC_C_AUTHN_DPA = 17;
        private const uint RPC_C_AUTHN_MSN = 18;
        private const uint RPC_C_AUTHN_DIGEST = 21;
        private const uint RPC_C_AUTHN_MQ = 100;
        private const uint RPC_C_AUTHN_DEFAULT = 0xFFFFFFFF;

        private const uint RPC_C_AUTHZ_NONE = 0;
        private const uint RPC_C_AUTHZ_NAME = 1;
        private const uint RPC_C_AUTHZ_DCE = 2;
        private const uint RPC_C_AUTHZ_DEFAULT = 0xffffffff;

        private const uint RPC_C_AUTHN_LEVEL_DEFAULT = 0;
        private const uint RPC_C_AUTHN_LEVEL_NONE = 1;
        private const uint RPC_C_AUTHN_LEVEL_CONNECT = 2;
        private const uint RPC_C_AUTHN_LEVEL_CALL = 3;
        private const uint RPC_C_AUTHN_LEVEL_PKT = 4;
        private const uint RPC_C_AUTHN_LEVEL_PKT_INTEGRITY = 5;
        private const uint RPC_C_AUTHN_LEVEL_PKT_PRIVACY = 6;

        private const uint RPC_C_IMP_LEVEL_ANONYMOUS = 1;
        private const uint RPC_C_IMP_LEVEL_IDENTIFY = 2;
        private const uint RPC_C_IMP_LEVEL_IMPERSONATE = 3;
        private const uint RPC_C_IMP_LEVEL_DELEGATE = 4;

        private const uint EOAC_NONE = 0x00;
        private const uint EOAC_MUTUAL_AUTH = 0x01;
        private const uint EOAC_CLOAKING = 0x10;
        private const uint EOAC_SECURE_REFS = 0x02;
        private const uint EOAC_ACCESS_CONTROL = 0x04;
        private const uint EOAC_APPID = 0x08;

        private enum COINIT : uint //tagCOINIT
        {
            COINIT_MULTITHREADED = 0x0, //Initializes the thread for multi-threaded object concurrency.
            COINIT_APARTMENTTHREADED = 0x2, //Initializes the thread for apartment-threaded object concurrency
            COINIT_DISABLE_OLE1DDE = 0x4, //Disables DDE for OLE1 support
            COINIT_SPEED_OVER_MEMORY = 0x8, //Trade memory for speed
        }

        /// <returns>If function succeeds, it returns 0(S_OK). Otherwise, it returns an error code.</returns>
        [DllImport("ole32.dll", CharSet = CharSet.Auto, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int CoInitializeEx(
            [In, Optional] IntPtr pvReserved,
            [In] COINIT dwCoInit //DWORD
            );

        /// <returns>If function succeeds, it returns 0(S_OK). Otherwise, it returns an error code.</returns>
        [DllImport("ole32.dll", CharSet = CharSet.Auto, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int CoUnInitialize();

        [DllImport("ole32.dll")]
        private static extern int CoInitializeSecurity(
            IntPtr pSecDesc,
            int cAuthSvc,
            SOLE_AUTHENTICATION_SERVICE[] asAuthSvc,
            IntPtr pReserved1,
            uint dwAuthnLevel,
            uint dwImpLevel,
            IntPtr pAuthList,
            uint dwCapabilities,
            IntPtr pReserved3);

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

        private const uint CLSCTX_INPROC_SERVER = 0x1;
        private const uint CLSCTX_INPROC_HANDLER = 0x2;
        private const uint CLSCTX_LOCAL_SERVER = 0x4;
        private const uint CLSCTX_REMOTE_SERVER = 0x10;

        private static readonly Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

        private const uint SEC_WINNT_AUTH_IDENTITY_ANSI = 0x1;
        private const uint SEC_WINNT_AUTH_IDENTITY_UNICODE = 0x2;

        [DllImport("ole32.dll")]
        private static extern void CoCreateInstanceEx(
            ref Guid clsid,
            [MarshalAs(UnmanagedType.IUnknown)]
            object           punkOuter,
            uint dwClsCtx,
            [In]
            ref COSERVERINFO pServerInfo,
            uint dwCount,
            [In, Out]
            MULTI_QI[]       pResults);

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

        [ComImport]
        [GuidAttribute("0000013D-0000-0000-C000-000000000046")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IClientSecurity
        {
            void QueryBlanket(
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

            void SetBlanket(
                [MarshalAs(UnmanagedType.IUnknown)]
                object pProxy,
                uint pAuthnSvc,
                uint pAuthzSvc,
                [MarshalAs(UnmanagedType.LPWStr)]
                string pServerPrincName,
                uint pAuthnLevel,
                uint pImpLevel,
                IntPtr pAuthInfo,
                uint pCapabilities);

            void CopyProxy(
                [MarshalAs(UnmanagedType.IUnknown)]
                object pProxy,
                [MarshalAs(UnmanagedType.IUnknown)]
                [Out] out object ppCopy);
        }

        [DllImport("ole32.dll")]
        private static extern void CoGetClassObject(
            [MarshalAs(UnmanagedType.LPStruct)]
            Guid clsid,
            uint dwClsContext,
            [In] ref COSERVERINFO pServerInfo,
            [MarshalAs(UnmanagedType.LPStruct)]
            Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)]
            [Out] out object ppv);
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
			public COSERVERINFO Allocate(string hostName, NetworkCredential credential, bool useConnectSecurity = false)
            {
                string userName = null;
                string password = null;
                string domain = null;

                if (credential != null)
                {
                    userName = credential.UserName;
                    password = credential.Password;
                    domain = credential.Domain;
                }

                m_hUserName = GCHandle.Alloc(userName, GCHandleType.Pinned);
                m_hPassword = GCHandle.Alloc(password, GCHandleType.Pinned);
                m_hDomain = GCHandle.Alloc(domain, GCHandleType.Pinned);

                m_hIdentity = new GCHandle();

                if (userName != null && userName != string.Empty)
                {
                    var identity = new COAUTHIDENTITY();

                    identity.User = m_hUserName.AddrOfPinnedObject();
                    identity.UserLength = (uint)((userName != null) ? userName.Length : 0);
                    identity.Password = m_hPassword.AddrOfPinnedObject();
                    identity.PasswordLength = (uint)((password != null) ? password.Length : 0);
                    identity.Domain = m_hDomain.AddrOfPinnedObject();
                    identity.DomainLength = (uint)((domain != null) ? domain.Length : 0);
                    identity.Flags = SEC_WINNT_AUTH_IDENTITY_UNICODE;

                    m_hIdentity = GCHandle.Alloc(identity, GCHandleType.Pinned);
                }

                var authInfo = new COAUTHINFO();
                authInfo.dwAuthnSvc = RPC_C_AUTHN_WINNT;
                authInfo.dwAuthzSvc = RPC_C_AUTHZ_NONE;
                authInfo.pwszServerPrincName = IntPtr.Zero;
                authInfo.dwAuthnLevel = (useConnectSecurity) ? RPC_C_AUTHN_LEVEL_CONNECT : RPC_C_AUTHN_LEVEL_PKT_INTEGRITY;
                authInfo.dwImpersonationLevel = RPC_C_IMP_LEVEL_IMPERSONATE;
                authInfo.pAuthIdentityData = (m_hIdentity.IsAllocated) ? m_hIdentity.AddrOfPinnedObject() : IntPtr.Zero;
                authInfo.dwCapabilities = EOAC_NONE;

                m_hAuthInfo = GCHandle.Alloc(authInfo, GCHandleType.Pinned);

                var serverInfo = new COSERVERINFO();

                serverInfo.pwszName = hostName;
                serverInfo.pAuthInfo = (credential != null) ? m_hAuthInfo.AddrOfPinnedObject() : IntPtr.Zero;
                serverInfo.dwReserved1 = 0;
                serverInfo.dwReserved2 = 0;

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
            GCHandle m_hUserName;
            GCHandle m_hPassword;
            GCHandle m_hDomain;
            GCHandle m_hIdentity;
            GCHandle m_hAuthInfo;
            #endregion
        }
        #endregion

        /// <summary>
        /// Initializes the COM library for use by the calling thread, sets the thread's concurrency model, and creates a new apartment for the thread if one is required. Before thread exit UnInitialize() must be called.
        /// </summary>
        /// <param name="coInit">The chosen threading model. Must be used for all threads.</param>
        public static void Initialize(uint coInit)
        {
            var error = CoInitializeEx(IntPtr.Zero, (COINIT)coInit);

            var result = GetResultID(error);

            if (result.IsError())
            {
                throw new ExternalException("CoInitializeEx: " + GetSystemMessage(error), error);
            }
        }

        /// <summary>
        /// Closes the COM library on the current thread, unloads all DLLs loaded by the thread, frees any other resources that the thread maintains, and forces all RPC connections on the thread to close.
        /// </summary>
        public static void UnInitialize()
        {
            var error = CoUnInitialize();

            var result = GetResultID(error);

            if (result.IsError())
            {
                throw new ExternalException("CoUnInitialize: " + GetSystemMessage(error), error);
            }
        }

        /// <summary>
        /// Initializes COM security.
        /// </summary>
        /// <param name="authenticationLevel">The default authentication level for the process. Both servers and clients use this parameter when they call CoInitializeSecurity. With the Windows Update KB5004442 a higher authentication level of Integrity must be used.</param>
        public static void InitializeSecurity(uint authenticationLevel)
        {
            var error = CoInitializeSecurity(
                IntPtr.Zero,
                -1,
                null,
                IntPtr.Zero,
                authenticationLevel,
                RPC_C_IMP_LEVEL_IDENTIFY,
                IntPtr.Zero,
                EOAC_NONE,
                IntPtr.Zero);

            var result = GetResultID(error);

            if (result.IsError())
            {
                throw new ExternalException("CoInitializeSecurity: " + GetSystemMessage(error), error);
            }
        }

        /// <summary>
        /// Creates an instance of a COM server.
        /// </summary>
		public static object CreateInstance(Guid clsid, string hostName, NetworkCredential credential, bool useConnectSecurity = false)
        {
            var serverInfo = new ServerInfo();
            var coserverInfo = serverInfo.Allocate(hostName, credential, useConnectSecurity);

            var hIID = GCHandle.Alloc(IID_IUnknown, GCHandleType.Pinned);

            var results = new MULTI_QI[1];

            results[0].iid = hIID.AddrOfPinnedObject();
            results[0].pItf = null;
            results[0].hr = 0;

            try
            {
                // check whether connecting locally or remotely.
                var clsctx = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER;

                if (hostName != null && hostName.Length > 0 && hostName != "localhost")
                {
                    clsctx = CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER;
                }

                // create an instance.
                CoCreateInstanceEx(
                    ref clsid,
                    null,
                    clsctx,
                    ref coserverInfo,
                    1,
                    results);   
            }
            finally
            {              
                if (hIID.IsAllocated) hIID.Free();

                serverInfo.Deallocate();
            }

            if (results[0].hr != 0)
            {
                throw new ExternalException("CoCreateInstanceEx: " + GetSystemMessage((int)results[0].hr));
            }

            return results[0].pItf;
        }

        /// <summary>
        /// Creates an instance of a COM server using the specified license key.
        /// </summary>
        public static object CreateInstanceWithLicenseKey(Guid clsid, string hostName, NetworkCredential credential, string licenseKey)
        {
            var serverInfo = new ServerInfo();
            var coserverInfo = serverInfo.Allocate(hostName, credential);
            object instance = null;
            try
            {
                // check whether connecting locally or remotely.
                var clsctx = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER;

                if (hostName != null && hostName.Length > 0)
                {
                    clsctx = CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER;
                }

                DCOMCallWatchdog.Set();

                // get the class factory.
                object unknown = null;

                CoGetClassObject(
                    clsid,
                    clsctx,
                    ref coserverInfo,
                    typeof(IClassFactory2).GUID,
                    out unknown);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new ExternalException("CoGetClassObject: COM Call was cancelled");
                }

                var factory = (IClassFactory2)unknown;

                // set the proper connect authentication level
                var security = (IClientSecurity)factory;

                uint pAuthnSvc = 0;
                uint pAuthzSvc = 0;
                var pServerPrincName = "";
                uint pAuthnLevel = 0;
                uint pImpLevel = 0;
                var pAuthInfo = IntPtr.Zero;
                uint pCapabilities = 0;

                // get existing security settings.
                security.QueryBlanket(
                    factory,
                    ref pAuthnSvc,
                    ref pAuthzSvc,
                    ref pServerPrincName,
                    ref pAuthnLevel,
                    ref pImpLevel,
                    ref pAuthInfo,
                    ref pCapabilities);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new ExternalException("CoGetClassObject: COM Call was cancelled");
                }

                pAuthnSvc = RPC_C_AUTHN_DEFAULT;
                pAuthnLevel = RPC_C_AUTHN_LEVEL_CONNECT;

                // update security settings.
                security.SetBlanket(
                    factory,
                    pAuthnSvc,
                    pAuthzSvc,
                    pServerPrincName,
                    pAuthnLevel,
                    pImpLevel,
                    pAuthInfo,
                    pCapabilities);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new ExternalException("CoGetClassObject: COM Call was cancelled");
                }

                // create instance.
                factory.CreateInstanceLic(
                    null,
                    null,
                    IID_IUnknown,
                    licenseKey,
                    out instance);

                if (DCOMCallWatchdog.IsCancelled)
                {
                    throw new ExternalException("CoGetClassObject: COM Call was cancelled");
                }

                DCOMCallWatchdog.Reset();

            }
            finally
            {
                serverInfo.Deallocate();
            }

            return instance;
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

            var pointers = new IntPtr[size];

            for (var ii = 0; ii < size; ii++)
            {
                pointers[ii] = Marshal.StringToCoTaskMemUni(values[ii]);
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
		/// This flag suppresses the conversion to local time done during marshalling.
		/// </summary>
		public static bool PreserveUtc {
            get { lock (typeof(Interop)) { return DaAeHdaClient.ApplicationInstance.TimeAsUtc; } }
            set { lock (typeof(Interop)) { DaAeHdaClient.ApplicationInstance.TimeAsUtc = value; } }
        }

        private static readonly DateTime FILETIME_BaseTime = new DateTime(1601, 1, 1);

        /// <summary>
        /// Marshals a DateTime as a WIN32 FILETIME.
        /// </summary>
        /// <param name="datetime">The DateTime object to marshal</param>
        /// <returns>The WIN32 FILETIME</returns>
        public static FILETIME GetFILETIME(DateTime datetime)
        {
            FILETIME filetime;

            if (datetime <= FILETIME_BaseTime)
            {
                filetime.dwHighDateTime = 0;
                filetime.dwLowDateTime = 0;
                return filetime;
            }

            // adjust for WIN32 FILETIME base.
            long ticks;
            if (PreserveUtc)
            {
                ticks = datetime.Subtract(new TimeSpan(FILETIME_BaseTime.Ticks)).Ticks;
            }
            else
            {
                ticks = (datetime.ToUniversalTime().Subtract(new TimeSpan(FILETIME_BaseTime.Ticks))).Ticks;
            }

            filetime.dwHighDateTime = (int)((ticks >> 32) & 0xFFFFFFFF);
            filetime.dwLowDateTime = (int)(ticks & 0xFFFFFFFF);

            return filetime;
        }

        /// <summary>
        /// Unmarshals a WIN32 FILETIME from a pointer.
        /// </summary>
        /// <param name="pFiletime">A pointer to a FILETIME structure.</param>
        /// <returns>A DateTime object.</returns>
        public static DateTime GetFILETIME(IntPtr pFiletime)
        {
            if (pFiletime == IntPtr.Zero)
            {
                return DateTime.MinValue;
            }

            return GetFILETIME((FILETIME)Marshal.PtrToStructure(pFiletime, typeof(FILETIME)));
        }

        /// <summary>
        /// Unmarshals a WIN32 FILETIME.
        /// </summary>
        public static DateTime GetFILETIME(FILETIME filetime)
        {
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
            if (PreserveUtc)
            {
                return FILETIME_BaseTime.Add(new TimeSpan(ticks));
            }
            else
            {
                return FILETIME_BaseTime.Add(new TimeSpan(ticks)).ToLocalTime();
            }
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

            var pFiletimes = Marshal.AllocCoTaskMem(count * Marshal.SizeOf(typeof(FILETIME)));

            var pos = pFiletimes;

            for (var ii = 0; ii < count; ii++)
            {
                Marshal.StructureToPtr(GetFILETIME(datetimes[ii]), pos, false);
                pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(FILETIME)));
            }

            return pFiletimes;
        }

        /// <summary>
        /// Unmarshals an array of WIN32 FILETIMEs as DateTimes.
        /// </summary>
        public static DateTime[] GetFILETIMEs(ref IntPtr pArray, int size, bool deallocate)
        {
            if (pArray == IntPtr.Zero || size <= 0)
            {
                return null;
            }

            var datetimes = new DateTime[size];

            var pos = pArray;

            for (var ii = 0; ii < size; ii++)
            {
                datetimes[ii] = GetFILETIME(pos);
                pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(FILETIME)));
            }

            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pArray);
                pArray = IntPtr.Zero;
            }

            return datetimes;
        }

        /// <summary>
        /// WIN32 GUID struct declaration.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct GUID
        {
            public int Data1;
            public short Data2;
            public short Data3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data4;
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
        /// The size, in bytes, of a VARIANT structure.
        /// </summary>
        private static int VARIANT_SIZE => (IntPtr.Size > 4) ? 0x18 : 0x10;

        /// <summary>
        /// Frees all memory referenced by a VARIANT stored in unmanaged memory.
        /// </summary>
        [DllImport("oleaut32.dll")]
        public static extern void VariantClear(IntPtr pVariant);

        /// <summary>
        /// Converts an object into a value that can be marshalled to a VARIANT.
        /// </summary>
        /// <param name="source">The object to convert.</param>
        /// <returns>The converted object.</returns>
        public static object GetVARIANT(object source)
        {
            // check for invalid args.
            if (source == null || source.GetType() == null)
            {
                return null;
            }

            // convert a decimal array to an object array since decimal arrays can't be converted to a variant.
            if (source.GetType() == typeof(decimal[]))
            {
                var srcArray = (decimal[])source;
                var dstArray = new object[srcArray.Length];

                for (var ii = 0; ii < srcArray.Length; ii++)
                {
                    try
                    {
                        dstArray[ii] = (object)srcArray[ii];
                    }
                    catch (Exception)
                    {
                        dstArray[ii] = double.NaN;
                    }
                }

                return dstArray;
            }

            // no conversion required.
            return source;
        }

        /// <summary>
        /// Marshals an array objects into an unmanaged array of VARIANTs.
        /// </summary>
        /// <param name="values">An array of the objects to be marshalled</param>
        /// <param name="preprocess">Whether the objects should have troublesome types removed before marhalling.</param>
        /// <returns>An pointer to the array in unmanaged memory</returns>
        public static IntPtr GetVARIANTs(object[] values, bool preprocess)
        {
            var count = (values != null) ? values.Length : 0;

            if (count <= 0)
            {
                return IntPtr.Zero;
            }

            var pValues = Marshal.AllocCoTaskMem(count * VARIANT_SIZE);

            var pos = pValues;

            for (var ii = 0; ii < count; ii++)
            {
                if (preprocess)
                {
                    Marshal.GetNativeVariantForObject(GetVARIANT(values[ii]), pos);
                }
                else
                {
                    Marshal.GetNativeVariantForObject(values[ii], pos);
                }

                pos = (IntPtr)(pos.ToInt64() + VARIANT_SIZE);
            }

            return pValues;
        }

        /// <summary>
        /// Unmarshals an array of VARIANTs as objects.
        /// </summary>
        public static object[] GetVARIANTs(ref IntPtr pArray, int size, bool deallocate)
        {
            // this method unmarshals VARIANTs one at a time because a single bad value throws 
            // an exception with GetObjectsForNativeVariants(). This approach simply sets the 
            // offending value to null.

            if (pArray == IntPtr.Zero || size <= 0)
            {
                return null;
            }

            var values = new object[size];

            var pos = pArray;

            var bytes = new byte[size * VARIANT_SIZE];
            Marshal.Copy(pos, bytes, 0, bytes.Length);

            for (var ii = 0; ii < size; ii++)
            {
                try
                {
                    values[ii] = Marshal.GetObjectForNativeVariant(pos);
                    if (deallocate) VariantClear(pos);
                }
                catch (Exception)
                {
                    values[ii] = null;
                }

                pos = (IntPtr)(pos.ToInt64() + VARIANT_SIZE);
            }

            if (deallocate)
            {
                Marshal.FreeCoTaskMem(pArray);
                pArray = IntPtr.Zero;
            }

            return values;
        }

        /// <summary>
        /// The constant used to selected the default locale.
        /// </summary>
        internal const int LOCALE_SYSTEM_DEFAULT = 0x800;
        internal const int LOCALE_USER_DEFAULT = 0x400;

        /// <summary>
        /// Converts a LCID to a Locale string.
        /// </summary>
        internal static string GetLocale(int input)
        {
            try
            {
                if (input == LOCALE_SYSTEM_DEFAULT || input == LOCALE_USER_DEFAULT || input == 0)
                {
                    return CultureInfo.InvariantCulture.Name;
                }

                return new CultureInfo(input).Name;
            }
            catch
            {
                throw new ExternalException("Invalid LCID", OpcResult.E_INVALIDARG.Code);
            }
        }

        /// <summary>
        /// Converts a Locale string to a LCID.
        /// </summary>
        internal static int GetLocale(string input)
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
        /// Converts the VARTYPE to a system type.
        /// </summary>
        internal static Type GetType(VarEnum input)
        {
            switch (input)
            {
                case VarEnum.VT_EMPTY: return null;
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
                case VarEnum.VT_CY: return typeof(decimal);
                case VarEnum.VT_BOOL: return typeof(bool);
                case VarEnum.VT_DATE: return typeof(DateTime);
                case VarEnum.VT_BSTR: return typeof(string);
                case VarEnum.VT_ARRAY | VarEnum.VT_I1: return typeof(sbyte[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_UI1: return typeof(byte[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_I2: return typeof(short[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_UI2: return typeof(ushort[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_I4: return typeof(int[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_UI4: return typeof(uint[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_I8: return typeof(long[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_UI8: return typeof(ulong[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_R4: return typeof(float[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_R8: return typeof(double[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_CY: return typeof(decimal[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_BOOL: return typeof(bool[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_DATE: return typeof(DateTime[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_BSTR: return typeof(string[]);
                case VarEnum.VT_ARRAY | VarEnum.VT_VARIANT: return typeof(object[]);
                default: return OpcType.ILLEGAL_TYPE;
            }
        }

        /// <summary>
        /// Converts the system type to a VARTYPE.
        /// </summary>
        internal static VarEnum GetType(Type input)
        {
            if (input == null) return VarEnum.VT_EMPTY;
            if (input == typeof(sbyte)) return VarEnum.VT_I1;
            if (input == typeof(byte)) return VarEnum.VT_UI1;
            if (input == typeof(short)) return VarEnum.VT_I2;
            if (input == typeof(ushort)) return VarEnum.VT_UI2;
            if (input == typeof(int)) return VarEnum.VT_I4;
            if (input == typeof(uint)) return VarEnum.VT_UI4;
            if (input == typeof(long)) return VarEnum.VT_I8;
            if (input == typeof(ulong)) return VarEnum.VT_UI8;
            if (input == typeof(float)) return VarEnum.VT_R4;
            if (input == typeof(double)) return VarEnum.VT_R8;
            if (input == typeof(decimal)) return VarEnum.VT_CY;
            if (input == typeof(bool)) return VarEnum.VT_BOOL;
            if (input == typeof(DateTime)) return VarEnum.VT_DATE;
            if (input == typeof(string)) return VarEnum.VT_BSTR;
            if (input == typeof(object)) return VarEnum.VT_EMPTY;
            if (input == typeof(sbyte[])) return VarEnum.VT_ARRAY | VarEnum.VT_I1;
            if (input == typeof(byte[])) return VarEnum.VT_ARRAY | VarEnum.VT_UI1;
            if (input == typeof(short[])) return VarEnum.VT_ARRAY | VarEnum.VT_I2;
            if (input == typeof(ushort[])) return VarEnum.VT_ARRAY | VarEnum.VT_UI2;
            if (input == typeof(int[])) return VarEnum.VT_ARRAY | VarEnum.VT_I4;
            if (input == typeof(uint[])) return VarEnum.VT_ARRAY | VarEnum.VT_UI4;
            if (input == typeof(long[])) return VarEnum.VT_ARRAY | VarEnum.VT_I8;
            if (input == typeof(ulong[])) return VarEnum.VT_ARRAY | VarEnum.VT_UI8;
            if (input == typeof(float[])) return VarEnum.VT_ARRAY | VarEnum.VT_R4;
            if (input == typeof(double[])) return VarEnum.VT_ARRAY | VarEnum.VT_R8;
            if (input == typeof(decimal[])) return VarEnum.VT_ARRAY | VarEnum.VT_CY;
            if (input == typeof(bool[])) return VarEnum.VT_ARRAY | VarEnum.VT_BOOL;
            if (input == typeof(DateTime[])) return VarEnum.VT_ARRAY | VarEnum.VT_DATE;
            if (input == typeof(string[])) return VarEnum.VT_ARRAY | VarEnum.VT_BSTR;
            if (input == typeof(object[])) return VarEnum.VT_ARRAY | VarEnum.VT_VARIANT;

            // check for special types.
            if (input == OpcType.ILLEGAL_TYPE) return (VarEnum)Enum.ToObject(typeof(VarEnum), 0x7FFF);
            if (input == typeof(Type)) return VarEnum.VT_I2;
            if (input == typeof(TsCDaQuality)) return VarEnum.VT_I2;
            if (input == typeof(TsDaAccessRights)) return VarEnum.VT_I4;
            if (input == typeof(TsDaEuType)) return VarEnum.VT_I4;
            return VarEnum.VT_EMPTY;
        }

        /// <summary>
        /// Converts the HRESULT to a system type.
        /// </summary>
        internal static OpcResult GetResultID(int input)
        {
            switch (input)
            {
                // data access.
                case Da.Result.S_OK: return new OpcResult(OpcResult.S_OK, input);
                case Da.Result.E_FAIL: return new OpcResult(OpcResult.E_FAIL, input);
                case Da.Result.E_INVALIDARG: return new OpcResult(OpcResult.E_INVALIDARG, input);
                case Da.Result.DISP_E_TYPEMISMATCH: return new OpcResult(OpcResult.Da.E_BADTYPE, input);
                case Da.Result.DISP_E_OVERFLOW: return new OpcResult(OpcResult.Da.E_RANGE, input);
                case Da.Result.E_OUTOFMEMORY: return new OpcResult(OpcResult.E_OUTOFMEMORY, input);
                case Da.Result.E_NOINTERFACE: return new OpcResult(OpcResult.E_NOTSUPPORTED, input);
                case Da.Result.E_INVALIDHANDLE: return new OpcResult(OpcResult.Da.E_INVALIDHANDLE, input);
                case Da.Result.E_BADTYPE: return new OpcResult(OpcResult.Da.E_BADTYPE, input);
                case Da.Result.E_UNKNOWNITEMID: return new OpcResult(OpcResult.Da.E_UNKNOWN_ITEM_NAME, input);
                case Da.Result.E_INVALIDITEMID: return new OpcResult(OpcResult.Da.E_INVALID_ITEM_NAME, input);
                case Da.Result.E_UNKNOWNPATH: return new OpcResult(OpcResult.Da.E_UNKNOWN_ITEM_PATH, input);
                case Da.Result.E_INVALIDFILTER: return new OpcResult(OpcResult.Da.E_INVALID_FILTER, input);
                case Da.Result.E_RANGE: return new OpcResult(OpcResult.Da.E_RANGE, input);
                case Da.Result.E_DUPLICATENAME: return new OpcResult(OpcResult.Da.E_DUPLICATENAME, input);
                case Da.Result.S_UNSUPPORTEDRATE: return new OpcResult(OpcResult.Da.S_UNSUPPORTEDRATE, input);
                case Da.Result.S_CLAMP: return new OpcResult(OpcResult.Da.S_CLAMP, input);
                case Da.Result.E_INVALID_PID: return new OpcResult(OpcResult.Da.E_INVALID_PID, input);
                case Da.Result.E_DEADBANDNOTSUPPORTED: return new OpcResult(OpcResult.Da.E_NO_ITEM_DEADBAND, input);
                case Da.Result.E_NOBUFFERING: return new OpcResult(OpcResult.Da.E_NO_ITEM_BUFFERING, input);
                case Da.Result.E_NOTSUPPORTED: return new OpcResult(OpcResult.Da.E_NO_WRITEQT, input);
                case Da.Result.E_INVALIDCONTINUATIONPOINT: return new OpcResult(OpcResult.Da.E_INVALIDCONTINUATIONPOINT, input);
                case Da.Result.S_DATAQUEUEOVERFLOW: return new OpcResult(OpcResult.Da.S_DATAQUEUEOVERFLOW, input);

                // complex data.
                case Cpx.Result.E_TYPE_CHANGED: return new OpcResult(OpcResult.Cpx.E_TYPE_CHANGED, input);
                case Cpx.Result.E_FILTER_DUPLICATE: return new OpcResult(OpcResult.Cpx.E_FILTER_DUPLICATE, input);
                case Cpx.Result.E_FILTER_INVALID: return new OpcResult(OpcResult.Cpx.E_FILTER_INVALID, input);
                case Cpx.Result.E_FILTER_ERROR: return new OpcResult(OpcResult.Cpx.E_FILTER_ERROR, input);
                case Cpx.Result.S_FILTER_NO_DATA: return new OpcResult(OpcResult.Cpx.S_FILTER_NO_DATA, input);

                // historical data access.
                case Hda.Result.E_MAXEXCEEDED: return new OpcResult(OpcResult.Hda.E_MAXEXCEEDED, input);
                case Hda.Result.S_NODATA: return new OpcResult(OpcResult.Hda.S_NODATA, input);
                case Hda.Result.S_MOREDATA: return new OpcResult(OpcResult.Hda.S_MOREDATA, input);
                case Hda.Result.E_INVALIDAGGREGATE: return new OpcResult(OpcResult.Hda.E_INVALIDAGGREGATE, input);
                case Hda.Result.S_CURRENTVALUE: return new OpcResult(OpcResult.Hda.S_CURRENTVALUE, input);
                case Hda.Result.S_EXTRADATA: return new OpcResult(OpcResult.Hda.S_EXTRADATA, input);
                case Hda.Result.W_NOFILTER: return new OpcResult(OpcResult.Hda.W_NOFILTER, input);
                case Hda.Result.E_UNKNOWNATTRID: return new OpcResult(OpcResult.Hda.E_UNKNOWNATTRID, input);
                case Hda.Result.E_NOT_AVAIL: return new OpcResult(OpcResult.Hda.E_NOT_AVAIL, input);
                case Hda.Result.E_INVALIDDATATYPE: return new OpcResult(OpcResult.Hda.E_INVALIDDATATYPE, input);
                case Hda.Result.E_DATAEXISTS: return new OpcResult(OpcResult.Hda.E_DATAEXISTS, input);
                case Hda.Result.E_INVALIDATTRID: return new OpcResult(OpcResult.Hda.E_INVALIDATTRID, input);
                case Hda.Result.E_NODATAEXISTS: return new OpcResult(OpcResult.Hda.E_NODATAEXISTS, input);
                case Hda.Result.S_INSERTED: return new OpcResult(OpcResult.Hda.S_INSERTED, input);
                case Hda.Result.S_REPLACED: return new OpcResult(OpcResult.Hda.S_REPLACED, input);

                // Alarms and Events.
                case Ae.Result.S_ALREADYACKED: return new OpcResult(OpcResult.Ae.S_ALREADYACKED, input);
                case Ae.Result.S_INVALIDBUFFERTIME: return new OpcResult(OpcResult.Ae.S_INVALIDBUFFERTIME, input);
                case Ae.Result.S_INVALIDMAXSIZE: return new OpcResult(OpcResult.Ae.S_INVALIDMAXSIZE, input);
                case Ae.Result.S_INVALIDKEEPALIVETIME: return new OpcResult(OpcResult.Ae.S_INVALIDKEEPALIVETIME, input);

                // This function returns Da.Result.E_INVALID_PID. AE specific code must map to E_INVALIDBRANCHNAME.
                // case Technosoftware.DaAeHdaClient.Com.Ae.Result.E_INVALIDBRANCHNAME:    return new OpcResult(OpcResult.Ae.E_INVALIDBRANCHNAME, input);

                case Ae.Result.E_INVALIDTIME: return new OpcResult(OpcResult.Ae.E_INVALIDTIME, input);
                case Ae.Result.E_BUSY: return new OpcResult(OpcResult.Ae.E_BUSY, input);
                case Ae.Result.E_NOINFO: return new OpcResult(OpcResult.Ae.E_NOINFO, input);

                default:
                {
                    // check for RPC error.
                    if ((input & 0x7FFF0000) == 0x00010000)
                    {
                        return new OpcResult(OpcResult.E_NETWORK_ERROR, input);
                    }

                    // chekc for success code.
                    if (input >= 0)
                    {
                        return new OpcResult(OpcResult.S_FALSE, input);
                    }

                    // return generic error.
                    return new OpcResult(OpcResult.E_FAIL, input);
                }
            }
        }

        /// <summary>
        /// Converts a result id to an HRESULT.
        /// </summary>
        internal static int GetResultID(OpcResult input)
		{				
			// data access.
			if (input.Name != null && input.Name.Namespace == OpcNamespace.OPC_DATA_ACCESS)
			{
				if (input == OpcResult.S_OK)                          return Da.Result.S_OK;
				if (input == OpcResult.E_FAIL)                        return Da.Result.E_FAIL;  
				if (input == OpcResult.E_INVALIDARG)                  return Da.Result.E_INVALIDARG; 
				if (input == OpcResult.Da.E_BADTYPE)                  return Da.Result.E_BADTYPE;  
				if (input == OpcResult.Da.E_READONLY)                 return Da.Result.E_BADRIGHTS;  
				if (input == OpcResult.Da.E_WRITEONLY)                return Da.Result.E_BADRIGHTS;  
				if (input == OpcResult.Da.E_RANGE)                    return Da.Result.E_RANGE;  
				if (input == OpcResult.E_OUTOFMEMORY)                 return Da.Result.E_OUTOFMEMORY;  
				if (input == OpcResult.E_NOTSUPPORTED)                return Da.Result.E_NOINTERFACE;  
				if (input == OpcResult.Da.E_INVALIDHANDLE)            return Da.Result.E_INVALIDHANDLE;  
				if (input == OpcResult.Da.E_UNKNOWN_ITEM_NAME)        return Da.Result.E_UNKNOWNITEMID;  
				if (input == OpcResult.Da.E_INVALID_ITEM_NAME)        return Da.Result.E_INVALIDITEMID;  
				if (input == OpcResult.Da.E_INVALID_ITEM_PATH)        return Da.Result.E_INVALIDITEMID; 
				if (input == OpcResult.Da.E_UNKNOWN_ITEM_PATH)        return Da.Result.E_UNKNOWNPATH;  
				if (input == OpcResult.Da.E_INVALID_FILTER)           return Da.Result.E_INVALIDFILTER;  
				if (input == OpcResult.Da.S_UNSUPPORTEDRATE)          return Da.Result.S_UNSUPPORTEDRATE; 
				if (input == OpcResult.Da.S_CLAMP)                    return Da.Result.S_CLAMP;  
				if (input == OpcResult.Da.E_INVALID_PID)              return Da.Result.E_INVALID_PID;  
				if (input == OpcResult.Da.E_NO_ITEM_DEADBAND)         return Da.Result.E_DEADBANDNOTSUPPORTED;  
				if (input == OpcResult.Da.E_NO_ITEM_BUFFERING)        return Da.Result.E_NOBUFFERING;
				if (input == OpcResult.Da.E_NO_WRITEQT)               return Da.Result.E_NOTSUPPORTED;
				if (input == OpcResult.Da.E_INVALIDCONTINUATIONPOINT) return Da.Result.E_INVALIDCONTINUATIONPOINT;
				if (input == OpcResult.Da.S_DATAQUEUEOVERFLOW)        return Da.Result.S_DATAQUEUEOVERFLOW;
			}

			// complex data.
			else if (input.Name != null && input.Name.Namespace == OpcNamespace.OPC_COMPLEX_DATA)
			{
				if (input == OpcResult.Cpx.E_TYPE_CHANGED)            return Cpx.Result.E_TYPE_CHANGED;
				if (input == OpcResult.Cpx.E_FILTER_DUPLICATE)        return Cpx.Result.E_FILTER_DUPLICATE;
				if (input == OpcResult.Cpx.E_FILTER_INVALID)          return Cpx.Result.E_FILTER_INVALID;
				if (input == OpcResult.Cpx.E_FILTER_ERROR)            return Cpx.Result.E_FILTER_ERROR;
				if (input == OpcResult.Cpx.S_FILTER_NO_DATA)          return Cpx.Result.S_FILTER_NO_DATA;
			}
							
			// historical data access.
			else if (input.Name != null && input.Name.Namespace == OpcNamespace.OPC_HISTORICAL_DATA_ACCESS)
			{
				if (input == OpcResult.Hda.E_MAXEXCEEDED)             return Hda.Result.E_MAXEXCEEDED;
				if (input == OpcResult.Hda.S_NODATA)                  return Hda.Result.S_NODATA;
				if (input == OpcResult.Hda.S_MOREDATA)                return Hda.Result.S_MOREDATA;
				if (input == OpcResult.Hda.E_INVALIDAGGREGATE)        return Hda.Result.E_INVALIDAGGREGATE;
				if (input == OpcResult.Hda.S_CURRENTVALUE)            return Hda.Result.S_CURRENTVALUE;
				if (input == OpcResult.Hda.S_EXTRADATA)               return Hda.Result.S_EXTRADATA;
				if (input == OpcResult.Hda.E_UNKNOWNATTRID)           return Hda.Result.E_UNKNOWNATTRID;
				if (input == OpcResult.Hda.E_NOT_AVAIL)               return Hda.Result.E_NOT_AVAIL;
				if (input == OpcResult.Hda.E_INVALIDDATATYPE)         return Hda.Result.E_INVALIDDATATYPE;
				if (input == OpcResult.Hda.E_DATAEXISTS)              return Hda.Result.E_DATAEXISTS;
				if (input == OpcResult.Hda.E_INVALIDATTRID)           return Hda.Result.E_INVALIDATTRID;
				if (input == OpcResult.Hda.E_NODATAEXISTS)            return Hda.Result.E_NODATAEXISTS;
				if (input == OpcResult.Hda.S_INSERTED)                return Hda.Result.S_INSERTED;
				if (input == OpcResult.Hda.S_REPLACED)                return Hda.Result.S_REPLACED;
			}

			// check for custom code.
			else if (input.Code == -1)
			{
				// default success code.
				if (input.Succeeded())
				{
					return Da.Result.S_FALSE;
				}

				// default error code.
				return Da.Result.E_FAIL;
			}

			// return custom code.
			return input.Code;
        }

        /// <summary>
        /// Returns an exception after extracting HRESULT from the exception.
        /// </summary>
        public static Exception CreateException(string message, Exception e)
        {
            return CreateException(message, Marshal.GetHRForException(e));
        }

        /// <summary>
        /// Returns an exception after extracting HRESULT from the exception.
        /// </summary>
        public static Exception CreateException(string message, int code)
        {
            return new OpcResultException(GetResultID(code), message);
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
    }
}
