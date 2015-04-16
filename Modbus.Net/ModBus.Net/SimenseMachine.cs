using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net
{
    public class SimenseMachine : BaseMachine
    {
        public SimenseMachine(SimenseType connectionType, string connectionString,
            IEnumerable<AddressUnit> getAddresses, bool keepConnect) : base(getAddresses, keepConnect)
        {
            BaseUtility = new SimenseUtility(connectionType, connectionString);
            AddressFormater = new AddressFormaterSimense();
            AddressCombiner = new AddressCombinerContinus();
        }

        public SimenseMachine(SimenseType connectionType, string connectionString,
            IEnumerable<AddressUnit> getAddresses)
            : this(connectionType, connectionString, getAddresses, false)
        {
        }
    }
}
