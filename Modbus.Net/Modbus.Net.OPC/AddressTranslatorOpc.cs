using System;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc地址解析器
    /// </summary>
    public class AddressTranslatorOpc : AddressTranslator
    {
        public override AddressDef AddressTranslate(string address, bool isRead)
        {
            throw new NotImplementedException();
        }

        public override double GetAreaByteLength(string area)
        {
            return 1;
        }
    }
}