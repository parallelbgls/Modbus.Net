using Modbus.Net;
using Modbus.Net.Modbus;
using MultipleMachinesJobScheduler = Modbus.Net.MultipleMachinesJobScheduler<Modbus.Net.IMachineMethodDatas, string, double>;

namespace SampleModbusRtuServer.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private bool[] zerox = new bool[10000];
        private byte[] threex = new byte[20000];

        private bool _isUpdate = false;

        private DateTime _updateTime = DateTime.MinValue;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ModbusRtuProtocolReceiver receiver = new ModbusRtuProtocolReceiver("COM2", 1);
            receiver.DataProcess = receiveContent =>
            {
                byte[]? returnBytes = null;
                var readContent = new byte[receiveContent.Count * 2];
                var values = receiveContent.WriteContent;
                var valueDic = new Dictionary<string, double>();
                var redisValues = new Dictionary<string, ReturnUnit<double>>();
                if (values != null)
                {
                    try
                    {
                        if (_isUpdate && DateTime.Now - _updateTime > TimeSpan.FromSeconds(9.5))
                        {
                            _logger.LogDebug($"receive content { String.Concat(receiveContent.WriteContent.Select(p => " " + p.ToString("X2")))}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error");
                    }
                    switch (receiveContent.FunctionCode)
                    {
                        case (byte)ModbusProtocolFunctionCode.WriteMultiRegister:
                            {
                                Array.Copy(receiveContent.WriteContent, 0, threex, receiveContent.StartAddress * 2, receiveContent.WriteContent.Length);
                                returnBytes = new WriteDataModbusProtocol().Format(receiveContent.SlaveAddress, receiveContent.FunctionCode, receiveContent.StartAddress, receiveContent.Count);
                                _isUpdate = true;
                                break;
                            }
                        case (byte)ModbusProtocolFunctionCode.WriteSingleCoil:
                            {
                                if (receiveContent.WriteContent[0] == 255)
                                {
                                    zerox[receiveContent.StartAddress] = true;
                                }
                                else
                                {
                                    zerox[receiveContent.StartAddress] = false;
                                }
                                returnBytes = new WriteDataModbusProtocol().Format(receiveContent.SlaveAddress, receiveContent.FunctionCode, receiveContent.StartAddress, receiveContent.WriteContent);
                                _isUpdate = true;
                                break;
                            }
                        case (byte)ModbusProtocolFunctionCode.WriteMultiCoil:
                            {
                                var pos = 0;
                                List<bool> bitList = new List<bool>();
                                for (int i = 0; i < receiveContent.WriteByteCount; i++)
                                {
                                    var bitArray = BigEndianLsbValueHelper.Instance.GetBits(receiveContent.WriteContent, ref pos);
                                    bitList.AddRange(bitArray.ToList());
                                }
                                Array.Copy(bitList.ToArray(), 0, zerox, receiveContent.StartAddress, bitList.Count);
                                returnBytes = new WriteDataModbusProtocol().Format(receiveContent.SlaveAddress, receiveContent.FunctionCode, receiveContent.StartAddress, receiveContent.Count);
                                _isUpdate = true;
                                break;
                            }
                        case (byte)ModbusProtocolFunctionCode.WriteSingleRegister:
                            {
                                Array.Copy(receiveContent.WriteContent, 0, threex, receiveContent.StartAddress * 2, receiveContent.Count * 2);
                                returnBytes = new WriteDataModbusProtocol().Format(receiveContent.SlaveAddress, receiveContent.FunctionCode, receiveContent.StartAddress, receiveContent.Count);
                                _isUpdate = true;
                                break;
                            }
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
                        case (byte)ModbusProtocolFunctionCode.ReadCoilStatus:
                            {
                                var bitCount = receiveContent.WriteByteCount * 8;
                                var boolContent = new bool[bitCount];
                                Array.Copy(zerox, receiveContent.StartAddress, boolContent, 0, bitCount);
                                var byteList = new List<byte>();
                                for (int i = 0; i < receiveContent.WriteByteCount; i++)
                                {
                                    byte result = 0;
                                    for (int j = i; j < i + 8; j++)
                                    {
                                        // 将布尔值转换为对应的位

                                        byte bit = boolContent[j] ? (byte)1 : (byte)0;

                                        // 使用左移位运算将位合并到结果字节中

                                        result = (byte)((result << 1) | bit);
                                    }
                                    byteList.Add(result);
                                }
                                readContent = byteList.ToArray();
                                returnBytes = new ReadDataModbusProtocol().Format(receiveContent.SlaveAddress, receiveContent.FunctionCode, receiveContent.WriteByteCount, readContent);
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