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
    /// The types of modifications that can be applied to an item.
    /// </summary>
    public enum TsCHdaEditType
    {
        /// <summary>
        /// The item was inserted.
        /// </summary>
        Insert = 1,

        /// <summary>
        /// The item was replaced.
        /// </summary>
        Replace = 2,

        /// <summary>
        /// The item was inserted or replaced during an insert/replace operation.
        /// </summary>
        InsertReplace = 3,

        /// <summary>
        /// The item was deleted.
        /// </summary>
        Delete = 4
    }
}
