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
            /*
            string ip = "192.168.4.247";
            try
            {
                BaseProtocal wrapper = new ModbusTcpProtocal(ip);
                Console.WriteLine("link ip is {0}",ip);
                //第一步：先生成一个输入信息的object数组
                object[] inputObjects = new object[] {(byte) 0x02, (byte) 0x01, (short) 0x13, (short) 0x25};
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


            /*string ip = "192.168.3.246";
            try
            {
                Console.WriteLine("link ip is {0}",ip);
                short ConNr = 63; // First connection；(0  63)；(max. 64 connections).
                string AccessPoint = "S7ONLINE"; // Default access point——S7ONLINE                  
                Prodave6.CON_TABLE_TYPE ConTable; // Connection table
                int ConTableLen = System.Runtime.InteropServices.Marshal.SizeOf(typeof (Prodave6.CON_TABLE_TYPE));
                // Length of the connection table
                int RetValue;
                string[] splitip = ip.Split('.');
                ConTable.Adr = new byte[] {byte.Parse(splitip[0]), byte.Parse(splitip[1]), byte.Parse(splitip[2]), byte.Parse(splitip[3]), 0, 0};
                ConTable.AdrType = 2; // Type of address: MPI/PB (1), IP (2), MAC (3)
                ConTable.SlotNr = 2; // 插槽号
                ConTable.RackNr = 0; // 机架号  
                RetValue = Prodave6.LoadConnection_ex6(ConNr, AccessPoint, ConTableLen, ref ConTable);
                if (RetValue > 0) throw new Exception();
                //以下测试SetActiveConnection_ex6
                UInt16 UConNr = (UInt16) ConNr;
                RetValue = Prodave6.SetActiveConnection_ex6(UConNr);
                if (RetValue > 0) throw new Exception();
                Console.WriteLine("prodave link success");
            }
            catch (Exception)
            {
                Console.WriteLine("prodave link failed");
            }
            Console.Read();
            */


            //先初始化一个协议转换器，这里构造Modbus/Tcp协议。
            //BaseProtocal wrapper = new ModbusTcpProtocal();

            /*
            //调用方法一：手动构造
            //第一步：先生成一个输入信息的object数组
            object[] inputObjects = new object[]{(byte)0x02,(byte)0x01,(short)0x13,(short)0x25};
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
            */

            //先初始化一个协议转换器，这里构造Modbus/Rtu协议。
            BaseProtocal wrapper = new ModbusRtuProtocal();

            /*
            //调用方法一：手动构造
            //第一步：先生成一个输入信息的object数组
            object[] inputObjects = new object[]{(byte)0x02,(byte)0x01,(short)0x00,(short)0x03};
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

            /*
            //调用方法二：自动构造
            //第一步：先生成一个输入结构体，然后向这个结构体中填写数据
            ReadCoilStatusModbusProtocal.ReadCoilStatusInputStruct readCoilStatusInputStruct = new ReadCoilStatusModbusProtocal.ReadCoilStatusInputStruct(0x02, "Q20", 0x25);
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

            ReadInputStatusModbusProtocal.ReadInputStatusInputStruct readInputStatusInputStruct = new ReadInputStatusModbusProtocal.ReadInputStatusInputStruct(0x02, "I20", 0x25);
            ReadInputStatusModbusProtocal.ReadInputStatusOutputStruct readInputStatusOutputStruct = (ReadInputStatusModbusProtocal.ReadInputStatusOutputStruct)wrapper.SendReceive(wrapper["ReadInputStatusModbusProtocal"], readInputStatusInputStruct);
            for (int i = 0; i < readInputStatusOutputStruct.InputStatus.Length; i++)
            {
                Console.WriteLine(readInputStatusOutputStruct.InputStatus[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();

            ReadHoldRegisterModbusProtocal.ReadHoldRegisterInputStruct readHoldRegisterInputStruct = new ReadHoldRegisterModbusProtocal.ReadHoldRegisterInputStruct(0x02, "0", 8);
            ReadHoldRegisterModbusProtocal.ReadHoldRegisterOutputStruct readHoldRegisterOutputStruct = (ReadHoldRegisterModbusProtocal.ReadHoldRegisterOutputStruct)wrapper.SendReceive(wrapper["ReadHoldRegisterModbusProtocal"], readHoldRegisterInputStruct);
            for (int i = 0; i < readHoldRegisterOutputStruct.HoldRegisterStatus.Length; i++)
            {
                Console.WriteLine(readHoldRegisterOutputStruct.HoldRegisterStatus[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();

            ReadInputRegisterModbusProtocal.ReadInputRegisterInputStruct readInputRegisterInputStruct = new ReadInputRegisterModbusProtocal.ReadInputRegisterInputStruct(0x02, "IW1", 3);
            ReadInputRegisterModbusProtocal.ReadInputRegisterOutputStruct readInputRegisterOutputStruct = (ReadInputRegisterModbusProtocal.ReadInputRegisterOutputStruct)wrapper.SendReceive(wrapper["ReadInputRegisterModbusProtocal"], readInputRegisterInputStruct);
            for (int i = 0; i < readInputRegisterOutputStruct.InputRegisterStatus.Length; i++)
            {
                Console.WriteLine(readInputRegisterOutputStruct.InputRegisterStatus[i]);
            }
            Console.WriteLine();
            Console.Read();
            Console.Read();

            ReadInputRegisterModbusProtocal.ReadInputRegisterInputStruct readInputRegisterInputStruct2 = new ReadInputRegisterModbusProtocal.ReadInputRegisterInputStruct(0x02, "E38", 8);
            ReadInputRegisterModbusProtocal.ReadEventOutputStruct readEventOutputStruct = (ReadInputRegisterModbusProtocal.ReadEventOutputStruct)wrapper.SendReceive(wrapper["ReadInputRegisterModbusProtocal"], readInputRegisterInputStruct2);
            Console.WriteLine(readEventOutputStruct.SoeEvent);
            Console.WriteLine(readEventOutputStruct.TestTime);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteOneCoilModbusProtocal.WriteOneCoilInputStruct writeOneCoilInputStruct = new WriteOneCoilModbusProtocal.WriteOneCoilInputStruct(0x02, "Q173", true);
            WriteOneCoilModbusProtocal.WriteOneCoilOutputStruct writeOneCoilOutputStruct = (WriteOneCoilModbusProtocal.WriteOneCoilOutputStruct)wrapper.SendReceive(wrapper["WriteOneCoilModbusProtocal"], writeOneCoilInputStruct);
            Console.WriteLine(writeOneCoilOutputStruct.StartAddress);
            Console.WriteLine(writeOneCoilOutputStruct.WriteValue);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteOneRegisterModbusProtocal.WriteOneRegisterInputStruct writeOneRegisterInputStruct = new WriteOneRegisterModbusProtocal.WriteOneRegisterInputStruct(0x02, "NW1", 100);
            WriteOneRegisterModbusProtocal.WriteOneRegisterOutputStruct writeOneRegisterOutputStruct = (WriteOneRegisterModbusProtocal.WriteOneRegisterOutputStruct)wrapper.SendReceive(wrapper["WriteOneRegisterModbusProtocal"], writeOneRegisterInputStruct);
            Console.WriteLine(writeOneRegisterOutputStruct.StartAddress);
            Console.WriteLine(writeOneRegisterOutputStruct.WriteValue);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteMultiCoilModbusProtocal.WriteMultiCoilInputStruct writeMultiCoilInputStruct = new WriteMultiCoilModbusProtocal.WriteMultiCoilInputStruct(0x02, "Q20", new bool[] { true, false, true, true, false, false, true, true, true, false });
            WriteMultiCoilModbusProtocal.WriteMultiCoilOutputStruct writeMultiCoilOutputStruct = (WriteMultiCoilModbusProtocal.WriteMultiCoilOutputStruct)wrapper.SendReceive(wrapper["WriteMultiCoilModbusProtocal"], writeMultiCoilInputStruct);
            Console.WriteLine(writeMultiCoilOutputStruct.StartAddress);
            Console.WriteLine(writeMultiCoilOutputStruct.WriteCount);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            WriteMultiRegisterModbusProtocal.WriteMultiRegisterInputStruct writeMultiRegisterInputStruct = new WriteMultiRegisterModbusProtocal.WriteMultiRegisterInputStruct(0x02, "MW2", new ushort[] { 0x000A, 0x0102 });
            WriteMultiRegisterModbusProtocal.WriteMultiRegisterOutputStruct writeMultiRegisterOutputStruct = (WriteMultiRegisterModbusProtocal.WriteMultiRegisterOutputStruct)wrapper.SendReceive(wrapper["WriteMultiRegisterModbusProtocal"], writeMultiRegisterInputStruct);
            Console.WriteLine(writeMultiRegisterOutputStruct.StartAddress);
            Console.WriteLine(writeMultiRegisterOutputStruct.WriteCount);
            Console.WriteLine();
            Console.Read();
            Console.Read();

            GetSystemTimeModbusProtocal.GetSystemTimeInputStruct getSystemTimeInputStruct = new GetSystemTimeModbusProtocal.GetSystemTimeInputStruct(0x02);
            GetSystemTimeModbusProtocal.GetSystemTimeOutputStruct getSystemTimeOutputStruct = (GetSystemTimeModbusProtocal.GetSystemTimeOutputStruct)wrapper.SendReceive(wrapper["GetSystemTimeModbusProtocal"], getSystemTimeInputStruct);
            Console.WriteLine(getSystemTimeOutputStruct.Time);
            Console.Read();
            Console.Read();

            SetSystemTimeModbusProtocal.SetSystemTimeInputStruct setSystemTimeInputStruct = new SetSystemTimeModbusProtocal.SetSystemTimeInputStruct(0x02, DateTime.Now);
            SetSystemTimeModbusProtocal.SetSystemTimeOutputStruct setSystemTimeOutputStruct = (SetSystemTimeModbusProtocal.SetSystemTimeOutputStruct)wrapper.SendReceive(wrapper["SetSystemTimeModbusProtocal"], setSystemTimeInputStruct);
            Console.WriteLine(setSystemTimeOutputStruct.StartAddress);
            Console.WriteLine(setSystemTimeOutputStruct.WriteCount);
            Console.Read();
            Console.Read();*/

            /*short ConNr = 63; // First connection；(0  63)；(max. 64 connections).
            string AccessPoint = "S7ONLINE"; // Default access point——S7ONLINE                  
            Prodave6.CON_TABLE_TYPE ConTable;// Connection table
            int ConTableLen = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Prodave6.CON_TABLE_TYPE));// Length of the connection table
            int RetValue;
            //ConTable.Adr = new byte[] { 115, 238, 36, 68, 0, 0 };
            ConTable.Adr = new byte[] { 192, 168, 3, 246, 0, 0 };
            ConTable.AdrType = 2; // Type of address: MPI/PB (1), IP (2), MAC (3)
            ConTable.SlotNr = 2; // 插槽号
            ConTable.RackNr = 0; // 机架号  
            RetValue = Prodave6.LoadConnection_ex6(ConNr, AccessPoint, ConTableLen, ref ConTable);

            //以下测试SetActiveConnection_ex6
            UInt16 UConNr = (UInt16)ConNr;
            RetValue = Prodave6.SetActiveConnection_ex6(UConNr);

            //以下测试db_write_ex6
            UInt16 BlkNr = 1;//data block号
            Prodave6.DatType DType = Prodave6.DatType.BYTE;//要读取的数据类型
            UInt16 StartNr = 0;//起始地址号
            UInt32 pAmount = 20;//需要读取类型的数量
            UInt32 BufLen = 200;//缓冲区长度（字节为单位）
            //参数：data block号、要写入的数据类型、起始地址号、需要写入类型的数量、缓冲区长度（字节为单位）、缓冲区
            byte[] pWriteBuffer = new byte[5];
            for (int i = 0; i < pWriteBuffer.Length; i++)
                pWriteBuffer[i] = (byte)(1);
            //RetValue = Prodave6.db_write_ex6(BlkNr, DType, StartNr, ref pAmount, BufLen, pWriteBuffer);            

            //以下测试db_read_ex6
            //参数：data block号、要读取的数据类型、起始地址号、需要读取类型的数量、缓冲区长度（字节为单位）、
            //缓冲区、缓冲区数据交互的长度
            byte[] pReadBuffer = new byte[200];
            UInt32 pDatLen = 0;
            //RetValue = Prodave6.as200_field_read_ex6(Prodave6.FieldType.V, BlkNr, StartNr, pAmount, BufLen, pReadBuffer,
            //    ref pDatLen);
            RetValue = Prodave6.db_read_ex6(BlkNr, DType, StartNr, ref pAmount, BufLen, pReadBuffer, ref pDatLen);
            //pBuffer = new byte[300];
            //RetValue = Prodave6.GetErrorMessage_ex6(RetValue, 300, pBuffer);
            //s = Encoding.ASCII.GetString(pBuffer);
            //以下测试field_read_ex6（测试DB区）
            //参数：data block号、要读取的数据类型、起始地址号、需要读取类型的数量、缓冲区长度（字节为单位）、
            //缓冲区、缓冲区数据交互的长度
            Prodave6.FieldType FType = Prodave6.FieldType.E;
            for (int i = 0; i < pWriteBuffer.Length; i++)
                pWriteBuffer[i] = (byte)(1);

            RetValue = Prodave6.field_read_ex6(FType, BlkNr, StartNr, pAmount, BufLen, pReadBuffer, ref pDatLen);

            RetValue = Prodave6.field_write_ex6(FType, BlkNr, StartNr, pAmount, BufLen, pWriteBuffer);
             
            RetValue = Prodave6.field_read_ex6(FType, BlkNr, StartNr, pAmount, BufLen, pReadBuffer, ref pDatLen);*/

        }
    }
}
