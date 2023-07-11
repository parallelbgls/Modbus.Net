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
	/// The description of an item sub-condition supported by the server.
	/// </summary>
	[Serializable]
	public class TsCAeSubCondition : ICloneable
	{
		#region Fields
        private int severity_ = 1;
        #endregion

		#region Properties
        /// <summary>
		/// The name assigned to the sub-condition.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// A server-specific expression which defines the sub-state represented by the sub-condition.
		/// </summary>
		public string Definition { get; set; }

		/// <summary>
		/// The severity of any event notifications generated on behalf of this sub-condition.
		/// </summary>
		public int Severity
		{
			get => severity_;
            set => severity_ = value;
        }

		/// <summary>
		/// The text string to be included in any event notification generated on behalf of this sub-condition.
		/// </summary>
		public string Description { get; set; }
        #endregion

		#region Helper Methods
        /// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Name;
		}
        #endregion

		#region ICloneable Members
        /// <summary>
		/// Creates a shallow copy of the object.
		/// </summary>
		public virtual object Clone() { return MemberwiseClone(); }
        #endregion
	}
}
