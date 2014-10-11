using System;
using System.Collections.Generic;
using System.Linq;
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

        public AddressTranslator AddressTranslator { get; set; }

        protected BaseUtility()
        {
            AddressTranslator = new AddressTranslatorBase();
        }
        /// <summary>
        /// 设置连接字符串
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public abstract void SetConnectionString(string connectionString);
        /// <summary>
        /// 设置连接类型
        /// </summary>
        /// <param name="connectionType">连接类型</param>
        public abstract void SetConnectionType(int connectionType);
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="belongAddress">从站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getCount">接收个数</param>
        /// <returns>接收到的byte数据</returns>
        public abstract byte[] GetDatas(byte belongAddress, string startAddress, int getCount);
        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="belongAddress">从站地址</param>
        /// <param name="startAddress">开始地址</param>
        /// <param name="setContents">设置数据</param>
        /// <returns>是否设置成功</returns>
        public abstract bool SetDatas(byte belongAddress, string startAddress, object[] setContents);

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
    }
}
