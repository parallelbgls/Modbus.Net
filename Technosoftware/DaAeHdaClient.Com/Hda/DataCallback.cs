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
using Technosoftware.OpcRcw.Hda;

#endregion

namespace Technosoftware.DaAeHdaClient.Com.Hda
{   
    /// <summary>
    ///  A class that implements the HDA data callback interface.
    /// </summary>
	internal class  DataCallback : IOPCHDA_DataCallback
    {   
        /// <summary>
        /// Initializes the object with the containing subscription object.
        /// </summary>
        public DataCallback() {}

        /// <summary>
        /// Fired when an exception occurs during callback processing.
        /// </summary>
        public event TsCHdaCallbackExceptionEventHandler CallbackExceptionEvent
        {
            add    {lock (this) { _callbackExceptionEvent += value; }}
            remove {lock (this) { _callbackExceptionEvent -= value; }}
        }
    
        /// <summary>
        /// Creates a new request object.
        /// </summary>
        public Request CreateRequest(object requestHandle, Delegate callback)
        {
            lock (this)
            {
                // create a new request.
                var request = new Request(requestHandle, callback, ++m_nextID);

                // no items yet - callback may return before async call returns.
                m_requests[request.RequestID] = request;    
        
                // return requests.
                return request;
            }
        }

        /// <summary>
        /// Cancels an existing request.
        /// </summary>
        public bool CancelRequest(Request request, TsCHdaCancelCompleteEventHandler callback)
        {
            lock (this)
            {
                // check if it is a valid request.
                if (!m_requests.Contains(request.RequestID))
                {
                    return false;
                }

                // request will be removed when the cancel complete callback arrives.
                if (callback != null)
                {
                    request.CancelCompleteEvent += callback;
                }

                // no confirmation required - remove request immediately.
                else
                {
                    m_requests.Remove(request.RequestID);
                }

                // request will be cancelled.
                return true;
            }
        }

