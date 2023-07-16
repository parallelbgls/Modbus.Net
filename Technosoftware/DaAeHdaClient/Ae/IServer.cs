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

namespace Technosoftware.DaAeHdaClient.Ae
{
    /// <summary>
    /// Defines functionality that is common to all OPC Alarms and Events servers.
    /// </summary>
    public interface ITsCAeServer : IOpcServer
    {
        /// <summary>
        /// Returns the current server status.
        /// </summary>
        /// <returns>The current server status.</returns>
        OpcServerStatus GetServerStatus();

        /// <summary>
        /// Creates a new event subscription.
        /// </summary>
        /// <param name="state">The initial state for the subscription.</param>
        /// <returns>The new subscription object.</returns>
        ITsCAeSubscription CreateSubscription(TsCAeSubscriptionState state);

        /// <summary>
        /// Returns the event filters supported by the server.
        /// </summary>
        /// <returns>A bit mask of all event filters supported by the server.</returns>
        int QueryAvailableFilters();

        /// <summary>       
        /// Returns the event categories supported by the server for the specified event types.
        /// </summary>
        /// <param name="eventType">A bit mask for the event types of interest.</param>
        /// <returns>A collection of event categories.</returns>
        TsCAeCategory[] QueryEventCategories(int eventType);

        /// <summary>
        /// Returns the condition names supported by the server for the specified event categories.
        /// </summary>
        /// <param name="eventCategory">A bit mask for the event categories of interest.</param>
        /// <returns>A list of condition names.</returns>
        string[] QueryConditionNames(int eventCategory);

        /// <summary>
        /// Returns the sub-condition names supported by the server for the specified event condition.
        /// </summary>
        /// <param name="conditionName">The name of the condition.</param>
        /// <returns>A list of sub-condition names.</returns>
        string[] QuerySubConditionNames(string conditionName);

        /// <summary>
        /// Returns the condition names supported by the server for the specified event source.
        /// </summary>
        /// <param name="sourceName">The name of the event source.</param>
        /// <returns>A list of condition names.</returns>
        string[] QueryConditionNames(string sourceName);

        /// <summary>       
        /// Returns the event attributes supported by the server for the specified event categories.
        /// </summary>
        /// <param name="eventCategory">The event category of interest.</param>
        /// <returns>A collection of event attributes.</returns>
        TsCAeAttribute[] QueryEventAttributes(int eventCategory);

        /// <summary>
        /// Returns the DA item ids for a set of attribute ids belonging to events which meet the specified filter criteria.
        /// </summary>
        /// <param name="sourceName">The event source of interest.</param>
        /// <param name="eventCategory">The id of the event category for the events of interest.</param>
        /// <param name="conditionName">The name of a condition within the event category.</param>
        /// <param name="subConditionName">The name of a sub-condition within a multi-state condition.</param>
        /// <param name="attributeIDs">The ids of the attributes to return item ids for.</param>
        /// <returns>A list of item urls for each specified attribute.</returns>
        TsCAeItemUrl[] TranslateToItemIDs(
          string sourceName,
          int eventCategory,
          string conditionName,
          string subConditionName,
          int[] attributeIDs);

        /// <summary>
        /// Returns the current state information for the condition instance corresponding to the source and condition name.
        /// </summary>
        /// <param name="sourceName">The source name</param>
        /// <param name="conditionName">A condition name for the source.</param>
        /// <param name="attributeIDs">The list of attributes to return with the condition state.</param>
        /// <returns>The current state of the connection.</returns>
        TsCAeCondition GetConditionState(
            string sourceName,
            string conditionName,
            int[] attributeIDs);

        /// <summary>
        /// Places the specified process areas into the enabled state.
        /// </summary>
        /// <param name="areas">A list of fully qualified area names.</param>
        /// <returns>The results of the operation for each area.</returns>
        OpcResult[] EnableConditionByArea(string[] areas);

        /// <summary>
        /// Places the specified process areas into the disabled state.
        /// </summary>
        /// <param name="areas">A list of fully qualified area names.</param>
        /// <returns>The results of the operation for each area.</returns>
        OpcResult[] DisableConditionByArea(string[] areas);

        /// <summary>
        /// Places the specified process areas into the enabled state.
        /// </summary>
        /// <param name="sources">A list of fully qualified source names.</param>
        /// <returns>The results of the operation for each area.</returns>
        OpcResult[] EnableConditionBySource(string[] sources);

        /// <summary>
        /// Places the specified process areas into the disabled state.
        /// </summary>
        /// <param name="sources">A list of fully qualified source names.</param>
        /// <returns>The results of the operation for each area.</returns>
        OpcResult[] DisableConditionBySource(string[] sources);

        /// <summary>
        /// Returns the enabled state for the specified process areas. 
        /// </summary>
        /// <param name="areas">A list of fully qualified area names.</param>
        TsCAeEnabledStateResult[] GetEnableStateByArea(string[] areas);

        /// <summary>
        /// Returns the enabled state for the specified event sources. 
        /// </summary>
        /// <param name="sources">A list of fully qualified source names.</param>
        TsCAeEnabledStateResult[] GetEnableStateBySource(string[] sources);

        /// <summary>
        /// Used to acknowledge one or more conditions in the event server.
        /// </summary>
        /// <param name="acknowledgerID">The identifier for who is acknowledging the condition.</param>
        /// <param name="comment">A comment associated with the acknowledgment.</param>
        /// <param name="conditions">The conditions being acknowledged.</param>
        /// <returns>A list of result id indictaing whether each condition was successfully acknowledged.</returns>
        OpcResult[] AcknowledgeCondition(
            string acknowledgerID,
            string comment,
            TsCAeEventAcknowledgement[] conditions);

        /// <summary>
        /// Browses for all children of the specified area that meet the filter criteria.
        /// </summary>
        /// <param name="areaID">The full-qualified id for the area.</param>
        /// <param name="browseType">The type of children to return.</param>
        /// <param name="browseFilter">The expression used to filter the names of children returned.</param>
        /// <returns>The set of elements that meet the filter criteria.</returns>
        TsCAeBrowseElement[] Browse(
            string areaID,
            TsCAeBrowseType browseType,
            string browseFilter);

        /// <summary>
        /// Browses for all children of the specified area that meet the filter criteria.
        /// </summary>
        /// <param name="areaID">The full-qualified id for the area.</param>
        /// <param name="browseType">The type of children to return.</param>
        /// <param name="browseFilter">The expression used to filter the names of children returned.</param>
        /// <param name="maxElements">The maximum number of elements to return.</param>
        /// <param name="position">The object used to continue the browse if the number nodes exceeds the maximum specified.</param>
        /// <returns>The set of elements that meet the filter criteria.</returns>
        TsCAeBrowseElement[] Browse(
            string areaID,
            TsCAeBrowseType browseType,
            string browseFilter,
            int maxElements,
            out IOpcBrowsePosition position);

        /// <summary>
        /// Continues browsing the server's address space at the specified position.
        /// </summary>
        /// <param name="maxElements">The maximum number of elements to return.</param>
        /// <param name="position">The position object used to continue a browse operation.</param>
        /// <returns>The set of elements that meet the filter criteria.</returns>
        TsCAeBrowseElement[] BrowseNext(int maxElements, ref IOpcBrowsePosition position);
    }
}
