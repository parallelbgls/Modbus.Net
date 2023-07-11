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

using System.Collections;

using Technosoftware.DaAeHdaClient.Da;
#endregion

namespace Technosoftware.DaAeHdaClient.Cpx
{
	/// <summary>
	/// A class that caches properties of complex data items.
	/// </summary>
	public class TsCCpxComplexTypeCache
	{

		///////////////////////////////////////////////////////////////////////
		#region Fields

		/// <summary>
		/// The active server for the application.
		/// </summary>
		private static TsCDaServer _server;

		/// <summary>
		/// A cache of item properties fetched from the active server.
		/// </summary>
		private static Hashtable _items = new Hashtable();

		/// <summary>
		/// A cache of type dictionaries fetched from the active server.
		/// </summary>
		private static Hashtable _dictionaries = new Hashtable();

		/// <summary>
		/// A cache of type descriptions fetched from the active server.
		/// </summary>
		private static Hashtable _descriptions = new Hashtable();

		#endregion

		///////////////////////////////////////////////////////////////////////
		#region Constructors, Destructor, Initialization
		
		/// <summary>
		/// Initializes the complex type cache with defaults.
		/// </summary>
		public TsCCpxComplexTypeCache() { }

		#endregion

		///////////////////////////////////////////////////////////////////////
		#region Properties

		/// <summary>
		/// Get or sets the server to use for the cache.
		/// </summary>
		public static TsCDaServer Server
		{
			get
			{
				lock (typeof(TsCCpxComplexTypeCache))
				{
					return _server;
				}
			}

			set
			{
				lock (typeof(TsCCpxComplexTypeCache))
				{
					_server = value;
					_items.Clear();
					_dictionaries.Clear();
					_descriptions.Clear();
				}
			}
		}

		#endregion

		///////////////////////////////////////////////////////////////////////
		#region Public Methods

		/// <summary>
		/// Returns the complex item for the specified item id.
		/// </summary>
		/// <param name="itemID">The item id.</param>
		public static TsCCpxComplexItem GetComplexItem(OpcItem itemID)
		{
			if (itemID == null) return null;

			lock (typeof(TsCCpxComplexTypeCache))
			{
				var item = new TsCCpxComplexItem(itemID);

				try
				{
					item.Update(_server);
				}
				catch
				{
					// item is not a valid complex data item.
					item = null;
				}

				_items[itemID.Key] = item;
				return item;
			}
		}

		/// <summary>
		/// Returns the complex item for the specified item browse element.
		/// </summary>
		/// <param name="element">The item browse element.</param>
		public static TsCCpxComplexItem GetComplexItem(TsCDaBrowseElement element)
		{
			if (element == null) return null;

			lock (typeof(TsCCpxComplexTypeCache))
			{
				return GetComplexItem(new OpcItem(element.ItemPath, element.ItemName));
			}
		}

		/// <summary>
		/// Fetches the type dictionary for the item.
		/// </summary>
		/// <param name="itemID">The item id.</param>
		public static string GetTypeDictionary(OpcItem itemID)
		{
			if (itemID == null) return null;

			lock (typeof(TsCCpxComplexTypeCache))
			{
				var dictionary = (string)_dictionaries[itemID.Key];

				if (dictionary != null)
				{
					return dictionary;
				}

				var item = GetComplexItem(itemID);

				if (item != null)
				{
					dictionary = item.GetTypeDictionary(_server);
				}

				return dictionary;
			}
		}

		/// <summary>
		/// Fetches the type description for the item.
		/// </summary>
		/// <param name="itemID">The item id.</param>
		public static string GetTypeDescription(OpcItem itemID)
		{
			if (itemID == null) return null;

			lock (typeof(TsCCpxComplexTypeCache))
			{
				string description = null;

				var item = GetComplexItem(itemID);

				if (item != null)
				{
					description = (string)_descriptions[item.TypeItemID.Key];

					if (description != null)
					{
						return description;
					}

					_descriptions[item.TypeItemID.Key] = description = item.GetTypeDescription(_server);
				}

				return description;
			}
		}

		#endregion
	}
}
