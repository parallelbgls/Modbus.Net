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

namespace Technosoftware.DaAeHdaClient.Ae
{
	/// <summary>
	/// The current state of a process area or an event source.
	/// </summary>
	public class TsCAeEnabledStateResult : IOpcResult
	{
		#region Fields
        private string qualifiedName_;
        #endregion

		#region Constructors, Destructor, Initialization
        /// <summary>
		/// Initializes the object with default values.
		/// </summary>
		public TsCAeEnabledStateResult() { }

		/// <summary>
		/// Initializes the object with an qualified name.
		/// </summary>
		public TsCAeEnabledStateResult(string qualifiedName)
		{
			qualifiedName_ = qualifiedName;
		}

		/// <summary>
		/// Initializes the object with an qualified name and Result.
		/// </summary>
		public TsCAeEnabledStateResult(string qualifiedName, OpcResult result)
		{
			qualifiedName_ = qualifiedName;
			Result = result;
		}
        #endregion

		#region Properties
        /// <summary>
		/// Whether if the area or source is enabled.
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// Whether the area or source is enabled and all areas within the hierarchy of its containing areas are enabled. 
		/// </summary>
		public bool EffectivelyEnabled { get; set; }
        #endregion

		#region IOpcResult Members
        /// <summary>
		/// The error id for the result of an operation on an item.
		/// </summary>
		public OpcResult Result { get; set; } = OpcResult.S_OK;

        /// <summary>
		/// Vendor specific diagnostic information (not the localized error text).
		/// </summary>
		public string DiagnosticInfo { get; set; }
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
