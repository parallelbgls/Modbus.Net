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
    public abstract class BaseProtocal : BaseProtocal<byte[], byte[], ProtocalUnit, PipeUnit>
    {
        /// <summary>
        ///     构造器
        /// </summary>
        protected BaseProtocal(byte slaveAddress, byte masterAddress, Endian endian)
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
                        ProtocalLinker);
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
            SendReceiveAsync(ProtocalUnit unit, IInputStruct content)
        {
            if (content != null)
            {
                var pipeUnit = new PipeUnit(ProtocalLinker);
                return await pipeUnit.SendReceiveAsync(unit, paramOut => content);
            }
            return null;
        }
    }

    /// <summary>
    ///     基本协议
    /// </summary>
    public abstract class BaseProtocal<TParamIn, TParamOut, TProtocalUnit, TPipeUnit> :
        IProtocal<TParamIn, TParamOut, TProtocalUnit, TPipeUnit>
        where TProtocalUnit : class, IProtocalFormatting<TParamIn, TParamOut>
        where TParamOut : class
        where TPipeUnit : PipeUnit<TParamIn, TParamOut, IProtocalLinker<TParamIn, TParamOut>, TProtocalUnit>
    {
        /// <summary>
        ///     构造器
        /// </summary>
        protected BaseProtocal(byte slaveAddress, byte masterAddress, Endian endian)
        {
            Endian = endian;
            Protocals = new Dictionary<string, TProtocalUnit>();
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
        protected Dictionary<string, TProtocalUnit> Protocals { get; }

        /// <summary>
        ///     协议索引器，这是一个懒加载协议，当字典中不存在协议时自动加载协议，否则调用已经加载的协议
        /// </summary>
        /// <param name="type">协议的类的GetType</param>
        /// <returns>协议的实例</returns>
        public TProtocalUnit this[Type type]
        {
            get
            {
                var protocalName = type.FullName;
                TProtocalUnit protocalUnitReturn = null;
                lock (Protocals)
                {
                    if (Protocals.ContainsKey(protocalName))
                    {
                        protocalUnitReturn = Protocals[protocalName];
                    }
                    else
                    {
                        //自动寻找存在的协议并将其加载
                        var protocalUnit =
                            Activator.CreateInstance(type.GetTypeInfo().Assembly
                                .GetType(protocalName)) as TProtocalUnit;
                        if (protocalUnit == null)
                            throw new InvalidCastException($"No ProtocalUnit {nameof(TProtocalUnit)} implemented");
                        protocalUnit.Endian = Endian;
                        Register(protocalUnit);
                    }
                }
                return protocalUnitReturn ?? Protocals[protocalName];
            }
        }

        /// <summary>
        ///     协议的连接器
        /// </summary>
        public IProtocalLinker<TParamIn, TParamOut> ProtocalLinker { get; protected set; }

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
            if (ProtocalLinker != null)
                return ProtocalLinker.Disconnect();
            return false;
        }

        /// <summary>
        ///     发送协议，通过传入需要使用的协议内容和输入结构
        /// </summary>
        /// <param name="unit">协议的实例</param>
        /// <param name="content">输入信息的结构化描述</param>
        /// <returns>输出信息的结构化描述</returns>
        public virtual TPipeUnit SendReceive(
            TProtocalUnit unit, IInputStruct content)
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
            SendReceiveAsync(TProtocalUnit unit, IInputStruct content)
        {
            if (content != null)
            {
                var pipeUnit =
                    new PipeUnit<TParamIn, TParamOut, IProtocalLinker<TParamIn, TParamOut>, TProtocalUnit>(
                        ProtocalLinker);
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
        public virtual T SendReceive<T>(TProtocalUnit unit, IInputStruct content) where T : class, IOutputStruct
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
        public virtual async Task<T> SendReceiveAsync<T>(TProtocalUnit unit, IInputStruct content)
            where T : class, IOutputStruct
        {
            return (await SendReceiveAsync(unit, content)).Unwrap<T>();
        }

        /// <summary>
        ///     注册一个协议
        /// </summary>
        /// <param name="linkProtocal">需要注册的协议</param>
        protected void Register(TProtocalUnit linkProtocal)
        {
            if (linkProtocal == null) return;
            Protocals.Add(linkProtocal.GetType().FullName, linkProtocal);
        }
    }
}