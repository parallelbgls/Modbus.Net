using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net.OPC
{
    public class OpcDaUtility : BaseUtility
    {
        public OpcDaUtility(string connectionString)
        {
            ConnectionString = connectionString;
            AddressTranslator = null;
            Wrapper = new OpcDaProtocal(ConnectionString);
        }

        public override void SetConnectionType(int connectionType)
        {
        }

        protected override async Task<byte[]> GetDatasAsync(byte belongAddress, byte masterAddress, string startAddress, int getByteCount)
        {
            try
            {
                var readRequestOpcInputStruct = new ReadRequestOpcInputStruct(startAddress, getByteCount);
                var readRequestOpcOutputStruct =
                     await
                         Wrapper.SendReceiveAsync(Wrapper[typeof(ReadRequestOpcProtocal)], readRequestOpcInputStruct) as ReadRequestOpcOutputStruct;
                return readRequestOpcOutputStruct?.GetValue;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override Task<bool> SetDatasAsync(byte belongAddress, byte masterAddress, string startAddress, object[] setContents)
        {
            throw new NotImplementedException();
        }
    }
}
