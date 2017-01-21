using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.Tests
{
    public sealed class AddressMaker
    {
        public TestAreas TestAreas { get; set; }
        public TestAddresses TestAddresses { get; set; }

        private AddressFormater AddressFormater { get; set; }
        private AddressTranslator AddressTranslator { get; set; }

        public List<AddressUnit> MakeAddresses(string areaKey, string addressKey, bool isRead)
        {
            var combinedAddress = new List<string>();

            foreach (var area in TestAreas[areaKey])
            {
                foreach (var address in TestAddresses[addressKey])
                {
                    for (double currentAddress = address.Item1, i = 0;
                        i < address.Item2;
                        currentAddress +=
                            ValueHelper.Instance.ByteLength[address.Item3.FullName]/
                            AddressTranslator.GetAreaByteLength(area), i++)
                    {
                        combinedAddress.Add(AddressFormater.FormatAddress(area, (int) currentAddress, (int)
                            ((currentAddress - (int) currentAddress)/
                             (ValueHelper.Instance.ByteLength[address.Item3.FullName]/
                              AddressTranslator.GetAreaByteLength(area)))));
                    }
                }
            }

            return combinedAddress.Select(p =>
            {
                var translateAns = AddressTranslator.AddressTranslate(p, isRead);
                return new AddressUnit()
                {
                    Area = translateAns.AreaString,
                    Address = translateAns.Address,
                    SubAddress = translateAns.SubAddress,
                    Id = p,
                    CommunicationTag = p
                };
            }).ToList();
        }
    }
}
