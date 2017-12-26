using System;
using System.Collections.Generic;
using System.Threading;
using Modbus.Net.Modbus;
using Serilog;

namespace Modbus.Net.PersistedTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Console().CreateLogger();

            IMachineProperty<int> machine = new ModbusMachine<int, string>(1, ModbusType.Rtu, "COM1",
                new List<AddressUnit>()
                {
                    new AddressUnit()
                    {
                        Id = "1",
                        Area = "4X",
                        Address = 1,
                        Name = "test 1",
                        DataType = typeof(ushort)
                    },
                    new AddressUnit()
                    {
                        Id = "2",
                        Area = "4X",
                        Address = 2,
                        Name = "test 2",
                        DataType = typeof(ushort)
                    },
                    new AddressUnit()
                    {
                        Id = "3",
                        Area = "4X",
                        Address = 3,
                        Name = "test 3",
                        DataType = typeof(ushort)
                    },
                }, true, 2, 1);
            IMachineProperty<int> machine2 = new ModbusMachine<int, string>(2, ModbusType.Rtu, "COM1",
                new List<AddressUnit>()
                {
                    new AddressUnit()
                    {
                        Id = "1",
                        Area = "4X",
                        Address = 11,
                        Name = "test 1",
                        DataType = typeof(ushort)
                    },
                    new AddressUnit()
                    {
                        Id = "2",
                        Area = "4X",
                        Address = 12,
                        Name = "test 2",
                        DataType = typeof(ushort)
                    },
                    new AddressUnit()
                    {
                        Id = "3",
                        Area = "4X",
                        Address = 13,
                        Name = "test 3",
                        DataType = typeof(ushort)
                    },
                }, true, 3, 1);
            IMachineProperty<int> machine3 = new ModbusMachine<int, string>(3, ModbusType.Rtu, "COM1",
                new List<AddressUnit>()
                {
                    new AddressUnit()
                    {
                        Id = "1",
                        Area = "4X",
                        Address = 21,
                        Name = "test 1",
                        DataType = typeof(ushort)
                    },
                    new AddressUnit()
                    {
                        Id = "2",
                        Area = "4X",
                        Address = 22,
                        Name = "test 2",
                        DataType = typeof(ushort)
                    },
                    new AddressUnit()
                    {
                        Id = "3",
                        Area = "4X",
                        Address = 23,
                        Name = "test 3",
                        DataType = typeof(ushort)
                    },
                }, true, 4, 1);

            TaskManager<int> manager = new TaskManager<int>(20, true);
            manager.AddMachines<string>(new List<IMachineProperty<int>>{machine, machine2, machine3});
            Random r = new Random();
            manager.InvokeTimerForMachine(1, new TaskItemSetData(() => new Dictionary<string, double>
            {
                {
                    "4X 1.0", r.Next() % 65536
                },
                {
                    "4X 2.0", r.Next() % 65536
                },
                {
                    "4X 3.0", r.Next() % 65536
                },
            }, MachineSetDataType.Address, 10000, 10000));
            manager.InvokeTimerForMachine(2, new TaskItemSetData(() => new Dictionary<string, double>
            {
                {
                    "4X 11.0", r.Next() % 65536
                },
                {
                    "4X 12.0", r.Next() % 65536
                },
                {
                    "4X 13.0", r.Next() % 65536
                },
            }, MachineSetDataType.Address, 10000, 10000));
            manager.InvokeTimerForMachine(3, new TaskItemSetData(() => new Dictionary<string, double>
            {
                {
                    "4X 21.0", r.Next() % 65536
                },
                {
                    "4X 22.0", r.Next() % 65536
                },
                {
                    "4X 23.0", r.Next() % 65536
                },
            }, MachineSetDataType.Address, 10000, 10000));           
            Thread.Sleep(5000);
            manager.InvokeTimerAll(new TaskItemGetData(data =>
            {
                foreach (var dataInner in data.ReturnValues)
                {
                    Console.WriteLine(dataInner.Key + " " + dataInner.Value.PlcValue);
                }
               
            }, MachineGetDataType.Address, 10000, 10000));

            Console.Read();
            Console.Read();
        }
    }
}
