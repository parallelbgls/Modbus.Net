using System;
using System.Collections.Generic;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     OpcDa设备
    /// </summary>
    public class OpcDaMachine<TKey, TUnitKey> : BaseMachine<TKey, TUnitKey> where TKey : IEquatable<TKey>
        where TUnitKey : IEquatable<TUnitKey>
    {
        public OpcDaMachine(string connectionString, IEnumerable<AddressUnit<TUnitKey>> getAddresses, bool keepConnect)
            : base(getAddresses, keepConnect)
        {
            BaseUtility = new OpcDaUtility(connectionString);
            AddressCombiner = new AddressCombinerSingle<TUnitKey>();
            AddressCombinerSet = new AddressCombinerSingle<TUnitKey>();
        }

        public OpcDaMachine(string connectionString, IEnumerable<AddressUnit<TUnitKey>> getAddresses)
            : this(connectionString, getAddresses, false)
        {
        }
    }

    /// <summary>
    ///     OpcDa设备
    /// </summary>
    public class OpcDaMachine : OpcDaMachine<string, string>
    {
        public OpcDaMachine(string connectionString, IEnumerable<AddressUnit<string>> getAddresses, bool keepConnect)
            : base(connectionString, getAddresses, keepConnect)
        {
        }

        public OpcDaMachine(string connectionString, IEnumerable<AddressUnit<string>> getAddresses)
            : base(connectionString, getAddresses)
        {
        }
    }
}