using System;
using System.Collections.Generic;
using System.Linq;

namespace ModBus.Net
{
    /// <summary>
    /// 地址组合器，组合后的每一组地址将只需一次向设备进行通讯
    /// </summary>
    public abstract class AddressCombiner
    {
        /// <summary>
        /// 组合地址
        /// </summary>
        /// <param name="addresses">需要进行组合的地址</param>
        /// <returns>组合完成后与设备通讯的地址</returns>
        public abstract IEnumerable<CommunicationUnit> Combine(IEnumerable<AddressUnit> addresses);
    }

    /// <summary>
    /// 连续的地址将组合成一组，向设备进行通讯
    /// </summary>
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
                foreach (var address in groupedAddress.OrderBy(address => address.Address))
                {
                    if (initNum < 0)
                    {
                        initNum = address.Address;
                        getCount = (int) BigEndianValueHelper.Instance.ByteLength[address.DataType.FullName];
                    }
                    else
                    {
                        if (address.Address > preNum + BigEndianValueHelper.Instance.ByteLength[preType.FullName])
                        {
                            ans.Add(new CommunicationUnit()
                            {
                                Area = area,
                                Address = initNum,
                                GetCount = getCount,
                                DataType = typeof (byte)
                            });
                            initNum = address.Address;
                            getCount = (int) BigEndianValueHelper.Instance.ByteLength[address.DataType.FullName];
                        }
                        else
                        {
                            getCount += (int) BigEndianValueHelper.Instance.ByteLength[address.DataType.FullName];
                        }
                    }
                    preNum = address.Address;
                    preType = address.DataType;
                }
                ans.Add(new CommunicationUnit()
                {
                    Area = area,
                    Address = initNum,
                    GetCount = getCount,
                    DataType = typeof (byte)
                });
            }
            return ans;
        }
    }

    public class AddressCombinerSingle : AddressCombiner
    {
        public override IEnumerable<CommunicationUnit> Combine(IEnumerable<AddressUnit> addresses)
        {
            return
                addresses.Select(
                    address =>
                        new CommunicationUnit()
                        {
                            Area = address.Area,
                            Address = address.Address,
                            DataType = address.DataType,
                            GetCount = 1
                        }).ToList();
        }
    }
}
