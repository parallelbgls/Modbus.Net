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
using System.Runtime.Serialization;
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>Used to raise an exception associated with a specified result code.</summary>
    /// <remarks>
    /// The OpcResultException includes the OPC result code within the Result
    /// property.
    /// </remarks>
    /// <seealso cref="OpcResult">OpcResult Structure</seealso>
    [Serializable]
    public class OpcResultException : ApplicationException
    {
        /// <remarks/>
        public OpcResult Result => result_;

        /// <remarks/>
		public OpcResultException(OpcResult result) : base(result.Description()) { result_ = result; }
        /// <remarks/>
        public OpcResultException(OpcResult result, string message) : base(message + ": " + result.ToString() + Environment.NewLine) { result_ = result; }
        /// <remarks/>
        public OpcResultException(OpcResult result, string message, Exception e) : base(message + ": " + result.ToString() + Environment.NewLine, e) { result_ = result; }
        /// <remarks/>
        protected OpcResultException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <remarks/>
        private OpcResult result_ = OpcResult.E_FAIL;
    }
}