        #region IOPCHDA_DataCallback Members
        /// <summary>
        /// Called when new data arrives for a subscription.
        /// </summary>
        public void OnDataChange(
            int           dwTransactionID, 
            int           hrStatus,
            int           dwNumItems, 
            OPCHDA_ITEM[] pItemValues,
            int[]         phrErrors)
        {
            try
            {
                lock (this)
                {
                    // lookup request transaction.
                    var request = (Request)m_requests[dwTransactionID];

                    if (request == null)
                    {
                        return;
                    }

                    // unmarshal results.
                    var results = new TsCHdaItemValueCollection[pItemValues.Length];

                    for (var ii = 0; ii < pItemValues.Length; ii++)
                    {
                        results[ii] = Interop.GetItemValueCollection(pItemValues[ii], false);

                        results[ii].ServerHandle = results[ii].ClientHandle;
                        results[ii].ClientHandle = null;
                        results[ii].Result     = Utilities.Interop.GetResultId(phrErrors[ii]);
                    }

                    // invoke callback - remove request if unexpected error occured.
                    if (request.InvokeCallback(results))
                    {
                        m_requests.Remove(request.RequestID);
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(dwTransactionID, exception);
            }
        }

        /// <summary>
        /// Called when an asynchronous read request completes.
        /// </summary>
        public void OnReadComplete(
            int           dwTransactionID, 
            int           hrStatus,
            int           dwNumItems, 
            OPCHDA_ITEM[] pItemValues,
            int[]         phrErrors)
        {
            try
            {
                lock (this)
                {
                    // lookup request transaction.
                    var request = (Request)m_requests[dwTransactionID];

                    if (request == null)
                    {
                        return;
                    }

                    // unmarshal results.
                    var results = new TsCHdaItemValueCollection[pItemValues.Length];

                    for (var ii = 0; ii < pItemValues.Length; ii++)
                    {
                        results[ii] = Interop.GetItemValueCollection(pItemValues[ii], false);

                        results[ii].ServerHandle = pItemValues[ii].hClient;
                        results[ii].Result     = Utilities.Interop.GetResultId(phrErrors[ii]);
                    }

                    // invoke callback - remove request if all results arrived.
                    if (request.InvokeCallback(results))
                    {
                        m_requests.Remove(request.RequestID);
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(dwTransactionID, exception);
            }
        }

        /// <summary>
        /// Called when an asynchronous read modified request completes.
        /// </summary>
        public void OnReadModifiedComplete(
            int                   dwTransactionID, 
            int                   hrStatus,
            int                   dwNumItems, 
            OPCHDA_MODIFIEDITEM[] pItemValues,
            int[]                 phrErrors)
        {
            try
            {
                lock (this)
                {
                    // lookup request transaction.
                    var request = (Request)m_requests[dwTransactionID];

                    if (request == null)
                    {
                        return;
                    }

                    // unmarshal results.
                    var results = new TsCHdaModifiedValueCollection[pItemValues.Length];

                    for (var ii = 0; ii < pItemValues.Length; ii++)
                    {
                        results[ii] = Interop.GetModifiedValueCollection(pItemValues[ii], false);

                        results[ii].ServerHandle = pItemValues[ii].hClient;
                        results[ii].Result     = Utilities.Interop.GetResultId(phrErrors[ii]);
                    }

                    // invoke callback - remove request if all results arrived.
                    if (request.InvokeCallback(results))
                    {
                        m_requests.Remove(request.RequestID);
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(dwTransactionID, exception);
            }
        }

        /// <summary>
        /// Called when an asynchronous read attributes request completes.
        /// </summary>
        public void OnReadAttributeComplete(
            int                dwTransactionID, 
            int                hrStatus,
            int                hClient, 
            int                dwNumItems, 
            OPCHDA_ATTRIBUTE[] pAttributeValues,
            int[]              phrErrors)
        {
            try
            {
                lock (this)
                {
                    // lookup request transaction.
                    var request = (Request)m_requests[dwTransactionID];

                    if (request == null)
                    {
                        return;
                    }

                    // create item object to collect results.
                    var item = new TsCHdaItemAttributeCollection();
                    item.ServerHandle = hClient;

                    // unmarshal results.
                    var results = new TsCHdaAttributeValueCollection[pAttributeValues.Length];

                    for (var ii = 0; ii < pAttributeValues.Length; ii++)
                    {
                        results[ii] = Interop.GetAttributeValueCollection(pAttributeValues[ii], false);

                        results[ii].Result = Utilities.Interop.GetResultId(phrErrors[ii]);
                    
                        item.Add(results[ii]);
                    }

                    // invoke callback - remove request if all results arrived.
                    if (request.InvokeCallback(item))
                    {
                        m_requests.Remove(request.RequestID);
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(dwTransactionID, exception);
            }
        }

        /// <summary>
        /// Called when an asynchronous read annotations request completes.
        /// </summary>
        public void OnReadAnnotations(
            int                 dwTransactionID, 
            int                 hrStatus,
            int                 dwNumItems, 
            OPCHDA_ANNOTATION[] pAnnotationValues,
            int[]               phrErrors)
        {
            try
            {
                lock (this)
                {
                    // lookup request transaction.
                    var request = (Request)m_requests[dwTransactionID];

                    if (request == null)
                    {
                        return;
                    }

                    // unmarshal results.
                    var results = new TsCHdaAnnotationValueCollection[pAnnotationValues.Length];

                    for (var ii = 0; ii < pAnnotationValues.Length; ii++)
                    {
                        results[ii] = Interop.GetAnnotationValueCollection(pAnnotationValues[ii], false);

                        results[ii].ServerHandle = pAnnotationValues[ii].hClient;
                        results[ii].Result     = Utilities.Interop.GetResultId(phrErrors[ii]);
                    }

                    // invoke callback - remove request if all results arrived.
                    if (request.InvokeCallback(results))
                    {
                        m_requests.Remove(request.RequestID);
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(dwTransactionID, exception);
            }
        }

        /// <summary>
        /// Called when an asynchronous insert annotations request completes.
        /// </summary>
        public void OnInsertAnnotations(
            int   dwTransactionID, 
            int   hrStatus,
            int   dwCount, 
            int[] phClients, 
            int[] phrErrors)
        {

            try
            {
                lock (this)
                {
                    // lookup request transaction.
                    var request = (Request)m_requests[dwTransactionID];

                    if (request == null)
                    {
                        return;
                    }

                    // unmarshal results.
                    var results = new ArrayList();

                    if (dwCount > 0)
                    {
                        // subscription results in collections for the same item id.
                        var currentHandle = phClients[0];

                        var itemResults = new TsCHdaResultCollection();

                        for (var ii = 0; ii < dwCount; ii++)
                        {
                            // create a new collection for the next item's results.
                            if (phClients[ii] != currentHandle)
                            {
                                itemResults.ServerHandle = currentHandle;
                                results.Add(itemResults);
                                
                                currentHandle = phClients[ii];
                                itemResults = new TsCHdaResultCollection();
                            }

                            var result = new TsCHdaResult(Utilities.Interop.GetResultId(phrErrors[ii]));
                            itemResults.Add(result);
                        }

                        // add the last set of item results.
                        itemResults.ServerHandle = currentHandle;
                        results.Add(itemResults);
                    }

                    // invoke callback - remove request if all results arrived.
                    if (request.InvokeCallback((TsCHdaResultCollection[])results.ToArray(typeof(TsCHdaResultCollection))))
                    {
                        m_requests.Remove(request.RequestID);
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(dwTransactionID, exception);
            }
        }

        /// <summary>
        /// Called when a batch of data from playback request arrives.
        /// </summary>
        public void OnPlayback(
            int    dwTransactionID, 
            int    hrStatus,
            int    dwNumItems, 
            IntPtr ppItemValues,
            int[]  phrErrors)
        {
            try
            {
                lock (this)
                {
                    // lookup request transaction.
                    var request = (Request)m_requests[dwTransactionID];

                    if (request == null)
                    {
                        return;
                    }

                    // unmarshal results.
                    var results = new TsCHdaItemValueCollection[dwNumItems];

                    // the data is transfered as a array of pointers to items instead of simply
                    // as an array of items. This is due to a mistake in the HDA IDL.
                    var pItems = Utilities.Interop.GetInt32s(ref ppItemValues, dwNumItems, false);

                    for (var ii = 0; ii < dwNumItems; ii++)
                    {
                        // get pointer to item.
                        var pItem = (IntPtr)pItems[ii];
            
                        // unmarshal item as an array of length 1.
                        var item = Interop.GetItemValueCollections(ref pItem, 1, false);

                        if (item != null && item.Length == 1)
                        {
                            results[ii]              = item[0];
                            results[ii].ServerHandle = results[ii].ClientHandle;
                            results[ii].ClientHandle = null;
                            results[ii].Result     = Utilities.Interop.GetResultId(phrErrors[ii]);
                        }
                    }

                    // invoke callback - remove request if unexpected error occured.
                    if (request.InvokeCallback(results))
                    {
                        m_requests.Remove(request.RequestID);
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(dwTransactionID, exception);
            }
        }

        /// <summary>
        /// Called when an asynchronous update request completes.
        /// </summary>
        public void OnUpdateComplete(
            int   dwTransactionID, 
            int   hrStatus,
            int   dwCount, 
            int[] phClients, 
            int[] phrErrors)
        {
            try
            {
                lock (this)
                {
                    // lookup request transaction.
                    var request = (Request)m_requests[dwTransactionID];

                    if (request == null)
                    {
                        return;
                    }

                    // unmarshal results.
                    var results = new ArrayList();

                    if (dwCount > 0)
                    {
                        // subscription results in collections for the same item id.
                        var currentHandle = phClients[0];

                        var itemResults = new TsCHdaResultCollection();

                        for (var ii = 0; ii < dwCount; ii++)
                        {
                            // create a new collection for the next item's results.
                            if (phClients[ii] != currentHandle)
                            {
                                itemResults.ServerHandle = currentHandle;
                                results.Add(itemResults);
                                
                                currentHandle = phClients[ii];
                                itemResults = new TsCHdaResultCollection();
                            }

                            var result = new TsCHdaResult(Utilities.Interop.GetResultId(phrErrors[ii]));
                            itemResults.Add(result);
                        }

                        // add the last set of item results.
                        itemResults.ServerHandle = currentHandle;
                        results.Add(itemResults);
                    }

                    // invoke callback - remove request if all results arrived.
                    if (request.InvokeCallback((TsCHdaResultCollection[])results.ToArray(typeof(TsCHdaResultCollection))))
                    {
                        m_requests.Remove(request.RequestID);
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(dwTransactionID, exception);
            }
        }

        /// <summary>
        /// Called when an asynchronous request was cancelled successfully.
        /// </summary>
        public void OnCancelComplete(int dwCancelID)
        {
            try
            {
                lock (this)
                {
                    // lookup request.
                    var request = (Request)m_requests[dwCancelID];

                    if (request == null)
                    {
                        return;
                    }

                    // send the cancel complete notification.
                    request.OnCancelComplete();

                    // remove the request.
                    m_requests.Remove(request.RequestID);
                }
            }
            catch (Exception exception)
            {
                HandleException(dwCancelID, exception);
            }
        }
        #endregion  

        #region Private Methods
        /// <summary>
        /// Fires an event indicating an exception occurred during callback processing.
        /// </summary>
        void HandleException(int requestID, Exception exception)
        {
            lock (this)
            {
                // lookup request.
                var request = (Request)m_requests[requestID];

                if (request != null)
                {
                    // send notification.
                    if (_callbackExceptionEvent != null)
                    {
                        _callbackExceptionEvent(request, exception);
                    }
                }
            }
        }
        #endregion

        #region Private Members
		private int m_nextID;
        private Hashtable m_requests = new Hashtable();
		private TsCHdaCallbackExceptionEventHandler _callbackExceptionEvent;
        #endregion
    }
}
