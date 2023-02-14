using System;
using System.Collections.Generic;
using Modbus.Net;
using Modbus.Net.Modbus;
using Serilog;


Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Console().CreateLogger();

ModbusMachine<int, string> machine = new ModbusMachine<int, string>(1, ModbusType.Rtu, "COM1",
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
ModbusMachine<int, string> machine2 = new ModbusMachine<int, string>(2, ModbusType.Rtu, "COM1",
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
ModbusMachine<int, string> machine3 = new ModbusMachine<int, string>(3, ModbusType.Rtu, "COM1",
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
Random r = new Random();
await MachineJobSchedulerCreator.CreateScheduler("Trigger1", -1, 10).Result.Apply(machine.Id+".Apply", new Dictionary<string, ReturnUnit>() {{
        "4X 1.0", new ReturnUnit(){DeviceValue = r.Next() % 65536 }
    },
    {
        "4X 2.0",  new ReturnUnit(){DeviceValue = r.Next() % 65536 }
    },
    {
        "4X 3.0",  new ReturnUnit(){DeviceValue = r.Next() % 65536 }
    } 
}, MachineDataType.Address).Result.Query().Result.To(machine.Id + ".To", machine).Result.Run();
await MachineJobSchedulerCreator.CreateScheduler("Trigger2", -1, 10).Result.Apply(machine2.Id + ".Apply", new Dictionary<string, ReturnUnit>() {{
        "4X 1.0", new ReturnUnit(){DeviceValue = r.Next() % 65536 }
    },
    {
        "4X 2.0",  new ReturnUnit(){DeviceValue = r.Next() % 65536 }
    },
    {
        "4X 3.0",  new ReturnUnit(){DeviceValue = r.Next() % 65536 }
    }
}, MachineDataType.Address).Result.Query().Result.To(machine2.Id + ".To", machine2).Result.Run();
await MachineJobSchedulerCreator.CreateScheduler("Trigger3", -1, 10).Result.Apply(machine3.Id + ".Apply", new Dictionary<string, ReturnUnit>() {{
        "4X 1.0", new ReturnUnit(){DeviceValue = r.Next() % 65536 }
    },
    {
        "4X 2.0",  new ReturnUnit(){DeviceValue = r.Next() % 65536 }
    },
    {
        "4X 3.0",  new ReturnUnit(){DeviceValue = r.Next() % 65536 }
    }
}, MachineDataType.Address).Result.Query().Result.To(machine3.Id + ".To", machine3).Result.Run();
Console.ReadLine();