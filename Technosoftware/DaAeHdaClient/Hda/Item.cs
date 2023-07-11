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
    /// Describes an item used in a request for processed or raw data.
    /// </summary>
    [Serializable]
    public class TsCHdaItem : OpcItem
    {
        #region Fields
        private int aggregate_ = TsCHdaAggregateID.NoAggregate;
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes object with the default values.
        /// </summary>
        public TsCHdaItem() { }

        /// <summary>
        /// Initializes object with the specified ItemIdentifier object.
        /// </summary>
        public TsCHdaItem(OpcItem item) : base(item) { }

        /// <summary>
        /// Initializes object with the specified Item object.
        /// </summary>
        public TsCHdaItem(TsCHdaItem item)
            : base(item)
        {
            if (item != null)
            {
                Aggregate = item.Aggregate;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// The aggregate to use to process the data.
        /// </summary>
        public int Aggregate
        {
            get => aggregate_;
            set => aggregate_ = value;
        }
        #endregion
    }
}
