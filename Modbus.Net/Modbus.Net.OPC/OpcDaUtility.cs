using System;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    /// <summary>
    ///     Opc Da协议Api入口
    /// </summary>
    public class OpcDaUtility : BaseUtility
    {
        public OpcDaUtility(string connectionString) : base(0, 0)
        {
            ConnectionString = connectionString;
            AddressTranslator = new AddressTranslatorOpc();
            Wrapper = new OpcDaProtocal(ConnectionString);
        }

        public override Endian Endian => Endian.BigEndianLsb;

        public override void SetConnectionType(int connectionType)
        {
        }

        public override async Task<byte[]> GetDatasAsync(string startAddress, int getByteCount)
        {
            try
            {
                var readRequestOpcInputStruct = new ReadRequestOpcInputStruct(startAddress);
                var readRequestOpcOutputStruct =
                    await
                        Wrapper.SendReceiveAsync(Wrapper[typeof (ReadRequestOpcProtocal)], readRequestOpcInputStruct) as
                        ReadRequestOpcOutputStruct;
                return readRequestOpcOutputStruct?.GetValue;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override async Task<bool> SetDatasAsync(string startAddress, object[] setContents)
        {
            try
            {
                var writeRequestOpcInputStruct = new WriteRequestOpcInputStruct(startAddress, setContents[0]);
                var writeRequestOpcOutputStruct =
                    await
                        Wrapper.SendReceiveAsync(Wrapper[typeof (WriteRequestOpcProtocal)], writeRequestOpcInputStruct)
                        as WriteRequestOpcOutputStruct;
                return writeRequestOpcOutputStruct?.WriteResult == true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}