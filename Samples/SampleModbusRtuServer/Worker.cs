using Modbus.Net.Modbus;
using MultipleMachinesJobScheduler = Modbus.Net.MultipleMachinesJobScheduler<Modbus.Net.IMachineMethodDatas, string, double>;

namespace SampleModbusRtuServer.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private byte[] threex = new byte[19998];

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ModbusRtuProtocolReceiver receiver = new ModbusRtuProtocolReceiver("COM2", 1);
            receiver.DataProcess = receiveContent =>
            {
                byte[] returnBytes = null;
                var readContent = new byte[receiveContent.Count * 2];
                var values = receiveContent.WriteContent;
                if (values != null)
                {
                    try
                    {
                        /*using (var context = new DatabaseWriteContext())
                        {
                            context.DatabaseWrites?.Add(new DatabaseWriteEntity
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
                        }*/
                        switch (receiveContent.FunctionCode)
                        {
                            case (byte)ModbusProtocolFunctionCode.WriteMultiRegister:
                                {
                                    Array.Copy(receiveContent.WriteContent, 0, threex, receiveContent.StartAddress * 2, receiveContent.Count);
                                    returnBytes = new WriteDataModbusProtocol().Format(receiveContent.SlaveAddress, receiveContent.FunctionCode, receiveContent.StartAddress, receiveContent.Count);
                                    break;
                                }
                        }
                    }
                    catch
                    {
                        //ignore
                    }
                }
                else
                {
                    switch (receiveContent.FunctionCode)
                    {
                        case (byte)ModbusProtocolFunctionCode.ReadHoldRegister:
                            {
                                Array.Copy(threex, receiveContent.StartAddress, readContent, 0, readContent.Length);
                                returnBytes = new ReadDataModbusProtocol().Format(receiveContent.SlaveAddress, receiveContent.FunctionCode, (byte)receiveContent.Count, readContent);
                                break;
                            }
                    }
                }
                if (returnBytes != null) return returnBytes;
                else return null;
            };
            await receiver.ConnectAsync();
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => MultipleMachinesJobScheduler.CancelJob());
        }
    }
}