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
    /// Contains a description of a single item property.
    /// </summary>
    [Serializable]
    public class TsCDaItemProperty : ICloneable, IOpcResult
    {
        #region Fields
        private OpcResult result_ = OpcResult.S_OK;
        #endregion

        #region Properties

        /// <summary>
        /// The property identifier.
        /// </summary>
        public TsDaPropertyID ID { get; set; }

        /// <summary>
        /// A short description of the property.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The data type of the property.
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// The value of the property.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// The primary identifier for the property if it is directly accessible as an item.
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// The secondary identifier for the property if it is directly accessible as an item.
        /// </summary>
        public string ItemPath { get; set; }
        #endregion

        #region IOpcResult Members
        /// <summary>
        /// The <see cref="OpcResult" /> object with the result of an operation on an property.
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

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public virtual object Clone()
        {
            var clone = (TsCDaItemProperty)MemberwiseClone();
            clone.Value = OpcConvert.Clone(Value);
            return clone;
        }
        #endregion
    }
}
