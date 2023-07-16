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
    /// Defines identifiers for well-known properties.
    /// </summary>
    public class TsDaProperty
    {
        #region Data Access Properties

        /// <summary><para>Item Canonical DataType</para></summary>
        public static readonly TsDaPropertyID DATATYPE = new TsDaPropertyID("dataType", 1, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary><para>Item Value</para></summary>
        /// <remarks>
        /// Note the type of value returned is as indicated by the "Item Canonical DataType"
        /// and depends on the item. This will behave like a read from DEVICE.
        /// </remarks>
        public static readonly TsDaPropertyID VALUE = new TsDaPropertyID("value", 2, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary><para>Item Quality</para></summary>
        /// <remarks>(OPCQUALITY stored in an I2). This will behave like a read from DEVICE.</remarks>
        public static readonly TsDaPropertyID QUALITY = new TsDaPropertyID("quality", 3, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary><para>Item Timestamp</para></summary>
        /// <remarks>
        /// (will be converted from FILETIME). This will behave like a read from
        /// DEVICE.
        /// </remarks>
        public static readonly TsDaPropertyID TIMESTAMP = new TsDaPropertyID("timestamp", 4, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary><para>Item Access Rights</para></summary>
        /// <remarks>(OPCACCESSRIGHTS stored in an I4)</remarks>
        public static readonly TsDaPropertyID ACCESSRIGHTS = new TsDaPropertyID("accessRights", 5, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary><para>Server Scan Rate</para></summary>
        /// <remarks>
        /// In Milliseconds. This represents the fastest rate at which the server could
        /// obtain data from the underlying data source. The nature of this source is not defined
        /// but is typically a DCS system, a SCADA system, a PLC via a COMM port or network, a
        /// Device Network, etc. This value generally represents the ‘best case’ fastest
        /// RequestedUpdateRate which could be used if this item were added to an OPCGroup.<br/>
        /// The accuracy of this value (the ability of the server to attain ‘best case’
        /// performance) can be greatly affected by system load and other factors.
        /// </remarks>
        public static readonly TsDaPropertyID SCANRATE = new TsDaPropertyID("scanRate", 6, OpcNamespace.OPC_DATA_ACCESS);

        /// <remarks>
        /// 	<para>Indicate the type of Engineering Units (EU) information (if any) contained in
        ///     EUINFO.</para>
        /// 	<list type="bullet">
        /// 		<item>
        ///             0 - No EU information available (EUINFO will be VT_EMPTY).
        ///         </item>
        /// 		<item>
        ///             1 - Analog - EUINFO will contain a SAFEARRAY of exactly two doubles
        ///             (VT_ARRAY | VT_R8) corresponding to the LOW and HI EU range.
        ///         </item>
        /// 		<item>2 - Enumerated - EUINFO will contain a SAFEARRAY of strings (VT_ARRAY |
        ///         VT_BSTR) which contains a list of strings (Example: “OPEN”, “CLOSE”, “IN
        ///         TRANSIT”, etc.) corresponding to sequential numeric values (0, 1, 2,
        ///         etc.)</item>
        /// 	</list>
        /// </remarks>
        /// <summary><para>Item EU Type</para></summary>
        public static readonly TsDaPropertyID EUTYPE = new TsDaPropertyID("euType", 7, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary><para>Item EUInfo</para></summary>
        /// <value>
        /// 	<para>
        ///         If EUTYPE is “Analog” EUINFO will contain a SAFEARRAY of exactly two doubles
        ///         (VT_ARRAY | VT_R8) corresponding to the LOW and HI EU range.
        ///     </para>
        /// 	<para>If EUTYPE is “Enumerated” - EUINFO will contain a SAFEARRAY of strings
        ///     (VT_ARRAY | VT_BSTR) which contains a list of strings (Example: “OPEN”, “CLOSE”,
        ///     “IN TRANSIT”, etc.) corresponding to sequential numeric values (0, 1, 2,
        ///     etc.)</para>
        /// </value>
        public static readonly TsDaPropertyID EUINFO = new TsDaPropertyID("euInfo", 8, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>EU Units</para>
        /// 	<para>e.g. "DEGC" or "GALLONS"</para>
        /// </summary>
        public static readonly TsDaPropertyID ENGINEERINGUINTS = new TsDaPropertyID("engineeringUnits", 100, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Item Description</para>
        /// 	<para>e.g. "Evaporator 6 Coolant Temp"</para>
        /// </summary>
        public static readonly TsDaPropertyID DESCRIPTION = new TsDaPropertyID("description", 101, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>High EU</para>
        /// 	<para>Present only for 'analog' data. This represents the highest value likely to
        ///     be obtained in normal operation and is intended for such use as automatically
        ///     scaling a bargraph display.</para>
        /// 	<para>e.g. 1400.0</para>
        /// </summary>
        public static readonly TsDaPropertyID HIGHEU = new TsDaPropertyID("highEU", 102, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Low EU</para>
        /// 	<para>Present only for 'analog' data. This represents the lowest value likely to be
        ///     obtained in normal operation and is intended for such use as automatically scaling
        ///     a bargraph display.</para>
        /// 	<para>e.g. -200.0</para>
        /// </summary>
        public static readonly TsDaPropertyID LOWEU = new TsDaPropertyID("lowEU", 103, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>High Instrument Range</para>
        /// 	<para>Present only for ‘analog’ data. This represents the highest value that can be
        ///     returned by the instrument.</para>
        /// 	<para>e.g. 9999.9</para>
        /// </summary>
        public static readonly TsDaPropertyID HIGHIR = new TsDaPropertyID("highIR", 104, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Low Instrument Range</para>
        /// 	<para>Present only for ‘analog’ data. This represents the lowest value that can be
        ///     returned by the instrument.</para>
        /// 	<para>e.g. -9999.9</para>
        /// </summary>
        public static readonly TsDaPropertyID LOWIR = new TsDaPropertyID("lowIR", 105, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Contact Close Label</para>
        /// 	<para>Present only for ‘discrete' data. This represents a string to be associated
        ///     with this contact when it is in the closed (non-zero) state</para>
        /// 	<para>e.g. "RUN", "CLOSE", "ENABLE", "SAFE" ,etc.</para>
        /// </summary>
        public static readonly TsDaPropertyID CLOSELABEL = new TsDaPropertyID("closeLabel", 106, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Contact Open Label</para>
        /// 	<para>Present only for ‘discrete' data. This represents a string to be associated
        ///     with this contact when it is in the open (zero) state</para>
        /// 	<para>e.g. "STOP", "OPEN", "DISABLE", "UNSAFE" ,etc.</para>
        /// </summary>
        public static readonly TsDaPropertyID OPENLABEL = new TsDaPropertyID("openLabel", 107, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Item Timezone</para>
        /// 	<para>The difference in minutes between the items UTC Timestamp and the local time
        ///     in which the item value was obtained.</para>
        /// </summary>
        /// <remarks>
        /// See the OPCGroup TimeBias property. Also see the WIN32 TIME_ZONE_INFORMATION
        /// structure.
        /// </remarks>
        public static readonly TsDaPropertyID TIMEZONE = new TsDaPropertyID("timeZone", 108, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Condition Status</para>
        /// 	<para>The current alarm or condition status associated with the Item<br/>
        ///     e.g. "NORMAL", "ACTIVE", "HI ALARM", etc</para>
        /// </summary>
        public static readonly TsDaPropertyID CONDITION_STATUS = new TsDaPropertyID("conditionStatus", 300, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Alarm Quick Help</para>
        /// 	<para>A short text string providing a brief set of instructions for the operator to
        ///     follow when this alarm occurs.</para>
        /// </summary>
        public static readonly TsDaPropertyID ALARM_QUICK_HELP = new TsDaPropertyID("alarmQuickHelp", 301, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Alarm Area List</para>
        /// 	<para>An array of stings indicating the plant or alarm areas which include this
        ///     ItemID.</para>
        /// </summary>
        public static readonly TsDaPropertyID ALARM_AREA_LIST = new TsDaPropertyID("alarmAreaList", 302, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Primary Alarm Area</para>
        /// 	<para>A string indicating the primary plant or alarm area including this
        ///     ItemID</para>
        /// </summary>
        public static readonly TsDaPropertyID PRIMARY_ALARM_AREA = new TsDaPropertyID("primaryAlarmArea", 303, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Condition Logic</para>
        /// 	<para>An arbitrary string describing the test being performed.</para>
        /// 	<para>e.g. "High Limit Exceeded" or "TAG.PV &gt;= TAG.HILIM"</para>
        /// </summary>
        public static readonly TsDaPropertyID CONDITION_LOGIC = new TsDaPropertyID("conditionLogic", 304, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Limit Exceeded</para>
        /// 	<para>For multistate alarms, the condition exceeded</para>
        /// 	<para>e.g. HIHI, HI, LO, LOLO</para>
        /// </summary>
        public static readonly TsDaPropertyID LIMIT_EXCEEDED = new TsDaPropertyID("limitExceeded", 305, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>Deadband</summary>
        public static readonly TsDaPropertyID DEADBAND = new TsDaPropertyID("deadband", 306, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>HiHi limit</summary>
        public static readonly TsDaPropertyID HIHI_LIMIT = new TsDaPropertyID("hihiLimit", 307, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>Hi Limit</summary>
        public static readonly TsDaPropertyID HI_LIMIT = new TsDaPropertyID("hiLimit", 308, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>Lo Limit</summary>
        public static readonly TsDaPropertyID LO_LIMIT = new TsDaPropertyID("loLimit", 309, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>LoLo Limit</summary>
        public static readonly TsDaPropertyID LOLO_LIMIT = new TsDaPropertyID("loloLimit", 310, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>Rate of Change Limit</summary>
        public static readonly TsDaPropertyID RATE_CHANGE_LIMIT = new TsDaPropertyID("rangeOfChangeLimit", 311, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>Deviation Limit</summary>
        public static readonly TsDaPropertyID DEVIATION_LIMIT = new TsDaPropertyID("deviationLimit", 312, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Sound File</para>
        /// 	<para>e.g. C:\MEDIA\FIC101.WAV, or .MID</para>
        /// </summary>
        public static readonly TsDaPropertyID SOUNDFILE = new TsDaPropertyID("soundFile", 313, OpcNamespace.OPC_DATA_ACCESS);
        #endregion

        #region Complex Data Properties
        /// <summary>
        /// 	<para>Type System ID</para>
        /// 	<para>Complex Data Property</para>
        /// </summary>
        public static readonly TsDaPropertyID TYPE_SYSTEM_ID = new TsDaPropertyID("typeSystemID", 600, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Dictionary ID</para>
        /// 	<para>Complex Data Property</para>
        /// </summary>
        public static readonly TsDaPropertyID DICTIONARY_ID = new TsDaPropertyID("dictionaryID", 601, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Type ID</para>
        /// 	<para>Complex Data Property</para>
        /// </summary>
        public static readonly TsDaPropertyID TYPE_ID = new TsDaPropertyID("typeID", 602, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Dictionary</para>
        /// 	<para>Complex Data Property</para>
        /// </summary>
        public static readonly TsDaPropertyID DICTIONARY = new TsDaPropertyID("dictionary", 603, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Type description</para>
        /// 	<para>Complex Data Property</para>
        /// </summary>
        public static readonly TsDaPropertyID TYPE_DESCRIPTION = new TsDaPropertyID("typeDescription", 604, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Consistency Window</para>
        /// 	<para>Complex Data Property</para>
        /// </summary>
        public static readonly TsDaPropertyID CONSISTENCY_WINDOW = new TsDaPropertyID("consistencyWindow", 605, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Write Behaviour</para>
        /// 	<para>Complex Data Property, defaults to “All or Nothing” if the complex data item
        ///     is writable. Not used for Read-Only items.</para>
        /// </summary>
        public static readonly TsDaPropertyID WRITE_BEHAVIOR = new TsDaPropertyID("writeBehavior", 606, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Unconverted Item ID</para>
        /// 	<para>Complex Data Property, the ID of the item that exposes the same complex data
        ///     value in its native format. This property is mandatory for items that implement
        ///     complex data type conversions.</para>
        /// </summary>
        public static readonly TsDaPropertyID UNCONVERTED_ITEM_ID = new TsDaPropertyID("unconvertedItemID", 607, OpcNamespace.OPC_DATA_ACCESS);


        /// <summary>
        /// 	<para>Unfiltered Item ID</para>
        /// 	<para>Complex Data Property, the ID the item that exposes the same complex data
        ///     value without any data filter or with the default query applied to it. It is
        ///     mandatory for items that implement complex data filters or queries.</para>
        /// </summary>
        public static readonly TsDaPropertyID UNFILTERED_ITEM_ID = new TsDaPropertyID("unfilteredItemID", 608, OpcNamespace.OPC_DATA_ACCESS);

        /// <summary>
        /// 	<para>Data Filter Value</para>
        /// 	<para>Complex Data Property, the value of the filter that is currently applied to
        ///     the item. It is mandatory for items that implement complex data filters or
        ///     queries.</para>
        /// </summary>
        public static readonly TsDaPropertyID DATA_FILTER_VALUE = new TsDaPropertyID("dataFilterValue", 609, OpcNamespace.OPC_DATA_ACCESS);
        #endregion

        #region XML Data Access Properties
        /// <remarks/>
        public static readonly TsDaPropertyID MINIMUM_VALUE = new TsDaPropertyID("minimumValue", 109, OpcNamespace.OPC_DATA_ACCESS);
        /// <remarks/>
        public static readonly TsDaPropertyID MAXIMUM_VALUE = new TsDaPropertyID("maximumValue", 110, OpcNamespace.OPC_DATA_ACCESS);
        /// <remarks/>
        public static readonly TsDaPropertyID VALUE_PRECISION = new TsDaPropertyID("valuePrecision", 111, OpcNamespace.OPC_DATA_ACCESS);
        #endregion

    }
}
