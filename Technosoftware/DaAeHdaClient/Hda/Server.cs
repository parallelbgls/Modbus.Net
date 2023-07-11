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
using System.Runtime.Serialization;
#endregion

namespace Technosoftware.DaAeHdaClient.Hda
{
    /// <summary>
    /// An in-process object used to access OPC Data Access servers.
    /// </summary>
    [Serializable]
    public class TsCHdaServer : OpcServer
    {
        #region Class Names

        /// <summary>
        /// A set of names for fields used in serialization.
        /// </summary>
        private class Names
        {
            internal const string Trends = "Trends";
        }
        #endregion

        #region Fields
        private Hashtable items_ = new Hashtable();
        private TsCHdaAttributeCollection attributes_ = new TsCHdaAttributeCollection();
        private TsCHdaAggregateCollection aggregates_ = new TsCHdaAggregateCollection();
        private TsCHdaTrendCollection trends_ = new TsCHdaTrendCollection();
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes the object with a factory and a default OpcUrl.
        /// </summary>
        /// <param name="factory">The TsOpcFactory used to connect to remote servers.</param>
        /// <param name="url">The network address of a remote server.</param>
        public TsCHdaServer(OpcFactory factory, OpcUrl url)
            :
            base(factory, url)
        {
        }

        /// <summary>
        /// Construct a server by de-serializing its OpcUrl from the stream.
        /// </summary>
        protected TsCHdaServer(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var trends = (TsCHdaTrend[])info.GetValue(Names.Trends, typeof(TsCHdaTrend[]));

            if (trends != null)
            {
                Array.ForEach(trends, trend =>
                {
                    trend.SetServer(this);
                    trends_.Add(trend);
                });
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns a collection of item attributes supported by the server.
        /// </summary>
        public TsCHdaAttributeCollection Attributes => attributes_;

        /// <summary>
		/// Returns a collection of aggregates supported by the server.
		/// </summary>
		public TsCHdaAggregateCollection Aggregates => aggregates_;

        /// <summary>
		/// Returns a collection of items with server handles assigned to them.
		/// </summary>
		public OpcItemCollection Items => new OpcItemCollection(items_.Values);

        /// <summary>
		/// Returns a collection of trends created for the server.
		/// </summary>
		public TsCHdaTrendCollection Trends => trends_;
        #endregion

        #region Public Methods
        /// <summary>
        /// Connects to the server with the specified OpcUrl and credentials.
        /// </summary>
        public override void Connect(OpcUrl url, OpcConnectData connectData)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            // connect to server.
            base.Connect(url, connectData);

            // fetch supported attributes.
            GetAttributes();

            // fetch supported aggregates.
            GetAggregates();

            // create items for trends.
            foreach (TsCHdaTrend trend in trends_)
            {
                var itemIDs = new ArrayList();

                foreach (TsCHdaItem item in trend.Items)
                {
                    itemIDs.Add(new OpcItem(item));
                }

                // save server handles for each item.
                var results = CreateItems((OpcItem[])itemIDs.ToArray(typeof(OpcItem)));

                if (results != null)
                {
                    for (var ii = 0; ii < results.Length; ii++)
                    {
                        trend.Items[ii].ServerHandle = null;

                        if (results[ii].Result.Succeeded())
                        {
                            trend.Items[ii].ServerHandle = results[ii].ServerHandle;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Disconnects from the server and releases all network resources.
        /// </summary>
        public override void Disconnect()
        {
            if (Server == null) throw new NotConnectedException();

            // dispose of all items first.
            if (items_.Count > 0)
            {

                try
                {
                    var items = new ArrayList(items_.Count);
                    items.AddRange(items_);

                    ((ITsCHdaServer)Server).ReleaseItems((OpcItem[])items.ToArray(typeof(OpcItem)));
                }
                catch
                {
                    // ignore errors.
                }

                items_.Clear();
            }

            // invalidate server handles for trends.
            foreach (TsCHdaTrend trend in trends_)
            {
                foreach (TsCHdaItem item in trend.Items)
                {
                    item.ServerHandle = null;
                }
            }

            // disconnect from server.
            base.Disconnect();
        }
        #endregion

        #region GetStatus
        /// <summary>
        /// Returns the current server status.
        /// </summary>
        /// <returns>The current server status.</returns>
        public OpcServerStatus GetServerStatus()
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();

            var status = ((ITsCHdaServer)Server).GetServerStatus();


            if (status != null)
            {
                if (status.StatusInfo == null)
                {
                    status.StatusInfo = GetString($"serverState.{status.ServerState}");
                }
            }
            else
            {
                throw new NotConnectedException();
            }

            return status;
        }
        #endregion

        #region GetAttributes
        /// <summary>
        /// Returns the item attributes supported by the server.
        /// </summary>
        /// <returns>The a set of item attributes and their descriptions.</returns>
        public TsCHdaAttribute[] GetAttributes()
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            // clear existing cached list.
            attributes_.Clear();

            var attributes = ((ITsCHdaServer)Server).GetAttributes();

            // save a locale copy.
            if (attributes != null)
            {
                attributes_.Init(attributes);
            }

            return attributes;
        }
        #endregion

        #region GetAggregates
        /// <summary>
        /// Returns the aggregates supported by the server.
        /// </summary>
        /// <returns>The a set of aggregates and their descriptions.</returns>
        public TsCHdaAggregate[] GetAggregates()
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            // discard existing cached list.
            aggregates_.Clear();

            var aggregates = ((ITsCHdaServer)Server).GetAggregates();

            // save a locale copy.
            if (aggregates != null)
            {
                aggregates_.Init(aggregates);
            }

            return aggregates;
        }
        #endregion

        #region CreateBrowser
        /// <summary>
        /// Creates a object used to browse the server address space.
        /// </summary>
        /// <param name="filters">The set of attribute filters to use when browsing.</param>
        /// <param name="results">A result code for each individual filter.</param>
        /// <returns>A browser object that must be released by calling Dispose().</returns>
        public ITsCHdaBrowser CreateBrowser(TsCHdaBrowseFilter[] filters, out OpcResult[] results)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).CreateBrowser(filters, out results);
        }
        #endregion

        #region CreateItems
        /// <summary>
        /// Creates a set of items.
        /// </summary>
        /// <param name="items">The identifiers for the items to create.</param>
        /// <returns>The results for each item containing the server handle and result code.</returns>
        public OpcItemResult[] CreateItems(OpcItem[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            var results = ((ITsCHdaServer)Server).CreateItems(items);

            // save items for future reference.
            if (results != null)
            {
                foreach (var result in results)
                {
                    if (result.Result.Succeeded())
                    {
                        items_.Add(result.ServerHandle, new OpcItem(result));
                    }
                }
            }

            return results;
        }
        #endregion

        #region ReleaseItems
        /// <summary>
        /// Releases a set of previously created items.
        /// </summary>
        /// <param name="items">The server handles for the items to release.</param>
        /// <returns>The results for each item containing the result code.</returns>
        public OpcItemResult[] ReleaseItems(OpcItem[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();

            var results = ((ITsCHdaServer)Server).ReleaseItems(items);

            // remove items from local cache.
            if (results != null)
            {
                foreach (var result in results)
                {
                    if (result.Result.Succeeded())
                    {
                        items_.Remove(result.ServerHandle);
                    }
                }
            }

            return results;
        }
        #endregion

        #region ValidateItems
        /// <summary>
        /// Validates a set of items.
        /// </summary>
        /// <param name="items">The identifiers for the items to validate.</param>
        /// <returns>The results for each item containing the result code.</returns>
        public OpcItemResult[] ValidateItems(OpcItem[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).ValidateItems(items);
        }
        #endregion

        #region ReadRaw
        /// <summary>
        /// Reads raw (unprocessed) data from the historian database for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="maxValues">The number of values to be read for each item.</param>
        /// <param name="includeBounds">Whether the bounding item values should be returned.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <returns>A set of values, qualities and timestamps within the requested time range for each item.</returns>
        internal TsCHdaItemValueCollection[] ReadRaw(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            int maxValues,
            bool includeBounds,
            OpcItem[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).ReadRaw(startTime, endTime, maxValues, includeBounds, items);
        }

        /// <summary>
        /// Sends an asynchronous request to read raw data from the historian database for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="maxValues">The number of values to be read for each item.</param>
        /// <param name="includeBounds">Whether the bounding item values should be returned.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        internal OpcItemResult[] ReadRaw(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            int maxValues,
            bool includeBounds,
            OpcItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).ReadRaw(startTime, endTime, maxValues, includeBounds, items, requestHandle, callback, out request);
        }

        /// <summary>
        /// Requests that the server periodically send notifications when new data becomes available for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="updateInterval">The frequency, in seconds, that the server should check for new data.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        internal OpcItemResult[] AdviseRaw(
            TsCHdaTime startTime,
            decimal updateInterval,
            OpcItem[] items,
            object requestHandle,
            TsCHdaDataUpdateEventHandler callback,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).AdviseRaw(startTime, updateInterval, items, requestHandle, callback, out request);
        }

