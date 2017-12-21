using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net.Siemens
{
    /// <summary>
    ///     西门子Ppi协议连接器
    /// </summary>
    public class SiemensPpiProtocalLinker : ComProtocalLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="com">串口地址</param>
        /// <param name="slaveAddress">从站号</param>
        public SiemensPpiProtocalLinker(string com, int slaveAddress)
            : base(com, 9600, Parity.Even, StopBits.One, 8, slaveAddress)
        {
            ((BaseConnector)BaseConnector).AddController(new FIFOController(500));
        }

        /// <summary>
        ///     发送协议内容并接收返回
        /// </summary>
        /// <param name="content">发送的报文</param>
        /// <returns>接收的报文</returns>
        public override async Task<byte[]> SendReceiveAsync(byte[] content)
        {
            var extBytes = BytesExtend(content);
            if (extBytes[6] == 0x7c)
            {
                var inputStruct2 = new ComConfirmMessageSiemensInputStruct(content[4], content[5]);
                var receiveBytes2 =
                    await SendReceiveWithoutExtAndDecAsync(
                        new ComConfirmMessageSiemensProtocal().Format(inputStruct2));
            }
            var receiveBytes = await SendReceiveWithoutExtAndDecAsync(extBytes);
            if (content.Length > 6 && receiveBytes.Length == 1 && receiveBytes[0] == 0xe5)
            {
                var inputStruct2 = new ComConfirmMessageSiemensInputStruct(content[4], content[5]);
                var receiveBytes2 =
                    await SendReceiveWithoutExtAndDecAsync(
                        new ComConfirmMessageSiemensProtocal().Format(inputStruct2));
                return BytesDecact(receiveBytes2);
            }
            return BytesDecact(receiveBytes);
        }

        /// <summary>
        ///     发送协议内容并接收返回，不进行协议扩展和收缩
        /// </summary>
        /// <param name="content">发送的报文</param>
        /// <returns>接收的报文</returns>
        public override async Task<byte[]> SendReceiveWithoutExtAndDecAsync(byte[] content)
        {
            var ans = await base.SendReceiveWithoutExtAndDecAsync(content);
            while (ans.Length == 1 && ans[0] == 0xf9)
            {
                Thread.Sleep(500);
                if (content.Length <= 6)
                {
                    var inputStruct2 = new ComConfirmMessageSiemensInputStruct(content[1], content[2]);
                    ans =
                        await SendReceiveWithoutExtAndDecAsync(
                            new ComConfirmMessageSiemensProtocal().Format(inputStruct2));
                }
                else
                {
                    var inputStruct2 = new ComConfirmMessageSiemensInputStruct(content[4], content[5]);
                    ans =
                        await SendReceiveWithoutExtAndDecAsync(
                            new ComConfirmMessageSiemensProtocal().Format(inputStruct2));
                }
            }
            return ans;
        }

        /// <summary>
        ///     校验报文
        /// </summary>
        /// <param name="content">设备返回的信息</param>
        /// <returns>报文是否正确</returns>
        public override bool? CheckRight(byte[] content)
        {
            if (!base.CheckRight(content).Value) return false;
            var fcsCheck = 0;
            if (content.Length == 1 && content[0] == 0xe5)
                return true;
            if (content.Length == 6 && content[3] == 0) return true;
            for (var i = 4; i < content.Length - 2; i++)
                fcsCheck += content[i];
            fcsCheck = fcsCheck % 256;
            if (fcsCheck != content[content.Length - 2]) return false;
            if (content[content.Length - 1] != 0x16) return false;
            if (content[1] != content.Length - 6) return false;
            return true;
        }
    }
}