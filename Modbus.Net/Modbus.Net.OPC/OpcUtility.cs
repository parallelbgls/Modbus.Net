using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    public abstract class OpcUtility : BaseUtility<OpcParamIn, OpcParamOut, ProtocalUnit<OpcParamIn, OpcParamOut>>
    { 
        protected OpcUtility(string connectionString) : base(0, 0)
        {
            ConnectionString = connectionString;
            AddressTranslator = new AddressTranslatorOpc();
        }

        public delegate char GetSeperatorDelegate();

        public event GetSeperatorDelegate GetSeperator;

        public override Endian Endian => Endian.BigEndianLsb;

        public override void SetConnectionType(int connectionType)
        {
            throw new NotImplementedException();
        }

        public override async Task<byte[]> GetDatasAsync(string startAddress, int getByteCount)
        {
            try
            {
                var split = GetSeperator?.Invoke() ?? '/';
                var readRequestOpcInputStruct = new ReadRequestOpcInputStruct(startAddress, split);
                var readRequestOpcOutputStruct =
                    await
						Wrapper.SendReceiveAsync<ReadRequestOpcOutputStruct>(Wrapper[typeof(ReadRequestOpcProtocal)], readRequestOpcInputStruct);
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
                var split = GetSeperator?.Invoke() ?? '/';
                var writeRequestOpcInputStruct = new WriteRequestOpcInputStruct(startAddress, split, setContents[0]);
                var writeRequestOpcOutputStruct =
                    await
						Wrapper.SendReceiveAsync<WriteRequestOpcOutputStruct>(Wrapper[typeof(WriteRequestOpcProtocal)], writeRequestOpcInputStruct);
                return writeRequestOpcOutputStruct?.WriteResult == true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
