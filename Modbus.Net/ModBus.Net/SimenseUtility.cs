using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum SimenseType
{
    Ppi = 0,
    Mpi = 1,
    Tcp = 2
}

namespace ModBus.Net
{
    public class SimenseUtility : BaseUtility
    {
        private string _connectionString;

        public override string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                string[] splitStrings = value.Split(',');
                switch (splitStrings[1])
                {
                    case "200":
                        {
                            _taspSrc = 0x4d57;
                            _tsapDst = 0x4d57;
                            _maxCalling = 0x0001;
                            _maxCalled = 0x0001;
                            _maxPdu = 0x03c0;
                            break;
                        }
                }
                _connectionString = splitStrings[0];              
            }
        }

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
                        Wrapper = ConnectionString == null ? new SimenseTcpProtocal(_taspSrc, _tsapDst, _maxCalling, _maxCalled, _maxPdu) : new SimenseTcpProtocal(_taspSrc, _tsapDst, _maxCalling, _maxCalled, _maxPdu, ConnectionString);
                        break;
                    }
                }
            }
        }

        public SimenseUtility(SimenseType connectionType, string connectionString)
        {
            ConnectionString = connectionString;
            ConnectionType = connectionType;
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
