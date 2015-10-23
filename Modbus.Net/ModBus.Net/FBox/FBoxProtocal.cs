using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModBus.Net;
using ModBus.Net.FBox;

namespace ModBus.Net.FBox
{
    public abstract class FBoxProtocal : BaseProtocal
    {
        public override bool Connect()
        {
            return ProtocalLinker.Connect();
        }

        public override async Task<bool> ConnectAsync()
        {
            return await ProtocalLinker.ConnectAsync();
        }
    }

    public class ReadRequestFBoxInputStruct : InputStruct
    {
        public ReadRequestFBoxInputStruct(string startAddress, ushort getCount, AddressTranslator addressTranslator)
        {
            var address = addressTranslator.AddressTranslate(startAddress, true);
            Address = address.Key;
            Area = address.Value;
            GetCount = getCount;
        }

        public int Area { get; set; }
        public int Address { get; set; }
        public ushort GetCount { get; set; }
    }


    public class ReadRequestFBoxOutputStruct : OutputStruct
    {
        public ReadRequestFBoxOutputStruct(byte[] value)
        {
            GetValue = value;
        }

        public byte[] GetValue { get; private set; }
    }

    public class ReadRequestFBoxProtocal : SpecialProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (ReadRequestFBoxInputStruct) message;
            return Format(r_message.Area, r_message.Address, r_message.GetCount);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int pos)
        {
            var values = new byte[messageBytes.Length];
            Array.Copy(messageBytes, pos, values, 0, messageBytes.Length);
            return new ReadRequestFBoxOutputStruct(values);
        }
    }
}
