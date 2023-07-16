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
    /// The item id and network location of a DA item associated with an event source.
    /// </summary>
    [Serializable]
    public class TsCAeItemUrl : OpcItem
    {
        #region Fields
        private OpcUrl url_ = new OpcUrl();
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes the object with default values.
        /// </summary>
        public TsCAeItemUrl() { }

        /// <summary>
        /// Initializes the object with an ItemIdentifier object.
        /// </summary>
        public TsCAeItemUrl(OpcItem item) : base(item) { }

        /// <summary>
        /// Initializes the object with an ItemIdentifier object and url.
        /// </summary>
        public TsCAeItemUrl(OpcItem item, OpcUrl url)
            : base(item)
        {
            Url = url;
        }

        /// <summary>
        /// Initializes object with the specified ItemResult object.
        /// </summary>
        public TsCAeItemUrl(TsCAeItemUrl item) : base(item)
        {
            if (item != null)
            {
                Url = item.Url;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// The url of the server that contains the item.
        /// </summary>
        public OpcUrl Url
        {
            get => url_;
            set => url_ = value;
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public override object Clone()
        {
            return new TsCAeItemUrl(this);
        }
        #endregion

    }
}