        /// <summary>
        /// Begins the playback raw data from the historian database for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="maxValues">The number of values to be read for each item.</param>      
        /// <param name="updateInterval">The frequency, in seconds, that the server send data.</param>
        /// <param name="playbackDuration">The duration, in seconds, of the timespan returned with each update.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        internal OpcItemResult[] PlaybackRaw(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            int maxValues,
            decimal updateInterval,
            decimal playbackDuration,
            OpcItem[] items,
            object requestHandle,
            TsCHdaDataUpdateEventHandler callback,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).PlaybackRaw(startTime, endTime, maxValues, updateInterval, playbackDuration, items, requestHandle, callback, out request);
        }
        #endregion

        #region ReadProcessed
        /// <summary>
        /// Reads processed data from the historian database for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="resampleInterval">The interval between returned values.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <returns>A set of values, qualities and timestamps within the requested time range for each item.</returns>
        internal TsCHdaItemValueCollection[] ReadProcessed(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            decimal resampleInterval,
            TsCHdaItem[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).ReadProcessed(startTime, endTime, resampleInterval, items);
        }

        /// <summary>
        /// Sends an asynchronous request to read processed data from the historian database for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="resampleInterval">The interval between returned values.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        internal OpcItemResult[] ReadProcessed(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            decimal resampleInterval,
            TsCHdaItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();

            var results = ((ITsCHdaServer)Server).ReadProcessed(
                startTime,
                endTime,
                resampleInterval,
                items,
                requestHandle,
                callback,
                out request);

            return results;
        }

        /// <summary>
        /// Requests that the server periodically send notifications when new data becomes available for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="resampleInterval">The interval between returned values.</param>
        /// <param name="numberOfIntervals">The number of resample intervals that the server should return in each callback.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        internal OpcItemResult[] AdviseProcessed(
            TsCHdaTime startTime,
            decimal resampleInterval,
            int numberOfIntervals,
            TsCHdaItem[] items,
            object requestHandle,
            TsCHdaDataUpdateEventHandler callback,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();

            var results = ((ITsCHdaServer)Server).AdviseProcessed(
                startTime,
                resampleInterval,
                numberOfIntervals,
                items,
                requestHandle,
                callback,
                out request);

            return results;
        }

        /// <summary>
        /// Begins the playback of processed data from the historian database for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="resampleInterval">The interval between returned values.</param>
        /// <param name="numberOfIntervals">The number of resample intervals that the server should return in each callback.</param>
        /// <param name="updateInterval">The frequency, in seconds, that the server send data.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        internal OpcItemResult[] PlaybackProcessed(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            decimal resampleInterval,
            int numberOfIntervals,
            decimal updateInterval,
            TsCHdaItem[] items,
            object requestHandle,
            TsCHdaDataUpdateEventHandler callback,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();

            var results = ((ITsCHdaServer)Server).PlaybackProcessed(
                startTime,
                endTime,
                resampleInterval,
                numberOfIntervals,
                updateInterval,
                items,
                requestHandle,
                callback,
                out request);

            return results;
        }
        #endregion

        #region ReadAtTime
        /// <summary>
        /// Reads data from the historian database for a set of items at specific times.
        /// </summary>
        /// <param name="timestamps">The set of timestamps to use when reading items values.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <returns>A set of values, qualities and timestamps within the requested time range for each item.</returns>
        internal TsCHdaItemValueCollection[] ReadAtTime(DateTime[] timestamps, OpcItem[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).ReadAtTime(timestamps, items);
        }

        /// <summary>
        /// Sends an asynchronous request to read item values at specific times.
        /// </summary>
        /// <param name="timestamps">The set of timestamps to use when reading items values.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        internal OpcItemResult[] ReadAtTime(
            DateTime[] timestamps,
            OpcItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();

            var results = ((ITsCHdaServer)Server).ReadAtTime(
                timestamps,
                items,
                requestHandle,
                callback,
                out request);

            return results;
        }
        #endregion

        #region ReadModified
        /// <summary>
        /// Reads item values that have been deleted or replaced.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="maxValues">The number of values to be read for each item.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <returns>A set of values, qualities and timestamps within the requested time range for each item.</returns>
        internal TsCHdaModifiedValueCollection[] ReadModified(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            int maxValues,
            OpcItem[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).ReadModified(startTime, endTime, maxValues, items);
        }

        /// <summary>
        /// Sends an asynchronous request to read item values that have been deleted or replaced.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="maxValues">The number of values to be read for each item.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        internal OpcItemResult[] ReadModified(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            int maxValues,
            OpcItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();

            var results = ((ITsCHdaServer)Server).ReadModified(
                startTime,
                endTime,
                maxValues,
                items,
                requestHandle,
                callback,
                out request);

            return results;
        }
        #endregion

        #region ReadAttributes
        /// <summary>
        /// Reads the current or historical values for the attributes of an item.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="item">The item to read (must include the item name).</param>
        /// <param name="attributeIDs">The attributes to read.</param>
        /// <returns>A set of attribute values for each requested attribute.</returns>
        internal TsCHdaItemAttributeCollection ReadAttributes(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem item,
            int[] attributeIDs)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).ReadAttributes(startTime, endTime, item, attributeIDs);
        }

        /// <summary>
        /// Sends an asynchronous request to read the attributes of an item.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="item">The item to read (must include the item name).</param>
        /// <param name="attributeIDs">The attributes to read.</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the attribute ids.</returns>
        internal TsCHdaResultCollection ReadAttributes(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem item,
            int[] attributeIDs,
            object requestHandle,
            TsCHdaReadAttributesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            var results = ((ITsCHdaServer)Server).ReadAttributes(
                startTime,
                endTime,
                item,
                attributeIDs,
                requestHandle,
                callback,
                out request);

            return results;
        }
        #endregion

        #region ReadAnnotations
        /// <summary>
        /// Reads any annotations for an item within the a time interval.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <returns>A set of annotations within the requested time range for each item.</returns>
        internal TsCHdaAnnotationValueCollection[] ReadAnnotations(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).ReadAnnotations(startTime, endTime, items);
        }

        /// <summary>
        /// Sends an asynchronous request to read the annotations for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        internal OpcItemResult[] ReadAnnotations(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem[] items,
            object requestHandle,
            TsCHdaReadAnnotationsCompleteEventHandler callback,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();

            var results = ((ITsCHdaServer)Server).ReadAnnotations(
                startTime,
                endTime,
                items,
                requestHandle,
                callback,
                out request);

            return results;
        }
        #endregion

        #region InsertAnnotations
        /// <summary>
        /// Inserts annotations for one or more items.
        /// </summary>
        /// <param name="items">A list of annotations to add for each item (must include the item name).</param>
        /// <returns>The results of the insert operation for each annotation set.</returns>
        public TsCHdaResultCollection[] InsertAnnotations(TsCHdaAnnotationValueCollection[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).InsertAnnotations(items);
        }
        /// <summary>
        /// Sends an asynchronous request to inserts annotations for one or more items.
        /// </summary>
        /// <param name="items">A list of annotations to add for each item (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] InsertAnnotations(
            TsCHdaAnnotationValueCollection[] items,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();

            var results = ((ITsCHdaServer)Server).InsertAnnotations(
                items,
                requestHandle,
                callback,
                out request);

            return results;
        }
        #endregion

        #region Insert
        /// <summary>
        /// Inserts the values into the history database for one or more items. 
        /// </summary>
        /// <param name="items">The set of values to insert.</param>
        /// <param name="replace">Whether existing values should be replaced.</param>
        /// <returns></returns>
        public TsCHdaResultCollection[] Insert(TsCHdaItemValueCollection[] items, bool replace)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).Insert(items, replace);
        }

        /// <summary>
        /// Sends an asynchronous request to inserts values for one or more items.
        /// </summary>
        /// <param name="items">The set of values to insert.</param>
        /// <param name="replace">Whether existing values should be replaced.</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] Insert(
            TsCHdaItemValueCollection[] items,
            bool replace,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();

            var results = ((ITsCHdaServer)Server).Insert(
                items,
                replace,
                requestHandle,
                callback,
                out request);

            return results;
        }
        #endregion

        #region Replace
        /// <summary>
        /// Replace the values into the history database for one or more items. 
        /// </summary>
        /// <param name="items">The set of values to replace.</param>
        /// <returns></returns>
        public TsCHdaResultCollection[] Replace(TsCHdaItemValueCollection[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).Replace(items);
        }


        /// <summary>
        /// Sends an asynchronous request to replace values for one or more items.
        /// </summary>
        /// <param name="items">The set of values to replace.</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        public OpcItemResult[] Replace(
            TsCHdaItemValueCollection[] items,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();

            var results = ((ITsCHdaServer)Server).Replace(
                items,
                requestHandle,
                callback,
                out request);

            return results;
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes the values with the specified time domain for one or more items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to delete.</param>
        /// <param name="endTime">The end of the history period to be delete.</param>
        /// <param name="items">The set of items to delete (must include the item name).</param>
        /// <returns>The results of the delete operation for each item.</returns>
        internal OpcItemResult[] Delete(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).Delete(startTime, endTime, items);
        }

        /// <summary>
        /// Sends an asynchronous request to delete values for one or more items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to delete.</param>
        /// <param name="endTime">The end of the history period to be delete.</param>
        /// <param name="items">The set of items to delete (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        internal OpcItemResult[] Delete(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem[] items,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();

            var results = ((ITsCHdaServer)Server).Delete(
                startTime,
                endTime,
                items,
                requestHandle,
                callback,
                out request);

            return results;
        }
        #endregion

        #region DeleteAtTime
        /// <summary>
        /// Deletes the values at the specified times for one or more items. 
        /// </summary>
        /// <param name="items">The set of timestamps to delete for one or more items.</param>
        /// <returns>The results of the operation for each timestamp.</returns>
        internal TsCHdaResultCollection[] DeleteAtTime(TsCHdaItemTimeCollection[] items)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            return ((ITsCHdaServer)Server).DeleteAtTime(items);
        }

        /// <summary>
        /// Sends an asynchronous request to delete values for one or more items at a specified times.
        /// </summary>
        /// <param name="items">The set of timestamps to delete for one or more items.</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        internal OpcItemResult[] DeleteAtTime(
            TsCHdaItemTimeCollection[] items,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();

            var results = ((ITsCHdaServer)Server).DeleteAtTime(
                items,
                requestHandle,
                callback,
                out request);

            return results;
        }
        #endregion

        #region CancelRequest
        /// <summary>
        /// Cancels an asynchronous request.
        /// </summary>
        /// <param name="request">The state object for the request to cancel.</param>
        public void CancelRequest(IOpcRequest request)
        {
            LicenseHandler.ValidateFeatures(LicenseHandler.ProductFeature.HistoricalAccess);
            if (Server == null) throw new NotConnectedException();
            ((ITsCHdaServer)Server).CancelRequest(request);
        }

        /// <summary>
        /// Cancels an asynchronous request.
        /// </summary>
        /// <param name="request">The state object for the request to cancel.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        public void CancelRequest(IOpcRequest request, TsCHdaCancelCompleteEventHandler callback)
        {
            if (Server == null) throw new NotConnectedException();
            ((ITsCHdaServer)Server).CancelRequest(request, callback);
        }
        #endregion

        #region ISerializable Members
        /// <summary>
        /// Serializes a server into a stream.
        /// </summary>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            TsCHdaTrend[] trends = null;

            if (trends_.Count > 0)
            {
                trends = new TsCHdaTrend[trends_.Count];

                for (var ii = 0; ii < trends.Length; ii++)
                {
                    trends[ii] = trends_[ii];
                }
            }

            info.AddValue(Names.Trends, trends);
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Returns an unconnected copy of the server with the same OpcUrl. 
        /// </summary>
        public override object Clone()
        {
            // clone the base object.
            var clone = (TsCHdaServer)base.Clone();

            // return clone.
            return clone;
        }
        #endregion
    }
}
