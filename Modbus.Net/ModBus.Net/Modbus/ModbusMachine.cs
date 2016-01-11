using System.Collections.Generic;

namespace ModBus.Net.Modbus
{
    public class ModbusMachine : BaseMachine
    {
        public ModbusMachine(ModbusType connectionType, string connectionString,
            IEnumerable<AddressUnit> getAddresses, bool keepConnect) : base(getAddresses, keepConnect)
        {
            BaseUtility = new ModbusUtility(connectionType, connectionString);
            AddressFormater = new AddressFormaterModbus();
            AddressCombiner = new AddressCombinerContinus();
        }

        public ModbusMachine(ModbusType connectionType, string connectionString,
            IEnumerable<AddressUnit> getAddresses)
            : this(connectionType, connectionString, getAddresses, false)
        {
        }
    }
}
