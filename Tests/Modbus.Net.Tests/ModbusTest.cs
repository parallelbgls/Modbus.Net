using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Modbus;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class ModbusTest
    {
        private BaseMachine? _modbusTcpMachine;

        private BaseMachine? _modbusRtuMachine;

        private BaseMachine? _modbusAsciiMachine;

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

            _modbusTcpMachine!.GetAddresses = addresses;
            _modbusAsciiMachine!.GetAddresses = addresses;
            _modbusRtuMachine!.GetAddresses = addresses;
            await _modbusTcpMachine.SetDatasAsync(MachineDataType.Address, dic1);
            await _modbusAsciiMachine.SetDatasAsync(MachineDataType.Address, dic1);
            await _modbusRtuMachine.SetDatasAsync(MachineDataType.Address, dic1);
            var ans = await _modbusTcpMachine.GetDatasAsync(MachineDataType.Address);
            var ans2 = await _modbusRtuMachine.GetDatasAsync(MachineDataType.Address);
            var ans3 = await _modbusAsciiMachine.GetDatasAsync(MachineDataType.Address);
            Assert.AreEqual(ans.Datas["0X 1.0"].DeviceValue, dic1["0X 1.0"]);
            Assert.AreEqual(ans2.Datas["0X 1.0"].DeviceValue, dic1["0X 1.0"]);
            Assert.AreEqual(ans3.Datas["0X 1.0"].DeviceValue, dic1["0X 1.0"]);
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

            _modbusTcpMachine!.GetAddresses = addresses;
            _modbusRtuMachine!.GetAddresses = addresses;
            _modbusAsciiMachine!.GetAddresses = addresses;
            var ans = await _modbusTcpMachine.GetDatasAsync(MachineDataType.Address);
            var ans2 = await _modbusRtuMachine.GetDatasAsync(MachineDataType.Address);
            var ans3 = await _modbusAsciiMachine.GetDatasAsync(MachineDataType.Address);
            Assert.AreEqual(ans.Datas["1X 1.0"].DeviceValue, 0);
            Assert.AreEqual(ans2.Datas["1X 1.0"].DeviceValue, 0);
            Assert.AreEqual(ans3.Datas["1X 1.0"].DeviceValue, 0);
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

            _modbusTcpMachine!.GetAddresses = addresses;
            _modbusRtuMachine!.GetAddresses = addresses;
            _modbusAsciiMachine!.GetAddresses = addresses;
            var ans = await _modbusTcpMachine.GetDatasAsync(MachineDataType.Address);
            var ans2 = await _modbusRtuMachine.GetDatasAsync(MachineDataType.Address);
            var ans3 = await _modbusAsciiMachine.GetDatasAsync(MachineDataType.Address);
            Assert.AreEqual(ans.Datas["3X 1.0"].DeviceValue, 0);
            Assert.AreEqual(ans2.Datas["3X 1.0"].DeviceValue, 0);
            Assert.AreEqual(ans3.Datas["3X 1.0"].DeviceValue, 0);
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

            _modbusTcpMachine!.GetAddresses = addresses;
            _modbusAsciiMachine!.GetAddresses = addresses;
            _modbusRtuMachine!.GetAddresses = addresses;
            await _modbusTcpMachine.SetDatasAsync(MachineDataType.Address, dic1);
            await _modbusAsciiMachine.SetDatasAsync(MachineDataType.Address, dic1);
            await _modbusRtuMachine.SetDatasAsync(MachineDataType.Address, dic1);
            var ans = await _modbusTcpMachine.GetDatasAsync(MachineDataType.Address);
            var ans2 = await _modbusRtuMachine.GetDatasAsync(MachineDataType.Address);
            var ans3 = await _modbusAsciiMachine.GetDatasAsync(MachineDataType.Address);
            Assert.AreEqual(ans.Datas["4X 1.0"].DeviceValue, dic1["4X 1"]);
            Assert.AreEqual(ans2.Datas["4X 1.0"].DeviceValue, dic1["4X 1"]);
            Assert.AreEqual(ans3.Datas["4X 1.0"].DeviceValue, dic1["4X 1"]);
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

            _modbusTcpMachine!.GetAddresses = addresses;
            _modbusRtuMachine!.GetAddresses = addresses;
            _modbusAsciiMachine!.GetAddresses = addresses;
            await _modbusTcpMachine.SetDatasAsync(MachineDataType.CommunicationTag, dic1);
            await _modbusRtuMachine.SetDatasAsync(MachineDataType.CommunicationTag, dic1);
            await _modbusAsciiMachine.SetDatasAsync(MachineDataType.CommunicationTag, dic1);

            var ans = await _modbusTcpMachine.GetDatasAsync(MachineDataType.CommunicationTag);
            var ans2 = await _modbusRtuMachine.GetDatasAsync(MachineDataType.CommunicationTag);
            var ans3 = await _modbusAsciiMachine.GetDatasAsync(MachineDataType.CommunicationTag);

            Assert.AreEqual(ans.Datas["A1"].DeviceValue, dic1["A1"]);
            Assert.AreEqual(ans.Datas["A2"].DeviceValue, dic1["A2"]);
            Assert.AreEqual(ans.Datas["A3"].DeviceValue, dic1["A3"]);
            Assert.AreEqual(ans.Datas["A4"].DeviceValue, dic1["A4"]);
            Assert.AreEqual(ans.Datas["A5"].DeviceValue, dic1["A5"]);
            Assert.AreEqual(ans.Datas["A6"].DeviceValue, dic1["A6"]);
            Assert.AreEqual(ans2.Datas["A1"].DeviceValue, dic1["A1"]);
            Assert.AreEqual(ans2.Datas["A2"].DeviceValue, dic1["A2"]);
            Assert.AreEqual(ans2.Datas["A3"].DeviceValue, dic1["A3"]);
            Assert.AreEqual(ans2.Datas["A4"].DeviceValue, dic1["A4"]);
            Assert.AreEqual(ans2.Datas["A5"].DeviceValue, dic1["A5"]);
            Assert.AreEqual(ans2.Datas["A6"].DeviceValue, dic1["A6"]);
            Assert.AreEqual(ans3.Datas["A1"].DeviceValue, dic1["A1"]);
            Assert.AreEqual(ans3.Datas["A2"].DeviceValue, dic1["A2"]);
            Assert.AreEqual(ans3.Datas["A3"].DeviceValue, dic1["A3"]);
            Assert.AreEqual(ans3.Datas["A4"].DeviceValue, dic1["A4"]);
            Assert.AreEqual(ans3.Datas["A5"].DeviceValue, dic1["A5"]);
            Assert.AreEqual(ans3.Datas["A6"].DeviceValue, dic1["A6"]);
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

            await _modbusTcpMachine!.BaseUtility.GetUtilityMethods<IUtilityMethodWriteSingleCoil>().SetSingleCoilAsync("4X 1", dic1["4X 1"]);
            await _modbusAsciiMachine!.BaseUtility.GetUtilityMethods<IUtilityMethodWriteSingleCoil>().SetSingleCoilAsync("4X 1", dic1["4X 1"]);
            await _modbusRtuMachine!.BaseUtility.GetUtilityMethods<IUtilityMethodWriteSingleCoil>().SetSingleCoilAsync("4X 1", dic1["4X 1"]);
            var ans = await _modbusTcpMachine.BaseUtility.GetUtilityMethods<IUtilityMethodData>().GetDatasAsync<ushort>("4X 1", 1);
            var ans2 = await _modbusRtuMachine.BaseUtility.GetUtilityMethods<IUtilityMethodData>().GetDatasAsync<ushort>("4X 1", 1);
            var ans3 = await _modbusAsciiMachine.BaseUtility.GetUtilityMethods<IUtilityMethodData>().GetDatasAsync<ushort>("4X 1", 1);
            Assert.AreEqual(ans.Datas[0], dic1["4X 1"]);
            Assert.AreEqual(ans2.Datas[0], dic1["4X 1"]);
            Assert.AreEqual(ans3.Datas[0], dic1["4X 1"]);
            await _modbusTcpMachine.BaseUtility.GetUtilityMethods<IUtilityMethodWriteSingleCoil>().SetSingleCoilAsync("0X 1", dic2["0X 1"] >= 1);
            await _modbusAsciiMachine.BaseUtility.GetUtilityMethods<IUtilityMethodWriteSingleCoil>().SetSingleCoilAsync("0X 1", dic2["0X 1"] >= 1);
            await _modbusRtuMachine.BaseUtility.GetUtilityMethods<IUtilityMethodWriteSingleCoil>().SetSingleCoilAsync("0X 1", dic2["0X 1"] >= 1);
            var ans21 = await _modbusTcpMachine.BaseUtility.GetUtilityMethods<IUtilityMethodData>().GetDatasAsync<bool>("0X 1", 1);
            var ans22 = await _modbusRtuMachine.BaseUtility.GetUtilityMethods<IUtilityMethodData>().GetDatasAsync<bool>("0X 1", 1);
            var ans23 = await _modbusAsciiMachine.BaseUtility.GetUtilityMethods<IUtilityMethodData>().GetDatasAsync<bool>("0X 1", 1);
            Assert.AreEqual(ans21.Datas[0] ? 1 : 0, dic2["0X 1"]);
            Assert.AreEqual(ans22.Datas[0] ? 1 : 0, dic2["0X 1"]);
            Assert.AreEqual(ans23.Datas[0] ? 1 : 0, dic2["0X 1"]);
        }


        [TestCleanup]
        public void MachineClean()
        {
            _modbusAsciiMachine!.Disconnect();
            _modbusRtuMachine!.Disconnect();
            _modbusTcpMachine!.Disconnect();
        }
    }
}
