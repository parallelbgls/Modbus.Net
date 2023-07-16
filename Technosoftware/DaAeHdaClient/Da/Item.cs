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

namespace Technosoftware.DaAeHdaClient.Da
{
    /// <summary>
    /// Describes how an item in the server address space should be accessed. 
    /// </summary>
    [Serializable]
    public class TsCDaItem : OpcItem
    {
        #region Fields
        private bool active_ = true;
        private float deadband_;
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes the object with default values.
        /// </summary>
        public TsCDaItem() { }

        /// <summary>
        /// Initializes object with the specified ItemIdentifier object.
        /// </summary>
        public TsCDaItem(OpcItem item)
        {
            if (item == null)
            {
                return;
            }
            ItemName = item.ItemName;
            ItemPath = item.ItemPath;
            ClientHandle = item.ClientHandle;
            ServerHandle = item.ServerHandle;
        }

        /// <summary>
        /// Initializes object with the specified Item object.
        /// </summary>
        public TsCDaItem(TsCDaItem item)
            : base(item)
        {
            if (item == null)
            {
                return;
            }
            ReqType = item.ReqType;
            MaxAge = item.MaxAge;
            MaxAgeSpecified = item.MaxAgeSpecified;
            Active = item.Active;
            ActiveSpecified = item.ActiveSpecified;
            Deadband = item.Deadband;
            DeadbandSpecified = item.DeadbandSpecified;
            SamplingRate = item.SamplingRate;
            SamplingRateSpecified = item.SamplingRateSpecified;
            EnableBuffering = item.EnableBuffering;
            EnableBufferingSpecified = item.EnableBufferingSpecified;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The data type to use when returning the item value.
        /// </summary>
        public Type ReqType { get; set; }

        /// <summary>
        /// The oldest (in milliseconds) acceptable cached value when reading an item.
        /// </summary>
		public int MaxAge { get; set; }

        /// <summary>
        /// Whether the Max Age is specified.
        /// </summary>
		public bool MaxAgeSpecified { get; set; }

        /// <summary>
        /// Whether the server should send data change updates. 
        /// </summary>
        public bool Active
        {
            get => active_;
            set => active_ = value;
        }

        /// <summary>
        /// Whether the Active state is specified.
        /// </summary>
		public bool ActiveSpecified { get; set; }

        /// <summary>
        /// The minimum percentage change required to trigger a data update for an item. 
        /// </summary>
        public float Deadband
        {
            get => deadband_;
            set => deadband_ = value;
        }

        /// <summary>
        /// Whether the Deadband is specified.
        /// </summary>
		public bool DeadbandSpecified { get; set; }

        /// <summary>
        /// How frequently the server should sample the item value.
        /// </summary>
		public int SamplingRate { get; set; }

        /// <summary>
        /// Whether the Sampling Rate is specified.
        /// </summary>
		public bool SamplingRateSpecified { get; set; }

        /// <summary>
        /// Whether the server should buffer multiple data changes between data updates.
        /// </summary>
		public bool EnableBuffering { get; set; }

        /// <summary>
        /// Whether the Enable Buffering is specified.
        /// </summary>
		public bool EnableBufferingSpecified { get; set; }
        #endregion
    }
}
