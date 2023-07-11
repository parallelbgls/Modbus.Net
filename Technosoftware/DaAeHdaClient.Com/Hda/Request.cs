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
using System.Collections;

using Technosoftware.DaAeHdaClient.Hda;

#endregion

namespace Technosoftware.DaAeHdaClient.Com.Hda
{
    /// <summary>
    /// An object that mainatains the state of asynchronous requests.
    /// </summary>
    internal class Request : IOpcRequest, ITsCHdaActualTime
    {
        /// <summary>
        /// The unique id assigned to the request when it was created.
        /// </summary>
        public int RequestID => m_requestID;

        /// <summary>
        /// The unqiue id assigned by the server when it was created.
        /// </summary>
        public int CancelID => m_cancelID;

        /// <summary>
        /// Fired when the server acknowledges that a request was cancelled.
        /// </summary>
        public event TsCHdaCancelCompleteEventHandler CancelCompleteEvent
        {
            add { lock (this) { m_cancelCompleteEvent += value; } }
            remove { lock (this) { m_cancelCompleteEvent -= value; } }
        }

        /// <summary>
        /// Initializes the object with all required information.
        /// </summary>
        public Request(object requestHandle, Delegate callback, int requestID)
        {           
            m_requestHandle = requestHandle; 
            m_callback      = callback; 
            m_requestID     = requestID; 
        }

        /// <summary>
        /// Updates the request with the initial results.  
        /// </summary>
        public bool Update(int cancelID, OpcItem[] results)
        {
            lock (this)
            {
                // save the server assigned id.
                m_cancelID = cancelID; 

                // create a table of items indexed by the handle returned by the server in a callback.
                m_items = new Hashtable();

                foreach (var result in results)
                {
                    if (!typeof(IOpcResult).IsInstanceOfType(result) || ((IOpcResult)result).Result.Succeeded())
                    {
                        m_items[result.ServerHandle] = new OpcItem(result);
                    }
                }

                // nothing more to do - no good items.
                if (m_items.Count == 0)
                {
                    return true;
                }

                // invoke callbacks for results that have already arrived.
                var complete = false;

                if (m_results != null)
                {
                    foreach (var result in m_results)
                    {
                        complete = InvokeCallback(result);
                    }
                }

                // all done.
                return complete;
            }
        }

        /// <summary>
        /// Invokes the callback for the request.
        /// </summary>
        public bool InvokeCallback(object results)
        {
            lock (this)
            {
                // save the results if the initial call to the server has not completed yet.
                if (m_items == null)
                {
                    // create cache for results.
                    if (m_results == null)
                    {
                        m_results = new ArrayList();
                    }

                    m_results.Add(results);

                    // request not initialized completely
                    return false;
                }

                // invoke on data update callback.
                if (typeof(TsCHdaDataUpdateEventHandler).IsInstanceOfType(m_callback))
                {
                    return InvokeCallback((TsCHdaDataUpdateEventHandler)m_callback, results);
                }

                // invoke read completed callback.
                if (typeof(TsCHdaReadValuesCompleteEventHandler).IsInstanceOfType(m_callback))
                {
                    return InvokeCallback((TsCHdaReadValuesCompleteEventHandler)m_callback, results);
                }
                
                // invoke read attributes completed callback.
                if (typeof(TsCHdaReadAttributesCompleteEventHandler).IsInstanceOfType(m_callback))
                {
                    return InvokeCallback((TsCHdaReadAttributesCompleteEventHandler)m_callback, results);
                }
                
                // invoke read annotations completed callback.
                if (typeof(TsCHdaReadAnnotationsCompleteEventHandler).IsInstanceOfType(m_callback))
                {
                    return InvokeCallback((TsCHdaReadAnnotationsCompleteEventHandler)m_callback, results);
                }
                
                // invoke update completed callback.
                if (typeof(TsCHdaUpdateCompleteEventHandler).IsInstanceOfType(m_callback))
                {
                    return InvokeCallback((TsCHdaUpdateCompleteEventHandler)m_callback, results);
                }
                
                // callback not supported.
                return true;
            }
        }

        /// <summary>
        /// Called when the server acknowledges that a request was cancelled. 
        /// </summary>
        public void OnCancelComplete()
        {
            lock (this)
            {
                if (m_cancelCompleteEvent != null)
                {
                    m_cancelCompleteEvent(this);
                }
            }
        }

        #region IOpcRequest Members
        /// <summary>
        /// An unique identifier, assigned by the client, for the request.
        /// </summary>
        public object Handle => m_requestHandle;

        #endregion

        #region IActualTime Members
        /// <summary>
        /// The actual start time used by a server while processing a request.
        /// </summary>
        public DateTime StartTime
        {
            get => m_startTime;
            set => m_startTime = value;
        }

