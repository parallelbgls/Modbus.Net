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
#endregion

namespace Technosoftware.DaAeHdaClient.Da
{
	/// <summary>
	///     <para>Defines the possible quality status bits.</para>
	///     <para>These flags represent the quality state for an item's data value. This is
	///     intended to be similar to but slightly simpler than the Field-bus Data Quality
	///     Specification (section 4.4.1 in the H1 Final Specifications). This design makes it
	///     fairly easy for both servers and client applications to determine how much
	///     functionality they want to implement.</para>
	/// </summary>
	public enum TsDaQualityBits
    {
		/// <summary>The Quality of the value is Good.</summary>
		Good = 0x000000C0,
		/// <summary>The value has been Overridden. Typically this means the input has been disconnected and a manually entered value has been 'forced'.</summary>
		GoodLocalOverride = 0x000000D8,
		/// <summary>The value is bad but no specific reason is known.</summary>
		Bad = 0x00000000,
		/// <summary>
		/// There is some server specific problem with the configuration. For example the
		/// item in question has been deleted from the configuration.
		/// </summary>
		BadConfigurationError = 0x00000004,
		/// <summary>
		/// The input is required to be logically connected to something but is not. This
		/// quality may reflect that no value is available at this time, for reasons like the value
		/// may have not been provided by the data source.
		/// </summary>
		BadNotConnected = 0x00000008,
		/// <summary>A device failure has been detected.</summary>
		BadDeviceFailure = 0x0000000c,
		/// <summary>
        /// A sensor failure had been detected (the ’Limits’ field can provide additional
		/// diagnostic information in some situations).
		/// </summary>
		BadSensorFailure = 0x00000010,
		/// <summary>
		/// Communications have failed. However, the last known value is available. Note that
        /// the ‘age’ of the value may be determined from the time stamp in the item state.
		/// </summary>
		BadLastKnownValue = 0x00000014,
		/// <summary>Communications have failed. There is no last known value is available.</summary>
		BadCommFailure = 0x00000018,
		/// <summary>
		/// The block is off scan or otherwise locked. This quality is also used when the
		/// active state of the item or the subscription containing the item is InActive.
		/// </summary>
		BadOutOfService = 0x0000001C,
		/// <summary>
		/// After Items are added to a subscription, it may take some time for the server to
		/// actually obtain values for these items. In such cases the client might perform a read
		/// (from cache), or establish a ConnectionPoint based subscription and/or execute a
		/// Refresh on such a subscription before the values are available. This sub-status is only
		/// available from OPC DA 3.0 or newer servers.
		/// </summary>
		BadWaitingForInitialData = 0x00000020,
		/// <summary>There is no specific reason why the value is uncertain.</summary>
		Uncertain = 0x00000040,
		/// <summary>
		/// Whatever was writing this value has stopped doing so. The returned value should
        /// be regarded as ‘stale’. Note that this differs from a BAD value with sub-status
		/// badLastKnownValue (Last Known Value). That status is associated specifically with a
        /// detectable communications error on a ‘fetched’ value. This error is associated with the
        /// failure of some external source to ‘put’ something into the value within an acceptable
        /// period of time. Note that the ‘age’ of the value can be determined from the time stamp
		/// in the item state.
		/// </summary>
		UncertainLastUsableValue = 0x00000044,
		/// <summary>
        /// Either the value has ‘pegged’ at one of the sensor limits (in which case the
		/// limit field should be set to low or high) or the sensor is otherwise known to be out of
		/// calibration via some form of internal diagnostics (in which case the limit field should
		/// be none).
		/// </summary>
		UncertainSensorNotAccurate = 0x00000050,
		/// <summary>
		/// The returned value is outside the limits defined for this parameter. Note that in
        /// this case (per the Field-bus Specification) the ‘Limits’ field indicates which limit has
		/// been exceeded but does NOT necessarily imply that the value cannot move farther out of
		/// range.
		/// </summary>
		UncertainEUExceeded = 0x00000054,
		/// <summary>
		/// The value is derived from multiple sources and has less than the required number
		/// of Good sources.
		/// </summary>
		UncertainSubNormal = 0x00000058
	}
}
