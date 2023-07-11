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
using System.Runtime.InteropServices;
using Technosoftware.OpcRcw.Comn;
#endregion

namespace Technosoftware.DaAeHdaClient.Com
{
    /// <summary>
    /// A wrapper for the COM IEnumString interface.
    /// </summary>
    internal class EnumString : IDisposable
    {   
        /// <summary>
        /// A reference to the remote COM object.
        /// </summary>
        private IEnumString m_enumerator = null;

        /// <summary>
        /// Initializes the object with an enumerator.
        /// </summary>
        public EnumString(object enumerator)
        {
            m_enumerator = (IEnumString)enumerator;
        }
        
        /// <summary>
        /// Releases the remote COM object.
        /// </summary>
        public void Dispose()
        {
            Utilities.Interop.ReleaseServer(m_enumerator);
            m_enumerator = null;
        }

        //=====================================================================
        // IEnumString

        /// <summary>
        /// Fetches the next subscription of strings. 
        /// </summary>
        public string[] Next(int count)
        {
            try
            {
                // create buffer.
                var buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(IntPtr))*count);

                try
                {
                    // fetch next subscription of strings.
                    var fetched = 0;

                    m_enumerator.RemoteNext(
                        count,
                        buffer,
                        out fetched);

                    // return empty array if at end of list.
                    if (fetched == 0)
                    {
                        return new string[0];
                    }

                    return Interop.GetUnicodeStrings(ref buffer, fetched, true);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(buffer);
                }
            }

            // return null on any error.
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Skips a number of strings.
        /// </summary>
        public void Skip(int count)
        {
            m_enumerator.Skip(count);
        }

        /// <summary>
        /// Sets pointer to the start of the list.
        /// </summary>
        public void Reset()
        {
            m_enumerator.Reset();
        }

        /// <summary>
        /// Clones the enumerator.
        /// </summary>
        public EnumString Clone()
        {
            IEnumString enumerator;
            m_enumerator.Clone(out enumerator);
            return new EnumString(enumerator);
        }
    }
}
