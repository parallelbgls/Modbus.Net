using System;
using System.Collections.Generic;
using System.Linq;

namespace Modbus.Net
{
    /// <summary>
    ///     地址组合器，组合后的每一组地址将只需一次向设备进行通讯
    /// </summary>
    public abstract class AddressCombiner : AddressCombiner<string>
    {
    }

    /// <summary>
    ///     地址组合器，组合后的每一组地址将只需一次向设备进行通讯
    /// </summary>
    public abstract class AddressCombiner<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        ///     组合地址
        /// </summary>
        /// <param name="addresses">需要进行组合的地址</param>
        /// <returns>组合完成后与设备通讯的地址</returns>
        public abstract IEnumerable<CommunicationUnit<TKey>> Combine(IEnumerable<AddressUnit<TKey>> addresses);
    }

    /// <summary>
    ///     连续的地址将组合成一组，向设备进行通讯
    /// </summary>
    public class AddressCombinerContinus : AddressCombinerContinus<string>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="addressTranslator">地址转换器</param>
        /// <param name="maxLength">单个发送协议允许的数据最长长度（字节）</param>
        public AddressCombinerContinus(AddressTranslator addressTranslator, int maxLength)
            : base(addressTranslator, maxLength)
        {
        }
    }

    /// <summary>
    ///     连续的地址将组合成一组，向设备进行通讯
    /// </summary>
    public class AddressCombinerContinus<TKey> : AddressCombiner<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="addressTranslator">地址转换器</param>
        /// <param name="maxLength">单个发送协议允许的数据最长长度（字节）</param>
        public AddressCombinerContinus(AddressTranslator addressTranslator, int maxLength)
        {
            AddressTranslator = addressTranslator;
            MaxLength = maxLength;
        }

        /// <summary>
        ///     协议的数据最长长度（字节）
        /// </summary>
        protected int MaxLength { get; set; }

        /// <summary>
        ///     地址转换器
        /// </summary>
        protected AddressTranslator AddressTranslator { get; set; }

        /// <summary>
        ///     组合地址
        /// </summary>
        /// <param name="addresses">需要组合的地址</param>
        /// <returns>组合后的地址</returns>
        public override IEnumerable<CommunicationUnit<TKey>> Combine(IEnumerable<AddressUnit<TKey>> addresses)
        {
            //按从小到大的顺序对地址进行排序
            var groupedAddresses = from address in addresses
                orderby
                AddressHelper.GetProtocolCoordinate(address.Address, address.SubAddress,
                    AddressTranslator.GetAreaByteLength(address.Area))
                group address by address.Area
                into grouped
                select grouped;
            var ans = new List<CommunicationUnit<TKey>>();
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
                var originalAddresses = new List<AddressUnit<TKey>>();
                //对组合内地址从小到大进行排序
                var orderedAddresses =
                    groupedAddress.OrderBy(
                        address =>
                            AddressHelper.GetProtocolCoordinate(address.Address, address.SubAddress,
                                AddressTranslator.GetAreaByteLength(address.Area)));
                foreach (var address in orderedAddresses)
                {
                    //第一次进入时直接压入地址
                    if (initNum < 0)
                    {
                        initNum = AddressHelper.GetProtocolCoordinate(address.Address, address.SubAddress,
                            AddressTranslator.GetAreaByteLength(address.Area));
                        originalAddresses.Add(address);
                    }
                    else
                    {
                        //如果当前地址小于已经记录的地址域，表示这个地址的开始已经记录过了
                        if (AddressHelper.GetProtocolCoordinate(address.Address, address.SubAddress,
                                AddressTranslator.GetAreaByteLength(address.Area)) <
                            AddressHelper.GetProtocolCoordinateNextPosition(preNum,
                                preType,
                                AddressTranslator.GetAreaByteLength(address.Area)))
                        {
                            originalAddresses.Add(address);
                            //如果当前地址的末尾被记录，表示地址被记录的地址域覆盖，这个地址没有记录的必要
                            if (AddressHelper.GetProtocolCoordinateNextPosition(
                                    AddressHelper.GetProtocolCoordinate(address.Address, address.SubAddress,
                                        AddressTranslator.GetAreaByteLength(address.Area)),
                                    address.DataType,
                                    AddressTranslator.GetAreaByteLength(address.Area)) <=
                                AddressHelper.GetProtocolCoordinateNextPosition(preNum,
                                    preType,
                                    AddressTranslator.GetAreaByteLength(address.Area)))
                                continue;
                        }
                        //如果当前地址大于记录的地址域的开头，则表示地址已经不连续了
                        else if (AddressHelper.GetProtocolCoordinate(address.Address, address.SubAddress,
                                     AddressTranslator.GetAreaByteLength(address.Area)) >
                                 AddressHelper.GetProtocolCoordinateNextPosition(preNum,
                                     preType,
                                     AddressTranslator.GetAreaByteLength(address.Area)))
                        {
                            //上一个地址域压入返回结果，并把当前记录的结果清空。
                            ans.Add(new CommunicationUnit<TKey>
                            {
                                Area = area,
                                Address = (int) Math.Floor(initNum),
                                GetCount =
                                    (int)
                                    Math.Ceiling(
                                        AddressHelper.MapProtocolGetCountToAbstractByteCount(
                                            preNum - (int) Math.Floor(initNum),
                                            AddressTranslator.GetAreaByteLength(address.Area),
                                            BigEndianValueHelper.Instance.ByteLength[preType.FullName])),
                                DataType = typeof(byte),
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
                    preNum = AddressHelper.GetProtocolCoordinate(address.Address, address.SubAddress,
                        AddressTranslator.GetAreaByteLength(address.Area));
                    preType = address.DataType;
                }
                //最后一个地址域压入返回结果
                ans.Add(new CommunicationUnit<TKey>
                {
                    Area = area,
                    Address = (int) Math.Floor(initNum),
                    GetCount =
                        (int)
                        Math.Ceiling(
                            AddressHelper.MapProtocolGetCountToAbstractByteCount(
                                preNum - (int) Math.Floor(initNum), AddressTranslator.GetAreaByteLength(area),
                                BigEndianValueHelper.Instance.ByteLength[preType.FullName])),
                    DataType = typeof(byte),
                    OriginalAddresses = originalAddresses.ToList()
                });
            }
            var newAns = new List<CommunicationUnit<TKey>>();
            foreach (var communicationUnit in ans)
            {
                var oldByteCount = communicationUnit.GetCount *
                                   BigEndianValueHelper.Instance.ByteLength[communicationUnit.DataType.FullName];
                while (oldByteCount * BigEndianValueHelper.Instance.ByteLength[communicationUnit.DataType.FullName] >
                       MaxLength)
                {
                    var newOriginalAddresses = new List<AddressUnit<TKey>>();
                    var oldOriginalAddresses = communicationUnit.OriginalAddresses.ToList();
                    var newByteCount = 0.0;
                    do
                    {
                        var currentAddressUnit = oldOriginalAddresses.First();
                        newByteCount += BigEndianValueHelper.Instance.ByteLength[currentAddressUnit.DataType.FullName];
                        if (newByteCount > MaxLength) break;
                        oldByteCount -= BigEndianValueHelper.Instance.ByteLength[currentAddressUnit.DataType.FullName];
                        newOriginalAddresses.Add(currentAddressUnit);
                        oldOriginalAddresses.RemoveAt(0);
                    } while (newByteCount < MaxLength);
                    var newCommunicationUnit = new CommunicationUnit<TKey>
                    {
                        Area = communicationUnit.Area,
                        Address = communicationUnit.Address,
                        SubAddress = communicationUnit.SubAddress,
                        DataType = communicationUnit.DataType,
                        GetCount =
                            (int)
                            Math.Ceiling(newByteCount /
                                         BigEndianValueHelper.Instance.ByteLength[communicationUnit.DataType.FullName]),
                        OriginalAddresses = newOriginalAddresses
                    };

                    newAns.Add(newCommunicationUnit);
                }
                communicationUnit.GetCount =
                    (int)
                    Math.Ceiling(oldByteCount /
                                 BigEndianValueHelper.Instance.ByteLength[communicationUnit.DataType.FullName]);
                newAns.Add(communicationUnit);
            }
            return newAns;
        }
    }

    /// <summary>
    ///     单个地址变为一组，每一个地址都进行一次查询
    /// </summary>
    public class AddressCombinerSingle : AddressCombinerSingle<string>
    {
    }

    /// <summary>
    ///     单个地址变为一组，每一个地址都进行一次查询
    /// </summary>
    public class AddressCombinerSingle<TKey> : AddressCombiner<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        ///     组合地址
        /// </summary>
        /// <param name="addresses">需要组合的地址</param>
        /// <returns>组合后的地址</returns>
        public override IEnumerable<CommunicationUnit<TKey>> Combine(IEnumerable<AddressUnit<TKey>> addresses)
        {
            return
                addresses.Select(
                    address =>
                        new CommunicationUnit<TKey>
                        {
                            Area = address.Area,
                            Address = address.Address,
                            SubAddress = address.SubAddress,
                            DataType = address.DataType,
                            GetCount = 1,
                            OriginalAddresses = new List<AddressUnit<TKey>> {address}
                        }).ToList();
        }
    }

    /// <summary>
    ///     两个CommunicationUnit之间的间隔
    /// </summary>
    internal class CommunicationUnitGap<TKey> where TKey : IEquatable<TKey>
    {
        public CommunicationUnit<TKey> EndUnit { get; set; }
        public int GapNumber { get; set; }
    }

    /// <summary>
    ///     可以调过多少数量的地址，把两个地址段变为一组通讯
    /// </summary>
    public class AddressCombinerNumericJump : AddressCombinerNumericJump<string>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="jumpByteCount">需要跳过的字节个数</param>
        /// <param name="maxLength">单个协议允许的数据最长长度（字节）</param>
        /// <param name="addressTranslator">地址转换器</param>
        public AddressCombinerNumericJump(int jumpByteCount, int maxLength, AddressTranslator addressTranslator)
            : base(jumpByteCount, maxLength, addressTranslator)
        {
        }
    }

    /// <summary>
    ///     可以调过多少数量的地址，把两个地址段变为一组通讯
    /// </summary>
    public class AddressCombinerNumericJump<TKey> : AddressCombinerContinus<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="jumpByteCount">需要跳过的字节个数</param>
        /// <param name="maxLength">单个协议允许的数据最长长度（字节）</param>
        /// <param name="addressTranslator">地址转换器</param>
        public AddressCombinerNumericJump(int jumpByteCount, int maxLength, AddressTranslator addressTranslator)
            : base(addressTranslator, maxLength)
        {
            JumpNumber = jumpByteCount;
        }

        /// <summary>
        ///     跳过的地址长度
        /// </summary>
        private int JumpNumber { get; }

        /// <summary>
        ///     组合地址
        /// </summary>
        /// <param name="addresses">需要组合的地址</param>
        /// <returns>组合后的地址</returns>
        public override IEnumerable<CommunicationUnit<TKey>> Combine(IEnumerable<AddressUnit<TKey>> addresses)
        {
            var continusAddresses = base.Combine(addresses).ToList();
            var addressesGaps = new List<CommunicationUnitGap<TKey>>();
            CommunicationUnit<TKey> preCommunicationUnit = null;
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
                    var gap = new CommunicationUnitGap<TKey>
                    {
                        EndUnit = continusAddress,
                        GapNumber =
                            (int)
                            Math.Ceiling(AddressHelper.MapProtocolCoordinateToAbstractCoordinate(
                                             continusAddress.Address, preCommunicationUnit.Address,
                                             AddressTranslator.GetAreaByteLength(continusAddress.Area)) -
                                         preCommunicationUnit.GetCount *
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
                if (orderedGap.GapNumber <= 0) continue;
                var nowAddress = orderedGap.EndUnit;
                var index = continusAddresses.FindIndex(p=>p.Area == nowAddress.Area && p.Address == nowAddress.Address && p.SubAddress == nowAddress.SubAddress);
                nowAddress = continusAddresses[index];
                index--;
                var preAddress = continusAddresses[index];
                if (nowAddress.GetCount * BigEndianValueHelper.Instance.ByteLength[nowAddress.DataType.FullName] +
                    preAddress.GetCount * BigEndianValueHelper.Instance.ByteLength[preAddress.DataType.FullName] +
                    orderedGap.GapNumber > MaxLength) continue;
                jumpNumberInner -= orderedGap.GapNumber;
                if (jumpNumberInner < 0) break;
                continusAddresses.RemoveAt(index);
                continusAddresses.RemoveAt(index);
                //合并两个已有的地址段，变为一个新的地址段
                var newAddress = new CommunicationUnit<TKey>
                {
                    Area = nowAddress.Area,
                    Address = preAddress.Address,
                    GetCount =
                        (int)
                        (preAddress.GetCount * BigEndianValueHelper.Instance.ByteLength[preAddress.DataType.FullName]) +
                        orderedGap.GapNumber +
                        (int)
                        (nowAddress.GetCount * BigEndianValueHelper.Instance.ByteLength[nowAddress.DataType.FullName]),
                    DataType = typeof(byte),
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
    public class AddressCombinerPercentageJump : AddressCombinerPercentageJump<string>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="percentage">允许跳过的字节数除以待组合的地址的字节数的百分比</param>
        /// <param name="maxLength">单个协议允许的数据最大长度</param>
        /// <param name="addressTranslator">地址转换器</param>
        public AddressCombinerPercentageJump(double percentage, int maxLength, AddressTranslator addressTranslator)
            : base(percentage, maxLength, addressTranslator)
        {
        }
    }

    /// <summary>
    ///     可以调过多少百分比的地址，把两个地址段变为一个
    /// </summary>
    public class AddressCombinerPercentageJump<TKey> : AddressCombinerContinus<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="percentage">允许跳过的字节数除以待组合的地址的字节数的百分比</param>
        /// <param name="maxLength">单个协议允许的数据最大长度</param>
        /// <param name="addressTranslator">地址转换器</param>
        public AddressCombinerPercentageJump(double percentage, int maxLength, AddressTranslator addressTranslator)
            : base(addressTranslator, maxLength)
        {
            if (percentage < 0) percentage = 0;
            Percentage = percentage;
        }

        /// <summary>
        ///     跳过的百分比
        /// </summary>
        private double Percentage { get; }

        /// <summary>
        ///     组合地址
        /// </summary>
        /// <param name="addresses">需要组合的地址</param>
        /// <returns>组合后的地址</returns>
        public override IEnumerable<CommunicationUnit<TKey>> Combine(IEnumerable<AddressUnit<TKey>> addresses)
        {
            var addressUnits = addresses as IList<AddressUnit<TKey>> ?? addresses.ToList();
            var count = addressUnits.Sum(address => BigEndianValueHelper.Instance.ByteLength[address.DataType.FullName]);
            return
                new AddressCombinerNumericJump<TKey>((int) (count * Percentage / 100.0), MaxLength, AddressTranslator)
                    .Combine(
                        addressUnits);
        }
    }
}