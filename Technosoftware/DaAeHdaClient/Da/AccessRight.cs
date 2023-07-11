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
	///     <para>Defines possible item access rights.</para>
	///     <para align="left">Indicates if this item is read only, write only or read/write.
	///     This is NOT related to security but rather to the nature of the underlying
	///     hardware.</para>
	/// </summary>
	public enum TsDaAccessRights
    {
		/// <summary>The access rights for this item are server.</summary>
		Unknown = 0x00,
		/// <summary>The client can read the data item's value.</summary>
		Readable = 0x01,
		/// <summary>The client can change the data item's value.</summary>
		Writable = 0x02,
		/// <summary>The client can read and change the data item's value.</summary>
		ReadWritable = 0x03
	}
}
