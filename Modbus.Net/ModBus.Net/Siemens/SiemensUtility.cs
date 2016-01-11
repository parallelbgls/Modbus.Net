using System;
using System.Threading.Tasks;

namespace ModBus.Net.Siemens
{
    public enum SiemensType
    {
        Ppi = 0,
        Mpi = 1,
        Tcp = 2
    };

    public enum SiemensMachineModel
    {
        S7_200 = 0,
        S7_200_Smart = 1,
        S7_300 = 2,
        S7_400 = 3,
        S7_1200 = 4,
        S7_1500 = 5
    };


    public class SiemensUtility : BaseUtility
    {
        private byte _tdpuSize;
        private ushort _taspSrc;
        private ushort _tsapDst;
        private ushort _maxCalling;
        private ushort _maxCalled;
        private ushort _maxPdu;
        
        private SiemensType _siemensType;

        public SiemensType ConnectionType
        {
            get
            {
                return _siemensType;
            }
            set
            {
                _siemensType = value;
                switch (_siemensType)
                {
                    case SiemensType.Ppi:
                        {
                            throw new NotImplementedException();
                        }
                    case SiemensType.Mpi:
                        {
                            throw new NotImplementedException();
                        }
                    case SiemensType.Tcp:
                    {
                        Wrapper = ConnectionString == null ? new SiemensTcpProtocal(_tdpuSize, _taspSrc, _tsapDst, _maxCalling, _maxCalled, _maxPdu) : new SiemensTcpProtocal(_tdpuSize, _taspSrc, _tsapDst, _maxCalling, _maxCalled, _maxPdu, ConnectionString);
                        break;
                    }
                }
            }
        }

        public SiemensUtility(SiemensType connectionType, string connectionString, SiemensMachineModel model)
        {
            ConnectionString = connectionString;
            switch (model)
            {
                case SiemensMachineModel.S7_200:
                    {
                        _tdpuSize = 0x09;
                        _taspSrc = 0x1001;
                        _tsapDst = 0x1000;
                        _maxCalling = 0x0001;
                        _maxCalled = 0x0001;
                        _maxPdu = 0x03c0;
                        break;
                    }
                case SiemensMachineModel.S7_300:
                case SiemensMachineModel.S7_400:
                    {
                        _tdpuSize = 0x1a;
                        _taspSrc = 0x4b54;
                        _tsapDst = 0x0302;
                        _maxCalling = 0x0001;
                        _maxCalled = 0x0001;
                        _maxPdu = 0x00f0;
                        break;
                    }
                case SiemensMachineModel.S7_1200:
                case SiemensMachineModel.S7_1500:
                {
                    _tdpuSize = 0x09;
                    _taspSrc = 0x4b54;
                    _tsapDst = 0x0300;
                    _maxCalling = 0x0001;
                    _maxCalled = 0x0001;
                    _maxPdu = 0x00f0;
                    break;
                }
            }
            ConnectionType = connectionType;
            AddressTranslator = new AddressTranslatorSiemens();            
        }

        public override void SetConnectionType(int connectionType)
        {
            ConnectionType = (SiemensType) connectionType;
        }

        protected override async Task<byte[]> GetDatasAsync(byte belongAddress, byte materAddress, string startAddress, int getByteCount)
        {
            try
            {
                var readRequestSiemensInputStruct = new ReadRequestSiemensInputStruct(0xd3c7, SiemensTypeCode.Byte, startAddress, (ushort)getByteCount, AddressTranslator);
                var readRequestSiemensOutputStruct =
                     (ReadRequestSiemensOutputStruct) await
                         Wrapper.SendReceiveAsync(Wrapper[typeof(ReadRequestSiemensProtocal)], readRequestSiemensInputStruct);
                return readRequestSiemensOutputStruct.GetValue;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override async Task<bool> SetDatasAsync(byte belongAddress, byte materAddress, string startAddress, object[] setContents)
        {
            try
            {
                var writeRequestSiemensInputStruct = new WriteRequestSiemensInputStruct(0xd3c8, startAddress, setContents, AddressTranslator);
                var writeRequestSiemensOutputStruct =
                    (WriteRequestSiemensOutputStruct) await
                        Wrapper.SendReceiveAsync(Wrapper[typeof(WriteRequestSiemensProtocal)], writeRequestSiemensInputStruct);
                if (writeRequestSiemensOutputStruct.AccessResult == SiemensAccessResult.NoError)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /*
        public override DateTime GetTime(byte belongAddress)
        {
            throw new NotImplementedException();
        }

        public override bool SetTime(byte belongAddress, DateTime setTime)
        {
            throw new NotImplementedException();
        }
        */
    }
}
