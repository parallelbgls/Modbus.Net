using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Modbus.Net
{
    public class PipeUnit : PipeUnit<byte[], byte[], IProtocalLinker<byte[], byte[]>, ProtocalUnit>
    {
        public PipeUnit(IProtocalLinker<byte[], byte[]> protocalLinker) : base(protocalLinker)
        {

        }

        protected PipeUnit(IProtocalLinker<byte[], byte[]> protocalLinker, ProtocalUnit protocalUnit, byte[] parameters,
            bool success) : base(protocalLinker, protocalUnit, parameters, success)
        {
        }

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

        public async Task<PipeUnit> SendReceiveAsync(
                ProtocalUnit unit,
                Func<byte[], IInputStruct> inputStructCreator)
        {
            var receiveContent = await SendReceiveAsync1(unit, inputStructCreator);
            if (receiveContent != null)
                return new PipeUnit(ProtocalLinker, unit,
                    receiveContent, true);
            return new PipeUnit(ProtocalLinker, unit, ReturnParams,
                false);
        }

        public byte[] Unwrap()
        {
            return ReturnParams;
        }
    }

    public class PipeUnit<TParamIn, TParamOut, TProtocalLinker, TProtocalUnit>
        where TProtocalUnit : class, IProtocalFormatting<TParamIn, TParamOut>
        where TProtocalLinker : class, IProtocalLinker<TParamIn, TParamOut>
        where TParamOut : class
    {
        public PipeUnit(TProtocalLinker protocalLinker) : this(protocalLinker, null, null, true)
        {
            
        }

        protected PipeUnit(TProtocalLinker protocalLinker, TProtocalUnit protocalUnit, TParamOut parameters, bool success)
        {
            ProtocalLinker = protocalLinker;
            ProtocalUnit = protocalUnit;
            ReturnParams = parameters;
            Success = success;
        }

        protected TProtocalLinker ProtocalLinker { get; set; }

        protected TProtocalUnit ProtocalUnit { get; set; }

        protected TParamOut ReturnParams { get; set; }

        public bool Success { get; }

        protected async Task<TParamOut> SendReceiveAsync1(TProtocalUnit unit,
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

        public virtual async Task<PipeUnit<TParamIn, TParamOut, TProtocalLinker, TProtocalUnit>> SendReceiveAsync(
            TProtocalUnit unit,
            Func<TParamOut, IInputStruct> inputStructCreator)
        {
            var receiveContent = await SendReceiveAsync1(unit, inputStructCreator);
            if (receiveContent != null)
                return new PipeUnit<TParamIn, TParamOut, TProtocalLinker, TProtocalUnit>(ProtocalLinker, unit,
                    receiveContent, true);
            return new PipeUnit<TParamIn, TParamOut, TProtocalLinker, TProtocalUnit>(ProtocalLinker, unit, ReturnParams,
                false);
        }

        public T Unwrap<T>() where T : class, IOutputStruct
        {
            var t = 0;
            return ProtocalUnit.Unformat<T>(ReturnParams, ref t);
        }
    }
}