using System;

public enum SimenseType
{
    Ppi = 0,
    Mpi = 1,
    Tcp = 2
};

public enum SimenseMachineModel
{
    S7_200 = 0,
    S7_200_Smart = 1,
    S7_300 = 2,
    S7_400 = 3,
    S7_1200 = 4,
    S7_1500 = 5
};

namespace ModBus.Net
{
    public class SimenseUtility : BaseUtility
    {
        private byte _tdpuSize;
        private ushort _taspSrc;
        private ushort _tsapDst;
        private ushort _maxCalling;
        private ushort _maxCalled;
        private ushort _maxPdu;
        
        private SimenseType _simenseType;

        public SimenseType ConnectionType
        {
            get
            {
                return _simenseType;
            }
            set
            {
                _simenseType = value;
                switch (_simenseType)
                {
                    case SimenseType.Ppi:
                        {
                            throw new NotImplementedException();
                        }
                    case SimenseType.Mpi:
                        {
                            throw new NotImplementedException();
                        }
                    case SimenseType.Tcp:
                    {
                        Wrapper = ConnectionString == null ? new SimenseTcpProtocal(_tdpuSize, _taspSrc, _tsapDst, _maxCalling, _maxCalled, _maxPdu) : new SimenseTcpProtocal(_tdpuSize, _taspSrc, _tsapDst, _maxCalling, _maxCalled, _maxPdu, ConnectionString);
                        break;
                    }
                }
            }
        }

        public SimenseUtility(SimenseType connectionType, string connectionString, SimenseMachineModel model)
        {
            ConnectionString = connectionString;
            switch (model)
            {
                case SimenseMachineModel.S7_200:
                    {
                        _tdpuSize = 0x09;
                        _taspSrc = 0x1001;
                        _tsapDst = 0x1000;
                        _maxCalling = 0x0001;
                        _maxCalled = 0x0001;
                        _maxPdu = 0x03c0;
                        break;
                    }
                case SimenseMachineModel.S7_300:
                case SimenseMachineModel.S7_400:
                    {
                        _tdpuSize = 0x1a;
                        _taspSrc = 0x4b54;
                        _tsapDst = 0x0302;
                        _maxCalling = 0x0001;
                        _maxCalled = 0x0001;
                        _maxPdu = 0x00f0;
                        break;
                    }
            }
            ConnectionType = connectionType;
            AddressTranslator = new AddressTranslatorSimense();            
        }

        public override void SetConnectionType(int connectionType)
        {
            ConnectionType = (SimenseType) connectionType;
        }

        protected override byte[] GetDatas(byte belongAddress, byte materAddress, string startAddress, int getByteCount)
        {
            var readRequestSimenseInputStruct = new ReadRequestSimenseInputStruct(0xd3c7, SimenseTypeCode.Byte, startAddress, (ushort)getByteCount, AddressTranslator);
            var readRequestSimenseOutputStruct =
                 (ReadRequestSimenseOutputStruct)
                     Wrapper.SendReceive(Wrapper[typeof(ReadRequestSimenseProtocal)], readRequestSimenseInputStruct);
            return readRequestSimenseOutputStruct.GetValue;
        }

        public override bool SetDatas(byte belongAddress, byte materAddress, string startAddress, object[] setContents)
        {
            var writeRequestSimenseInputStruct = new WriteRequestSimenseInputStruct(0xd3c8, startAddress, setContents, AddressTranslator);
            var writeRequestSimenseOutputStruct =
                (WriteRequestSimenseOutputStruct)
                    Wrapper.SendReceive(Wrapper[typeof(WriteRequestSimenseProtocal)], writeRequestSimenseInputStruct);
            if (writeRequestSimenseOutputStruct.AccessResult == SimenseAccessResult.NoError) 
                return true;
            else 
                return false;
        }

        public override DateTime GetTime(byte belongAddress)
        {
            throw new NotImplementedException();
        }

        public override bool SetTime(byte belongAddress, DateTime setTime)
        {
            throw new NotImplementedException();
        }
    }
}
