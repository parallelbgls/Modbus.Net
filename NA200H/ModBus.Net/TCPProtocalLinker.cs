using System;
using System.Collections.Generic;
using System.Linq;

namespace ModBus.Net
{
    public abstract class TcpProtocalLinker : ProtocalLinker
    {
        /// <summary>
        /// 连接对象
        /// </summary>
        private TcpSocket _socket;
        
        protected TcpProtocalLinker()
        {
            //初始化连对象
            _socket = new TcpSocket(ConfigurationManager.IP, int.Parse(ConfigurationManager.Port), false);
        }

        public override byte[] SendReceive(byte[] content)
        {
            //接收数据
            byte[] receiveBytes = BytesDecact(_socket.SendMsg(BytesExtend(content)));
            //容错处理
            if (!CheckRight(receiveBytes)) return null;
            //返回数据
            return receiveBytes;
        }


        public override bool SendOnly(byte[] content)
        {
            return _socket.SendMsgWithoutReturn(BytesExtend(content));
        }

    }
    

    public class ProtocalErrorException : Exception
    {
        public ProtocalErrorException(string message):base(message)
        {
            
        }
    }

    public class ModbusProtocalErrorException : ProtocalErrorException
    {
        public int ErrorMessageNumber { get; private set; }
        private static readonly Dictionary<int, string> ProtocalErrorDictionary = new Dictionary<int, string>()
        {
            {1, "ILLEGAL_FUNCTION"},
            {2, "ILLEGAL_DATA_ACCESS"},
            {3, "ILLEGAL_DATA_VALUE"},
            {4, "SLAVE_DEVICE_FAILURE"},
            {5, "ACKNOWLWDGE"},
            {6, "SLAVE_DEVICE_BUSY"},
        };

        public ModbusProtocalErrorException(int messageNumber) : base(ProtocalErrorDictionary[messageNumber])
        {
            ErrorMessageNumber = messageNumber;
        }
    }
}