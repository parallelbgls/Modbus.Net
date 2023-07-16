using System;
using System.Collections.Generic;

namespace Modbus.Net.Opc
{
    /// <summary>
    ///     Opc设备
    /// </summary>
    public class OpcMachine<TKey, TUnitKey> : BaseMachine<TKey, TUnitKey, string, string> where TKey : IEquatable<TKey>
        where TUnitKey : IEquatable<TUnitKey>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="id">设备的ID号</param>
        /// <param name="connectionType">连接类型</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="getAddresses">需要读写的地址</param>
        /// <param name="tagSpliter">连接各个字段用的符号</param>
        public OpcMachine(TKey id, OpcType connectionType, string connectionString, IEnumerable<AddressUnit<TUnitKey, string, string>> getAddresses, string tagSpliter)
            : base(id, getAddresses, true)
        {
            BaseUtility = new OpcUtility(connectionType, connectionString);
            AddressFormater = new AddressFormaterOpc<TKey, TUnitKey, string, string>((machine, unit) => { var ans = unit.Area; if (unit.Address != null) ans += tagSpliter + unit.Address; if (unit.SubAddress != null) ans += tagSpliter + unit.SubAddress; return ans; }, this);
        }
    }
}