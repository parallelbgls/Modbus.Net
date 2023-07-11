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
using System.Runtime.Serialization;
#endregion

namespace Technosoftware.DaAeHdaClient.Hda
{
    /// <summary>
    /// Manages a set of items and a set of read, update, subscribe or playback request parameters. 
    /// </summary>
    [Serializable]
    public class TsCHdaTrend : ISerializable, ICloneable
    {
        #region Class Names
        /// <summary>
        /// A set of names for fields used in serialization.
        /// </summary>
        private class Names
        {
            internal const string Name = "Name";
            internal const string AggregateId = "AggregateID";
            internal const string StartTime = "StartTime";
            internal const string EndTime = "EndTime";
            internal const string MaxValues = "MaxValues";
            internal const string IncludeBounds = "IncludeBounds";
            internal const string ResampleInterval = "ResampleInterval";
            internal const string UpdateInterval = "UpdateInterval";
            internal const string PlaybackInterval = "PlaybackInterval";
            internal const string PlaybackDuration = "PlaybackDuration";
            internal const string Timestamps = "Timestamps";
            internal const string Items = "Items";
        }
        #endregion

        #region Fields
        private static int count_;

        private TsCHdaServer hdaServer_;
        private int aggregate_ = TsCHdaAggregateID.NoAggregate;
        private decimal resampleInterval_;
        private TsCHdaItemTimeCollection timeStamps_ = new TsCHdaItemTimeCollection();
        private TsCHdaItemCollection items_ = new TsCHdaItemCollection();
        private decimal updateInterval_;
        private decimal playbackInterval_;
        private decimal playbackDuration_;

        private IOpcRequest subscription_;
        private IOpcRequest playback_;
        #endregion

        #region Constructors, Destructor, Initialization

        /// <summary>
        /// Initializes the object with the specified server.
        /// </summary>
        public TsCHdaTrend(TsCHdaServer server)
        {
            // save a reference to a server.
            hdaServer_ = server ?? throw new ArgumentNullException(nameof(server));

            // create a default name.
            do
            {
                Name = $"Trend{++count_,2:00}";
            }
            while (hdaServer_.Trends[Name] != null);
        }

