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

namespace Technosoftware.DaAeHdaClient.Ae
{
    /// <summary>
    /// Contains a writable collection attribute ids.
    /// </summary>
    [Serializable]
    public class TsCAeAttributeCollection : OpcWriteableCollection
    {
        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Creates an empty collection.
        /// </summary>
        internal TsCAeAttributeCollection() : base(null, typeof(int)) { }

        /// <summary>
        /// Creates a collection from an array.
        /// </summary>
        internal TsCAeAttributeCollection(int[] attributeIDs) : base(attributeIDs, typeof(int)) { }
        #endregion

        #region Public Methods
        /// <summary>
        /// An indexer for the collection.
        /// </summary>
        public new int this[int index] => (int)Array[index];

        /// <summary>
		/// Returns a copy of the collection as an array.
		/// </summary>
		public new int[] ToArray()
        {
            return (int[])Array.ToArray(typeof(int));
        }
        #endregion
    }
}
