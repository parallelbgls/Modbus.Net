using System.Reflection;

namespace ModBus.Net
{
    public abstract class ProtocalLinker
    {
        public abstract byte[] SendReceive(byte[] content);

        public abstract bool SendOnly(byte[] content);

        public byte[] BytesExtend(byte[] content)
        {
            ProtocalLinkerBytesExtend bytesExtend =
                Assembly.Load("ModBus.Net").CreateInstance(this.GetType().FullName + "BytesExtend") as
                    ProtocalLinkerBytesExtend;
            return bytesExtend.BytesExtend(content);
        }

        public byte[] BytesDecact(byte[] content)
        {
            ProtocalLinkerBytesExtend bytesExtend =
                Assembly.Load("ModBus.Net").CreateInstance(this.GetType().FullName + "BytesExtend") as
                    ProtocalLinkerBytesExtend;
            return bytesExtend.BytesDecact(content);
        }
    }
}