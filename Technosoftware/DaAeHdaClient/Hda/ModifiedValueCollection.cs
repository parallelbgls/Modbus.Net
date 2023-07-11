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
    /// A collection of modified item values with a result code indicating the results of a read operation.
    /// </summary>
    [Serializable]
    public class TsCHdaModifiedValueCollection : TsCHdaItemValueCollection
    {
        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initialize object with default values.
        /// </summary>
        public TsCHdaModifiedValueCollection() { }

        /// <summary>
        /// Initialize object with the specified ItemIdentifier object.
        /// </summary>
        public TsCHdaModifiedValueCollection(OpcItem item) : base(item) { }

        /// <summary>
        /// Initializes object with the specified Item object.
        /// </summary>
        public TsCHdaModifiedValueCollection(TsCHdaItem item) : base(item) { }

        /// <summary>
        /// Initializes object with the specified ItemValueCollection object.
        /// </summary>
        public TsCHdaModifiedValueCollection(TsCHdaItemValueCollection item) : base(item) { }
        #endregion

        #region Properties
        /// <summary>
        /// Accessor for elements in the collection.
        /// </summary>
        public new TsCHdaModifiedValue this[int index]
        {
            get => (TsCHdaModifiedValue)this[index];
            set => this[index] = value;
        }
        #endregion
    }
}
