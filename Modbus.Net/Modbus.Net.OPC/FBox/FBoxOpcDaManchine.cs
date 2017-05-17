using System.Collections.Generic;
using System.Configuration;

namespace Modbus.Net.OPC.FBox
{
    public class FBoxOpcDaMachine : OpcDaMachine
    {
        public FBoxOpcDaMachine(string localSequence, string linkerName,
            IEnumerable<AddressUnit> getAddresses, bool keepConnect)
            : base(ConfigurationManager.AppSettings["FBoxOpcDaHost"], getAddresses, keepConnect)
        {
            LocalSequence = localSequence;
            LinkerName = linkerName;
            AddressFormater =
                new AddressFormaterOpc<string, string>(
                    (machine, unit) =>
                        new[]
                        {
                            "(.*)", ((FBoxOpcDaMachine) machine).LinkerName, ((FBoxOpcDaMachine) machine).LocalSequence,
                            unit.Name
                        }, this, '.');
        }

        public FBoxOpcDaMachine(string localSequence, string linkerName,
            IEnumerable<AddressUnit> getAddresses)
            : this(localSequence, linkerName, getAddresses, false)
        {
        }

        public string LocalSequence { get; set; }

        public string LinkerName { get; set; }
    }
}