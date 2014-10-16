using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net
{
    public class SimenseTcpProtocal : SimenseProtocal
    {
        private ushort _taspSrc;
        private ushort _tsapDst;
        private ushort _maxCalling;
        private ushort _maxCalled;
        private ushort _maxPdu;
        private string _ip;
        private int connectTryCount;

        public SimenseTcpProtocal(ushort tsapSrc, ushort tsapDst, ushort maxCalling, ushort maxCalled, ushort maxPdu) : this(tsapSrc, tsapDst, maxCalling, maxCalled, maxPdu, ConfigurationManager.IP)
        {
        }

        public SimenseTcpProtocal(ushort tsapSrc, ushort tsapDst, ushort maxCalling, ushort maxCalled, ushort maxPdu, string ip)
        {
            _taspSrc = tsapSrc;
            _tsapDst = tsapDst;
            _maxCalling = maxCalling;
            _maxCalled = maxCalled;
            _maxPdu = maxPdu;
            _ip = ip;
            connectTryCount = 0;
            Connected();
        }

        public override byte[] SendReceive(params object[] content)
        {
            while (!ProtocalLinker.IsConnected)
            {
                Connected();
            }
            return base.SendReceive(content);
        }

        public override OutputStruct SendReceive(ProtocalUnit unit, InputStruct content)
        {
            if (!ProtocalLinker.IsConnected)
            {
                if (connectTryCount > 10) return null;
                Connected();
            }
            return base.SendReceive(unit, content);
        }

        private OutputStruct ForceSendReceive(ProtocalUnit unit, InputStruct content)
        {
            return base.SendReceive(unit, content);
        }

        protected void Connected()
        {
            connectTryCount++;
            ProtocalLinker = new SimenseTcpProtocalLinker(_ip);
            var inputStruct = new CreateReferenceSimenseInputStruct(0x1a, _taspSrc, _tsapDst);
            var outputStruct =
                (CreateReferenceSimenseOutputStruct) ForceSendReceive(this[typeof(CreateReferenceSimenseProtocal)], inputStruct);
            if (!ProtocalLinker.IsConnected) return;
            var inputStruct2 = new EstablishAssociationSimenseInputStruct(0x0101, _maxCalling, _maxCalled, _maxPdu);
            var outputStruct2 = (EstablishAssociationSimenseOutputStruct)SendReceive(this[typeof(EstablishAssociationSimenseProtocal)], inputStruct2);
        }
    }
}
