using System;
using System.Collections.Generic;

namespace Modbus.Net.OPC
{
    public class OpcDaMachine<TKey, TUnitKey> : OpcMachine<TKey, TUnitKey> where TKey : IEquatable<TKey>
        where TUnitKey : IEquatable<TUnitKey>
    {
        public OpcDaMachine(string connectionString, IEnumerable<AddressUnit<TUnitKey>> getAddresses, bool keepConnect)
            : base(getAddresses, keepConnect)
        {
            BaseUtility = new OpcDaUtility(connectionString);
            ((OpcUtility) BaseUtility).GetSeperator +=
                () => ((AddressFormaterOpc<TKey, TUnitKey>) AddressFormater).Seperator;
        }

        public OpcDaMachine(string connectionString, IEnumerable<AddressUnit<TUnitKey>> getAddresses)
            : this(connectionString, getAddresses, false)
        {
        }
    }

    public class OpcDaMachine : OpcMachine
    {
        public OpcDaMachine(string connectionString, IEnumerable<AddressUnit> getAddresses, bool keepConnect)
            : base(getAddresses, keepConnect)
        {
            BaseUtility = new OpcDaUtility(connectionString);
            ((OpcUtility) BaseUtility).GetSeperator +=
                () => ((AddressFormaterOpc<string, string>) AddressFormater).Seperator;
        }

        public OpcDaMachine(string connectionString, IEnumerable<AddressUnit> getAddresses)
            : this(connectionString, getAddresses, false)
        {
        }
    }
}