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
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// A description of an interface version defined by an OPC specification.
    /// </summary>
    [Serializable]
    public struct OpcSpecification
    {
        #region Constants
        /// <summary>OPC Alarms&amp;Events 1.0 and OPC ALarms&amp;Events 1.1.</summary>
        public static readonly OpcSpecification OPC_AE_10 = new OpcSpecification("58E13251-AC87-11d1-84D5-00608CB8A7E9", "Alarms and Event 1.XX");

        /// <summary>OPC Data Access 1.0.</summary>
        public static readonly OpcSpecification OPC_DA_10 = new OpcSpecification("63D5F430-CFE4-11d1-B2C8-0060083BA1FB", "Data Access 1.0a");
        /// <summary>OPC Data Access 2.0.</summary>
        public static readonly OpcSpecification OPC_DA_20 = new OpcSpecification("63D5F432-CFE4-11d1-B2C8-0060083BA1FB", "Data Access 2.XX");
        /// <summary>OPC Data Access 3.0.</summary>
        public static readonly OpcSpecification OPC_DA_30 = new OpcSpecification("CC603642-66D7-48f1-B69A-B625E73652D7", "Data Access 3.00");
        /// <summary>OPC XML-DA 1.0.</summary>
        public static readonly OpcSpecification XML_DA_10 = new OpcSpecification("3098EDA4-A006-48b2-A27F-247453959408", "XML Data Access 1.00");

        /// <summary>OPC Historical Data Access 1.0.</summary>
        public static readonly OpcSpecification OPC_HDA_10 = new OpcSpecification("7DE5B060-E089-11d2-A5E6-000086339399", "Historical Data Access 1.XX");
        #endregion

        #region Fields
        private string id_;
        private string description_;
        #endregion

        #region Constructors, Destructor, Initialization
        /// <summary>
        /// Initializes the object with the description and a GUID as a string.
        /// </summary>
        public OpcSpecification(string id, string description)
        {
            id_ = id;
            description_ = description;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The unique identifier for the interface version. 
        /// </summary>
        public string Id
        {
            get => id_;
            set => id_ = value;
        }

        /// <summary>
        /// The human readable description for the interface version.
        /// </summary>
        public string Description
        {
            get => description_;
            set => description_ = value;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns true if the objects are equal.
        /// </summary>
        public static bool operator ==(OpcSpecification a, OpcSpecification b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Returns true if the objects are not equal.
        /// </summary>
        public static bool operator !=(OpcSpecification a, OpcSpecification b)
        {
            return !a.Equals(b);
        }
        #endregion

        #region Object Member Overrides
        /// <summary>
        /// Determines if the object is equal to the specified value.
        /// </summary>
        public override bool Equals(object target)
        {
            if (target != null && (target is OpcSpecification))
            {
                return (Id == ((OpcSpecification)target).Id);
            }

            return false;
        }

        /// <summary>
        /// Converts the object to a string used for display.
        /// </summary>
        public override string ToString()
        {
            return Description;
        }

        /// <summary>
        /// Returns a suitable hash code for the result.
        /// </summary>
        public override int GetHashCode()
        {
            return (Id != null) ? Id.GetHashCode() : base.GetHashCode();
        }
        #endregion
    }
}
