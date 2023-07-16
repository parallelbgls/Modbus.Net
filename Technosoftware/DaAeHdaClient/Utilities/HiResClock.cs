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
#endregion

namespace Technosoftware.DaAeHdaClient.Utilities
{
    /// <summary>
    /// Produces high resolution timestamps.
    /// </summary>
    internal class HiResClock
    {
        /// <summary>
        /// Returns the current UTC time (bugs in HALs on some computers can result in time jumping backwards).
        /// </summary>
        public static DateTime UtcNow
        {
            get
            {
                if (s_Default.m_disabled)
                {
                    return DateTime.UtcNow;
                }

                long counter;
                if (NativeMethods.QueryPerformanceCounter(out counter) == 0)
                {
                    return DateTime.UtcNow;
                }

                var ticks = (counter - s_Default.m_baseline) * s_Default.m_ratio;

                return new DateTime((long)ticks + s_Default.m_offset);
            }
        }

        /// <summary>
        /// Disables the hi-res clock (may be necessary on some machines with bugs in the HAL).
        /// </summary>
        public static bool Disabled
        {
            get => s_Default.m_disabled;

            set => s_Default.m_disabled = value;
        }

        /// <summary>
        /// Constructs a class.
        /// </summary>
        private HiResClock()
        {
            if (NativeMethods.QueryPerformanceFrequency(out m_frequency) == 0)
            {
                m_frequency = TimeSpan.TicksPerSecond;
            }

            m_offset = DateTime.UtcNow.Ticks;

            if (NativeMethods.QueryPerformanceCounter(out m_baseline) == 0)
            {
                m_baseline = m_offset;
            }

            m_ratio = ((decimal)TimeSpan.TicksPerSecond) / m_frequency;
        }

        /// <summary>
        /// Defines a global instance.
        /// </summary>
        private static readonly HiResClock s_Default = new HiResClock();

        /// <summary>
        /// Defines the native methods used by the class.
        /// </summary>
        private static class NativeMethods
        {
            [DllImport("Kernel32.dll")]
            public static extern int QueryPerformanceFrequency(out long lpFrequency);

            [DllImport("Kernel32.dll")]
            public static extern int QueryPerformanceCounter(out long lpFrequency);
        }

        private long m_frequency;
        private long m_baseline;
        private long m_offset;
        private decimal m_ratio;
        private bool m_disabled;
    }

}
