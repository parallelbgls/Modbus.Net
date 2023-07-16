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
using System.IO;
using System.Xml;
using Technosoftware.DaAeHdaClient.Da;
#endregion

namespace Technosoftware.DaAeHdaClient.Cpx
{
    /// <summary>
    /// A class that contains complex data related properties for an item.
    /// </summary>
    public class TsCCpxComplexItem : OpcItem
    {
        ///////////////////////////////////////////////////////////////////////
        #region Constants

        /// <summary>
        /// The reserved name for complex data branch in the server namespace.
        /// </summary>
        public const string CPX_BRANCH = "CPX";

        /// <summary>
        /// The reserved name for the data filters branch in the CPX namespace.
        /// </summary>
        public const string CPX_DATA_FILTERS = "DataFilters";

        /// <summary>
        /// The set of all complex data item properties.
        /// </summary>
        public static readonly TsDaPropertyID[] CPX_PROPERTIES = new TsDaPropertyID[]
        {
            TsDaProperty.TYPE_SYSTEM_ID,
            TsDaProperty.DICTIONARY_ID,
            TsDaProperty.TYPE_ID,
            TsDaProperty.UNCONVERTED_ITEM_ID,
            TsDaProperty.UNFILTERED_ITEM_ID,
            TsDaProperty.DATA_FILTER_VALUE
        };

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Fields

        private string _typeSystemID;
        private string _dictionaryID;
        private string _typeID;
        private OpcItem _dictionaryItemID;
        private OpcItem _typeItemID;
        private OpcItem _unconvertedItemID;
        private OpcItem _unfilteredItemID;
        private OpcItem _filterItem;

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Constructors, Destructor, Initialization

        /// <summary>
        /// Initializes the object with the default values.
        /// </summary>
        public TsCCpxComplexItem() { }

        /// <summary>
        /// Initializes the object from an item identifier.
        /// </summary>
        /// <param name="itemID">The item id.</param>
        public TsCCpxComplexItem(OpcItem itemID)
        {
            ItemPath = itemID.ItemPath;
            ItemName = itemID.ItemName;
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Properties

        /// <summary>
        /// The name of the item in the server address space.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The type system id for the complex item.
        /// </summary>
        public string TypeSystemID => _typeSystemID;

        /// <summary>
		/// The dictionary id for the complex item.
		/// </summary>
		public string DictionaryID => _dictionaryID;

        /// <summary>
		/// The type id for the complex item.
		/// </summary>
		public string TypeID => _typeID;

        /// <summary>
		/// The id of the item containing the dictionary for the item.
		/// </summary>
		public OpcItem DictionaryItemID => _dictionaryItemID;

        /// <summary>
		/// The id of the item containing the type description for the item.
		/// </summary>
		public OpcItem TypeItemID => _typeItemID;

        /// <summary>
		/// The id of the unconverted version of the item. Only valid for items which apply type conversions to the item. 
		/// </summary>
		public OpcItem UnconvertedItemID => _unconvertedItemID;

        /// <summary>
		/// The id of the unfiltered version of the item. Only valid for items apply data filters to the item. 
		/// </summary>
		public OpcItem UnfilteredItemID => _unfilteredItemID;

        /// <summary>
		/// The item used to create new data filters for the complex data item (null is item does not support it). 
		/// </summary>
		public OpcItem DataFilterItem => _filterItem;

        /// <summary>
		/// The current data filter value. Only valid for items apply data filters to the item.
		/// </summary>
		public string DataFilterValue { get; set; }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Public Methods

        /// <summary>
        /// Returns an appropriate string representation of the object.
        /// </summary>
        public override string ToString()
        {
            if (Name != null || Name.Length != 0)
            {
                return Name;
            }

            return ItemName;
        }

        /// <summary>
        /// Returns the root complex data item for the object.
        /// </summary>
        public TsCCpxComplexItem GetRootItem()
        {
            if (_unconvertedItemID != null)
            {
                return TsCCpxComplexTypeCache.GetComplexItem(_unconvertedItemID);
            }

            if (_unfilteredItemID != null)
            {
                return TsCCpxComplexTypeCache.GetComplexItem(_unfilteredItemID);
            }

            return this;
        }

        /// <summary>
        /// Reads the current complex data item properties from the server.
        /// </summary>
        /// <param name="server">The server object</param>
        public void Update(TsCDaServer server)
        {
            // clear the existing state.
            Clear();

            // check if the item supports any of the complex data properties. 
            var results = server.GetProperties(
                new OpcItem[] { this },
                CPX_PROPERTIES,
                true);

            // unexpected return value.
            if (results == null || results.Length != 1)
            {
                throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "Unexpected results returned from server.");
            }

            // update object.
            if (!Init((TsCDaItemProperty[])results[0].ToArray(typeof(TsCDaItemProperty))))
            {
                throw new OpcResultException(new OpcResult((int)OpcResult.E_INVALIDARG.Code, OpcResult.FuncCallType.SysFuncCall, null), "Not a valid complex item.");
            }

            // check if data filters are suppported for the item.
            GetDataFilterItem(server);
        }

