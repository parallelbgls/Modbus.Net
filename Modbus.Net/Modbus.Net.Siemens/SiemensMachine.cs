using System.Collections.Generic;

namespace Modbus.Net.Siemens
{
    public class SiemensMachine : BaseMachine
    {
        public SiemensMachine(SiemensType connectionType, string connectionString, SiemensMachineModel model,
            IEnumerable<AddressUnit> getAddresses, bool keepConnect) : base(getAddresses, keepConnect)
        {
            BaseUtility = new SiemensUtility(connectionType, connectionString, model);
            AddressFormater = new AddressFormaterSiemens();
            AddressCombiner = new AddressCombinerContinus();
            AddressCombinerSet = new AddressCombinerContinus();
        }

        public SiemensMachine(SiemensType connectionType, string connectionString, SiemensMachineModel model,
            IEnumerable<AddressUnit> getAddresses)
            : this(connectionType, connectionString, model, getAddresses, false)
        {
        }
    }
}
