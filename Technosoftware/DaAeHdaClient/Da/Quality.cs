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
    /// Contains the quality field for an item value.
    /// </summary>
    [Serializable]
    public struct TsCDaQuality
    {
        #region Fields
        private TsDaQualityBits qualityBits_;
        private TsDaLimitBits limitBits_;
        private byte vendorBits_;
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes the object with the specified quality.
        /// </summary>
        public TsCDaQuality(TsDaQualityBits quality)
        {
            qualityBits_ = quality;
            limitBits_ = TsDaLimitBits.None;
            vendorBits_ = 0;
        }

        /// <summary>
        /// Initializes the object from the contents of a 16 bit integer.
        /// </summary>
        public TsCDaQuality(short code)
        {
            qualityBits_ = (TsDaQualityBits)(code & (short)TsDaQualityMasks.QualityMask);
            limitBits_ = (TsDaLimitBits)(code & (short)TsDaQualityMasks.LimitMask);
            vendorBits_ = (byte)((code & (short)TsDaQualityMasks.VendorMask) >> 8);
        }
        #endregion

        #region Properties
        /// <summary>
        /// The value in the quality bits field.
        /// </summary>
        public TsDaQualityBits QualityBits
        {
            get => qualityBits_;
            set => qualityBits_ = value;
        }

        /// <summary>
        /// The value in the limit bits field.
        /// </summary>
        public TsDaLimitBits LimitBits
        {
            get => limitBits_;
            set => limitBits_ = value;
        }

        /// <summary>
        /// The value in the quality bits field.
        /// </summary>
        public byte VendorBits
        {
            get => vendorBits_;
            set => vendorBits_ = value;
        }

        /// <summary>
        /// A 'good' quality value.
        /// </summary>
        public static readonly TsCDaQuality Good = new TsCDaQuality(TsDaQualityBits.Good);

        /// <summary>
        /// An 'bad' quality value.
        /// </summary>
        public static readonly TsCDaQuality Bad = new TsCDaQuality(TsDaQualityBits.Bad);
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the quality as a 16 bit integer.
        /// </summary>
        public short GetCode()
        {
            ushort code = 0;

            code |= (ushort)QualityBits;
            code |= (ushort)LimitBits;
            code |= (ushort)(VendorBits << 8);

            return (code <= short.MaxValue) ? (short)code : (short)-((ushort.MaxValue + 1 - code));
        }

        /// <summary>
        /// Initializes the quality from a 16 bit integer.
        /// </summary>
        public void SetCode(short code)
        {
            qualityBits_ = (TsDaQualityBits)(code & (short)TsDaQualityMasks.QualityMask);
            limitBits_ = (TsDaLimitBits)(code & (short)TsDaQualityMasks.LimitMask);
            vendorBits_ = (byte)((code & (short)TsDaQualityMasks.VendorMask) >> 8);
        }

        /// <summary>
        /// Returns true if the objects are equal.
        /// </summary>
        public static bool operator ==(TsCDaQuality a, TsCDaQuality b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Returns true if the objects are not equal.
        /// </summary>
        public static bool operator !=(TsCDaQuality a, TsCDaQuality b)
        {
            return !a.Equals(b);
        }
        #endregion

        #region Object Member Overrides
        /// <summary>
        /// Converts a quality to a string with the format: 'quality[limit]:vendor'.
        /// </summary>
        public override string ToString()
        {
            string text = null;

            switch (QualityBits)
            {
                case TsDaQualityBits.Good:
                    text += "(Good";
                    break;
                case TsDaQualityBits.GoodLocalOverride:
                    text += "(Good:Local Override";
                    break;
                case TsDaQualityBits.Bad:
                    text += "(Bad";
                    break;
                case TsDaQualityBits.BadConfigurationError:
                    text += "(Bad:Configuration Error";
                    break;
                case TsDaQualityBits.BadNotConnected:
                    text += "(Bad:Not Connected";
                    break;
                case TsDaQualityBits.BadDeviceFailure:
                    text += "(Bad:Device Failure";
                    break;
                case TsDaQualityBits.BadSensorFailure:
                    text += "(Bad:Sensor Failure";
                    break;
                case TsDaQualityBits.BadLastKnownValue:
                    text += "(Bad:Last Known Value";
                    break;
                case TsDaQualityBits.BadCommFailure:
                    text += "(Bad:Communication Failure";
                    break;
                case TsDaQualityBits.BadOutOfService:
                    text += "(Bad:Out of Service";
                    break;
                case TsDaQualityBits.BadWaitingForInitialData:
                    text += "(Bad:Waiting for Initial Data";
                    break;
                case TsDaQualityBits.Uncertain:
                    text += "(Uncertain";
                    break;
                case TsDaQualityBits.UncertainLastUsableValue:
                    text += "(Uncertain:Last Usable Value";
                    break;
                case TsDaQualityBits.UncertainSensorNotAccurate:
                    text += "(Uncertain:Sensor not Accurate";
                    break;
                case TsDaQualityBits.UncertainEUExceeded:
                    text += "(Uncertain:Engineering Unit exceeded";
                    break;
                case TsDaQualityBits.UncertainSubNormal:
                    text += "(Uncertain:Sub Normal";
                    break;
            }

            if (LimitBits != TsDaLimitBits.None)
            {
                text += $":[{LimitBits.ToString()}]";
            }
            else
            {
                text += ":Not Limited";
            }

            if (VendorBits != 0)
            {
                text += $":{VendorBits,0:X})";
            }
            else
            {
                text += ")";
            }

            return text;
        }

        /// <summary>
        /// Determines whether the specified Object is equal to the current Quality
        /// </summary>
        public override bool Equals(object target)
        {
            if (target == null || target.GetType() != typeof(TsCDaQuality)) return false;

            var quality = (TsCDaQuality)target;

            if (QualityBits != quality.QualityBits) return false;
            if (LimitBits != quality.LimitBits) return false;
            if (VendorBits != quality.VendorBits) return false;

            return true;
        }

        /// <summary>
        /// Returns hash code for the current Quality.
        /// </summary>
        public override int GetHashCode()
        {
            return GetCode();
        }
        #endregion
    }
}