        /// <summary>
        /// Fetches the set of type conversions from the server.
        /// </summary>
        /// <param name="server">The server object</param>
        public TsCCpxComplexItem[] GetTypeConversions(TsCDaServer server)
        {
            // only the root item can have type conversions.
            if (_unconvertedItemID != null || _unfilteredItemID != null)
            {
                return null;
            }

            TsCDaBrowsePosition position = null;

            try
            {
                // look for the 'CPX' branch.
                var filters = new TsCDaBrowseFilters { ElementNameFilter = CPX_BRANCH, BrowseFilter = TsCDaBrowseFilter.Branch, ReturnAllProperties = false, PropertyIDs = null, ReturnPropertyValues = false };

                var elements = server.Browse(this, filters, out position);

                // nothing found.
                if (elements == null || elements.Length == 0)
                {
                    return null;
                }

                // release the browse position object.
                if (position != null)
                {
                    position.Dispose();
                    position = null;
                }

                // browse for type conversions.
                var itemID = new OpcItem(elements[0].ItemPath, elements[0].ItemName);

                filters.ElementNameFilter = null;
                filters.BrowseFilter = TsCDaBrowseFilter.Item;
                filters.ReturnAllProperties = false;
                filters.PropertyIDs = CPX_PROPERTIES;
                filters.ReturnPropertyValues = true;

                elements = server.Browse(itemID, filters, out position);

                // nothing found.
                if (elements == null || elements.Length == 0)
                {
                    return new TsCCpxComplexItem[0];
                }

                // contruct an array of complex data items for each available conversion.
                var conversions = new ArrayList(elements.Length);

                Array.ForEach(elements, element =>
                {
                    if (element.Name != CPX_DATA_FILTERS)
                    {
                        var item = new TsCCpxComplexItem();
                        if (item.Init(element))
                        {
                            // check if data filters supported for type conversion.
                            item.GetDataFilterItem(server);
                            conversions.Add(item);
                        }
                    }
                });

                // return the set of available conversions.
                return (TsCCpxComplexItem[])conversions.ToArray(typeof(TsCCpxComplexItem));
            }
            finally
            {
                if (position != null)
                {
                    position.Dispose();
                    position = null;
                }
            }
        }

        /// <summary>
        /// Fetches the set of data filters from the server.
        /// </summary>
        /// <param name="server">The server object</param>
        public TsCCpxComplexItem[] GetDataFilters(TsCDaServer server)
        {
            // not a valid operation for data filter items. 
            if (_unfilteredItemID != null)
            {
                return null;
            }

            // data filters not supported by the item.
            if (_filterItem == null)
            {
                return null;
            }

            TsCDaBrowsePosition position = null;

            try
            {
                // browse any existing filter instances.
                var filters = new TsCDaBrowseFilters { ElementNameFilter = null, BrowseFilter = TsCDaBrowseFilter.Item, ReturnAllProperties = false, PropertyIDs = CPX_PROPERTIES, ReturnPropertyValues = true };

                var elements = server.Browse(_filterItem, filters, out position);

                // nothing found.
                if (elements == null || elements.Length == 0)
                {
                    return new TsCCpxComplexItem[0];
                }

                // contruct an array of complex data items for each available data filter.
                var dataFilters = new ArrayList(elements.Length);

                Array.ForEach(elements, element =>
                {
                    var item = new TsCCpxComplexItem();
                    if (item.Init(element))
                        dataFilters.Add(item);
                });

                // return the set of available data filters.
                return (TsCCpxComplexItem[])dataFilters.ToArray(typeof(TsCCpxComplexItem));
            }
            finally
            {
                if (position != null)
                {
                    position.Dispose();
                    position = null;
                }
            }
        }

