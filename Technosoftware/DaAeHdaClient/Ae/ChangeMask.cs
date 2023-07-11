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
	/// The bits indicating what changes generated an event notification.
	/// </summary>
	[Flags]
	public enum TsCAeChangeMask
	{
		/// <summary>
		/// The condition’s active state has changed.
		/// </summary>
		ActiveState = 0x0001,

		/// <summary>
		/// The condition’s acknowledgment state has changed.
		/// </summary>
		AcknowledgeState = 0x0002,

		/// <summary>
		/// The condition’s enabled state has changed.
		/// </summary>
		EnableState = 0x0004,

		/// <summary>
		/// The condition quality has changed.
		/// </summary>
		Quality = 0x0008,

		/// <summary>
		/// The severity level has changed.
		/// </summary>
		Severity = 0x0010,

		/// <summary>
		/// The condition has transitioned into a new sub-condition.
		/// </summary>
		SubCondition = 0x0020,

		/// <summary>
		/// The event message has changed.
		/// </summary>
		Message = 0x0040,

		/// <summary>
		/// One or more event attributes have changed.
		/// </summary>
		Attribute = 0x0080
	}
}
