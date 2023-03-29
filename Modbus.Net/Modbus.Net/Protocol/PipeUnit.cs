using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     管道单元
    /// </summary>
    public class PipeUnit : PipeUnit<byte[], byte[], IProtocolLinker<byte[], byte[]>, ProtocolUnit<byte[], byte[]>>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="protocolLinker">连接器</param>
        public PipeUnit(IProtocolLinker<byte[], byte[]> protocolLinker) : base(protocolLinker)
        {

        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="protocolLinker">连接器</param>
        /// <param name="protocolUnit">协议单元</param>
        /// <param name="parameters">传递给输入结构的参数</param>
        /// <param name="success">上次的管道是否成功执行</param>
        protected PipeUnit(IProtocolLinker<byte[], byte[]> protocolLinker, ProtocolUnit<byte[], byte[]> protocolUnit, byte[] parameters,
            bool success) : base(protocolLinker, protocolUnit, parameters, success)
        {
        }

        /// <summary>
        ///     再次发送数据
        /// </summary>
        /// <param name="endian">端格式</param>
        /// <param name="inputStructCreator">构造输入结构的函数</param>
        /// <returns>发送完成之后新的管道实例</returns>
        public async Task<PipeUnit> SendReceiveAsync(Endian endian, Func<byte[], object[]> inputStructCreator)
        {
            if (Success)
            {
                var content = inputStructCreator.Invoke(ReturnParams);
                if (ProtocolLinker != null)
                    return new PipeUnit(ProtocolLinker, null,
                        await ProtocolLinker.SendReceiveAsync(ProtocolUnit<byte[], byte[]>.TranslateContent(endian, content)),
                        true);
            }
            return new PipeUnit(ProtocolLinker, null, ReturnParams, false);
        }

        /// <summary>
        ///     再次发送数据
        /// </summary>
        /// <param name="unit">协议单元</param>
        /// <param name="inputStructCreator">构造输入结构的函数</param>
        /// <returns>发送完成之后新的管道实例</returns>
        public new async Task<PipeUnit> SendReceiveAsync(
                ProtocolUnit<byte[], byte[]> unit,
                Func<byte[], IInputStruct> inputStructCreator)
        {
            var receiveContent = await SendReceiveAsyncParamOut(unit, inputStructCreator);
            if (receiveContent != null)
                return new PipeUnit(ProtocolLinker, unit,
                    receiveContent, true);
            return new PipeUnit(ProtocolLinker, unit, ReturnParams,
                false);
        }

        /// <summary>
        ///     管道完成，返回最终结果
        /// </summary>
        /// <returns>最后的字节数组结构</returns>
        public byte[] Unwrap()
        {
            return ReturnParams;
        }
    }

    /// <summary>
    ///     管道单元
    /// </summary>
    /// <typeparam name="TParamIn">输入参数</typeparam>
    /// <typeparam name="TParamOut">输出参数</typeparam>
    /// <typeparam name="TProtocolLinker">连接器</typeparam>
    /// <typeparam name="TProtocolUnit">协议单元</typeparam>
    public class PipeUnit<TParamIn, TParamOut, TProtocolLinker, TProtocolUnit>
        where TProtocolUnit : class, IProtocolFormatting<TParamIn, TParamOut>
        where TProtocolLinker : class, IProtocolLinker<TParamIn, TParamOut>
        where TParamOut : class
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="protocolLinker">连接器</param>
        public PipeUnit(TProtocolLinker protocolLinker) : this(protocolLinker, null, null, true)
        {

        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="protocolLinker">连接器</param>
        /// <param name="protocolUnit">协议单元</param>
        /// <param name="parameters">输入参数</param>
        /// <param name="success">上一次管道结果是否成功</param>
        protected PipeUnit(TProtocolLinker protocolLinker, TProtocolUnit protocolUnit, TParamOut parameters, bool success)
        {
            ProtocolLinker = protocolLinker;
            ProtocolUnit = protocolUnit;
            ReturnParams = parameters;
            Success = success;
        }

        /// <summary>
        ///     协议连接器
        /// </summary>
        protected TProtocolLinker ProtocolLinker { get; set; }

        /// <summary>
        ///     协议单元
        /// </summary>
        protected TProtocolUnit ProtocolUnit { get; set; }

        /// <summary>
        ///     输入结构传入的参数
        /// </summary>
        protected TParamOut ReturnParams { get; set; }

        /// <summary>
        ///     本次管道是否成功
        /// </summary>
        public bool Success { get; }

        /// <summary>
        ///     向设备发送数据，返回输出参数
        /// </summary>
        /// <param name="unit">协议单元</param>
        /// <param name="inputStructCreator">输入参数生成函数</param>
        /// <returns>输出参数</returns>
        protected async Task<TParamOut> SendReceiveAsyncParamOut(TProtocolUnit unit,
            Func<TParamOut, IInputStruct> inputStructCreator)
        {
            if (Success)
            {
                var content = inputStructCreator.Invoke(ReturnParams);
                var formatContent = unit.Format(content);
                if (formatContent != null)
                {
                    TParamOut receiveContent;
                    //如果为特别处理协议的话，跳过协议扩展收缩
                    if (unit.GetType().GetTypeInfo().GetCustomAttributes(typeof(SpecialProtocolUnitAttribute)).Any())
                        receiveContent = await ProtocolLinker.SendReceiveWithoutExtAndDecAsync(formatContent);
                    else
                        receiveContent = await ProtocolLinker.SendReceiveAsync(formatContent);
                    return receiveContent;
                }
            }
            return null;
        }

        /// <summary>
        ///     向设备发送数据，返回管道
        /// </summary>
        /// <param name="unit">协议单元</param>
        /// <param name="inputStructCreator">输入参数生成函数</param>
        /// <returns>管道实体</returns>
        public virtual async Task<PipeUnit<TParamIn, TParamOut, TProtocolLinker, TProtocolUnit>> SendReceiveAsync(
            TProtocolUnit unit,
            Func<TParamOut, IInputStruct> inputStructCreator)
        {
            var receiveContent = await SendReceiveAsyncParamOut(unit, inputStructCreator);
            if (receiveContent != null)
                return new PipeUnit<TParamIn, TParamOut, TProtocolLinker, TProtocolUnit>(ProtocolLinker, unit,
                    receiveContent, true);
            return new PipeUnit<TParamIn, TParamOut, TProtocolLinker, TProtocolUnit>(ProtocolLinker, unit, ReturnParams,
                false);
        }

        /// <summary>
        ///     所有管道执行结束，输出结果
        /// </summary>
        /// <typeparam name="T">输出的类型</typeparam>
        /// <returns>输出结果</returns>
        public T Unwrap<T>() where T : class, IOutputStruct
        {
            var t = 0;
            return ProtocolUnit.Unformat<T>(ReturnParams, ref t);
        }
    }
}