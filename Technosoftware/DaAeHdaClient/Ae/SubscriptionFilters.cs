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

namespace Technosoftware.DaAeHdaClient.Ae
{
	/// <summary>
	/// Describes the event filters for a subscription.
	/// </summary>
	[Serializable]
	public class TsCAeSubscriptionFilters : ICloneable, ISerializable
	{
		#region CategoryCollection Class
        /// <summary>
		/// Contains a writable collection category ids.
		/// </summary>
		[Serializable]
		public class CategoryCollection : OpcWriteableCollection
		{
			#region Constructors, Destructor, Initialization
			/// <summary>
			/// Creates an empty collection.
			/// </summary>
			internal CategoryCollection() : base(null, typeof(int)) { }

			#region ISerializable Members
            /// <summary>
			/// Constructs an object by deserializing it from a stream.
			/// </summary>
			protected CategoryCollection(SerializationInfo info, StreamingContext context) : base(info, context) { }
            #endregion
            #endregion

			#region Public Methods
            /// <summary>
			/// An indexer for the collection.
			/// </summary>
			public new int this[int index] => (int)Array[index];

            /// <summary>
			/// Returns a copy of the collection as an array.
			/// </summary>
			public new int[] ToArray()
			{
				return (int[])Array.ToArray(typeof(int));
			}
            #endregion
		}
        #endregion

		#region StringCollection Class
        /// <summary>
		/// Contains a writable collection of strings.
		/// </summary>
		[Serializable]
		public class StringCollection : OpcWriteableCollection
		{
			#region Constructors, Destructor, Initialization
			/// <summary>
			/// Creates an empty collection.
			/// </summary>
			internal StringCollection() : base(null, typeof(string)) { }
            #endregion

			#region Public Methods
			/// <summary>
			/// An indexer for the collection.
			/// </summary>
			public new string this[int index] => (string)Array[index];

            /// <summary>
			/// Returns a copy of the collection as an array.
			/// </summary>
			public new string[] ToArray()
			{
				return (string[])Array.ToArray(typeof(string));
			}
            #endregion

			#region ISerializable Members
            /// <summary>
			/// Constructs an object by deserializing it from a stream.
			/// </summary>
			protected StringCollection(SerializationInfo info, StreamingContext context) : base(info, context) { }
            #endregion

		}
        #endregion

		#region Names Class
		/// <summary>
		/// A set of names for fields used in serialization.
		/// </summary>
		private class Names
		{
			internal const string EventTypes = "ET";
			internal const string Categories = "CT";
			internal const string HighSeverity = "HS";
			internal const string LowSeverity = "LS";
			internal const string Areas = "AR";
			internal const string Sources = "SR";
		}
        #endregion

		#region Fields
        private int eventTypes_ = (int)TsCAeEventType.All;
		private CategoryCollection categories_ = new CategoryCollection();
		private int highSeverity_ = 1000;
		private int lowSeverity_ = 1;
		private StringCollection areas_ = new StringCollection();
		private StringCollection sources_ = new StringCollection();
        #endregion

		#region Constructors, Destructor, Initialization
        /// <summary>
		/// Initializes object with default values.
		/// </summary>
		public TsCAeSubscriptionFilters() { }

		/// <summary>
		/// Constructs a server by de-serializing its OpcUrl from the stream.
		/// </summary>
		protected TsCAeSubscriptionFilters(SerializationInfo info, StreamingContext context)
		{
			eventTypes_ = (int)info.GetValue(Names.EventTypes, typeof(int));
			categories_ = (CategoryCollection)info.GetValue(Names.Categories, typeof(CategoryCollection));
			highSeverity_ = (int)info.GetValue(Names.HighSeverity, typeof(int));
			lowSeverity_ = (int)info.GetValue(Names.LowSeverity, typeof(int));
			areas_ = (StringCollection)info.GetValue(Names.Areas, typeof(StringCollection));
			sources_ = (StringCollection)info.GetValue(Names.Sources, typeof(StringCollection));
		}
        #endregion

		#region Properties
		/// <summary>
		/// A mask indicating which event types should be sent to the client.
		/// </summary>
		public int EventTypes
		{
			get => eventTypes_;
            set => eventTypes_ = value;
        }

		/// <summary>
		/// The highest severity for the events that should be sent to the client.
		/// </summary>
		public int HighSeverity
		{
			get => highSeverity_;
            set => highSeverity_ = value;
        }

		/// <summary>
		/// The lowest severity for the events that should be sent to the client.
		/// </summary>
		public int LowSeverity
		{
			get => lowSeverity_;
            set => lowSeverity_ = value;
        }

		/// <summary>
		/// The category ids for the events that should be sent to the client.
		/// </summary>
		public CategoryCollection Categories => categories_;

        /// <summary>
		/// A list of full-qualified ids for process areas of interest - only events or conditions in these areas will be reported.
		/// </summary>
		public StringCollection Areas => areas_;

        /// <summary>
		/// A list of full-qualified ids for sources of interest - only events or conditions from these sources will be reported.
		/// </summary>
		public StringCollection Sources => sources_;
        #endregion

		#region ISerializable Members
        /// <summary>
		/// Serializes a server into a stream.
		/// </summary>
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(Names.EventTypes, eventTypes_);
			info.AddValue(Names.Categories, categories_);
			info.AddValue(Names.HighSeverity, highSeverity_);
			info.AddValue(Names.LowSeverity, lowSeverity_);
			info.AddValue(Names.Areas, areas_);
			info.AddValue(Names.Sources, sources_);
		}
		#endregion

		#region ICloneable Members
        /// <summary>
		/// Creates a deep copy of the object.
		/// </summary>
		public virtual object Clone()
		{
			var filters = (TsCAeSubscriptionFilters)MemberwiseClone();

			filters.categories_ = (CategoryCollection)categories_.Clone();
			filters.areas_ = (StringCollection)areas_.Clone();
			filters.sources_ = (StringCollection)sources_.Clone();

			return filters;
		}
		#endregion
	}
}
