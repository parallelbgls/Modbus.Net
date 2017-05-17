using System;
using System.Collections.Generic;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     OpcDa设备
    /// </summary>
    public abstract class OpcMachine<TKey, TUnitKey> : BaseMachine<TKey, TUnitKey> where TKey : IEquatable<TKey>
        where TUnitKey : IEquatable<TUnitKey>
    {
        protected OpcMachine(IEnumerable<AddressUnit<TUnitKey>> getAddresses, bool keepConnect)
            : base(getAddresses, keepConnect)
        {
            AddressCombiner = new AddressCombinerSingle<TUnitKey>();
            AddressCombinerSet = new AddressCombinerSingle<TUnitKey>();
        }
    }

    /// <summary>
    ///     OpcDa设备
    /// </summary>
    public abstract class OpcMachine : BaseMachine
    {
        protected OpcMachine(IEnumerable<AddressUnit> getAddresses, bool keepConnect)
            : base(getAddresses, keepConnect)
        {
            AddressCombiner = new AddressCombinerSingle();
            AddressCombinerSet = new AddressCombinerSingle();
        }
    }
}