        /// <summary>
        /// Construct a server by de-serializing its OpcUrl from the stream.
        /// </summary>
        protected TsCHdaTrend(SerializationInfo info, StreamingContext context)
        {
            // deserialize basic parameters.
            Name = (string)info.GetValue(Names.Name, typeof(string));
            aggregate_ = (int)info.GetValue(Names.AggregateId, typeof(int));
            StartTime = (TsCHdaTime)info.GetValue(Names.StartTime, typeof(TsCHdaTime));
            EndTime = (TsCHdaTime)info.GetValue(Names.EndTime, typeof(TsCHdaTime));
            MaxValues = (int)info.GetValue(Names.MaxValues, typeof(int));
            IncludeBounds = (bool)info.GetValue(Names.IncludeBounds, typeof(bool));
            resampleInterval_ = (decimal)info.GetValue(Names.ResampleInterval, typeof(decimal));
            updateInterval_ = (decimal)info.GetValue(Names.UpdateInterval, typeof(decimal));
            playbackInterval_ = (decimal)info.GetValue(Names.PlaybackInterval, typeof(decimal));
            playbackDuration_ = (decimal)info.GetValue(Names.PlaybackDuration, typeof(decimal));

            // deserialize timestamps.
            var timestamps = (DateTime[])info.GetValue(Names.Timestamps, typeof(DateTime[]));

            if (timestamps != null)
            {
                Array.ForEach(timestamps, timestamp => timeStamps_.Add(timestamp));
            }

            // deserialize items.
            var items = (TsCHdaItem[])info.GetValue(Names.Items, typeof(TsCHdaItem[]));

            if (items != null)
            {
                Array.ForEach(items, item => items_.Add(item));
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// The server containing the data in the trend.
        /// </summary>
        public TsCHdaServer Server => hdaServer_;

        /// <summary>
		/// A name for the trend used to display to the user.
		/// </summary>
		public string Name { get; set; }

        /// <summary>
        /// The default aggregate to use for the trend.
        /// </summary>
        public int Aggregate
        {
            get => aggregate_;
            set => aggregate_ = value;
        }

        /// <summary>
        /// The start time for the trend.
        /// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
        /// the time format (UTC or local time).
        /// </summary>
        public TsCHdaTime StartTime { get; set; }

        /// <summary>
        /// The end time for the trend.
        /// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
        /// the time format (UTC or local time).
        /// </summary>
        public TsCHdaTime EndTime { get; set; }

        /// <summary>
        /// The maximum number of data points per item in the trend.
        /// </summary>
        public int MaxValues { get; set; }

        /// <summary>
        /// Whether the trend includes the bounding values.
        /// </summary>
        public bool IncludeBounds { get; set; }

        /// <summary>
        /// The re-sampling interval (in seconds) to use for processed reads.
        /// </summary>
        public decimal ResampleInterval
        {
            get => resampleInterval_;
            set => resampleInterval_ = value;
        }

        /// <summary>
        /// The discrete set of timestamps for the trend.
        /// </summary>
        public TsCHdaItemTimeCollection Timestamps
        {
            get => timeStamps_;

            set => timeStamps_ = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The interval between updates from the server when subscribing to new data.
        /// </summary>
        /// <remarks>This specifies a number of seconds for raw data or the number of re-sample intervals for processed data.</remarks>
        public decimal UpdateInterval
        {
            get => updateInterval_;
            set => updateInterval_ = value;
        }

        /// <summary>
        /// Whether the server is currently sending updates for the trend.
        /// </summary>
        public bool SubscriptionActive => subscription_ != null;

        /// <summary>
		/// The interval between updates from the server when playing back existing data. 
		/// </summary>
		/// <remarks>This specifies a number of seconds for raw data and for processed data.</remarks>
		public decimal PlaybackInterval
        {
            get => playbackInterval_;
            set => playbackInterval_ = value;
        }

        /// <summary>
        /// The amount of data that should be returned with each update when playing back existing data.
        /// </summary>
        /// <remarks>This specifies a number of seconds for raw data or the number of re-sample intervals for processed data.</remarks>
        public decimal PlaybackDuration
        {
            get => playbackDuration_;
            set => playbackDuration_ = value;
        }

        /// <summary>
        /// Whether the server is currently playing data back for the trend.
        /// </summary>
        public bool PlaybackActive => playback_ != null;

        /// <summary>
		/// The items
		/// </summary>
		public TsCHdaItemCollection Items => items_;
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the items in a trend as an array.
        /// </summary>
        public TsCHdaItem[] GetItems()
        {
            var items = new TsCHdaItem[items_.Count];

            for (var ii = 0; ii < items_.Count; ii++)
            {
                items[ii] = items_[ii];
            }

            return items;
        }

        /// <summary>
        /// Creates a handle for an item and adds it to the trend.
        /// </summary>
        public TsCHdaItem AddItem(OpcItem itemId)
        {
            if (itemId == null) throw new ArgumentNullException(nameof(itemId));

            // assign client handle.
            if (itemId.ClientHandle == null)
            {
                itemId.ClientHandle = Guid.NewGuid().ToString();
            }

            // create server handle.
            var results = hdaServer_.CreateItems(new[] { itemId });

            // check for valid results.
            if (results == null || results.Length != 1)
            {
                throw new OpcResultException(new OpcResult(OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "The browse operation cannot continue");
            }

            // check result code.
            if (results[0].Result.Failed())
            {
                throw new OpcResultException(results[0].Result, "Could not add item to trend.");
            }

            // add new item.
            var item = new TsCHdaItem(results[0]);
            items_.Add(item);

            // return new item.
            return item;
        }

        /// <summary>
        /// Removes an item from the trend.
        /// </summary>
        public void RemoveItem(TsCHdaItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            for (var ii = 0; ii < items_.Count; ii++)
            {
                if (item.Equals(items_[ii]))
                {
                    hdaServer_.ReleaseItems(new OpcItem[] { item });
                    items_.RemoveAt(ii);
                    return;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(item), item.Key, @"Item not found in collection.");
        }

        /// <summary>
        /// Removes all items from the trend.
        /// </summary>
        public void ClearItems()
        {
            hdaServer_.ReleaseItems(GetItems());
            items_.Clear();
        }

        #region Read
        /// <summary>
        /// Reads the values for a for all items in the trend.
        /// </summary>
        public TsCHdaItemValueCollection[] Read()
        {
            return Read(GetItems());
        }

        /// <summary>
        /// Reads the values for a for a set of items. 
        /// </summary>
        public TsCHdaItemValueCollection[] Read(TsCHdaItem[] items)
        {
            // read raw data.
            if (Aggregate == TsCHdaAggregateID.NoAggregate)
            {
                return ReadRaw(items);
            }

            // read processed data.
            return ReadProcessed(items);
        }


        /// <summary>
        /// Starts an asynchronous read request for all items in the trend. 
        /// </summary>
        public OpcItemResult[] Read(
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            return Read(GetItems(), requestHandle, callback, out request);
        }

        /// <summary>
		/// Starts an asynchronous read request for a set of items. 
		/// </summary>
		public OpcItemResult[] Read(
            TsCHdaItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            // read raw data.
            if (Aggregate == TsCHdaAggregateID.NoAggregate)
            {
                return ReadRaw(items, requestHandle, callback, out request);
            }

            // read processed data.

            return ReadProcessed(items, requestHandle, callback, out request);
        }
        #endregion

        #region ReadRaw
        /// <summary>
        /// Reads the raw values for a for all items in the trend.
        /// </summary>
        public TsCHdaItemValueCollection[] ReadRaw()
        {
            return ReadRaw(GetItems());
        }

        /// <summary>
        /// Reads the raw values for a for a set of items. 
        /// </summary>
        public TsCHdaItemValueCollection[] ReadRaw(TsCHdaItem[] items)
        {
            var results = hdaServer_.ReadRaw(
                StartTime,
                EndTime,
                MaxValues,
                IncludeBounds,
                items);

            return results;
        }

        /// <summary>
		/// Starts an asynchronous read raw request for all items in the trend. 
		/// </summary>
		public OpcItemResult[] ReadRaw(
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            return Read(GetItems(), requestHandle, callback, out request);
        }

        /// <summary>
        /// Starts an asynchronous read raw request for a set of items. 
        /// </summary>
        public OpcItemResult[] ReadRaw(
            OpcItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            var results = hdaServer_.ReadRaw(
                StartTime,
                EndTime,
                MaxValues,
                IncludeBounds,
                items,
                requestHandle,
                callback,
                out request);

            return results;
        }
        #endregion

        #region ReadProcessed
        /// <summary>
        /// Reads the processed values for a for all items in the trend.
        /// </summary>
        public TsCHdaItemValueCollection[] ReadProcessed()
        {
            return ReadProcessed(GetItems());
        }

        /// <summary>
        /// Reads the processed values for a for a set of items. 
        /// </summary>
        public TsCHdaItemValueCollection[] ReadProcessed(TsCHdaItem[] items)
        {
            var localItems = ApplyDefaultAggregate(items);

            var results = hdaServer_.ReadProcessed(
                StartTime,
                EndTime,
                ResampleInterval,
                localItems);

            return results;
        }

        /// <summary>
        /// Starts an asynchronous read processed request for all items in the trend. 
        /// </summary>
        public OpcItemResult[] ReadProcessed(
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            return ReadProcessed(GetItems(), requestHandle, callback, out request);
        }

        /// <summary>
        /// Starts an asynchronous read processed request for a set of items. 
        /// </summary>
        public OpcItemResult[] ReadProcessed(
            TsCHdaItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            var localItems = ApplyDefaultAggregate(items);

            var results = hdaServer_.ReadProcessed(
                StartTime,
                EndTime,
                ResampleInterval,
                localItems,
                requestHandle,
                callback,
                out request);

            return results;
        }
        #endregion

        #region Subscribe
        /// <summary>
        /// Establishes a subscription for the trend.
        /// </summary>
        public OpcItemResult[] Subscribe(
            object subscriptionHandle,
            TsCHdaDataUpdateEventHandler callback)
        {
            OpcItemResult[] results = null;

            // subscribe to raw data.
            if (Aggregate == TsCHdaAggregateID.NoAggregate)
            {
                results = hdaServer_.AdviseRaw(
                    StartTime,
                    UpdateInterval,
                    GetItems(),
                    subscriptionHandle,
                    callback,
                    out subscription_);
            }

            // subscribe processed data.
            else
            {
                var localItems = ApplyDefaultAggregate(GetItems());

                results = hdaServer_.AdviseProcessed(
                    StartTime,
                    ResampleInterval,
                    (int)UpdateInterval,
                    localItems,
                    subscriptionHandle,
                    callback,
                    out subscription_);
            }

            return results;
        }

        /// <summary>
        /// Cancels an existing subscription.
        /// </summary>
        public void SubscribeCancel()
        {
            if (subscription_ != null)
            {
                hdaServer_.CancelRequest(subscription_);
                subscription_ = null;
            }
        }
        #endregion

        #region Playback
        /// <summary>
        /// Begins playback of data for a trend.
        /// </summary>
        public OpcItemResult[] Playback(
            object playbackHandle,
            TsCHdaDataUpdateEventHandler callback)
        {
            OpcItemResult[] results;

            // playback raw data.
            if (Aggregate == TsCHdaAggregateID.NoAggregate)
            {
                results = hdaServer_.PlaybackRaw(
                    StartTime,
                    EndTime,
                    MaxValues,
                    PlaybackInterval,
                    PlaybackDuration,
                    GetItems(),
                    playbackHandle,
                    callback,
                    out playback_);
            }

            // playback processed data.
            else
            {
                var localItems = ApplyDefaultAggregate(GetItems());

                results = hdaServer_.PlaybackProcessed(
                    StartTime,
                    EndTime,
                    ResampleInterval,
                    (int)PlaybackDuration,
                    PlaybackInterval,
                    localItems,
                    playbackHandle,
                    callback,
                    out playback_);
            }

            return results;
        }

        /// <summary>
        /// Cancels an existing playback operation.
        /// </summary>
        public void PlaybackCancel()
        {
            if (playback_ != null)
            {
                hdaServer_.CancelRequest(playback_);
                playback_ = null;
            }
        }
        #endregion

        #region ReadModified
        /// <summary>
        /// Reads the modified values for all items in the trend.
        /// </summary>
        public TsCHdaModifiedValueCollection[] ReadModified()
        {
            return ReadModified(GetItems());
        }

        /// <summary>
        /// Reads the modified values for a for a set of items. 
        /// </summary>
        public TsCHdaModifiedValueCollection[] ReadModified(TsCHdaItem[] items)
        {
            var results = hdaServer_.ReadModified(
                StartTime,
                EndTime,
                MaxValues,
                items);

            return results;
        }

        /// <summary>
        /// Starts an asynchronous read modified request for all items in the trend.
        /// </summary>
        public OpcItemResult[] ReadModified(
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            return ReadModified(GetItems(), requestHandle, callback, out request);
        }

        /// <summary>
        /// Starts an asynchronous read modified request for a set of items. 
        /// </summary>
        public OpcItemResult[] ReadModified(
            TsCHdaItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            var results = hdaServer_.ReadModified(
                StartTime,
                EndTime,
                MaxValues,
                items,
                requestHandle,
                callback,
                out request);

            return results;
        }
        #endregion

        #region ReadAtTime
        /// <summary>
        /// Reads the values at specific times for a for all items in the trend.
        /// </summary>
        public TsCHdaItemValueCollection[] ReadAtTime()
        {
            return ReadAtTime(GetItems());
        }

        /// <summary>
        /// Reads the values at specific times for a for a set of items. 
        /// </summary>
        public TsCHdaItemValueCollection[] ReadAtTime(TsCHdaItem[] items)
        {
            var timestamps = new DateTime[Timestamps.Count];

            for (var ii = 0; ii < Timestamps.Count; ii++)
            {
                timestamps[ii] = Timestamps[ii];
            }

            return hdaServer_.ReadAtTime(timestamps, items);
        }

        /// <summary>
        /// Starts an asynchronous read values at specific times request for all items in the trend. 
        /// </summary>
        public OpcItemResult[] ReadAtTime(
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            return ReadAtTime(GetItems(), requestHandle, callback, out request);
        }

        /// <summary>
        /// Starts an asynchronous read values at specific times request for a set of items.
        /// </summary>
        public OpcItemResult[] ReadAtTime(
            TsCHdaItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            var timestamps = new DateTime[Timestamps.Count];

            for (var ii = 0; ii < Timestamps.Count; ii++)
            {
                timestamps[ii] = Timestamps[ii];
            }

            return hdaServer_.ReadAtTime(timestamps, items, requestHandle, callback, out request);
        }
        #endregion

        #region ReadAttributes
        /// <summary>
        /// Reads the attributes at specific times for a for an item. 
        /// </summary>
        public TsCHdaItemAttributeCollection ReadAttributes(OpcItem item, int[] attributeIDs)
        {
            return hdaServer_.ReadAttributes(StartTime, EndTime, item, attributeIDs);
        }

        /// <summary>
        /// Starts an asynchronous read attributes at specific times request for an item. 
        /// </summary>
        public TsCHdaResultCollection ReadAttributes(
            OpcItem item,
            int[] attributeIDs,
            object requestHandle,
            TsCHdaReadAttributesCompleteEventHandler callback,
            out IOpcRequest request)
        {
            var results = hdaServer_.ReadAttributes(
                StartTime,
                EndTime,
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
        /// Reads the annotations for a for all items in the trend.
        /// </summary>
        public TsCHdaAnnotationValueCollection[] ReadAnnotations()
        {
            return ReadAnnotations(GetItems());
        }

        /// <summary>
        /// Reads the annotations for a for a set of items. 
        /// </summary>
        public TsCHdaAnnotationValueCollection[] ReadAnnotations(TsCHdaItem[] items)
        {
            var results = hdaServer_.ReadAnnotations(
                StartTime,
                EndTime,
                items);

            return results;
        }

        /// <summary>
        /// Starts an asynchronous read annotations request for all items in the trend.
        /// </summary>
        public OpcItemResult[] ReadAnnotations(
            object requestHandle,
            TsCHdaReadAnnotationsCompleteEventHandler callback,
            out IOpcRequest request)
        {
            return ReadAnnotations(GetItems(), requestHandle, callback, out request);
        }

        /// <summary>
        /// Starts an asynchronous read annotations request for a set of items. 
        /// </summary>
        public OpcItemResult[] ReadAnnotations(
            TsCHdaItem[] items,
            object requestHandle,
            TsCHdaReadAnnotationsCompleteEventHandler callback,
            out IOpcRequest request)
        {
            var results = hdaServer_.ReadAnnotations(
                StartTime,
                EndTime,
                items,
                requestHandle,
                callback,
                out request);

            return results;
        }
        #endregion

        #region Delete
        /// <summary>
        /// Deletes the raw values for a for all items in the trend.
        /// </summary>
        public OpcItemResult[] Delete()
        {
            return Delete(GetItems());
        }

        /// <summary>
        /// Deletes the raw values for a for a set of items. 
        /// </summary>
        public OpcItemResult[] Delete(TsCHdaItem[] items)
        {
            var results = hdaServer_.Delete(
                StartTime,
                EndTime,
                items);

            return results;
        }

        /// <summary>
        /// Starts an asynchronous delete raw request for all items in the trend. 
        /// </summary>
        public OpcItemResult[] Delete(
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request)
        {
            return Delete(GetItems(), requestHandle, callback, out request);
        }

        /// <summary>
        /// Starts an asynchronous delete raw request for a set of items. 
        /// </summary>
        public OpcItemResult[] Delete(
            OpcItem[] items,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request)
        {
            var results = hdaServer_.Delete(
                StartTime,
                EndTime,
                items,
                requestHandle,
                callback,
                out request);

            return results;
        }
        #endregion

        #region DeleteAtTime
        /// <summary>
        /// Deletes the values at specific times for a for all items in the trend.
        /// </summary>
        public TsCHdaResultCollection[] DeleteAtTime()
        {
            return DeleteAtTime(GetItems());
        }

        /// <summary>
        /// Deletes the values at specific times for a for a set of items. 
        /// </summary>
        public TsCHdaResultCollection[] DeleteAtTime(TsCHdaItem[] items)
        {
            var times = new TsCHdaItemTimeCollection[items.Length];

            for (var ii = 0; ii < items.Length; ii++)
            {
                times[ii] = (TsCHdaItemTimeCollection)Timestamps.Clone();

                times[ii].ItemName = items[ii].ItemName;
                times[ii].ItemPath = items[ii].ItemPath;
                times[ii].ClientHandle = items[ii].ClientHandle;
                times[ii].ServerHandle = items[ii].ServerHandle;
            }

            return hdaServer_.DeleteAtTime(times);
        }

        /// <summary>
        /// Starts an asynchronous delete values at specific times request for all items in the trend. 
        /// </summary>
        public OpcItemResult[] DeleteAtTime(
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request)
        {
            return DeleteAtTime(GetItems(), requestHandle, callback, out request);
        }

        /// <summary>
        /// Starts an asynchronous delete values at specific times request for a set of items.
        /// </summary>
        public OpcItemResult[] DeleteAtTime(
            TsCHdaItem[] items,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request)
        {
            var times = new TsCHdaItemTimeCollection[items.Length];

            for (var ii = 0; ii < items.Length; ii++)
            {
                times[ii] = (TsCHdaItemTimeCollection)Timestamps.Clone();

                times[ii].ItemName = items[ii].ItemName;
                times[ii].ItemPath = items[ii].ItemPath;
                times[ii].ClientHandle = items[ii].ClientHandle;
                times[ii].ServerHandle = items[ii].ServerHandle;
            }

            return hdaServer_.DeleteAtTime(times, requestHandle, callback, out request);
        }
        #endregion
        #endregion

        #region ISerializable Members
        /// <summary>
        /// Serializes a server into a stream.
        /// </summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // serialize basic parameters.
            info.AddValue(Names.Name, Name);
            info.AddValue(Names.AggregateId, aggregate_);
            info.AddValue(Names.StartTime, StartTime);
            info.AddValue(Names.EndTime, EndTime);
            info.AddValue(Names.MaxValues, MaxValues);
            info.AddValue(Names.IncludeBounds, IncludeBounds);
            info.AddValue(Names.ResampleInterval, resampleInterval_);
            info.AddValue(Names.UpdateInterval, updateInterval_);
            info.AddValue(Names.PlaybackInterval, playbackInterval_);
            info.AddValue(Names.PlaybackDuration, playbackDuration_);

            // serialize timestamps.
            DateTime[] timestamps = null;

            if (timeStamps_.Count > 0)
            {
                timestamps = new DateTime[timeStamps_.Count];

                for (var ii = 0; ii < timestamps.Length; ii++)
                {
                    timestamps[ii] = timeStamps_[ii];
                }
            }

            info.AddValue(Names.Timestamps, timestamps);

            // serialize items.
            TsCHdaItem[] items = null;

            if (items_.Count > 0)
            {
                items = new TsCHdaItem[items_.Count];

                for (var ii = 0; ii < items.Length; ii++)
                {
                    items[ii] = items_[ii];
                }
            }

            info.AddValue(Names.Items, items);
        }

        /// <summary>
        /// Used to set the server after the object is deserialized.
        /// </summary>
        internal void SetServer(TsCHdaServer server)
        {
            hdaServer_ = server;
        }
        #endregion

        #region ICloneable Members
        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        public virtual object Clone()
        {
            // clone simple properties.
            var clone = (TsCHdaTrend)MemberwiseClone();

            // clone items.
            clone.items_ = new TsCHdaItemCollection();

            foreach (TsCHdaItem item in items_)
            {
                clone.items_.Add(item.Clone());
            }

            // clone timestamps.
            clone.timeStamps_ = new TsCHdaItemTimeCollection();

            foreach (DateTime timestamp in timeStamps_)
            {
                clone.timeStamps_.Add(timestamp);
            }

            // clear dynamic state information.
            clone.subscription_ = null;
            clone.playback_ = null;

            return clone;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Creates a copy of the items that have a valid aggregate set.
        /// </summary>
        private TsCHdaItem[] ApplyDefaultAggregate(TsCHdaItem[] items)
        {
            // use interpolative aggregate if none specified for the trend.
            var defaultId = Aggregate;

            if (defaultId == TsCHdaAggregateID.NoAggregate)
            {
                defaultId = TsCHdaAggregateID.Interpolative;
            }

            // apply default aggregate to items that have no aggregate specified.
            var localItems = new TsCHdaItem[items.Length];

            for (var ii = 0; ii < items.Length; ii++)
            {
                localItems[ii] = new TsCHdaItem(items[ii]);

                if (localItems[ii].Aggregate == TsCHdaAggregateID.NoAggregate)
                {
                    localItems[ii].Aggregate = defaultId;
                }
            }

            // return updated items.
            return localItems;
        }
        #endregion
    }
}
