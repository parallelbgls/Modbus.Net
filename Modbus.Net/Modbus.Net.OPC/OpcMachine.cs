using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     OpcDa设备
    /// </summary>
    public abstract class OpcMachine<TKey, TUnitKey> : BaseMachine<TKey, TUnitKey> where TKey : IEquatable<TKey>
        where TUnitKey : IEquatable<TUnitKey>
    {
        public OpcMachine(string connectionString, IEnumerable<AddressUnit<TUnitKey>> getAddresses, bool keepConnect)
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
        public OpcMachine(string connectionString, IEnumerable<AddressUnit> getAddresses, bool keepConnect)
            : base(getAddresses, keepConnect)
        {
            AddressCombiner = new AddressCombinerSingle();
            AddressCombinerSet = new AddressCombinerSingle();
        }
    }
}