        /// <summary>
        /// Creates a new data filter.
        /// </summary>
        /// <param name="server">The server object</param>
        /// <param name="filterName">The name of the filter</param>
        /// <param name="filterValue">The value of the filter</param>
        public TsCCpxComplexItem CreateDataFilter(TsCDaServer server, string filterName, string filterValue)
        {
            // not a valid operation for data filter items. 
            if (_unfilteredItemID != null)
            {
                return null;
            }

            // data filters not supported by the item.
            if (_filterItem == null)
            {
                return null;
            }

            TsCDaBrowsePosition position = null;

            try
            {
                // write the desired filter to the server.
                var item = new TsCDaItemValue(_filterItem);

                // create the filter parameters document.
                using (var ostrm = new StringWriter())
                {
                    using (var writer = new XmlTextWriter(ostrm))
                    {
                        writer.WriteStartElement("DataFilters");
                        writer.WriteAttributeString("Name", filterName);
                        writer.WriteString(filterValue);
                        writer.WriteEndElement();
                        writer.Close();
                    }
                    // create the value to write.
                    item.Value = ostrm.ToString();
                }
                item.Quality = TsCDaQuality.Bad;
                item.QualitySpecified = false;
                item.Timestamp = DateTime.MinValue;
                item.TimestampSpecified = false;

                // write the value.
                var result = server.Write(new TsCDaItemValue[] { item });

                if (result == null || result.Length == 0)
                {
                    throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "Unexpected results returned from server.");
                }

                if (result[0].Result.Failed())
                {
                    throw new OpcResultException(new OpcResult((int)OpcResult.Cpx.E_FILTER_ERROR.Code, OpcResult.FuncCallType.SysFuncCall, null), "Could not create new data filter.");
                }

                // browse for new data filter item.
                var filters = new TsCDaBrowseFilters { ElementNameFilter = filterName, BrowseFilter = TsCDaBrowseFilter.Item, ReturnAllProperties = false, PropertyIDs = CPX_PROPERTIES, ReturnPropertyValues = true };

                var elements = server.Browse(_filterItem, filters, out position);

                // nothing found.
                if (elements == null || elements.Length == 0)
                {
                    throw new OpcResultException(new OpcResult((int)OpcResult.Cpx.E_FILTER_ERROR.Code, OpcResult.FuncCallType.SysFuncCall, null), "Could not browse to new data filter.");
                }

                var filterItem = new TsCCpxComplexItem();

                if (!filterItem.Init(elements[0]))
                {
                    throw new OpcResultException(new OpcResult((int)OpcResult.Cpx.E_FILTER_ERROR.Code, OpcResult.FuncCallType.SysFuncCall, null), "Could not initialize to new data filter.");
                }

                // return the new data filter.
                return filterItem;
            }
            finally
            {
                if (position != null)
                {
                    position.Dispose();
                }
            }
        }

        /// <summary>
        /// Updates a data filter.
        /// </summary>
        /// <param name="server">The server object</param>
        /// <param name="filterValue">The value of the filter</param>
        public void UpdateDataFilter(TsCDaServer server, string filterValue)
        {
            // not a valid operation for non data filter items. 
            if (_unfilteredItemID == null)
            {
                throw new OpcResultException(new OpcResult((int)OpcResult.Cpx.E_FILTER_ERROR.Code, OpcResult.FuncCallType.SysFuncCall, null), "Cannot update the data filter for this item.");
            }

            // create the value to write.
            var item = new TsCDaItemValue(this) { Value = filterValue, Quality = TsCDaQuality.Bad, QualitySpecified = false, Timestamp = DateTime.MinValue, TimestampSpecified = false };

            // write the value.
            var result = server.Write(new TsCDaItemValue[] { item });

            if (result == null || result.Length == 0)
            {
                throw new OpcResultException(new OpcResult((int)OpcResult.E_FAIL.Code, OpcResult.FuncCallType.SysFuncCall, null), "Unexpected results returned from server.");
            }

            if (result[0].Result.Failed())
            {
                throw new OpcResultException(new OpcResult((int)OpcResult.Cpx.E_FILTER_ERROR.Code, OpcResult.FuncCallType.SysFuncCall, null), "Could not update data filter.");
            }

            // update locale copy of the filter value.
            DataFilterValue = filterValue;
        }

        /// <summary>
        /// Fetches the type dictionary for the item.
        /// </summary>
        /// <param name="server">The server object</param>
        public string GetTypeDictionary(TsCDaServer server)
        {
            var results = server.GetProperties(
                new OpcItem[] { _dictionaryItemID },
                new TsDaPropertyID[] { TsDaProperty.DICTIONARY },
                true);

            if (results == null || results.Length == 0 || results[0].Count == 0)
            {
                return null;
            }

            var property = results[0][0];

            if (!property.Result.Succeeded())
            {
                return null;
            }

            return (string)property.Value;
        }

