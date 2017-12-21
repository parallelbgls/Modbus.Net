using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     管道单元
    /// </summary>
    public class PipeUnit : PipeUnit<byte[], byte[], IProtocalLinker<byte[], byte[]>, ProtocalUnit>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="protocalLinker">连接器</param>
        public PipeUnit(IProtocalLinker<byte[], byte[]> protocalLinker) : base(protocalLinker)
        {

        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="protocalLinker">连接器</param>
        /// <param name="protocalUnit">协议单元</param>
        /// <param name="parameters">传递给输入结构的参数</param>
        /// <param name="success">上次的管道是否成功执行</param>
        protected PipeUnit(IProtocalLinker<byte[], byte[]> protocalLinker, ProtocalUnit protocalUnit, byte[] parameters,
            bool success) : base(protocalLinker, protocalUnit, parameters, success)
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
                if (ProtocalLinker != null)
                    return new PipeUnit(ProtocalLinker, null,
                        await ProtocalLinker.SendReceiveAsync(ProtocalUnit.TranslateContent(endian, content)),
                        true);
            }
            return new PipeUnit(ProtocalLinker, null, ReturnParams, false);
        }

        /// <summary>
        ///     再次发送数据
        /// </summary>
        /// <param name="unit">协议单元</param>
        /// <param name="inputStructCreator">构造输入结构的函数</param>
        /// <returns>发送完成之后新的管道实例</returns>
        public new async Task<PipeUnit> SendReceiveAsync(
                ProtocalUnit unit,
                Func<byte[], IInputStruct> inputStructCreator)
        {
            var receiveContent = await SendReceiveAsyncParamOut(unit, inputStructCreator);
            if (receiveContent != null)
                return new PipeUnit(ProtocalLinker, unit,
                    receiveContent, true);
            return new PipeUnit(ProtocalLinker, unit, ReturnParams,
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
    /// <typeparam name="TProtocalLinker">连接器</typeparam>
    /// <typeparam name="TProtocalUnit">协议单元</typeparam>
    public class PipeUnit<TParamIn, TParamOut, TProtocalLinker, TProtocalUnit>
        where TProtocalUnit : class, IProtocalFormatting<TParamIn, TParamOut>
        where TProtocalLinker : class, IProtocalLinker<TParamIn, TParamOut>
        where TParamOut : class
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="protocalLinker">连接器</param>
        public PipeUnit(TProtocalLinker protocalLinker) : this(protocalLinker, null, null, true)
        {
            
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="protocalLinker">连接器</param>
        /// <param name="protocalUnit">协议单元</param>
        /// <param name="parameters">输入参数</param>
        /// <param name="success">上一次管道结果是否成功</param>
        protected PipeUnit(TProtocalLinker protocalLinker, TProtocalUnit protocalUnit, TParamOut parameters, bool success)
        {
            ProtocalLinker = protocalLinker;
            ProtocalUnit = protocalUnit;
            ReturnParams = parameters;
            Success = success;
        }

        /// <summary>
        ///     协议连接器
        /// </summary>
        protected TProtocalLinker ProtocalLinker { get; set; }

        /// <summary>
        ///     协议单元
        /// </summary>
        protected TProtocalUnit ProtocalUnit { get; set; }

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
        protected async Task<TParamOut> SendReceiveAsyncParamOut(TProtocalUnit unit,
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
                    if (unit.GetType().GetTypeInfo().GetCustomAttributes(typeof(SpecialProtocalUnitAttribute)).Any())
                        receiveContent = await ProtocalLinker.SendReceiveWithoutExtAndDecAsync(formatContent);
                    else
                        receiveContent = await ProtocalLinker.SendReceiveAsync(formatContent);
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
        public virtual async Task<PipeUnit<TParamIn, TParamOut, TProtocalLinker, TProtocalUnit>> SendReceiveAsync(
            TProtocalUnit unit,
            Func<TParamOut, IInputStruct> inputStructCreator)
        {
            var receiveContent = await SendReceiveAsyncParamOut(unit, inputStructCreator);
            if (receiveContent != null)
                return new PipeUnit<TParamIn, TParamOut, TProtocalLinker, TProtocalUnit>(ProtocalLinker, unit,
                    receiveContent, true);
            return new PipeUnit<TParamIn, TParamOut, TProtocalLinker, TProtocalUnit>(ProtocalLinker, unit, ReturnParams,
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
            return ProtocalUnit.Unformat<T>(ReturnParams, ref t);
        }
    }
}