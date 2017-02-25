using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     基本协议
    /// </summary>
    public abstract class BaseProtocal
    {
        /// <summary>
        ///     构造器
        /// </summary>
        protected BaseProtocal(byte slaveAddress, byte masterAddress)
        {
            Protocals = new Dictionary<string, ProtocalUnit>();
            SlaveAddress = slaveAddress;
            MasterAddress = masterAddress;
        }

        public byte SlaveAddress { get; set; }
        public byte MasterAddress { get; set; }

        /// <summary>
        ///     协议的连接器
        /// </summary>
        public ProtocalLinker ProtocalLinker { get; protected set; }

        /// <summary>
        ///     协议索引器，这是一个懒加载协议，当字典中不存在协议时自动加载协议，否则调用已经加载的协议
        /// </summary>
        /// <param name="type">协议的类的GetType</param>
        /// <returns>协议的实例</returns>
        public ProtocalUnit this[Type type]
        {
            get
            {
                var protocalName = type.FullName;
                if (Protocals.ContainsKey(protocalName))
                {
                    return Protocals[protocalName];
                }
                //自动寻找存在的协议并将其加载
                var protocalUnit =
                    Assembly.Load(type.Assembly.FullName).CreateInstance(protocalName) as ProtocalUnit;
                if (protocalUnit == null) throw new InvalidCastException("没有相应的协议内容");
                Register(protocalUnit);
                return Protocals[protocalName];
            }
        }

        /// <summary>
        ///     协议集合
        /// </summary>
        protected Dictionary<string, ProtocalUnit> Protocals { get; }

        /// <summary>
        ///     注册一个协议
        /// </summary>
        /// <param name="linkProtocal">需要注册的协议</param>
        protected void Register(ProtocalUnit linkProtocal)
        {
            if (linkProtocal == null) return;
            Protocals.Add(linkProtocal.GetType().FullName, linkProtocal);
        }

        /// <summary>
        ///     发送协议内容并接收，一般方法
        /// </summary>
        /// <param name="isLittleEndian">是否是小端格式</param>
        /// <param name="content">写入的内容，使用对象数组描述</param>
        /// <returns>从设备获取的字节流</returns>
        public virtual byte[] SendReceive(Endian isLittleEndian, params object[] content)
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync(isLittleEndian, content));
        }

        /// <summary>
        ///     发送协议内容并接收，一般方法
        /// </summary>
        /// <param name="isLittleEndian">是否是小端格式</param>
        /// <param name="content">写入的内容，使用对象数组描述</param>
        /// <returns>从设备获取的字节流</returns>
        public virtual async Task<byte[]> SendReceiveAsync(Endian isLittleEndian, params object[] content)
        {
            if (ProtocalLinker == null || !ProtocalLinker.IsConnected)
            {
                await ConnectAsync();
            }
            if (ProtocalLinker != null)
            {
                return await ProtocalLinker.SendReceiveAsync(ProtocalUnit.TranslateContent(isLittleEndian, content));
            }
            return null;
        }

        /// <summary>
        ///     发送协议，通过传入需要使用的协议内容和输入结构
        /// </summary>
        /// <param name="unit">协议的实例</param>
        /// <param name="content">输入信息的结构化描述</param>
        /// <returns>输出信息的结构化描述</returns>
        public virtual IOutputStruct SendReceive(ProtocalUnit unit, IInputStruct content)
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync(unit, content));
        }

        /// <summary>
        ///     发送协议，通过传入需要使用的协议内容和输入结构
        /// </summary>
        /// <param name="unit">协议的实例</param>
        /// <param name="content">输入信息的结构化描述</param>
        /// <returns>输出信息的结构化描述</returns>
        public virtual async Task<IOutputStruct> SendReceiveAsync(ProtocalUnit unit, IInputStruct content)
        {
            var t = 0;
            var formatContent = unit.Format(content);
            if (formatContent != null)
            {
                byte[] receiveContent;
                //如果为特别处理协议的话，跳过协议扩展收缩
                if (unit is ISpecialProtocalUnit)
                {
                    receiveContent = await ProtocalLinker.SendReceiveWithoutExtAndDecAsync(formatContent);
                }
                else
                {
                    receiveContent = await ProtocalLinker.SendReceiveAsync(formatContent);
                }
                if (receiveContent != null)
                {
                    return unit.Unformat(receiveContent, ref t);
                }
            }
            return null;
        }

        /// <summary>
        ///     协议连接开始
        /// </summary>
        /// <returns></returns>
        public abstract bool Connect();

        /// <summary>
        ///     协议连接开始（异步）
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> ConnectAsync();

        /// <summary>
        ///     协议连接断开
        /// </summary>
        /// <returns></returns>
        public virtual bool Disconnect()
        {
            if (ProtocalLinker != null)
            {
                return ProtocalLinker.Disconnect();
            }
            return false;
        }
    }
}