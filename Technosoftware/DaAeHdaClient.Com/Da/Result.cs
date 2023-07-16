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
    namespace Da
    {
        /// <summary>
        /// Defines all well known COM DA HRESULT codes.
        /// </summary>
        internal struct Result
        {
            /// <remarks/>
            public const int S_OK = +0x00000000; // 0x00000000
            /// <remarks/>
            public const int S_FALSE = +0x00000001; // 0x00000001
            /// <remarks/>
            public const int E_NOTIMPL = -0x7FFFBFFF; // 0x80004001
            /// <remarks/>
            public const int E_OUTOFMEMORY = -0x7FF8FFF2; // 0x8007000E
            /// <remarks/>
            public const int E_INVALIDARG = -0x7FF8FFA9; // 0x80070057
            /// <remarks/>
            public const int E_NOINTERFACE = -0x7FFFBFFE; // 0x80004002
            /// <remarks/>
            public const int E_POINTER = -0x7FFFBFFD; // 0x80004003
            /// <remarks/>
            public const int E_FAIL = -0x7FFFBFFB; // 0x80004005
            /// <remarks/>
            public const int CONNECT_E_NOCONNECTION = -0x7FFBFE00; // 0x80040200
            /// <remarks/>
            public const int CONNECT_E_ADVISELIMIT = -0x7FFBFDFF; // 0x80040201
            /// <remarks/>
            public const int DISP_E_TYPEMISMATCH = -0x7FFDFFFB; // 0x80020005
            /// <remarks/>
            public const int DISP_E_OVERFLOW = -0x7FFDFFF6; // 0x8002000A
            /// <remarks/>
            public const int E_INVALIDHANDLE = -0x3FFBFFFF; // 0xC0040001
            /// <remarks/>
            public const int E_BADTYPE = -0x3FFBFFFC; // 0xC0040004
            /// <remarks/>
            public const int E_PUBLIC = -0x3FFBFFFB; // 0xC0040005
            /// <remarks/>
            public const int E_BADRIGHTS = -0x3FFBFFFA; // 0xC0040006
            /// <remarks/>
            public const int E_UNKNOWNITEMID = -0x3FFBFFF9; // 0xC0040007
            /// <remarks/>
            public const int E_INVALIDITEMID = -0x3FFBFFF8; // 0xC0040008
            /// <remarks/>
            public const int E_INVALIDFILTER = -0x3FFBFFF7; // 0xC0040009
            /// <remarks/>
            public const int E_UNKNOWNPATH = -0x3FFBFFF6; // 0xC004000A
            /// <remarks/>
            public const int E_RANGE = -0x3FFBFFF5; // 0xC004000B
            /// <remarks/>
            public const int E_DUPLICATENAME = -0x3FFBFFF4; // 0xC004000C
            /// <remarks/>
            public const int S_UNSUPPORTEDRATE = +0x0004000D; // 0x0004000D
            /// <remarks/>
            public const int S_CLAMP = +0x0004000E; // 0x0004000E
            /// <remarks/>
            public const int S_INUSE = +0x0004000F; // 0x0004000F
            /// <remarks/>
            public const int E_INVALIDCONFIGFILE = -0x3FFBFFF0; // 0xC0040010
            /// <remarks/>
            public const int E_NOTFOUND = -0x3FFBFFEF; // 0xC0040011
            /// <remarks/>
            public const int E_INVALID_PID = -0x3FFBFDFD; // 0xC0040203
            /// <remarks/>
            public const int E_DEADBANDNOTSET = -0x3FFBFC00; // 0xC0040400
            /// <remarks/>
            public const int E_DEADBANDNOTSUPPORTED = -0x3FFBFBFF; // 0xC0040401
            /// <remarks/>
            public const int E_NOBUFFERING = -0x3FFBFBFE; // 0xC0040402
            /// <remarks/>
            public const int E_INVALIDCONTINUATIONPOINT = -0x3FFBFBFD; // 0xC0040403
            /// <remarks/>
            public const int S_DATAQUEUEOVERFLOW = +0x00040404; // 0x00040404 
            /// <remarks/>
            public const int E_RATENOTSET = -0x3FFBFBFB; // 0xC0040405
            /// <remarks/>
            public const int E_NOTSUPPORTED = -0x3FFBFBFA; // 0xC0040406
        }
    }

    namespace Cpx
    {
        /// <summary>
        /// Defines all well known Complex Data HRESULT codes.
        /// </summary>
        internal struct Result
        {
            /// <remarks/>
            public const int E_TYPE_CHANGED = -0x3FFBFBF9; // 0xC0040407
            /// <remarks/>
            public const int E_FILTER_DUPLICATE = -0x3FFBFBF8; // 0xC0040408
            /// <remarks/>
            public const int E_FILTER_INVALID = -0x3FFBFBF7; // 0xC0040409
            /// <remarks/>
            public const int E_FILTER_ERROR = -0x3FFBFBF6; // 0xC004040A
            /// <remarks/>
            public const int S_FILTER_NO_DATA = +0x0004040B; // 0xC004040B
        }
    }
}
