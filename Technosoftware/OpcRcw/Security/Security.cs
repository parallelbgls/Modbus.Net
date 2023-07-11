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

using System.Runtime.InteropServices;
#endregion

#pragma warning disable 1591

namespace Technosoftware.OpcRcw.Security
{       
    /// <exclude />
	[ComImport]
	[GuidAttribute("7AA83A01-6C77-11d3-84F9-00008630A38B")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)] 
    public interface IOPCSecurityNT
    {
	    void IsAvailableNT(
		    [Out][MarshalAs(UnmanagedType.I4)]
		    out int pbAvailable);

	    void QueryMinImpersonationLevel(
		    [Out][MarshalAs(UnmanagedType.I4)]
		    out int pdwMinImpLevel);

	    void ChangeUser();
    };

    /// <exclude />
	[ComImport]
	[GuidAttribute("7AA83A02-6C77-11d3-84F9-00008630A38B")]
	[InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)] 
    public interface IOPCSecurityPrivate
    {
        void IsAvailablePriv(
		    [Out][MarshalAs(UnmanagedType.I4)]
		    out int pbAvailable);

        void Logon(
			[MarshalAs(UnmanagedType.LPWStr)]
		    string szUserID, 
			[MarshalAs(UnmanagedType.LPWStr)]
		    string szPassword);

        void Logoff();
    };
}
