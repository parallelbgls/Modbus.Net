using System.Configuration;
using System.Threading.Tasks;

namespace Modbus.Net.Siemens
{
    /// <summary>
    ///     西门子Tcp协议
    /// </summary>
    public class SiemensTcpProtocal : SiemensProtocal
    {
        private readonly string _ip;
        private readonly ushort _maxCalled;
        private readonly ushort _maxCalling;
        private readonly ushort _maxPdu;
        private readonly int _port;
        private readonly ushort _taspSrc;
        private readonly byte _tdpuSize;
        private readonly ushort _tsapDst;
        private int _connectTryCount;

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="tdpuSize"></param>
        /// <param name="tsapSrc"></param>
        /// <param name="tsapDst"></param>
        /// <param name="maxCalling"></param>
        /// <param name="maxCalled"></param>
        /// <param name="maxPdu"></param>
        public SiemensTcpProtocal(byte tdpuSize, ushort tsapSrc, ushort tsapDst, ushort maxCalling, ushort maxCalled,
            ushort maxPdu)
            : this(tdpuSize, tsapSrc, tsapDst, maxCalling, maxCalled, maxPdu, ConfigurationManager.AppSettings["IP"])
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="tdpuSize"></param>
        /// <param name="tsapSrc"></param>
        /// <param name="tsapDst"></param>
        /// <param name="maxCalling"></param>
        /// <param name="maxCalled"></param>
        /// <param name="maxPdu"></param>
        /// <param name="ip">IP地址</param>
        public SiemensTcpProtocal(byte tdpuSize, ushort tsapSrc, ushort tsapDst, ushort maxCalling, ushort maxCalled,
            ushort maxPdu, string ip)
            : this(
                tdpuSize, tsapSrc, tsapDst, maxCalling, maxCalled, maxPdu, ip,
                int.Parse(ConfigurationManager.AppSettings["SiemensPort"] ?? "102"))
        {
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="tdpuSize"></param>
        /// <param name="tsapSrc"></param>
        /// <param name="tsapDst"></param>
        /// <param name="maxCalling"></param>
        /// <param name="maxCalled"></param>
        /// <param name="maxPdu"></param>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口</param>
        public SiemensTcpProtocal(byte tdpuSize, ushort tsapSrc, ushort tsapDst, ushort maxCalling, ushort maxCalled,
            ushort maxPdu, string ip, int port) : base(0, 0)
        {
            _taspSrc = tsapSrc;
            _tsapDst = tsapDst;
            _maxCalling = maxCalling;
            _maxCalled = maxCalled;
            _maxPdu = maxPdu;
            _tdpuSize = tdpuSize;
            _ip = ip;
            _port = port;
            _connectTryCount = 0;
        }

        /// <summary>
        ///     发送数据并接收
        /// </summary>
        /// <param name="content">发送的数据</param>
        /// <returns>返回的数据</returns>
        public override PipeUnit SendReceive(params object[] content)
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync(Endian, content));
        }

        /// <summary>
        ///     发送数据并接收
        /// </summary>
        /// <param name="content">发送的数据</param>
        /// <returns>返回的数据</returns>
        public override async Task<PipeUnit> SendReceiveAsync(params object[] content)
        {
            if (ProtocalLinker == null || !ProtocalLinker.IsConnected)
                await ConnectAsync();
            return await base.SendReceiveAsync(Endian, content);
        }

        /// <summary>
        ///     发送数据并接收
        /// </summary>
        /// <param name="unit">协议的核心</param>
        /// <param name="content">协议的参数</param>
        /// <returns>返回的数据</returns>
        public override PipeUnit SendReceive(ProtocalUnit unit, IInputStruct content)
        {
            return AsyncHelper.RunSync(() => SendReceiveAsync(unit, content));
        }

        /// <summary>
        ///     发送数据并接收
        /// </summary>
        /// <param name="unit">发送的数据</param>
        /// <param name="content">协议的参数</param>
        /// <returns>返回的数据</returns>
        public override async Task<PipeUnit> SendReceiveAsync(ProtocalUnit unit, IInputStruct content)
        {
            if (ProtocalLinker != null && ProtocalLinker.IsConnected) return await base.SendReceiveAsync(unit, content);
            if (_connectTryCount > 10) return null;
            return
                await
                    ConnectAsync()
                        .ContinueWith(answer => answer.Result ? base.SendReceiveAsync(unit, content) : null).Unwrap();
        }

        /// <summary>
        ///     强制发送数据并接收
        /// </summary>
        /// <param name="unit">发送的数据</param>
        /// <param name="content">协议的参数</param>
        /// <returns>返回的数据</returns>
        private async Task<PipeUnit> ForceSendReceiveAsync(ProtocalUnit unit, IInputStruct content)
        {
            return await base.SendReceiveAsync(unit, content);
        }

        /// <summary>
        ///     连接设备
        /// </summary>
        /// <returns>设备是否连接成功</returns>
        public override async Task<bool> ConnectAsync()
        {
            _connectTryCount++;
            ProtocalLinker = new SiemensTcpProtocalLinker(_ip, _port);
            if (!await ProtocalLinker.ConnectAsync()) return false;
            _connectTryCount = 0;
            var inputStruct = new CreateReferenceSiemensInputStruct(_tdpuSize, _taspSrc, _tsapDst);
            var outputStruct =
                //先建立连接，然后建立设备的引用
                (await (await
                    ForceSendReceiveAsync(this[typeof(CreateReferenceSiemensProtocal)], inputStruct)).SendReceiveAsync(
                    this[typeof(EstablishAssociationSiemensProtocal)], answer =>
                        new EstablishAssociationSiemensInputStruct(0x0101, _maxCalling,
                            _maxCalled,
                            _maxPdu))).Unwrap<EstablishAssociationSiemensOutputStruct>();
            return outputStruct != null;
        }
    }
}