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
using System.Collections.Generic;
using System.Runtime.Serialization;
#endregion

namespace Technosoftware.DaAeHdaClient
{

    /// <summary>
    /// The default class used to instantiate server objects.
    /// </summary>
    [Serializable]
    public class OpcFactory : IOpcFactory, ISerializable, ICloneable
    {
        #region Class Names
        /// <summary>
        /// A set of names for fields used in serialization.
        /// </summary>
        private class Names
        {
            internal const string SystemType = "SystemType";
        }
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes the object with the type of the servers it can instantiate.
        /// </summary>
        /// <param name="systemType">The System.Type of the server object that the factory can create.</param>
        public OpcFactory(Type systemType)
        {
            SystemType = systemType;
        }

        /// <summary>
        /// Maybe overridden to release unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            // do nothing.
        }

        /// <summary>
        /// Construct a server by de-serializing its OpcUrl from the stream.
        /// </summary>
        protected OpcFactory(SerializationInfo info, StreamingContext context)
        {
            SystemType = (Type)info.GetValue(Names.SystemType, typeof(Type));
        }
        #endregion

        #region Properties
        /// <summary>
        /// The system type used to instantiate the remote server object.
        /// </summary>
        protected Type SystemType { get; set; }

        /// <summary>
        /// Can be used to force OPC DA 2.0 even if OPC DA 3.0 server features are available
        /// </summary>
        public bool ForceDa20Usage { get; set; }

        /// <summary>
        /// List of supported OPC specifications
        /// </summary>
        public IList<OpcSpecification> SupportedSpecifications { get; set; }
        #endregion

        #region ISerializable
        /// <summary>
        /// Serializes a server into a stream.
        /// </summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(Names.SystemType, SystemType);
        }
        #endregion

        #region ICloneable
        /// <summary>
        /// Returns a clone of the factory.
        /// </summary>
        public virtual object Clone()
        {
            return MemberwiseClone();
        }
        #endregion

        #region IOpcFactory
        /// <summary>
        /// Creates a new instance of the server.
        /// </summary>
        public virtual IOpcServer CreateInstance(OpcUrl url, OpcConnectData connectData)
        {
            var server = (IOpcServer)Activator.CreateInstance(SystemType, new object[] { url, connectData });

            return server;
        }
        #endregion
    }
}
