using System.Collections.Generic;

namespace ModBus.Net.Simense
{
    public class SimenseMachine : BaseMachine
    {
        public SimenseMachine(SimenseType connectionType, string connectionString, SimenseMachineModel model,
            IEnumerable<AddressUnit> getAddresses, bool keepConnect) : base(getAddresses, keepConnect)
        {
            BaseUtility = new SimenseUtility(connectionType, connectionString, model);
            AddressFormater = new AddressFormaterSimense();
            AddressCombiner = new AddressCombinerContinus();
        }

        public SimenseMachine(SimenseType connectionType, string connectionString, SimenseMachineModel model,
            IEnumerable<AddressUnit> getAddresses)
            : this(connectionType, connectionString, model, getAddresses, false)
        {
        }
    }
}
