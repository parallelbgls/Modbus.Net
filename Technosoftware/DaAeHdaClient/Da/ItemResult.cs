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

namespace Technosoftware.DaAeHdaClient.Da
{
    /// <summary>
    /// The results of an operation on a uniquely identifiable item.
    /// </summary>
    [Serializable]
    public class TsCDaItemResult : TsCDaItem, IOpcResult
    {
        #region Fields
        private OpcResult result_ = OpcResult.S_OK;
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes the object with default values.
        /// </summary>
        public TsCDaItemResult() { }

        /// <summary>
        /// Initializes the object with an ItemIdentifier object.
        /// </summary>
        public TsCDaItemResult(OpcItem item) : base(item) { }

        /// <summary>
        /// Initializes the object with an ItemIdentifier object and Result.
        /// </summary>
        public TsCDaItemResult(OpcItem item, OpcResult resultId)
            : base(item)
        {
            Result = resultId;
        }

        /// <summary>
        /// Initializes the object with an Item object.
        /// </summary>
        public TsCDaItemResult(TsCDaItem item) : base(item) { }

        /// <summary>
        /// Initializes the object with an Item object and Result.
        /// </summary>
        public TsCDaItemResult(TsCDaItem item, OpcResult resultId)
            : base(item)
        {
            Result = resultId;
        }

        /// <summary>
        /// Initializes object with the specified ItemResult object.
        /// </summary>
        public TsCDaItemResult(TsCDaItemResult item)
            : base(item)
        {
            if (item != null)
            {
                Result = item.Result;
                DiagnosticInfo = item.DiagnosticInfo;
            }
        }
        #endregion

        #region IOpcResult Members

        /// <summary>
        /// The error id for the result of an operation on an property.
        /// </summary>
        public OpcResult Result
        {
            get => result_;
            set => result_ = value;
        }

        /// <summary>
        /// Vendor specific diagnostic information (not the localized error text).
        /// </summary>
        public string DiagnosticInfo { get; set; }
        #endregion
    }
}
