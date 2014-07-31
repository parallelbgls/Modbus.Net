namespace ModBus.Net
{
    public abstract class ProtocalLinker
    {
        public abstract byte[] SendReceive(byte[] content);

        public abstract bool SendOnly(byte[] content);
    }
}