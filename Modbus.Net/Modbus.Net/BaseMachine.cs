using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    /// 获取设备值的方式
    /// </summary>
    public enum MachineGetDataType
    {
        /// <summary>
        /// 地址
        /// </summary>
        Address,
        /// <summary>
        /// 通讯标识
        /// </summary>
        CommunicationTag
    }

    /// <summary>
    /// 向设备设置值的方式
    /// </summary>
    public enum MachineSetDataType
    {
        /// <summary>
        /// 地址
        /// </summary>
        Address,
        /// <summary>
        /// 通讯标识
        /// </summary>
        CommunicationTag
    }

    public abstract class BaseMachine : IMachineProperty
    {
        private int ErrorCount { get; set; } = 0;
        private int _maxErrorCount = 3;

        /// <summary>
        /// 设备的Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 设备所在工程的名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// 设备的名称
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// 是否处于连接状态
        /// </summary>
        public bool IsConnected => BaseUtility.IsConnected;

        /// <summary>
        /// 标识设备的连接关键字
        /// </summary>
        public string ConnectionToken => BaseUtility.ConnectionToken;

        /// <summary>
        /// 地址编码器
        /// </summary>
        public AddressFormater AddressFormater { get; set; }

        /// <summary>
        /// 获取地址组合器
        /// </summary>
        public AddressCombiner AddressCombiner { get; set; }

        /// <summary>
        /// 写入地址组合器
        /// </summary>
        public AddressCombiner AddressCombinerSet { get; set; }

        /// <summary>
        /// 地址转换器
        /// </summary>
        public AddressTranslator AddressTranslator
        {
            get { return BaseUtility.AddressTranslator; }
            set { BaseUtility.AddressTranslator = value; }
        }

        /// <summary>
        /// 与设备实际通讯的连续地址
        /// </summary>
        protected IEnumerable<CommunicationUnit> CommunicateAddresses => GetAddresses != null ? AddressCombiner.Combine(GetAddresses) : null;

        /// <summary>
        /// 描述需要与设备通讯的地址
        /// </summary>
        public IEnumerable<AddressUnit> GetAddresses { get; set; }

        /// <summary>
        /// 是否保持连接
        /// </summary>
        public bool KeepConnect { get; set; }

        /// <summary>
        /// 从站号
        /// </summary>
        public byte SlaveAddress { get; set; } = 2;

        /// <summary>
        /// 主站号
        /// </summary>
        public byte MasterAddress { get; set; } = 0;

        /// <summary>
        /// 设备的连接器
        /// </summary>
        protected BaseUtility BaseUtility { get; set; }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="getAddresses">需要与设备通讯的地址</param>
        protected BaseMachine(IEnumerable<AddressUnit> getAddresses)
            : this(getAddresses, false)
        {
        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="getAddresses">需要与设备通讯的地址</param>
        /// <param name="keepConnect">是否保持连接</param>
        protected BaseMachine(IEnumerable<AddressUnit> getAddresses, bool keepConnect)
        {
            GetAddresses = getAddresses;
            KeepConnect = keepConnect;
        }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="getAddresses">需要与设备通讯的地址</param>
        /// <param name="keepConnect">是否保持连接</param>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        protected BaseMachine(IEnumerable<AddressUnit> getAddresses, bool keepConnect, byte slaveAddress, byte masterAddress) : this(getAddresses, keepConnect)
        {
            SlaveAddress = slaveAddress;
            MasterAddress = masterAddress;
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns>从设备读取的数据</returns>
        public Dictionary<string, ReturnUnit> GetDatas(MachineGetDataType getDataType)
        {
            return AsyncHelper.RunSync(()=>GetDatasAsync(getDataType));
        }



        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns>从设备读取的数据</returns>
        public async Task<Dictionary<string, ReturnUnit>> GetDatasAsync(MachineGetDataType getDataType)
        {
            try
            {
                Dictionary<string, ReturnUnit> ans = new Dictionary<string, ReturnUnit>();
                //检测并连接设备
                if (!BaseUtility.IsConnected)
                {
                    await BaseUtility.ConnectAsync();
                }
                //如果无法连接，终止
                if (!BaseUtility.IsConnected) return null;
                //遍历每一个实际向设备获取数据的连续地址
                foreach (var communicateAddress in CommunicateAddresses)
                {
                    //获取数据
                    var datas =
                        await
                            BaseUtility.GetDatasAsync(
                                AddressFormater.FormatAddress(communicateAddress.Area, communicateAddress.Address, 0),
                                (int)
                                    Math.Ceiling(communicateAddress.GetCount*
                                                 BigEndianValueHelper.Instance.ByteLength[
                                                     communicateAddress.DataType.FullName]));


                    //如果没有数据，终止
                    if (datas == null || (datas.Length != 0 && datas.Length <
                        (int)
                            Math.Ceiling(communicateAddress.GetCount*
                                         BigEndianValueHelper.Instance.ByteLength[
                                             communicateAddress.DataType.FullName])))
                        return null;


                    foreach (var address in communicateAddress.OriginalAddresses)
                    {
                        var localPos = (address.Address - communicateAddress.Address)*
                                       AddressTranslator.GetAreaByteLength(communicateAddress.Area) +
                                       address.SubAddress/8.0;
                        var localMainPos = (int) localPos;
                        var localSubPos = (int) ((localPos - localMainPos)*8);

                        string key;
                        switch (getDataType)
                        {
                            case MachineGetDataType.CommunicationTag:
                            {
                                key = address.CommunicationTag;
                                break;
                            }
                            case MachineGetDataType.Address:
                            {
                                key = AddressFormater.FormatAddress(address.Area, address.Address, address.SubAddress);
                                break;
                            }
                            default:
                            {
                                key = address.CommunicationTag;
                                break;
                            }
                        }

                        if (datas.Length == 0)
                        {
                            ans.Add(key, new ReturnUnit
                            {
                                PlcValue = null,
                                UnitExtend = address.UnitExtend
                            });
                        }
                        else
                        {
                            //将获取的数据和对应的通讯标识对应
                            ans.Add(key,
                                new ReturnUnit
                                {
                                    PlcValue =
                                        Convert.ToDouble(
                                            BaseUtility.GetLittleEndian
                                                ? ValueHelper.Instance.GetValue(datas, ref localMainPos, ref localSubPos,
                                                    address.DataType)
                                                    .ToString()
                                                : BigEndianValueHelper.Instance.GetValue(datas, ref localMainPos,
                                                    ref localSubPos,
                                                    address.DataType))*address.Zoom,
                                    UnitExtend = address.UnitExtend
                                });
                        }
                    }
                }
                //如果不保持连接，断开连接
                if (!KeepConnect)
                {
                    BaseUtility.Disconnect();
                }
                //返回数据
                if (ans.All(p => p.Value.PlcValue == null)) ans = null;
                ErrorCount = 0;
                return ans;
            }
            catch (Exception e)
            {
                Console.WriteLine(ConnectionToken + " " + e.Message);
                ErrorCount++;
                if (ErrorCount >= _maxErrorCount)
                {
                    Disconnect();
                }
                return null;
            }
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="setDataType">写入类型</param>
        /// <param name="values">需要写入的数据字典，当写入类型为Address时，键为需要写入的地址，当写入类型为CommunicationTag时，键为需要写入的单元的描述</param>
        /// <returns>是否写入成功</returns>
        public bool SetDatas(MachineSetDataType setDataType, Dictionary<string, double> values)
        {
            return AsyncHelper.RunSync(() => SetDatasAsync(setDataType, values));
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="setDataType">写入类型</param>
        /// <param name="values">需要写入的数据字典，当写入类型为Address时，键为需要写入的地址，当写入类型为CommunicationTag时，键为需要写入的单元的描述</param>
        /// <returns>是否写入成功</returns>
        public async Task<bool> SetDatasAsync(MachineSetDataType setDataType, Dictionary<string, double> values)
        {
            try
            {
                //检测并连接设备
                if (!BaseUtility.IsConnected)
                {
                    await BaseUtility.ConnectAsync();
                }
                //如果设备无法连接，终止
                if (!BaseUtility.IsConnected) return false;
                List<AddressUnit> addresses = new List<AddressUnit>();
                //遍历每个要设置的值
                foreach (var value in values)
                {
                    //根据设置类型找到对应的地址描述
                    AddressUnit address = null;
                    switch (setDataType)
                    {
                        case MachineSetDataType.Address:
                        {
                            address =
                                GetAddresses.SingleOrDefault(
                                    p =>
                                        AddressFormater.FormatAddress(p.Area, p.Address, p.SubAddress) == value.Key ||
                                        (p.DataType != typeof (bool) &&
                                         AddressFormater.FormatAddress(p.Area, p.Address) == value.Key));
                            break;
                        }
                        case MachineSetDataType.CommunicationTag:
                        {
                            address =
                                GetAddresses.SingleOrDefault(p => p.CommunicationTag == value.Key);
                            break;
                        }
                    }
                    if (address == null)
                    {
                        Console.WriteLine($"Machine {ConnectionToken} Address {value.Key} doesn't exist.");
                        continue;
                    }
                    if (!address.CanWrite)
                    {
                        Console.WriteLine($"Machine {ConnectionToken} Address {value.Key} cannot write.");
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

                    var datasReturn = await BaseUtility.GetDatasAsync(
                        AddressFormater.FormatAddress(communicateAddress.Area, communicateAddress.Address, 0),
                        (int)
                            Math.Ceiling(communicateAddress.GetCount*
                                         BigEndianValueHelper.Instance.ByteLength[
                                             communicateAddress.DataType.FullName]));

                    var valueHelper = BaseUtility.SetLittleEndian
                        ? ValueHelper.Instance
                        : BigEndianValueHelper.Instance;
                    //如果设备本身能获取到数据但是没有数据
                    var datas = datasReturn;

                    //如果没有数据，终止
                    if (datas == null || datas.Length <
                        (int)
                            Math.Ceiling(communicateAddress.GetCount*
                                         BigEndianValueHelper.Instance.ByteLength[
                                             communicateAddress.DataType.FullName]))
                        return false;

                    foreach (var addressUnit in communicateAddress.OriginalAddresses)
                    {
                        var byteCount = (addressUnit.Address - communicateAddress.Address +
                                        addressUnit.SubAddress*0.125/
                                        AddressTranslator.GetAreaByteLength(communicateAddress.Area)) *
                                        AddressTranslator.GetAreaByteLength(communicateAddress.Area);
                        var mainByteCount = (int) byteCount;
                        var localByteCount = (int) ((byteCount - (int) byteCount)*8);

                        var localPos = byteCount/AddressTranslator.GetAreaByteLength(communicateAddress.Area);
                        //编码当前地址
                        var subPos =
                            (int)
                                ((localPos - (int) localPos)/
                                 (0.125/AddressTranslator.GetAreaByteLength(communicateAddress.Area)));
                        var address = AddressFormater.FormatAddress(communicateAddress.Area,
                            communicateAddress.Address + (int) localPos, subPos);
                        var address2 = subPos != 0
                            ? null
                            : AddressFormater.FormatAddress(communicateAddress.Area,
                                communicateAddress.Address + (int) localPos);
                        //获取写入类型
                        Type dataType = addressUnit.DataType;
                        switch (setDataType)
                        {
                            case MachineSetDataType.Address:
                            {
                                //获取要写入的值
                                var value =
                                    values.SingleOrDefault(
                                        p => p.Key == address || (address2 != null && p.Key == address2));
                                //将要写入的值加入队列
                                var data = Convert.ChangeType(value.Value/addressUnit.Zoom, dataType);

                                if (!valueHelper.SetValue(datas, mainByteCount, localByteCount, data))
                                    return false;
                                break;
                            }
                            case MachineSetDataType.CommunicationTag:
                            {
                                var value = values.SingleOrDefault(p => p.Key == addressUnit.CommunicationTag);
                                var data = Convert.ChangeType(value.Value/addressUnit.Zoom, dataType);
                                if (!valueHelper.SetValue(datas, mainByteCount, localByteCount, data))
                                    return false;
                                break;
                            }
                        }
                    }
                    //写入数据
                    if (AddressCombiner is AddressCombinerSingle)
                    {
                        await
                            BaseUtility.SetDatasAsync(addressStart,
                                valueHelper.ByteArrayToObjectArray(datas,
                                    new KeyValuePair<Type, int>(communicateAddress.DataType, 1)));
                    }
                    else
                    {
                        await
                            BaseUtility.SetDatasAsync(addressStart,
                                valueHelper.ByteArrayToObjectArray(datas,
                                    new KeyValuePair<Type, int>(typeof (byte), datas.Length)));
                    }
                }
                //如果不保持连接，断开连接
                if (!KeepConnect)
                {
                    BaseUtility.Disconnect();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(ConnectionToken + " " + e.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <returns>是否连接成功</returns>
        public bool Connect()
        {
            return BaseUtility.Connect();
        }

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <returns>是否连接成功</returns>
        public async Task<bool> ConnectAsync()
        {
            return await BaseUtility.ConnectAsync();
        }

        /// <summary>
        /// 断开设备
        /// </summary>
        /// <returns>是否断开成功</returns>
        public bool Disconnect()
        {
            return BaseUtility.Disconnect();
        }

        /// <summary>
        /// 将获取的数据转换成可以向设备写入的数据格式
        /// </summary>
        /// <param name="getValues">获取的数据</param>
        /// <returns>写入的数据</returns>
        public static Dictionary<string, double> MapGetValuesToSetValues(Dictionary<string, ReturnUnit> getValues)
        {
            if (getValues == null) return null;
            return (from getValue in getValues where getValue.Value.PlcValue != null
                select new KeyValuePair<string, double>(getValue.Key, getValue.Value.PlcValue.Value)).ToDictionary(p=>p.Key,p=>p.Value);
        } 
    }

    public class BaseMachineEqualityComparer : IEqualityComparer<BaseMachine>
    {
        public bool Equals(BaseMachine x, BaseMachine y)
        {
            return x.ConnectionToken == y.ConnectionToken;
        }

        public int GetHashCode(BaseMachine obj)
        {
            return obj.GetHashCode();
        }
    }

    /// <summary>
    /// 通讯单元
    /// </summary>
    public class CommunicationUnit
    {
        /// <summary>
        /// 区域
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public int Address { get; set; }
        /// <summary>
        /// 获取个数
        /// </summary>
        public int GetCount { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public Type DataType { get; set; }
        /// <summary>
        /// 原始的地址
        /// </summary>
        public IEnumerable<AddressUnit> OriginalAddresses { get; set; }
    }

    /// <summary>
    /// 数据单元扩展，返回数据时会同时将其返回
    /// </summary>
    public class UnitExtend
    {
        
    }

    /// <summary>
    /// 返回的数据单元
    /// </summary>
    public class ReturnUnit
    {
        /// <summary>
        /// 返回的数据
        /// </summary>
        public double? PlcValue { get; set; }
        /// <summary>
        /// 数据的扩展
        /// </summary>
        public UnitExtend UnitExtend { get; set; }
    }

    public class AddressUnit
    {
        /// <summary>
        /// 数据单元Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 数据所属的区域
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public int Address { get; set; }
        /// <summary>
        /// bit位地址
        /// </summary>
        public int SubAddress { get; set; } = 0;
        /// <summary>
        /// 数据类型
        /// </summary>
        public Type DataType { get; set; }
        /// <summary>
        /// 放缩比例
        /// </summary>
        public double Zoom { get; set; }
        /// <summary>
        /// 小数位数
        /// </summary>
        public int DecimalPos { get; set; }
        /// <summary>
        /// 通讯标识名称
        /// </summary>
        public string CommunicationTag { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 是否可写，默认可写
        /// </summary>
        public bool CanWrite { get; set; } = true;
        /// <summary>
        /// 扩展
        /// </summary>
        public UnitExtend UnitExtend { get; set; }
    }

    public struct AddressUnitEqualityComparer : IEqualityComparer<AddressUnit>
    {
        public bool Equals(AddressUnit x, AddressUnit y)
        {
            return x.Area.ToUpper() == y.Area.ToUpper() && x.Address == y.Address;
        }

        public int GetHashCode(AddressUnit obj)
        {
            return obj.GetHashCode();
        }
    }

    /// <summary>
    /// 设备的抽象
    /// </summary>
    public interface IMachineProperty
    {
        /// <summary>
        /// Id
        /// </summary>
        string Id { get; set; }
        /// <summary>
        /// 工程名
        /// </summary>
        string ProjectName { get; set; }
        /// <summary>
        /// 设备名
        /// </summary>
        string MachineName { get; set; }
        /// <summary>
        /// 标识设备的连接关键字
        /// </summary>
        string ConnectionToken { get; }
    }
}
