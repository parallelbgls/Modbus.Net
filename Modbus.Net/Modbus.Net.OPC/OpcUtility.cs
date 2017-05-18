using System;
using System.Threading.Tasks;
using Serilog;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc通用Api入口
    /// </summary>
    public abstract class OpcUtility : BaseUtility<OpcParamIn, OpcParamOut, ProtocalUnit<OpcParamIn, OpcParamOut>>
    {
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
        public override async Task<byte[]> GetDatasAsync(string startAddress, int getByteCount)
        {
            try
            {
                var split = GetSeperator?.Invoke() ?? '/';
                var readRequestOpcInputStruct = new ReadRequestOpcInputStruct(startAddress, split);
                var readRequestOpcOutputStruct =
                    await
                        Wrapper.SendReceiveAsync<ReadRequestOpcOutputStruct>(Wrapper[typeof(ReadRequestOpcProtocal)],
                            readRequestOpcInputStruct);
                return readRequestOpcOutputStruct?.GetValue;
            }
            catch (Exception e)
            {
                Log.Error(e, $"OpcUtility -> GetDatas: {ConnectionString} error");
                return null;
            }
        }

        /// <summary>
        ///     设置数据
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="setContents">设置数据</param>
        /// <returns>是否设置成功</returns>
        public override async Task<bool> SetDatasAsync(string startAddress, object[] setContents)
        {
            try
            {
                var split = GetSeperator?.Invoke() ?? '/';
                var writeRequestOpcInputStruct = new WriteRequestOpcInputStruct(startAddress, split, setContents[0]);
                var writeRequestOpcOutputStruct =
                    await
                        Wrapper.SendReceiveAsync<WriteRequestOpcOutputStruct>(Wrapper[typeof(WriteRequestOpcProtocal)],
                            writeRequestOpcInputStruct);
                return writeRequestOpcOutputStruct?.WriteResult == true;
            }
            catch (Exception e)
            {
                Log.Error(e, $"OpcUtility -> SetDatas: {ConnectionString} error");
                return false;
            }
        }
    }
}