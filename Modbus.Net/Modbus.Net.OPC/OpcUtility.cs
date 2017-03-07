using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.OPC
{
    public abstract class OpcUtility : BaseUtility
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
                var readRequestOpcInputStruct = new ReadRequestOpcInputStruct(startAddress, split.ToString());
                var readRequestOpcOutputStruct =
                    await
                        Wrapper.SendReceiveAsync(Wrapper[typeof(ReadRequestOpcProtocal)], readRequestOpcInputStruct) as
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
                var split = GetSeperator?.Invoke() ?? '/';
                var writeRequestOpcInputStruct = new WriteRequestOpcInputStruct(startAddress, split.ToString(), setContents[0]);
                var writeRequestOpcOutputStruct =
                    await
                        Wrapper.SendReceiveAsync(Wrapper[typeof(WriteRequestOpcProtocal)], writeRequestOpcInputStruct)
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
