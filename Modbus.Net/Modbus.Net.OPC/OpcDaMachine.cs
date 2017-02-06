using System.Collections.Generic;

namespace Modbus.Net.OPC
{
    public class OpcDaMachine : BaseMachine
    {
        public OpcDaMachine(string connectionString, IEnumerable<AddressUnit> getAddresses, bool keepConnect)
            : base(getAddresses, keepConnect)
        {
            BaseUtility = new OpcDaUtility(connectionString);
            AddressCombiner = new AddressCombinerSingle();
            AddressCombinerSet = new AddressCombinerSingle();
        }

        public OpcDaMachine(string connectionString, IEnumerable<AddressUnit> getAddresses)
            : this(connectionString, getAddresses, false)
        {
        }
    }
}