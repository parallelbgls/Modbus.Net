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
using Technosoftware.OpcRcw.Comn;
#endregion

namespace Technosoftware.DaAeHdaClient.Com
{
    /// <summary>
    /// Adds and removes a connection point to a server.
    /// </summary>
    internal class ConnectionPoint : IDisposable
    {
        /// <summary>
        /// The COM server that supports connection points.
        /// </summary>
		private IConnectionPoint server_;

        /// <summary>
        /// The id assigned to the connection by the COM server.
        /// </summary>
		private int cookie_;
        
        /// <summary>
        /// The number of times Advise() has been called without a matching Unadvise(). 
        /// </summary>
		private int refs_;
        
        /// <summary>
        /// Initializes the object by finding the specified connection point.
        /// </summary>
        public ConnectionPoint(object server, Guid iid)
        {
            ((IConnectionPointContainer)server).FindConnectionPoint(ref iid, out server_);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (server_ != null)
            {
                while (Unadvise() > 0)
                {
                }
                try
                {
                    Utilities.Interop.ReleaseServer(server_);
                }
                catch
                {
                    // Ignore. COM Server probably no longer connected
                }

                server_ = null;
            }
        }

        /// <summary> 
        /// The cookie returned in the advise call. 
        /// </summary> 
        public int Cookie => cookie_;

        //=====================================================================
        // IConnectionPoint

        /// <summary>
        /// Establishes a connection, if necessary and increments the reference count.
        /// </summary>
        public int Advise(object callback)
        {
            if (refs_++ == 0) server_.Advise(callback, out cookie_);
            return refs_;
        }

        /// <summary>
        /// Decrements the reference count and closes the connection if no more references.
        /// </summary>
        public int Unadvise()
        {
            if (--refs_ == 0) server_.Unadvise(cookie_);
            return refs_;
        }
    }
}
