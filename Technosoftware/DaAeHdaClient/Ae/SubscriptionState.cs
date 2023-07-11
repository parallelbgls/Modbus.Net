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
	/// Describes the state of a subscription.
	/// </summary>
	[Serializable]
	public class TsCAeSubscriptionState : ICloneable
	{
		#region Fields
        private bool active_ = true;
        #endregion

		#region Constructors, Destructor, Initialization
        #endregion

		#region Properties
        /// <summary>
		/// A descriptive name for the subscription.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// A unique identifier for the subscription assigned by the client.
		/// </summary>
		public object ClientHandle { get; set; }

		/// <summary>
		/// Whether the subscription is monitoring for events to send to the client.
		/// </summary>
		public bool Active
		{
			get => active_;
            set => active_ = value;
        }

		/// <summary>
		/// The maximum rate at which the server send event notifications.
		/// </summary>
		public int BufferTime { get; set; }

		/// <summary>
		/// The requested maximum number of events that will be sent in a single callback.
		/// </summary>
		public int MaxSize { get; set; }

		/// <summary>
		/// The maximum period between updates sent to the client.
		/// </summary>
		public int KeepAlive { get; set; }
        #endregion

		#region ICloneable Members
        /// <summary>
		/// Creates a shallow copy of the object.
		/// </summary>
		public virtual object Clone()
		{
			return MemberwiseClone();
		}
        #endregion
	}
}
