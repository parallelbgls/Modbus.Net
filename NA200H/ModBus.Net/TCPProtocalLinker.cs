using System;
using System.Collections.Generic;
using System.Linq;

namespace ModBus.Net
{
    public class TcpProtocalLinker : ProtocalLinker
    {
        private TcpSocket _socket;

         
        public TcpProtocalLinker()
        {
            _socket = new TcpSocket(ConfigurationManager.IP, int.Parse(ConfigurationManager.Port), false);
        }

        public override byte[] SendReceive(byte[] content)
        {
            byte[] receiveBytes = BytesDecact(_socket.SendMsg(BytesExtend(content)));
            if (receiveBytes[1] > 127)
            {
                string message;
                throw new ModbusProtocalErrorException(receiveBytes[2]);
            }
            return receiveBytes;
        }

        public override bool SendOnly(byte[] content)
        {
            return _socket.SendMsgWithoutReturn(BytesExtend(content));
        }

    }

    public abstract class ProtocalLinkerBytesExtend
    {
        public abstract byte[] BytesExtend(byte[] content);

        public abstract byte[] BytesDecact(byte[] content);
    }

    public class TcpProtocalLinkerBytesExtend : ProtocalLinkerBytesExtend
    {
        public override byte[] BytesExtend(byte[] content)
        {
            byte[] newFormat = new byte[6 + content.Length];
            int tag = 0;
            ushort leng = (ushort)content.Length;
            Array.Copy(ValueHelper.Instance.GetBytes(tag), 0, newFormat, 0, 4);
            Array.Copy(ValueHelper.Instance.GetBytes(leng), 0, newFormat, 4, 2);
            Array.Copy(content, 0, newFormat, 6, content.Length);
            return newFormat;
        }

        public override byte[] BytesDecact(byte[] content)
        {
            byte[] newContent = new byte[content.Length - 6];
            Array.Copy(content, 6, newContent, 0, newContent.Length);
            return newContent;
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