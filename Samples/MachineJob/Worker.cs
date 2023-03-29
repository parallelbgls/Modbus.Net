using Modbus.Net;
using Modbus.Net.Modbus;
using Modbus.Net.Siemens;

namespace MachineJob.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

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

            List<AddressUnit> _addresses2 = new List<AddressUnit>
            {
                new AddressUnit() { Area = "DB1", Address = 0, DataType = typeof(short), Id = "1", Name = "Test1" },
                new AddressUnit() { Area = "DB1", Address = 2, DataType = typeof(short), Id = "2", Name = "Test2" },
                new AddressUnit() { Area = "DB1", Address = 4, DataType = typeof(short), Id = "3", Name = "Test3" },
                new AddressUnit() { Area = "DB1", Address = 6, DataType = typeof(short), Id = "4", Name = "Test4" },
                new AddressUnit() { Area = "DB1", Address = 8, DataType = typeof(short), Id = "5", Name = "Test5" },
                new AddressUnit() { Area = "DB1", Address = 10, DataType = typeof(short), Id = "6", Name = "Test6" },
                new AddressUnit() { Area = "DB1", Address = 12, DataType = typeof(short), Id = "7", Name = "Test7" },
                new AddressUnit() { Area = "DB1", Address = 14, DataType = typeof(short), Id = "8", Name = "Test8" },
                new AddressUnit() { Area = "DB1", Address = 16, DataType = typeof(short), Id = "9", Name = "Test9" },
                new AddressUnit() { Area = "DB1", Address = 18, DataType = typeof(short), Id = "10", Name = "Test10" }
            };

            IMachine<string> machine = new ModbusMachine("ModbusMachine1", ModbusType.Tcp, null, _addresses, true, 1, 2, Endian.BigEndianLsb);
            IMachine<string> machine2 = new SiemensMachine("SiemensMachine1", SiemensType.Tcp, null, SiemensMachineModel.S7_1200, _addresses2, true, 1, 2);
            IMachine<string> machine3 = new ModbusMachine("ModbusMachine2", ModbusType.Rtu, "COM1", _addresses, true, 3, 2);
            var machines = new List<IMachine<string>>() { machine, machine2, machine3 };
            return Task.Run(() => MultipleMachinesJobScheduler.RunScheduler(machines, async (machine, scheduler) =>
            {
                await scheduler.From(machine.Id, machine, MachineDataType.Name).Result.Query(machine.Id + ".ConsoleQuery", QueryConsole).Result.To(machine.Id + ".To", machine).Result.Deal(machine.Id + ".Deal", OnSuccess, OnFailure).Result.Run();
            }, -1, 10));
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => MultipleMachinesJobScheduler.CancelJob());
        }

        public Task OnSuccess(string machineId)
        {
            _logger.LogInformation("Machine {0} set success", machineId);
            return Task.CompletedTask;
        }

        public Task OnFailure(string machineId, int errorCode, string errorMsg)
        {
            _logger.LogError("Machine {0} set failure: {1}", machineId, errorMsg);
            return Task.CompletedTask;
        }

        private Dictionary<string, double>? QueryConsole(DataReturnDef dataReturnDef)
        {
            var values = dataReturnDef.ReturnValues.Datas;
            if (dataReturnDef.ReturnValues.IsSuccess)
            {
                foreach (var value in values)
                {
                    _logger.LogInformation(dataReturnDef.MachineId + " " + value.Key + " " + value.Value.DeviceValue);
                }

                try
                {
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
                }
                catch
                {
                    //ignore
                }

                Random r = new Random();
                foreach (var value in values)
                {
                    value.Value.DeviceValue = r.Next(65536) - 32768;
                }

                return values.MapGetValuesToSetValues();
            }
            return null;
        }
    }
}