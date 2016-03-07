using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Modbus.Net
{
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
        /// <summary>
        /// 设备的Id
        /// </summary>
        public int Id { get; set; }

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
        /// 地址解码器
        /// </summary>
        public AddressCombiner AddressCombiner { get; set; }

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
        protected IEnumerable<CommunicationUnit> CommunicateAddresses => AddressCombiner.Combine(GetAddresses);

        /// <summary>
        /// 描述需要与设备通讯的地址
        /// </summary>
        public IEnumerable<AddressUnit> GetAddresses { get; set; }

        /// <summary>
        /// 是否保持连接
        /// </summary>
        public bool KeepConnect { get; set; }

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
        /// 读取数据
        /// </summary>
        /// <returns>从设备读取的数据</returns>
        public Dictionary<string, ReturnUnit> GetDatas()
        {
            return AsyncHelper.RunSync(GetDatasAsync);
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns>从设备读取的数据</returns>
        public async Task<Dictionary<string,ReturnUnit>> GetDatasAsync()
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
                    var datasReturn =
                        await
                            BaseUtility.GetDatasAsync(2, 0,
                                AddressFormater.FormatAddress(communicateAddress.Area, communicateAddress.Address),
                                (int)
                                    Math.Ceiling(communicateAddress.GetCount*
                                                 BigEndianValueHelper.Instance.ByteLength[
                                                     communicateAddress.DataType.FullName]));
                    var datas = datasReturn.ReturnValue;

                    //如果没有数据，终止
                    if (datas == null || datas.Length == 0 || datas.Length != 
                                (int)
                                    Math.Ceiling(communicateAddress.GetCount *
                                                 BigEndianValueHelper.Instance.ByteLength[
                                                     communicateAddress.DataType.FullName])) return null;
                    int pos = 0;
                    //解码数据
                    while (pos < communicateAddress.GetCount)
                    {
                        //获取地址
                        var address =
                            GetAddresses.SingleOrDefault(
                                p => p.Area == communicateAddress.Area && p.Address == pos + communicateAddress.Address);
                        if (address != null)
                        {
                            //将获取的数据和对应的通讯标识对应
                            ans.Add(address.CommunicationTag,
                                new ReturnUnit
                                {
                                    PlcValue =
                                        Double.Parse(
                                            datasReturn.IsLittleEndian ? ValueHelper.Instance.GetValue(datas, ref pos, address.DataType)
                                                .ToString() : BigEndianValueHelper.Instance.GetValue(datas, ref pos, address.DataType)
                                                .ToString()) *address.Zoom,
                                    UnitExtend = address.UnitExtend
                                });
                        }
                        else
                        {
                            pos++;
                        }
                    }
                }
                //如果不保持连接，断开连接
                if (!KeepConnect)
                {
                    BaseUtility.Disconnect();
                }
                //返回数据
                if (ans.Count == 0) ans = null;
                return ans;
            }
            catch (Exception e)
            {
                Console.WriteLine(ConnectionToken + " " + e.Message);
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
                                GetAddresses.SingleOrDefault(p => AddressFormater.FormatAddress(p.Area, p.Address) == value.Key);
                            break;
                        }
                        case MachineSetDataType.CommunicationTag:
                        {
                            address =
                                GetAddresses.SingleOrDefault(p => p.CommunicationTag == value.Key);
                            break;
                        }
                    }
                    if (address == null) return false;
                    addresses.Add(address);
                }
                //将地址编码成与实际设备通讯的地址，注意这个地址必须是连续的
                var communcationUnits = new AddressCombinerContinus().Combine(addresses);
                //遍历每条通讯的连续地址
                foreach (var communicateAddress in communcationUnits)
                {
                    List<object> datasList = new List<object>();
                    //需要设置的字节数，计数
                    var setCount = (int)
                        Math.Ceiling(communicateAddress.GetCount*
                                     BigEndianValueHelper.Instance.ByteLength[
                                         communicateAddress.DataType.FullName]);
                    //总数
                    var allBytes = setCount;
                    //编码开始地址
                    var addressStart = AddressFormater.FormatAddress(communicateAddress.Area,
                            communicateAddress.Address);
                    while (setCount > 0)
                    {
                        //编码当前地址
                        var address = AddressFormater.FormatAddress(communicateAddress.Area,
                            communicateAddress.Address + allBytes - setCount);
                        //找到对应的描述地址
                        var addressUnit =
                            GetAddresses.SingleOrDefault(
                                p =>
                                    p.Area == communicateAddress.Area &&
                                    p.Address == communicateAddress.Address + allBytes - setCount);
                        //如果没有相应地址，跳过
                        if (addressUnit == null) continue;
                        //获取写入类型
                        Type dataType = addressUnit.DataType;
                        switch (setDataType)
                        {
                            case MachineSetDataType.Address:
                            {
                                //获取要写入的值
                                var value = values.SingleOrDefault(p => p.Key == address);
                                //将要写入的值加入队列
                                datasList.Add(Convert.ChangeType(value.Value, dataType));
                                break;
                            }
                            case MachineSetDataType.CommunicationTag:
                            {
                                var value = values.SingleOrDefault(p => p.Key == addressUnit.CommunicationTag);
                                datasList.Add(Convert.ChangeType(value.Value, dataType));
                                break;
                            }
                        }
                        setCount -= (int) BigEndianValueHelper.Instance.ByteLength[dataType.FullName];
                    }
                    //写入数据
                    await BaseUtility.SetDatasAsync(2, 0, addressStart, datasList.ToArray());
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
            return (from getValue in getValues
                select new KeyValuePair<string, double>(getValue.Key, getValue.Value.PlcValue)).ToDictionary(p=>p.Key,p=>p.Value);
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
        public double PlcValue { get; set; }
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
        public int Id { get; set; }
        /// <summary>
        /// 数据所属的区域
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public int Address { get; set; }
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
        int Id { get; set; }
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
