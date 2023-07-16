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

namespace Technosoftware.DaAeHdaClient.Cpx
{
    /// <summary>
    /// Stores the current serialization context.
    /// </summary>
    internal struct TsCCpxContext
    {
        ///////////////////////////////////////////////////////////////////////
        #region Constructors, Destructor, Initialization

        public TsCCpxContext(byte[] buffer)
        {
            Buffer = buffer;
            Index = 0;
            Dictionary = null;
            Type = null;
            BigEndian = false;
            CharWidth = 2;
            StringEncoding = STRING_ENCODING_UCS2;
            FloatFormat = FLOAT_FORMAT_IEEE754;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Public Fields

        public byte[] Buffer;
        public int Index;
        public TypeDictionary Dictionary;
        public TypeDescription Type;
        public bool BigEndian;
        public int CharWidth;
        public string StringEncoding;
        public string FloatFormat;


        public const string STRING_ENCODING_ACSII = "ASCII";
        public const string STRING_ENCODING_UCS2 = "UCS-2";
        public const string FLOAT_FORMAT_IEEE754 = "IEEE-754";

        #endregion

    }
}
