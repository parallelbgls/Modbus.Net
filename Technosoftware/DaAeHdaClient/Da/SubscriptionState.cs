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
	/// Describes the state of a subscription.
	/// </summary>
	[Serializable]
	public class TsCDaSubscriptionState : ICloneable
	{
		#region Fields
        private bool active_ = true;
		private int updateRate_ = 500;
		private float deadband_;
        #endregion

		#region Properties
        /// <summary>
		/// A unique name for the subscription controlled by the client.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// A unique identifier for the subscription assigned by the client.
		/// </summary>
		public object ClientHandle { get; set; }

		/// <summary>
		/// A unique subscription identifier assigned by the server.
		/// </summary>
		public object ServerHandle { get; set; }

		/// <summary>
		/// The locale used for any error messages or results returned to the client.
		/// </summary>
		public string Locale { get; set; }

		/// <summary>
		/// Whether the subscription is scanning for updates to send to the client.
		/// </summary>
		public bool Active
		{
			get => active_;
            set => active_ = value;
        }

        /// <summary>
        /// The rate in milliseconds at which the server checks of updates to send to the
        /// client.
        /// </summary>
        /// <remarks>
        ///     Client Specifies the fastest rate at which data changes may be sent to the
        ///     <see cref="TsCDaDataChangedEventHandler">DataChangedHandler</see>
        ///     for items in this subscription. This also indicates the desired accuracy of Cached
        ///     Data. This is intended only to control the behavior of the interface. How the
        ///     server deals with the update rate and how often it actually polls the hardware
        ///     internally is an implementation detail. Passing 0 indicates the server should use
        ///     the fastest practical rate.
        /// </remarks>
        public int UpdateRate
		{
			get => updateRate_;
            set => updateRate_ = value;
        }

        /// <summary><para>The maximum period in milliseconds between updates sent to the client.</para></summary>
        /// <remarks>
        /// 	<para>Clients can set the keep-alive time for a subscription to cause the server to
        ///     provide client callbacks on the subscription when there are no new events to
        ///     report. Clients can then be assured of the health of the server and subscription
        ///     without resorting to pinging the server with calls to GetStatus().</para>
        /// 	<para>Using this facility, a client can expect a callback (data or keep-alive)
        ///     within the specified keep-alive time.</para>
        /// 	<para>Servers shall reset their keep-alive timers when real data is sent (i.e. it
        ///     is not acceptable to constantly send the keep-alive callback at a fixed period
        ///     equal to the keep-alive time irrespective of data callbacks).</para>
        /// 	<para>
        ///         The keep-alive callback consists of a call to the
        ///         <see cref="TsCDaDataChangedEventHandler">DataChangedEventHandler</see>
        ///         with an empty value list.
        ///     </para>
        /// 	<para>
        ///         Keep-alive callbacks will not occur when the subscription is inactive.
        ///         Keep-alive callbacks do not affect the value of the
        ///         <see cref="OpcServerStatus.LastUpdateTime">LastUpdateTime</see> returned by
        ///         <see cref="TsCDaServer.GetServerStatus">GetServerStatus()</see> .
        ///     </para>
        /// 	<para><strong>Available only for OPC Data Access 3.0 and OPC XML-DA
        ///     servers.</strong></para>
        /// </remarks>
        public int KeepAlive { get; set; }

		/// <summary>
		/// The minimum percentage change from 0.0 to 100.0 required to trigger a data update
		/// for an item.
		/// </summary>
		/// <remarks>
		/// 	<para>The range of the Deadband is from 0.0 to 100.0 Percent. Deadband will only
		///     apply to items in the subscription that have a dwEUType of Analog available. If the
		///     EU Type is Analog, then the EU Low and EU High values for the item can be used to
		///     calculate the range for the item. This range will be multiplied with the Deadband
		///     to generate an exception limit. An exception is determined as follows:</para>
		/// 	<blockquote dir="ltr" style="MARGIN-RIGHT: 0px">
		/// 		<para>Exception if (absolute value of (last cached value - current value) &gt;
		///         (pPercentDeadband/100.0) * (EU High - EU Low) )</para>
		/// 	</blockquote>
		/// 	<para>The PercentDeadband can be set when CreateSubscription is called, allowing
		///     the same PercentDeadband to be used for all items within that particular
		///     subscription. However, with OPC DA 3.0, it is allowable to set the PercentDeadband
		///     on a per item basis. This means that each item can potentially override the
		///     PercentDeadband set for the subscription it resides within.</para>
		/// 	<para>If the exception limit is exceeded, then the last cached value is updated
		///     with the new value and a notification will be sent to the client’s callback (if
		///     any). The PercentDeadband is an optional behavior for the server. If the client
		///     does not specify this value on a server that does support the behavior, the default
		///     value of 0 (zero) will be assumed, and all value changes will update the CACHE.
		///     Note that the timestamp will be updated regardless of whether the cached value is
		///     updated. A server which does not support deadband should return an error
		///     (OPC_E_DEADBANDNOTSUPPORTED) if the client requests a deadband other than
		///     0.0.</para>
		/// 	<para>The UpdateRate for a subscription or the sampling rate of the item, if set,
		///     determines time between when a value is checked to see if the exception limit has
		///     been exceeded. The PercentDeadband is used to keep noisy signals from updating the
		///     client unnecessarily.</para>
		/// </remarks>
		public float Deadband
		{
			get => deadband_;
            set => deadband_ = value;
        }

		/// <summary>
		/// TimeZone Bias of Group (in minutes).
		/// </summary>
		public int TimeBias { get; set; }
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
