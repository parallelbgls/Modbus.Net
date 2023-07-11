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
	/// A subscription for a set of items on a single OPC server.
	/// </summary>
	public interface ITsCDaSubscription : IDisposable
	{
		#region Events
        /// <summary>
        /// An event to receive data change updates.
        /// </summary>
        event TsCDaDataChangedEventHandler DataChangedEvent;
        #endregion

		#region Result Filters
        /// <summary>
		/// Returns the filters applied by the server to any item results returned to the client.
		/// </summary>
		/// <returns>A bit mask indicating which fields should be returned in any item results.</returns>
		int GetResultFilters();

		/// <summary>
		/// Sets the filters applied by the server to any item results returned to the client.
		/// </summary>
		/// <param name="filters">A bit mask indicating which fields should be returned in any item results.</param>
		void SetResultFilters(int filters);
        #endregion

		#region State Management
        /// <summary>
		/// Returns the current state of the subscription.
		/// </summary>
		/// <returns>The current state of the subscription.</returns>
		TsCDaSubscriptionState GetState();

		/// <summary>
		/// Changes the state of a subscription.
		/// </summary>
		/// <param name="masks">A bit mask that indicates which elements of the subscription state are changing.</param>
		/// <param name="state">The new subscription state.</param>
		/// <returns>The actual subscription state after applying the changes.</returns>
		TsCDaSubscriptionState ModifyState(int masks, TsCDaSubscriptionState state);
        #endregion

		#region Item Management
        /// <summary>
		/// Adds items to the subscription.
		/// </summary>
		/// <param name="items">The set of items to add to the subscription.</param>
		/// <returns>The results of the add item operation for each item.</returns>
		TsCDaItemResult[] AddItems(TsCDaItem[] items);

		/// <summary>
		/// Modifies the state of items in the subscription
		/// </summary>
		/// <param name="masks">Specifies which item state parameters are being modified.</param>
		/// <param name="items">The new state for each item.</param>
		/// <returns>The results of the modify item operation for each item.</returns>
		TsCDaItemResult[] ModifyItems(int masks, TsCDaItem[] items);

		/// <summary>
		/// Removes items from the subscription.
		/// </summary>
		/// <param name="items">The identifiers (i.e. server handles) for the items being removed.</param>
		/// <returns>The results of the remove item operation for each item.</returns>
		OpcItemResult[] RemoveItems(OpcItem[] items);
        #endregion

		#region Synchronous I/O
        /// <summary>
		/// Reads the values for a set of items in the subscription.
		/// </summary>
		/// <param name="items">The identifiers (i.e. server handles) for the items being read.</param>
		/// <returns>The value for each of items.</returns>
		TsCDaItemValueResult[] Read(TsCDaItem[] items);

		/// <summary>
		/// Writes the value, quality and timestamp for a set of items in the subscription.
		/// </summary>
		/// <param name="items">The item values to write.</param>
		/// <returns>The results of the write operation for each item.</returns>
		OpcItemResult[] Write(TsCDaItemValue[] items);
        #endregion

		#region Asynchronous I/O
        /// <summary>
        /// Begins an asynchronous read operation for a set of items.
        /// </summary>
        /// <param name="items">The set of items to read (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        OpcItemResult[] Read(
            TsCDaItem[] items,
            object requestHandle,
            TsCDaReadCompleteEventHandler callback,
            out IOpcRequest request);

        /// <summary>
        /// Begins an asynchronous write operation for a set of items.
        /// </summary>
        /// <param name="items">The set of item values to write (must include the item name).</param>
        /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
        /// <param name="callback">A delegate used to receive notifications when the request completes.</param>
        /// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
        /// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
        OpcItemResult[] Write(
            TsCDaItemValue[] items,
            object requestHandle,
            TsCDaWriteCompleteEventHandler callback,
            out IOpcRequest request);

        /// <summary>
        /// Cancels an asynchronous read or write operation.
        /// </summary>
        /// <param name="request">The object returned from the BeginRead or BeginWrite request.</param>
        /// <param name="callback">The function to invoke when the cancel completes.</param>
        void Cancel(IOpcRequest request, TsCDaCancelCompleteEventHandler callback);

		/// <summary>
		/// Causes the server to send a data changed notification for all active items. 
		/// </summary>
		void Refresh();

		/// <summary>
		/// Causes the server to send a data changed notification for all active items. 
		/// </summary>
		/// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
		/// <param name="request">An object that contains the state of the request (used to cancel the request).</param>
		/// <returns>A set of results containing any errors encountered when the server validated the items.</returns>
		void Refresh(
			object requestHandle,
			out IOpcRequest request);

		/// <summary>
		/// Enables or disables data change notifications from the server.
		/// </summary>
		/// <param name="enabled">Whether data change notifications are enabled.</param>
		void SetEnabled(bool enabled);

		/// <summary>
		/// Checks whether data change notifications from the server are enabled.
		/// </summary>
		/// <returns>Whether data change notifications are enabled.</returns>
		bool GetEnabled();
        #endregion
	}

	#region Delegate Declarations
    /// <summary>
    /// A delegate to receive data change updates from the server.
    /// </summary>
    /// <param name="subscriptionHandle">
    /// A unique identifier for the subscription assigned by the client. If the parameter
    /// <see cref="TsCDaSubscriptionState.ClientHandle">ClientHandle</see> is not defined this
    /// parameter is empty.
    /// </param>
    /// <param name="requestHandle">
    /// An identifier for the request assigned by the caller. This parameter is empty if
    /// the corresponding parameter in the calls Read(), Write() or Refresh() is not    defined.
    /// Can be used to Cancel an outstanding operation.
    /// </param>
    /// <param name="values">
    /// <para class="MsoBodyText" style="MARGIN: 1pt 0in">The set of changed values.</para>
    /// <para class="MsoBodyText" style="MARGIN: 1pt 0in">Each value will always have
    /// item’s ClientHandle field specified.</para>
    /// </param>
    public delegate void TsCDaDataChangedEventHandler(object subscriptionHandle, object requestHandle, TsCDaItemValueResult[] values);

    /// <summary>
    /// A delegate to receive asynchronous read completed notifications.
    /// </summary>
    /// <param name="requestHandle">
    /// An identifier for the request assigned by the caller. This parameter    is empty if
    /// the corresponding parameter in the calls    Read(), Write() or Refresh() is not defined.
    /// Can be used to Cancel an    outstanding operation.
    /// </param>
    /// <param name="results">The results of    the last asynchronous read operation.</param>
    public delegate void TsCDaReadCompleteEventHandler(object requestHandle, TsCDaItemValueResult[] results);

    /// <summary>
    /// A delegate to receive asynchronous write    completed notifications.
    /// </summary>
    /// <param name="requestHandle">
    /// An identifier for the request assigned by the caller. This parameter is empty if
    /// the corresponding parameter in the calls Read(), Write() or Refresh() is not defined.
    /// Can be used to Cancel an outstanding operation.
    /// </param>
    /// <param name="results">The results of the last asynchronous write operation.</param>
    public delegate void TsCDaWriteCompleteEventHandler(object requestHandle, OpcItemResult[] results);

    /// <summary>
    /// A delegate to receive asynchronous cancel completed notifications.
    /// </summary>
    /// <param name="requestHandle">An identifier for the request assigned by the caller.</param>
    public delegate void TsCDaCancelCompleteEventHandler(object requestHandle);
    #endregion
}