        /// <summary>
        /// The actual end time used by a server while processing a request.
        /// </summary>
        public DateTime EndTime
        {
            get => m_endTime;
            set => m_endTime = value;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Invokes callback for a data change update.
        /// </summary>
        private bool InvokeCallback(TsCHdaDataUpdateEventHandler callback, object results)
        {
            // check for valid result type.
            if (!typeof(TsCHdaItemValueCollection[]).IsInstanceOfType(results))
            {
                return false;
            }

            var values = (TsCHdaItemValueCollection[])results;

            // update item handles and actual times.
            UpdateResults(values);

            try
            {
                callback(this, values);
            }
            catch 
            {
                // ignore exceptions in the callbacks.
            }
            
            // request never completes.
            return false;
        }

        /// <summary>
        /// Invokes callback for a read request.
        /// </summary>
        private bool InvokeCallback(TsCHdaReadValuesCompleteEventHandler callback, object results)
        {
            // check for valid result type.
            if (!typeof(TsCHdaItemValueCollection[]).IsInstanceOfType(results))
            {
                return false;
            }

            var values = (TsCHdaItemValueCollection[])results;

            // update item handles and actual times.
            UpdateResults(values);

            try
            {
                callback(this, values);
            }
            catch 
            {
                // ignore exceptions in the callbacks.
            }

            // check if all data has been sent.
            foreach (var value in values)
            {
                if (value.Result == OpcResult.Hda.S_MOREDATA)
                {
                    return false;
                }
            }

            // request is complete.
            return true;
        }   

        /// <summary>
        /// Invokes callback for a read attributes request.
        /// </summary>
        private bool InvokeCallback(TsCHdaReadAttributesCompleteEventHandler callback, object results)
        {
            // check for valid result type.
            if (!typeof(TsCHdaItemAttributeCollection).IsInstanceOfType(results))
            {
                return false;
            }

            var values = (TsCHdaItemAttributeCollection)results;

            // update item handles and actual times.
            UpdateResults(new TsCHdaItemAttributeCollection[] { values });

            try
            {
                callback(this, values);
            }
            catch 
            {
                // ignore exceptions in the callbacks.
            }

            // request always completes
            return true;
        }
        
        /// <summary>
        /// Invokes callback for a read annotations request.
        /// </summary>
        private bool InvokeCallback(TsCHdaReadAnnotationsCompleteEventHandler callback, object results)
        {
            // check for valid result type.
            if (!typeof(TsCHdaAnnotationValueCollection[]).IsInstanceOfType(results))
            {
                return false;
            }

            var values = (TsCHdaAnnotationValueCollection[])results;

            // update item handles and actual times.
            UpdateResults(values);

            try
            {
                callback(this, values);
            }
            catch 
            {
                // ignore exceptions in the callbacks.
            }

            // request always completes
            return true;
        }

        /// <summary>
        /// Invokes callback for a read annotations request.
        /// </summary>
        private bool InvokeCallback(TsCHdaUpdateCompleteEventHandler callback, object results)
        {
            // check for valid result type.
            if (!typeof(TsCHdaResultCollection[]).IsInstanceOfType(results))
            {
                return false;
            }

            var values = (TsCHdaResultCollection[])results;

            // update item handles and actual times.
            UpdateResults(values);

            try
            {
                callback(this, values);
            }
            catch 
            {
                // ignore exceptions in the callbacks.
            }

            // request always completes
            return true;
        }

        /// <summary>
        /// Updates the result objects with locally cached information.
        /// </summary>
        private void UpdateResults(OpcItem[] results)
        {
            foreach (var result in results)
            {
                // update actual times.
                if (typeof(ITsCHdaActualTime).IsInstanceOfType(result))
                {
                    ((ITsCHdaActualTime)result).StartTime = StartTime;
                    ((ITsCHdaActualTime)result).EndTime   = EndTime;
                }

                // add item identifier to value collection.
                var itemID = (OpcItem)m_items[result.ServerHandle];

                if (itemID != null)
                {
                    result.ItemName     = itemID.ItemName;
                    result.ItemPath     = itemID.ItemPath;
                    result.ServerHandle = itemID.ServerHandle;
                    result.ClientHandle = itemID.ClientHandle;
                }
            }
        }
        #endregion

        #region Private Members
        private object m_requestHandle = null; 
        private Delegate m_callback = null; 
        private int m_requestID = 0; 
        private int m_cancelID = 0;
        private DateTime m_startTime = DateTime.MinValue;
        private DateTime m_endTime = DateTime.MinValue;
        private Hashtable m_items = null;
        private ArrayList m_results = null;
        private event TsCHdaCancelCompleteEventHandler m_cancelCompleteEvent = null;
        #endregion
    }
}
