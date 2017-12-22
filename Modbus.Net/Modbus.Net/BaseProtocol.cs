using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     基本协议
    /// </summary>
    public abstract class BaseProtocol : BaseProtocol<byte[], byte[], ProtocolUnit, PipeUnit>
    {
        /// <summary>
        ///     构造器
        /// </summary>
        protected BaseProtocol(byte slaveAddress, byte masterAddress, Endian endian)
            : base(slaveAddress, masterAddress, endian)
        {
        }

        /// <summary>
        ///     发送协议内容并接收，一般方法
        /// </summary>
        /// <param name="content">写入的内容，使用对象数组描述</param>
        /// <returns>从设备获取的字节流</returns>
        public override async Task<PipeUnit> SendReceiveAsync(params object[] content)
        {
            if (content != null)
            {
                var pipeUnit =
                    new PipeUnit(
                        ProtocolLinker);
                return await pipeUnit.SendReceiveAsync(Endian, paramOut => content);
            }
            return null;
        }

        /// <summary>
        ///     发送协议，通过传入需要使用的协议内容和输入结构
        /// </summary>
        /// <param name="unit">协议的实例</param>
        /// <param name="content">输入信息的结构化描述</param>
        /// <returns>输出信息的结构化描述</returns>
        public override async Task<PipeUnit>
            SendReceiveAsync(ProtocolUnit unit, IInputStruct content)
        {
            if (content != null)
            {
                var pipeUnit = new PipeUnit(ProtocolLinker);
                return await pipeUnit.SendReceiveAsync(unit, paramOut => content);
            }
            return null;
        }
    }

    /// <summary>
    ///     基本协议
    /// </summary>
    public abstract class BaseProtocol<TParamIn, TParamOut, TProtocolUnit, TPipeUnit> :
        IProtocol<TParamIn, TParamOut, TProtocolUnit, TPipeUnit>
        where TProtocolUnit : class, IProtocolFormatting<TParamIn, TParamOut>
        where TParamOut : class
        where TPipeUnit : PipeUnit<TParamIn, TParamOut, IProtocolLinker<TParamIn, TParamOut>, TProtocolUnit>
    {
        /// <summary>
        ///     构造器
        /// </summary>
        protected BaseProtocol(byte slaveAddress, byte masterAddress, Endian endian)
        {
            Endian = endian;
            Protocols = new Dictionary<string, TProtocolUnit>();
            SlaveAddress = slaveAddress;
            MasterAddress = masterAddress;
        }

        /// <summary>
        ///     协议的端格式
        /// </summary>
        protected Endian Endian { get; set; }

        /// <summary>
        ///     从站地址
        /// </summary>
        public byte SlaveAddress { get; set; }

        /// <summary>
        ///     主站地址
        /// </summary>
        public byte MasterAddress { get; set; }

        /// <summary>
        ///     协议集合
        /// </summary>
        protected Dictionary<string, TProtocolUnit> Protocols { get; }

        /// <summary>
        ///     协议索引器，这是一个懒加载协议，当字典中不存在协议时自动加载协议，否则调用已经加载的协议
        /// </summary>
        /// <param name="type">协议的类的GetType</param>
        /// <returns>协议的实例</returns>
        public TProtocolUnit this[Type type]
        {
            get
            {
                var protocalName = type.FullName;
                TProtocolUnit protocalUnitReturn = null;
                lock (Protocols)
                {
                    if (Protocols.ContainsKey(protocalName))
                    {
                        protocalUnitReturn = Protocols[protocalName];
                    }
                    else
                    {
                        //自动寻找存在的协议并将其加载
                        var protocalUnit =
                            Activator.CreateInstance(type.GetTypeInfo().Assembly
                                .GetType(protocalName)) as TProtocolUnit;
                        if (protocalUnit == null)
                            throw new InvalidCastException($"No ProtocolUnit {nameof(TProtocolUnit)} implemented");
                        protocalUnit.Endian = Endian;
                        Register(protocalUnit);
                    }
                }
                return protocalUnitReturn ?? Protocols[protocalName];
            }
        }

        /// <summary>
        ///     协议的连接器
        /// </summary>
        public IProtocolLinker<TParamIn, TParamOut> ProtocolLinker { get; protected set; }

        /// <summary>
        ///     协议连接开始
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> ConnectAsync();

        /// <summary>
        ///     协议连接断开
        /// </summary>
        /// <returns></returns>
        public virtual bool Disconnect()
        {
            if (ProtocolLinker != null)
                return ProtocolLinker.Disconnect();
            return false;
        }

        /// <summary>
        ///     发送协议，通过传入需要使用的协议内容和输入结构
        /// </summary>
        /// <param name="unit">协议的实例</param>
        /// <param name="content">输入信息的结构化描述</param>
        /// <returns>输出信息的结构化描述</returns>
        public virtual TPipeUnit SendReceive(
            TProtocolUnit unit, IInputStruct content)
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync(unit, content));
        }

        /// <summary>
        ///     发送协议，通过传入需要使用的协议内容和输入结构
        /// </summary>
        /// <param name="unit">协议的实例</param>
        /// <param name="content">输入信息的结构化描述</param>
        /// <returns>输出信息的结构化描述</returns>
        public virtual async Task<TPipeUnit>
            SendReceiveAsync(TProtocolUnit unit, IInputStruct content)
        {
            if (content != null)
            {
                var pipeUnit =
                    new PipeUnit<TParamIn, TParamOut, IProtocolLinker<TParamIn, TParamOut>, TProtocolUnit>(
                        ProtocolLinker);
                return await pipeUnit.SendReceiveAsync(unit, paramOut => content) as TPipeUnit;
            }
            return null;
        }

        /// <summary>
        ///     发送协议内容并接收，一般方法
        /// </summary>
        /// <param name="content">写入的内容，使用对象数组描述</param>
        /// <returns>从设备获取的字节流</returns>
        public virtual TPipeUnit SendReceive(params object[] content)
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync(content));
        }

        /// <summary>
        ///     发送协议内容并接收，一般方法（不能使用，如需使用请继承）
        /// </summary>
        /// <param name="content">写入的内容，使用对象数组描述</param>
        /// <returns>从设备获取的字节流</returns>
        public virtual Task<TPipeUnit> SendReceiveAsync(params object[] content)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     发送协议，通过传入需要使用的协议内容和输入结构
        /// </summary>
        /// <param name="unit">协议的实例</param>
        /// <param name="content">输入信息的结构化描述</param>
        /// <returns>输出信息的结构化描述</returns>
        /// <typeparam name="T">IOutputStruct的具体类型</typeparam>
        public virtual T SendReceive<T>(TProtocolUnit unit, IInputStruct content) where T : class, IOutputStruct
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync<T>(unit, content));
        }

        /// <summary>
        ///     发送协议，通过传入需要使用的协议内容和输入结构
        /// </summary>
        /// <param name="unit">协议的实例</param>
        /// <param name="content">输入信息的结构化描述</param>
        /// <returns>输出信息的结构化描述</returns>
        /// <typeparam name="T">IOutputStruct的具体类型</typeparam>
        public virtual async Task<T> SendReceiveAsync<T>(TProtocolUnit unit, IInputStruct content)
            where T : class, IOutputStruct
        {
            return (await SendReceiveAsync(unit, content)).Unwrap<T>();
        }

        /// <summary>
        ///     注册一个协议
        /// </summary>
        /// <param name="linkProtocol">需要注册的协议</param>
        protected void Register(TProtocolUnit linkProtocol)
        {
            if (linkProtocol == null) return;
            Protocols.Add(linkProtocol.GetType().FullName, linkProtocol);
        }
    }
}