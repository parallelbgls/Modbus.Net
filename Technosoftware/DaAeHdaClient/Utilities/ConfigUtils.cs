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
using System.IO;
// ReSharper disable UnusedMember.Global

#endregion

namespace Technosoftware.DaAeHdaClient.Utilities
{
    /// <summary>
    /// Utility functions used by COM applications.
    /// </summary>
    internal static class ConfigUtils
    {
        /// <summary>
        /// Gets the log file directory and ensures it is writable.
        /// </summary>
        public static string GetLogFileDirectory()
        {
            // try the program data directory.
            var logFileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            logFileDirectory += "\\Technosoftware\\Logs";

            try
            {
                // create the directory.
                if (!Directory.Exists(logFileDirectory))
                {
                    Directory.CreateDirectory(logFileDirectory);
                }
            }
            catch (Exception)
            {
                // try the MyDocuments directory instead.
                logFileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                logFileDirectory += "Technosoftware\\Logs";

                if (!Directory.Exists(logFileDirectory))
                {
                    Directory.CreateDirectory(logFileDirectory);
                }
            }

            return logFileDirectory;
        }

        /// <summary>
        /// Enable the trace.
        /// </summary>
        /// <param name="path">The path to use.</param>
        /// <param name="filename">The filename.</param>
        public static void EnableTrace(string path, string filename)
        {
            Utils.SetTraceOutput(Utils.TraceOutput.FileOnly);
            Utils.SetTraceMask(int.MaxValue);

            var logFilePath = path + "\\" + filename;
            Utils.SetTraceLog(logFilePath, false);
            Utils.Trace("Log File Set to: {0}", logFilePath);
        }
    }
}
