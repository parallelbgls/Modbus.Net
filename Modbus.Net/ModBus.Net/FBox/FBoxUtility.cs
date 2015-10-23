using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net.FBox
{
    public enum FBoxType
    {
        AddressSync = 0,
        CommunicationTagSync = 1
    }

    public class FBoxUtility :BaseUtility
    {
        private FBoxType _fboxType;
        private SignalRSigninMsg SigninMsg { get; set; }
        
        protected IEnumerable<CommunicationUnit> CommunicationUnits { get; set; } 
        public FBoxType ConnectionType
        {
            get
            {
                return _fboxType;
            }
            set
            {
                _fboxType = value;
                switch (_fboxType)
                {
                    case FBoxType.AddressSync:
                        {
                            throw new NotImplementedException();
                        }
                    case FBoxType.CommunicationTagSync:
                        {
                            Wrapper = new FBoxSignalRProtocal(ConnectionString, SigninMsg);
                            break;
                        }
                }
            }
        }

        public FBoxUtility(FBoxType fBoxType, string connectionString, IEnumerable<CommunicationUnit> communicationUnits, SignalRSigninMsg msg)
        {
            ConnectionString = connectionString;
            CommunicationUnits = communicationUnits.AsEnumerable();
            SigninMsg = msg;

            ConnectionType = fBoxType;           
            AddressTranslator = new AddressTranslatorFBox();
        }

        public override void SetConnectionType(int connectionType)
        {
            ConnectionType = (FBoxType) connectionType;
        }

        protected override async Task<byte[]> GetDatasAsync(byte belongAddress, byte masterAddress, string startAddress, int getByteCount)
        {
            try
            {
                var readRequestFBoxInputStruct = new ReadRequestFBoxInputStruct(startAddress, (ushort)getByteCount, AddressTranslator);
                var readRequestSimenseOutputStruct =
                     (ReadRequestFBoxOutputStruct)await
                         Wrapper.SendReceiveAsync(Wrapper[typeof(ReadRequestFBoxProtocal)], readRequestFBoxInputStruct);
                return readRequestSimenseOutputStruct.GetValue;
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
