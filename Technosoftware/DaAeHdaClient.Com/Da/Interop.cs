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
using System.Runtime.InteropServices;
using System.Reflection;

using Technosoftware.DaAeHdaClient.Da;
#endregion

#pragma warning disable 0618

namespace Technosoftware.DaAeHdaClient.Com.Da
{
    /// <summary>
    /// Contains state information for a single asynchronous Technosoftware.DaAeHdaClient.Com.Da.Interop.
    /// </summary>
    internal class Interop
    {
        /// <summary>
        /// Converts a standard FILETIME to an OpcRcw.Da.FILETIME structure.
        /// </summary>
        internal static OpcRcw.Da.FILETIME Convert(FILETIME input)
        {
            var output = new OpcRcw.Da.FILETIME();
            output.dwLowDateTime = input.dwLowDateTime;
            output.dwHighDateTime = input.dwHighDateTime;
            return output;
        }

        /// <summary>
        /// Converts an OpcRcw.Da.FILETIME to a standard FILETIME structure.
        /// </summary>
        internal static FILETIME Convert(OpcRcw.Da.FILETIME input)
        {
            var output = new FILETIME();
            output.dwLowDateTime = input.dwLowDateTime;
            output.dwHighDateTime = input.dwHighDateTime;
            return output;
        }

