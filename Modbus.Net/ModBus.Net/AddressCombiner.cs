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
                orderby address.Address
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

    class CommunicationUnitGap
    {
        public CommunicationUnit EndUnit { get; set; }
        public int GapNumber { get; set; }
    }

    public class AddressCombinerNumericJump : AddressCombiner
    {
        private int JumpNumber { get; }

        public AddressCombinerNumericJump(int jumpNumber)
        {
            JumpNumber = jumpNumber;
        }

        public override IEnumerable<CommunicationUnit> Combine(IEnumerable<AddressUnit> addresses)
        {
            var continusAddresses = new AddressCombinerContinus().Combine(addresses).ToList();
            List<CommunicationUnitGap> addressesGaps = new List<CommunicationUnitGap>();
            CommunicationUnit preCommunicationUnit = null;
            foreach (var continusAddress in continusAddresses)
            {
                if (preCommunicationUnit == null)
                {
                    preCommunicationUnit = continusAddress;
                    continue;
                }
                if (continusAddress.Area == preCommunicationUnit.Area)
                {
                    var gap = new CommunicationUnitGap()
                    {
                        EndUnit = continusAddress,
                        GapNumber = continusAddress.Address - preCommunicationUnit.Address - (int)(preCommunicationUnit.GetCount * BigEndianValueHelper.Instance.ByteLength[preCommunicationUnit.DataType.FullName])
                    };
                    addressesGaps.Add(gap);
                }
                preCommunicationUnit = continusAddress;
            }
            var orderedGaps = addressesGaps.OrderBy(p => p.GapNumber);
            int jumpNumberInner = JumpNumber;
            foreach (var orderedGap in orderedGaps)
            {
                jumpNumberInner -= orderedGap.GapNumber;
                if (jumpNumberInner < 0) break;
                var nowAddress = orderedGap.EndUnit;
                var index = continusAddresses.IndexOf(nowAddress);
                index--;
                var preAddress = continusAddresses[index];
                continusAddresses.RemoveAt(index);
                continusAddresses.RemoveAt(index);
                var newAddress = new CommunicationUnit()
                {
                    Area = nowAddress.Area,
                    Address = preAddress.Address,
                    GetCount =
                        (int)
                            (preAddress.GetCount*BigEndianValueHelper.Instance.ByteLength[preAddress.DataType.FullName]) +
                        orderedGap.GapNumber +
                        (int)
                            (nowAddress.GetCount*BigEndianValueHelper.Instance.ByteLength[nowAddress.DataType.FullName]),
                    DataType = typeof (byte)
                };
                continusAddresses.Insert(index, newAddress);
            }
            return continusAddresses;
        }
    }

    public class AddressCombinerPercentageJump : AddressCombiner
    {
        private double Percentage { get; }

        public AddressCombinerPercentageJump(double percentage)
        {
            if (percentage < 0 || percentage > 100) throw new ArgumentException();
            Percentage = percentage;
        }

        public override IEnumerable<CommunicationUnit> Combine(IEnumerable<AddressUnit> addresses)
        {
            var addressUnits = addresses as IList<AddressUnit> ?? addresses.ToList();
            double count = addressUnits.Sum(address => BigEndianValueHelper.Instance.ByteLength[address.DataType.FullName]);
            return new AddressCombinerNumericJump((int)(count * Percentage / 100.0)).Combine(addressUnits);
        }
    }
}
