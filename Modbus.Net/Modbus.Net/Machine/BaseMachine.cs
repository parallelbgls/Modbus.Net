﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     设备
    /// </summary>
    /// <typeparam name="TKey">设备的Id类型</typeparam>
    /// <typeparam name="TUnitKey">设备中使用的AddressUnit的Id类型</typeparam>
    public abstract class BaseMachine<TKey, TUnitKey> : BaseMachine<TKey, TUnitKey, int, int>
        where TKey : IEquatable<TKey>
        where TUnitKey : IEquatable<TUnitKey>
    {
        private static readonly ILogger<BaseMachine<TKey, TUnitKey>> logger = LogProvider.CreateLogger<BaseMachine<TKey, TUnitKey>>();

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="id">设备的ID号</param>
        /// <param name="getAddresses">需要与设备通讯的地址</param>
        /// <param name="keepConnect">是否保持连接</param>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        protected BaseMachine(TKey id, IEnumerable<AddressUnit<TUnitKey, int, int>> getAddresses, bool keepConnect, byte slaveAddress,
            byte masterAddress) : base(id, getAddresses, keepConnect)
        {
            SlaveAddress = slaveAddress;
            MasterAddress = masterAddress;
        }


        private readonly int _maxErrorCount = 3;

        private int ErrorCount { get; set; }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; set; } = 2;

        /// <summary>
        ///     主站号
        /// </summary>
        public byte MasterAddress { get; set; }

        /// <summary>
        ///     与设备实际通讯的连续地址
        /// </summary>
        protected IEnumerable<CommunicationUnit<TUnitKey, int, int>> CommunicateAddresses
            => GetAddresses != null ? AddressCombiner.Combine(GetAddresses) : null;

        /// <summary>
        ///     获取地址组合器
        /// </summary>
        public AddressCombiner<TUnitKey, int, int> AddressCombiner { get; set; }

        /// <summary>
        ///     写入地址组合器
        /// </summary>
        public AddressCombiner<TUnitKey, int, int> AddressCombinerSet { get; set; }

        /// <summary>
        ///     读取数据
        /// </summary>
        /// <returns>从设备读取的数据</returns>
        public async override Task<ReturnStruct<Dictionary<string, ReturnUnit<double>>>> GetDatasAsync(MachineDataType getDataType)
        {
            try
            {
                var ans = new Dictionary<string, ReturnUnit<double>>();
                //检测并连接设备
                if (!BaseUtility.IsConnected)
                    await BaseUtility.ConnectAsync();
                //如果无法连接，终止
                if (!BaseUtility.IsConnected) return
                        new ReturnStruct<Dictionary<string, ReturnUnit<double>>>()
                        {
                            Datas = null,
                            IsSuccess = false,
                            ErrorCode = -1,
                            ErrorMsg = "Connection Error"
                        };
                //遍历每一个实际向设备获取数据的连续地址
                foreach (var communicateAddress in CommunicateAddresses)
                {
                    //获取数据
                    var datas =
                        await
                            BaseUtility.GetUtilityMethods<IUtilityMethodDatas>().GetDatasAsync(
                                AddressFormater.FormatAddress(communicateAddress.Area, communicateAddress.Address,
                                    communicateAddress.SubAddress),
                                (int)
                                Math.Ceiling(communicateAddress.GetCount *
                                             ValueHelper.ByteLength[
                                                 communicateAddress.DataType.FullName]),
                                communicateAddress.GetOriginalCount);

                    //如果已知没有返回，终止
                    if (datas.IsSuccess == null)
                    {
                        return new ReturnStruct<Dictionary<string, ReturnUnit<double>>>()
                        {
                            Datas = null,
                            IsSuccess = null,
                            ErrorCode = datas.ErrorCode,
                            ErrorMsg = datas.ErrorMsg
                        };
                    }
                    //如果没有数据，终止
                    else if (datas.IsSuccess == false || datas.Datas == null)
                    {
                        return new ReturnStruct<Dictionary<string, ReturnUnit<double>>>()
                        {
                            Datas = null,
                            IsSuccess = false,
                            ErrorCode = datas.ErrorCode,
                            ErrorMsg = datas.ErrorMsg
                        };
                    }
                    else if (datas.Datas.Length != 0 && datas.Datas.Length <
                        (int)
                        Math.Ceiling(communicateAddress.GetCount *
                                     ValueHelper.ByteLength[
                                         communicateAddress.DataType.FullName]))
                    {
                        return new ReturnStruct<Dictionary<string, ReturnUnit<double>>>()
                        {
                            Datas = null,
                            IsSuccess = false,
                            ErrorCode = -2,
                            ErrorMsg = "Data length mismatch"
                        };
                    }



                    foreach (var address in communicateAddress.OriginalAddresses)
                    {
                        //字节坐标的位置
                        var localPos = AddressHelper.MapProtocolCoordinateToAbstractCoordinate(address.Address,
                                           communicateAddress.Address,
                                           AddressTranslator.GetAreaByteLength(communicateAddress.Area)) +
                                       address.SubAddress * 0.125;
                        //字节坐标的主地址位置
                        var localMainPos = (int)localPos;
                        //字节坐标的子地址位置
                        var localSubPos = (int)((localPos - localMainPos) * 8);

                        //根据类型选择返回结果的键是通讯标识还是地址
                        string key;
                        switch (getDataType)
                        {
                            case MachineDataType.CommunicationTag:
                                {
                                    key = address.CommunicationTag;
                                    break;
                                }
                            case MachineDataType.Address:
                                {
                                    key = AddressFormater.FormatAddress(address.Area, address.Address, address.SubAddress);
                                    break;
                                }
                            case MachineDataType.Name:
                                {
                                    key = address.Name;
                                    break;
                                }
                            case MachineDataType.Id:
                                {
                                    key = address.Id.ToString();
                                    break;
                                }
                            default:
                                {
                                    key = address.CommunicationTag;
                                    break;
                                }
                        }

                        try
                        {
                            //如果没有数据返回空
                            if (datas.Datas.Length == 0)
                                ans.Add(key, new ReturnUnit<double>
                                {
                                    DeviceValue = null,
                                    AddressUnit = address.MapAddressUnitTUnitKeyToAddressUnit(),
                                });
                            else
                                ans.Add(key,
                                    new ReturnUnit<double>
                                    {
                                        DeviceValue =
                                            Math.Round(Convert.ToDouble(
                                                ValueHelper.GetInstance(BaseUtility.Endian)
                                                    .GetValue(datas.Datas, ref localMainPos, ref localSubPos,
                                                        address.DataType)) * address.Zoom, address.DecimalPos),
                                        AddressUnit = address.MapAddressUnitTUnitKeyToAddressUnit(),
                                    });
                        }
                        catch (Exception e)
                        {
                            ErrorCount++;
                            logger.LogError(e, $"BaseMachine -> GetDatas, Id:{Id} Connection:{ConnectionToken} key {key} existing. ErrorCount {ErrorCount}.");

                            if (ErrorCount >= _maxErrorCount)
                                Disconnect();
                            return new ReturnStruct<Dictionary<string, ReturnUnit<double>>>()
                            {
                                Datas = null,
                                IsSuccess = false,
                                ErrorCode = -3,
                                ErrorMsg = "Data translation mismatch"
                            };
                        }
                    }
                }
                //如果不保持连接，断开连接
                if (!KeepConnect)
                    BaseUtility.Disconnect();
                //返回数据
                if (ans.All(p => p.Value.DeviceValue == null)) ans = null;
                ErrorCount = 0;
                return new ReturnStruct<Dictionary<string, ReturnUnit<double>>>
                {
                    Datas = ans,
                    IsSuccess = true,
                    ErrorCode = 0,
                    ErrorMsg = ""
                };
            }
            catch (Exception e)
            {
                ErrorCount++;
                logger.LogError(e, $"BaseMachine -> GetDatas, Id:{Id} Connection:{ConnectionToken} error. ErrorCount {ErrorCount}.");

                if (ErrorCount >= _maxErrorCount)
                    Disconnect();
                return new ReturnStruct<Dictionary<string, ReturnUnit<double>>>()
                {
                    Datas = null,
                    IsSuccess = false,
                    ErrorCode = -100,
                    ErrorMsg = "Unknown Exception"
                };
            }
        }

        /// <summary>
        ///     写入数据
        /// </summary>
        /// <param name="setDataType">写入类型</param>
        /// <param name="values">需要写入的数据字典，当写入类型为Address时，键为需要写入的地址，当写入类型为CommunicationTag时，键为需要写入的单元的描述</param>
        /// <returns>是否写入成功</returns>
        public async override Task<ReturnStruct<bool>> SetDatasAsync(MachineDataType setDataType, Dictionary<string, double> values)
        {
            try
            {
                //检测并连接设备
                if (!BaseUtility.IsConnected)
                    await BaseUtility.ConnectAsync();
                //如果设备无法连接，终止
                if (!BaseUtility.IsConnected) return new ReturnStruct<bool>()
                {
                    Datas = false,
                    IsSuccess = false,
                    ErrorCode = -1,
                    ErrorMsg = "Connection Error"
                };
                var addresses = new List<AddressUnit<TUnitKey, int, int>>();
                //遍历每个要设置的值
                foreach (var value in values)
                {
                    //根据设置类型找到对应的地址描述
                    AddressUnit<TUnitKey, int, int> address = null;
                    switch (setDataType)
                    {
                        case MachineDataType.Address:
                            {
                                address =
                                    GetAddresses.SingleOrDefault(
                                        p =>
                                            AddressFormater.FormatAddress(p.Area, p.Address, p.SubAddress) == value.Key ||
                                            p.DataType != typeof(bool) &&
                                            AddressFormater.FormatAddress(p.Area, p.Address) == value.Key);
                                break;
                            }
                        case MachineDataType.CommunicationTag:
                            {
                                address =
                                    GetAddresses.SingleOrDefault(p => p.CommunicationTag == value.Key);
                                break;
                            }
                        case MachineDataType.Name:
                            {
                                address = GetAddresses.SingleOrDefault(p => p.Name == value.Key);
                                break;
                            }
                        case MachineDataType.Id:
                            {
                                address = GetAddresses.SingleOrDefault(p => p.Id.ToString() == value.Key);
                                break;
                            }
                        default:
                            {
                                address =
                                    GetAddresses.SingleOrDefault(p => p.CommunicationTag == value.Key);
                                break;
                            }
                    }
                    //地址为空报错
                    if (address == null)
                    {
                        logger.LogError($"Machine {ConnectionToken} Address {value.Key} doesn't exist.");
                        continue;
                    }
                    //不能写报错
                    if (!address.CanWrite)
                    {
                        logger.LogError($"Machine {ConnectionToken} Address {value.Key} cannot write.");
                        continue;
                    }
                    addresses.Add(address);
                }
                //将地址编码成与实际设备通讯的地址
                var communcationUnits = AddressCombinerSet.Combine(addresses);
                //遍历每条通讯的连续地址
                foreach (var communicateAddress in communcationUnits)
                {
                    //编码开始地址
                    var addressStart = AddressFormater.FormatAddress(communicateAddress.Area,
                        communicateAddress.Address);

                    var datasReturn =
                        await BaseUtility.GetUtilityMethods<IUtilityMethodDatas>().GetDatasAsync(
                            AddressFormater.FormatAddress(communicateAddress.Area, communicateAddress.Address, 0),
                            (int)
                            Math.Ceiling(communicateAddress.GetCount *
                                         ValueHelper.ByteLength[
                                             communicateAddress.DataType.FullName]),
                            communicateAddress.GetOriginalCount);

                    var valueHelper = ValueHelper.GetInstance(BaseUtility.Endian);
                    //如果设备本身能获取到数据但是没有数据
                    var datas = datasReturn;

                    //没有返回，直接设0
                    if (datas.IsSuccess == null)
                    {
                        datas.Datas = new byte[(int)
                            Math.Ceiling(communicateAddress.GetCount *
                                         ValueHelper.ByteLength[
                                             communicateAddress.DataType.FullName])];
                    }
                    //如果没有数据，终止
                    if (datas.IsSuccess == false || datas.Datas == null)
                    {
                        return new ReturnStruct<bool>()
                        {
                            Datas = false,
                            IsSuccess = false,
                            ErrorCode = datas.ErrorCode,
                            ErrorMsg = datas.ErrorMsg
                        };
                    }
                    else if (datas.Datas.Length <
                    (int)
                    Math.Ceiling(communicateAddress.GetCount *
                                 ValueHelper.ByteLength[
                                     communicateAddress.DataType.FullName]))
                        return new ReturnStruct<bool>()
                        {
                            Datas = false,
                            IsSuccess = false,
                            ErrorCode = -2,
                            ErrorMsg = "Data length not match"
                        };

                    foreach (var addressUnit in communicateAddress.OriginalAddresses)
                    {
                        //字节坐标地址
                        var byteCount =
                            AddressHelper.MapProtocolGetCountToAbstractByteCount(
                                addressUnit.Address - communicateAddress.Address +
                                addressUnit.SubAddress * 0.125 /
                                AddressTranslator.GetAreaByteLength(communicateAddress.Area),
                                AddressTranslator.GetAreaByteLength(communicateAddress.Area), 0);
                        //字节坐标主地址
                        var mainByteCount = (int)byteCount;
                        //字节坐标自地址
                        var localByteCount = (int)((byteCount - (int)byteCount) * 8);

                        //协议坐标地址
                        var localPos = byteCount / AddressTranslator.GetAreaByteLength(communicateAddress.Area);
                        //协议坐标子地址
                        var subPos =
                            (int)
                            ((localPos - (int)localPos) /
                             (0.125 / AddressTranslator.GetAreaByteLength(communicateAddress.Area)));
                        //协议主地址字符串
                        var address = AddressFormater.FormatAddress(communicateAddress.Area,
                            communicateAddress.Address + (int)localPos, subPos);
                        //协议完整地址字符串
                        var address2 = subPos != 0
                            ? null
                            : AddressFormater.FormatAddress(communicateAddress.Area,
                                communicateAddress.Address + (int)localPos);
                        //获取写入类型
                        var dataType = addressUnit.DataType;
                        KeyValuePair<string, double> value;
                        switch (setDataType)
                        {
                            case MachineDataType.Address:
                                {
                                    //获取要写入的值
                                    value =
                                        values.SingleOrDefault(
                                            p => p.Key == address || address2 != null && p.Key == address2);
                                    break;
                                }
                            case MachineDataType.CommunicationTag:
                                {
                                    value = values.SingleOrDefault(p => p.Key == addressUnit.CommunicationTag);
                                    break;
                                }
                            case MachineDataType.Name:
                                {
                                    value = values.SingleOrDefault(p => p.Key == addressUnit.Name);
                                    break;
                                }
                            case MachineDataType.Id:
                                {
                                    value = values.SingleOrDefault(p => p.Key == addressUnit.Id.ToString());
                                    break;
                                }
                            default:
                                {
                                    value = values.SingleOrDefault(p => p.Key == addressUnit.CommunicationTag);
                                    break;
                                }
                        }
                        //将要写入的值加入队列
                        var data = Convert.ChangeType(value.Value / addressUnit.Zoom, dataType);

                        if (!valueHelper.SetValue(datas.Datas, mainByteCount, localByteCount, data))
                            return new ReturnStruct<bool>()
                            {
                                Datas = false,
                                IsSuccess = false,
                                ErrorCode = -3,
                                ErrorMsg = "Data translation mismatch"
                            }; ;
                    }
                    //写入数据
                    await
                        BaseUtility.GetUtilityMethods<IUtilityMethodDatas>().SetDatasAsync(addressStart,
                            valueHelper.ByteArrayToObjectArray(datas.Datas,
                                new KeyValuePair<Type, int>(communicateAddress.DataType, communicateAddress.GetCount)), communicateAddress.GetOriginalCount);
                }
                //如果不保持连接，断开连接
                if (!KeepConnect)
                    BaseUtility.Disconnect();
            }
            catch (Exception e)
            {
                ErrorCount++;
                logger.LogError(e, $"BaseMachine -> SetDatas, Id:{Id} Connection:{ConnectionToken} error. ErrorCount {ErrorCount}.");

                if (ErrorCount >= _maxErrorCount)
                    Disconnect();
                return new ReturnStruct<bool>()
                {
                    Datas = false,
                    IsSuccess = false,
                    ErrorCode = -100,
                    ErrorMsg = "Unknown Exception"
                };
            }
            return new ReturnStruct<bool>()
            {
                Datas = true,
                IsSuccess = true,
                ErrorCode = 0,
                ErrorMsg = ""
            };
        }
    }

    /// <summary>
    ///     设备
    /// </summary>
    /// <typeparam name="TKey">设备的Id类型</typeparam>
    /// <typeparam name="TUnitKey">设备中使用的AddressUnit的Id类型</typeparam>
    /// <typeparam name="TAddressKey">设备中使用的AddressUnit的Address类型</typeparam>
    /// <typeparam name="TSubAddressKey">设备中使用的AddressUnit的SubAddress类型</typeparam>
    public abstract class BaseMachine<TKey, TUnitKey, TAddressKey, TSubAddressKey> : IMachine<TKey>
        where TKey : IEquatable<TKey>
        where TUnitKey : IEquatable<TUnitKey>
        where TAddressKey : IEquatable<TAddressKey>
        where TSubAddressKey : IEquatable<TSubAddressKey>
    {
        private static readonly ILogger<BaseMachine<TKey, TUnitKey, TAddressKey, TSubAddressKey>> logger = LogProvider.CreateLogger<BaseMachine<TKey, TUnitKey, TAddressKey, TSubAddressKey>>();

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="id">设备的ID号</param>
        /// <param name="getAddresses">需要与设备通讯的地址</param>
        protected BaseMachine(TKey id, IEnumerable<AddressUnit<TUnitKey, TAddressKey, TSubAddressKey>> getAddresses)
            : this(id, getAddresses, false)
        {
        }

        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="id">设备的ID号</param>
        /// <param name="getAddresses">需要与设备通讯的地址</param>
        /// <param name="keepConnect">是否保持连接</param>
        protected BaseMachine(TKey id, IEnumerable<AddressUnit<TUnitKey, TAddressKey, TSubAddressKey>> getAddresses, bool keepConnect)
        {
            Id = id;
            GetAddresses = getAddresses;
            KeepConnect = keepConnect;
        }

        private readonly int _maxErrorCount = 3;

        private int ErrorCount { get; set; }

        /// <summary>
        ///     是否处于连接状态
        /// </summary>
        public bool IsConnected => BaseUtility.IsConnected;

        /// <summary>
        ///     是否保持连接
        /// </summary>
        public bool KeepConnect { get; set; }

        /// <summary>
        ///     设备的连接器
        /// </summary>
        public IUtilityProperty BaseUtility { get; protected set; }

        /// <summary>
        ///     设备的Id
        /// </summary>
        public TKey Id { get; set; }

        /// <summary>
        ///     设备所在工程的名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        ///     设备的名称
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        ///     标识设备的连接关键字
        /// </summary>
        public string ConnectionToken => BaseUtility.ConnectionToken;

        /// <summary>
        ///     描述需要与设备通讯的地址
        /// </summary>
        private IEnumerable<AddressUnit<TUnitKey, TAddressKey, TSubAddressKey>> getAddresses;

        private object getAddressesLock = new object();

        /// <summary>
        ///     描述需要与设备通讯的地址
        /// </summary>
        public IEnumerable<AddressUnit<TUnitKey, TAddressKey, TSubAddressKey>> GetAddresses
        {
            get
            {
                return getAddresses;
            }
            set
            {
                lock (getAddressesLock)
                {
                    getAddresses = value;
                }
            }
        }

        /// <summary>
        ///     地址编码器
        /// </summary>
        public AddressFormater<TAddressKey, TSubAddressKey> AddressFormater { get; set; }

        /// <summary>
        ///     地址转换器
        /// </summary>
        public AddressTranslator AddressTranslator
        {
            get => BaseUtility.AddressTranslator;
            set => BaseUtility.AddressTranslator = value;
        }

        /// <summary>
        ///     读取数据
        /// </summary>
        /// <returns>从设备读取的数据</returns>
        public async virtual Task<ReturnStruct<Dictionary<string, ReturnUnit<double>>>> GetDatasAsync(MachineDataType getDataType)
        {
            try
            {
                var ans = new Dictionary<string, ReturnUnit<double>>();
                //检测并连接设备
                if (!BaseUtility.IsConnected)
                    await BaseUtility.ConnectAsync();
                //如果无法连接，终止
                if (!BaseUtility.IsConnected) return
                        new ReturnStruct<Dictionary<string, ReturnUnit<double>>>()
                        {
                            Datas = null,
                            IsSuccess = false,
                            ErrorCode = -1,
                            ErrorMsg = "Connection Error"
                        };
                //遍历每一个实际向设备获取数据的连续地址
                foreach (var address in GetAddresses)
                {
                    //获取数据
                    var datas =
                        await
                            BaseUtility.GetUtilityMethods<IUtilityMethodDatas>().GetDatasAsync(
                                AddressFormater.FormatAddress(address.Area, address.Address,
                                    address.SubAddress),
                                (int)
                                Math.Ceiling(ValueHelper.ByteLength[
                                                 address.DataType.FullName]),
                                (int)
                                Math.Ceiling(ValueHelper.ByteLength[
                                                 address.DataType.FullName]));


                    //如果没有数据，终止
                    if (datas.IsSuccess == false || datas.Datas == null)
                    {
                        return new ReturnStruct<Dictionary<string, ReturnUnit<double>>>()
                        {
                            Datas = null,
                            IsSuccess = false,
                            ErrorCode = datas.ErrorCode,
                            ErrorMsg = datas.ErrorMsg
                        };
                    }
                    else if (datas.Datas.Length != 0 && datas.Datas.Length <
                        (int)
                        Math.Ceiling(ValueHelper.ByteLength[
                                         address.DataType.FullName]))
                    {
                        return new ReturnStruct<Dictionary<string, ReturnUnit<double>>>()
                        {
                            Datas = null,
                            IsSuccess = false,
                            ErrorCode = -2,
                            ErrorMsg = "Data length mismatch"
                        };
                    }

                    //字节坐标的主地址位置
                    var localMainPos = 0;
                    //字节坐标的子地址位置
                    var localSubPos = 0;

                    //根据类型选择返回结果的键是通讯标识还是地址
                    string key;
                    switch (getDataType)
                    {
                        case MachineDataType.CommunicationTag:
                            {
                                key = address.CommunicationTag;
                                break;
                            }
                        case MachineDataType.Address:
                            {
                                key = AddressFormater.FormatAddress(address.Area, address.Address, address.SubAddress);
                                break;
                            }
                        case MachineDataType.Name:
                            {
                                key = address.Name;
                                break;
                            }
                        case MachineDataType.Id:
                            {
                                key = address.Id.ToString();
                                break;
                            }
                        default:
                            {
                                key = address.CommunicationTag;
                                break;
                            }
                    }

                    try
                    {
                        //如果没有数据返回空
                        if (datas.Datas.Length == 0)
                            ans.Add(key, new ReturnUnit<double>
                            {
                                DeviceValue = null,
                                AddressUnit = address.MapAddressUnitTUnitKeyToAddressUnit(),
                            });
                        else
                            ans.Add(key,
                                new ReturnUnit<double>
                                {
                                    DeviceValue =
                                        Math.Round(Convert.ToDouble(
                                            ValueHelper.GetInstance(BaseUtility.Endian)
                                                .GetValue(datas.Datas, ref localMainPos, ref localSubPos,
                                                    address.DataType)) * address.Zoom, address.DecimalPos),
                                    AddressUnit = address.MapAddressUnitTUnitKeyToAddressUnit(),
                                });
                    }
                    catch (Exception e)
                    {
                        ErrorCount++;
                        logger.LogError(e, $"BaseMachine -> GetDatas, Id:{Id} Connection:{ConnectionToken} key {key} existing. ErrorCount {ErrorCount}.");

                        if (ErrorCount >= _maxErrorCount)
                            Disconnect();
                        return new ReturnStruct<Dictionary<string, ReturnUnit<double>>>()
                        {
                            Datas = null,
                            IsSuccess = false,
                            ErrorCode = -3,
                            ErrorMsg = "Data translation mismatch"
                        };
                    }
                }

                //如果不保持连接，断开连接
                if (!KeepConnect)
                    BaseUtility.Disconnect();
                //返回数据
                if (ans.All(p => p.Value.DeviceValue == null)) ans = null;
                ErrorCount = 0;
                return new ReturnStruct<Dictionary<string, ReturnUnit<double>>>
                {
                    Datas = ans,
                    IsSuccess = true,
                    ErrorCode = 0,
                    ErrorMsg = ""
                };
            }
            catch (Exception e)
            {
                ErrorCount++;
                logger.LogError(e, $"BaseMachine -> GetDatas, Id:{Id} Connection:{ConnectionToken} error. ErrorCount {ErrorCount}.");

                if (ErrorCount >= _maxErrorCount)
                    Disconnect();
                return new ReturnStruct<Dictionary<string, ReturnUnit<double>>>()
                {
                    Datas = null,
                    IsSuccess = false,
                    ErrorCode = -100,
                    ErrorMsg = "Unknown Exception"
                };
            }
        }

        /// <summary>
        ///     写入数据
        /// </summary>
        /// <param name="setDataType">写入类型</param>
        /// <param name="values">需要写入的数据字典，当写入类型为Address时，键为需要写入的地址，当写入类型为CommunicationTag时，键为需要写入的单元的描述</param>
        /// <returns>是否写入成功</returns>
        public async virtual Task<ReturnStruct<bool>> SetDatasAsync(MachineDataType setDataType, Dictionary<string, double> values)
        {
            try
            {
                //检测并连接设备
                if (!BaseUtility.IsConnected)
                    await BaseUtility.ConnectAsync();
                //如果设备无法连接，终止
                if (!BaseUtility.IsConnected) return new ReturnStruct<bool>()
                {
                    Datas = false,
                    IsSuccess = false,
                    ErrorCode = -1,
                    ErrorMsg = "Connection Error"
                };
                var addresses = new List<AddressUnit<TUnitKey, TAddressKey, TSubAddressKey>>();
                //遍历每个要设置的值
                foreach (var value in values)
                {
                    //根据设置类型找到对应的地址描述
                    AddressUnit<TUnitKey, TAddressKey, TSubAddressKey> address = null;
                    switch (setDataType)
                    {
                        case MachineDataType.Address:
                            {
                                address =
                                    GetAddresses.SingleOrDefault(
                                        p =>
                                            AddressFormater.FormatAddress(p.Area, p.Address, p.SubAddress) == value.Key ||
                                            p.DataType != typeof(bool) &&
                                            AddressFormater.FormatAddress(p.Area, p.Address) == value.Key);
                                break;
                            }
                        case MachineDataType.CommunicationTag:
                            {
                                address =
                                    GetAddresses.SingleOrDefault(p => p.CommunicationTag == value.Key);
                                break;
                            }
                        case MachineDataType.Name:
                            {
                                address = GetAddresses.SingleOrDefault(p => p.Name == value.Key);
                                break;
                            }
                        case MachineDataType.Id:
                            {
                                address = GetAddresses.SingleOrDefault(p => p.Id.ToString() == value.Key);
                                break;
                            }
                        default:
                            {
                                address =
                                    GetAddresses.SingleOrDefault(p => p.CommunicationTag == value.Key);
                                break;
                            }
                    }
                    //地址为空报错
                    if (address == null)
                    {
                        logger.LogError($"Machine {ConnectionToken} Address {value.Key} doesn't exist.");
                        continue;
                    }
                    //不能写报错
                    if (!address.CanWrite)
                    {
                        logger.LogError($"Machine {ConnectionToken} Address {value.Key} cannot write.");
                        continue;
                    }
                    addresses.Add(address);
                }

                //遍历每条通讯的地址


                var valueHelper = ValueHelper.GetInstance(BaseUtility.Endian);

                foreach (var addressUnit in addresses)
                {
                    //协议主地址字符串
                    var address = AddressFormater.FormatAddress(addressUnit.Area,
                        addressUnit.Address, addressUnit.SubAddress);
                    //获取写入类型
                    var dataType = addressUnit.DataType;
                    KeyValuePair<string, double> value;
                    switch (setDataType)
                    {
                        case MachineDataType.Address:
                            {
                                //获取要写入的值
                                value = values.SingleOrDefault(p => p.Key == address);
                                break;
                            }
                        case MachineDataType.CommunicationTag:
                            {
                                value = values.SingleOrDefault(p => p.Key == addressUnit.CommunicationTag);
                                break;
                            }
                        case MachineDataType.Name:
                            {
                                value = values.SingleOrDefault(p => p.Key == addressUnit.Name);
                                break;
                            }
                        case MachineDataType.Id:
                            {
                                value = values.SingleOrDefault(p => p.Key == addressUnit.Id.ToString());
                                break;
                            }
                        default:
                            {
                                value = values.SingleOrDefault(p => p.Key == addressUnit.CommunicationTag);
                                break;
                            }
                    }
                    //将要写入的值加入队列
                    var data = Convert.ChangeType(value.Value / addressUnit.Zoom, dataType);

                    //写入数据
                    await
                        BaseUtility.GetUtilityMethods<IUtilityMethodDatas>().SetDatasAsync(address,
                            new object[] { data } , 1);
                }
                //如果不保持连接，断开连接
                if (!KeepConnect)
                    BaseUtility.Disconnect();
            }
            catch (Exception e)
            {
                ErrorCount++;
                logger.LogError(e, $"BaseMachine -> SetDatas, Id:{Id} Connection:{ConnectionToken} error. ErrorCount {ErrorCount}.");

                if (ErrorCount >= _maxErrorCount)
                    Disconnect();
                return new ReturnStruct<bool>()
                {
                    Datas = false,
                    IsSuccess = false,
                    ErrorCode = -100,
                    ErrorMsg = "Unknown Exception"
                };
            }
            return new ReturnStruct<bool>()
            {
                Datas = true,
                IsSuccess = true,
                ErrorCode = 0,
                ErrorMsg = ""
            };
        }

        /// <summary>
        ///     通过Id获取数据字段定义
        /// </summary>
        /// <param name="addressUnitId">数据字段Id</param>
        /// <returns>数据字段</returns>
        public AddressUnit<TUnitKey, TAddressKey, TSubAddressKey> GetAddressUnitById(TUnitKey addressUnitId)
        {
            try
            {
                return GetAddresses.SingleOrDefault(p => p.Id.Equals(addressUnitId));
            }
            catch (Exception e)
            {
                logger.LogError(e, $"BaseMachine -> GetAddressUnitById Id:{Id} ConnectionToken:{ConnectionToken} addressUnitId:{addressUnitId} Repeated");
                return null;
            }
        }

        /// <summary>
        ///     获取设备的方法集合
        /// </summary>
        /// <typeparam name="TMachineMethod">方法集合的类型</typeparam>
        /// <returns>设备的方法集合</returns>
        public TMachineMethod GetMachineMethods<TMachineMethod>() where TMachineMethod : class, IMachineMethod
        {
            if (this is TMachineMethod)
            {
                return this as TMachineMethod;
            }
            return null;
        }

        /// <summary>
        ///     获取Utility的执行方法
        /// </summary>
        /// <typeparam name="TUtilityMethod">Utility实现的接口名称</typeparam>
        /// <returns></returns>
        public TUtilityMethod GetUtilityMethods<TUtilityMethod>() where TUtilityMethod : class, IUtilityMethod
        {
            return BaseUtility as TUtilityMethod;
        }

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>是否连接成功</returns>
        public async Task<bool> ConnectAsync()
        {
            return await BaseUtility.ConnectAsync();
        }

        /// <summary>
        ///     断开设备
        /// </summary>
        /// <returns>是否断开成功</returns>
        public bool Disconnect()
        {
            return BaseUtility.Disconnect();
        }
    }

    internal class BaseMachineEqualityComparer<TKey> : IEqualityComparer<IMachineProperty<TKey>>
        where TKey : IEquatable<TKey>
    {
        public bool Equals(IMachineProperty<TKey> x, IMachineProperty<TKey> y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(IMachineProperty<TKey> obj)
        {
            return obj.GetHashCode();
        }
    }

    /// <summary>
    ///     通讯单元
    /// </summary>
    public class CommunicationUnit<TKey, TAddressKey, TSubAddressKey> where TKey : IEquatable<TKey> where TAddressKey : IEquatable<TAddressKey> where TSubAddressKey : IEquatable<TSubAddressKey>
    {
        /// <summary>
        ///     区域
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        ///     地址
        /// </summary>
        public TAddressKey Address { get; set; }

        /// <summary>
        ///     子地址
        /// </summary>
        public TSubAddressKey SubAddress { get; set; }

        /// <summary>
        ///     获取个数
        /// </summary>
        public int GetCount { get; set; }

        /// <summary>
        ///     获取原始个数
        /// </summary>
        public int GetOriginalCount { get; set; }

        /// <summary>
        ///     数据类型
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        ///     原始的地址
        /// </summary>
        public IEnumerable<AddressUnit<TKey, TAddressKey, TSubAddressKey>> OriginalAddresses { get; set; }
    }

    /// <summary>
    ///     返回的数据单元
    /// </summary>
    public class ReturnUnit<TReturn> where TReturn : struct
    {
        /// <summary>
        ///     返回的数据
        /// </summary>
        public TReturn? DeviceValue { get; set; }

        /// <summary>
        ///     数据定义
        /// </summary>
        public AddressUnit<string, string, string> AddressUnit { get; set; }
    }

    /// <summary>
    ///     地址单元
    /// </summary>
    public class AddressUnit<TKey, TAddressKey, TSubAddressKey> : IEquatable<AddressUnit<TKey, TAddressKey, TSubAddressKey>> where TKey : IEquatable<TKey> where TAddressKey : IEquatable<TAddressKey> where TSubAddressKey : IEquatable<TSubAddressKey>
    {
        /// <summary>
        ///     数据单元Id
        /// </summary>
        public TKey Id { get; set; }

        /// <summary>
        ///     数据所属的区域
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        ///     地址
        /// </summary>
        public TAddressKey Address { get; set; }

        /// <summary>
        ///     bit位地址
        /// </summary>
        public TSubAddressKey SubAddress { get; set; }

        /// <summary>
        ///     数据类型
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        ///     放缩比例
        /// </summary>
        public double Zoom { get; set; } = 1;

        /// <summary>
        ///     小数位数
        /// </summary>
        public int DecimalPos { get; set; }

        /// <summary>
        ///     通讯标识名称
        /// </summary>
        public string CommunicationTag { get; set; }

        /// <summary>
        ///     名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        ///     是否可写，默认可写
        /// </summary>
        public bool CanWrite { get; set; } = true;

        /// <summary>
        ///     两个地址是否一致
        /// </summary>
        /// <param name="other">另一个地址</param>
        /// <returns>是否一致</returns>
        public bool Equals(AddressUnit<TKey, TAddressKey, TSubAddressKey> other)
        {
            return Area.ToUpper() == other.Area.ToUpper() && Address.Equals(other.Address) || Id.Equals(other.Id);
        }
    }
}