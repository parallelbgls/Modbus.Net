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

namespace Technosoftware.DaAeHdaClient.Hda
{
    /// <summary>
    /// Defines constants for well-known item aggregates.
    /// </summary>
    /// <remarks>This indicates the aggregate to be used when retrieving processed history. The precise meaning of each aggregate may be server specific. Aggregates not supported by the server shall return E_INVALIDARG in the error code for that aggregate. Additional aggregates may be defined by vendors. Server specific aggregates must be defined with values beginning at 0x80000000. The OPC foundation reserves all aggregates IDs from 0 to 0x7fffffff.</remarks>
    public class TsCHdaAggregateID
    {
        #region Constants
        /// <summary>
        /// Do not retrieve an aggregate.
        /// </summary>
        public const int NoAggregate = 0;
        /// <summary>
        /// Do not retrieve an aggregate. This is used for retrieving interpolated values.
        /// </summary>
        public const int Interpolative = 1;
        /// <summary>
        /// Retrieve the totalized value (time integral) of the data over the re-sample interval.
        /// </summary>
        public const int Total = 2;
        /// <summary>
        /// Retrieve the average data over the re-sample interval.
        /// </summary>
        public const int Average = 3;
        /// <summary>
        /// Retrieve the time weighted average data over the re-sample interval.
        /// </summary>
        public const int TimeAverage = 4;
        /// <summary>
        /// Retrieve the number of raw values over the re-sample interval.
        /// </summary>
        public const int Count = 5;
        /// <summary>
        /// Retrieve the standard deviation over the re-sample interval.
        /// </summary>
        public const int StandardDeviation = 6;
        /// <summary>
        /// Retrieve the minimum value in the re-sample interval and the timestamp of the minimum value.
        /// </summary>
        public const int MinimumActualTime = 7;
        /// <summary>
        /// Retrieve the minimum value in the re-sample interval.
        /// </summary>
        public const int Minimum = 8;
        /// <summary>
        /// Retrieve the maximum value in the re-sample interval and the timestamp of the maximum value.
        /// </summary>
        public const int MaximumActualTime = 9;
        /// <summary>
        /// Retrieve the maximum value in the re-sample interval.
        /// </summary>
        public const int Maximum = 10;
        /// <summary>
        /// Retrieve the value at the beginning of the re-sample interval. The time stamp is the time stamp of the beginning of the interval.
        /// </summary>
        public const int Start = 11;
        /// <summary>
        /// Retrieve the value at the end of the re-sample interval. The time stamp is the time stamp of the end of the interval. 
        /// </summary>
        public const int End = 12;
        /// <summary>
        /// Retrieve the difference between the first and last value in the re-sample interval.
        /// </summary>
        public const int Delta = 13;
        /// <summary>
        /// Retrieve the slope of the regression line over the re-sample interval.
        /// </summary>
        public const int RegSlope = 14;
        /// <summary>
        /// Retrieve the intercept of the regression line over the re-sample interval. This is the value of the regression line at the start of the interval.
        /// </summary>
        public const int RegConst = 15;
        /// <summary>
        /// Retrieve the standard deviation of the regression line over the re-sample interval.
        /// </summary>
        public const int RegDev = 16;
        /// <summary>
        /// Retrieve the variance over the sample interval.
        /// </summary>
        public const int Variance = 17;
        /// <summary>
        /// Retrieve the difference between the minimum and maximum value over the sample interval.
        /// </summary>
        public const int Range = 18;
        /// <summary>
        /// Retrieve the duration (in seconds) of time in the interval during which the data is good.
        /// </summary>
        public const int DurationGood = 19;
        /// <summary>
        /// Retrieve the duration (in seconds) of time in the interval during which the data is bad.
        /// </summary>
        public const int DurationBad = 20;
        /// <summary>
        /// Retrieve the percent of data (1 equals 100 percent) in the interval which has good quality.
        /// </summary>
        public const int PercentGood = 21;
        /// <summary>
        /// Retrieve the percent of data (1 equals 100 percent) in the interval which has bad quality.
        /// </summary>
        public const int PercentBad = 22;
        /// <summary>
        /// Retrieve the worst quality of data in the interval.
        /// </summary>
        public const int WorstQuality = 23;
        /// <summary>
        /// Retrieve the number of annotations in the interval.
        /// </summary>
        public const int Annotations = 24;
        #endregion

        #region Constructors, Destructor, Initialization
        private TsCHdaAggregateID() { }
        #endregion
    }
}
