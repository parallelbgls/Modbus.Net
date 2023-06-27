using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Modbus;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class ModbusTest
    {
        private BaseMachine<string, string>? _modbusTcpMachine;

        private BaseMachine<string, string>? _modbusRtuMachine;

        private BaseMachine<string, string>? _modbusAsciiMachine;

        private string _machineIp = "10.10.18.251";

        private string _machineCom = "COM1";

        private string _machineCom2 = "COM3";

        [TestInitialize]
        public void Init()
        {
            _modbusTcpMachine = new ModbusMachine<string, string>("1", ModbusType.Tcp, _machineIp, null, true, 2, 0, Endian.BigEndianLsb);

            _modbusRtuMachine = new ModbusMachine<string, string>("2", ModbusType.Rtu, _machineCom, null, true, 2, 0, Endian.BigEndianLsb);

            _modbusAsciiMachine = new ModbusMachine<string, string>("3", ModbusType.Ascii, _machineCom2, null, true, 2, 0, Endian.BigEndianLsb);
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
            Assert.AreEqual(ans.Datas?["0X 1.0"].DeviceValue, dic1["0X 1.0"]);
            Assert.AreEqual(ans2.Datas?["0X 1.0"].DeviceValue, dic1["0X 1.0"]);
            Assert.AreEqual(ans3.Datas?["0X 1.0"].DeviceValue, dic1["0X 1.0"]);
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
            Assert.AreEqual(ans.Datas?["1X 1.0"].DeviceValue, 0);
            Assert.AreEqual(ans2.Datas?["1X 1.0"].DeviceValue, 0);
            Assert.AreEqual(ans3.Datas?["1X 1.0"].DeviceValue, 0);
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
            Assert.AreEqual(ans.Datas?["3X 1.0"].DeviceValue, 0);
            Assert.AreEqual(ans2.Datas?["3X 1.0"].DeviceValue, 0);
            Assert.AreEqual(ans3.Datas?["3X 1.0"].DeviceValue, 0);
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
            Assert.AreEqual(ans.Datas?["4X 1.0"].DeviceValue, dic1["4X 1"]);
            Assert.AreEqual(ans2.Datas?["4X 1.0"].DeviceValue, dic1["4X 1"]);
            Assert.AreEqual(ans3.Datas?["4X 1.0"].DeviceValue, dic1["4X 1"]);
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

            Assert.AreEqual(ans.Datas?["A1"].DeviceValue, dic1["A1"]);
            Assert.AreEqual(ans.Datas?["A2"].DeviceValue, dic1["A2"]);
            Assert.AreEqual(ans.Datas?["A3"].DeviceValue, dic1["A3"]);
            Assert.AreEqual(ans.Datas?["A4"].DeviceValue, dic1["A4"]);
            Assert.AreEqual(ans.Datas?["A5"].DeviceValue, dic1["A5"]);
            Assert.AreEqual(ans.Datas?["A6"].DeviceValue, dic1["A6"]);
            Assert.AreEqual(ans2.Datas?["A1"].DeviceValue, dic1["A1"]);
            Assert.AreEqual(ans2.Datas?["A2"].DeviceValue, dic1["A2"]);
            Assert.AreEqual(ans2.Datas?["A3"].DeviceValue, dic1["A3"]);
            Assert.AreEqual(ans2.Datas?["A4"].DeviceValue, dic1["A4"]);
            Assert.AreEqual(ans2.Datas?["A5"].DeviceValue, dic1["A5"]);
            Assert.AreEqual(ans2.Datas?["A6"].DeviceValue, dic1["A6"]);
            Assert.AreEqual(ans3.Datas?["A1"].DeviceValue, dic1["A1"]);
            Assert.AreEqual(ans3.Datas?["A2"].DeviceValue, dic1["A2"]);
            Assert.AreEqual(ans3.Datas?["A3"].DeviceValue, dic1["A3"]);
            Assert.AreEqual(ans3.Datas?["A4"].DeviceValue, dic1["A4"]);
            Assert.AreEqual(ans3.Datas?["A5"].DeviceValue, dic1["A5"]);
            Assert.AreEqual(ans3.Datas?["A6"].DeviceValue, dic1["A6"]);
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
