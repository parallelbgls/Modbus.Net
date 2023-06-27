using FastEnumUtility;
using System.IO.Ports;

namespace Modbus.Net
{
    /// <summary>
    ///     串口连接对象
    /// </summary>
    public abstract class ComProtocolLinker : ProtocolLinker
    {
        /// <summary>
        ///     构造器
        /// </summary>
        /// <param name="com">串口端口号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">校验位</param>
        /// <param name="stopBits">停止位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="handshake">流控制</param>
        /// <param name="connectionTimeout">超时时间</param>
        /// <param name="slaveAddress">从站地址</param>
        /// <param name="isFullDuplex">是否为全双工</param>
        protected ComProtocolLinker(string com, int slaveAddress, BaudRate? baudRate = null, Parity? parity = null, StopBits? stopBits = null, DataBits? dataBits = null, Handshake? handshake = null,
            int? connectionTimeout = null, bool? isFullDuplex = null)
        {
            baudRate = FastEnum.Parse<BaudRate>(baudRate != null ? baudRate.ToString() : null ?? ConfigurationReader.GetValue("COM:" + com, "BaudRate"));
            parity = FastEnum.Parse<Parity>(parity != null ? parity.ToString() : null ?? ConfigurationReader.GetValue("COM:" + com, "Parity"));
            stopBits = FastEnum.Parse<StopBits>(stopBits != null ? stopBits.ToString() : null ?? ConfigurationReader.GetValue("COM:" + com, "StopBits"));
            dataBits = FastEnum.Parse<DataBits>(dataBits != null ? dataBits.ToString() : null ?? ConfigurationReader.GetValue("COM:" + com, "DataBits"));
            handshake = FastEnum.Parse<Handshake>(handshake != null ? handshake.ToString() : null ?? ConfigurationReader.GetValue("COM:" + com, "Handshake"));
            connectionTimeout = int.Parse(connectionTimeout != null ? connectionTimeout.ToString() : null ?? ConfigurationReader.GetValue("COM:" + com, "ConnectionTimeout"));
            isFullDuplex = bool.Parse(isFullDuplex != null ? isFullDuplex.ToString() : null ?? ConfigurationReader.GetValue("COM:" + com, "FullDuplex"));
            BaseConnector = new ComConnector(com + ":" + slaveAddress, baudRate.Value, parity.Value, stopBits.Value, dataBits.Value, handshake.Value, connectionTimeout.Value, isFullDuplex.Value);
        }
    }
}