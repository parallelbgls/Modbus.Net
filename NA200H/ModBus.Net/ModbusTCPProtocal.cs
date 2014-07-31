using System;

namespace ModBus.Net
{
    public class ModbusTCPProtocal : ModbusProtocal
    {
        public ModbusTCPProtocal()
        {
            _protocalLinker = new TCPProtocalLinker();
        }
    }

    public class ReadCoilStatusTCPProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (ReadCoilStatusInputStruct) message;
            return Format(r_message.Tag, r_message.Leng, r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.GetCount);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            int tag = ValueHelper.Instance.GetInt(messageBytes, ref flag);
            ushort leng = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte coilCount = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            var coilStatusArr = new bool[coilCount*8];
            for (int i = 0; i < coilCount; i++)
            {
                byte coilStatusGet = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                for (int j = 0; j < 8; j++)
                {
                    if (coilStatusGet%2 == 0) coilStatusArr[8*i + j] = false;
                    else coilStatusArr[8*i + j] = true;
                    coilStatusGet /= 2;
                }
            }
            return new ReadCoilStatusOutputStruct(tag, leng, belongAddress, functionCode, coilCount, coilStatusArr);
        }

        public class ReadCoilStatusInputStruct : InputStruct
        {
            public ReadCoilStatusInputStruct(byte belongAddress, string startAddress, ushort getCount)
            {
                Tag = 0;
                BelongAddress = belongAddress;
                FunctionCode = (int) ModbusProtocalReg.ReadCoilStatus;
                StartAddress = AddressTranslatorNA200H.GetInstance().AddressTranslate(startAddress);
                GetCount = getCount;
                Leng = 6;
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort GetCount { get; private set; }
        }

        public class ReadCoilStatusOutputStruct : OutputStruct
        {
            public ReadCoilStatusOutputStruct(int tag, ushort leng, byte belongAddress, byte functionCode,
                byte coilCount, bool[] coilStatus)
            {
                Tag = tag;
                Leng = leng;
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                CoilCount = coilCount*8;
                CoilStatus = coilStatus.Clone() as bool[];
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public int CoilCount { get; private set; }

            public bool[] CoilStatus { get; private set; }
        }
    }

    public class ReadInputStatusTCPProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (ReadInputStatusInputStruct) message;
            return Format(r_message.Tag, r_message.Leng, r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.GetCount);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            int tag = ValueHelper.Instance.GetInt(messageBytes, ref flag);
            ushort leng = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte inputCount = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            var inputStatusArr = new bool[inputCount*8];
            for (int i = 0; i < inputCount; i++)
            {
                byte inputStatusGet = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                for (int j = 0; j < 8; j++)
                {
                    if (inputStatusGet%2 == 0) inputStatusArr[8*i + j] = false;
                    else inputStatusArr[8*i + j] = true;
                    inputStatusGet /= 2;
                }
            }
            return new ReadInputStatusOutputStruct(tag, leng, belongAddress, functionCode, inputCount, inputStatusArr);
        }

        public class ReadInputStatusInputStruct : InputStruct
        {
            public ReadInputStatusInputStruct(byte belongAddress, string startAddress, ushort getCount)
            {
                Tag = 0;
                BelongAddress = belongAddress;
                FunctionCode = (int) ModbusProtocalReg.ReadInputStatus;
                StartAddress = AddressTranslatorNA200H.GetInstance().AddressTranslate(startAddress);
                GetCount = getCount;
                Leng = 6;
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort GetCount { get; private set; }
        }

        public class ReadInputStatusOutputStruct : OutputStruct
        {
            public ReadInputStatusOutputStruct(int tag, ushort leng, byte belongAddress, byte functionCode,
                byte inputCount, bool[] inputStatus)
            {
                Tag = tag;
                Leng = leng;
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                InputCount = inputCount*8;
                InputStatus = inputStatus.Clone() as bool[];
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public int InputCount { get; private set; }

            public bool[] InputStatus { get; private set; }
        }
    }

    public class ReadHoldRegisterTCPProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (ReadHoldRegisterInputStruct) message;
            return Format(r_message.Tag, r_message.Leng, r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.GetCount);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            int tag = ValueHelper.Instance.GetInt(messageBytes, ref flag);
            ushort leng = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte holdRegisterCount = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            var holdRegisterArr = new ushort[holdRegisterCount/2];
            for (int i = 0; i < holdRegisterCount/2; i++)
            {
                holdRegisterArr[i] = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            }
            return new ReadHoldRegisterOutputStruct(tag, leng, belongAddress, functionCode, holdRegisterCount,
                holdRegisterArr);
        }

        public class ReadHoldRegisterInputStruct : InputStruct
        {
            public ReadHoldRegisterInputStruct(byte belongAddress, string startAddress, ushort getCount)
            {
                Tag = 0;
                BelongAddress = belongAddress;
                FunctionCode = (int) ModbusProtocalReg.ReadHoldRegister;
                StartAddress = AddressTranslatorNA200H.GetInstance().AddressTranslate(startAddress);
                GetCount = getCount;
                Leng = 6;
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort GetCount { get; private set; }
        }

        public class ReadHoldRegisterOutputStruct : OutputStruct
        {
            public ReadHoldRegisterOutputStruct(int tag, ushort leng, byte belongAddress, byte functionCode,
                byte holdRegisterCount, ushort[] holdRegisterStatus)
            {
                Tag = tag;
                Leng = leng;
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                HoldRegisterCount = holdRegisterCount/2;
                HoldRegisterStatus = holdRegisterStatus.Clone() as ushort[];
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public int HoldRegisterCount { get; private set; }

            public ushort[] HoldRegisterStatus { get; private set; }
        }
    }

    public class ReadInputRegisterTCPProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (ReadInputRegisterInputStruct) message;
            return Format(r_message.Tag, r_message.Leng, r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.GetCount);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            if (messageBytes.Length == 25)
            {
                int tag = ValueHelper.Instance.GetInt(messageBytes, ref flag);
                ushort leng = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
                byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte eventByteCount = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte soeProperty = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte soeEvent = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte month = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                int year = ValueHelper.Instance.GetByte(messageBytes, ref flag) + 2002;
                byte hour = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte day = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte second = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte minute = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                ushort millisecond = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
                ushort testPoint = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
                flag += 1;
                byte testValue = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                flag += 1;
                flag += 1;
                return new ReadEventOutputStruct(tag, leng, belongAddress, functionCode, eventByteCount, soeProperty,
                    soeEvent,
                    new DateTime(year, month == 0 ? 1 : 0, day == 0 ? 1 : 0, hour, minute, second, millisecond),
                    testPoint, testValue);
            }
            else
            {
                int tag = ValueHelper.Instance.GetInt(messageBytes, ref flag);
                ushort leng = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
                byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                byte inputRegisterCount = ValueHelper.Instance.GetByte(messageBytes, ref flag);
                var holdRegisterArr = new ushort[inputRegisterCount/2];
                for (int i = 0; i < inputRegisterCount/2; i++)
                {
                    holdRegisterArr[i] = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
                }
                return new ReadInputRegisterOutputStruct(tag, leng, belongAddress, functionCode, inputRegisterCount,
                    holdRegisterArr);
            }
        }

        public class ReadEventOutputStruct : OutputStruct
        {
            public ReadEventOutputStruct(int tag, ushort leng, byte belongAddress, byte functionCode,
                byte eventByteCount, byte soeProperty, byte soeEvent, DateTime time, ushort testPoint, byte testValue)
            {
                Tag = tag;
                Leng = leng;
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                EventByteCount = eventByteCount;
                SoeProperty = soeProperty;
                SoeEvent = soeEvent;
                TestTime = time;
                TestPoint = testPoint;
                TestValue = testValue;
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public byte EventByteCount { get; private set; }

            public byte SoeProperty { get; private set; }

            public byte SoeEvent { get; private set; }

            public DateTime TestTime { get; private set; }

            public ushort TestPoint { get; private set; }

            public byte TestValue { get; private set; }
        }

        public class ReadInputRegisterInputStruct : InputStruct
        {
            public ReadInputRegisterInputStruct(byte belongAddress, string startAddress, ushort getCount)
            {
                Tag = 0;
                BelongAddress = belongAddress;
                FunctionCode = (int) ModbusProtocalReg.ReadInputRegister;
                StartAddress = AddressTranslatorNA200H.GetInstance().AddressTranslate(startAddress);
                GetCount = getCount;
                Leng = 6;
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort GetCount { get; private set; }
        }

        public class ReadInputRegisterOutputStruct : OutputStruct
        {
            public ReadInputRegisterOutputStruct(int tag, ushort leng, byte belongAddress, byte functionCode,
                byte inputRegisterCount, ushort[] inputRegisterStatus)
            {
                Tag = tag;
                Leng = leng;
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                InputRegisterCount = inputRegisterCount/2;
                InputRegisterStatus = inputRegisterStatus.Clone() as ushort[];
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public int InputRegisterCount { get; private set; }

            public ushort[] InputRegisterStatus { get; private set; }
        }
    }

    public class WriteOneCoilTCPProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (WriteOneCoilInputStruct) message;
            return Format(r_message.Tag, r_message.Leng, r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.WriteValue ? new byte[] {0xFF, 0x00} : new byte[] {0x00, 0x00});
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            int tag = ValueHelper.Instance.GetInt(messageBytes, ref flag);
            ushort leng = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort startAddress = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            ushort writeValue = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new WriteOneCoilOutputStruct(tag, leng, belongAddress, functionCode, startAddress,
                writeValue == 0 ? false : true);
        }

        public class WriteOneCoilInputStruct : InputStruct
        {
            public WriteOneCoilInputStruct(byte belongAddress, string startAddress, bool writeValue)
            {
                Tag = 0;
                BelongAddress = belongAddress;
                FunctionCode = (int) ModbusProtocalReg.WriteOneCoil;
                StartAddress = AddressTranslatorNA200H.GetInstance().AddressTranslate(startAddress);
                WriteValue = writeValue;
                Leng = 6;
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public bool WriteValue { get; private set; }
        }

        public class WriteOneCoilOutputStruct : OutputStruct
        {
            public WriteOneCoilOutputStruct(int tag, ushort leng, byte belongAddress, byte functionCode,
                ushort startAddress, bool writeValue)
            {
                Tag = tag;
                Leng = leng;
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                StartAddress = startAddress;
                WriteValue = writeValue;
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public bool WriteValue { get; private set; }
        }
    }

    public class WriteOneRegisterTCPProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (WriteOneRegisterInputStruct) message;
            return Format(r_message.Tag, r_message.Leng, r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.WriteValue);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            int tag = ValueHelper.Instance.GetInt(messageBytes, ref flag);
            ushort leng = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort startAddress = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            ushort writeValue = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new WriteOneRegisterOutputStruct(tag, leng, belongAddress, functionCode, startAddress, writeValue);
        }

        public class WriteOneRegisterInputStruct : InputStruct
        {
            public WriteOneRegisterInputStruct(byte belongAddress, string startAddress, ushort writeValue)
            {
                Tag = 0;
                BelongAddress = belongAddress;
                FunctionCode = (int) ModbusProtocalReg.WriteOneRegister;
                StartAddress = AddressTranslatorNA200H.GetInstance().AddressTranslate(startAddress);
                WriteValue = writeValue;
                Leng = 6;
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteValue { get; private set; }
        }

        public class WriteOneRegisterOutputStruct : OutputStruct
        {
            public WriteOneRegisterOutputStruct(int tag, ushort leng, byte belongAddress, byte functionCode,
                ushort startAddress, ushort writeValue)
            {
                Tag = tag;
                Leng = leng;
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                StartAddress = startAddress;
                WriteValue = writeValue;
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteValue { get; private set; }
        }
    }

    public class WriteMultiCoilTCPProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (WriteMultiCoilInputStruct) message;
            return Format(r_message.Tag, r_message.Leng, r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.WriteCount, r_message.WriteByteCount, r_message.WriteValue);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            int tag = ValueHelper.Instance.GetInt(messageBytes, ref flag);
            ushort leng = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort startAddress = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            ushort writeCount = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new WriteMultiCoilOutputStruct(tag, leng, belongAddress, functionCode, startAddress, writeCount);
        }

        public class WriteMultiCoilInputStruct : InputStruct
        {
            public WriteMultiCoilInputStruct(byte belongAddress, string startAddress, bool[] writeValue)
            {
                Tag = 0;
                BelongAddress = belongAddress;
                FunctionCode = (int) ModbusProtocalReg.WriteMultiCoil;
                StartAddress = AddressTranslatorNA200H.GetInstance().AddressTranslate(startAddress);
                WriteCount = (ushort) writeValue.Length;
                WriteByteCount = WriteCount%8 > 0 ? (byte) (WriteCount/8 + 1) : (byte) (WriteCount/8);
                WriteValue = new byte[WriteByteCount];
                for (int i = 0; i < writeValue.Length; i += 8)
                {
                    int bytenum = 0;
                    for (int j = 7; j >= 0; j--)
                    {
                        int t = i + j < writeValue.Length && writeValue[i + j] ? 1 : 0;
                        bytenum = bytenum*2 + t;
                    }
                    WriteValue[i/8] = (byte) bytenum;
                }
                Leng = (ushort) (7 + WriteByteCount);
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteCount { get; private set; }

            public byte WriteByteCount { get; private set; }

            public byte[] WriteValue { get; private set; }
        }

        public class WriteMultiCoilOutputStruct : OutputStruct
        {
            public WriteMultiCoilOutputStruct(int tag, ushort leng, byte belongAddress, byte functionCode,
                ushort startAddress, ushort writeCount)
            {
                Tag = tag;
                Leng = leng;
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                StartAddress = startAddress;
                WriteCount = writeCount;
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteCount { get; private set; }
        }
    }

    public class WriteMultiRegisterTCPProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (WriteMultiRegisterInputStruct) message;
            return Format(r_message.Tag, r_message.Leng, r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.WriteCount, r_message.WriteByteCount, r_message.WriteValue);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            int tag = ValueHelper.Instance.GetInt(messageBytes, ref flag);
            ushort leng = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort startAddress = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            ushort writeCount = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new WriteMultiRegisterOutputStruct(tag, leng, belongAddress, functionCode, startAddress, writeCount);
        }

        public class WriteMultiRegisterInputStruct : InputStruct
        {
            public WriteMultiRegisterInputStruct(byte belongAddress, string startAddress, ushort[] writeValue)
            {
                Tag = 0;
                BelongAddress = belongAddress;
                FunctionCode = (int) ModbusProtocalReg.WriteMultiRegister;
                StartAddress = AddressTranslatorNA200H.GetInstance().AddressTranslate(startAddress);
                WriteCount = (ushort) writeValue.Length;
                WriteByteCount = (byte) (WriteCount*2);
                WriteValue = writeValue.Clone() as ushort[];
                Leng = (ushort) (7 + WriteByteCount);
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteCount { get; private set; }

            public byte WriteByteCount { get; private set; }

            public ushort[] WriteValue { get; private set; }
        }

        public class WriteMultiRegisterOutputStruct : OutputStruct
        {
            public WriteMultiRegisterOutputStruct(int tag, ushort leng, byte belongAddress, byte functionCode,
                ushort startAddress, ushort writeCount)
            {
                Tag = tag;
                Leng = leng;
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                StartAddress = startAddress;
                WriteCount = writeCount;
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteCount { get; private set; }
        }
    }

    public class GetSystemTimeTCPProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (GetSystemTimeInputStruct) message;
            return Format(r_message.Tag, r_message.Leng, r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.GetCount);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            int tag = ValueHelper.Instance.GetInt(messageBytes, ref flag);
            ushort leng = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte writeByteCount = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort year = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            byte day = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte month = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort hour = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            byte second = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte minute = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort millisecond = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new GetSystemTimeOutputStruct(tag, leng, belongAddress, functionCode, writeByteCount, year, day,
                month, hour, second, minute, millisecond);
        }

        public class GetSystemTimeInputStruct : InputStruct
        {
            public GetSystemTimeInputStruct(byte belongAddress)
            {
                Tag = 0;
                Leng = 6;
                BelongAddress = belongAddress;
                FunctionCode = (int) ModbusProtocalReg.GetSystemTime;
                StartAddress = 30000;
                GetCount = 5;
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort GetCount { get; private set; }
        }

        public class GetSystemTimeOutputStruct : OutputStruct
        {
            public GetSystemTimeOutputStruct(int tag, ushort leng, byte belongAddress, byte functionCode,
                byte writeByteCount, ushort year, byte day, byte month, ushort hour, byte second, byte minute,
                ushort millisecond)
            {
                Tag = tag;
                Leng = leng;
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                WriteByteCount = writeByteCount;
                Time = new DateTime(year, month, day, hour, minute, second, millisecond);
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public byte WriteByteCount { get; private set; }

            public DateTime Time { get; private set; }
        }
    }

    public class SetSystemTimeTCPProtocal : ProtocalUnit
    {
        public override byte[] Format(InputStruct message)
        {
            var r_message = (SetSystemTimeInputStruct) message;
            return Format(r_message.Tag, r_message.Leng, r_message.BelongAddress, r_message.FunctionCode,
                r_message.StartAddress, r_message.WriteCount, r_message.WriteByteCount, r_message.Year, r_message.Day,
                r_message.Month, r_message.Hour, r_message.Second, r_message.Minute, r_message.Millisecond);
        }

        public override OutputStruct Unformat(byte[] messageBytes, ref int flag)
        {
            int tag = ValueHelper.Instance.GetInt(messageBytes, ref flag);
            ushort leng = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            byte belongAddress = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            byte functionCode = ValueHelper.Instance.GetByte(messageBytes, ref flag);
            ushort startAddress = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            ushort writeCount = ValueHelper.Instance.GetUShort(messageBytes, ref flag);
            return new SetSystemTimeOutputStruct(tag, leng, belongAddress, functionCode, startAddress, writeCount);
        }

        public class SetSystemTimeInputStruct : InputStruct
        {
            public SetSystemTimeInputStruct(byte belongAddress, DateTime time)
            {
                Tag = 0;
                BelongAddress = belongAddress;
                FunctionCode = (int) ModbusProtocalReg.SetSystemTime;
                StartAddress = 30000;
                WriteCount = 5;
                WriteByteCount = 10;
                Year = (ushort) time.Year;
                Day = (byte) time.Day;
                Month = (byte) time.Month;
                Hour = (ushort) time.Hour;
                Second = (byte) time.Second;
                Minute = (byte) time.Minute;
                Millisecond = (ushort) time.Millisecond;
                Leng = 17;
            }

            public int Tag { get; private set; }

            public short Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteCount { get; private set; }

            public byte WriteByteCount { get; private set; }

            public ushort Year { get; private set; }

            public byte Day { get; private set; }

            public byte Month { get; private set; }

            public ushort Hour { get; private set; }

            public byte Second { get; private set; }

            public byte Minute { get; private set; }

            public ushort Millisecond { get; private set; }
        }

        public class SetSystemTimeOutputStruct : OutputStruct
        {
            public SetSystemTimeOutputStruct(int tag, ushort leng, byte belongAddress, byte functionCode,
                ushort startAddress, ushort writeCount)
            {
                Tag = tag;
                Leng = leng;
                BelongAddress = belongAddress;
                FunctionCode = functionCode;
                StartAddress = startAddress;
                WriteCount = writeCount;
            }

            public int Tag { get; private set; }

            public ushort Leng { get; private set; }

            public byte BelongAddress { get; private set; }

            public byte FunctionCode { get; private set; }

            public ushort StartAddress { get; private set; }

            public ushort WriteCount { get; private set; }
        }
    }
}