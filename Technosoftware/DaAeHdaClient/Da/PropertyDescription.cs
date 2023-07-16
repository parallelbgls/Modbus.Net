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
using System.Reflection;
#endregion

namespace Technosoftware.DaAeHdaClient.Da
{
    /// <summary>
    /// Describes an item property.
    /// </summary>
    [Serializable]
    public class TsDaPropertyDescription
    {
        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes the object with the specified values.
        /// </summary>
        public TsDaPropertyDescription(TsDaPropertyID id, Type type, string name)
        {
            ID = id;
            Type = type;
            Name = name;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The unique identifier for the property.
        /// </summary>
        public TsDaPropertyID ID { get; set; }

        /// <summary>
        /// The .NET data type for the property.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// The short description defined in the OPC specifications.
        /// </summary>
        public string Name { get; set; }
        #endregion

        #region Data Access Properties
        /// <remarks/>
        public static readonly TsDaPropertyDescription DATATYPE = new TsDaPropertyDescription(TsDaProperty.DATATYPE, typeof(Type), "Item Canonical DataType");
        /// <remarks/>
        public static readonly TsDaPropertyDescription VALUE = new TsDaPropertyDescription(TsDaProperty.VALUE, typeof(object), "Item Value");
        /// <remarks/>    
        public static readonly TsDaPropertyDescription QUALITY = new TsDaPropertyDescription(TsDaProperty.QUALITY, typeof(TsCDaQuality), "Item Quality");
        /// <remarks/>
        public static readonly TsDaPropertyDescription TIMESTAMP = new TsDaPropertyDescription(TsDaProperty.TIMESTAMP, typeof(DateTime), "Item Timestamp");
        /// <remarks/>
        public static readonly TsDaPropertyDescription ACCESSRIGHTS = new TsDaPropertyDescription(TsDaProperty.ACCESSRIGHTS, typeof(TsDaAccessRights), "Item Access Rights");
        /// <remarks/>
        public static readonly TsDaPropertyDescription SCANRATE = new TsDaPropertyDescription(TsDaProperty.SCANRATE, typeof(float), "Server Scan Rate");
        /// <remarks/>
        public static readonly TsDaPropertyDescription EUTYPE = new TsDaPropertyDescription(TsDaProperty.EUTYPE, typeof(TsDaEuType), "Item EU Type");
        /// <remarks/>
        public static readonly TsDaPropertyDescription EUINFO = new TsDaPropertyDescription(TsDaProperty.EUINFO, typeof(string[]), "Item EU Info");
        /// <remarks/>
        public static readonly TsDaPropertyDescription ENGINEERINGUINTS = new TsDaPropertyDescription(TsDaProperty.ENGINEERINGUINTS, typeof(string), "EU Units");
        /// <remarks/>
        public static readonly TsDaPropertyDescription DESCRIPTION = new TsDaPropertyDescription(TsDaProperty.DESCRIPTION, typeof(string), "Item Description");
        /// <remarks/>
        public static readonly TsDaPropertyDescription HIGHEU = new TsDaPropertyDescription(TsDaProperty.HIGHEU, typeof(double), "High EU");
        /// <remarks/>
        public static readonly TsDaPropertyDescription LOWEU = new TsDaPropertyDescription(TsDaProperty.LOWEU, typeof(double), "Low EU");
        /// <remarks/>
        public static readonly TsDaPropertyDescription HIGHIR = new TsDaPropertyDescription(TsDaProperty.HIGHIR, typeof(double), "High Instrument Range");
        /// <remarks/>
        public static readonly TsDaPropertyDescription LOWIR = new TsDaPropertyDescription(TsDaProperty.LOWIR, typeof(double), "Low Instrument Range");
        /// <remarks/>
        public static readonly TsDaPropertyDescription CLOSELABEL = new TsDaPropertyDescription(TsDaProperty.CLOSELABEL, typeof(string), "Contact Close Label");
        /// <remarks/>     
        public static readonly TsDaPropertyDescription OPENLABEL = new TsDaPropertyDescription(TsDaProperty.OPENLABEL, typeof(string), "Contact Open Label");
        /// <remarks/>
        public static readonly TsDaPropertyDescription TIMEZONE = new TsDaPropertyDescription(TsDaProperty.TIMEZONE, typeof(int), "Timezone");
        /// <remarks/>
        public static readonly TsDaPropertyDescription CONDITION_STATUS = new TsDaPropertyDescription(TsDaProperty.CONDITION_STATUS, typeof(string), "Condition Status");
        /// <remarks/>
        public static readonly TsDaPropertyDescription ALARM_QUICK_HELP = new TsDaPropertyDescription(TsDaProperty.ALARM_QUICK_HELP, typeof(string), "Alarm Quick Help");
        /// <remarks/>
        public static readonly TsDaPropertyDescription ALARM_AREA_LIST = new TsDaPropertyDescription(TsDaProperty.ALARM_AREA_LIST, typeof(string), "Alarm Area List");
        /// <remarks/>
        public static readonly TsDaPropertyDescription PRIMARY_ALARM_AREA = new TsDaPropertyDescription(TsDaProperty.PRIMARY_ALARM_AREA, typeof(string), "Primary Alarm Area");
        /// <remarks/>
        public static readonly TsDaPropertyDescription CONDITION_LOGIC = new TsDaPropertyDescription(TsDaProperty.CONDITION_LOGIC, typeof(string), "Condition Logic");
        /// <remarks/>
        public static readonly TsDaPropertyDescription LIMIT_EXCEEDED = new TsDaPropertyDescription(TsDaProperty.LIMIT_EXCEEDED, typeof(string), "Limit Exceeded");
        /// <remarks/>
        public static readonly TsDaPropertyDescription DEADBAND = new TsDaPropertyDescription(TsDaProperty.DEADBAND, typeof(double), "Deadband");
        /// <remarks/>
        public static readonly TsDaPropertyDescription HIHI_LIMIT = new TsDaPropertyDescription(TsDaProperty.HIHI_LIMIT, typeof(double), "HiHi Limit");
        /// <remarks/>
        public static readonly TsDaPropertyDescription HI_LIMIT = new TsDaPropertyDescription(TsDaProperty.HI_LIMIT, typeof(double), "Hi Limit");
        /// <remarks/>
        public static readonly TsDaPropertyDescription LO_LIMIT = new TsDaPropertyDescription(TsDaProperty.LO_LIMIT, typeof(double), "Lo Limit");
        /// <remarks/>
        public static readonly TsDaPropertyDescription LOLO_LIMIT = new TsDaPropertyDescription(TsDaProperty.LOLO_LIMIT, typeof(double), "LoLo Limit");
        /// <remarks/>
        public static readonly TsDaPropertyDescription RATE_CHANGE_LIMIT = new TsDaPropertyDescription(TsDaProperty.RATE_CHANGE_LIMIT, typeof(double), "Rate of Change Limit");
        /// <remarks/>
        public static readonly TsDaPropertyDescription DEVIATION_LIMIT = new TsDaPropertyDescription(TsDaProperty.DEVIATION_LIMIT, typeof(double), "Deviation Limit");
        /// <remarks/>
        public static readonly TsDaPropertyDescription SOUNDFILE = new TsDaPropertyDescription(TsDaProperty.SOUNDFILE, typeof(string), "Sound File");
        #endregion

        #region Complex Data Properties
        /// <remarks/>
        public static readonly TsDaPropertyDescription TYPE_SYSTEM_ID = new TsDaPropertyDescription(TsDaProperty.TYPE_SYSTEM_ID, typeof(string), "Type System ID");
        /// <remarks/>
        public static readonly TsDaPropertyDescription DICTIONARY_ID = new TsDaPropertyDescription(TsDaProperty.DICTIONARY_ID, typeof(string), "Dictionary ID");
        /// <remarks/>
        public static readonly TsDaPropertyDescription TYPE_ID = new TsDaPropertyDescription(TsDaProperty.TYPE_ID, typeof(string), "Type ID");
        /// <remarks/>
        public static readonly TsDaPropertyDescription DICTIONARY = new TsDaPropertyDescription(TsDaProperty.DICTIONARY, typeof(object), "Dictionary");
        /// <remarks/>
        public static readonly TsDaPropertyDescription TYPE_DESCRIPTION = new TsDaPropertyDescription(TsDaProperty.TYPE_DESCRIPTION, typeof(string), "Type Description");
        /// <remarks/>
        public static readonly TsDaPropertyDescription CONSISTENCY_WINDOW = new TsDaPropertyDescription(TsDaProperty.CONSISTENCY_WINDOW, typeof(string), "Consistency Window");
        /// <remarks/>
        public static readonly TsDaPropertyDescription WRITE_BEHAVIOR = new TsDaPropertyDescription(TsDaProperty.WRITE_BEHAVIOR, typeof(string), "Write Behavior");
        /// <remarks/>
        public static readonly TsDaPropertyDescription UNCONVERTED_ITEM_ID = new TsDaPropertyDescription(TsDaProperty.UNCONVERTED_ITEM_ID, typeof(string), "Unconverted Item ID");
        /// <remarks/>
        public static readonly TsDaPropertyDescription UNFILTERED_ITEM_ID = new TsDaPropertyDescription(TsDaProperty.UNFILTERED_ITEM_ID, typeof(string), "Unfiltered Item ID");
        /// <remarks/>
        public static readonly TsDaPropertyDescription DATA_FILTER_VALUE = new TsDaPropertyDescription(TsDaProperty.DATA_FILTER_VALUE, typeof(string), "Data Filter Value");
        #endregion

        #region Object Member Overrides
        /// <summary>
        /// Converts the description to a string.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns the description for the specified property.
        /// </summary>
        public static TsDaPropertyDescription Find(TsDaPropertyID id)
        {
            var fields = typeof(TsDaPropertyDescription).GetFields(BindingFlags.Static | BindingFlags.Public);

            foreach (var field in fields)
            {
                var property = (TsDaPropertyDescription)field.GetValue(typeof(TsDaPropertyDescription));

                if (property.ID == id)
                {
                    return property;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns an array of all well-known property descriptions.
        /// </summary>
        public static TsDaPropertyDescription[] Enumerate()
        {
            var values = new ArrayList();

            var fields = typeof(TsDaPropertyDescription).GetFields(BindingFlags.Static | BindingFlags.Public);

            Array.ForEach(fields, field => values.Add(field.GetValue(typeof(TsDaPropertyDescription))));

            return (TsDaPropertyDescription[])values.ToArray(typeof(TsDaPropertyDescription));
        }
        #endregion
    }
}
