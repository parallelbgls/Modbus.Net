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
            A a= new A();
            a.Do();
        }
    }

    public class A
    {
        public void Do()
        {
            BaseProtocal wrapper = new ModbusTCPProtocal();

            ReadCoilStatusTCPProtocal.ReadCoilStatusInputStruct readCoilStatusInputStruct = new ReadCoilStatusTCPProtocal.ReadCoilStatusInputStruct(0x11, "Q20", 0x25);
            ReadCoilStatusTCPProtocal.ReadCoilStatusOutputStruct readCoilStatusOutputStruct = (ReadCoilStatusTCPProtocal.ReadCoilStatusOutputStruct)wrapper.SendReceive(wrapper["ReadCoilStatusTCPProtocal"],readCoilStatusInputStruct);
            for (int i = 0; i < readCoilStatusOutputStruct.CoilStatus.Length; i++)
            {
                Console.WriteLine(readCoilStatusOutputStruct.CoilStatus[i]);
            }
            Console.WriteLine();
            Console.Read();

            ReadInputStatusTCPProtocal.ReadInputStatusInputStruct readInputStatusInputStruct = new ReadInputStatusTCPProtocal.ReadInputStatusInputStruct(0x11, "I20", 0x25);
            ReadInputStatusTCPProtocal.ReadInputStatusOutputStruct readInputStatusOutputStruct = (ReadInputStatusTCPProtocal.ReadInputStatusOutputStruct)wrapper.SendReceive(wrapper["ReadInputStatusTCPProtocal"],readInputStatusInputStruct);
            for (int i = 0; i < readInputStatusOutputStruct.InputStatus.Length; i++)
            {
                Console.WriteLine(readInputStatusOutputStruct.InputStatus[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();

            ReadHoldRegisterTCPProtocal.ReadHoldRegisterInputStruct readHoldRegisterInputStruct = new ReadHoldRegisterTCPProtocal.ReadHoldRegisterInputStruct(0x11, "MW1", 8);
            ReadHoldRegisterTCPProtocal.ReadHoldRegisterOutputStruct readHoldRegisterOutputStruct = (ReadHoldRegisterTCPProtocal.ReadHoldRegisterOutputStruct)wrapper.SendReceive(wrapper["ReadHoldRegisterTCPProtocal"],readHoldRegisterInputStruct);
            for (int i = 0; i < readHoldRegisterOutputStruct.HoldRegisterStatus.Length; i++)
            {
                Console.WriteLine(readHoldRegisterOutputStruct.HoldRegisterStatus[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();

            ReadInputRegisterTCPProtocal.ReadInputRegisterInputStruct readInputRegisterInputStruct = new ReadInputRegisterTCPProtocal.ReadInputRegisterInputStruct(0x11, "IW1", 3);
            ReadInputRegisterTCPProtocal.ReadInputRegisterOutputStruct readInputRegisterOutputStruct = (ReadInputRegisterTCPProtocal.ReadInputRegisterOutputStruct)wrapper.SendReceive(wrapper["ReadInputRegisterTCPProtocal"], readInputRegisterInputStruct);
            for (int i = 0; i < readInputRegisterOutputStruct.InputRegisterStatus.Length; i++)
            {
                Console.WriteLine(readInputRegisterOutputStruct.InputRegisterStatus[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();

            ReadInputRegisterTCPProtocal.ReadInputRegisterInputStruct readInputRegisterInputStruct2 = new ReadInputRegisterTCPProtocal.ReadInputRegisterInputStruct(0x11, "E38", 8);
            ReadInputRegisterTCPProtocal.ReadEventOutputStruct readEventOutputStruct = (ReadInputRegisterTCPProtocal.ReadEventOutputStruct)wrapper.SendReceive(wrapper["ReadInputRegisterTCPProtocal"], readInputRegisterInputStruct2);
            Console.WriteLine(readEventOutputStruct.SoeEvent);
            Console.WriteLine(readEventOutputStruct.TestTime);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteOneCoilTCPProtocal.WriteOneCoilInputStruct writeOneCoilInputStruct = new WriteOneCoilTCPProtocal.WriteOneCoilInputStruct(0x11, "Q173", true);
            WriteOneCoilTCPProtocal.WriteOneCoilOutputStruct writeOneCoilOutputStruct = (WriteOneCoilTCPProtocal.WriteOneCoilOutputStruct)wrapper.SendReceive(wrapper["WriteOneCoilTCPProtocal"], writeOneCoilInputStruct);
            Console.WriteLine(writeOneCoilOutputStruct.StartAddress);
            Console.WriteLine(writeOneCoilOutputStruct.WriteValue);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteOneRegisterTCPProtocal.WriteOneRegisterInputStruct writeOneRegisterInputStruct = new WriteOneRegisterTCPProtocal.WriteOneRegisterInputStruct(0x11, "NW1", 100);
            WriteOneRegisterTCPProtocal.WriteOneRegisterOutputStruct writeOneRegisterOutputStruct = (WriteOneRegisterTCPProtocal.WriteOneRegisterOutputStruct)wrapper.SendReceive(wrapper["WriteOneRegisterTCPProtocal"], writeOneRegisterInputStruct);
            Console.WriteLine(writeOneRegisterOutputStruct.StartAddress);
            Console.WriteLine(writeOneRegisterOutputStruct.WriteValue);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteMultiCoilTCPProtocal.WriteMultiCoilInputStruct writeMultiCoilInputStruct = new WriteMultiCoilTCPProtocal.WriteMultiCoilInputStruct(0x11, "Q20", new bool[]{true, false, true, true, false, false, true, true, true, false});
            WriteMultiCoilTCPProtocal.WriteMultiCoilOutputStruct writeMultiCoilOutputStruct = (WriteMultiCoilTCPProtocal.WriteMultiCoilOutputStruct)wrapper.SendReceive(wrapper["WriteMultiCoilTCPProtocal"], writeMultiCoilInputStruct);
            Console.WriteLine(writeMultiCoilOutputStruct.StartAddress);
            Console.WriteLine(writeMultiCoilOutputStruct.WriteCount);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteMultiRegisterTCPProtocal.WriteMultiRegisterInputStruct writeMultiRegisterInputStruct = new WriteMultiRegisterTCPProtocal.WriteMultiRegisterInputStruct(0x11, "MW2", new ushort[]{0x000A,0x0102});
            WriteMultiRegisterTCPProtocal.WriteMultiRegisterOutputStruct writeMultiRegisterOutputStruct = (WriteMultiRegisterTCPProtocal.WriteMultiRegisterOutputStruct)wrapper.SendReceive(wrapper["WriteMultiRegisterTCPProtocal"], writeMultiRegisterInputStruct);
            Console.WriteLine(writeMultiRegisterOutputStruct.StartAddress);
            Console.WriteLine(writeMultiRegisterOutputStruct.WriteCount);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            GetSystemTimeTCPProtocal.GetSystemTimeInputStruct getSystemTimeInputStruct = new GetSystemTimeTCPProtocal.GetSystemTimeInputStruct(0x11);
            GetSystemTimeTCPProtocal.GetSystemTimeOutputStruct getSystemTimeOutputStruct = (GetSystemTimeTCPProtocal.GetSystemTimeOutputStruct)wrapper.SendReceive(wrapper["GetSystemTimeTCPProtocal"], getSystemTimeInputStruct);
            Console.WriteLine(getSystemTimeOutputStruct.Time);
            Console.Read();
            Console.Read();

            SetSystemTimeTCPProtocal.SetSystemTimeInputStruct setSystemTimeInputStruct = new SetSystemTimeTCPProtocal.SetSystemTimeInputStruct(0x11, DateTime.Now);
            SetSystemTimeTCPProtocal.SetSystemTimeOutputStruct setSystemTimeOutputStruct = (SetSystemTimeTCPProtocal.SetSystemTimeOutputStruct)wrapper.SendReceive(wrapper["SetSystemTimeTCPProtocal"], setSystemTimeInputStruct);
            Console.WriteLine(setSystemTimeOutputStruct.StartAddress);
            Console.WriteLine(setSystemTimeOutputStruct.WriteCount);
            Console.Read();
            Console.Read();
        }
    }
}
