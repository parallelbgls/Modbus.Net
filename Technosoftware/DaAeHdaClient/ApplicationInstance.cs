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
using Technosoftware.DaAeHdaClient.Utilities;
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// Manages the license to enable the different product versions.
    /// </summary>
    public partial class ApplicationInstance
    {
        #region Properties
        /// <summary>
        /// This flag suppresses the conversion to local time done during marshalling.
        /// </summary>
        public static bool TimeAsUtc { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the log file directory and ensures it is writable.
        /// </summary>
        public static string GetLogFileDirectory()
        {
            return ConfigUtils.GetLogFileDirectory();
        }

        /// <summary>
        /// Enable the trace.
        /// </summary>
        /// <param name="path">The path to use.</param>
        /// <param name="filename">The filename.</param>
        public static void EnableTrace(string path, string filename)
        {
            ConfigUtils.EnableTrace(path, filename);
        }
        #endregion

        #region Internal Fields
        internal static bool InitializeSecurityCalled = false;
        #endregion
    }
}
