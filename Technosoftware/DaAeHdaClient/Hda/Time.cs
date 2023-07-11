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
using System.Text;
#endregion

namespace Technosoftware.DaAeHdaClient.Hda
{
    /// <summary>
    /// A time specified as either an absolute or relative value.
    /// </summary>
    [Serializable]
    public class TsCHdaTime
    {
        #region Fields

        private DateTime absoluteTime_ = DateTime.MinValue;
        private TsCHdaRelativeTime baseTime_ = TsCHdaRelativeTime.Now;
        private TsCHdaTimeOffsetCollection offsets_ = new TsCHdaTimeOffsetCollection();
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes the object with its default values.
        /// </summary>
        public TsCHdaTime() { }

        /// <summary>
        /// Initializes the object with an absolute time.
        /// </summary>
        /// <param name="time">The absolute time.</param>
        public TsCHdaTime(DateTime time)
        {
            AbsoluteTime = time;
        }

        /// <summary>
        /// Initializes the object with a relative time.
        /// </summary>
        /// <param name="time">The relative time.</param>
        public TsCHdaTime(string time)
        {
            var value = Parse(time);

            absoluteTime_ = DateTime.MinValue;
            baseTime_ = value.baseTime_;
            offsets_ = value.offsets_;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether the time is a relative or absolute time.
        /// </summary>
        public bool IsRelative
        {
            get => (absoluteTime_ == DateTime.MinValue);
            set => absoluteTime_ = DateTime.MinValue;
        }

        /// <summary>
        /// The time as absolute value.
        /// The <see cref="ApplicationInstance.TimeAsUtc">ApplicationInstance.TimeAsUtc</see> property defines
        /// the time format (UTC or local time).
        /// </summary>
        public DateTime AbsoluteTime
        {
            get => absoluteTime_;
            set => absoluteTime_ = value;
        }

        /// <summary>
        /// The base for a relative time value.
        /// </summary>
        public TsCHdaRelativeTime BaseTime
        {
            get => baseTime_;
            set => baseTime_ = value;
        }

        /// <summary>
        /// The set of offsets to be applied to the base of a relative time.
        /// </summary>
        public TsCHdaTimeOffsetCollection Offsets => offsets_;
        #endregion

        #region Public Methods
        /// <summary>
        /// Converts a relative time to an absolute time by using the system clock.
        /// </summary>
        public DateTime ResolveTime()
        {
            // nothing special to do for absolute times.
            if (!IsRelative)
            {
                return absoluteTime_;
            }

            // get local time from the system.
            var time = DateTime.UtcNow;

            var years = time.Year;
            var months = time.Month;
            var days = time.Day;
            var hours = time.Hour;
            var minutes = time.Minute;
            var seconds = time.Second;
            var milliseconds = time.Millisecond;

            // move to the beginning of the period indicated by the base time.
            switch (BaseTime)
            {
                case TsCHdaRelativeTime.Year:
                    {
                        months = 0;
                        days = 0;
                        hours = 0;
                        minutes = 0;
                        seconds = 0;
                        milliseconds = 0;
                        break;
                    }

                case TsCHdaRelativeTime.Month:
                    {
                        days = 0;
                        hours = 0;
                        minutes = 0;
                        seconds = 0;
                        milliseconds = 0;
                        break;
                    }

                case TsCHdaRelativeTime.Week:
                case TsCHdaRelativeTime.Day:
                    {
                        hours = 0;
                        minutes = 0;
                        seconds = 0;
                        milliseconds = 0;
                        break;
                    }

                case TsCHdaRelativeTime.Hour:
                    {
                        minutes = 0;
                        seconds = 0;
                        milliseconds = 0;
                        break;
                    }

                case TsCHdaRelativeTime.Minute:
                    {
                        seconds = 0;
                        milliseconds = 0;
                        break;
                    }

                case TsCHdaRelativeTime.Second:
                    {
                        milliseconds = 0;
                        break;
                    }
            }

            // construct base time.
            time = new DateTime(years, months, days, hours, minutes, seconds, milliseconds);

            // adjust to beginning of week.
            if (BaseTime == TsCHdaRelativeTime.Week && time.DayOfWeek != DayOfWeek.Sunday)
            {
                time = time.AddDays(-((int)time.DayOfWeek));
            }

            // add offsets.
            foreach (TsCHdaTimeOffset offset in Offsets)
            {
                switch (offset.Type)
                {
                    case TsCHdaRelativeTime.Year: { time = time.AddYears(offset.Value); break; }
                    case TsCHdaRelativeTime.Month: { time = time.AddMonths(offset.Value); break; }
                    case TsCHdaRelativeTime.Week: { time = time.AddDays(offset.Value * 7); break; }
                    case TsCHdaRelativeTime.Day: { time = time.AddDays(offset.Value); break; }
                    case TsCHdaRelativeTime.Hour: { time = time.AddHours(offset.Value); break; }
                    case TsCHdaRelativeTime.Minute: { time = time.AddMinutes(offset.Value); break; }
                    case TsCHdaRelativeTime.Second: { time = time.AddSeconds(offset.Value); break; }
                }
            }

            // return resolved time.
            return time;
        }

        /// <summary>
        /// Returns a String that represents the current Object.
        /// </summary>
        /// <returns>A String that represents the current Object.</returns>
        public override string ToString()
        {
            if (!IsRelative)
            {
                return OpcConvert.ToString(absoluteTime_);
            }

            var buffer = new StringBuilder(256);

            buffer.Append(BaseTypeToString(BaseTime));
            buffer.Append(Offsets);

            return buffer.ToString();
        }

        /// <summary>
        /// Parses a string representation of a time.
        /// </summary>
        /// <param name="buffer">The string representation to parse.</param>
        /// <returns>A Time object initialized with the string.</returns>
        public static TsCHdaTime Parse(string buffer)
        {
            // remove trailing and leading white spaces.
            buffer = buffer.Trim();

            var time = new TsCHdaTime();

            // determine if string is a relative time.
            var isRelative = false;

            foreach (TsCHdaRelativeTime baseTime in Enum.GetValues(typeof(TsCHdaRelativeTime)))
            {
                var token = BaseTypeToString(baseTime);

                if (buffer.StartsWith(token))
                {
                    buffer = buffer.Substring(token.Length).Trim();
                    time.BaseTime = baseTime;
                    isRelative = true;
                    break;
                }
            }

            // parse an absolute time string.
            if (!isRelative)
            {
                time.AbsoluteTime = Convert.ToDateTime(buffer).ToUniversalTime();
                return time;
            }

            // parse the offset portion of the relative time.
            if (buffer.Length > 0)
            {
                time.Offsets.Parse(buffer);
            }

            return time;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Converts a base time to a string token.
        /// </summary>
        /// <param name="baseTime">The base time value to convert.</param>
        /// <returns>The string token representing the base time.</returns>
        private static string BaseTypeToString(TsCHdaRelativeTime baseTime)
        {
            switch (baseTime)
            {
                case TsCHdaRelativeTime.Now: { return "NOW"; }
                case TsCHdaRelativeTime.Second: { return "SECOND"; }
                case TsCHdaRelativeTime.Minute: { return "MINUTE"; }
                case TsCHdaRelativeTime.Hour: { return "HOUR"; }
                case TsCHdaRelativeTime.Day: { return "DAY"; }
                case TsCHdaRelativeTime.Week: { return "WEEK"; }
                case TsCHdaRelativeTime.Month: { return "MONTH"; }
                case TsCHdaRelativeTime.Year: { return "YEAR"; }
            }

            throw new ArgumentOutOfRangeException(nameof(baseTime), baseTime.ToString(), @"Invalid value for relative base time.");
        }
        #endregion
    }
}
