using System;
using System.Collections.Generic;
using System.Linq;

namespace Modbus.Net
{
    /// <summary>
    ///     地址组合器，组合后的每一组地址将只需一次向设备进行通讯
    /// </summary>
    public abstract class AddressCombiner
    {
        /// <summary>
        ///     组合地址
        /// </summary>
        /// <param name="addresses">需要进行组合的地址</param>
        /// <returns>组合完成后与设备通讯的地址</returns>
        public abstract IEnumerable<CommunicationUnit> Combine(IEnumerable<AddressUnit> addresses);
    }

    /// <summary>
    ///     连续的地址将组合成一组，向设备进行通讯
    /// </summary>
    public class AddressCombinerContinus : AddressCombiner
    {
        public AddressCombinerContinus(AddressTranslator addressTranslator)
        {
            AddressTranslator = addressTranslator;
        }

        protected AddressTranslator AddressTranslator { get; set; }

        public override IEnumerable<CommunicationUnit> Combine(IEnumerable<AddressUnit> addresses)
        {
            //按从小到大的顺序对地址进行排序
            var groupedAddresses = from address in addresses
                orderby
                    AddressHelper.GetProtocalCoordinate(address.Address, address.SubAddress,
                        AddressTranslator.GetAreaByteLength(address.Area))
                group address by address.Area
                into grouped
                select grouped;
            var ans = new List<CommunicationUnit>();
            foreach (var groupedAddress in groupedAddresses)
            {
                var area = groupedAddress.Key;
                //初始地址
                double initNum = -1;
                //上一个地址
                double preNum = -1;
                //上一个地址类型
                Type preType = null;
                //记录一个地址组合当中的所有原始地址
                var originalAddresses = new List<AddressUnit>();
                //对组合内地址从小到大进行排序
                var orderedAddresses =
                    groupedAddress.OrderBy(
                        address =>
                            AddressHelper.GetProtocalCoordinate(address.Address, address.SubAddress,
                                AddressTranslator.GetAreaByteLength(address.Area)));
                foreach (var address in orderedAddresses)
                {
                    //第一次进入时直接压入地址
                    if (initNum < 0)
                    {
                        initNum = AddressHelper.GetProtocalCoordinate(address.Address, address.SubAddress,
                            AddressTranslator.GetAreaByteLength(address.Area));
                        originalAddresses.Add(address);
                    }
                    else
                    {
                        //如果当前地址小于已经记录的地址域，表示这个地址的开始已经记录过了
                        if (AddressHelper.GetProtocalCoordinate(address.Address, address.SubAddress,
                            AddressTranslator.GetAreaByteLength(address.Area)) <
                            AddressHelper.GetProtocalCoordinateNextPosition(preNum,
                                preType,
                                AddressTranslator.GetAreaByteLength(address.Area)))
                        {
                            originalAddresses.Add(address);
                            //如果当前地址的末尾被记录，表示地址被记录的地址域覆盖，这个地址没有记录的必要
                            if (AddressHelper.GetProtocalCoordinateNextPosition(
                                AddressHelper.GetProtocalCoordinate(address.Address, address.SubAddress,
                                    AddressTranslator.GetAreaByteLength(address.Area)),
                                address.DataType,
                                AddressTranslator.GetAreaByteLength(address.Area)) <=
                                AddressHelper.GetProtocalCoordinateNextPosition(preNum,
                                    preType,
                                    AddressTranslator.GetAreaByteLength(address.Area)))
                            {
                                continue;
                            }
                        }
                        //如果当前地址大于记录的地址域的开头，则表示地址已经不连续了
                        else if (AddressHelper.GetProtocalCoordinate(address.Address, address.SubAddress,
                            AddressTranslator.GetAreaByteLength(address.Area)) >
                                 AddressHelper.GetProtocalCoordinateNextPosition(preNum,
                                     preType,
                                     AddressTranslator.GetAreaByteLength(address.Area)))
                        {
                            //上一个地址域压入返回结果，并把当前记录的结果清空。
                            ans.Add(new CommunicationUnit
                            {
                                Area = area,
                                Address = (int) Math.Floor(initNum),
                                GetCount =
                                    (int)
                                        Math.Ceiling(
                                            AddressHelper.MapProtocalGetCountToAbstractByteCount(
                                                preNum - (int) Math.Floor(initNum),
                                                AddressTranslator.GetAreaByteLength(address.Area),
                                                BigEndianValueHelper.Instance.ByteLength[preType.FullName])),
                                DataType = typeof (byte),
                                OriginalAddresses = originalAddresses.ToList()
                            });
                            initNum = address.Address;
                            originalAddresses.Clear();
                            originalAddresses.Add(address);
                        }
                        else
                        {
                            //地址连续，压入当前记录的结果
                            originalAddresses.Add(address);
                        }
                    }
                    //把当前地址变为上一个地址
                    preNum = AddressHelper.GetProtocalCoordinate(address.Address, address.SubAddress,
                        AddressTranslator.GetAreaByteLength(address.Area));
                    preType = address.DataType;
                }
                //最后一个地址域压入返回结果
                ans.Add(new CommunicationUnit
                {
                    Area = area,
                    Address = (int) Math.Floor(initNum),
                    GetCount =
                        (int)
                            Math.Ceiling(
                                AddressHelper.MapProtocalGetCountToAbstractByteCount(
                                    preNum - (int) Math.Floor(initNum), AddressTranslator.GetAreaByteLength(area),
                                    BigEndianValueHelper.Instance.ByteLength[preType.FullName])),
                    DataType = typeof (byte),
                    OriginalAddresses = originalAddresses.ToList()
                });
            }
            return ans;
        }
    }