        /// <summary>
        /// Fetches the type description for the item.
        /// </summary>
        /// <param name="server">The server object</param>
        public string GetTypeDescription(TsCDaServer server)
        {
            var results = server.GetProperties(
                new OpcItem[] { _typeItemID },
                new TsDaPropertyID[] { TsDaProperty.TYPE_DESCRIPTION },
                true);

            if (results == null || results.Length == 0 || results[0].Count == 0)
            {
                return null;
            }

            var property = results[0][0];

            if (!property.Result.Succeeded())
            {
                return null;
            }

            return (string)property.Value;
        }

        /// <summary>
        /// Fetches the item id for the data filters items and stores it in the internal cache.
        /// </summary>
        /// <param name="server">The server object</param>
        public void GetDataFilterItem(TsCDaServer server)
        {
            _filterItem = null;

            // not a valid operation for data filter items. 
            if (_unfilteredItemID != null)
            {
                return;
            }

            TsCDaBrowsePosition position = null;

            try
            {
                var itemID = new OpcItem(this);

                // browse any existing filter instances.
                var filters = new TsCDaBrowseFilters { ElementNameFilter = CPX_DATA_FILTERS, BrowseFilter = TsCDaBrowseFilter.All, ReturnAllProperties = false, PropertyIDs = null, ReturnPropertyValues = false };

                TsCDaBrowseElement[] elements = null;

                // browse for the 'CPX' branch first.
                if (_unconvertedItemID == null)
                {
                    filters.ElementNameFilter = CPX_BRANCH;

                    elements = server.Browse(itemID, filters, out position);

                    // nothing found.
                    if (elements == null || elements.Length == 0)
                    {
                        return;
                    }

                    // release the position object.
                    if (position != null)
                    {
                        position.Dispose();
                        position = null;
                    }

                    // update the item for the next browse operation.
                    itemID = new OpcItem(elements[0].ItemPath, elements[0].ItemName);

                    filters.ElementNameFilter = CPX_DATA_FILTERS;
                }

                // browse for the 'DataFilters' branch.
                elements = server.Browse(itemID, filters, out position);

                // nothing found.
                if (elements == null || elements.Length == 0)
                {
                    return;
                }

                _filterItem = new OpcItem(elements[0].ItemPath, elements[0].ItemName);
            }
            finally
            {
                if (position != null)
                {
                    position.Dispose();
                }
            }
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Private Methods

        /// <summary>
        /// Sets all object properties to their default values.
        /// </summary>
        private void Clear()
        {
            _typeSystemID = null;
            _dictionaryID = null;
            _typeID = null;
            _dictionaryItemID = null;
            _typeItemID = null;
            _unconvertedItemID = null;
            _unfilteredItemID = null;
            _filterItem = null;
            DataFilterValue = null;
        }

        /// <summary>
        /// Initializes the object from a browse element.
        /// </summary>
        private bool Init(TsCDaBrowseElement element)
        {
            // update the item id.
            ItemPath = element.ItemPath;
            ItemName = element.ItemName;
            Name = element.Name;

            return Init(element.Properties);
        }

        /// <summary>
        /// Initializes the object from a list of properties.
        /// </summary>
        private bool Init(TsCDaItemProperty[] properties)
        {
            // put the object into default state.
            Clear();

            // must have at least three properties defined.
            if (properties == null || properties.Length < 3)
            {
                return false;
            }

            foreach (var property in properties)
            {
                // continue - ignore invalid properties.
                if (!property.Result.Succeeded())
                {
                    continue;
                }

                // type system id.
                if (property.ID == TsDaProperty.TYPE_SYSTEM_ID)
                {
                    _typeSystemID = (string)property.Value;
                    continue;
                }

                // dictionary id
                if (property.ID == TsDaProperty.DICTIONARY_ID)
                {
                    _dictionaryID = (string)property.Value;
                    _dictionaryItemID = new OpcItem(property.ItemPath, property.ItemName);
                    continue;
                }

                // type id
                if (property.ID == TsDaProperty.TYPE_ID)
                {
                    _typeID = (string)property.Value;
                    _typeItemID = new OpcItem(property.ItemPath, property.ItemName);
                    continue;
                }

                // unconverted item id
                if (property.ID == TsDaProperty.UNCONVERTED_ITEM_ID)
                {
                    _unconvertedItemID = new OpcItem(ItemPath, (string)property.Value);
                    continue;
                }

                // unfiltered item id
                if (property.ID == TsDaProperty.UNFILTERED_ITEM_ID)
                {
                    _unfilteredItemID = new OpcItem(ItemPath, (string)property.Value);
                    continue;
                }

                // data filter value.
                if (property.ID == TsDaProperty.DATA_FILTER_VALUE)
                {
                    DataFilterValue = (string)property.Value;
                    continue;
                }
            }

            // validate object.
            if (_typeSystemID == null || _dictionaryID == null || _typeID == null)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
