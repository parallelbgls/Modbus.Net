using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net.FBox
{
    public class AddressCombinerFBox : AddressCombiner
    {
        public override IEnumerable<CommunicationUnit> Combine(IEnumerable<AddressUnit> addresses)
        {
            return (from address in addresses
                    select
                        new CommunicationUnit()
                        {
                            Area = address.Area,
                            Address = address.Address,
                            GetCount = 1,
                            DataType = address.DataType
                        });
        }
    }
}
