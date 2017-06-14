using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

/// <summary>
///     端格式
/// </summary>
public enum Endian
{
    /// <summary>
    ///     小端
    /// </summary>
    LittleEndianLsb,

    /// <summary>
    ///     大端-小端位
    /// </summary>
    BigEndianLsb,

    /// <summary>
    ///     大端-大端位
    /// </summary>
    BigEndianMsb
}

namespace Modbus.Net
{
    /// <summary>
    ///     基础Api入口
    /// </summary>
    public abstract class BaseUtility : BaseUtility<byte[], byte[], ProtocalUnit>
    {
        /// <summary>
        ///     构造器
        /// </summary>
        protected BaseUtility(byte slaveAddress, byte masterAddress) : base(slaveAddress, masterAddress)
        {
        }
    }

    /// <summary>
    ///     基础Api入口
    /// </summary>
    public abstract class BaseUtility<TParamIn, TParamOut, TProtocalUnit> : IUtilityProperty, IUtilityMethodData
        where TProtocalUnit : class, IProtocalFormatting<TParamIn, TParamOut> where TParamOut : class
    {
        /// <summary>
        ///     协议收发主体
        /// </summary>
        protected IProtocal<TParamIn, TParamOut, TProtocalUnit> Wrapper;

        /// <summary>
        ///     构造器
        /// </summary>
        protected BaseUtility(byte slaveAddress, byte masterAddress)
        {
            SlaveAddress = slaveAddress;
            MasterAddress = masterAddress;
            AddressTranslator = new AddressTranslatorBase();
        }

        /// <summary>
        ///     连接字符串
        /// </summary>
        protected string ConnectionString { get; set; }

        /// <summary>
        ///     从站号
        /// </summary>
        public byte SlaveAddress { get; set; }

        /// <summary>
        ///     主站号
        /// </summary>
        public byte MasterAddress { get; set; }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getByteCount">获取字节数个数</param>
        /// <returns>接收到的byte数据</returns>
        public virtual byte[] GetDatas(string startAddress, int getByteCount)
        {
            return AsyncHelper.RunSync(() => GetDatasAsync(startAddress, getByteCount));
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getByteCount">获取字节数个数</param>
        /// <returns>接收到的byte数据</returns>
        public abstract Task<byte[]> GetDatasAsync(string startAddress, int getByteCount);

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getTypeAndCount">获取类型和个数</param>
        /// <returns>接收到的对应的类型和数据</returns>
        public virtual object[] GetDatas(string startAddress,
            KeyValuePair<Type, int> getTypeAndCount)
        {
            return AsyncHelper.RunSync(() => GetDatasAsync(startAddress, getTypeAndCount));
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getTypeAndCount">获取类型和个数</param>
        /// <returns>接收到的对应的类型和数据</returns>
        public virtual async Task<object[]> GetDatasAsync(string startAddress,
            KeyValuePair<Type, int> getTypeAndCount)
        {
            try
            {
                var typeName = getTypeAndCount.Key.FullName;
                var bCount = BigEndianValueHelper.Instance.ByteLength[typeName];
                var getReturnValue = await GetDatasAsync(startAddress,
                    (int) Math.Ceiling(bCount * getTypeAndCount.Value));
                var getBytes = getReturnValue;
                return ValueHelper.GetInstance(Endian).ByteArrayToObjectArray(getBytes, getTypeAndCount);
            }
            catch (Exception e)
            {
                Log.Error(e, $"ModbusUtility -> GetDatas: {ConnectionString} error");
                return null;
            }
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <typeparam name="T">需要接收的类型</typeparam>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getByteCount">获取字节数个数</param>
        /// <returns>接收到的对应的类型和数据</returns>
        public virtual T[] GetDatas<T>(string startAddress,
            int getByteCount)
        {
            return AsyncHelper.RunSync(() => GetDatasAsync<T>(startAddress, getByteCount));
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <typeparam name="T">需要接收的类型</typeparam>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getByteCount">获取字节数个数</param>
        /// <returns>接收到的对应的类型和数据</returns>
        public virtual async Task<T[]> GetDatasAsync<T>(string startAddress,
            int getByteCount)
        {
            try
            {
                var getBytes = await GetDatasAsync(startAddress,
                    new KeyValuePair<Type, int>(typeof(T), getByteCount));
                return ValueHelper.GetInstance(Endian).ObjectArrayToDestinationArray<T>(getBytes);
            }
            catch (Exception e)
            {
                Log.Error(e, $"ModbusUtility -> GetDatas Generic: {ConnectionString} error");
                return null;
            }
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getTypeAndCountList">获取类型和个数的队列</param>
        /// <returns>获取数据的对象数组，请强制转换成相应类型</returns>
        public virtual object[] GetDatas(string startAddress,
            IEnumerable<KeyValuePair<Type, int>> getTypeAndCountList)
        {
            return
                AsyncHelper.RunSync(() => GetDatasAsync(startAddress, getTypeAndCountList));
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getTypeAndCountList">获取类型和个数的队列</param>
        public virtual async Task<object[]> GetDatasAsync(string startAddress,
            IEnumerable<KeyValuePair<Type, int>> getTypeAndCountList)
        {
            try
            {
                var translateTypeAndCount = getTypeAndCountList as IList<KeyValuePair<Type, int>> ??
                                            getTypeAndCountList.ToList();
                var bAllCount = (
                    from getTypeAndCount in translateTypeAndCount
                    let typeName = getTypeAndCount.Key.FullName
                    let bCount = BigEndianValueHelper.Instance.ByteLength[typeName]
                    select (int) Math.Ceiling(bCount * getTypeAndCount.Value)).Sum();
                var getReturnValue = await GetDatasAsync(startAddress, bAllCount);
                var getBytes = getReturnValue;
                return ValueHelper.GetInstance(Endian).ByteArrayToObjectArray(getBytes, translateTypeAndCount);
            }
            catch (Exception e)
            {
                Log.Error(e, $"ModbusUtility -> GetDatas pair: {ConnectionString} error");
                return null;
            }
        }

        /// <summary>
        ///     设置数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="setContents">设置数据</param>
        /// <returns>是否设置成功</returns>
        public virtual bool SetDatas(string startAddress, object[] setContents)
        {
            return AsyncHelper.RunSync(() => SetDatasAsync(startAddress, setContents));
        }

        /// <summary>
        ///     设置数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="setContents">设置数据</param>
        /// <returns>是否设置成功</returns>
        public abstract Task<bool> SetDatasAsync(string startAddress, object[] setContents);

        /// <summary>
        ///     协议是否遵循小端格式
        /// </summary>
        public abstract Endian Endian { get; }

        /// <summary>
        ///     设备是否已经连接
        /// </summary>
        public bool IsConnected => Wrapper?.ProtocalLinker != null && Wrapper.ProtocalLinker.IsConnected;

        /// <summary>
        ///     标识设备的连接关键字
        /// </summary>
        public string ConnectionToken
            => Wrapper?.ProtocalLinker == null ? ConnectionString : Wrapper.ProtocalLinker.ConnectionToken;

        /// <summary>
        ///     地址翻译器
        /// </summary>
        public AddressTranslator AddressTranslator { get; set; }

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        public bool Connect()
        {
            return Wrapper.Connect();
        }

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        public async Task<bool> ConnectAsync()
        {
            return await Wrapper.ConnectAsync();
        }

        /// <summary>
        ///     断开设备
        /// </summary>
        /// <returns>设备是否断开成功</returns>
        public bool Disconnect()
        {
            return Wrapper.Disconnect();
        }

        /// <summary>
        ///     返回Utility的方法集合
        /// </summary>
        /// <typeparam name="TUtilityMethod">Utility方法集合类型</typeparam>
        /// <returns>Utility方法集合</returns>
        public TUtilityMethod GetUtilityMethods<TUtilityMethod>() where TUtilityMethod : class, IUtilityMethod
        {
            if (this is TUtilityMethod)
            {
                return this as TUtilityMethod;
            }
            return null;
        }

        /// <summary>
        ///     设置连接类型
        /// </summary>
        /// <param name="connectionType">连接类型</param>
        public abstract void SetConnectionType(int connectionType);
    }

    /// <summary>
    ///     Api入口的抽象
    /// </summary>
    public interface IUtilityProperty
    {
        /// <summary>
        ///     协议是否遵循小端格式
        /// </summary>
        Endian Endian { get; }

        /// <summary>
        ///     设备是否已经连接
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        ///     标识设备的连接关键字
        /// </summary>
        string ConnectionToken { get; }

        /// <summary>
        ///     地址翻译器
        /// </summary>
        AddressTranslator AddressTranslator { get; set; }

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        bool Connect();

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        Task<bool> ConnectAsync();

        /// <summary>
        ///     断开设备
        /// </summary>
        /// <returns>设备是否断开成功</returns>
        bool Disconnect();

        /// <summary>
        ///     返回Utility的方法集合
        /// </summary>
        /// <typeparam name="TUtilityMethod">Utility方法集合类型</typeparam>
        /// <returns>Utility方法集合</returns>
        TUtilityMethod GetUtilityMethods<TUtilityMethod>() where TUtilityMethod : class, IUtilityMethod;
    }
}