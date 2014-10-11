using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModBus.Net;
using System.Reflection;

namespace NA200H.UI.ConsoleApp
{
    class Program
    {
        private static void Main(string[] args)
        {
            string ip = "192.168.3.247";
            //先初始化一个协议转换器，这里构造Modbus/Tcp协议。
            BaseProtocal wrapper = new ModbusTcpProtocal(ip);

            /*
            try
            {
                Console.WriteLine("link ip is {0}",ip);
                //第一步：先生成一个输入信息的object数组
                object[] inputObjects = new object[] {(byte) 0x02, (byte) 0x01, (short) 0x4e20, (short) 0x0004};
                //第二步：向仪器发送这个信息，并接收信息
                byte[] outputBytes = wrapper.SendReceive(inputObjects);
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
            byte[] outputBytes = wrapper.SendReceive(inputObjects);
            //第三步：输出信息
            for (int i = 0; i < outputBytes.Length; i++)
            {
                Console.WriteLine(outputBytes[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();*/

            
            //调用方法二：自动构造
            //第一步：先生成一个输入结构体，然后向这个结构体中填写数据
            AddressTranslator.CreateTranslator(new AddressTranslatorNA200H());

            ReadDataInputStruct readCoilStatusInputStruct = new ReadDataInputStruct(0x02, "N1", 0x0a);
            //第二步：再生成一个输出结构体，执行相应协议的发送指令，并将输出信息自动转换到输出结构体中
            ReadDataOutputStruct readCoilStatusOutputStruct = (ReadDataOutputStruct)wrapper.SendReceive(wrapper["ReadDataModbusProtocal"], readCoilStatusInputStruct);
            //第三步：读取这个输出结构体的信息。
            bool[] array =
                ValueHelper.Instance.ObjectArrayToDestinationArray<bool>(
                    ValueHelper.Instance.ByteArrayToObjectArray(readCoilStatusOutputStruct.DataValue,
                        new KeyValuePair<Type, int>(typeof (bool), 0x0a)));
            for (int i = 0; i < array.Length; i++)
            {
                Console.WriteLine(array[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();

            ReadDataInputStruct readHoldRegisterInputStruct = new ReadDataInputStruct(0x02, "NW1", 4);
            ReadDataOutputStruct readHoldRegisterOutputStruct = (ReadDataOutputStruct)wrapper.SendReceive(wrapper["ReadDataModbusProtocal"], readHoldRegisterInputStruct);
            ushort[] array2 =
                ValueHelper.Instance.ObjectArrayToDestinationArray<ushort>(
                    ValueHelper.Instance.ByteArrayToObjectArray(readHoldRegisterOutputStruct.DataValue,
                        new KeyValuePair<Type, int>(typeof (ushort), 8)));
            for (int i = 0; i < array2.Length; i++)
            {
                Console.WriteLine(array2[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteDataInputStruct writeMultiCoilInputStruct = new WriteDataInputStruct(0x02, "Q20", new object[] { true, false, true, true, false, false, true, true, true, false });
            WriteDataOutputStruct writeMultiCoilOutputStruct = (WriteDataOutputStruct)wrapper.SendReceive(wrapper["WriteDataModbusProtocal"], writeMultiCoilInputStruct);
            Console.WriteLine(writeMultiCoilOutputStruct.StartAddress);
            Console.WriteLine(writeMultiCoilOutputStruct.WriteCount);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteDataInputStruct writeMultiRegisterInputStruct = new WriteDataInputStruct(0x02, "NW1", new object[] { (ushort)25, (ushort)18, (ushort)17 });
            WriteDataOutputStruct writeMultiRegisterOutputStruct = (WriteDataOutputStruct)wrapper.SendReceive(wrapper["WriteDataModbusProtocal"], writeMultiRegisterInputStruct);
            Console.WriteLine(writeMultiRegisterOutputStruct.StartAddress);
            Console.WriteLine(writeMultiRegisterOutputStruct.WriteCount);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            GetSystemTimeInputStruct getSystemTimeInputStruct = new GetSystemTimeInputStruct(0x02);
            GetSystemTimeOutputStruct getSystemTimeOutputStruct = (GetSystemTimeOutputStruct)wrapper.SendReceive(wrapper["GetSystemTimeModbusProtocal"], getSystemTimeInputStruct);
            Console.WriteLine(getSystemTimeOutputStruct.Time);
            Console.Read();
            Console.Read();

            SetSystemTimeInputStruct setSystemTimeInputStruct = new SetSystemTimeInputStruct(0x02, DateTime.Now);
            SetSystemTimeOutputStruct setSystemTimeOutputStruct = (SetSystemTimeOutputStruct)wrapper.SendReceive(wrapper["SetSystemTimeModbusProtocal"], setSystemTimeInputStruct);
            Console.WriteLine(setSystemTimeOutputStruct.StartAddress);
            Console.WriteLine(setSystemTimeOutputStruct.WriteCount);
            Console.Read();
            Console.Read();
            
        }
    }
}
