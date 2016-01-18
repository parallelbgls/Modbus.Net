using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModBus.Net
{
    public abstract class BaseUtility
    {
        /// <summary>
        /// 协议收发主体
        /// </summary>
        protected BaseProtocal Wrapper;
        protected string ConnectionString { get; set; }

        public bool IsConnected
        {
            get
            {
                if (Wrapper == null || Wrapper.ProtocalLinker == null) return false;
                return Wrapper.ProtocalLinker.IsConnected;
            }
        }

        public string ConnectionToken
        {
            get { return Wrapper.ProtocalLinker.ConnectionToken; }
        }

        public AddressTranslator AddressTranslator { get; set; }

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
        /// <param name="getByteCount">获取类型和个数</param>
        /// <returns>接收到的byte数据</returns>
        protected virtual byte[] GetDatas(byte belongAddress, byte masterAddress, string startAddress, int getByteCount)
        {
            return AsyncHelper.RunSync(() => GetDatasAsync(belongAddress, masterAddress, startAddress, getByteCount));
        }

        /// <summary>
        /// 获取数据（异步）
        /// </summary>
        /// <param name="belongAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getByteCount">获取类型和个数</param>
        /// <returns>接收到的byte数据</returns>
        protected abstract Task<byte[]> GetDatasAsync(byte belongAddress, byte masterAddress, string startAddress, int getByteCount);

        public virtual object[] GetDatas(byte belongAddress, byte masterAddress, string startAddress,
            KeyValuePair<Type, int> getTypeAndCount)
        {
            try
            {
                string typeName = getTypeAndCount.Key.FullName;
                double bCount = BigEndianValueHelper.Instance.ByteLength[typeName];
                byte[] getBytes = GetDatas(belongAddress, masterAddress, startAddress,
                    (int) Math.Ceiling(bCount*getTypeAndCount.Value));
                return BigEndianValueHelper.Instance.ByteArrayToObjectArray(getBytes, getTypeAndCount);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task<object[]> GetDatasAsync(byte belongAddress, byte masterAddress, string startAddress,
            KeyValuePair<Type, int> getTypeAndCount)
        {
            try
            {
                string typeName = getTypeAndCount.Key.FullName;
                double bCount = BigEndianValueHelper.Instance.ByteLength[typeName];
                byte[] getBytes = await GetDatasAsync(belongAddress, masterAddress, startAddress,
                    (int) Math.Ceiling(bCount*getTypeAndCount.Value));
                return BigEndianValueHelper.Instance.ByteArrayToObjectArray(getBytes, getTypeAndCount);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual T[] GetDatas<T>(byte belongAddress, byte masterAddress, string startAddress,
            int getByteCount)
        {
            try
            {
                var getBytes = GetDatas(belongAddress, masterAddress, startAddress,
                    new KeyValuePair<Type, int>(typeof (T), getByteCount));
                return BigEndianValueHelper.Instance.ObjectArrayToDestinationArray<T>(getBytes);
            }
            catch (Exception)
            {
                return null;
            }
        }

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

        public virtual object[] GetDatas(byte belongAddress, byte masterAddress, string startAddress,
            IEnumerable<KeyValuePair<Type, int>> getTypeAndCountList)
        {
            try
            {
                int bAllCount = 0;
                foreach (var getTypeAndCount in getTypeAndCountList)
                {
                    string typeName = getTypeAndCount.Key.FullName;
                    double bCount = BigEndianValueHelper.Instance.ByteLength[typeName];
                    bAllCount += (int)Math.Ceiling(bCount * getTypeAndCount.Value);
                }
                byte[] getBytes = GetDatas(belongAddress, masterAddress, startAddress, bAllCount);
                return BigEndianValueHelper.Instance.ByteArrayToObjectArray(getBytes, getTypeAndCountList);
            }
            catch (Exception)
            {
                return null;
            }  
        }

        public virtual async Task<object[]> GetDatasAsync(byte belongAddress, byte masterAddress, string startAddress,
            IEnumerable<KeyValuePair<Type, int>> getTypeAndCountList)
        {
            try
            {
                int bAllCount = 0;
                foreach (var getTypeAndCount in getTypeAndCountList)
                {
                    string typeName = getTypeAndCount.Key.FullName;
                    double bCount = BigEndianValueHelper.Instance.ByteLength[typeName];
                    bAllCount += (int)Math.Ceiling(bCount * getTypeAndCount.Value);
                }
                byte[] getBytes = await GetDatasAsync(belongAddress, masterAddress, startAddress, bAllCount);
                return BigEndianValueHelper.Instance.ByteArrayToObjectArray(getBytes, getTypeAndCountList);
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
        /// 设置数据（异步）
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

        public bool Connect()
        {
            return Wrapper.Connect();
        }

        public async Task<bool> ConnectAsync()
        {
            return await Wrapper.ConnectAsync();
        }

        public bool Disconnect()
        {
            return Wrapper.Disconnect();
        }

    }
}
