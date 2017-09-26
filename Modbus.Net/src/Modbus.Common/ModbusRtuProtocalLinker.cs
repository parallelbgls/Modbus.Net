using System.IO.Ports;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Rtu协议连接器
    /// </summary>
    public class ModbusRtuProtocalLinker : ComProtocalLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="com">串口地址</param>
        /// <param name="slaveAddress">从站号</param>
        public ModbusRtuProtocalLinker(string com, int slaveAddress)
            : base(com, 9600, Parity.None, StopBits.One, 8, slaveAddress)
        {
        }

        /// <summary>
        ///     校验返回数据
        /// </summary>
        /// <param name="content">设备返回的数据</param>
        /// <returns>数据是否正确</returns>
        public override bool? CheckRight(byte[] content)
        {
            //ProtocalLinker的CheckRight不会返回null
            if (!base.CheckRight(content).Value) return false;
            //CRC校验失败
            if (!Crc16.GetInstance().CrcEfficacy(content))
                throw new ModbusProtocalErrorException(501);
            //Modbus协议错误
            if (content[1] > 127)
                throw new ModbusProtocalErrorException(content[2]);
            return true;
        }
    }
}