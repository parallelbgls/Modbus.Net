using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
        protected abstract byte[] GetDatas(byte belongAddress, byte masterAddress, string startAddress, int getByteCount);

        public virtual object[] GetDatas(byte belongAddress, byte masterAddress, string startAddress,
            KeyValuePair<Type, int> getTypeAndCount)
        {
            string typeName = getTypeAndCount.Key.FullName;
            double bCount = ValueHelper.Instance.ByteLength[typeName];
            byte[] getBytes = GetDatas(belongAddress, masterAddress, startAddress, (int)Math.Ceiling(bCount * getTypeAndCount.Value));
            return ValueHelper.Instance.ByteArrayToObjectArray(getBytes, getTypeAndCount);
        }

        public virtual T[] GetDatas<T>(byte belongAddress, byte masterAddress, string startAddress,
            int getByteCount)
        {
            var getBytes = GetDatas(belongAddress, masterAddress, startAddress, new KeyValuePair<Type, int>(typeof(T), getByteCount));
            return ValueHelper.Instance.ObjectArrayToDestinationArray<T>(getBytes);
        }

        public virtual object[] GetDatas(byte belongAddress, byte masterAddress, string startAddress,
            IEnumerable<KeyValuePair<Type, int>> getTypeAndCountList)
        {
            int bAllCount = 0;
            foreach (var getTypeAndCount in getTypeAndCountList)
            {
                string typeName = getTypeAndCount.Key.FullName;
                double bCount = ValueHelper.Instance.ByteLength[typeName];
                bAllCount += (int)Math.Ceiling(bCount*getTypeAndCount.Value);
            }
            byte[] getBytes = GetDatas(belongAddress, masterAddress, startAddress, bAllCount);
            return ValueHelper.Instance.ByteArrayToObjectArray(getBytes, getTypeAndCountList);
        }
        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="belongAddress">从站地址</param>
        /// <param name="masterAddress">主站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="setContents">设置数据</param>
        /// <returns>是否设置成功</returns>
        public abstract bool SetDatas(byte belongAddress, byte masterAddress, string startAddress, object[] setContents);

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

        public bool Connect()
        {
            return Wrapper.Connect();
        }

        public bool Disconnect()
        {
            return Wrapper.Disconnect();
        }
    }
}
