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
using System.Diagnostics;
using Technosoftware.DaAeHdaClient.Utilities;
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// Manages the license to enable the different product versions.
    /// </summary>
    public class LicenseHandler
    {
        #region Nested Enums
        /// <summary>
        /// The possible products.
        /// </summary>
        [Flags]
        public enum ProductLicense : uint
        {
            /// <summary>
            /// No product selected
            /// </summary>
            None = 0,

            /// <summary>
            /// OPC DA/AE/HDA Client .NET
            /// </summary>
            Client = 1,

            /// <summary>
            /// OPC DA/AE Server .NET
            /// </summary>
            Server = 2,

            /// <summary>
            /// Evaluation
            /// </summary>
            Evaluation = 4,

            /// <summary>
            /// Expired Evaluation or License
            /// </summary>
            Expired = 8,
        }

        /// <summary>
        /// The possible products.
        /// </summary>
        [Flags]
        public enum ProductFeature : uint
        {
            /// <summary>
            /// Basic OPC Features enabled
            /// </summary>
            None = 0,

            /// <summary>
            /// OPC DataAccess enabled
            /// </summary>
            DataAccess = 1,

            /// <summary>
            /// OPC Alarms and Events enabled
            /// </summary>
            AlarmsConditions = 2,

            /// <summary>
            /// OPC Historical Dara Access enabled
            /// </summary>
            HistoricalAccess = 4,

            /// <summary>
            /// All supported OPC DA/AE/HDA Features enabled
            /// </summary>
            AllFeatures = uint.MaxValue,
        }
        #endregion

        #region Constants
        /// <summary>
        /// License Validation Parameters String for the OPC UA Solution .NET
        /// </summary> 
        private const string LicenseParameter =
            @"";
        #endregion

        #region Internal Fields
        internal static bool LicenseTraceDone;
        #endregion

        #region Properties
        /// <summary>
        /// Returns whether the product is a licensed product.
        /// </summary>
        /// <returns>Returns true if the product is licensed; false if it is used in evaluation mode or license is expired.</returns>
        public static bool IsLicensed
        {
            get
            {
                if ((LicensedProduct == ProductLicense.None) ||
                    ((LicensedProduct & ProductLicense.Expired) == ProductLicense.Expired) ||
                    ((LicensedProduct & ProductLicense.Evaluation) == ProductLicense.Evaluation))
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Returns whether the product is an evaluation version.
        /// </summary>
        /// <returns>Returns true if the product is an evaluation; false if it is a product or license is expired.</returns>
        public static bool IsEvaluation
        {
            get
            {
                if ((LicensedProduct & ProductLicense.Evaluation) == ProductLicense.Evaluation)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Indicates whether the evaluation period and a restart is required or not.
        /// </summary>
        public static bool IsExpired
        {
            get
            {
                if ((LicensedProduct & ProductLicense.Expired) == ProductLicense.Expired)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Returns the Version of the product.
        /// </summary>
        public static string Version
        {
            get
            {
                string versionString;

                try
                {
                    var assembly = (typeof(LicenseHandler).Assembly);

                    var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);

                    var major = versionInfo.FileMajorPart;
                    var minor = versionInfo.FileMinorPart;
                    var build = versionInfo.FileBuildPart;

                    versionString = $"{major}.{minor}.{build}";
                }
                catch (Exception)
                {

                    versionString = "Unknown";
                }
                return versionString;
            }
        }

        /// <summary>
        /// Returns the licensed products.
        /// </summary>
        public static ProductLicense LicensedProduct { get; set; } = ProductLicense.Client;

        /// <summary>
        /// Returns the licensed OPC UA Features.
        /// </summary>
        public static ProductFeature LicensedFeatures { get; set; } = ProductFeature.AllFeatures;

        /// <summary>
        /// Returns the licensed product name.
        /// </summary>
        public static string Product
        {
            get
            {
                var product = "Expired Evaluation or License";

                if ((LicensedProduct & ProductLicense.Expired) == ProductLicense.Expired)
                {
                    // It's an expired evaluation or license
                    if (((LicensedProduct & ProductLicense.Client) == ProductLicense.Client) &&
                        ((LicensedProduct & ProductLicense.Server) == ProductLicense.Server))
                    {
                        product = "Expired OPC DA/AE/HDA Bundle .NET license";
                    }
                    else if ((LicensedProduct & ProductLicense.Client) == ProductLicense.Client)
                    {
                        product = "Expired OPC DA/AE/HDA Client .NET license";
                    }
                    else if ((LicensedProduct & ProductLicense.Server) == ProductLicense.Server)
                    {
                        product = "Expired OPC DA/HDA Server .NET license";
                    }
                    else if ((LicensedProduct & ProductLicense.Evaluation) == ProductLicense.Evaluation)
                    {
                        product = "Expired OPC DA/AE/HDA Bundle .NET Evaluation";
                    }
                    return product;
                }

                // It's a license or evaluation
                if (((LicensedProduct & ProductLicense.Client) == ProductLicense.Client) &&
                     ((LicensedProduct & ProductLicense.Server) == ProductLicense.Server))
                {
                    product = "OPC DA/AE/HDAUA Bundle .NET";
                }
                else if ((LicensedProduct & ProductLicense.Client) == ProductLicense.Client)
                {
                    product = "OPC DA/AE/HDA Client .NET";
                }
                else if ((LicensedProduct & ProductLicense.Server) == ProductLicense.Server)
                {
                    product = "OPC DA/AE Server .NET";
                }
                else if ((LicensedProduct & ProductLicense.Evaluation) == ProductLicense.Evaluation)
                {
                    product = "OPC DA/AE/HDA Bundle .NET Evaluation";
                }

                return product;
            }
        }

        /// <summary>
        /// Returns the product information.
        /// </summary>
        public static string ProductInformation
        {
            get
            {
                if (IsLicensed)
                {
                    return Product;
                }
                if (!Check())
                {
                    return Product + " EVALUATION EXPIRED !!!";
                }
                return Product + " EVALUATION";
            }
        }

        internal static bool Checked { get; private set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Validate the license.
        /// </summary>
        /// <param name="serialNumber">Serial Number</param>
        public static bool Validate(string serialNumber)
        {
            return CheckLicense(serialNumber);
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Validate the license.
        /// </summary>
        /// <param name="serialNumber">Serial Number</param>
        protected static bool CheckLicense(string serialNumber)
        {
            var check = CheckLicenseClient(serialNumber);
            CheckProductFeature(serialNumber);

            if (check)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if the licensed product provided through ValidateLicense qualifies for the given application type and license edition.
        /// </summary>
        /// <returns>True if the license qualifies for the requested application and edition or if the evaluation period is still running; otherwise False.</returns>
        protected static bool CheckLicense()
        {
            if (Checked && LicensedProduct == ProductLicense.None)
            {
                return false;
            }

            if (Checked &&
               ((LicensedProduct & ProductLicense.Client) == ProductLicense.Client))
            {
                return true;
            }
            CheckProductFeature("");
            return Check();
        }

        #endregion

        #region Internal Methods
        /// <summary>
        /// Core Feature validation
        /// </summary>
        /// <returns>True if valid; false otherwise</returns>
        /// <exception cref="BadInternalErrorException"></exception>
        public static void ValidateFeatures(ProductFeature requiredProductFeature = ProductFeature.None, bool silent = false)
        {
            var valid = CheckLicense();

            if (!LicenseTraceDone)
            {
                LicenseTraceDone = true;
                Utils.Trace("Used Product = {0}, Features = {1}, Version = {2}.", Product, LicensedFeatures, Version);
            }

            if (!valid && !IsLicensed && !silent)
            {
                throw new BadInternalErrorException("Evaluation time expired! You need to restart the application.");
            }
            if (!valid && !silent)
            {
                throw new BadInternalErrorException("License required! You can't use this feature.");
            }

            if (requiredProductFeature != ProductFeature.None && LicensedFeatures != ProductFeature.AllFeatures)
            {
                if (((requiredProductFeature & LicensedFeatures) != requiredProductFeature) && !silent)
                {
                    var message =
                        $"Feature {requiredProductFeature} required but only {LicensedFeatures} licensed! You can't use this feature.";
                    throw new BadInternalErrorException(message);
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Validate the license.
        /// </summary>
        /// <param name="licenseKey">The license key</param>
        protected static void CheckProductFeature(string licenseKey)
        {
        }

        /// <summary>
        /// Validate the license.
        /// </summary>
        /// <param name="licenseKey">The license key</param>
        protected static bool CheckLicenseClient(string licenseKey)
        {
            return true;
        }

        internal static bool Check()
        {
            return true;
        }
        #endregion
    }
}