    /// <summary>
    ///     单个地址变为一组，每一个地址都进行一次查询
    /// </summary>
    public class AddressCombinerSingle : AddressCombiner
    {
        public override IEnumerable<CommunicationUnit> Combine(IEnumerable<AddressUnit> addresses)
        {
            return
                addresses.Select(
                    address =>
                        new CommunicationUnit
                        {
                            Area = address.Area,
                            Address = address.Address,
                            SubAddress = address.SubAddress,
                            DataType = address.DataType,
                            GetCount = 1,
                            OriginalAddresses = new List<AddressUnit> {address}
                        }).ToList();
        }
    }

    /// <summary>
    ///     两个CommunicationUnit之间的间隔
    /// </summary>
    internal class CommunicationUnitGap
    {
        public CommunicationUnit EndUnit { get; set; }
        public int GapNumber { get; set; }
    }

    /// <summary>
    ///     可以调过多少数量的地址，把两个地址段变为一组通讯
    /// </summary>
    public class AddressCombinerNumericJump : AddressCombinerContinus
    {
        public AddressCombinerNumericJump(int jumpByteCount, AddressTranslator addressTranslator)
            : base(addressTranslator)
        {
            JumpNumber = jumpByteCount;
        }

        private int JumpNumber { get; }

        public override IEnumerable<CommunicationUnit> Combine(IEnumerable<AddressUnit> addresses)
        {
            var continusAddresses = base.Combine(addresses).ToList();
            var addressesGaps = new List<CommunicationUnitGap>();
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
                    //计算间隔
                    var gap = new CommunicationUnitGap
                    {
                        EndUnit = continusAddress,
                        GapNumber =
                            (int)
                                Math.Ceiling(AddressHelper.MapProtocalCoordinateToAbstractCoordinate(
                                    continusAddress.Address, preCommunicationUnit.Address,
                                    AddressTranslator.GetAreaByteLength(continusAddress.Area)) -
                                             preCommunicationUnit.GetCount*
                                             BigEndianValueHelper.Instance.ByteLength[
                                                 preCommunicationUnit.DataType.FullName])
                    };
                    addressesGaps.Add(gap);
                }
                preCommunicationUnit = continusAddress;
            }
            //减去间隔
            var orderedGaps = addressesGaps.OrderBy(p => p.GapNumber);
            var jumpNumberInner = JumpNumber;
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
                //变为新的地址段
                var newAddress = new CommunicationUnit
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

    /// <summary>
    ///     可以调过多少百分比的地址，把两个地址段变为一个
    /// </summary>
    public class AddressCombinerPercentageJump : AddressCombinerContinus
    {
        public AddressCombinerPercentageJump(double percentage, AddressTranslator addressTranslator)
            : base(addressTranslator)
        {
            if (percentage < 0) percentage = 0;
            Percentage = percentage;
        }

        private double Percentage { get; }

        public override IEnumerable<CommunicationUnit> Combine(IEnumerable<AddressUnit> addresses)
        {
            var addressUnits = addresses as IList<AddressUnit> ?? addresses.ToList();
            var count = addressUnits.Sum(address => BigEndianValueHelper.Instance.ByteLength[address.DataType.FullName]);
            return
                new AddressCombinerNumericJump((int) (count*Percentage/100.0), AddressTranslator).Combine(addressUnits);
        }
    }
}