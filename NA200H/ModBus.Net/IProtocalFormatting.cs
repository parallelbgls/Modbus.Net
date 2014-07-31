namespace ModBus.Net
{
    public interface IProtocalFormatting
    {
        byte[] Format(InputStruct message);

        byte[] Format(params object[] message);

        OutputStruct Unformat(byte[] messageBytes, ref int pos);
    }
}