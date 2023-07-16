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
    /// Defines functionality that is common to all OPC Historical Data Access servers.
    /// </summary>
    public interface ITsCHdaServer : IOpcServer
    {
        /// <summary>
        /// Returns the current server status.
        /// </summary>
        /// <returns>The current server status.</returns>
        OpcServerStatus GetServerStatus();

        /// <summary>
        /// Returns the item attributes supported by the server.
        /// </summary>
        /// <returns>The a set of item attributes and their descriptions.</returns>
        TsCHdaAttribute[] GetAttributes();

        /// <summary>
        /// Returns the aggregates supported by the server.
        /// </summary>
        /// <returns>The a set of aggregates and their descriptions.</returns>
        TsCHdaAggregate[] GetAggregates();

        /// <summary>
        /// Creates a object used to browse the server address space.
        /// </summary>
        /// <param name="filters">The set of attribute filters to use when browsing.</param>
        /// <param name="results">A result code for each individual filter.</param>
        /// <returns>A browser object that must be released by calling Dispose().</returns>
        ITsCHdaBrowser CreateBrowser(TsCHdaBrowseFilter[] filters, out OpcResult[] results);

        /// <summary>
        /// Creates a set of items.
        /// </summary>
        /// <param name="items">The identifiers for the items to create.</param>
        /// <returns>The results for each item containing the server handle and result code.</returns>
        OpcItemResult[] CreateItems(OpcItem[] items);

        /// <summary>
        /// Releases a set of previously created items.
        /// </summary>
        /// <param name="items">The server handles for the items to release.</param>
        /// <returns>The results for each item containing the result code.</returns>
        OpcItemResult[] ReleaseItems(OpcItem[] items);

        /// <summary>
        /// Validates a set of items.
        /// </summary>
        /// <param name="items">The identifiers for the items to validate.</param>
        /// <returns>The results for each item containing the result code.</returns>
        OpcItemResult[] ValidateItems(OpcItem[] items);

        /// <summary>
        /// Reads raw (unprocessed) data from the historian database for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="maxValues">The number of values to be read for each item.</param>
        /// <param name="includeBounds">Whether the bounding item values should be returned.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <returns>A set of values, qualities and timestamps within the requested time range for each item.</returns>
        TsCHdaItemValueCollection[] ReadRaw(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            int maxValues,
            bool includeBounds,
            OpcItem[] items);

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
        OpcItemResult[] ReadRaw(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            int maxValues,
            bool includeBounds,
            OpcItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request);

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
        OpcItemResult[] AdviseRaw(
            TsCHdaTime startTime,
            decimal updateInterval,
            OpcItem[] items,
            object requestHandle,
            TsCHdaDataUpdateEventHandler callback,
            out IOpcRequest request);

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
        OpcItemResult[] PlaybackRaw(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            int maxValues,
            decimal updateInterval,
            decimal playbackDuration,
            OpcItem[] items,
            object requestHandle,
            TsCHdaDataUpdateEventHandler callback,
            out IOpcRequest request);

        /// <summary>
        /// Reads processed data from the historian database for a set of items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="resampleInterval">The interval between returned values.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <returns>A set of values, qualities and timestamps within the requested time range for each item.</returns>
        TsCHdaItemValueCollection[] ReadProcessed(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            decimal resampleInterval,
            TsCHdaItem[] items);

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
        OpcItemResult[] ReadProcessed(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            decimal resampleInterval,
            TsCHdaItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request);

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
        OpcItemResult[] AdviseProcessed(
            TsCHdaTime startTime,
            decimal resampleInterval,
            int numberOfIntervals,
            TsCHdaItem[] items,
            object requestHandle,
            TsCHdaDataUpdateEventHandler callback,
            out IOpcRequest request);

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
        OpcItemResult[] PlaybackProcessed(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            decimal resampleInterval,
            int numberOfIntervals,
            decimal updateInterval,
            TsCHdaItem[] items,
            object requestHandle,
            TsCHdaDataUpdateEventHandler callback,
            out IOpcRequest request);

        /// <summary>
        /// Reads data from the historian database for a set of items at specific times.
        /// </summary>
        /// <param name="timestamps">The set of timestamps to use when reading items values.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <returns>A set of values, qualities and timestamps within the requested time range for each item.</returns>
        TsCHdaItemValueCollection[] ReadAtTime(DateTime[] timestamps, OpcItem[] items);

        /// <summary>
        /// Sends an asynchronous request to read item values at specific times.
        /// </summary>
        /// <param name="timestamps">The set of timestamps to use when reading items values.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        OpcItemResult[] ReadAtTime(
            DateTime[] timestamps,
            OpcItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request);


        /// <summary>
        /// Reads item values that have been deleted or replaced.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="maxValues">The number of values to be read for each item.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <returns>A set of values, qualities and timestamps within the requested time range for each item.</returns>
        TsCHdaModifiedValueCollection[] ReadModified(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            int maxValues,
            OpcItem[] items);

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
        OpcItemResult[] ReadModified(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            int maxValues,
            OpcItem[] items,
            object requestHandle,
            TsCHdaReadValuesCompleteEventHandler callback,
            out IOpcRequest request);

        /// <summary>
        /// Reads the current or historical values for the attributes of an item.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="item">The item to read (must include the item name).</param>
        /// <param name="attributeIDs">The attributes to read.</param>
        /// <returns>A set of attribute values for each requested attribute.</returns>
        TsCHdaItemAttributeCollection ReadAttributes(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem item,
            int[] attributeIDs);

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
        TsCHdaResultCollection ReadAttributes(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem item,
            int[] attributeIDs,
            object requestHandle,
            TsCHdaReadAttributesCompleteEventHandler callback,
            out IOpcRequest request);

        /// <summary>
        /// Reads any annotations for an item within the a time interval.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to read.</param>
        /// <param name="endTime">The end of the history period to be read.</param>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <returns>A set of annotations within the requested time range for each item.</returns>
        TsCHdaAnnotationValueCollection[] ReadAnnotations(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem[] items);

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
        OpcItemResult[] ReadAnnotations(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem[] items,
            object requestHandle,
            TsCHdaReadAnnotationsCompleteEventHandler callback,
            out IOpcRequest request);

        /// <summary>
        /// Inserts annotations for one or more items.
        /// </summary>
        /// <param name="items">A list of annotations to add for each item (must include the item name).</param>
        /// <returns>The results of the insert operation for each annotation set.</returns>
        TsCHdaResultCollection[] InsertAnnotations(TsCHdaAnnotationValueCollection[] items);

        /// <summary>
        /// Sends an asynchronous request to inserts annotations for one or more items.
        /// </summary>
        /// <param name="items">A list of annotations to add for each item (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        OpcItemResult[] InsertAnnotations(
            TsCHdaAnnotationValueCollection[] items,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request);

        /// <summary>
        /// Inserts the values into the history database for one or more items. 
        /// </summary>
        /// <param name="items">The set of values to insert.</param>
        /// <param name="replace">Whether existing values should be replaced.</param>
        /// <returns></returns>
        TsCHdaResultCollection[] Insert(TsCHdaItemValueCollection[] items, bool replace);

        /// <summary>
        /// Sends an asynchronous request to inserts values for one or more items.
        /// </summary>
        /// <param name="items">The set of values to insert.</param>
        /// <param name="replace">Whether existing values should be replaced.</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        OpcItemResult[] Insert(
            TsCHdaItemValueCollection[] items,
            bool replace,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request);

        /// <summary>
        /// Replace the values into the history database for one or more items. 
        /// </summary>
        /// <param name="items">The set of values to replace.</param>
        /// <returns></returns>
        TsCHdaResultCollection[] Replace(TsCHdaItemValueCollection[] items);

        /// <summary>
        /// Sends an asynchronous request to replace values for one or more items.
        /// </summary>
        /// <param name="items">The set of values to replace.</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        OpcItemResult[] Replace(
            TsCHdaItemValueCollection[] items,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request);

        /// <summary>
        /// Deletes the values with the specified time domain for one or more items.
        /// </summary>
        /// <param name="startTime">The beginning of the history period to delete.</param>
        /// <param name="endTime">The end of the history period to be delete.</param>
        /// <param name="items">The set of items to delete (must include the item name).</param>
        /// <returns>The results of the delete operation for each item.</returns>
        OpcItemResult[] Delete(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem[] items);

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
        OpcItemResult[] Delete(
            TsCHdaTime startTime,
            TsCHdaTime endTime,
            OpcItem[] items,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request);

        /// <summary>
        /// Deletes the values at the specified times for one or more items. 
        /// </summary>
        /// <param name="items">The set of timestamps to delete for one or more items.</param>
        /// <returns>The results of the operation for each timestamp.</returns>
        TsCHdaResultCollection[] DeleteAtTime(TsCHdaItemTimeCollection[] items);

        /// <summary>
        /// Sends an asynchronous request to delete values for one or more items at a specified times.
        /// </summary>
        /// <param name="items">The set of timestamps to delete for one or more items.</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        OpcItemResult[] DeleteAtTime(
            TsCHdaItemTimeCollection[] items,
            object requestHandle,
            TsCHdaUpdateCompleteEventHandler callback,
            out IOpcRequest request);

        /// <summary>
        /// Cancels an asynchronous request.
        /// </summary>
        /// <param name="request">The state object for the request to cancel.</param>
        void CancelRequest(IOpcRequest request);

        /// <summary>
        /// Cancels an asynchronous request.
        /// </summary>
        /// <param name="request">The state object for the request to cancel.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        void CancelRequest(IOpcRequest request, TsCHdaCancelCompleteEventHandler callback);

    }

    #region Delegate Declarations

    /// <summary>
    /// Used to receive notifications when an exception occurs while processing a callback.
    /// </summary>
    /// <param name="request">An identifier for the request assigned by the caller.</param>
    /// <param name="exception">Exception which occured.</param>
    public delegate void TsCHdaCallbackExceptionEventHandler(IOpcRequest request, Exception exception);

    /// <summary>
    /// Used to receive data update notifications.
    /// </summary>
    /// <param name="request">An identifier for the request assigned by the caller.</param>
    /// <param name="results">A collection of results.</param>
    public delegate void TsCHdaDataUpdateEventHandler(IOpcRequest request, TsCHdaItemValueCollection[] results);

    /// <summary>
    /// Used to receive notifications when a read values request completes.
    /// </summary>
    /// <param name="request">An identifier for the request assigned by the caller.</param>
    /// <param name="results">A collection of results.</param>
    public delegate void TsCHdaReadValuesCompleteEventHandler(IOpcRequest request, TsCHdaItemValueCollection[] results);

    /// <summary>
    /// Used to receive notifications when a read attributes request completes.
    /// </summary>
    /// <param name="request">An identifier for the request assigned by the caller.</param>
    /// <param name="results">A collection of results.</param>
    public delegate void TsCHdaReadAttributesCompleteEventHandler(IOpcRequest request, TsCHdaItemAttributeCollection results);

    /// <summary>
    /// Used to receive notifications when a read annotations request completes.
    /// </summary>
    /// <param name="request">An identifier for the request assigned by the caller.</param>
    /// <param name="results">A collection of results.</param>
    public delegate void TsCHdaReadAnnotationsCompleteEventHandler(IOpcRequest request, TsCHdaAnnotationValueCollection[] results);

    /// <summary>
    /// Used to receive notifications when an update request completes.
    /// </summary>
    /// <param name="request">An identifier for the request assigned by the caller.</param>
    /// <param name="results">A collection of results.</param>
    public delegate void TsCHdaUpdateCompleteEventHandler(IOpcRequest request, TsCHdaResultCollection[] results);

    /// <summary>
    /// Used to receive notifications when a request is cancelled.
    /// </summary>
    /// <param name="request">An identifier for the request assigned by the caller.</param>
    public delegate void TsCHdaCancelCompleteEventHandler(IOpcRequest request);

    #endregion

}
