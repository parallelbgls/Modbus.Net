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
using System.Reflection;
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// Defines constants for standard data types.
    /// </summary>
    public class OpcType
    {
        /// <remarks/>
        public static Type SBYTE = typeof(sbyte);
        /// <remarks/>
        public static Type BYTE = typeof(byte);
        /// <remarks/>
        public static Type SHORT = typeof(short);
        /// <remarks/>
        public static Type USHORT = typeof(ushort);
        /// <remarks/>
        public static Type INT = typeof(int);
        /// <remarks/>
        public static Type UINT = typeof(uint);
        /// <remarks/>
        public static Type LONG = typeof(long);
        /// <remarks/>
        public static Type ULONG = typeof(ulong);
        /// <remarks/>
        public static Type FLOAT = typeof(float);
        /// <remarks/>
        public static Type DOUBLE = typeof(double);
        /// <remarks/>
        public static Type DECIMAL = typeof(decimal);
        /// <remarks/>
        public static Type BOOLEAN = typeof(bool);
        /// <remarks/>
        public static Type DATETIME = typeof(DateTime);
        /// <remarks/>
        public static Type DURATION = typeof(TimeSpan);
        /// <remarks/>
        public static Type STRING = typeof(string);
        /// <remarks/>
        public static Type ANY_TYPE = typeof(object);
        /// <remarks/>
        public static Type BINARY = typeof(byte[]);
        /// <remarks/>
        public static Type ARRAY_SHORT = typeof(short[]);
        /// <remarks/>
        public static Type ARRAY_USHORT = typeof(ushort[]);
        /// <remarks/>
        public static Type ARRAY_INT = typeof(int[]);
        /// <remarks/>
        public static Type ARRAY_UINT = typeof(uint[]);
        /// <remarks/>
        public static Type ARRAY_LONG = typeof(long[]);
        /// <remarks/>
        public static Type ARRAY_ULONG = typeof(ulong[]);
        /// <remarks/>
        public static Type ARRAY_FLOAT = typeof(float[]);
        /// <remarks/>
        public static Type ARRAY_DOUBLE = typeof(double[]);
        /// <remarks/>
        public static Type ARRAY_DECIMAL = typeof(decimal[]);
        /// <remarks/>
        public static Type ARRAY_BOOLEAN = typeof(bool[]);
        /// <remarks/>
        public static Type ARRAY_DATETIME = typeof(DateTime[]);
        /// <remarks/>
        public static Type ARRAY_STRING = typeof(string[]);
        /// <remarks/>
        public static Type ARRAY_ANY_TYPE = typeof(object[]);
        /// <remarks/>
        public static Type ILLEGAL_TYPE = typeof(OpcType);


        /// <summary>
        /// Returns an array of all well-known types.
        /// </summary>
        public static Type[] Enumerate()
        {
            var values = new ArrayList();

            var fields = typeof(OpcType).GetFields(BindingFlags.Static | BindingFlags.Public);

            Array.ForEach(fields, field => values.Add(field.GetValue(typeof(Type))));

            return (Type[])values.ToArray(typeof(Type));
        }
    }
}
