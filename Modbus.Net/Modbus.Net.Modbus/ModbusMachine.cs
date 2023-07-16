using System;
using System.Collections.Generic;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus设备
    /// </summary>
    public class ModbusMachine<TKey, TUnitKey> : BaseMachine<TKey, TUnitKey> where TKey : IEquatable<TKey>
        where TUnitKey : IEquatable<TUnitKey>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="id">设备的ID号</param>
        /// <param name="connectionType">连接类型</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="getAddresses">读写的地址</param>
        /// <param name="keepConnect">是否保持连接</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="endian">端格式</param>
        public ModbusMachine(TKey id, ModbusType connectionType, string connectionString,
            IEnumerable<AddressUnit<TUnitKey, int, int>> getAddresses, bool keepConnect, byte slaveAddress, byte masterAddress,
            Endian endian)
            : base(id, getAddresses, keepConnect, slaveAddress, masterAddress)
        {
            BaseUtility = new ModbusUtility(connectionType, connectionString, slaveAddress, masterAddress, endian);
            AddressFormater = new AddressFormaterModbus();
            AddressCombiner = new AddressCombinerContinus<TUnitKey>(AddressTranslator, 100);
            AddressCombinerSet = new AddressCombinerContinus<TUnitKey>(AddressTranslator, 100);
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="id">设备的ID号</param>
        /// <param name="connectionType">连接类型</param>
        /// <param name="connectionString">连接地址</param>
        /// <param name="getAddresses">读写的地址</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        /// <param name="endian">端格式</param>
        public ModbusMachine(TKey id, ModbusType connectionType, string connectionString,
            IEnumerable<AddressUnit<TUnitKey, int, int>> getAddresses, byte slaveAddress, byte masterAddress,
            Endian endian)
            : this(id, connectionType, connectionString, getAddresses, true, slaveAddress, masterAddress, endian)
        {
        }
    }

}