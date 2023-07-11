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
#endregion

namespace Technosoftware.DaAeHdaClient.Da
{
	/// <summary>
	/// Defines functionality that is common to all OPC Data Access servers.
	/// </summary>
	public interface ITsDaServer : IOpcServer
	{
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

        /// <summary>
        /// Returns the current server status.
        /// </summary>
        /// <returns>The current server status.</returns>
        OpcServerStatus GetServerStatus();

		/// <summary>
		/// Reads the current values for a set of items. 
		/// </summary>
		/// <param name="items">The set of items to read.</param>
		/// <returns>The results of the read operation for each item.</returns>
		TsCDaItemValueResult[] Read(TsCDaItem[] items);

		/// <summary>
		/// Writes the value, quality and timestamp for a set of items.
		/// </summary>
		/// <param name="values">The set of item values to write.</param>
		/// <returns>The results of the write operation for each item.</returns>
		OpcItemResult[] Write(TsCDaItemValue[] values);

		/// <summary>
		/// Creates a new subscription.
		/// </summary>
		/// <param name="state">The initial state of the subscription.</param>
		/// <returns>The new subscription object.</returns>
		ITsCDaSubscription CreateSubscription(TsCDaSubscriptionState state);

		/// <summary>
		/// Cancels a subscription and releases all resources allocated for it.
		/// </summary>
		/// <param name="subscription">The subscription to cancel.</param>
		void CancelSubscription(ITsCDaSubscription subscription);

		/// <summary>
		/// Fetches the children of a branch that meet the filter criteria.
		/// </summary>
		/// <param name="itemId">The identifier of branch which is the target of the search.</param>
		/// <param name="filters">The filters to use to limit the set of child elements returned.</param>
		/// <param name="position">An object used to continue a browse that could not be completed.</param>
		/// <returns>The set of elements found.</returns>
		TsCDaBrowseElement[] Browse(
			OpcItem itemId,
			TsCDaBrowseFilters filters,
			out TsCDaBrowsePosition position);

		/// <summary>
		/// Continues a browse operation with previously specified search criteria.
		/// </summary>
		/// <param name="position">An object containing the browse operation state information.</param>
		/// <returns>The set of elements found.</returns>
		TsCDaBrowseElement[] BrowseNext(ref TsCDaBrowsePosition position);

		/// <summary>
		/// Returns the item properties for a set of items.
		/// </summary>
		/// <param name="itemIds">A list of item identifiers.</param>
		/// <param name="propertyIDs">A list of properties to fetch for each item.</param>
		/// <param name="returnValues">Whether the property values should be returned with the properties.</param>
		/// <returns>A list of properties for each item.</returns>
		TsCDaItemPropertyCollection[] GetProperties(
			OpcItem[] itemIds,
			TsDaPropertyID[] propertyIDs,
			bool returnValues);
	}
}
