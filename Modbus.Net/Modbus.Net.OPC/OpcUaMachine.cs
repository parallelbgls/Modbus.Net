using System;
using System.Collections.Generic;

namespace Modbus.Net.OPC
{
    public class OpcUaMachine<TKey, TUnitKey> : OpcMachine<TKey, TUnitKey> where TKey : IEquatable<TKey>
        where TUnitKey : IEquatable<TUnitKey>
    {
        public OpcUaMachine(string connectionString, IEnumerable<AddressUnit<TUnitKey>> getAddresses, bool keepConnect)
            : base(getAddresses, keepConnect)
        {
            BaseUtility = new OpcUaUtility(connectionString);
            ((OpcUtility) BaseUtility).GetSeperator +=
                () => ((AddressFormaterOpc<string, string>) AddressFormater).Seperator;
        }

        public OpcUaMachine(string connectionString, IEnumerable<AddressUnit<TUnitKey>> getAddresses)
            : this(connectionString, getAddresses, false)
        {
        }
    }

    public class OpcUaMachine : OpcMachine
    {
        public OpcUaMachine(string connectionString, IEnumerable<AddressUnit> getAddresses, bool keepConnect)
            : base(getAddresses, keepConnect)
        {
            BaseUtility = new OpcUaUtility(connectionString);
            ((OpcUtility) BaseUtility).GetSeperator +=
                () => ((AddressFormaterOpc<string, string>) AddressFormater).Seperator;
        }

        public OpcUaMachine(string connectionString, IEnumerable<AddressUnit> getAddresses)
            : this(connectionString, getAddresses, false)
        {
        }
    }
}