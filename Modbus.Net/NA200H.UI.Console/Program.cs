using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Net;
using System.Reflection;
using Modbus.Net.Modbus;
using Modbus.Net.Siemens;

namespace NA200H.UI.ConsoleApp
{
    class Program
    {
        private static void Main(string[] args)
        {
            string ip = "192.168.3.191";
            //先初始化一个协议转换器，这里构造Modbus/Tcp协议。
            BaseProtocal wrapper = new ModbusTcpProtocal(ip, 2, 0);

            /*
            try
            {
                Console.WriteLine("link ip is {0}",ip);
                //第一步：先生成一个输入信息的object数组
                object[] inputObjects = new object[] {(byte) 0x02, (byte) 0x01, (short) 0x4e20, (short) 0x0004};
                //第二步：向仪器发送这个信息，并接收信息
                byte[] outputBytes = wrapper.SendReceive(false, inputObjects);
                //第三步：输出信息
                for (int i = 0; i < outputBytes.Length; i++)
                {
                    Console.WriteLine(outputBytes[i]);
                }
                Console.WriteLine("modbus link success");
            }
            catch (Exception)
            {
                Console.WriteLine("modbus link failed");
            }
            Console.Read();
            */

            /*
            //调用方法一：手动构造
            //第一步：先生成一个输入信息的object数组
            object[] inputObjects = new object[]{(byte)0x02,(byte)0x01,(short)0x4e20,(short)0x0004};
            //第二步：向仪器发送这个信息，并接收信息
            byte[] outputBytes = wrapper.SendReceive(false, inputObjects);
            //第三步：输出信息
            for (int i = 0; i < outputBytes.Length; i++)
            {
                Console.WriteLine(outputBytes[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();

            
            //调用方法二：自动构造
            //第一步：先生成一个输入结构体，然后向这个结构体中填写数据
            AddressTranslator addressTranslator = new AddressTranslatorNA200H();

            ReadDataModbusInputStruct readCoilStatusInputStruct = new ReadDataModbusInputStruct(0x02, "N 1", 0x0a, addressTranslator);
            //第二步：再生成一个输出结构体，执行相应协议的发送指令，并将输出信息自动转换到输出结构体中
            ReadDataModbusOutputStruct readCoilStatusOutputStruct = (ReadDataModbusOutputStruct)wrapper.SendReceive(wrapper[typeof(ReadDataModbusProtocal)], readCoilStatusInputStruct);
            //第三步：读取这个输出结构体的信息。
            bool[] array =
                BigEndianValueHelper.Instance.ObjectArrayToDestinationArray<bool>(
                    BigEndianValueHelper.Instance.ByteArrayToObjectArray(readCoilStatusOutputStruct.DataValue,
                        new KeyValuePair<Type, int>(typeof (bool), 0x0a)));
            for (int i = 0; i < array.Length; i++)
            {
                Console.WriteLine(array[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();

            ReadDataModbusInputStruct readHoldRegisterInputStruct = new ReadDataModbusInputStruct(0x02, "NW 1", 4, addressTranslator);
            ReadDataModbusOutputStruct readHoldRegisterOutputStruct = (ReadDataModbusOutputStruct)wrapper.SendReceive(wrapper[typeof(ReadDataModbusProtocal)], readHoldRegisterInputStruct);
            ushort[] array2 =
                BigEndianValueHelper.Instance.ObjectArrayToDestinationArray<ushort>(
                    BigEndianValueHelper.Instance.ByteArrayToObjectArray(readHoldRegisterOutputStruct.DataValue,
                        new KeyValuePair<Type, int>(typeof (ushort), 8)));
            for (int i = 0; i < array2.Length; i++)
            {
                Console.WriteLine(array2[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteDataModbusInputStruct writeMultiCoilInputStruct = new WriteDataModbusInputStruct(0x02, "Q 20", new object[] { true, false, true, true, false, false, true, true, true, false }, addressTranslator);
            WriteDataModbusOutputStruct writeMultiCoilOutputStruct = (WriteDataModbusOutputStruct)wrapper.SendReceive(wrapper[typeof(WriteDataModbusProtocal)], writeMultiCoilInputStruct);
            Console.WriteLine(writeMultiCoilOutputStruct.StartAddress);
            Console.WriteLine(writeMultiCoilOutputStruct.WriteCount);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteDataModbusInputStruct writeMultiRegisterInputStruct = new WriteDataModbusInputStruct(0x02, "NW 1", new object[] { (ushort)25, (ushort)18, (ushort)17 }, addressTranslator);
            WriteDataModbusOutputStruct writeMultiRegisterOutputStruct = (WriteDataModbusOutputStruct)wrapper.SendReceive(wrapper[typeof(WriteDataModbusProtocal)], writeMultiRegisterInputStruct);
            Console.WriteLine(writeMultiRegisterOutputStruct.StartAddress);
            Console.WriteLine(writeMultiRegisterOutputStruct.WriteCount);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            GetSystemTimeModbusInputStruct getSystemTimeInputStruct = new GetSystemTimeModbusInputStruct(0x02);
            GetSystemTimeModbusOutputStruct getSystemTimeOutputStruct = (GetSystemTimeModbusOutputStruct)wrapper.SendReceive(wrapper[typeof(GetSystemTimeModbusProtocal)], getSystemTimeInputStruct);
            Console.WriteLine(getSystemTimeOutputStruct.Time);
            Console.Read();
            Console.Read();

            SetSystemTimeModbusInputStruct setSystemTimeInputStruct = new SetSystemTimeModbusInputStruct(0x02, DateTime.Now);
            SetSystemTimeModbusOutputStruct setSystemTimeOutputStruct = (SetSystemTimeModbusOutputStruct)wrapper.SendReceive(wrapper[typeof(SetSystemTimeModbusProtocal)], setSystemTimeInputStruct);
            Console.WriteLine(setSystemTimeOutputStruct.StartAddress);
            Console.WriteLine(setSystemTimeOutputStruct.WriteCount);
            Console.Read();
            Console.Read();
            */

            /*
            BaseProtocal wrapper = new SiemensTcpProtocal(0x09, 0x1001, 0x1000, 0x0001, 0x0001, 0x03c0, ip);
            if (!wrapper.ProtocalLinker.IsConnected) return;
            AddressTranslator addressTranslator = new AddressTranslatorSiemens();

            var readRequestSiemensInputStruct = new ReadRequestSiemensInputStruct(2, 0, 0xaacc, SiemensTypeCode.Byte, "V 0", 4, addressTranslator);
            var readRequestSiemensOutputStruct =
                (ReadRequestSiemensOutputStruct)
                    wrapper.SendReceive(wrapper[typeof(ReadRequestSiemensProtocal)], readRequestSiemensInputStruct);
            ushort[] array =
                BigEndianValueHelper.Instance.ObjectArrayToDestinationArray<ushort>(
                    BigEndianValueHelper.Instance.ByteArrayToObjectArray(readRequestSiemensOutputStruct.GetValue,
                        new KeyValuePair<Type, int>(typeof (ushort), 2)));
            for (int i = 0; i < array.Length; i++)
            {
                Console.WriteLine(array[i]);
            }
            Console.Read();
            Console.Read();

            var writeRequestSiemensInputStruct = new WriteRequestSiemensInputStruct(2, 0, 0xaadd, "V 100",
                new object[] { (ushort)280, (ushort)12, (ushort)56, (ushort)72, (ushort)88, (ushort)525, (ushort)477, (ushort)151, (ushort)52 }, addressTranslator);
            var writeRequestSiemensOutputStruct =
                (WriteRequestSiemensOutputStruct)
                    wrapper.SendReceive(wrapper[typeof(WriteRequestSiemensProtocal)], writeRequestSiemensInputStruct);
            Console.WriteLine(writeRequestSiemensOutputStruct.AccessResult.ToString());
            Console.Read();
            Console.Read();        
            */
        }
    }
}
