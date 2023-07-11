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

namespace Technosoftware.DaAeHdaClient.Hda
{
    /// <summary>
    /// Defines constants for well-known item attributes.
    /// </summary>
    public class TsCHdaAttributeID
    {
        /// <remarks/>
        public const int DATA_TYPE = 0x01;
        /// <remarks/>
        public const int DESCRIPTION = 0x02;
        /// <remarks/>
        public const int ENG_UNITS = 0x03;
        /// <remarks/>
        public const int STEPPED = 0x04;
        /// <remarks/>
        public const int ARCHIVING = 0x05;
        /// <remarks/>
        public const int DERIVE_EQUATION = 0x06;
        /// <remarks/>
        public const int NODE_NAME = 0x07;
        /// <remarks/>
        public const int PROCESS_NAME = 0x08;
        /// <remarks/>
        public const int SOURCE_NAME = 0x09;
        /// <remarks/>
        public const int SOURCE_TYPE = 0x0a;
        /// <remarks/>
        public const int NORMAL_MAXIMUM = 0x0b;
        /// <remarks/>
        public const int NORMAL_MINIMUM = 0x0c;
        /// <remarks/>
        public const int ITEMID = 0x0d;
        /// <remarks/>
        public const int MAX_TIME_INT = 0x0e;
        /// <remarks/>
        public const int MIN_TIME_INT = 0x0f;
        /// <remarks/>
        public const int EXCEPTION_DEV = 0x10;
        /// <remarks/>
        public const int EXCEPTION_DEV_TYPE = 0x11;
        /// <remarks/>
        public const int HIGH_ENTRY_LIMIT = 0x12;
        /// <remarks/>
        public const int LOW_ENTRY_LIMIT = 0x13;
    }
}
