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
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// A result code associated with a unique item identifier.
    /// </summary>
    [Serializable]
    public class OpcItemResult : OpcItem, IOpcResult
    {
        #region Fields
        private OpcResult result_ = OpcResult.S_OK;
        private string diagnosticInfo_;
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initialize object with default values.
        /// </summary>
        public OpcItemResult() { }

        /// <summary>
        /// Initialize object with the specified OpcItem object.
        /// </summary>
        public OpcItemResult(OpcItem item)
            : base(item)
        {
        }

        /// <summary>
        /// Initialize object with the specified IdentifiedResult object.
        /// </summary>
        public OpcItemResult(OpcItemResult item)
            : base(item)
        {
            if (item != null)
            {
                Result = item.Result;
                DiagnosticInfo = item.DiagnosticInfo;
            }
        }

        /// <summary>
        /// Initializes the object with the specified item name and result code.
        /// </summary>
        public OpcItemResult(string itemName, OpcResult resultId)
            : base(itemName)
        {
            Result = resultId;
        }

        /// <summary>
        /// Initialize object with the specified item name, result code and diagnostic info.
        /// </summary>
        public OpcItemResult(string itemName, OpcResult resultId, string diagnosticInfo)
            : base(itemName)
        {
            Result = resultId;
            DiagnosticInfo = diagnosticInfo;
        }

        /// <summary>
        /// Initialize object with the specified OpcItem and result code.
        /// </summary>
        public OpcItemResult(OpcItem item, OpcResult resultId)
            : base(item)
        {
            Result = resultId;
        }

        /// <summary>
        /// Initialize object with the specified OpcItem, result code and diagnostic info.
        /// </summary>
        public OpcItemResult(OpcItem item, OpcResult resultId, string diagnosticInfo)
            : base(item)
        {
            Result = resultId;
            DiagnosticInfo = diagnosticInfo;
        }
        #endregion

        #region IOpcResult Members
        /// <summary>
        /// The error id for the result of an operation on an item.
        /// </summary>
        public OpcResult Result
        {
            get => result_;
            set => result_ = value;
        }

        /// <summary>
        /// Vendor specific diagnostic information (not the localized error text).
        /// </summary>
        public string DiagnosticInfo
        {
            get => diagnosticInfo_;
            set => diagnosticInfo_ = value;
        }
        #endregion
    }
}