        /// <summary>
        /// Allocates and marshals a OPCSERVERSTATUS structure.
        /// </summary>
        internal static OpcRcw.Da.OPCSERVERSTATUS GetServerStatus(OpcServerStatus input, int groupCount)
        {
            var output = new OpcRcw.Da.OPCSERVERSTATUS();

            if (input != null)
            {
                output.szVendorInfo = input.VendorInfo;
                output.wMajorVersion = 0;
                output.wMinorVersion = 0;
                output.wBuildNumber = 0;
                output.dwServerState = (OpcRcw.Da.OPCSERVERSTATE)input.ServerState;
                output.ftStartTime = Convert(Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIME(input.StartTime));
                output.ftCurrentTime = Convert(Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIME(input.CurrentTime));
                output.ftLastUpdateTime = Convert(Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIME(input.LastUpdateTime));
                output.dwBandWidth = -1;
                output.dwGroupCount = groupCount;
                output.wReserved = 0;

                if (input.ProductVersion != null)
                {
                    var versions = input.ProductVersion.Split(new char[] { '.' });

                    if (versions.Length > 0)
                    {
                        try { output.wMajorVersion = System.Convert.ToInt16(versions[0]); }
                        catch { output.wMajorVersion = 0; }
                    }

                    if (versions.Length > 1)
                    {
                        try { output.wMinorVersion = System.Convert.ToInt16(versions[1]); }
                        catch { output.wMinorVersion = 0; }
                    }

                    output.wBuildNumber = 0;

                    for (var ii = 2; ii < versions.Length; ii++)
                    {
                        try
                        {
                            output.wBuildNumber = (short)(output.wBuildNumber * 100 + System.Convert.ToInt16(versions[ii]));
                        }
                        catch
                        {
                            output.wBuildNumber = 0;
                            break;
                        }
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Unmarshals and deallocates a OPCSERVERSTATUS structure.
        /// </summary>
        internal static OpcServerStatus GetServerStatus(ref IntPtr pInput, bool deallocate)
        {
            OpcServerStatus output = null;

            if (pInput != IntPtr.Zero)
            {
                var status = (OpcRcw.Da.OPCSERVERSTATUS)Marshal.PtrToStructure(pInput, typeof(OpcRcw.Da.OPCSERVERSTATUS));

                output = new OpcServerStatus();

                output.VendorInfo = status.szVendorInfo;
                output.ProductVersion = string.Format("{0}.{1}.{2}", status.wMajorVersion, status.wMinorVersion, status.wBuildNumber);
                output.ServerState = (OpcServerState)status.dwServerState;
                output.StatusInfo = null;
				output.StartTime      = Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIME(Convert(status.ftStartTime));
				output.CurrentTime    = Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIME(Convert(status.ftCurrentTime));
				output.LastUpdateTime = Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIME(Convert(status.ftLastUpdateTime));

                if (deallocate)
                {
                    Marshal.DestroyStructure(pInput, typeof(OpcRcw.Da.OPCSERVERSTATUS));
                    Marshal.FreeCoTaskMem(pInput);
                    pInput = IntPtr.Zero;
                }
            }

            return output;
        }

        /// <summary>
        /// Converts a browseFilter values to the COM equivalent.
        /// </summary>
        internal static OpcRcw.Da.OPCBROWSEFILTER GetBrowseFilter(TsCDaBrowseFilter input)
        {
            switch (input)
            {
                case TsCDaBrowseFilter.All: return OpcRcw.Da.OPCBROWSEFILTER.OPC_BROWSE_FILTER_ALL;
                case TsCDaBrowseFilter.Branch: return OpcRcw.Da.OPCBROWSEFILTER.OPC_BROWSE_FILTER_BRANCHES;
                case TsCDaBrowseFilter.Item: return OpcRcw.Da.OPCBROWSEFILTER.OPC_BROWSE_FILTER_ITEMS;
            }

            return OpcRcw.Da.OPCBROWSEFILTER.OPC_BROWSE_FILTER_ALL;
        }

        /// <summary>
        /// Converts a browseFilter values from the COM equivalent.
        /// </summary>
        internal static TsCDaBrowseFilter GetBrowseFilter(OpcRcw.Da.OPCBROWSEFILTER input)
        {
            switch (input)
            {
                case OpcRcw.Da.OPCBROWSEFILTER.OPC_BROWSE_FILTER_ALL: return TsCDaBrowseFilter.All;
                case OpcRcw.Da.OPCBROWSEFILTER.OPC_BROWSE_FILTER_BRANCHES: return TsCDaBrowseFilter.Branch;
                case OpcRcw.Da.OPCBROWSEFILTER.OPC_BROWSE_FILTER_ITEMS: return TsCDaBrowseFilter.Item;
            }

            return TsCDaBrowseFilter.All;
        }

        /// <summary>
        /// Allocates and marshals an array of HRESULT codes.
        /// </summary>
        internal static IntPtr GetHRESULTs(IOpcResult[] results)
        {
            // extract error codes from results.
            var errors = new int[results.Length];

            for (var ii = 0; ii < results.Length; ii++)
            {
                if (results[ii] != null)
                {
                    errors[ii] = Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(results[ii].Result);
                }
                else
                {
                    errors[ii] = Result.E_INVALIDHANDLE;
                }
            }

            // marshal error codes.
            var pErrors = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)) * results.Length);
            Marshal.Copy(errors, 0, pErrors, results.Length);

            // return results.
            return pErrors;
        }

        /// <summary>
        /// Unmarshals and deallocates an array of OPCBROWSEELEMENT structures.
        /// </summary>
        internal static TsCDaBrowseElement[] GetBrowseElements(ref IntPtr pInput, int count, bool deallocate)
        {
            TsCDaBrowseElement[] output = null;

            if (pInput != IntPtr.Zero && count > 0)
            {
                output = new TsCDaBrowseElement[count];

                var pos = pInput;

                for (var ii = 0; ii < count; ii++)
                {
                    output[ii] = GetBrowseElement(pos, deallocate);
                    pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(OpcRcw.Da.OPCBROWSEELEMENT)));
                }

                if (deallocate)
                {
                    Marshal.FreeCoTaskMem(pInput);
                    pInput = IntPtr.Zero;
                }
            }

            return output;
        }

        /// <summary>
        /// Allocates and marshals an array of OPCBROWSEELEMENT structures.
        /// </summary>
        internal static IntPtr GetBrowseElements(TsCDaBrowseElement[] input, bool propertiesRequested)
        {
            var output = IntPtr.Zero;

            if (input != null && input.Length > 0)
            {
                output = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(OpcRcw.Da.OPCBROWSEELEMENT)) * input.Length);

                var pos = output;

