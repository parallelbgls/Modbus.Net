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
using System.Runtime.InteropServices;

using Technosoftware.DaAeHdaClient.Da;
using Technosoftware.DaAeHdaClient.Hda;
#endregion

#pragma warning disable 0618

namespace Technosoftware.DaAeHdaClient.Com.Hda
{
	/// <summary>
    /// Contains state information for a single asynchronous Technosoftware.DaAeHdaClient.Com.Hda.Interop.
	/// </summary>
	internal class Interop
	{
		/// <summary>
		/// Converts a standard FILETIME to an OpcRcw.Da.FILETIME structure.
		/// </summary>
		internal static OpcRcw.Hda.OPCHDA_FILETIME Convert(FILETIME input)
		{
			var output = new OpcRcw.Hda.OPCHDA_FILETIME();
			output.dwLowDateTime   = input.dwLowDateTime;
			output.dwHighDateTime  = input.dwHighDateTime;
			return output;
		}

		/// <summary>
		/// Converts an OpcRcw.Da.FILETIME to a standard FILETIME structure.
		/// </summary>
		internal static FILETIME Convert(OpcRcw.Hda.OPCHDA_FILETIME input)
		{
			var output       = new FILETIME();
			output.dwLowDateTime  = input.dwLowDateTime;
			output.dwHighDateTime = input.dwHighDateTime;
			return output;
		}

		/// <summary>
		/// Converts a decimal value to a OpcRcw.Hda.OPCHDA_TIME structure.
		/// </summary>
		internal static OpcRcw.Hda.OPCHDA_FILETIME GetFILETIME(decimal input)
		{
			var output = new OpcRcw.Hda.OPCHDA_FILETIME();	

			output.dwHighDateTime = (int)((((ulong)(input*10000000)) & 0xFFFFFFFF00000000)>>32);
			output.dwLowDateTime  = (int)((((ulong)(input*10000000)) & 0x00000000FFFFFFFF));

			return output;
		}

		/// <summary>
		/// Returns an array of FILETIMEs.
		/// </summary>
		internal static OpcRcw.Hda.OPCHDA_FILETIME[] GetFILETIMEs(DateTime[] input)
		{
            OpcRcw.Hda.OPCHDA_FILETIME[] output = null;

			if (input != null)
			{
				output = new OpcRcw.Hda.OPCHDA_FILETIME[input.Length];

				for (var ii = 0; ii < input.Length; ii++)
				{
					output[ii] = Convert(Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIME(input[ii]));
				}
			}

			return output;
		}

		/// <summary>
		/// Converts a Technosoftware.DaAeHdaClient.Time object to a Technosoftware.DaAeHdaClient.Com.Hda.OPCHDA_TIME structure.
		/// </summary>
		internal static OpcRcw.Hda.OPCHDA_TIME GetTime(TsCHdaTime input)
		{
			var output = new OpcRcw.Hda.OPCHDA_TIME();

			if (input != null)
			{
				output.ftTime  = Convert(Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIME(input.AbsoluteTime));
				output.szTime  = (input.IsRelative)?input.ToString():"";
				output.bString = (input.IsRelative)?1:0;
			}

			// create a null value for a time structure.
			else
			{
				output.ftTime  = Convert(Technosoftware.DaAeHdaClient.Com.Interop.GetFILETIME(DateTime.MinValue));
				output.szTime = "";
				output.bString = 1;
			}

			return output;
		}

		/// <summary>
		/// Unmarshals and deallocates an array of OPCHDA_ITEM structures.
		/// </summary>
		internal static TsCHdaItemValueCollection[] GetItemValueCollections(ref IntPtr pInput, int count, bool deallocate)
		{
			TsCHdaItemValueCollection[] output = null;

			if (pInput != IntPtr.Zero && count > 0)
			{
				output = new TsCHdaItemValueCollection[count];

				var pos = pInput;

				for (var ii = 0; ii < count; ii++)
				{
					output[ii] = GetItemValueCollection(pos, deallocate);
                    pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(OpcRcw.Hda.OPCHDA_ITEM)));
				}

				if (deallocate)
				{
					Marshal.FreeCoTaskMem(pInput);
					pInput = IntPtr.Zero;
				}
			}

