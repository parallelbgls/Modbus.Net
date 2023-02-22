using Modbus.Net;
using Modbus.Net.Modbus;

namespace MachineJob.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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

            IMachine<string> machine = new ModbusMachine<string, string>("ModbusMachine1", ModbusType.Tcp, "192.168.0.172", _addresses, true, 1, 2, Endian.BigEndianLsb);
            //IMachine<string> machine2 = new ModbusMachine<string, string>("ModbusMachine2", ModbusType.Tcp, "192.168.0.172", _addresses, true, 3, 2, Endian.BigEndianLsb);

            await MachineJobSchedulerCreator.CreateScheduler("Trigger1", -1, 5).Result.From(machine.Id, machine, MachineDataType.Name).Result.Query(machine.Id + ".ConsoleQuery", QueryConsole).Result.To(machine.Id + ".To", machine).Result.Deal(machine.Id+".Deal", OnSuccess, OnFailure).Result.Run();
            //await MachineJobSchedulerCreator.CreateScheduler("Trigger2", -1, 5).Result.Apply(machine2.Id + ".Apply", null, MachineDataType.Name).Result.Query(machine2.Id + ".ConsoleQuery", QueryConsole2).Result.To(machine2.Id + ".To", machine2).Result.Deal(machine.Id + ".Deal", OnSuccess, OnFailure).Result.From(machine2.Id, machine2, MachineDataType.Name).Result.Query(machine2.Id + ".ConsoleQuery2", QueryConsole).Result.Run();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await MachineJobSchedulerCreator.CancelJob("Trigger1");
        }

        public Task OnSuccess(string machineId)
        {
            Console.WriteLine("Machine {0} set success", machineId);
            return Task.CompletedTask;
        }

        public Task OnFailure(string machineId)
        {
            Console.WriteLine("Machine {0} set failure", machineId);
            return Task.CompletedTask;
        }

        private Dictionary<string, double> QueryConsole(DataReturnDef dataReturnDef)
        {
            var values = dataReturnDef.ReturnValues;
            foreach (var value in values)
            {
                Console.WriteLine(dataReturnDef.MachineId + " " + value.Key + " " + value.Value.DeviceValue);
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

        private Dictionary<string, double> QueryConsole2(DataReturnDef dataReturnDef)
        {
            Random r = new Random();
            var datas = new Dictionary<string, double>()
            {
                {
                    "Test1", r.Next(65536) - 32768
                },
                {
                    "Test2", r.Next(65536) - 32768 
                },
                {
                    "Test3", r.Next(65536) - 32768
                },
                {
                    "Test4", r.Next(65536) - 32768
                },
                {
                    "Test5", r.Next(65536) - 32768
                },
                {
                    "Test6", r.Next(65536) - 32768
                },
                {
                    "Test7", r.Next(65536) - 32768
                },
                {
                    "Test8", r.Next(65536) - 32768
                },
                {
                    "Test9", r.Next(65536) - 32768
                },
                {
                    "Test10", r.Next(65536) - 32768
                }
            };
            return datas;
        }

    }
}