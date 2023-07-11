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
using System.ComponentModel;
using System.Runtime.InteropServices;
#pragma warning disable 618

#endregion

namespace Technosoftware.DaAeHdaClient.Hda
{
    /// <summary>
    /// A value of an item at in instant of time.
    /// </summary>
    [Serializable]
    public class TsCHdaItemValue : ICloneable
    {
        #region Fields
        private DateTime timestamp_ = DateTime.MinValue;
        private Da.TsCDaQuality daQuality_ = Da.TsCDaQuality.Bad;
        private TsCHdaQuality historianQuality_ = TsCHdaQuality.NoData;
        #endregion

        #region Properties
        /// <summary>
        /// The value of the item.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// The timestamp associated with the value.
        /// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
        /// the time format (UTC or local time).
        /// </summary>
        public DateTime Timestamp
        {
            get => timestamp_;
            set => timestamp_ = value;
        }

        /// <summary>
		/// The quality associated with the value when it was acquired from the data source.
		/// </summary>
		public Da.TsCDaQuality Quality
        {
            get => daQuality_;
            set => daQuality_ = value;
        }

        /// <summary>
        /// The quality associated with the value when it was retrieved from the hiatorian database.
        /// </summary>
        public TsCHdaQuality HistorianQuality
        {
            get => historianQuality_;
            set => historianQuality_ = value;
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public object Clone()
        {
            var value = (TsCHdaItemValue)MemberwiseClone();
            value.Value = OpcConvert.Clone(Value);
            return value;
        }
        #endregion
    }
}
