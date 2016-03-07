using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Modbus.Net
{
    public class GetDataReturnDef
    {
        public byte[] ReturnValue { get; set; }
        public bool IsLittleEndian { get; set; }
    }

    public abstract class BaseUtility
    {
        /// <summary>
        /// 协议收发主体
        /// </summary>
        protected BaseProtocal Wrapper;
        protected string ConnectionString { get; set; }

        /// <summary>
        /// 设备是否已经连接
        /// </summary>
        public bool IsConnected => Wrapper?.ProtocalLinker != null && Wrapper.ProtocalLinker.IsConnected;

        /// <summary>
        /// 标识设备的连接关键字
        /// </summary>
        public string ConnectionToken => Wrapper.ProtocalLinker.ConnectionToken;

        /// <summary>
        /// 地址翻译器
        /// </summary>
        public AddressTranslator AddressTranslator { get; set; }

        /// <summary>
        /// 构造器
        /// </summary>
        protected BaseUtility()
        {
            AddressTranslator = new AddressTranslatorBase();
        }

        /// <summary>
        /// 设置连接类型
        /// </summary>
        /// <param name="connectionType">连接类型</param>
        public abstract void SetConnectionType(int connectionType);

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="belongAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getByteCount">获取字节数个数</param>
        /// <returns>接收到的byte数据</returns>
        public virtual GetDataReturnDef GetDatas(byte belongAddress, byte masterAddress, string startAddress, int getByteCount)
        {
            return AsyncHelper.RunSync(() => GetDatasAsync(belongAddress, masterAddress, startAddress, getByteCount));
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="belongAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getByteCount">获取字节数个数</param>
        /// <returns>接收到的byte数据</returns>
        public abstract Task<GetDataReturnDef> GetDatasAsync(byte belongAddress, byte masterAddress, string startAddress, int getByteCount);

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="belongAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getTypeAndCount">获取类型和个数</param>
        /// <returns>接收到的对应的类型和数据</returns>
        public virtual object[] GetDatas(byte belongAddress, byte masterAddress, string startAddress,
            KeyValuePair<Type, int> getTypeAndCount)
        {
            return AsyncHelper.RunSync(() => GetDatasAsync(belongAddress, masterAddress, startAddress, getTypeAndCount));
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="belongAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getTypeAndCount">获取类型和个数</param>
        /// <returns>接收到的对应的类型和数据</returns>
        public virtual async Task<object[]> GetDatasAsync(byte belongAddress, byte masterAddress, string startAddress,
            KeyValuePair<Type, int> getTypeAndCount)
        {
            try
            {
                string typeName = getTypeAndCount.Key.FullName;
                double bCount = BigEndianValueHelper.Instance.ByteLength[typeName];
                var getReturnValue = await GetDatasAsync(belongAddress, masterAddress, startAddress,
                    (int)Math.Ceiling(bCount * getTypeAndCount.Value));
                var getBytes = getReturnValue.ReturnValue;
                return getReturnValue.IsLittleEndian
                    ? ValueHelper.Instance.ByteArrayToObjectArray(getBytes, getTypeAndCount)
                    : BigEndianValueHelper.Instance.ByteArrayToObjectArray(getBytes, getTypeAndCount);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T">需要接收的类型</typeparam>
        /// <param name="belongAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getByteCount">获取字节数个数</param>
        /// <returns>接收到的对应的类型和数据</returns>
        public virtual T[] GetDatas<T>(byte belongAddress, byte masterAddress, string startAddress,
            int getByteCount)
        {
            return AsyncHelper.RunSync(() => GetDatasAsync<T>(belongAddress, masterAddress, startAddress, getByteCount));
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T">需要接收的类型</typeparam>
        /// <param name="belongAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getByteCount">获取字节数个数</param>
        /// <returns>接收到的对应的类型和数据</returns>
        public virtual async Task<T[]> GetDatasAsync<T>(byte belongAddress, byte masterAddress, string startAddress,
            int getByteCount)
        {
            try
            {
                var getBytes = await GetDatasAsync(belongAddress, masterAddress, startAddress,
                    new KeyValuePair<Type, int>(typeof(T), getByteCount));
                return BigEndianValueHelper.Instance.ObjectArrayToDestinationArray<T>(getBytes);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="belongAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getTypeAndCountList">获取类型和个数的队列</param>
        /// <returns>获取数据的对象数组，请强制转换成相应类型</returns>
        public virtual object[] GetDatas(byte belongAddress, byte masterAddress, string startAddress,
            IEnumerable<KeyValuePair<Type, int>> getTypeAndCountList)
        {
            return
                AsyncHelper.RunSync(() => GetDatasAsync(belongAddress, masterAddress, startAddress, getTypeAndCountList));
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="belongAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getTypeAndCountList">获取类型和个数的队列</param>
        public virtual async Task<object[]> GetDatasAsync(byte belongAddress, byte masterAddress, string startAddress,
            IEnumerable<KeyValuePair<Type, int>> getTypeAndCountList)
        {
            try
            {
                var translateTypeAndCount = getTypeAndCountList as IList<KeyValuePair<Type, int>> ??
                                            getTypeAndCountList.ToList();
                int bAllCount = (
                    from getTypeAndCount in translateTypeAndCount
                    let typeName = getTypeAndCount.Key.FullName
                    let bCount = BigEndianValueHelper.Instance.ByteLength[typeName]
                    select (int) Math.Ceiling(bCount*getTypeAndCount.Value)).Sum();
                var getReturnValue = await GetDatasAsync(belongAddress, masterAddress, startAddress, bAllCount);
                byte[] getBytes = getReturnValue.ReturnValue;
                return getReturnValue.IsLittleEndian
                    ? ValueHelper.Instance.ByteArrayToObjectArray(getBytes, translateTypeAndCount)
                    : BigEndianValueHelper.Instance.ByteArrayToObjectArray(getBytes, translateTypeAndCount);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="belongAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="setContents">设置数据</param>
        /// <returns>是否设置成功</returns>
        public virtual bool SetDatas(byte belongAddress, byte masterAddress, string startAddress, object[] setContents)
        {
            return AsyncHelper.RunSync(() => SetDatasAsync(belongAddress, masterAddress, startAddress, setContents));
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="belongAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="setContents">设置数据</param>
        /// <returns>是否设置成功</returns>
        public abstract Task<bool> SetDatasAsync(byte belongAddress, byte masterAddress, string startAddress, object[] setContents);
        
        /*
        /// <summary>
        /// 获取PLC时间
        /// </summary>
        /// <param name="belongAddress">从站地址</param>
        /// <returns>PLC时间</returns>
        public abstract DateTime GetTime(byte belongAddress);

        /// <summary>
        /// 设置PLC时间
        /// </summary>
        /// <param name="belongAddress">从站地址</param>
        /// <param name="setTime">设置PLC时间</param>
        /// <returns>设置是否成功</returns>
        public abstract bool SetTime(byte belongAddress, DateTime setTime);
        */

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        public bool Connect()
        {
            return Wrapper.Connect();
        }

        /// <summary>
        /// 连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        public async Task<bool> ConnectAsync()
        {
            return await Wrapper.ConnectAsync();
        }

        /// <summary>
        /// 断开设备
        /// </summary>
        /// <returns>设备是否断开成功</returns>
        public bool Disconnect()
        {
            return Wrapper.Disconnect();
        }

    }
}