			return output;
		}

		/// <summary>
		/// Unmarshals and deallocates an OPCHDA_ITEM structure.
		/// </summary>
		internal static TsCHdaItemValueCollection GetItemValueCollection(IntPtr pInput, bool deallocate)
		{
			TsCHdaItemValueCollection output = null;

			if (pInput != IntPtr.Zero)
			{
				var item = Marshal.PtrToStructure(pInput, typeof(OpcRcw.Hda.OPCHDA_ITEM));

				output = GetItemValueCollection((OpcRcw.Hda.OPCHDA_ITEM)item, deallocate);

				if (deallocate)
				{
					Marshal.DestroyStructure(pInput, typeof(OpcRcw.Hda.OPCHDA_ITEM));
				}
			}

			return output;
		}

		/// <summary>
		/// Unmarshals and deallocates an OPCHDA_ITEM structure.
		/// </summary>
		internal static TsCHdaItemValueCollection GetItemValueCollection(OpcRcw.Hda.OPCHDA_ITEM input, bool deallocate)
		{
			var output = new TsCHdaItemValueCollection();

			output.ClientHandle = input.hClient;
			output.Aggregate = input.haAggregate;

            var values = Com.Interop.GetVARIANTs(ref input.pvDataValues, input.dwCount, deallocate);
			var timestamps = Utilities.Interop.GetDateTimes(ref input.pftTimeStamps, input.dwCount, deallocate);
			var qualities = Utilities.Interop.GetInt32s(ref input.pdwQualities, input.dwCount, deallocate);

			for (var ii = 0; ii < input.dwCount; ii++)
			{
				var value = new TsCHdaItemValue();

				value.Value = values[ii];
				value.Timestamp = timestamps[ii];
                value.Quality = new TsCDaQuality((short)(qualities[ii] & 0x0000FFFF));
                value.HistorianQuality = (TsCHdaQuality)((int)(qualities[ii] & 0xFFFF0000));

				output.Add(value);
			}

			return output;
		}

		/// <summary>
		/// Unmarshals and deallocates an array of OPCHDA_MODIFIEDITEM structures.
		/// </summary>
		internal static TsCHdaModifiedValueCollection[] GetModifiedValueCollections(ref IntPtr pInput, int count, bool deallocate)
		{
			TsCHdaModifiedValueCollection[] output = null;

			if (pInput != IntPtr.Zero && count > 0)
			{
				output = new TsCHdaModifiedValueCollection[count];

				var pos = pInput;

				for (var ii = 0; ii < count; ii++)
				{
					output[ii] = GetModifiedValueCollection(pos, deallocate);
                    pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(OpcRcw.Hda.OPCHDA_MODIFIEDITEM)));
				}

				if (deallocate)
				{
					Marshal.FreeCoTaskMem(pInput);
					pInput = IntPtr.Zero;
				}
			}

			return output;
		}

		/// <summary>
		/// Unmarshals and deallocates an OPCHDA_MODIFIEDITEM structure.
		/// </summary>
		internal static TsCHdaModifiedValueCollection GetModifiedValueCollection(IntPtr pInput, bool deallocate)
		{
			TsCHdaModifiedValueCollection output = null;

			if (pInput != IntPtr.Zero)
			{
				var item = Marshal.PtrToStructure(pInput, typeof(OpcRcw.Hda.OPCHDA_MODIFIEDITEM));

				output = GetModifiedValueCollection((OpcRcw.Hda.OPCHDA_MODIFIEDITEM)item, deallocate);

				if (deallocate)
				{
					Marshal.DestroyStructure(pInput, typeof(OpcRcw.Hda.OPCHDA_MODIFIEDITEM));
				}
			}

			return output;
		}

		/// <summary>
		/// Unmarshals and deallocates an OPCHDA_MODIFIEDITEM structure.
		/// </summary>
		internal static TsCHdaModifiedValueCollection GetModifiedValueCollection(OpcRcw.Hda.OPCHDA_MODIFIEDITEM input, bool deallocate)
		{
			var output = new TsCHdaModifiedValueCollection();

			output.ClientHandle = input.hClient;

            var values = Com.Interop.GetVARIANTs(ref input.pvDataValues, input.dwCount, deallocate);
			var timestamps = Utilities.Interop.GetDateTimes(ref input.pftTimeStamps, input.dwCount, deallocate);
			var qualities = Utilities.Interop.GetInt32s(ref input.pdwQualities, input.dwCount, deallocate);
			var modificationTimes = Utilities.Interop.GetDateTimes(ref input.pftModificationTime, input.dwCount, deallocate);
			var editTypes = Utilities.Interop.GetInt32s(ref input.pEditType, input.dwCount, deallocate);
			var users = Utilities.Interop.GetUnicodeStrings(ref input.szUser, input.dwCount, deallocate);

			for (var ii = 0; ii < input.dwCount; ii++)
			{
				var value = new TsCHdaModifiedValue();

				value.Value = values[ii];
				value.Timestamp = timestamps[ii];
                value.Quality = new TsCDaQuality((short)(qualities[ii] & 0x0000FFFF));
                value.HistorianQuality = (TsCHdaQuality)((int)(qualities[ii] & 0xFFFF0000));
				value.ModificationTime = modificationTimes[ii];
				value.EditType = (TsCHdaEditType)editTypes[ii];
				value.User = users[ii];

				output.Add(value);
			}

			return output;
		}

		/// <summary>
		/// Unmarshals and deallocates an array of OPCHDA_ATTRIBUTE structures.
		/// </summary>
		internal static TsCHdaAttributeValueCollection[] GetAttributeValueCollections(ref IntPtr pInput, int count, bool deallocate)
		{
			TsCHdaAttributeValueCollection[] output = null;

			if (pInput != IntPtr.Zero && count > 0)
			{
				output = new TsCHdaAttributeValueCollection[count];

				var pos = pInput;

				for (var ii = 0; ii < count; ii++)
				{
					output[ii] = GetAttributeValueCollection(pos, deallocate);
                    pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(OpcRcw.Hda.OPCHDA_ATTRIBUTE)));
				}

				if (deallocate)
				{
					Marshal.FreeCoTaskMem(pInput);
					pInput = IntPtr.Zero;
				}
			}

			return output;
		}

		/// <summary>
		/// Unmarshals and deallocates an OPCHDA_ATTRIBUTE structure.
		/// </summary>
		internal static TsCHdaAttributeValueCollection GetAttributeValueCollection(IntPtr pInput, bool deallocate)
		{
			TsCHdaAttributeValueCollection output = null;

			if (pInput != IntPtr.Zero)
			{
				var item = Marshal.PtrToStructure(pInput, typeof(OpcRcw.Hda.OPCHDA_ATTRIBUTE));

				output = GetAttributeValueCollection((OpcRcw.Hda.OPCHDA_ATTRIBUTE)item, deallocate);

				if (deallocate)
				{
					Marshal.DestroyStructure(pInput, typeof(OpcRcw.Hda.OPCHDA_ATTRIBUTE));
				}
			}

			return output;
		}

		/// <summary>
		/// Unmarshals and deallocates an OPCHDA_ATTRIBUTE structure.
		/// </summary>
		internal static TsCHdaAttributeValueCollection GetAttributeValueCollection(OpcRcw.Hda.OPCHDA_ATTRIBUTE input, bool deallocate)
		{
			var output = new TsCHdaAttributeValueCollection();

			output.AttributeID = input.dwAttributeID;

            var values = Com.Interop.GetVARIANTs(ref input.vAttributeValues, input.dwNumValues, deallocate);
			var timestamps = Utilities.Interop.GetDateTimes(ref input.ftTimeStamps, input.dwNumValues, deallocate);

			for (var ii = 0; ii < input.dwNumValues; ii++)
			{
				var value = new TsCHdaAttributeValue();

				value.Value = values[ii];
				value.Timestamp = timestamps[ii];

				output.Add(value);
			}

			return output;
		}

		/// <summary>
		/// Unmarshals and deallocates an array of OPCHDA_ANNOTATION structures.
		/// </summary>
		internal static TsCHdaAnnotationValueCollection[] GetAnnotationValueCollections(ref IntPtr pInput, int count, bool deallocate)
		{
			TsCHdaAnnotationValueCollection[] output = null;

			if (pInput != IntPtr.Zero && count > 0)
			{
				output = new TsCHdaAnnotationValueCollection[count];

				var pos = pInput;

				for (var ii = 0; ii < count; ii++)
				{
					output[ii] = GetAnnotationValueCollection(pos, deallocate);
                    pos = (IntPtr)(pos.ToInt64() + Marshal.SizeOf(typeof(OpcRcw.Hda.OPCHDA_ANNOTATION)));
				}

				if (deallocate)
				{
					Marshal.FreeCoTaskMem(pInput);
					pInput = IntPtr.Zero;
				}
			}

			return output;
		}

		/// <summary>
		/// Unmarshals and deallocates an OPCHDA_ANNOTATION structure.
		/// </summary>
		internal static TsCHdaAnnotationValueCollection GetAnnotationValueCollection(IntPtr pInput, bool deallocate)
		{
			TsCHdaAnnotationValueCollection output = null;

			if (pInput != IntPtr.Zero)
			{
				var item = Marshal.PtrToStructure(pInput, typeof(OpcRcw.Hda.OPCHDA_ANNOTATION));

				output = GetAnnotationValueCollection((OpcRcw.Hda.OPCHDA_ANNOTATION)item, deallocate);

				if (deallocate)
				{
					Marshal.DestroyStructure(pInput, typeof(OpcRcw.Hda.OPCHDA_ANNOTATION));
				}
			}

			return output;
		}

		/// <summary>
		/// Unmarshals and deallocates an OPCHDA_ANNOTATION structure.
		/// </summary>
		internal static TsCHdaAnnotationValueCollection GetAnnotationValueCollection(OpcRcw.Hda.OPCHDA_ANNOTATION input, bool deallocate)
		{
			var output = new TsCHdaAnnotationValueCollection();

			output.ClientHandle = input.hClient;

			var timestamps = Utilities.Interop.GetDateTimes(ref input.ftTimeStamps, input.dwNumValues, deallocate);
			var annotations = Utilities.Interop.GetUnicodeStrings(ref input.szAnnotation, input.dwNumValues, deallocate);
			var creationTimes = Utilities.Interop.GetDateTimes(ref input.ftAnnotationTime, input.dwNumValues, deallocate);
			var users = Utilities.Interop.GetUnicodeStrings(ref input.szUser, input.dwNumValues, deallocate);

			for (var ii = 0; ii < input.dwNumValues; ii++)
			{
				var value = new TsCHdaAnnotationValue();

				value.Timestamp = timestamps[ii];
				value.Annotation = annotations[ii];
				value.CreationTime = creationTimes[ii];
				value.User = users[ii];

				output.Add(value);
			}

			return output;
		}
	}
}
