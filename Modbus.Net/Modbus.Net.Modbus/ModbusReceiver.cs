using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AddressUnit = Modbus.Net.AddressUnit<string, int, int>;
using DataReturnDef = Modbus.Net.DataReturnDef<string, double>;

namespace Modbus.Net.Modbus
{
    public class ModbusRtuDataReceiver
    {
        private ModbusRtuProtocolReceiver _receiver;

        private readonly IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();

        private bool[] zerox = new bool[10000];
        private byte[] threex = new byte[20000];

        public delegate Dictionary<string, double> ReturnValuesDelegate(DataReturnDef returnValues);

        public event ReturnValuesDelegate ReturnValueDictionary;

        protected void AddValueToValueDic(Dictionary<string, double> valueDic, Dictionary<string, ReturnUnit<double>> returnDic, AddressUnit address, double value, MachineDataType dataType)
        {
            switch (dataType)
            {
                case MachineDataType.Id:
                    {
                        valueDic.Add(address.Id, value);
                        returnDic.Add(address.Id, new ReturnUnit<double>() { AddressUnit = address.MapAddressUnitTUnitKeyToAddressUnit(), DeviceValue = value });
                        break;
                    }
                case MachineDataType.Name:
                    {
                        valueDic.Add(address.Name, value);
                        returnDic.Add(address.Name, new ReturnUnit<double>() { AddressUnit = address.MapAddressUnitTUnitKeyToAddressUnit(), DeviceValue = value });
                        break;
                    }
                case MachineDataType.Address:
                    {
                        valueDic.Add(new AddressFormaterModbus().FormatAddress(address.Area, address.Address, address.SubAddress), value);
                        returnDic.Add(new AddressFormaterModbus().FormatAddress(address.Area, address.Address, address.SubAddress), new ReturnUnit<double>() { AddressUnit = address.MapAddressUnitTUnitKeyToAddressUnit(), DeviceValue = value });
                        break;
                    }
                case MachineDataType.CommunicationTag:
                    {
                        valueDic.Add(address.CommunicationTag, value);
                        returnDic.Add(address.CommunicationTag, new ReturnUnit<double>() { AddressUnit = address.MapAddressUnitTUnitKeyToAddressUnit(), DeviceValue = value });
                        break;
                    }
            }
        }

        public ModbusRtuDataReceiver(MachineDataType dataType)
        {
            _receiver = new ModbusRtuProtocolReceiver(ConfigurationReader.GetValue("Receiver", "d:connectionString"), int.Parse(ConfigurationReader.GetValue("Receiver", "g:slaveAddress")));
            var machineName = ConfigurationReader.GetValue("Receiver", "a:id");
            var addressMapName = ConfigurationReader.GetValue("Receiver", "e:addressMap");
            var endian = ValueHelper.GetInstance(Endian.Parse(ConfigurationReader.GetValue("Receiver", "i:endian")));
            _receiver.DataProcess = receiveContent =>
            {
                var returnTime = DateTime.Now;
                byte[] returnBytes = null;
                var readContent = new byte[receiveContent.Count * 2];
                var values = receiveContent.WriteContent;
                var valueDic = new Dictionary<string, double>();
                var returnDic = new Dictionary<string, ReturnUnit<double>>();
                List<AddressUnit> addressMap = AddressReader<string, int, int>.ReadAddresses(addressMapName).ToList();
                if (values != null)
                {
                    try
                    {                              
                        for (int i = 0; i < addressMap.Count; i++)
                        {
                            var pos = (addressMap[i].Address - 1) * 2;
                            var subpos = addressMap[i].SubAddress;
                            string valueString = null;
                            if (addressMap[i].Area == "4X")
                            {
                                valueString = endian.GetValue(threex, ref pos, ref subpos, addressMap[i].DataType).ToString();
                            }
                            else if (addressMap[i].Area == "0X")
                            {
                                valueString = zerox[addressMap[i].Address - 1].ToString();
                            }
                            if (valueString == "True") valueString = "1";
                            if (valueString == "False") valueString = "0";
                            var value = double.Parse(valueString);
                            value = value * addressMap[i].Zoom;
                            value = Math.Round(value, addressMap[i].DecimalPos);
                            AddValueToValueDic(valueDic, returnDic, addressMap[i], value, dataType);
                            if (ReturnValueDictionary != null)
                            {
                                var dataReturn = new DataReturnDef();
                                dataReturn.MachineId = machineName;
                                dataReturn.ReturnValues = new ReturnStruct<Dictionary<string, ReturnUnit<double>>>() { IsSuccess = true, Datas = returnDic };
                                ReturnValueDictionary(dataReturn);
                            }
                        }                     
                    }
                    catch (Exception ex)
                    {
                        //_logger.LogError(ex, "Error");
                    }
                    switch (receiveContent.FunctionCode)
                    {
                        case (byte)ModbusProtocolFunctionCode.WriteMultiRegister:
                            {
                                Array.Copy(receiveContent.WriteContent, 0, threex, receiveContent.StartAddress * 2, receiveContent.WriteContent.Length);
                                returnBytes = new WriteDataModbusProtocol().Format(receiveContent.SlaveAddress, receiveContent.FunctionCode, receiveContent.StartAddress, receiveContent.Count);
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
                                break;
                            }
                        case (byte)ModbusProtocolFunctionCode.WriteSingleRegister:
                            {
                                Array.Copy(receiveContent.WriteContent, 0, threex, receiveContent.StartAddress * 2, receiveContent.Count * 2);
                                returnBytes = new WriteDataModbusProtocol().Format(receiveContent.SlaveAddress, receiveContent.FunctionCode, receiveContent.StartAddress, receiveContent.Count);
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
        }
    }
}
