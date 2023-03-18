using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc通用Api入口
    /// </summary>
    public abstract class OpcUtility : BaseUtility<OpcParamIn, OpcParamOut, ProtocolUnit<OpcParamIn, OpcParamOut>,
        PipeUnit<OpcParamIn, OpcParamOut, IProtocolLinker<OpcParamIn, OpcParamOut>,
            ProtocolUnit<OpcParamIn, OpcParamOut>>>
    {
        private static readonly ILogger<OpcUtility> logger = LogProvider.CreateLogger<OpcUtility>();

        /// <summary>
        ///     获取分隔符
        /// </summary>
        /// <returns>分隔符</returns>
        public delegate char GetSeperatorDelegate();

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="connectionString">连接地址</param>
        protected OpcUtility(string connectionString) : base(0, 0)
        {
            ConnectionString = connectionString;
            AddressTranslator = new AddressTranslatorOpc();
        }

        /// <summary>
        ///     端格式（大端）
        /// </summary>
        public override Endian Endian => Endian.BigEndianLsb;

        /// <summary>
        ///     获取分隔符
        /// </summary>
        public event GetSeperatorDelegate GetSeperator;

        /// <summary>
        ///     设置连接方式(Opc忽略该函数)
        /// </summary>
        /// <param name="connectionType">连接方式</param>
        public override void SetConnectionType(int connectionType)
        {
            //ignore
        }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="getByteCount">获取字节数个数</param>
        /// <returns>接收到的byte数据</returns>
        public override async Task<ReturnStruct<byte[]>> GetDatasAsync(string startAddress, int getByteCount)
        {
            try
            {
                var split = GetSeperator?.Invoke() ?? '/';
                var readRequestOpcInputStruct = new ReadRequestOpcInputStruct(startAddress.Split('\r'), split);
                var readRequestOpcOutputStruct =
                    await
                        Wrapper.SendReceiveAsync<ReadRequestOpcOutputStruct>(Wrapper[typeof(ReadRequestOpcProtocol)],
                            readRequestOpcInputStruct);
                return new ReturnStruct<byte[]>
                {
                    Datas = readRequestOpcOutputStruct?.GetValue,
                    IsSuccess = true,
                    ErrorCode = 0,
                    ErrorMsg = ""
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, $"OpcUtility -> GetDatas: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<byte[]>
                {
                    Datas = null,
                    IsSuccess = true,
                    ErrorCode = -100,
                    ErrorMsg = e.Message
                };
            }
        }

        /// <summary>
        ///     设置数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="setContents">设置数据</param>
        /// <returns>是否设置成功</returns>
        public override async Task<ReturnStruct<bool>> SetDatasAsync(string startAddress, object[] setContents)
        {
            try
            {
                var split = GetSeperator?.Invoke() ?? '/';
                var writeRequestOpcInputStruct =
                    new WriteRequestOpcInputStruct(startAddress.Split('\r'), split, setContents[0]);
                var writeRequestOpcOutputStruct =
                    await
                        Wrapper.SendReceiveAsync<WriteRequestOpcOutputStruct>(Wrapper[typeof(WriteRequestOpcProtocol)],
                            writeRequestOpcInputStruct);
                return new ReturnStruct<bool>
                {
                    Datas = writeRequestOpcOutputStruct?.WriteResult == true,
                    IsSuccess = writeRequestOpcOutputStruct?.WriteResult == true,
                    ErrorCode = writeRequestOpcOutputStruct?.WriteResult == true ? 0 : 1,
                    ErrorMsg = writeRequestOpcOutputStruct?.WriteResult == true ? "" : "Write Failed"
                };
            }
            catch (Exception e)
            {
                logger.LogError(e, $"OpcUtility -> SetDatas: {ConnectionString} error: {e.Message}");
                return new ReturnStruct<bool>
                {
                    Datas = false,
                    IsSuccess = false,
                    ErrorCode = -100,
                    ErrorMsg = e.Message
                };
            }
        }
    }
}