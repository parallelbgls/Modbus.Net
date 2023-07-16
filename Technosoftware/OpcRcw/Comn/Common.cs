#region Copyright (c) 2022-2023 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2022-2023 Technosoftware GmbH. All rights reserved
// Web: https://technosoftware.com 
//
// The Software is based on the OPC Foundation MIT License. 
// The complete license agreement for that can be found here:
// http://opcfoundation.org/License/MIT/1.00/
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved

#region Using Directives
using System;
using System.Runtime.InteropServices;

/* Unmerged change from project 'Technosoftware.OpcRcw (net472)'
Before:
#endregion
After:
using Technosoftware.OpcRcw;
using Technosoftware.OpcRcw.Comn;
using Technosoftware.OpcRcw;
#endregion
*/
#endregion

#pragma warning disable 1591

namespace Technosoftware.OpcRcw.Comn
{
    /// <exclude />
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct CONNECTDATA
    {
        [MarshalAs(UnmanagedType.IUnknown)]
        object pUnk;
        [MarshalAs(UnmanagedType.I4)]
        int dwCookie;
    }

    /// <exclude />
    [ComImport]
    [Guid("B196B287-BAB4-101A-B69C-00AA00341D07")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumConnections
    {
        /// <summary>
        /// Retrieves a specified number of items in the enumeration sequence.
        /// </summary>
        /// <param name="cConnections"></param>
        /// <param name="rgcd"></param>
        /// <param name="pcFetched"></param>
        void RemoteNext(
            [MarshalAs(UnmanagedType.I4)]
            int cConnections,
            [Out]
            IntPtr rgcd,
            [Out][MarshalAs(UnmanagedType.I4)]
            out int pcFetched);

        /// <summary>
        /// Skips a specified number of items in the enumeration sequence.
        /// </summary>
        /// <param name="cConnections"></param>
        void Skip(
            [MarshalAs(UnmanagedType.I4)]
            int cConnections);

        /// <summary>
        /// Retrieves a specified number of items in the enumeration sequence.
        /// </summary>
        void Reset();

        /// <summary>
        /// Creates a new enumerator that contains the same enumeration state as the current one.
        /// </summary>
        /// <param name="ppEnum"></param>
        void Clone(
            [Out]
            out IEnumConnections ppEnum);
    }

    /// <exclude />
    [ComImport]
    [Guid("B196B286-BAB4-101A-B69C-00AA00341D07")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IConnectionPoint
    {
        void GetConnectionInterface(
            [Out]
            out Guid pIID);

        void GetConnectionPointContainer(
            [Out]
            out IConnectionPointContainer ppCPC);

        void Advise(
            [MarshalAs(UnmanagedType.IUnknown)]
            object pUnkSink,
            [Out][MarshalAs(UnmanagedType.I4)]
            out int pdwCookie);

        void Unadvise(
            [MarshalAs(UnmanagedType.I4)]
            int dwCookie);

        void EnumConnections(
            [Out]
            out IEnumConnections ppEnum);
    }

    /// <exclude />
    [ComImport]
    [Guid("B196B285-BAB4-101A-B69C-00AA00341D07")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumConnectionPoints
    {
        void RemoteNext(
            [MarshalAs(UnmanagedType.I4)]
            int cConnections,
            [Out]
            IntPtr ppCP,
            [Out][MarshalAs(UnmanagedType.I4)]
            out int pcFetched);

        void Skip(
            [MarshalAs(UnmanagedType.I4)]
            int cConnections);

        void Reset();

        void Clone(
            [Out]
            out IEnumConnectionPoints ppEnum);
    }

    /// <exclude />
    [ComImport]
    [Guid("B196B284-BAB4-101A-B69C-00AA00341D07")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IConnectionPointContainer
    {
        void EnumConnectionPoints(
            [Out]
            out IEnumConnectionPoints ppEnum);

        void FindConnectionPoint(
            ref Guid riid,
            [Out]
            out IConnectionPoint ppCP);
    }

    /// <exclude />
	[ComImport]
    [Guid("F31DFDE1-07B6-11d2-B2D8-0060083BA1FB")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCShutdown
    {
        void ShutdownRequest(
            [MarshalAs(UnmanagedType.LPWStr)]
            string szReason);
    }

    /// <exclude />
	[ComImport]
    [Guid("F31DFDE2-07B6-11d2-B2D8-0060083BA1FB")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCCommon
    {
        void SetLocaleID(
            [MarshalAs(UnmanagedType.I4)]
            int dwLcid);

        void GetLocaleID(
            [Out][MarshalAs(UnmanagedType.I4)]
            out int pdwLcid);

        void QueryAvailableLocaleIDs(
            [Out][MarshalAs(UnmanagedType.I4)]
            out int pdwCount,
            [Out]
            out IntPtr pdwLcid);

        void GetErrorString(
            [MarshalAs(UnmanagedType.I4)]
            int dwError,
            [Out][MarshalAs(UnmanagedType.LPWStr)]
            out string ppString);

        void SetClientName(
            [MarshalAs(UnmanagedType.LPWStr)]
            string szName);
    }

    /// <exclude />
	[ComImport]
    [Guid("13486D50-4821-11D2-A494-3CB306C10000")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCServerList
    {
        void EnumClassesOfCategories(
            [MarshalAs(UnmanagedType.I4)]
            int cImplemented,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStruct, SizeParamIndex=0)]
            Guid[] rgcatidImpl,
            [MarshalAs(UnmanagedType.I4)]
            int cRequired,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStruct, SizeParamIndex=2)]
            Guid[] rgcatidReq,
            [Out][MarshalAs(UnmanagedType.IUnknown)]
            out object ppenumClsid);

        void GetClassDetails(
            ref Guid clsid,
            [Out][MarshalAs(UnmanagedType.LPWStr)]
            out string ppszProgID,
            [Out][MarshalAs(UnmanagedType.LPWStr)]
            out string ppszUserType);

        void CLSIDFromProgID(
            [MarshalAs(UnmanagedType.LPWStr)]
            string szProgId,
            [Out]
            out Guid clsid);
    }

    /// <exclude />
	[ComImport]
    [Guid("55C382C8-21C7-4e88-96C1-BECFB1E3F483")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCEnumGUID
    {
        void Next(
            [MarshalAs(UnmanagedType.I4)]
            int celt,
            [Out]
            IntPtr rgelt,
            [Out][MarshalAs(UnmanagedType.I4)]
            out int pceltFetched);

        void Skip(
            [MarshalAs(UnmanagedType.I4)]
            int celt);

        void Reset();

        void Clone(
            [Out]
            out IOPCEnumGUID ppenum);
    }

    /// <exclude />
	[ComImport]
    [Guid("0002E000-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumGUID
    {
        void Next(
            [MarshalAs(UnmanagedType.I4)]
            int celt,
            [Out]
            IntPtr rgelt,
            [Out][MarshalAs(UnmanagedType.I4)]
            out int pceltFetched);

        void Skip(
            [MarshalAs(UnmanagedType.I4)]
            int celt);

        void Reset();

        void Clone(
            [Out]
            out IEnumGUID ppenum);
    }

    /// <exclude />
	[ComImport]
    [Guid("00000100-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumUnknown
    {
        void RemoteNext(
            [MarshalAs(UnmanagedType.I4)]
            int celt,
            [Out]
            IntPtr rgelt,
            [Out][MarshalAs(UnmanagedType.I4)]
            out int pceltFetched);

        void Skip(
            [MarshalAs(UnmanagedType.I4)]
            int celt);

        void Reset();

        void Clone(
            [Out]
            out IEnumUnknown ppenum);
    }

    /// <exclude />
	[ComImport]
    [Guid("00000101-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumString
    {
        void RemoteNext(
            [MarshalAs(UnmanagedType.I4)]
            int celt,
            IntPtr rgelt,
            [Out][MarshalAs(UnmanagedType.I4)]
            out int pceltFetched);

        void Skip(
            [MarshalAs(UnmanagedType.I4)]
            int celt);

        void Reset();

        void Clone(
            [Out]
            out IEnumString ppenum);
    }

    /// <exclude />
	[ComImport]
    [Guid("9DD0B56C-AD9E-43ee-8305-487F3188BF7A")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCServerList2
    {
        void EnumClassesOfCategories(
            [MarshalAs(UnmanagedType.I4)]
            int cImplemented,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStruct, SizeParamIndex=0)]
            Guid[] rgcatidImpl,
            [MarshalAs(UnmanagedType.I4)]
            int cRequired,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType=UnmanagedType.LPStruct, SizeParamIndex=0)]
            Guid[] rgcatidReq,
            [Out]
            out IOPCEnumGUID ppenumClsid);

        void GetClassDetails(
            ref Guid clsid,
            [Out][MarshalAs(UnmanagedType.LPWStr)]
            out string ppszProgID,
            [Out][MarshalAs(UnmanagedType.LPWStr)]
            out string ppszUserType,
            [Out][MarshalAs(UnmanagedType.LPWStr)]
            out string ppszVerIndProgID);

        void CLSIDFromProgID(
            [MarshalAs(UnmanagedType.LPWStr)]
            string szProgId,
            [Out]
            out Guid clsid);
    }
}
