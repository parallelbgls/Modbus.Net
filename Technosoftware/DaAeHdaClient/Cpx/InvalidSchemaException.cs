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
using System.Runtime.Serialization;
#endregion

namespace Technosoftware.DaAeHdaClient.Cpx
{
	/// <summary>
	/// Raised if the schema contains errors or inconsistencies.
	/// </summary>
	[Serializable]
	public class TsCCpxInvalidSchemaException : ApplicationException
	{
		private const string Default = "The schema cannot be used because it contains errors or inconsitencies.";
		/// <remarks/>
		public TsCCpxInvalidSchemaException() : base(Default) { }
		/// <remarks/>
		public TsCCpxInvalidSchemaException(string message) : base(Default + Environment.NewLine + message) { }
		/// <remarks/>
		public TsCCpxInvalidSchemaException(Exception e) : base(Default, e) { }
		/// <remarks/>
		public TsCCpxInvalidSchemaException(string message, Exception innerException) : base(Default + Environment.NewLine + message, innerException) { }
		/// <remarks/>
		protected TsCCpxInvalidSchemaException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
