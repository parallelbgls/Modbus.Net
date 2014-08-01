using System;
using System.Linq;

namespace ModBus.Net
{
    public class TcpProtocalLinker : ProtocalLinker
    {
        private static TcpSocket _socket;

        public TcpProtocalLinker()
        {
            if (_socket == null)
            {
                _socket = new TcpSocket(ConfigurationManager.IP, int.Parse(ConfigurationManager.Port), false);
            }
        }

        public override byte[] SendReceive(byte[] content)
        {
            return BytesDecact(_socket.SendMsg(BytesExtend(content)));
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
}