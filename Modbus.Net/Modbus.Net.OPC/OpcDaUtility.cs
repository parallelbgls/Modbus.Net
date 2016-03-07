using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
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

        public override async Task<GetDataReturnDef> GetDatasAsync(byte belongAddress, byte masterAddress, string startAddress, int getByteCount)
        {
            try
            {
                var readRequestOpcInputStruct = new ReadRequestOpcInputStruct(startAddress, getByteCount);
                var readRequestOpcOutputStruct =
                     await
                         Wrapper.SendReceiveAsync(Wrapper[typeof(ReadRequestOpcProtocal)], readRequestOpcInputStruct) as ReadRequestOpcOutputStruct;
                return new GetDataReturnDef()
                {
                    ReturnValue = readRequestOpcOutputStruct?.GetValue,
                    IsLittleEndian = Wrapper[typeof (ReadRequestOpcProtocal)].IsLittleEndian
                };
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