                for (var ii = 0; ii < input.Length; ii++)
                {
                    var element = GetBrowseElement(input[ii], propertiesRequested);
                    Marshal.StructureToPtr(element, pos, false);
                    pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(OpcRcw.Da.OPCBROWSEELEMENT)));
                }
            }

            return output;
        }

        /// <summary>
        /// Unmarshals and deallocates a OPCBROWSEELEMENT structures.
        /// </summary>
        internal static TsCDaBrowseElement GetBrowseElement(IntPtr pInput, bool deallocate)
        {
            TsCDaBrowseElement output = null;

            if (pInput != IntPtr.Zero)
            {
                var element = (OpcRcw.Da.OPCBROWSEELEMENT)Marshal.PtrToStructure(pInput, typeof(OpcRcw.Da.OPCBROWSEELEMENT));

                output = new TsCDaBrowseElement();

                output.Name = element.szName;
                output.ItemPath = null;
                output.ItemName = element.szItemID;
                output.IsItem = ((element.dwFlagValue & OpcRcw.Da.Constants.OPC_BROWSE_ISITEM) != 0);
                output.HasChildren = ((element.dwFlagValue & OpcRcw.Da.Constants.OPC_BROWSE_HASCHILDREN) != 0);
                output.Properties = GetItemProperties(ref element.ItemProperties, deallocate);

                if (deallocate)
                {
                    Marshal.DestroyStructure(pInput, typeof(OpcRcw.Da.OPCBROWSEELEMENT));
                }
            }

            return output;
        }

        /// <summary>
        /// Allocates and marshals an OPCBROWSEELEMENT structure.
        /// </summary>
        internal static OpcRcw.Da.OPCBROWSEELEMENT GetBrowseElement(TsCDaBrowseElement input, bool propertiesRequested)
        {
            var output = new OpcRcw.Da.OPCBROWSEELEMENT();

            if (input != null)
            {
                output.szName = input.Name;
                output.szItemID = input.ItemName;
                output.dwFlagValue = 0;
                output.ItemProperties = GetItemProperties(input.Properties);

                if (input.IsItem)
                {
                    output.dwFlagValue |= OpcRcw.Da.Constants.OPC_BROWSE_ISITEM;
                }

                if (input.HasChildren)
                {
                    output.dwFlagValue |= OpcRcw.Da.Constants.OPC_BROWSE_HASCHILDREN;
                }
            }

            return output;
        }

        /// <summary>
        /// Creates an array of property codes.
        /// </summary>
        internal static int[] GetPropertyIDs(TsDaPropertyID[] propertyIDs)
        {
            var output = new ArrayList();

            if (propertyIDs != null)
            {
                foreach (var propertyID in propertyIDs)
                {
                    output.Add(propertyID.Code);
                }
            }

            return (int[])output.ToArray(typeof(int));
        }

        /// <summary>
        /// Creates an array of property codes.
        /// </summary>
        internal static TsDaPropertyID[] GetPropertyIDs(int[] propertyIDs)
        {
            var output = new ArrayList();

            if (propertyIDs != null)
            {
                foreach (var propertyID in propertyIDs)
                {
                    output.Add(GetPropertyID(propertyID));
                }
            }

            return (TsDaPropertyID[])output.ToArray(typeof(TsDaPropertyID));
        }

        /// <summary>
        /// Unmarshals and deallocates an array of OPCITEMPROPERTIES structures.
        /// </summary>
        internal static TsCDaItemPropertyCollection[] GetItemPropertyCollections(ref IntPtr pInput, int count, bool deallocate)
        {
            TsCDaItemPropertyCollection[] output = null;

            if (pInput != IntPtr.Zero && count > 0)
            {
                output = new TsCDaItemPropertyCollection[count];

                var pos = pInput;

                for (var ii = 0; ii < count; ii++)
                {
                    var list = (OpcRcw.Da.OPCITEMPROPERTIES)Marshal.PtrToStructure(pos, typeof(OpcRcw.Da.OPCITEMPROPERTIES));

                    output[ii] = new TsCDaItemPropertyCollection();
                    output[ii].ItemPath = null;
                    output[ii].ItemName = null;
                    output[ii].Result = Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(list.hrErrorID);

                    var properties = GetItemProperties(ref list, deallocate);

                    if (properties != null)
                    {
                        output[ii].AddRange(properties);
                    }

                    if (deallocate)
                    {
                        Marshal.DestroyStructure(pos, typeof(OpcRcw.Da.OPCITEMPROPERTIES));
                    }

                    pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(OpcRcw.Da.OPCITEMPROPERTIES)));
                }

                if (deallocate)
                {
                    Marshal.FreeCoTaskMem(pInput);
                    pInput = IntPtr.Zero;
                }
            }

            return output;
        }

        /// <summary>
        /// Allocates and marshals an array of OPCITEMPROPERTIES structures.
        /// </summary>
        internal static IntPtr GetItemPropertyCollections(TsCDaItemPropertyCollection[] input)
        {
            var output = IntPtr.Zero;

            if (input != null && input.Length > 0)
            {
                output = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(OpcRcw.Da.OPCITEMPROPERTIES)) * input.Length);

                var pos = output;

                for (var ii = 0; ii < input.Length; ii++)
                {
                    var properties = new OpcRcw.Da.OPCITEMPROPERTIES();

                    if (input[ii].Count > 0)
                    {
                        properties = GetItemProperties((TsCDaItemProperty[])input[ii].ToArray(typeof(TsCDaItemProperty)));
                    }

                    properties.hrErrorID = Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(input[ii].Result);

                    Marshal.StructureToPtr(properties, pos, false);

                    pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(OpcRcw.Da.OPCITEMPROPERTIES)));
                }
            }

            return output;
        }

        /// <summary>
        /// Unmarshals and deallocates a OPCITEMPROPERTIES structures.
        /// </summary>
        internal static TsCDaItemProperty[] GetItemProperties(ref OpcRcw.Da.OPCITEMPROPERTIES input, bool deallocate)
        {
            TsCDaItemProperty[] output = null;

            if (input.dwNumProperties > 0)
            {
                output = new TsCDaItemProperty[input.dwNumProperties];

                var pos = input.pItemProperties;

                for (var ii = 0; ii < output.Length; ii++)
                {
                    try
                    {
                        output[ii] = GetItemProperty(pos, deallocate);
                    }
                    catch (Exception e)
                    {
                        output[ii] = new TsCDaItemProperty();
                        output[ii].Description = e.Message;
                        output[ii].Result = OpcResult.E_FAIL;
                    }

                    pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(OpcRcw.Da.OPCITEMPROPERTY)));
                }

                if (deallocate)
                {
                    Marshal.FreeCoTaskMem(input.pItemProperties);
                    input.pItemProperties = IntPtr.Zero;
                }
            }

            return output;
        }

        /// <summary>
        /// Allocates and marshals an array of OPCITEMPROPERTIES structures.
        /// </summary>
        internal static OpcRcw.Da.OPCITEMPROPERTIES GetItemProperties(TsCDaItemProperty[] input)
        {
            var output = new OpcRcw.Da.OPCITEMPROPERTIES();

            if (input != null && input.Length > 0)
            {
                output.hrErrorID = Result.S_OK;
                output.dwReserved = 0;
                output.dwNumProperties = input.Length;
                output.pItemProperties = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(OpcRcw.Da.OPCITEMPROPERTY)) * input.Length);

                var error = false;

                var pos = output.pItemProperties;

                for (var ii = 0; ii < input.Length; ii++)
                {
                    var property = GetItemProperty(input[ii]);
                    Marshal.StructureToPtr(property, pos, false);
                    pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(OpcRcw.Da.OPCITEMPROPERTY)));

                    if (input[ii].Result.Failed())
                    {
                        error = true;
                    }
                }

                // set flag indicating one or more properties contained errors.
                if (error)
                {
                    output.hrErrorID = Result.S_FALSE;
                }
            }

            return output;
        }

        /// <summary>
        /// Unmarshals and deallocates a OPCITEMPROPERTY structures.
        /// </summary>
        internal static TsCDaItemProperty GetItemProperty(IntPtr pInput, bool deallocate)
        {
            TsCDaItemProperty output = null;

            if (pInput != IntPtr.Zero)
            {
                try
                {
                    var property = (OpcRcw.Da.OPCITEMPROPERTY)Marshal.PtrToStructure(pInput, typeof(OpcRcw.Da.OPCITEMPROPERTY));

                    output = new TsCDaItemProperty();

                    output.ID = GetPropertyID(property.dwPropertyID);
                    output.Description = property.szDescription;
                    output.DataType = Technosoftware.DaAeHdaClient.Com.Interop.GetType((VarEnum)property.vtDataType);
                    output.ItemPath = null;
                    output.ItemName = property.szItemID;
                    output.Value = UnmarshalPropertyValue(output.ID, property.vValue);
                    output.Result = Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(property.hrErrorID);

                    // convert COM DA code to unified DA code.
                    if (property.hrErrorID == Result.E_BADRIGHTS) output.Result = new OpcResult(OpcResult.Da.E_WRITEONLY, Result.E_BADRIGHTS);

                }
                catch (Exception)
                {
                }
                if (deallocate)
                {
                    Marshal.DestroyStructure(pInput, typeof(OpcRcw.Da.OPCITEMPROPERTY));
                }
            }

            return output;
        }

        /// <summary>
        /// Allocates and marshals an arary of OPCITEMPROPERTY structures.
        /// </summary>
        internal static OpcRcw.Da.OPCITEMPROPERTY GetItemProperty(TsCDaItemProperty input)
        {
            var output = new OpcRcw.Da.OPCITEMPROPERTY();

            if (input != null)
            {
                output.dwPropertyID = input.ID.Code;
                output.szDescription = input.Description;
                output.vtDataType = (short)Technosoftware.DaAeHdaClient.Com.Interop.GetType(input.DataType);
                output.vValue = MarshalPropertyValue(input.ID, input.Value);
                output.wReserved = 0;
                output.hrErrorID = Technosoftware.DaAeHdaClient.Com.Interop.GetResultID(input.Result);

                // set the property data type.
                var description = TsDaPropertyDescription.Find(input.ID);

                if (description != null)
                {
                    output.vtDataType = (short)Technosoftware.DaAeHdaClient.Com.Interop.GetType(description.Type);
                }

                // convert unified DA code to COM DA code.
                if (input.Result == OpcResult.Da.E_WRITEONLY) output.hrErrorID = Result.E_BADRIGHTS;
            }

            return output;
        }

        /// <remarks/>
        public static TsDaPropertyID GetPropertyID(int input)
        {
            var fields = typeof(TsDaProperty).GetFields(BindingFlags.Static | BindingFlags.Public);

            foreach (var field in fields)
            {
                var property = (TsDaPropertyID)field.GetValue(typeof(TsDaPropertyID));

                if (input == property.Code)
                {
                    return property;
                }
            }

            return new TsDaPropertyID(input);
        }

        /// <summary>
        /// Converts the property value to a type supported by the unified interface.
        /// </summary>
        internal static object UnmarshalPropertyValue(TsDaPropertyID propertyID, object input)
        {
            if (input == null) return null;

            try
            {
                if (propertyID == TsDaProperty.DATATYPE)
                {
                    return Technosoftware.DaAeHdaClient.Com.Interop.GetType((VarEnum)System.Convert.ToUInt16(input));
                }

                if (propertyID == TsDaProperty.ACCESSRIGHTS)
                {
                    switch (System.Convert.ToInt32(input))
                    {
                        case OpcRcw.Da.Constants.OPC_READABLE: return TsDaAccessRights.Readable;
                        case OpcRcw.Da.Constants.OPC_WRITEABLE: return TsDaAccessRights.Writable;

                        case OpcRcw.Da.Constants.OPC_READABLE | OpcRcw.Da.Constants.OPC_WRITEABLE:
                        {
                            return TsDaAccessRights.ReadWritable;
                        }
                    }

                    return null;
                }

                if (propertyID == TsDaProperty.EUTYPE)
                {
                    switch ((OpcRcw.Da.OPCEUTYPE)input)
                    {
                        case OpcRcw.Da.OPCEUTYPE.OPC_NOENUM: return TsDaEuType.NoEnum;
                        case OpcRcw.Da.OPCEUTYPE.OPC_ANALOG: return TsDaEuType.Analog;
                        case OpcRcw.Da.OPCEUTYPE.OPC_ENUMERATED: return TsDaEuType.Enumerated;
                    }

                    return null;
                }

                if (propertyID == TsDaProperty.QUALITY)
                {
                    return new TsCDaQuality(System.Convert.ToInt16(input));
                }

                // convert UTC time in property to local time for the unified DA interface.
                if (propertyID == TsDaProperty.TIMESTAMP)
                {
                    if (input.GetType() == typeof(DateTime))
                    {
                        var dateTime = (DateTime)input;

                        if (dateTime != DateTime.MinValue)
                        {
                            return dateTime.ToLocalTime();
                        }

                        return dateTime;
                    }
                }
            }
            catch { }

            return input;
        }

        /// <summary>
        /// Converts the property value to a type supported by COM-DA interface.
        /// </summary>
        internal static object MarshalPropertyValue(TsDaPropertyID propertyID, object input)
        {
            if (input == null) return null;

            try
            {
                if (propertyID == TsDaProperty.DATATYPE)
                {
                    return (short)Technosoftware.DaAeHdaClient.Com.Interop.GetType((Type)input);
                }

                if (propertyID == TsDaProperty.ACCESSRIGHTS)
                {
                    switch ((TsDaAccessRights)input)
                    {
                        case TsDaAccessRights.Readable: return OpcRcw.Da.Constants.OPC_READABLE;
                        case TsDaAccessRights.Writable: return OpcRcw.Da.Constants.OPC_WRITEABLE;
                        case TsDaAccessRights.ReadWritable: return OpcRcw.Da.Constants.OPC_READABLE | OpcRcw.Da.Constants.OPC_WRITEABLE;
                    }

                    return null;
                }

                if (propertyID == TsDaProperty.EUTYPE)
                {
                    switch ((TsDaEuType)input)
                    {
                        case TsDaEuType.NoEnum: return OpcRcw.Da.OPCEUTYPE.OPC_NOENUM;
                        case TsDaEuType.Analog: return OpcRcw.Da.OPCEUTYPE.OPC_ANALOG;
                        case TsDaEuType.Enumerated: return OpcRcw.Da.OPCEUTYPE.OPC_ENUMERATED;
                    }

                    return null;
                }

                if (propertyID == TsDaProperty.QUALITY)
                {
                    return ((TsCDaQuality)input).GetCode();
                }

                // convert local time in property to UTC time for the COM DA interface.
                if (propertyID == TsDaProperty.TIMESTAMP)
                {
                    if (input.GetType() == typeof(DateTime))
                    {
                        var dateTime = (DateTime)input;

                        if (dateTime != DateTime.MinValue)
                        {
                            return dateTime.ToUniversalTime();
                        }

                        return dateTime;
                    }
                }
            }
            catch { }

            return input;
        }

        /// <summary>
        /// Converts an array of item values to an array of OPCITEMVQT objects.
        /// </summary>
        internal static OpcRcw.Da.OPCITEMVQT[] GetOPCITEMVQTs(TsCDaItemValue[] input)
        {
            OpcRcw.Da.OPCITEMVQT[] output = null;

            if (input != null)
            {
                output = new OpcRcw.Da.OPCITEMVQT[input.Length];

                for (var ii = 0; ii < input.Length; ii++)
                {
                    output[ii] = new OpcRcw.Da.OPCITEMVQT();

                    var timestamp = (input[ii].TimestampSpecified) ? input[ii].Timestamp : DateTime.MinValue;

                    output[ii].vDataValue = Technosoftware.DaAeHdaClient.Com.Interop.GetVARIANT(input[ii].Value);
                    output[ii].bQualitySpecified = (input[ii].QualitySpecified) ? 1 : 0;
                    output[ii].wQuality = (input[ii].QualitySpecified) ? input[ii].Quality.GetCode() : (short)0;
                    output[ii].bTimeStampSpecified = (input[ii].TimestampSpecified) ? 1 : 0;
                    output[ii].ftTimeStamp = Convert(Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIME(timestamp));
                }

            }

            return output;
        }

        /// <summary>
        /// Converts an array of item objects to an array of GetOPCITEMDEF objects.
        /// </summary>
        internal static OpcRcw.Da.OPCITEMDEF[] GetOPCITEMDEFs(TsCDaItem[] input)
        {
            OpcRcw.Da.OPCITEMDEF[] output = null;

            if (input != null)
            {
                output = new OpcRcw.Da.OPCITEMDEF[input.Length];

                for (var ii = 0; ii < input.Length; ii++)
                {
                    output[ii] = new OpcRcw.Da.OPCITEMDEF();

                    output[ii].szItemID = input[ii].ItemName;
                    output[ii].szAccessPath = (input[ii].ItemPath == null) ? string.Empty : input[ii].ItemPath;
                    output[ii].bActive = (input[ii].ActiveSpecified) ? ((input[ii].Active) ? 1 : 0) : 1;
                    output[ii].vtRequestedDataType = (short)Technosoftware.DaAeHdaClient.Com.Interop.GetType(input[ii].ReqType);
                    output[ii].hClient = 0;
                    output[ii].dwBlobSize = 0;
                    output[ii].pBlob = IntPtr.Zero;
                }
            }

            return output;
        }

        /// <summary>
        /// Unmarshals and deallocates a OPCITEMSTATE structures.
        /// </summary>
        internal static TsCDaItemValue[] GetItemValues(ref IntPtr pInput, int count, bool deallocate)
        {
            TsCDaItemValue[] output = null;

            if (pInput != IntPtr.Zero && count > 0)
            {
                output = new TsCDaItemValue[count];

                var pos = pInput;

                for (var ii = 0; ii < count; ii++)
                {
                    var result = (OpcRcw.Da.OPCITEMSTATE)Marshal.PtrToStructure(pos, typeof(OpcRcw.Da.OPCITEMSTATE));

                    output[ii] = new TsCDaItemValue();
                    output[ii].ClientHandle = result.hClient;
                    output[ii].Value = result.vDataValue;
                    output[ii].Quality = new TsCDaQuality(result.wQuality);
                    output[ii].QualitySpecified = true;
                    output[ii].Timestamp = Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIME(Convert(result.ftTimeStamp));
                    output[ii].TimestampSpecified = output[ii].Timestamp != DateTime.MinValue;

                    if (deallocate)
                    {
                        Marshal.DestroyStructure(pos, typeof(OpcRcw.Da.OPCITEMSTATE));
                    }

                    pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(OpcRcw.Da.OPCITEMSTATE)));
                }

                if (deallocate)
                {
                    Marshal.FreeCoTaskMem(pInput);
                    pInput = IntPtr.Zero;
                }
            }

            return output;
        }

        /// <summary>
        /// Unmarshals and deallocates a OPCITEMRESULT structures.
        /// </summary>
        internal static int[] GetItemResults(ref IntPtr pInput, int count, bool deallocate)
        {
            int[] output = null;

            if (pInput != IntPtr.Zero && count > 0)
            {
                output = new int[count];

                var pos = pInput;

                for (var ii = 0; ii < count; ii++)
                {
                    var result = (OpcRcw.Da.OPCITEMRESULT)Marshal.PtrToStructure(pos, typeof(OpcRcw.Da.OPCITEMRESULT));

                    output[ii] = result.hServer;

                    if (deallocate)
                    {
                        Marshal.FreeCoTaskMem(result.pBlob);
                        result.pBlob = IntPtr.Zero;

                        Marshal.DestroyStructure(pos, typeof(OpcRcw.Da.OPCITEMRESULT));
                    }

                    pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(OpcRcw.Da.OPCITEMRESULT)));
                }

                if (deallocate)
                {
                    Marshal.FreeCoTaskMem(pInput);
                    pInput = IntPtr.Zero;
                }
            }

            return output;
        }

        /// <summary>
        /// Allocates and marshals an array of OPCBROWSEELEMENT structures.
        /// </summary>
        internal static IntPtr GetItemStates(TsCDaItemValueResult[] input)
        {
            var output = IntPtr.Zero;

            if (input != null && input.Length > 0)
            {
                output = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(OpcRcw.Da.OPCITEMSTATE)) * input.Length);

                var pos = output;

                for (var ii = 0; ii < input.Length; ii++)
                {
                    var item = new OpcRcw.Da.OPCITEMSTATE();

                    item.hClient = System.Convert.ToInt32(input[ii].ClientHandle);
                    item.vDataValue = input[ii].Value;
                    item.wQuality = (input[ii].QualitySpecified) ? input[ii].Quality.GetCode() : (short)0;
                    item.ftTimeStamp = Convert(Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIME(input[ii].Timestamp));
                    item.wReserved = 0;

                    Marshal.StructureToPtr(item, pos, false);
                    pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(OpcRcw.Da.OPCITEMSTATE)));
                }
            }

            return output;
        }
    }
}
