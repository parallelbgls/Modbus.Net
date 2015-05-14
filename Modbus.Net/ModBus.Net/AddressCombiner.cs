using System;
using System.Collections.Generic;
using System.Linq;

namespace ModBus.Net
{
    public abstract class AddressCombiner
    {
        public abstract IEnumerable<CommunicationUnit> Combine(IEnumerable<AddressUnit> addresses);
    }

    public class AddressCombinerContinus : AddressCombiner
    {
        public override IEnumerable<CommunicationUnit> Combine(IEnumerable<AddressUnit> addresses)
        {
            var groupedAddresses = from address in addresses
                group address by address.Area
                into grouped
                select grouped;
            var ans = new List<CommunicationUnit>();
            foreach (var groupedAddress in groupedAddresses)
            {
                string area = groupedAddress.Key;
                int initNum = -1;
                int preNum = -1;
                Type preType = null;
                int getCount = 0;
                foreach (var address in groupedAddress.OrderBy(address=>address.Address))
                {
                    if (initNum < 0)
                    {
                        initNum = address.Address;
                        getCount = (int)ValueHelper.Instance.ByteLength[address.DataType.FullName];
                    }
                    else
                    {
                        if (address.Address > preNum + ValueHelper.Instance.ByteLength[preType.FullName])
                        {
                            ans.Add(new CommunicationUnit(){Area = area, Address = initNum, GetCount = getCount, DataType = typeof(byte)});
                            initNum = address.Address;
                            getCount = (int)ValueHelper.Instance.ByteLength[address.DataType.FullName];
                        }
                        else
                        {
                            getCount += (int)ValueHelper.Instance.ByteLength[address.DataType.FullName];
                        }
                    }
                    preNum = address.Address;
                    preType = address.DataType;
                }
                ans.Add(new CommunicationUnit() { Area = area, Address = initNum, GetCount = getCount, DataType = typeof(byte) });
            }
            return ans;
        }
    }
}
