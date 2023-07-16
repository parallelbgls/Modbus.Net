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
using Technosoftware.DaAeHdaClient.Da;
#endregion

namespace Technosoftware.DaAeHdaClient.Ae
{
    /// <summary>
    /// The description of an item condition state supported by the server.
    /// </summary>
    [Serializable]
    public class TsCAeCondition : ICloneable
    {
        #region Fields
        private TsCAeSubCondition _activeSubcondition = new TsCAeSubCondition();
        private TsCDaQuality _quality = TsCDaQuality.Bad;
        private DateTime _lastAckTime = DateTime.MinValue;
        private DateTime _subCondLastActive = DateTime.MinValue;
        private DateTime _condLastActive = DateTime.MinValue;
        private DateTime _condLastInactive = DateTime.MinValue;
        private SubConditionCollection _subconditions = new SubConditionCollection();
        private AttributeValueCollection _attributes = new AttributeValueCollection();
        #endregion

        #region AttributeCollection Class
        /// <summary>
        /// Contains a read-only collection of AttributeValues.
        /// </summary>
        public class AttributeValueCollection : OpcWriteableCollection
        {
            /// <summary>
            /// An indexer for the collection.
            /// </summary>
            public new TsCAeAttributeValue this[int index] => (TsCAeAttributeValue)Array[index];

            /// <summary>
            /// Returns a copy of the collection as an array.
            /// </summary>
            public new TsCAeAttributeValue[] ToArray()
            {
                return (TsCAeAttributeValue[])Array.ToArray();
            }

            /// <summary>
            /// Creates an empty collection.
            /// </summary>
            internal AttributeValueCollection() : base(null, typeof(TsCAeAttributeValue)) { }
        }
        #endregion

        #region SubConditionCollection Class
        /// <summary>
        /// Contains a read-only collection of SubConditions.
        /// </summary>
        public class SubConditionCollection : OpcWriteableCollection
        {
            /// <summary>
            /// An indexer for the collection.
            /// </summary>
            public new TsCAeSubCondition this[int index] => (TsCAeSubCondition)Array[index];

            /// <summary>
            /// Returns a copy of the collection as an array.
            /// </summary>
            public new TsCAeSubCondition[] ToArray()
            {
                return (TsCAeSubCondition[])Array.ToArray();
            }

            /// <summary>
            /// Creates an empty collection.
            /// </summary>
            internal SubConditionCollection() : base(null, typeof(TsCAeSubCondition)) { }
        }
        #endregion

        #region Properties
        /// <summary>
        /// A bit mask indicating the current state of the condition
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// The currently active sub-condition, for multi-state conditions which are active. 
        /// For a single-state condition, this contains the information about the condition itself.
        /// For inactive conditions, this value is null.
        /// </summary>
        public TsCAeSubCondition ActiveSubCondition
        {
            get => _activeSubcondition;
            set => _activeSubcondition = value;
        }

        /// <summary>
		/// The quality associated with the condition state.
		/// </summary>
		public TsCDaQuality Quality
        {
            get => _quality;
            set => _quality = value;
        }

        /// <summary>
        /// The time of the most recent acknowledgment of this condition (of any sub-condition).
        /// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
        /// the time format (UTC or local   time).
        /// </summary>
        public DateTime LastAckTime
        {
            get => _lastAckTime;
            set => _lastAckTime = value;
        }

        /// <summary>
        /// Time of the most recent transition into active sub-condition. 
        /// This is the time value which must be specified when acknowledging the condition. 
        /// If the condition has never been active, this value is DateTime.MinValue.
        /// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
        /// the time format (UTC or local   time).
        /// </summary>
        public DateTime SubCondLastActive
        {
            get => _subCondLastActive;
            set => _subCondLastActive = value;
        }

        /// <summary>
        /// Time of the most recent transition into the condition. 
        /// There may be transitions among the sub-conditions which are more recent. 
        /// If the condition has never been active, this value is DateTime.MinValue.
        /// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
        /// the time format (UTC or local   time).
        /// </summary>
        public DateTime CondLastActive
        {
            get => _condLastActive;
            set => _condLastActive = value;
        }

        /// <summary>
        /// Time of the most recent transition out of this condition. 
        /// This value is DateTime.MinValue if the condition has never been active, 
        /// or if it is currently active for the first time and has never been exited.
        /// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
        /// the time format (UTC or local   time).
        /// </summary>
        public DateTime CondLastInactive
        {
            get => _condLastInactive;
            set => _condLastInactive = value;
        }

        /// <summary>
        /// This is the ID of the client who last acknowledged this condition. 
        /// This value is null if the condition has never been acknowledged.
        /// </summary>
        public string AcknowledgerID { get; set; }

        /// <summary>
        /// The comment string passed in by the client who last acknowledged this condition.
        /// This value is null if the condition has never been acknowledged.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The sub-conditions defined for this condition. 
        /// For single-state conditions, the collection will contain one element, the value of which describes the condition.
        /// </summary>
        public SubConditionCollection SubConditions => _subconditions;

        /// <summary>
        /// The values of the attributes requested for this condition. 
        /// </summary>
        public AttributeValueCollection Attributes => _attributes;

        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public virtual object Clone()
        {
            var clone = (TsCAeCondition)MemberwiseClone();

            clone._activeSubcondition = (TsCAeSubCondition)_activeSubcondition.Clone();
            clone._subconditions = (SubConditionCollection)_subconditions.Clone();
            clone._attributes = (AttributeValueCollection)_attributes.Clone();

            return clone;
        }
        #endregion

    }
}
