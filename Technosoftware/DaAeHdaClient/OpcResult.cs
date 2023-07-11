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
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>Contains an unique identifier for an OPC specific result code.</summary>
    /// <remarks>Most functions raises a OpcResultException if an error occur.</remarks>
    /// <seealso cref="OpcResultException">OpcResultException Class</seealso>
    [Serializable]
    public struct OpcResult : ISerializable
    {
        #region Serialization Functions
        /// <summary>
        /// A set of names for fields used in serialization.
        /// </summary>
        private class Names
        {
            internal const string Name = "NA";
            internal const string Namespace = "NS";
            internal const string Code = "CO";
        }

        // During deserialization, SerializationInfo is passed to the class using the constructor provided for this purpose. Any visibility 
        // constraints placed on the constructor are ignored when the object is deserialized; so you can mark the class as public, 
        // protected, internal, or private. However, it is best practice to make the constructor protected unless the class is sealed, in which case 
        // the constructor should be marked private. The constructor should also perform thorough input validation. To avoid misuse by malicious code, 
        // the constructor should enforce the same security checks and permissions required to obtain an instance of the class using any other 
        // constructor. 
        /// <summary>
        /// Construct a server by de-serializing its URL from the stream.
        /// </summary>
        public OpcResult(SerializationInfo info, StreamingContext context)
        {
            var name = (string)info.GetValue(Names.Name, typeof(string));
            var ns = (string)info.GetValue(Names.Namespace, typeof(string));
            name_ = new XmlQualifiedName(name, ns);
            code_ = (int)info.GetValue(Names.Code, typeof(int));
            type_ = CodeType.OpcSysCode;
            caller_ = null;
            message_ = null;
        }

        /// <summary>
        /// Serializes a server into a stream.
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (name_ != null)
            {
                info.AddValue(Names.Name, name_.Name);
                info.AddValue(Names.Namespace, name_.Namespace);
            }
            info.AddValue(Names.Code, code_);
        }
        #endregion

        /// <summary>
        /// Specifies the type identifier of the result or error code.
        /// </summary>
        public enum CodeType
        {
            /// <summary>
            /// System  specific code (result/error) returned by a system function. 
            /// </summary>
            SysCode,
            /// <summary>
            /// System  specific code (result/error) returned by an OPC function. 
            /// </summary>
            OpcSysCode,

            /// <summary>
            /// Data Access specific code (result/error) returned by an OPC function.
            /// </summary>
            DaCode,

            /// <summary>
            /// Alarms &amp; Events specific code (result/error) returned by an OPC function.
            /// </summary>
            AeCode,

            /// <summary>
            /// XML-DA  specific code (result/error) returned by an OPC function.
            /// </summary>
            XdaCode

        };

        /// <summary>
        /// Specifies the type of function call which returned the result or error code. This enumeration values are used only by the constructor of the OpcResult object.
        /// </summary>
        public enum FuncCallType
        {
            /// <summary>
            /// Identifies the code (result/error) passed to the constructor as a result of a system function.
            /// </summary>
            SysFuncCall,

            /// <summary>
            /// Identifies the code (result/error) passed to the constructor as a result of an OPC Data Access function.
            /// </summary>
            DaFuncCall,

            /// <summary>
            /// Identifies the code (result/error) passed to the constructor as a result of an OPC Alarms &amp; Events function.
            /// </summary>
            AeFuncCall,

        };

        /// <summary>
        /// Used for result codes identified by a qualified name.
        /// </summary>
        public XmlQualifiedName Name => name_;

        /// <summary>
		/// Used for result codes identified by a integer.
		/// </summary>
		public int Code => code_;

        /// <summary>
		/// Returns true if the objects are equal.
		/// </summary>
		public static bool operator ==(OpcResult a, OpcResult b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Returns true if the objects are not equal.
        /// </summary>
        public static bool operator !=(OpcResult a, OpcResult b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Checks for the 'S_' prefix that indicates a success condition.
        /// </summary>
        public bool Succeeded()
        {
            if (Code != -1) return (Code >= 0);
            if (Name != null) return Name.Name.StartsWith("S_");
            return false;
        }

        /// <summary>
        /// Checks for the 'E_' prefix that indicates an error condition.
        /// </summary>
        public bool Failed()
        {
            if (Code != -1) return (Code < 0);
            if (Name != null) return Name.Name.StartsWith("E_");
            return false;
        }

        /// <summary>
        /// Retrieves the type identifier of the code passed to the constructor.   
        /// </summary>
        /// <returns>CodeType   of the HRESULT code</returns>
        internal CodeType Type()
        {
            return (type_);
        }

        /// <summary>
        /// Indicates whether the result code represents an error value.  
        /// </summary>
        /// <returns>This function returns true if the associated result code is an error code.</returns>
        public bool IsError()
        {
            return (code_ < 0);
        }

        /// <summary>
        /// Indicates whether the result code represents an error value.  
        /// </summary>
        internal static bool IsError(int hResult)
        {
            return (hResult < 0);
        }

        /// <summary>
        /// Indicates whether the result code represents an error free value. 
        /// </summary>
        /// <returns>This function returns true if the associated result code is an error free value.</returns>
        public bool IsSuccess()
        {
            return (code_ >= 0);

        }

        /// <summary>
        /// Indicates whether the result code represents an error free value. 
        /// </summary>
        internal static bool IsSuccess(int hResult)
        {
            return (hResult >= 0);

        }

        /// <summary>
        /// Indicates whether the result code represents an error value.
        /// </summary>
        /// <returns>This function returns true if the associated result code is 0.</returns>
        public bool IsOk()
        {
            return (code_ == 0);
        }

        /// <summary>
        /// Retrieves a text string with a description for the code stored in the OpcResult object.
        /// </summary>
        /// <returns>This method returns the description for the code recorded within the OpcResult object. If no description text is
        /// found, then a generic message "Server error 0x#dwErrorCode" is returned.</returns>
        public string Description()
        {
            switch (type_)
            {
                case CodeType.DaCode:
                    if (caller_ != null)
                    {
                        message_ = ((Technosoftware.DaAeHdaClient.Da.TsCDaServer)caller_).GetErrorText(((Technosoftware.DaAeHdaClient.Da.TsCDaServer)caller_).GetLocale(), this);
                    }
                    else
                    {
                        message_ = $"Server error 0x{code_,0:X}";
                    }
                    break;

                default:
                    message_ = GetSystemMessage(code_, LOCALE_SYSTEM_DEFAULT) ??
                               $"Server error 0x{code_,0:X}";
                    break;
            }
            return message_;
        }

        #region Constructors
        /// <summary>
        /// Constructs a OpcResult object.
        /// </summary>
        /// <param name="hResult">The code returned by a system function or OPC function. The code can be retrieved with the member function Code() and a description text can be retrieved with the member function <see cref="Description" />. </param>
        /// <param name="eFuncType">Specifies the type of function which has returned the code. This parameter is used to create the code type which can be retrieved with the member function <see cref="Type" />. </param>
        /// <param name="caller">Object which caused the error. Can be null</param>
        /// <returns></returns>
        public OpcResult(int hResult, FuncCallType eFuncType, object caller)
        {
            name_ = null;
            message_ = null;
            code_ = hResult;
            caller_ = caller;
            type_ = CodeType.OpcSysCode;

            if (eFuncType == FuncCallType.SysFuncCall)
            {
                type_ = CodeType.SysCode;                              // System specific errror returned by a system function
            }
            else if ((((hResult) >> 16) & 0x1fff) == 0x4)
            {     // FACILITY_ITF                    0x4
                if (eFuncType == FuncCallType.DaFuncCall) type_ = CodeType.DaCode;
                else type_ = CodeType.AeCode;
            }
            else
            {
                type_ = CodeType.OpcSysCode;                           // System specific error returned by an OPC function
            }
        }

        /// <summary>
        /// Constructs a OpcResult object.
        /// </summary>
        /// <param name="resultId">The code returned by a   system function or OPC function. The code can be retrieved with the member function Code() and a description text can be retrieved with the member function <see cref="Description" />. </param>
        /// <param name="eFuncType">Specifies   the type of function which has returned the code. This parameter is used to create the code type which can be retrieved with the member function <see cref="Type" />. </param>
        /// <param name="caller">Object which   caused the error. Can be null</param>
        /// <returns></returns>
        public OpcResult(OpcResult resultId, FuncCallType eFuncType, object caller)
        {
            name_ = null;
            message_ = null;
            code_ = resultId.Code;
            caller_ = caller;
            type_ = CodeType.OpcSysCode;

            if (eFuncType == FuncCallType.SysFuncCall)
            {
                type_ = CodeType.SysCode;                              // System specific error returned by a system function
            }
            else if ((((resultId.Code) >> 16) & 0x1fff) == 0x4)
            {     // FACILITY_ITF                    0x4
                if (eFuncType == FuncCallType.DaFuncCall) type_ = CodeType.DaCode;
                else type_ = CodeType.AeCode;
            }
            else
            {
                type_ = CodeType.OpcSysCode;                           // System specific error returned by an OPC function
            }
        }

        /// <summary>
        /// Initializes a result code identified by a qualified name.
        /// </summary>
        internal OpcResult(XmlQualifiedName name)
        {
            name_ = name;
            message_ = null;
            code_ = -1;
            type_ = CodeType.XdaCode;
            caller_ = null;
        }

        /// <summary>
		/// Initializes a result code identified by an integer.
		/// </summary>
		public OpcResult(long code)
        {
            name_ = null;
            message_ = null;


            if (code > int.MaxValue)
            {
                code = -(((long)uint.MaxValue) + 1 - code);
            }

            code_ = (int)code;

            type_ = CodeType.OpcSysCode;
            caller_ = null;

        }

        /// <summary>
        /// Initializes a result code identified by a qualified name.
        /// </summary>
        public OpcResult(string name, string ns)
        {
            name_ = new XmlQualifiedName(name, ns);
            message_ = null;

            code_ = -1;
            type_ = CodeType.OpcSysCode;
            caller_ = null;

        }

        /// <summary>
        /// Initializes a   result code identified by a qualified name and a specific result code.
        /// </summary>
        public OpcResult(string name, string ns, long code)
        {
            name_ = new XmlQualifiedName(name, ns);
            if (code > int.MaxValue)
            {
                code = -(((long)uint.MaxValue) + 1 - code);
            }

            code_ = (int)code;
            type_ = CodeType.OpcSysCode;
            caller_ = null;
            message_ = null;

        }

        /// <summary>
        /// Initializes a result code with a general result code and a specific result code.
        /// </summary>
        public OpcResult(OpcResult resultId, long code)
        {
            name_ = resultId.Name;

            if (code > int.MaxValue)
            {
                code = -(((long)uint.MaxValue) + 1 - code);
            }

            code_ = (int)code;
            type_ = CodeType.OpcSysCode;
            caller_ = null;
            message_ = null;

        }
        #endregion

        #region Private Methods
        /// <summary>
        /// The constant used to selected the default locale.
        /// </summary>
        internal const int LOCALE_SYSTEM_DEFAULT = 0x800;

        /// <summary>
        /// The WIN32 user default locale.
        /// </summary>
        public const int LOCALE_USER_DEFAULT = 0x400;

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

        [DllImport("Kernel32.dll")]
        private static extern int GetSystemDefaultLangID();

        [DllImport("Kernel32.dll")]
        private static extern int GetUserDefaultLangID();

        /// <summary>
        /// Retrieves the system message text for the specified error.
        /// </summary>
        private static string GetSystemMessage(int error)
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

                if (!string.IsNullOrEmpty(msg))
                {
                    return msg.Trim();
                }
            }

            return $"0x{error:X8}";
        }
        #endregion

        #region Object Method Overrides
        /// <summary>
        /// Returns true if the target object is equal to the object.
        /// </summary>
        public override bool Equals(object target)
        {
            if (target != null && (target is OpcResult))
            {
                var resultId = (OpcResult)target;

                // compare by integer if both specify valid integers.
                if (resultId.Code != -1 && Code != -1)
                {
                    return (resultId.Code == Code) && (resultId.Name == Name);
                }

                // compare by name if both specify valid names.
                if (resultId.Name != null && Name != null)
                {
                    return (resultId.Name == Name);
                }
            }

            return false;
        }

        /// <summary>
        /// Formats the result identifier as a string.
        /// </summary>
        public override string ToString()
        {
            if (Name != null) return Name.Name;
            return $"0x{Code,0:X}";
        }

        /// <summary>
        /// Returns a useful hash code for the object.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #region Private Members
        private XmlQualifiedName name_;
        private int code_;
        private CodeType type_;
        private object caller_;
        private string message_;


        #endregion

        /// <summary>The function was successful (Return Code: 0x00000000).</summary>
        public static readonly OpcResult S_OK = new OpcResult("S_OK", OpcNamespace.OPC_DATA_ACCESS, 0x00000000);
        /// <summary>The function completed with an error (Return Code: 0x00000001).</summary>
        public static readonly OpcResult S_FALSE = new OpcResult("S_FALSE", OpcNamespace.OPC_DATA_ACCESS, 0x00000001);
        /// <summary>The function was unsuccessfull (Return Code: 0x80004005).</summary>
        public static readonly OpcResult E_FAIL = new OpcResult("E_FAIL", OpcNamespace.OPC_DATA_ACCESS, 0x80004005);
        /// <summary>The interface asked for is not supported   by the server (Return Code: 0x80004002).</summary>
        public static readonly OpcResult E_NOINTERFACE = new OpcResult("E_NOINTERFACE", OpcNamespace.OPC_DATA_ACCESS, 0x80004002);
        /// <summary>The value of one or more parameters was not valid. This is generally used in place of a more specific error where it is expected that problems are unlikely or will be easy to identify (for example when there is only one parameter) (Return Code: 0x80070057).</summary>
        public static readonly OpcResult E_INVALIDARG = new OpcResult("E_INVALIDARG", OpcNamespace.OPC_DATA_ACCESS, 0x80070057);
        /// <summary>Function is not implemented (Return Code: 0x80004001).</summary>
        public static readonly OpcResult E_NOTIMPL = new OpcResult("E_NOTIMPL", OpcNamespace.OPC_DATA_ACCESS, 0x80004001);
        /// <summary>Not enough memory to   complete the requested operation. This can happen any time the server needs to allocate memory to complete the requested operation (Return Code: 0x8007000E).</summary>
        public static readonly OpcResult E_OUTOFMEMORY = new OpcResult("E_OUTOFMEMORY", OpcNamespace.OPC_DATA_ACCESS, 0x8007000E);
        /// <summary>Return Code:   0x80004004</summary>
        public static readonly OpcResult E_ABORT = new OpcResult("E_ABORT", OpcNamespace.OPC_DATA_ACCESS, 0x80004004);
        /// <summary>NULL   pointer argument.</summary>
        public static readonly OpcResult E_POINTER = new OpcResult("E_POINTER", OpcNamespace.OPC_DATA_ACCESS, 0x80004003);
        /// <summary>Cannot Unadvise - there is no existing connection (Return Code: 0x80040200).</summary>
        public static readonly OpcResult CONNECT_E_NOCONNECTION = new OpcResult("CONNECT_E_NOCONNECTION", OpcNamespace.OPC_DATA_ACCESS, 0x80040200);
        /// <summary>The operation took too long to complete.</summary>
        public static readonly OpcResult E_TIMEDOUT = new OpcResult("E_TIMEDOUT", OpcNamespace.OPC_DATA_ACCESS);
        /// <summary>General network error.</summary>
        public static readonly OpcResult E_NETWORK_ERROR = new OpcResult("E_NETWORK_ERROR", OpcNamespace.OPC_DATA_ACCESS);
        /// <summary>The server denies access (Return Code: 0x80070005).</summary>
        public static readonly OpcResult E_ACCESS_DENIED = new OpcResult("E_ACCESS_DENIED", OpcNamespace.OPC_DATA_ACCESS, 0x80070005);
        /// <summary>Invalid class string (Return Code: 0x800401F3).</summary>
        public static readonly OpcResult CO_E_CLASSSTRING = new OpcResult("CO_E_CLASSSTRING", OpcNamespace.OPC_DATA_ACCESS, 0x800401F3);
        /// <summary>The object application has been disconnected from the remoting system (Return Code: 0x800401FD).</summary>
        public static readonly OpcResult CO_E_OBJNOTCONNECTED = new OpcResult("CO_E_OBJNOTCONNECTED", OpcNamespace.OPC_DATA_ACCESS, 0x800401fd);
        /// <summary>
        /// The server does not support the requested function with the specified parameters.
        /// </summary>
        public static readonly OpcResult E_NOTSUPPORTED = new OpcResult("E_NOTSUPPORTED", OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// Results codes for Data Access.
        /// </summary>
        public class Da
        {
            /// <summary>Indicates  that not every detected change has been returned for this item. This is an indicator that servers buffer reached its limit and had to purge out the oldest data. Only the most recent data is provided. The server should only remove the oldest data for those items that have newer samples available in the buffer. This will allow single samplings of older items to be returned to the client (Return Code: 0x00040404).</summary>
            public static readonly OpcResult S_DATAQUEUEOVERFLOW = new OpcResult("S_DATAQUEUEOVERFLOW", OpcNamespace.OPC_DATA_ACCESS, 0x00040404);
            /// <summary>The server does not support the requested data rate but will use the closest available rate (Return Code: 0x0004000D).</summary>
            public static readonly OpcResult S_UNSUPPORTEDRATE = new OpcResult("S_UNSUPPORTEDRATE", OpcNamespace.OPC_DATA_ACCESS, 0x0004000D);
            /// <summary>A value passed to WRITE was accepted but the output was clamped (Return Code: 0x0004000E).</summary>
            public static readonly OpcResult S_CLAMP = new OpcResult("S_CLAMP", OpcNamespace.OPC_DATA_ACCESS, 0x0004000E);
            /// <summary>The value  of the handle is invalid.
            /// Note: a client  should never pass an invalid handle to a server. If this error occurs, it is due to a programming error in the client or possibly in the server (Return Code: 0xC0040001).</summary>
            public static readonly OpcResult E_INVALIDHANDLE = new OpcResult("E_INVALIDHANDLE", OpcNamespace.OPC_DATA_ACCESS, 0xC0040001);
            /// <summary>The item ID is not defined in  the server address space (on add or validate) or no longer exists in the server address space (for read or write) (Returnd Code: 0xC0040007).</summary>
            public static readonly OpcResult E_UNKNOWN_ITEM_NAME = new OpcResult("E_UNKNOWN_ITEM_NAME", OpcNamespace.OPC_DATA_ACCESS, 0xC0040007);
            /// <summary>The item name does not conform the server's syntax.</summary>
            public static readonly OpcResult E_INVALID_ITEM_NAME = new OpcResult("E_INVALID_ITEM_NAME", OpcNamespace.OPC_DATA_ACCESS);
            /// <summary>The item path  is no longer available in the server address space.</summary>
            public static readonly OpcResult E_UNKNOWN_ITEM_PATH = new OpcResult("E_UNKNOWN_ITEM_PATH", OpcNamespace.OPC_DATA_ACCESS);
            /// <summary>The item path  does not conform the server's syntax</summary>
            public static readonly OpcResult E_INVALID_ITEM_PATH = new OpcResult("E_INVALID_ITEM_PATH", OpcNamespace.OPC_DATA_ACCESS);
            /// <summary>The passed property ID is not valid for the item (Return Code: 0xC0040203).</summary>
            public static readonly OpcResult E_INVALID_PID = new OpcResult("E_INVALID_PID", OpcNamespace.OPC_DATA_ACCESS, 0xC0040203);
            /// <summary>An invalid subscription handle was passed to the request.</summary>
            public static readonly OpcResult E_NO_SUBSCRIPTION = new OpcResult("E_NO_SUBSCRIPTION", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The value is read only and may not be written to</summary>
            public static readonly OpcResult E_READONLY = new OpcResult("E_READONLY", OpcNamespace.OPC_DATA_ACCESS);
            /// <summary>The value is write-only and may not be read from or returned as part of a write response</summary>
            public static readonly OpcResult E_WRITEONLY = new OpcResult("E_WRITEONLY", OpcNamespace.OPC_DATA_ACCESS);
            /// <summary>The server cannot  convert the data between the specified format/ requested data type and the canonical data type (Return Code: 0xC0040004).</summary>
            public static readonly OpcResult E_BADTYPE = new OpcResult("E_BADTYPE", OpcNamespace.OPC_DATA_ACCESS, 0xC0040004);
            /// <summary>The value  was out of range (Return Code: 0xC004000B).</summary>
            public static readonly OpcResult E_RANGE = new OpcResult("E_RANGE", OpcNamespace.OPC_DATA_ACCESS, 0xC004000B);
            /// <summary>Duplicate name not allowed.</summary>
            public static readonly OpcResult E_DUPLICATENAME = new OpcResult("E_DUPLICATENAME", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The filter string  was not valid (Return Code: 0xC0040009).</summary>
            public static readonly OpcResult E_INVALID_FILTER = new OpcResult("E_INVALID_FILTER", OpcNamespace.OPC_DATA_ACCESS, 0xC0040009);
            /// <summary>The continuation point is  not valid (Return Code: 0xC0040403).</summary>
            public static readonly OpcResult E_INVALIDCONTINUATIONPOINT = new OpcResult("E_INVALIDCONTINUATIONPOINT", OpcNamespace.OPC_DATA_ACCESS, 0xC0040403);
            /// <summary>The server does not support writing of quality and/or timestamp.</summary>
            public static readonly OpcResult E_NO_WRITEQT = new OpcResult("E_NO_WRITEQT", OpcNamespace.OPC_DATA_ACCESS);
            /// <summary>The item deadband  has not been set for this item (Return Code: 0xC0040400).</summary>
            public static readonly OpcResult E_NO_ITEM_DEADBAND = new OpcResult("E_NO_ITEM_DEADBAND", OpcNamespace.OPC_DATA_ACCESS, 0xC0040400);
            /// <remarks/>
            public static readonly OpcResult E_NO_ITEM_SAMPLING = new OpcResult("E_NO_ITEM_SAMPLING", OpcNamespace.OPC_DATA_ACCESS);
            /// <summary>The server does not support buffering  of data items that are collected at a faster rate than the subscription update rate (Return Code: 0xC0040402)</summary>
            public static readonly OpcResult E_NO_ITEM_BUFFERING = new OpcResult("E_NO_ITEM_BUFFERING", OpcNamespace.OPC_DATA_ACCESS, 0xC0040402);
            //  
            //public  static readonly OpcResult E_DUPPLICATE_FULLITEMNAME   = new OpcResult("E_DUPPLICATE_FULLITEMNAME",  Namespace.OPC_DATA_ACCESS_XML10);
        }

        /// <summary>
        /// Results codes for XML-DA.
        /// </summary>
        public class Xda
        {

            /// <summary>The function was successful.</summary>
            public static readonly OpcResult S_OK = new OpcResult("S_OK", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>A value passed to WRITE was accepted but the output was clamped.</summary>
            public static readonly OpcResult S_CLAMP = new OpcResult("S_CLAMP", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>Indicates  that not every detected change has been returned for this item. This is an indicator that servers buffer reached its limit and had to purge out the oldest data. Only the most recent data is provided. The server should only remove the oldest data for those items that have newer samples available in the buffer. This will allow single samplings of older items to be returned to the client.</summary>
            public static readonly OpcResult S_DATAQUEUEOVERFLOW = new OpcResult("S_DATAQUEUEOVERFLOW", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The server does not support the requested data rate but will use the closest available rate.</summary>
            public static readonly OpcResult S_UNSUPPORTEDRATE = new OpcResult("S_UNSUPPORTEDRATE", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The server denies access.</summary>
            public static readonly OpcResult E_ACCESS_DENIED = new OpcResult("E_ACCESS_DENIED", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>A refresh is currently in progress.</summary>
            public static readonly OpcResult E_BUSY = new OpcResult("E_BUSY", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The function was unsuccessfull.</summary>
            public static readonly OpcResult E_FAIL = new OpcResult("E_FAIL", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The continuation point is  not valid.</summary>
            public static readonly OpcResult E_INVALIDCONTINUATIONPOINT = new OpcResult("E_INVALIDCONTINUATIONPOINT", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The filter string is not valid.</summary>
            public static readonly OpcResult E_INVALIDFILTER = new OpcResult("E_INVALIDFILTER", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The hold time is too long (determined by server).</summary>
            public static readonly OpcResult E_INVALIDHOLDTIME = new OpcResult("E_INVALIDHOLDTIME", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The item name does not conform the server's syntax.</summary>
            public static readonly OpcResult E_INVALIDITEMNAME = new OpcResult("E_INVALIDITEMNAME", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The item path does not conform the server's syntax</summary>
            public static readonly OpcResult E_INVALIDITEMPATH = new OpcResult("E_INVALIDITEMPATH", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The passed property ID is not valid for the item.</summary>
            public static readonly OpcResult E_INVALIDPID = new OpcResult("E_INVALIDPID", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>An invalid subscription handle was passed to the request.</summary>
            public static readonly OpcResult E_NO_SUBSCRIPTION = new OpcResult("E_NO_SUBSCRIPTION", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The server does not support the requested function with the specified parameters.</summary>
            public static readonly OpcResult E_NOT_SUPPORTED = new OpcResult("E_NOT_SUPPORTED", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>Not enough memory to complete the requested operation. This can happen any time the server needs to allocate memory to complete the requested operation.</summary>
            public static readonly OpcResult E_OUTOFMEMORY = new OpcResult("E_OUTOFMEMORY", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The value was out of range.</summary>
            public static readonly OpcResult E_RANGE = new OpcResult("E_RANGE", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The value is read only and may not be written to</summary>
			public static readonly OpcResult E_READONLY = new OpcResult("E_READONLY", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The operation could not complete due to an abnormal server state.</summary>
            public static readonly OpcResult E_SERVERSTATE = new OpcResult("E_SERVERSTATE", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The operation took too long to complete.</summary>
            public static readonly OpcResult E_TIMEDOUT = new OpcResult("E_TIMEDOUT", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The item ID is not defined in  the server address space (on add or validate) or no longer exists in the server address space (for read or write).</summary>
            public static readonly OpcResult E_UNKNOWNITEMNAME = new OpcResult("E_UNKNOWNITEMNAME", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The item path  is no longer available in the server address space.</summary>
            public static readonly OpcResult E_UNKNOWNITEMPATH = new OpcResult("E_UNKNOWNITEMPATH", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The value is write-only and may not be read from or returned as part of a write response</summary>
            public static readonly OpcResult E_WRITEONLY = new OpcResult("E_WRITEONLY", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The value of one or more parameters was not valid. This is generally used in place of a more specific error where it is expected that problems are unlikely or will be easy to identify (for example when there is only one parameter).</summary>
            public static readonly OpcResult E_INVALIDARG = new OpcResult("E_INVALIDARG", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>The server cannot  convert the data between the specified format/ requested data type and the canonical data type.</summary>
            public static readonly OpcResult E_BADTYPE = new OpcResult("E_BADTYPE", OpcNamespace.OPC_DATA_ACCESS_XML10);
            /// <summary>Item whith this FullItemName was allready defined.</summary>
            public static readonly OpcResult E_DUPPLICATE_FULLITEMNAME = new OpcResult("E_DUPPLICATE_FULLITEMNAME", OpcNamespace.OPC_DATA_ACCESS_XML10);
        }

        /// <summary>
		/// Results codes for Complex Data.
		/// </summary>
		public class Cpx
        {
            /// <summary>
            /// The dictionary and/or type description for the item has changed.
            /// </summary>
            public static readonly OpcResult E_TYPE_CHANGED = new OpcResult("E_TYPE_CHANGED", OpcNamespace.OPC_COMPLEX_DATA);
            /// <summary>
            /// A data filter item with the specified name already exists.
            /// </summary>
            public static readonly OpcResult E_FILTER_DUPLICATE = new OpcResult("E_FILTER_DUPLICATE", OpcNamespace.OPC_COMPLEX_DATA);
            /// <summary>
            /// The data filter value does not conform to the server's syntax.
            /// </summary>
            public static readonly OpcResult E_FILTER_INVALID = new OpcResult("E_FILTER_INVALID", OpcNamespace.OPC_COMPLEX_DATA);
            /// <summary>
            /// An error occurred when the filter value was applied to the source data.
            /// </summary>
            public static readonly OpcResult E_FILTER_ERROR = new OpcResult("E_FILTER_ERROR", OpcNamespace.OPC_COMPLEX_DATA);
            /// <summary>
            /// The item value is empty because the data filter has excluded all fields.
            /// </summary>
            public static readonly OpcResult S_FILTER_NO_DATA = new OpcResult("S_FILTER_NO_DATA", OpcNamespace.OPC_COMPLEX_DATA);
        }

        /// <summary>
        /// Results codes for Historical Data Access.
        /// </summary>
        public class Hda
        {
            /// <summary>The server does not support writing of quality and/or timestamp.</summary>
            public static readonly OpcResult E_MAXEXCEEDED = new OpcResult("E_MAXEXCEEDED", OpcNamespace.OPC_HISTORICAL_DATA_ACCESS);
            /// <summary>There is no data within the specified parameters.</summary>
            public static readonly OpcResult S_NODATA = new OpcResult("S_NODATA", OpcNamespace.OPC_HISTORICAL_DATA_ACCESS);
            /// <summary>There is more data satisfying the query than was returned.</summary>
            public static readonly OpcResult S_MOREDATA = new OpcResult("S_MOREDATA", OpcNamespace.OPC_HISTORICAL_DATA_ACCESS);
            /// <summary>The aggregate requested is not valid.</summary>
            public static readonly OpcResult E_INVALIDAGGREGATE = new OpcResult("E_INVALIDAGGREGATE", OpcNamespace.OPC_HISTORICAL_DATA_ACCESS);
            /// <summary>The server only returns current values for the requested item attributes.</summary>
            public static readonly OpcResult S_CURRENTVALUE = new OpcResult("S_CURRENTVALUE", OpcNamespace.OPC_HISTORICAL_DATA_ACCESS);
            /// <summary>Additional data satisfying the query was found.</summary>
            public static readonly OpcResult S_EXTRADATA = new OpcResult("S_EXTRADATA", OpcNamespace.OPC_HISTORICAL_DATA_ACCESS);
            /// <summary>The server does not support this filter.</summary>
            public static readonly OpcResult W_NOFILTER = new OpcResult("W_NOFILTER", OpcNamespace.OPC_HISTORICAL_DATA_ACCESS);
            /// <summary>The server does not support this attribute.</summary>
            public static readonly OpcResult E_UNKNOWNATTRID = new OpcResult("E_UNKNOWNATTRID", OpcNamespace.OPC_HISTORICAL_DATA_ACCESS);
            /// <summary>The requested aggregate is not available for the specified item.</summary>
            public static readonly OpcResult E_NOT_AVAIL = new OpcResult("E_NOT_AVAIL", OpcNamespace.OPC_HISTORICAL_DATA_ACCESS);
            /// <summary>The supplied value for the attribute is not a correct data type.</summary>
            public static readonly OpcResult E_INVALIDDATATYPE = new OpcResult("E_INVALIDDATATYPE", OpcNamespace.OPC_HISTORICAL_DATA_ACCESS);
            /// <summary>Unable to insert - data already present.</summary>
            public static readonly OpcResult E_DATAEXISTS = new OpcResult("E_DATAEXISTS", OpcNamespace.OPC_HISTORICAL_DATA_ACCESS);
            /// <summary>The supplied attribute ID is not valid.</summary>
            public static readonly OpcResult E_INVALIDATTRID = new OpcResult("E_INVALIDATTRID", OpcNamespace.OPC_HISTORICAL_DATA_ACCESS);
            /// <summary>The server has no value for the specified time and item ID.</summary>
            public static readonly OpcResult E_NODATAEXISTS = new OpcResult("E_NODATAEXISTS", OpcNamespace.OPC_HISTORICAL_DATA_ACCESS);
            /// <summary>The requested insert occurred.</summary>
            public static readonly OpcResult S_INSERTED = new OpcResult("S_INSERTED", OpcNamespace.OPC_HISTORICAL_DATA_ACCESS);
            /// <summary>The requested replace occurred.</summary>
            public static readonly OpcResult S_REPLACED = new OpcResult("S_REPLACED", OpcNamespace.OPC_HISTORICAL_DATA_ACCESS);
        }

        /// <summary>
		/// Results codes for Alarms and Events
		/// </summary>
		public class Ae
        {
            /// <summary>The condition has already been acknowleged.</summary>
            public static readonly OpcResult S_ALREADYACKED = new OpcResult("S_ALREADYACKED", OpcNamespace.OPC_ALARM_AND_EVENTS, 0x00040200);
            /// <summary>The buffer time parameter was invalid.</summary>
            public static readonly OpcResult S_INVALIDBUFFERTIME = new OpcResult("S_INVALIDBUFFERTIME", OpcNamespace.OPC_ALARM_AND_EVENTS, 0x00040201);
            /// <summary>The max size parameter was invalid.</summary>
            public static readonly OpcResult S_INVALIDMAXSIZE = new OpcResult("S_INVALIDMAXSIZE", OpcNamespace.OPC_ALARM_AND_EVENTS, 0x00040202);
            /// <summary>The KeepAliveTime parameter was invalid.</summary>
            public static readonly OpcResult S_INVALIDKEEPALIVETIME = new OpcResult("S_INVALIDKEEPALIVETIME", OpcNamespace.OPC_ALARM_AND_EVENTS, 0x00040203);
            /// <summary>The string was not recognized as an area name.</summary>
            public static readonly OpcResult E_INVALIDBRANCHNAME = new OpcResult("E_INVALIDBRANCHNAME", OpcNamespace.OPC_ALARM_AND_EVENTS, 0xC0040203);
            /// <summary>The time does not match the latest active time.</summary>
            public static readonly OpcResult E_INVALIDTIME = new OpcResult("E_INVALIDTIME", OpcNamespace.OPC_ALARM_AND_EVENTS, 0xC0040204);
            /// <summary>A refresh is currently in progress.</summary>
            public static readonly OpcResult E_BUSY = new OpcResult("E_BUSY", OpcNamespace.OPC_ALARM_AND_EVENTS, 0xC0040205);
            /// <summary>Information is not available.</summary>
            public static readonly OpcResult E_NOINFO = new OpcResult("E_NOINFO", OpcNamespace.OPC_ALARM_AND_EVENTS, 0xC0040206);
        }

    }
}
