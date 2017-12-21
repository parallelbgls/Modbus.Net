using System.Configuration;
using System.Threading.Tasks;

namespace Modbus.Net.Siemens
{
    /// <summary>
    ///     西门子Ppi协议
    /// </summary>
    public class SiemensPpiProtocal : SiemensProtocal
    {
        private readonly string _com;

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        public SiemensPpiProtocal(byte slaveAddress, byte masterAddress)
            : this(ConfigurationManager.AppSettings["COM"], slaveAddress, masterAddress)
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="com">串口地址</param>
        /// <param name="slaveAddress">从站号</param>
        /// <param name="masterAddress">主站号</param>
        public SiemensPpiProtocal(string com, byte slaveAddress, byte masterAddress)
            : base(slaveAddress, masterAddress)
        {
            _com = com;
        }

        /// <summary>
        ///     发送协议内容并接收，一般方法
        /// </summary>
        /// <param name="content">写入的内容，使用对象数组描述</param>
        /// <returns>从设备获取的字节流</returns>
        public override PipeUnit SendReceive(params object[] content)
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync(Endian, content));
        }

        /// <summary>
        ///     发送协议内容并接收，一般方法
        /// </summary>
        /// <param name="content">写入的内容，使用对象数组描述</param>
        /// <returns>从设备获取的字节流</returns>
        public override async Task<PipeUnit> SendReceiveAsync(params object[] content)
        {
            if (ProtocalLinker == null || !ProtocalLinker.IsConnected)
                await ConnectAsync();
            return await base.SendReceiveAsync(Endian, content);
        }

        /// <summary>
        ///     强行发送，不检测连接状态
        /// </summary>
        /// <param name="unit">协议核心</param>
        /// <param name="content">协议的参数</param>
        /// <returns>设备返回的信息</returns>
        private async Task<PipeUnit> ForceSendReceiveAsync(ProtocalUnit unit, IInputStruct content)
        {
            return await base.SendReceiveAsync(unit, content);
        }

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>是否连接成功</returns>
        public override async Task<bool> ConnectAsync()
        {
            ProtocalLinker = new SiemensPpiProtocalLinker(_com, SlaveAddress);
            var inputStruct = new ComCreateReferenceSiemensInputStruct(SlaveAddress, MasterAddress);
            var outputStruct =
                (await (await
                    ForceSendReceiveAsync(this[typeof(ComCreateReferenceSiemensProtocal)],
                            inputStruct)).
                        SendReceiveAsync(this[typeof(ComConfirmMessageSiemensProtocal)], answer =>
                        
                            new ComConfirmMessageSiemensInputStruct(SlaveAddress, MasterAddress)
                        )).Unwrap<ComConfirmMessageSiemensOutputStruct>();
            return outputStruct != null;
        }
    }
}