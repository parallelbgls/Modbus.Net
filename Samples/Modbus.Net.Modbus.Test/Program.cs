// See https://aka.ms/new-console-template for more information
using Modbus.Net;
using Modbus.Net.Modbus;
using Modbus.Net.Modbus.Test;

List<AddressUnit> _addresses = new List<AddressUnit>
{
    new AddressUnit() { Area = "4X", Address = 1, DataType = typeof(short), Id = "1", Name = "Test1" },
    new AddressUnit() { Area = "4X", Address = 2, DataType = typeof(short), Id = "2", Name = "Test2" },
    new AddressUnit() { Area = "4X", Address = 3, DataType = typeof(short), Id = "3", Name = "Test3" },
    new AddressUnit() { Area = "4X", Address = 4, DataType = typeof(short), Id = "4", Name = "Test4" },
    new AddressUnit() { Area = "4X", Address = 5, DataType = typeof(short), Id = "5", Name = "Test5" },
    new AddressUnit() { Area = "4X", Address = 6, DataType = typeof(short), Id = "6", Name = "Test6" },
    new AddressUnit() { Area = "4X", Address = 7, DataType = typeof(short), Id = "7", Name = "Test7" },
    new AddressUnit() { Area = "4X", Address = 8, DataType = typeof(short), Id = "8", Name = "Test8" },
    new AddressUnit() { Area = "4X", Address = 9, DataType = typeof(short), Id = "9", Name = "Test9" },
    new AddressUnit() { Area = "4X", Address = 10, DataType = typeof(short), Id = "10", Name = "Test10" }
};

IMachine<string> machine = new ModbusMachine<string, string>("ModbusMachine1", ModbusType.Tcp, "192.168.0.172:502", _addresses, true, 1, 2, Endian.BigEndianLsb);

await MachineJobSchedulerCreator.CreateScheduler("Trigger1", -1, 5).Result.From(machine.Id, machine, MachineDataType.Name).Result.Query("ConsoleQuery", QueryConsole).Result.To(machine.Id + ".To", machine).Result.Run();

Console.ReadLine();

Dictionary<string, ReturnUnit> QueryConsole(Dictionary<string, ReturnUnit> values)
{
    foreach (var value in values)
    {
        Console.WriteLine(value.Key + " " + value.Value.DeviceValue);
    }

    using (var context = new DatabaseWriteContext())
    {
        context.DatabaseWrites.Add(new DatabaseWriteEntity
        {
            Value1 = values["Test1"].DeviceValue,
            Value2 = values["Test2"].DeviceValue,
            Value3 = values["Test3"].DeviceValue,
            Value4 = values["Test4"].DeviceValue,
            Value5 = values["Test5"].DeviceValue,
            Value6 = values["Test6"].DeviceValue,
            Value7 = values["Test7"].DeviceValue,
            Value8 = values["Test8"].DeviceValue,
            Value9 = values["Test9"].DeviceValue,
            Value10 = values["Test10"].DeviceValue,
            UpdateTime = DateTime.Now,
        });
        context.SaveChanges();
    }

    foreach (var value in values)
    {
        value.Value.DeviceValue = new Random().Next(65536) - 32768;
    }

    return values;
}


