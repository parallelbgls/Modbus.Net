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

namespace Technosoftware.DaAeHdaClient.Ae
{
	/// <summary>
	/// Specifies the information required to acknowledge an event.
	/// </summary>
	[Serializable]
	public class TsCAeEventAcknowledgement : ICloneable
	{
		#region Fields
        private DateTime activeTime_ = DateTime.MinValue;
        #endregion

		#region Properties
        /// <summary>
		/// The name of the source that generated the event.
		/// </summary>
		public string SourceName { get; set; }

		/// <summary>
		/// The name of the condition that is being acknowledged.
		/// </summary>
		public string ConditionName { get; set; }

		/// <summary>
		/// The time that the condition or sub-condition became active.
		/// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
		/// the time format (UTC or local   time).
		/// </summary>
		public DateTime ActiveTime
		{
			get => activeTime_;
            set => activeTime_ = value;
        }

		/// <summary>
		/// The cookie for the condition passed to client during the event notification.
		/// </summary>
		public int Cookie { get; set; }

		/// <summary>
		/// Constructs an acknowledgment with its default values.
		/// </summary>
		public TsCAeEventAcknowledgement() { }

		/// <summary>
		/// Constructs an acknowledgment from an event notification.
		/// </summary>
		public TsCAeEventAcknowledgement(TsCAeEventNotification notification)
		{
			SourceName = notification.SourceID;
			ConditionName = notification.ConditionName;
			activeTime_ = notification.ActiveTime;
			Cookie = notification.Cookie;
		}
        #endregion

		#region ICloneable Members
        /// <summary>
		/// Creates a deep copy of the object.
		/// </summary>
		public virtual object Clone()
		{
			return MemberwiseClone();
		}
        #endregion
	}
}
