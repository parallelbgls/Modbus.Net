using System.Collections.Generic;
using System.IO.Ports;

namespace Modbus.Net.Modbus
{
    /// <summary>
    ///     Modbus/Rtu协议连接器
    /// </summary>
    public class ModbusRtuProtocolLinker : ComProtocolLinker
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="com">串口地址</param>
        /// <param name="slaveAddress">从站号</param>
        public ModbusRtuProtocolLinker(string com, int slaveAddress)
            : base(com, 9600, Parity.None, StopBits.One, 8, slaveAddress)
        {
            ((BaseConnector)BaseConnector).AddController(new MatchController(new ICollection<(int,int)>[]{new List<(int,int)>{(0,0)}, new List<(int, int)>{(1,1)}}, 0));
        }

        /// <summary>
        ///     校验返回数据
        /// </summary>
        /// <param name="content">设备返回的数据</param>
        /// <returns>数据是否正确</returns>
        public override bool? CheckRight(byte[] content)
        {
            //ProtocolLinker的CheckRight不会返回null
            if (base.CheckRight(content) != true) return false;
            //CRC校验失败
            if (!Crc16.GetInstance().CrcEfficacy(content))
                throw new ModbusProtocolErrorException(501);
            //Modbus协议错误
            if (content[1] > 127)
                throw new ModbusProtocolErrorException(content[2]);
            return true;
        }
    }
}