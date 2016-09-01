using System;
using System.Collections.Generic;
using System.Linq;

namespace Modbus.Net
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
        protected AddressTranslator AddressTranslator { get; set; }

        public override IEnumerable<CommunicationUnit> Combine(IEnumerable<AddressUnit> addresses)
        {
            var groupedAddresses = from address in addresses
                orderby address.Address + address.SubAddress * (0.125 / AddressTranslator.GetAreaByteLength(address.Area))
                group address by address.Area
                into grouped
                select grouped;
            var ans = new List<CommunicationUnit>();
            foreach (var groupedAddress in groupedAddresses)
            {
                string area = groupedAddress.Key;
                double initNum = -1;
                double preNum = -1;
                Type preType = null;
                List<AddressUnit> originalAddresses = new List<AddressUnit>();
                var orderedAddresses =
                    groupedAddress.OrderBy(
                        address =>
                            address.Address +
                            address.SubAddress*(0.125/AddressTranslator.GetAreaByteLength(address.Area)));
                foreach (var address in orderedAddresses)
                {
                    if (initNum < 0)
                    {
                        initNum = address.Address + address.SubAddress * (0.125 / AddressTranslator.GetAreaByteLength(address.Area));
                        originalAddresses.Add(address);
                    }
                    else
                    {
                        if (address.Address +
                            address.SubAddress*(0.125/AddressTranslator.GetAreaByteLength(address.Area)) <
                            preNum +
                            BigEndianValueHelper.Instance.ByteLength[preType.FullName]/
                            AddressTranslator.GetAreaByteLength(address.Area))
                        {
                            originalAddresses.Add(address);
                            if (address.Address +
                                address.SubAddress*(0.125/AddressTranslator.GetAreaByteLength(address.Area)) +
                                BigEndianValueHelper.Instance.ByteLength[address.DataType.FullName]/
                                AddressTranslator.GetAreaByteLength(address.Area) <=
                                preNum +
                                BigEndianValueHelper.Instance.ByteLength[preType.FullName]/
                                AddressTranslator.GetAreaByteLength(address.Area))
                            {                               
                                continue;
                            }
                        }
                        else if (address.Address +
                            address.SubAddress*(0.125/AddressTranslator.GetAreaByteLength(address.Area)) >
                            preNum +
                            BigEndianValueHelper.Instance.ByteLength[preType.FullName]/
                            AddressTranslator.GetAreaByteLength(address.Area))
                        {
                            ans.Add(new CommunicationUnit()
                            {
                                Area = area,
                                Address = (int) Math.Floor(initNum),
                                GetCount = (int)Math.Ceiling((preNum - (int)Math.Floor(initNum)) * AddressTranslator.GetAreaByteLength(address.Area) + BigEndianValueHelper.Instance.ByteLength[preType.FullName]),
                                DataType = typeof (byte),
                                OriginalAddresses = originalAddresses.ToList(),
                            });
                            initNum = address.Address;
                            originalAddresses.Clear();
                            originalAddresses.Add(address);
                        }
                        else
                        {
                            originalAddresses.Add(address);
                        }
                    }
                    preNum = address.Address + address.SubAddress * (0.125 / AddressTranslator.GetAreaByteLength(address.Area));
                    preType = address.DataType;
                }
                ans.Add(new CommunicationUnit()
                {
                    Area = area,
                    Address = (int)Math.Floor(initNum),
                    GetCount = (int)Math.Ceiling((preNum - (int)Math.Floor(initNum)) * AddressTranslator.GetAreaByteLength(area) + BigEndianValueHelper.Instance.ByteLength[preType.FullName]),
                    DataType = typeof (byte),
                    OriginalAddresses = originalAddresses.ToList()
                });
            }
            return ans;
        }

        public AddressCombinerContinus(AddressTranslator addressTranslator)
        {
            AddressTranslator = addressTranslator;
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
                            GetCount = 1,
                            OriginalAddresses = new List<AddressUnit>() {address}
                        }).ToList();
        }
    }

    class CommunicationUnitGap
    {
        public CommunicationUnit EndUnit { get; set; }
        public int GapNumber { get; set; }
    }

    public class AddressCombinerNumericJump : AddressCombinerContinus
    {
        private int JumpNumber { get; }

        public AddressCombinerNumericJump(int jumpByteCount, AddressTranslator addressTranslator) : base(addressTranslator)
        {
            JumpNumber = jumpByteCount;
        }

        public override IEnumerable<CommunicationUnit> Combine(IEnumerable<AddressUnit> addresses)
        {
            var continusAddresses = base.Combine(addresses).ToList();
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
                        GapNumber = (int)Math.Ceiling((continusAddress.Address - preCommunicationUnit.Address) * AddressTranslator.GetAreaByteLength(continusAddress.Area) - preCommunicationUnit.GetCount * BigEndianValueHelper.Instance.ByteLength[preCommunicationUnit.DataType.FullName])
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
                    DataType = typeof (byte),
                    OriginalAddresses = preAddress.OriginalAddresses.ToList().Union(nowAddress.OriginalAddresses)
                };
                continusAddresses.Insert(index, newAddress);
            }
            return continusAddresses;
        }
    }

    public class AddressCombinerPercentageJump : AddressCombinerContinus
    {
        private double Percentage { get; }

        public AddressCombinerPercentageJump(double percentage, AddressTranslator addressTranslator) :base (addressTranslator)
        {
            if (percentage < 0) percentage = 0;
            Percentage = percentage;
        }

        public override IEnumerable<CommunicationUnit> Combine(IEnumerable<AddressUnit> addresses)
        {
            var addressUnits = addresses as IList<AddressUnit> ?? addresses.ToList();
            double count = addressUnits.Sum(address => BigEndianValueHelper.Instance.ByteLength[address.DataType.FullName]);
            return new AddressCombinerNumericJump((int)(count * Percentage / 100.0), AddressTranslator).Combine(addressUnits);
        }
    }
}
