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

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// Contains properties that describe the current status of an OPC server.
    /// </summary>
    [Serializable]
    public class OpcServerStatus : ICloneable
    {
        #region Fields
        private OpcServerState serverState_ = OpcServerState.Unknown;
        private DateTime startTime_ = DateTime.MinValue;
        private DateTime currentTime_ = DateTime.MinValue;
        private DateTime lastUpdateTime_ = DateTime.MinValue;
        private int bandWidth_ = -1;
        private short majorVersion_;
        private short minorVersion_;
        private short buildNumber_;
        #endregion

        #region Properties
        /// <summary>
        /// The vendor name and product name for the server.
        /// </summary>
        public string VendorInfo { get; set; }

        /// <summary>
        /// A string that contains the server software version number.
        /// </summary>
        public string ProductVersion { get; set; }

        /// <summary>
        /// The server for which the status is being reported.
        /// The ServerType enumeration is used to identify 
        /// the server. If the enumeration indicates multiple 
        /// server types, then this is the status of the entire 
        /// server. For example, if the server wraps an 
        /// OPC DA and OPC AE server, then if this ServerType 
        /// indicates both, the status is for the entire server, and 
        /// not for an individual wrapped server.
        /// </summary>
        public uint ServerType { get; set; }

        /// <summary>
        /// The current state of the server.
        /// </summary>
        public OpcServerState ServerState
        {
            get => serverState_;
            set => serverState_ = value;
        }

        /// <summary>
        /// A string that describes the current server state.
        /// </summary>
        public string StatusInfo { get; set; }

        /// <summary>
        /// The time when the server started.
        /// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
        /// the time format (UTC or local   time).
        /// </summary>
        public DateTime StartTime
        {
            get => startTime_;
            set => startTime_ = value;
        }

        /// <summary>
        /// Th current time at the server.
        /// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
        /// the time format (UTC or local   time).
        /// </summary>
        public DateTime CurrentTime
        {
            get => currentTime_;
            set => currentTime_ = value;
        }

        /// <summary>
        /// The maximum number of values that can be returned by the server on a per item basis. 
        /// </summary>
        public int MaxReturnValues { get; set; }

        /// <summary>
        /// The last time the server sent an data update to the client.
        /// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
        /// the time format (UTC or local   time).
        /// </summary>
        public DateTime LastUpdateTime
        {
            get => lastUpdateTime_;
            set => lastUpdateTime_ = value;
        }

        /// <summary>
        /// Total   number of groups being managed by the server.
        /// </summary>
        public int GroupCount { get; set; }

        /// <summary>
        /// The behavior of of this value   is server specific.
        /// </summary>
        public int BandWidth
        {
            get => bandWidth_;
            set => bandWidth_ = value;
        }

        /// <summary>
        /// The major   version of the used server issue.
        /// </summary>
        public short MajorVersion
        {
            get => majorVersion_;
            set => majorVersion_ = value;
        }

        /// <summary>
        /// The minor   version of the used server issue.
        /// </summary>
        public short MinorVersion
        {
            get => minorVersion_;
            set => minorVersion_ = value;
        }

        /// <summary>
        /// The build   number of the used server issue.
        /// </summary>
        public short BuildNumber
        {
            get => buildNumber_;
            set => buildNumber_ = value;
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep-copy of the object.
        /// </summary>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
}
