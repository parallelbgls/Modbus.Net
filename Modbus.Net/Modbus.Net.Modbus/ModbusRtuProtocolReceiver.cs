using System;

namespace Modbus.Net.Modbus
{
    public class ModbusRtuProtocolReceiver : ProtocolReceiver
    {
        public ModbusRtuProtocolReceiver(string com, int slaveAddress)
            : base(com, slaveAddress)
        {

        }

        protected override Func<byte[], ReceiveDataDef> DataExplain
        {
            get
            {
                return receiveBytes =>
                {
                    var writeContent = receiveBytes.Length > 6 ? new byte[receiveBytes.Length - 7] : null;
                    if (receiveBytes.Length > 6)
                    {
                        Array.Copy(receiveBytes, 7, writeContent, 0, receiveBytes.Length - 7);
                        return new ReceiveDataDef()
                        {
                            SlaveAddress = receiveBytes[0],
                            FunctionCode = receiveBytes[1],
                            StartAddress = (ushort)(receiveBytes[2] * 256 + receiveBytes[3]),
                            Count = (ushort)(receiveBytes[4] * 256 + receiveBytes[5]),
                            WriteByteCount = (byte)(receiveBytes.Length > 6 ? receiveBytes[6] : 0),
                            WriteContent = writeContent
                        };
                    }
                    else if (receiveBytes.Length == 6)
                    {
                        if (receiveBytes[1] == 1 || receiveBytes[1] == 3)
                        {
                            writeContent = null;
                            return new ReceiveDataDef()
                            {
                                SlaveAddress = receiveBytes[0],
                                FunctionCode = receiveBytes[1],
                                StartAddress = (ushort)(receiveBytes[2] * 256 + receiveBytes[3]),
                                Count = (ushort)(receiveBytes[4] * 256 + receiveBytes[5]),
                                WriteByteCount = 2,
                                WriteContent = writeContent
                            };
                        }
                        else
                        {
                            writeContent = new byte[2] { receiveBytes[4], receiveBytes[5] };
                            return new ReceiveDataDef()
                            {
                                SlaveAddress = receiveBytes[0],
                                FunctionCode = receiveBytes[1],
                                StartAddress = (ushort)(receiveBytes[2] * 256 + receiveBytes[3]),
                                Count = 1,
                                WriteByteCount = 2,
                                WriteContent = writeContent
                            };
                        }
                    }
                    else return null;
                };
            }
        }
    }
}