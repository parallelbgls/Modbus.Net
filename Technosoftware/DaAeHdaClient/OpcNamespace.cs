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

#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// Declares constants for common XML Schema and OPC namespaces.
    /// </summary>
    public class OpcNamespace
    {
        /// <summary>XML Schema</summary>
        public const string XML_SCHEMA                  =   "http://www.w3.org/2001/XMLSchema";
        /// <summary>XML Schema Instance</summary>
        public const string XML_SCHEMA_INSTANCE         =   "http://www.w3.org/2001/XMLSchema-instance";
        /// <summary>OPC Alarmes &amp; Events</summary>
        public const string OPC_ALARM_AND_EVENTS        =   "http://opcfoundation.org/AlarmAndEvents/";
        /// <summary>OPC Complex Data</summary>
        public const string OPC_COMPLEX_DATA            =   "http://opcfoundation.org/ComplexData/";
        /// <summary>OPC Data Exchange</summary>
        public const string OPC_DATA_EXCHANGE           =   "http://opcfoundation.org/DataExchange/";
        /// <summary>OPC Data Access</summary>
        public const string OPC_DATA_ACCESS             =   "http://opcfoundation.org/DataAccess/";
        /// <summary>OPC Historical Data Access</summary>
        public const string OPC_HISTORICAL_DATA_ACCESS  =   "http://opcfoundation.org/HistoricalDataAccess/";
        /// <summary>OPC Binary 1.0</summary>
        public const string OPC_BINARY                  =	"http://opcfoundation.org/OPCBinary/1.0/";
        /// <summary>OPC XML-DA 1.0</summary>
        public const string OPC_DATA_ACCESS_XML10       =   "http://opcfoundation.org/webservices/XMLDA/1.0/";
        /// <summary>OPC UA 1.0</summary>
        public const string OPC_UA10                    =   "http://opcfoundation.org/webservices/UA/1.0/";
    }
}
