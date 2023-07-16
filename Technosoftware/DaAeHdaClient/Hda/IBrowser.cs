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
#endregion

namespace Technosoftware.DaAeHdaClient.Hda
{
    /// <summary>
    /// Defines functionality that is common to all OPC Data Access servers.
    /// </summary>
    public interface ITsCHdaBrowser : IDisposable
    {
        /// <summary>
        /// Returns the set of attribute filters used by the browser. 
        /// </summary>
        TsCHdaBrowseFilterCollection Filters { get; }

        /// <summary>
        /// Browses the server's address space at the specified branch.
        /// </summary>
        /// <param name="itemId">The item id of the branch to search.</param>
        /// <returns>The set of elements that meet the filter criteria.</returns>
        TsCHdaBrowseElement[] Browse(OpcItem itemId);

        /// <summary>
        /// Begins a browsing the server's address space at the specified branch.
        /// </summary>
        /// <param name="itemId">The item id of the branch to search.</param>
        /// <param name="maxElements">The maximum number of elements to return.</param>
        /// <param name="position">The position object used to continue a browse operation.</param>
        /// <returns>The set of elements that meet the filter criteria.</returns>
        TsCHdaBrowseElement[] Browse(OpcItem itemId, int maxElements, out IOpcBrowsePosition position);

        /// <summary>
        /// Continues browsing the server's address space at the specified position.
        /// </summary>
        /// <param name="maxElements">The maximum number of elements to return.</param>
        /// <param name="position">The position object used to continue a browse operation.</param>
        /// <returns>The set of elements that meet the filter criteria.</returns>
        TsCHdaBrowseElement[] BrowseNext(int maxElements, ref IOpcBrowsePosition position);
    }
}
