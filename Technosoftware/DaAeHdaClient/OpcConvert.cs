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
using System.Text;
using System.Xml;
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// Defines various functions used to convert types.
    /// </summary>
    public class OpcConvert
    {
        #region Public Methods
        /// <summary>
        /// Checks whether the array contains any useful data.
        /// </summary>
        public static bool IsValid(Array array)
        {
            return (array != null && array.Length > 0);
        }

        /// <summary>
        /// Checks whether the array contains any useful data.
        /// </summary>
        public static bool IsEmpty(Array array)
        {
            return (array == null || array.Length == 0);
        }

        /// <summary>
        /// Checks whether the string contains any useful data.
        /// </summary>
        public static bool IsValid(string target)
        {
            return !string.IsNullOrEmpty(target);
        }

        /// <summary>
        /// Checks whether the string contains any useful data.
        /// </summary>
        public static bool IsEmpty(string target)
        {
            return string.IsNullOrEmpty(target);
        }

        /// <summary>
        /// Performs a deep copy of an object if possible.
        /// </summary>
        public static object Clone(object source)
        {
            if (source == null) return null;
            if (source.GetType().IsValueType) return source;

            if (source.GetType().IsArray || source.GetType() == typeof(Array))
            {
                var array = (Array)((Array)source).Clone();

                for (var ii = 0; ii < array.Length; ii++)
                {
                    array.SetValue(Clone(array.GetValue(ii)), ii);
                }

                return array;
            }

            try { return ((ICloneable)source).Clone(); }
            catch { throw new NotSupportedException("Object cannot be cloned."); }
        }

        /// <summary>
        /// Does a deep comparison between two objects.
        /// </summary>
        public static bool Compare(object a, object b)
        {
            if (a == null || b == null) return (a == null && b == null);

            var type1 = a.GetType();
            var type2 = b.GetType();

            if (type1 != type2) return false;

            if (type1.IsArray && type2.IsArray)
            {
                var array1 = (Array)a;
                var array2 = (Array)b;

                if (array1.Length != array2.Length) return false;

                for (var ii = 0; ii < array1.Length; ii++)
                {
                    if (!Compare(array1.GetValue(ii), array2.GetValue(ii))) return false;
                }

                return true;
            }

            return a.Equals(b);
        }

        /// <summary>
        /// Converts an object to the specified type and returns a deep copy.
        /// </summary>
        public static object ChangeType(object source, Type newType)
        {
            // check for null source object.
            if (source == null)
            {
                if (newType != null && newType.IsValueType)
                {
                    return Activator.CreateInstance(newType);
                }

                return null;
            }

            // check for null type or 'object' type.
            if (newType == null || newType == typeof(object) || newType == source.GetType())
            {
                return Clone(source);
            }

            var type = source.GetType();

            // convert between array types.
            if (type.IsArray && newType.IsArray)
            {
                var array = new ArrayList(((Array)source).Length);

                foreach (var element in (Array)source)
                {
                    array.Add(ChangeType(element, newType.GetElementType()));
                }

                return array.ToArray(newType.GetElementType() ?? throw new InvalidOperationException());
            }

            // convert scalar value to an array type.
            if (!type.IsArray && newType.IsArray)
            {
                var array = new ArrayList(1) { ChangeType(source, newType.GetElementType()) };
                return array.ToArray(newType.GetElementType() ?? throw new InvalidOperationException());
            }

            // convert single element array type to scalar type.
            if (type.IsArray && !newType.IsArray && ((Array)source).Length == 1)
            {
                return Convert.ChangeType(((Array)source).GetValue(0), newType);
            }

            // convert array type to string.
            if (type.IsArray && newType == typeof(string))
            {
                var buffer = new StringBuilder();

                buffer.Append("{ ");

                var count = 0;

                foreach (var element in (Array)source)
                {
                    buffer.AppendFormat("{0}", ChangeType(element, typeof(string)));

                    count++;

                    if (count < ((Array)source).Length)
                    {
                        buffer.Append(" | ");
                    }
                }

                buffer.Append(" }");

                return buffer.ToString();
            }

            // convert to enumerated type.
            if (newType.IsEnum)
            {
                if (type == typeof(string))
                {
                    // check for an integer passed as a string.
                    if (((string)source).Length > 0 && char.IsDigit((string)source, 0))
                    {
                        return Enum.ToObject(newType, Convert.ToInt32(source));
                    }

                    // parse a string value.
                    return Enum.Parse(newType, (string)source);
                }
                else
                {
                    // convert numerical value to an enum.
                    return Enum.ToObject(newType, source);
                }
            }

            // convert to boolean type.
            if (newType == typeof(bool))
            {
                // check for an integer passed as a string.
                if (source is string text)
                {
                    if (text.Length > 0 && (text[0] == '+' || text[0] == '-' || char.IsDigit(text, 0)))
                    {
                        return Convert.ToBoolean(Convert.ToInt32(source));
                    }
                }

                return Convert.ToBoolean(source);
            }

            // use default conversion.
            return Convert.ChangeType(source, newType);
        }

        /// <summary>
        /// Formats an item or property value as a string.
        /// </summary>
        public static string ToString(object source)
        {
            // check for null
            if (source == null) return "";

            var type = source.GetType();

            // check for invalid values in date times.
            if (type == typeof(DateTime))
            {
                if (((DateTime)source) == DateTime.MinValue)
                {
                    return string.Empty;
                }

                var date = (DateTime)source;

                if (date.Millisecond > 0)
                {
                    return date.ToString("yyyy-MM-dd HH:mm:ss.fff");
                }
                else
                {
                    return date.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }

            // use only the local name for qualified names.
            if (type == typeof(XmlQualifiedName))
            {
                return ((XmlQualifiedName)source).Name;
            }

            // use only the name for system types.
            if (type.FullName == "System.RuntimeType")
            {
                return ((Type)source).Name;
            }

            // treat byte arrays as a special case.
            if (type == typeof(byte[]))
            {
                var bytes = (byte[])source;

                var buffer = new StringBuilder(bytes.Length * 3);

                foreach (var character in bytes)
                {
                    buffer.Append(character.ToString("X2"));
                    buffer.Append(" ");
                }

                return buffer.ToString();
            }

            // show the element type and length for arrays.
            if (type.IsArray)
            {
                return $"{type.GetElementType()?.Name}[{((Array)source).Length}]";
            }

            // instances of array are always treated as arrays of objects.
            if (type == typeof(Array))
            {
                return $"Object[{((Array)source).Length}]";
            }

            // default behavior.
            return source.ToString();
        }

        /// <summary>
        /// Tests if the specified string matches the specified pattern.
        /// </summary>
        public static bool Match(string target, string pattern, bool caseSensitive)
        {
            // an empty pattern always matches.
            if (string.IsNullOrEmpty(pattern))
            {
                return true;
            }

            // an empty string never matches.
            if (string.IsNullOrEmpty(target))
            {
                return false;
            }

            // check for exact match
            if (caseSensitive)
            {
                if (target == pattern)
                {
                    return true;
                }
            }
            else
            {
                if (target.ToLower() == pattern.ToLower())
                {
                    return true;
                }
            }

            var pIndex = 0;
            var tIndex = 0;

            while (tIndex < target.Length && pIndex < pattern.Length)
            {
                var p = ConvertCase(pattern[pIndex++], caseSensitive);

                if (pIndex > pattern.Length)
                {
                    return (tIndex >= target.Length); // if end of string true
                }

                char c;
                switch (p)
                {
                    // match zero or more char.
                    case '*':
                        {
                            while (tIndex < target.Length)
                            {
                                if (Match(target.Substring(tIndex++), pattern.Substring(pIndex), caseSensitive))
                                {
                                    return true;
                                }
                            }

                            return Match(target, pattern.Substring(pIndex), caseSensitive);
                        }

                    // match any one char.
                    case '?':
                        {
                            // check if end of string when looking for a single character.
                            if (tIndex >= target.Length)
                            {
                                return false;
                            }

                            // check if end of pattern and still string data left.
                            if (pIndex >= pattern.Length && tIndex < target.Length - 1)
                            {
                                return false;
                            }

                            tIndex++;
                            break;
                        }

                    // match char set 
                    case '[':
                        {
                            c = ConvertCase(target[tIndex++], caseSensitive);

                            if (tIndex > target.Length)
                            {
                                return false; // syntax 
                            }

                            var l = '\0';

                            // match a char if NOT in set []
                            if (pattern[pIndex] == '!')
                            {
                                ++pIndex;

                                p = ConvertCase(pattern[pIndex++], caseSensitive);

                                while (pIndex < pattern.Length)
                                {
                                    if (p == ']') // if end of char set, then 
                                    {
                                        break; // no match found 
                                    }

                                    if (p == '-')
                                    {
                                        // check a range of chars? 
                                        p = ConvertCase(pattern[pIndex], caseSensitive);

                                        // get high limit of range 
                                        if (pIndex > pattern.Length || p == ']')
                                        {
                                            return false; // syntax 
                                        }

                                        if (c >= l && c <= p)
                                        {
                                            return false; // if in range, return false
                                        }
                                    }

                                    l = p;

                                    if (c == p) // if char matches this element 
                                    {
                                        return false; // return false 
                                    }

                                    p = ConvertCase(pattern[pIndex++], caseSensitive);
                                }
                            }
                            // match if char is in set []
                            else
                            {
                                p = ConvertCase(pattern[pIndex++], caseSensitive);

                                while (pIndex < pattern.Length)
                                {
                                    if (p == ']') // if end of char set, then no match found 
                                    {
                                        return false;
                                    }

                                    if (p == '-')
                                    {
                                        // check a range of chars? 
                                        p = ConvertCase(pattern[pIndex], caseSensitive);

                                        // get high limit of range 
                                        if (pIndex > pattern.Length || p == ']')
                                        {
                                            return false; // syntax 
                                        }

                                        if (c >= l && c <= p)
                                        {
                                            break; // if in range, move on 
                                        }
                                    }

                                    l = p;

                                    if (c == p) // if char matches this element move on 
                                    {
                                        break;
                                    }

                                    p = ConvertCase(pattern[pIndex++], caseSensitive);
                                }

                                while (pIndex < pattern.Length && p != ']') // got a match in char set skip to end of set
                                {
                                    p = pattern[pIndex++];
                                }
                            }

                            break;
                        }

                    // match digit.
                    case '#':
                        {
                            c = target[tIndex++];

                            if (!char.IsDigit(c))
                            {
                                return false; // not a digit
                            }

                            break;
                        }

                    // match exact char.
                    default:
                        {
                            c = ConvertCase(target[tIndex++], caseSensitive);

                            if (c != p) // check for exact char
                            {
                                return false; // not a match
                            }

                            // check if end of pattern and still string data left.
                            if (pIndex >= pattern.Length && tIndex < target.Length - 1)
                            {
                                return false;
                            }

                            break;
                        }
                }
            }

            return true;
        }

        #endregion

        #region Private Methods

        private static char ConvertCase(char c, bool caseSensitive)
        {
            return (caseSensitive) ? c : char.ToUpper(c);
        }

        #endregion
    }
}
