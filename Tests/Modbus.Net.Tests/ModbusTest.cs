using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Modbus;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class ModbusTest
    {
        private BaseMachine _modbusTcpMachine;

        private BaseMachine _modbusRtuMachine;

        private BaseMachine _modbusAsciiMachine;

        [TestInitialize]
        public void Init()
        {
            _modbusTcpMachine = new ModbusMachine("1", ModbusType.Tcp, "192.168.3.10", null, true, 2, 0);

            _modbusRtuMachine = new ModbusMachine("2", ModbusType.Rtu, "COM5", null, true, 2, 0);

            _modbusAsciiMachine = new ModbusMachine("3", ModbusType.Ascii, "COM5", null, true, 2, 0);
        }

        [TestMethod]
        public async Task ModbusCoilSingle()
        {
            Random r = new Random();

            var addresses = new List<AddressUnit>
            {
                new AddressUnit
                {
                    Id = "0",
                    Area = "0X",
                    Address = 1,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(bool)
                }
            };

            var dic1 = new Dictionary<string, double>()
            {
                {
                    "0X 1.0", r.Next(0, 2)
                }
            };

            _modbusTcpMachine.GetAddresses = addresses;
            _modbusAsciiMachine.GetAddresses = addresses;
            _modbusRtuMachine.GetAddresses = addresses;
            await _modbusTcpMachine.SetDatasAsync(MachineSetDataType.Address, dic1);
            await _modbusAsciiMachine.SetDatasAsync(MachineSetDataType.Address, dic1);
            await _modbusRtuMachine.SetDatasAsync(MachineSetDataType.Address, dic1);
            var ans = await _modbusTcpMachine.GetDatasAsync(MachineGetDataType.Address);
            var ans2 = await _modbusRtuMachine.GetDatasAsync(MachineGetDataType.Address);
            var ans3 = await _modbusAsciiMachine.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans["0X 1.0"].PlcValue, dic1["0X 1.0"]);
            Assert.AreEqual(ans2["0X 1.0"].PlcValue, dic1["0X 1.0"]);
            Assert.AreEqual(ans3["0X 1.0"].PlcValue, dic1["0X 1.0"]);
        }

        [TestMethod]
        public async Task ModbusDInputSingle()
        {
            var addresses = new List<AddressUnit>
            {
                new AddressUnit
                {
                    Id = "0",
                    Area = "1X",
                    Address = 1,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(bool)
                }
            };

            _modbusTcpMachine.GetAddresses = addresses;
            _modbusRtuMachine.GetAddresses = addresses;
            _modbusAsciiMachine.GetAddresses = addresses;
            var ans = await _modbusTcpMachine.GetDatasAsync(MachineGetDataType.Address);
            var ans2 = await _modbusRtuMachine.GetDatasAsync(MachineGetDataType.Address);
            var ans3 = await _modbusAsciiMachine.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans["1X 1.0"].PlcValue, 0);
            Assert.AreEqual(ans2["1X 1.0"].PlcValue, 0);
            Assert.AreEqual(ans3["1X 1.0"].PlcValue, 0);
        }

        [TestMethod]
        public async Task ModbusIRegSingle()
        {
            var addresses = new List<AddressUnit>
            {
                new AddressUnit
                {
                    Id = "0",
                    Area = "3X",
                    Address = 1,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(ushort)
                }
            };

            _modbusTcpMachine.GetAddresses = addresses;
            _modbusRtuMachine.GetAddresses = addresses;
            _modbusAsciiMachine.GetAddresses = addresses;
            var ans = await _modbusTcpMachine.GetDatasAsync(MachineGetDataType.Address);
            var ans2 = await _modbusRtuMachine.GetDatasAsync(MachineGetDataType.Address);
            var ans3 = await _modbusAsciiMachine.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans["3X 1.0"].PlcValue, 0);
            Assert.AreEqual(ans2["3X 1.0"].PlcValue, 0);
            Assert.AreEqual(ans3["3X 1.0"].PlcValue, 0);
        }

        [TestMethod]
        public async Task ModbusRegSingle()
        {
            Random r = new Random();

            var addresses = new List<AddressUnit>
            {
                new AddressUnit
                {
                    Id = "0",
                    Area = "4X",
                    Address = 1,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(ushort)
                }
            };

            var dic1 = new Dictionary<string, double>()
            {
                {
                    "4X 1", r.Next(0, UInt16.MaxValue)
                }
            };

            _modbusTcpMachine.GetAddresses = addresses;
            _modbusAsciiMachine.GetAddresses = addresses;
            _modbusRtuMachine.GetAddresses = addresses;
            await _modbusTcpMachine.SetDatasAsync(MachineSetDataType.Address, dic1);
            await _modbusAsciiMachine.SetDatasAsync(MachineSetDataType.Address, dic1);
            await _modbusRtuMachine.SetDatasAsync(MachineSetDataType.Address, dic1);
            var ans = await _modbusTcpMachine.GetDatasAsync(MachineGetDataType.Address);
            var ans2 = await _modbusRtuMachine.GetDatasAsync(MachineGetDataType.Address);
            var ans3 = await _modbusAsciiMachine.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans["4X 1.0"].PlcValue, dic1["4X 1"]);
            Assert.AreEqual(ans2["4X 1.0"].PlcValue, dic1["4X 1"]);
            Assert.AreEqual(ans3["4X 1.0"].PlcValue, dic1["4X 1"]);
        }

        [TestMethod]
        public async Task ModbusRegMultiple()
        {
            Random r = new Random();

            var addresses = new List<AddressUnit>
            {
                new AddressUnit
                {
                    Id = "0",
                    Area = "4X",
                    Address = 2,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Id = "1",
                    Area = "4X",
                    Address = 3,
                    SubAddress = 0,
                    CommunicationTag = "A2",
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Id = "2",
                    Area = "4X",
                    Address = 4,
                    SubAddress = 0,
                    CommunicationTag = "A3",
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Id = "3",
                    Area = "4X",
                    Address = 5,
                    SubAddress = 0,
                    CommunicationTag = "A4",
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Id = "4",
                    Area = "4X",
                    Address = 6,
                    SubAddress = 0,
                    CommunicationTag = "A5",
                    DataType = typeof(uint)
                },
                new AddressUnit
                {
                    Id = "5",
                    Area = "4X",
                    Address = 8,
                    SubAddress = 0,
                    CommunicationTag = "A6",
                    DataType = typeof(uint)
                }
            };

            var dic1 = new Dictionary<string, double>()
            {
                {
                    "A1", r.Next(0, UInt16.MaxValue)
                },
                {
                    "A2", r.Next(0, UInt16.MaxValue)
                },
                {
                    "A3", r.Next(0, UInt16.MaxValue)
                },
                {
                    "A4", r.Next(0, UInt16.MaxValue)
                },
                {
                    "A5", r.Next()
                },
                {
                    "A6", r.Next()
                },
            };

            _modbusTcpMachine.GetAddresses = addresses;
            _modbusRtuMachine.GetAddresses = addresses;
            _modbusAsciiMachine.GetAddresses = addresses;
            await _modbusTcpMachine.SetDatasAsync(MachineSetDataType.CommunicationTag, dic1);
            await _modbusRtuMachine.SetDatasAsync(MachineSetDataType.CommunicationTag, dic1);
            await _modbusAsciiMachine.SetDatasAsync(MachineSetDataType.CommunicationTag, dic1);

            var ans = await _modbusTcpMachine.GetDatasAsync(MachineGetDataType.CommunicationTag);
            var ans2 = await _modbusRtuMachine.GetDatasAsync(MachineGetDataType.CommunicationTag);
            var ans3 = await _modbusAsciiMachine.GetDatasAsync(MachineGetDataType.CommunicationTag);

            Assert.AreEqual(ans["A1"].PlcValue, dic1["A1"]);
            Assert.AreEqual(ans["A2"].PlcValue, dic1["A2"]);
            Assert.AreEqual(ans["A3"].PlcValue, dic1["A3"]);
            Assert.AreEqual(ans["A4"].PlcValue, dic1["A4"]);
            Assert.AreEqual(ans["A5"].PlcValue, dic1["A5"]);
            Assert.AreEqual(ans["A6"].PlcValue, dic1["A6"]);
            Assert.AreEqual(ans2["A1"].PlcValue, dic1["A1"]);
            Assert.AreEqual(ans2["A2"].PlcValue, dic1["A2"]);
            Assert.AreEqual(ans2["A3"].PlcValue, dic1["A3"]);
            Assert.AreEqual(ans2["A4"].PlcValue, dic1["A4"]);
            Assert.AreEqual(ans2["A5"].PlcValue, dic1["A5"]);
            Assert.AreEqual(ans2["A6"].PlcValue, dic1["A6"]);
            Assert.AreEqual(ans3["A1"].PlcValue, dic1["A1"]);
            Assert.AreEqual(ans3["A2"].PlcValue, dic1["A2"]);
            Assert.AreEqual(ans3["A3"].PlcValue, dic1["A3"]);
            Assert.AreEqual(ans3["A4"].PlcValue, dic1["A4"]);
            Assert.AreEqual(ans3["A5"].PlcValue, dic1["A5"]);
            Assert.AreEqual(ans3["A6"].PlcValue, dic1["A6"]);
        }

        [TestMethod]
        public async Task ModbusWriteSingleTest()
        {
            Random r = new Random();

            var dic1 = new Dictionary<string, double>()
            {
                {
                    "4X 1", r.Next(0, UInt16.MaxValue)
                }
            };

            var dic2 = new Dictionary<string, double>()
            {
                {
                    "0X 1", r.Next(0, 2)
                }
            };

            await _modbusTcpMachine.BaseUtility.GetUtilityMethods<IUtilityMethodWriteSingle>().SetSingleDataAsync("4X 1", dic1["4X 1"]);
            await _modbusAsciiMachine.BaseUtility.GetUtilityMethods<IUtilityMethodWriteSingle>().SetSingleDataAsync("4X 1", dic1["4X 1"]);
            await _modbusRtuMachine.BaseUtility.GetUtilityMethods<IUtilityMethodWriteSingle>().SetSingleDataAsync("4X 1", dic1["4X 1"]);
            var ans = await _modbusTcpMachine.BaseUtility.GetUtilityMethods<IUtilityMethodData>().GetDatasAsync<ushort>("4X 1", 1);
            var ans2 = await _modbusRtuMachine.BaseUtility.GetUtilityMethods<IUtilityMethodData>().GetDatasAsync<ushort>("4X 1", 1);
            var ans3 = await _modbusAsciiMachine.BaseUtility.GetUtilityMethods<IUtilityMethodData>().GetDatasAsync<ushort>("4X 1", 1);
            Assert.AreEqual(ans[0], dic1["4X 1"]);
            Assert.AreEqual(ans2[0], dic1["4X 1"]);
            Assert.AreEqual(ans3[0], dic1["4X 1"]);
            await _modbusTcpMachine.BaseUtility.GetUtilityMethods<IUtilityMethodWriteSingle>().SetSingleDataAsync("0X 1", dic2["0X 1"] >= 1);
            await _modbusAsciiMachine.BaseUtility.GetUtilityMethods<IUtilityMethodWriteSingle>().SetSingleDataAsync("0X 1", dic2["0X 1"] >= 1);
            await _modbusRtuMachine.BaseUtility.GetUtilityMethods<IUtilityMethodWriteSingle>().SetSingleDataAsync("0X 1", dic2["0X 1"] >= 1);
            var ans21 = await _modbusTcpMachine.BaseUtility.GetUtilityMethods<IUtilityMethodData>().GetDatasAsync<bool>("0X 1", 1);
            var ans22 = await _modbusRtuMachine.BaseUtility.GetUtilityMethods<IUtilityMethodData>().GetDatasAsync<bool>("0X 1", 1);
            var ans23 = await _modbusAsciiMachine.BaseUtility.GetUtilityMethods<IUtilityMethodData>().GetDatasAsync<bool>("0X 1", 1);
            Assert.AreEqual(ans21[0] ? 1 : 0, dic2["0X 1"]);
            Assert.AreEqual(ans22[0] ? 1 : 0, dic2["0X 1"]);
            Assert.AreEqual(ans23[0] ? 1 : 0, dic2["0X 1"]);
        }


        [TestCleanup]
        public void MachineClean()
        {
            _modbusAsciiMachine.Disconnect();
            _modbusRtuMachine.Disconnect();
            _modbusTcpMachine.Disconnect();
        }
    }
}
