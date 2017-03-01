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
        public ModbusMachine(ModbusType connectionType, string connectionString,
            IEnumerable<AddressUnit<TUnitKey>> getAddresses, bool keepConnect, byte slaveAddress, byte masterAddress,
            Endian endian = Endian.BigEndianLsb)
            : base(getAddresses, keepConnect, slaveAddress, masterAddress)
        {
            BaseUtility = new ModbusUtility(connectionType, connectionString, slaveAddress, masterAddress, endian);
            AddressFormater = new AddressFormaterModbus();
            AddressCombiner = new AddressCombinerContinus<TUnitKey>(AddressTranslator);
            AddressCombinerSet = new AddressCombinerContinus<TUnitKey>(AddressTranslator);
        }

        public ModbusMachine(ModbusType connectionType, string connectionString,
            IEnumerable<AddressUnit<TUnitKey>> getAddresses, byte slaveAddress, byte masterAddress,
            Endian endian = Endian.BigEndianLsb)
            : this(connectionType, connectionString, getAddresses, false, slaveAddress, masterAddress, endian)
        {
        }
    }

    /// <summary>
    ///     Modbus设备
    /// </summary>
    public class ModbusMachine : ModbusMachine<string, string>
    {
        public ModbusMachine(ModbusType connectionType, string connectionString,
            IEnumerable<AddressUnit<string>> getAddresses,
            bool keepConnect, byte slaveAddress, byte masterAddress, Endian endian = Endian.BigEndianLsb)
            : base(connectionType, connectionString, getAddresses, keepConnect, slaveAddress, masterAddress, endian)
        {
        }

        public ModbusMachine(ModbusType connectionType, string connectionString,
            IEnumerable<AddressUnit<string>> getAddresses,
            byte slaveAddress, byte masterAddress, Endian endian = Endian.BigEndianLsb)
            : base(connectionType, connectionString, getAddresses, slaveAddress, masterAddress, endian)
        {
        }
    }
}