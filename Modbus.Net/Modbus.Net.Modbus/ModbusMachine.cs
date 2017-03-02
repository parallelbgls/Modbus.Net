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
    public class ModbusMachine : BaseMachine
    {
        public ModbusMachine(ModbusType connectionType, string connectionString,
            IEnumerable<AddressUnit> getAddresses, bool keepConnect, byte slaveAddress, byte masterAddress,
            Endian endian = Endian.BigEndianLsb)
            : base(getAddresses, keepConnect, slaveAddress, masterAddress)
        {
            BaseUtility = new ModbusUtility(connectionType, connectionString, slaveAddress, masterAddress, endian);
            AddressFormater = new AddressFormaterModbus();
            AddressCombiner = new AddressCombinerContinus(AddressTranslator);
            AddressCombinerSet = new AddressCombinerContinus(AddressTranslator);
        }

        public ModbusMachine(ModbusType connectionType, string connectionString,
            IEnumerable<AddressUnit> getAddresses, byte slaveAddress, byte masterAddress,
            Endian endian = Endian.BigEndianLsb)
            : this(connectionType, connectionString, getAddresses, false, slaveAddress, masterAddress, endian)
        {
        }
    }
}