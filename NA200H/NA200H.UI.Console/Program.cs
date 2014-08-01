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
        static void Main(string[] args)
        {
            //先初始化一个协议转换器，这里构造Modbus/Tcp协议。
            BaseProtocal wrapper = new ModbusTcpProtocal();

            //调用方法一：手动构造
            //第一步：先生成一个输入信息的object数组
            object[] inputObjects = new object[]{(byte)0x11,(byte)0x01,(short)0x13,(short)0x25};
            //第二步：向仪器发送这个信息，并接收信息
            byte[] outputBytes = wrapper.SendReceive(inputObjects);
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
            ReadCoilStatusModbusProtocal.ReadCoilStatusInputStruct readCoilStatusInputStruct = new ReadCoilStatusModbusProtocal.ReadCoilStatusInputStruct(0x11, "Q20", 0x25);
            //第二步：再生成一个输出结构体，执行相应协议的发送指令，并将输出信息自动转换到输出结构体中
            ReadCoilStatusModbusProtocal.ReadCoilStatusOutputStruct readCoilStatusOutputStruct = (ReadCoilStatusModbusProtocal.ReadCoilStatusOutputStruct)wrapper.SendReceive(wrapper["ReadCoilStatusModbusProtocal"], readCoilStatusInputStruct);
            //第三步：读取这个输出结构体的信息。
            for (int i = 0; i < readCoilStatusOutputStruct.CoilStatus.Length; i++)
            {
                Console.WriteLine(readCoilStatusOutputStruct.CoilStatus[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();

            ReadInputStatusModbusProtocal.ReadInputStatusInputStruct readInputStatusInputStruct = new ReadInputStatusModbusProtocal.ReadInputStatusInputStruct(0x11, "I20", 0x25);
            ReadInputStatusModbusProtocal.ReadInputStatusOutputStruct readInputStatusOutputStruct = (ReadInputStatusModbusProtocal.ReadInputStatusOutputStruct)wrapper.SendReceive(wrapper["ReadInputStatusModbusProtocal"], readInputStatusInputStruct);
            for (int i = 0; i < readInputStatusOutputStruct.InputStatus.Length; i++)
            {
                Console.WriteLine(readInputStatusOutputStruct.InputStatus[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();

            ReadHoldRegisterModbusProtocal.ReadHoldRegisterInputStruct readHoldRegisterInputStruct = new ReadHoldRegisterModbusProtocal.ReadHoldRegisterInputStruct(0x11, "MW1", 8);
            ReadHoldRegisterModbusProtocal.ReadHoldRegisterOutputStruct readHoldRegisterOutputStruct = (ReadHoldRegisterModbusProtocal.ReadHoldRegisterOutputStruct)wrapper.SendReceive(wrapper["ReadHoldRegisterModbusProtocal"], readHoldRegisterInputStruct);
            for (int i = 0; i < readHoldRegisterOutputStruct.HoldRegisterStatus.Length; i++)
            {
                Console.WriteLine(readHoldRegisterOutputStruct.HoldRegisterStatus[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();

            ReadInputRegisterModbusProtocal.ReadInputRegisterInputStruct readInputRegisterInputStruct = new ReadInputRegisterModbusProtocal.ReadInputRegisterInputStruct(0x11, "IW1", 3);
            ReadInputRegisterModbusProtocal.ReadInputRegisterOutputStruct readInputRegisterOutputStruct = (ReadInputRegisterModbusProtocal.ReadInputRegisterOutputStruct)wrapper.SendReceive(wrapper["ReadInputRegisterModbusProtocal"], readInputRegisterInputStruct);
            for (int i = 0; i < readInputRegisterOutputStruct.InputRegisterStatus.Length; i++)
            {
                Console.WriteLine(readInputRegisterOutputStruct.InputRegisterStatus[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();

            ReadInputRegisterModbusProtocal.ReadInputRegisterInputStruct readInputRegisterInputStruct2 = new ReadInputRegisterModbusProtocal.ReadInputRegisterInputStruct(0x11, "E38", 8);
            ReadInputRegisterModbusProtocal.ReadEventOutputStruct readEventOutputStruct = (ReadInputRegisterModbusProtocal.ReadEventOutputStruct)wrapper.SendReceive(wrapper["ReadInputRegisterModbusProtocal"], readInputRegisterInputStruct2);
            Console.WriteLine(readEventOutputStruct.SoeEvent);
            Console.WriteLine(readEventOutputStruct.TestTime);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteOneCoilModbusProtocal.WriteOneCoilInputStruct writeOneCoilInputStruct = new WriteOneCoilModbusProtocal.WriteOneCoilInputStruct(0x11, "Q173", true);
            WriteOneCoilModbusProtocal.WriteOneCoilOutputStruct writeOneCoilOutputStruct = (WriteOneCoilModbusProtocal.WriteOneCoilOutputStruct)wrapper.SendReceive(wrapper["WriteOneCoilModbusProtocal"], writeOneCoilInputStruct);
            Console.WriteLine(writeOneCoilOutputStruct.StartAddress);
            Console.WriteLine(writeOneCoilOutputStruct.WriteValue);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteOneRegisterModbusProtocal.WriteOneRegisterInputStruct writeOneRegisterInputStruct = new WriteOneRegisterModbusProtocal.WriteOneRegisterInputStruct(0x11, "NW1", 100);
            WriteOneRegisterModbusProtocal.WriteOneRegisterOutputStruct writeOneRegisterOutputStruct = (WriteOneRegisterModbusProtocal.WriteOneRegisterOutputStruct)wrapper.SendReceive(wrapper["WriteOneRegisterModbusProtocal"], writeOneRegisterInputStruct);
            Console.WriteLine(writeOneRegisterOutputStruct.StartAddress);
            Console.WriteLine(writeOneRegisterOutputStruct.WriteValue);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteMultiCoilModbusProtocal.WriteMultiCoilInputStruct writeMultiCoilInputStruct = new WriteMultiCoilModbusProtocal.WriteMultiCoilInputStruct(0x11, "Q20", new bool[] { true, false, true, true, false, false, true, true, true, false });
            WriteMultiCoilModbusProtocal.WriteMultiCoilOutputStruct writeMultiCoilOutputStruct = (WriteMultiCoilModbusProtocal.WriteMultiCoilOutputStruct)wrapper.SendReceive(wrapper["WriteMultiCoilModbusProtocal"], writeMultiCoilInputStruct);
            Console.WriteLine(writeMultiCoilOutputStruct.StartAddress);
            Console.WriteLine(writeMultiCoilOutputStruct.WriteCount);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteMultiRegisterModbusProtocal.WriteMultiRegisterInputStruct writeMultiRegisterInputStruct = new WriteMultiRegisterModbusProtocal.WriteMultiRegisterInputStruct(0x11, "MW2", new ushort[] { 0x000A, 0x0102 });
            WriteMultiRegisterModbusProtocal.WriteMultiRegisterOutputStruct writeMultiRegisterOutputStruct = (WriteMultiRegisterModbusProtocal.WriteMultiRegisterOutputStruct)wrapper.SendReceive(wrapper["WriteMultiRegisterModbusProtocal"], writeMultiRegisterInputStruct);
            Console.WriteLine(writeMultiRegisterOutputStruct.StartAddress);
            Console.WriteLine(writeMultiRegisterOutputStruct.WriteCount);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            GetSystemTimeModbusProtocal.GetSystemTimeInputStruct getSystemTimeInputStruct = new GetSystemTimeModbusProtocal.GetSystemTimeInputStruct(0x11);
            GetSystemTimeModbusProtocal.GetSystemTimeOutputStruct getSystemTimeOutputStruct = (GetSystemTimeModbusProtocal.GetSystemTimeOutputStruct)wrapper.SendReceive(wrapper["GetSystemTimeModbusProtocal"], getSystemTimeInputStruct);
            Console.WriteLine(getSystemTimeOutputStruct.Time);
            Console.Read();
            Console.Read();

            SetSystemTimeModbusProtocal.SetSystemTimeInputStruct setSystemTimeInputStruct = new SetSystemTimeModbusProtocal.SetSystemTimeInputStruct(0x11, DateTime.Now);
            SetSystemTimeModbusProtocal.SetSystemTimeOutputStruct setSystemTimeOutputStruct = (SetSystemTimeModbusProtocal.SetSystemTimeOutputStruct)wrapper.SendReceive(wrapper["SetSystemTimeModbusProtocal"], setSystemTimeInputStruct);
            Console.WriteLine(setSystemTimeOutputStruct.StartAddress);
            Console.WriteLine(setSystemTimeOutputStruct.WriteCount);
            Console.Read();
            Console.Read();
        }
    }
}
