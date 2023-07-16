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

namespace Technosoftware.DaAeHdaClient.Ae
{
    /// <summary>
    /// An interface to an object which implements a AE event subscription.
    /// </summary>
    public interface ITsCAeSubscription : IDisposable
    {
        #region Events
        /// <summary>
        /// An event to receive event change updates.
        /// </summary>
        event TsCAeDataChangedEventHandler DataChangedEvent;
        #endregion

        #region State Management
        /// <summary>
        /// Returns the current state of the subscription.
        /// </summary>
        /// <returns>The current state of the subscription.</returns>
        TsCAeSubscriptionState GetState();

        /// <summary>
        /// Changes the state of a subscription.
        /// </summary>
        /// <param name="masks">A bit mask that indicates which elements of the subscription state are changing.</param>
        /// <param name="state">The new subscription state.</param>
        /// <returns>The actual subscription state after applying the changes.</returns>
        TsCAeSubscriptionState ModifyState(int masks, TsCAeSubscriptionState state);
        #endregion

        #region Filter Management
        /// <summary>
        /// Returns the current filters for the subscription.
        /// </summary>
        /// <returns>The current filters for the subscription.</returns>
        TsCAeSubscriptionFilters GetFilters();

        /// <summary>
        /// Sets the current filters for the subscription.
        /// </summary>
        /// <param name="filters">The new filters to use for the subscription.</param>
        void SetFilters(TsCAeSubscriptionFilters filters);
        #endregion

        #region Attribute Management
        /// <summary>
        /// Returns the set of attributes to return with event notifications.
        /// </summary>      
        /// <param name="eventCategory">The specific event category for which the attributes apply.</param>
        /// <returns>The set of attribute ids which returned with event notifications.</returns>
        int[] GetReturnedAttributes(int eventCategory);

        /// <summary>
        /// Selects the set of attributes to return with event notifications.
        /// </summary>
        /// <param name="eventCategory">The specific event category for which the attributes apply.</param>
        /// <param name="attributeIDs">The list of attribute ids to return.</param>
        void SelectReturnedAttributes(int eventCategory, int[] attributeIDs);
        #endregion

        #region Refresh
        /// <summary>
        /// Force a refresh for all active conditions and inactive, unacknowledged conditions whose event notifications match the filter of the event subscription.
        /// </summary>
        void Refresh();

        /// <summary>
        /// Cancels an outstanding refresh request.
        /// </summary>
        void CancelRefresh();
        #endregion
    }

    #region Delegate Declarations
    /// <summary>
    /// A delegate to receive data change updates from the server.
    /// </summary>
    /// <param name="notifications">The notifications sent by the server when a event change occurs.</param>
    /// <param name="refresh">TRUE if this is a subscription refresh</param>
    /// <param name="lastRefresh">TRUE if this is the last subscription refresh in response to a specific invocation of the Refresh method.</param>
    public delegate void TsCAeDataChangedEventHandler(TsCAeEventNotification[] notifications, bool refresh, bool lastRefresh);
    #endregion 
}
