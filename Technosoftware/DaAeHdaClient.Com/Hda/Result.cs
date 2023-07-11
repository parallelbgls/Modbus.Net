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

namespace Technosoftware.DaAeHdaClient.Com
{
    namespace Hda
    {
        /// <summary>
        /// Defines all well known COM HDA HRESULT codes.
        /// </summary>
        internal struct Result
        {   
            /// <remarks/>
            public const int E_MAXEXCEEDED      = -0X3FFBEFFF; // 0xC0041001
            /// <remarks/>
            public const int S_NODATA           = +0x40041002; // 0x40041002
            /// <remarks/>
            public const int S_MOREDATA         = +0x40041003; // 0x40041003
            /// <remarks/>
            public const int E_INVALIDAGGREGATE = -0X3FFBEFFC; // 0xC0041004
            /// <remarks/>
            public const int S_CURRENTVALUE     = +0x40041005; // 0x40041005
            /// <remarks/>
            public const int S_EXTRADATA        = +0x40041006; // 0x40041006
            /// <remarks/>
            public const int W_NOFILTER         = -0x7FFBEFF9; // 0x80041007
            /// <remarks/>
            public const int E_UNKNOWNATTRID    = -0x3FFBEFF8; // 0xC0041008
            /// <remarks/>
            public const int E_NOT_AVAIL        = -0x3FFBEFF7; // 0xC0041009
            /// <remarks/>
            public const int E_INVALIDDATATYPE  = -0x3FFBEFF6; // 0xC004100A
            /// <remarks/>
            public const int E_DATAEXISTS       = -0x3FFBEFF5; // 0xC004100B
            /// <remarks/>
            public const int E_INVALIDATTRID    = -0x3FFBEFF4; // 0xC004100C
            /// <remarks/>
            public const int E_NODATAEXISTS     = -0x3FFBEFF3; // 0xC004100D
            /// <remarks/>
            public const int S_INSERTED         = +0x4004100E; // 0x4004100E
            /// <remarks/>
            public const int S_REPLACED         = +0x4004100F; // 0x4004100F
        }
    }
}
