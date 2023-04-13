using Modbus.Net;
using DataReturnDef = Modbus.Net.DataReturnDef<string, double>;
using MultipleMachinesJobScheduler = Modbus.Net.MultipleMachinesJobScheduler<Modbus.Net.IMachineMethodDatas, string, double>;

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
            var machines = MachineReader.ReadMachines();
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