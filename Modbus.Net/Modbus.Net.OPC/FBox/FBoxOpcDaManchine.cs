using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.OPC.FBox
{
    public class FBoxOpcDaMachine : OpcDaMachine
    {
        public string LocalSequence { get; set; }

        public string LinkerName { get; set; }

        public FBoxOpcDaMachine(string localSequence, string linkerName,
            IEnumerable<AddressUnit> getAddresses, bool keepConnect) : base(ConfigurationManager.FBoxOpcDaHost, getAddresses, keepConnect)
        {
            LocalSequence = localSequence;
            LinkerName = linkerName;
            AddressFormater =
                new AddressFormaterOpc(
                    (machine, unit) =>
                        new string[]
                        {
                            "他人分享", ((FBoxOpcDaMachine) machine).LinkerName, ((FBoxOpcDaMachine) machine).LocalSequence,
                            unit.Name
                        }, this, '.');
        }

        public FBoxOpcDaMachine(string localSequence, string linkerName,
            IEnumerable<AddressUnit> getAddresses)
            : this(localSequence, linkerName, getAddresses, false)
        {
        }
    }
}
