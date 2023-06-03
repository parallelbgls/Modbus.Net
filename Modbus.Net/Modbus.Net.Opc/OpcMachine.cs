using System;
using System.Collections.Generic;

namespace Modbus.Net.Opc
{
    /// <summary>
    ///     Opc设备
    /// </summary>
    public class OpcMachine<TKey, TUnitKey> : BaseMachine<TKey, TUnitKey> where TKey : IEquatable<TKey>
        where TUnitKey : IEquatable<TUnitKey>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="id">设备的ID号</param>
        /// <param name="connectionType">连接类型</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="getAddresses">需要读写的地址</param>
        public OpcMachine(TKey id, OpcType connectionType, string connectionString, IEnumerable<AddressUnit<TUnitKey>> getAddresses)
            : base(id, getAddresses, true)
        {
            BaseUtility = new OpcUtility(connectionType, connectionString);
            AddressFormater = new AddressFormaterOpc<TKey, TUnitKey>((machine, unit) => { return new string[] { unit.Area }; }, this);
            AddressCombiner = new AddressCombinerSingle<TUnitKey>();
            AddressCombinerSet = new AddressCombinerSingle<TUnitKey>();
        }
    }
}