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
    /// Contains the value for a single item.
    /// </summary>
    [Serializable]
    public class TsCDaItemValue : OpcItem
    {
        #region Fields
        private TsCDaQuality daQuality_ = TsCDaQuality.Bad;
        private DateTime timestamp_ = DateTime.MinValue;
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes the object with default values.
        /// </summary>
        public TsCDaItemValue() { }

        /// <summary>
        /// Initializes the object with and ItemIdentifier object.
        /// </summary>
        public TsCDaItemValue(OpcItem item)
        {
            if (item == null)
            {
                return;
            }
            ItemName = item.ItemName;
            ItemPath = item.ItemPath;
            ClientHandle = item.ClientHandle;
            ServerHandle = item.ServerHandle;
        }

        /// <summary>
        /// Initializes the object with the specified item name.
        /// </summary>
        public TsCDaItemValue(string itemName)
            : base(itemName)
        {
        }

        /// <summary>
        /// Initializes object with the specified ItemValue object.
        /// </summary>
        public TsCDaItemValue(TsCDaItemValue item)
            : base(item)
        {
            if (item == null)
            {
                return;
            }
            Value = OpcConvert.Clone(item.Value);
            Quality = item.Quality;
            QualitySpecified = item.QualitySpecified;
            Timestamp = item.Timestamp;
            TimestampSpecified = item.TimestampSpecified;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The item value.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// The quality of the item value.
        /// </summary>
        public TsCDaQuality Quality
        {
            get => daQuality_;
            set => daQuality_ = value;
        }

        /// <summary>
        /// Whether the quality is specified.
        /// </summary>
        public bool QualitySpecified { get; set; }

        /// <summary>
        /// The timestamp for the item value.
        /// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
        /// the time format (UTC or local time).
        /// </summary>
        public DateTime Timestamp
        {
            get => timestamp_;
            set => timestamp_ = value;
        }

        /// <summary>
        /// Whether the timestamp is specified.
        /// </summary>
        public bool TimestampSpecified { get; set; }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public override object Clone()
        {
            var clone = (TsCDaItemValue)MemberwiseClone();
            clone.Value = OpcConvert.Clone(Value);
            return clone;
        }
        #endregion
    }
}
