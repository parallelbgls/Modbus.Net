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
#endregion

namespace Technosoftware.DaAeHdaClient.Hda
{
    /// <summary>
    /// A collection of time offsets used in a relative time.
    /// </summary>
    [Serializable]
    public class TsCHdaTimeOffsetCollection : ArrayList
    {
        #region Properties
        /// <summary>
        /// Accessor for elements in the time offset collection.
        /// </summary>
        public new TsCHdaTimeOffset this[int index]
        {
            get => this[index];
            set => this[index] = value;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a new offset to the collection.
        /// </summary>
        /// <param name="value">The offset value.</param>
        /// <param name="type">The offset type.</param>
        public int Add(int value, TsCHdaRelativeTime type)
        {
            var offset = new TsCHdaTimeOffset { Value = value, Type = type };

            return base.Add(offset);
        }

        /// <summary>
        /// Returns a String that represents the current Object.
        /// </summary>
        /// <returns>A String that represents the current Object.</returns>
        public override string ToString()
        {
            var buffer = new StringBuilder(256);

            foreach (TsCHdaTimeOffset offset in (ICollection)this)
            {
                if (offset.Value >= 0)
                {
                    buffer.Append("+");
                }

                buffer.AppendFormat("{0}", offset.Value);
                buffer.Append(TsCHdaTimeOffset.OffsetTypeToString(offset.Type));
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Initializes the collection from a set of offsets contained in a string. 
        /// </summary>
        /// <param name="buffer">A string containing the time offset fields.</param>
        public void Parse(string buffer)
        {
            // clear existing offsets.
            Clear();

            // parse the offsets.
            var positive = true;
            var magnitude = 0;
            var units = "";
            var state = 0;

            // state = 0 - looking for start of next offset field.
            // state = 1 - looking for beginning of offset value.
            // state = 2 - reading offset value.
            // state = 3 - reading offset type.

            for (var ii = 0; ii < buffer.Length; ii++)
            {
                // check for sign part of the offset field.
                if (buffer[ii] == '+' || buffer[ii] == '-')
                {
                    if (state == 3)
                    {
                        Add(CreateOffset(positive, magnitude, units));

                        magnitude = 0;
                        units = "";
                        state = 0;
                    }

                    if (state != 0)
                    {
                        throw new FormatException("Unexpected token encountered while parsing relative time string.");
                    }

                    positive = buffer[ii] == '+';
                    state = 1;
                }

                // check for integer part of the offset field.
                else if (char.IsDigit(buffer, ii))
                {
                    if (state == 3)
                    {
                        Add(CreateOffset(positive, magnitude, units));

                        magnitude = 0;
                        units = "";
                        state = 0;
                    }

                    if (state != 0 && state != 1 && state != 2)
                    {
                        throw new FormatException("Unexpected token encountered while parsing relative time string.");
                    }

                    magnitude *= 10;
                    magnitude += Convert.ToInt32(buffer[ii] - '0');

                    state = 2;
                }

                // check for units part of the offset field.
                else if (!char.IsWhiteSpace(buffer, ii))
                {
                    if (state != 2 && state != 3)
                    {
                        throw new FormatException("Unexpected token encountered while parsing relative time string.");
                    }

                    units += buffer[ii];
                    state = 3;
                }
            }

            // process final field.
            if (state == 3)
            {
                Add(CreateOffset(positive, magnitude, units));
                state = 0;
            }

            // check final state.
            if (state != 0)
            {
                throw new FormatException("Unexpected end of string encountered while parsing relative time string.");
            }
        }
        #endregion

        #region ICollection Members
        /// <summary>
        /// Copies the objects to an Array, starting at a the specified index.
        /// </summary>
        /// <param name="array">The one-dimensional Array that is the destination for the objects.</param>
        /// <param name="index">The zero-based index in the Array at which copying begins.</param>
        public void CopyTo(TsCHdaTimeOffset[] array, int index)
        {
            CopyTo((Array)array, index);
        }
        #endregion

        #region IList Members
        /// <summary>
        /// Inserts an item to the IList at the specified position.
        /// </summary>
        /// <param name="index">The zero-based index at which value should be inserted.</param>
        /// <param name="value">The Object to insert into the IList. </param>
        public void Insert(int index, TsCHdaTimeOffset value)
        {
            Insert(index, (object)value);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the IList.
        /// </summary>
        /// <param name="value">The Object to remove from the IList.</param>
        public void Remove(TsCHdaTimeOffset value)
        {
            Remove((object)value);
        }

        /// <summary>
        /// Determines whether the IList contains a specific value.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>true if the Object is found in the IList; otherwise, false.</returns>
        public bool Contains(TsCHdaTimeOffset value)
        {
            return Contains((object)value);
        }

        /// <summary>
        /// Determines the index of a specific item in the IList.
        /// </summary>
        /// <param name="value">The Object to locate in the IList.</param>
        /// <returns>The index of value if found in the list; otherwise, -1.</returns>
        public int IndexOf(TsCHdaTimeOffset value)
        {
            return IndexOf((object)value);
        }

        /// <summary>
        /// Adds an item to the IList.
        /// </summary>
        /// <param name="value">The Object to add to the IList. </param>
        /// <returns>The position into which the new element was inserted.</returns>
        public int Add(TsCHdaTimeOffset value)
        {
            return Add((object)value);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Creates a new offset object from the components extracted from a string.
        /// </summary>
        private static TsCHdaTimeOffset CreateOffset(bool positive, int magnitude, string units)
        {
            foreach (TsCHdaRelativeTime offsetType in Enum.GetValues(typeof(TsCHdaRelativeTime)))
            {
                if (offsetType == TsCHdaRelativeTime.Now)
                {
                    continue;
                }

                if (units == TsCHdaTimeOffset.OffsetTypeToString(offsetType))
                {
                    var offset = new TsCHdaTimeOffset { Value = (positive) ? magnitude : -magnitude, Type = offsetType };

                    return offset;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(units), units, @"String is not a valid offset time type.");
        }
        #endregion
    }
}
