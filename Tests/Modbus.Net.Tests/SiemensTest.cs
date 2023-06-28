using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Siemens;
using AddressUnit = Modbus.Net.AddressUnit<string, int, int>;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class SiemensTest
    {
        private BaseMachine<string, string>? _siemensTcpMachine;

        private BaseMachine<string, string>? _siemensPpiMachine;

        private string _machineIp = "10.10.18.251";

        private string _machineCom = "COM11";

        [TestInitialize]
        public void Init()
        {
            _siemensTcpMachine = new SiemensMachine<string, string>("1", SiemensType.Tcp, _machineIp, SiemensMachineModel.S7_1200, null, true, 2, 0);

            _siemensPpiMachine = new SiemensMachine<string, string>("2", SiemensType.Ppi, _machineCom, SiemensMachineModel.S7_200, null, true, 2, 0, 1, 0);
        }

        [TestMethod]
        public async Task SiemensCoilSingle()
        {
            Random r = new Random();

            var addresses = new List<AddressUnit>
            {
                new AddressUnit
                {
                    Id = "0",
                    Area = "Q",
                    Address = 0,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(bool)
                }
            };

            var dic1 = new Dictionary<string, double>()
            {
                {
                    "Q 0.0", r.Next(0, 2)
                }
            };

            _siemensTcpMachine!.GetAddresses = addresses;
            await _siemensTcpMachine.SetDatasAsync(MachineDataType.Address, dic1);
            _siemensPpiMachine!.GetAddresses = addresses;
            await _siemensPpiMachine.SetDatasAsync(MachineDataType.Address, dic1);

            var ans = await _siemensTcpMachine.GetDatasAsync(MachineDataType.Address);
            var ans2 = await _siemensPpiMachine.GetDatasAsync(MachineDataType.Address);

            Assert.AreEqual(ans.Datas["Q 0.0"].DeviceValue, dic1["Q 0.0"]);
            Assert.AreEqual(ans2.Datas["Q 0.0"].DeviceValue, dic1["Q 0.0"]);
        }

        [TestMethod]
        public async Task SiemensDInputSingle()
        {
            var addresses = new List<AddressUnit>
            {
                new AddressUnit
                {
                    Id = "0",
                    Area = "I",
                    Address = 0,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(bool)
                }
            };

            _siemensTcpMachine!.GetAddresses = addresses;
            _siemensPpiMachine!.GetAddresses = addresses;

            var ans = await _siemensTcpMachine.GetDatasAsync(MachineDataType.Address);
            var ans2 = await _siemensPpiMachine.GetDatasAsync(MachineDataType.Address);

            Assert.AreEqual(ans.Datas["I 0.0"].DeviceValue, 0);
            Assert.AreEqual(ans2.Datas["I 0.0"].DeviceValue, 0);
        }

        [TestMethod]
        public async Task SiemensMSingle()
        {
            Random r = new Random();

            var addresses = new List<AddressUnit>
            {
                new AddressUnit
                {
                    Id = "0",
                    Area = "M",
                    Address = 0,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(ushort)
                }
            };

            _siemensTcpMachine!.GetAddresses = addresses;
            _siemensPpiMachine!.GetAddresses = addresses;

            var dic1 = new Dictionary<string, double>()
            {
                {
                    "M 0", r.Next(0, UInt16.MaxValue)
                }
            };

            await _siemensTcpMachine.SetDatasAsync(MachineDataType.Address, dic1);
            await _siemensPpiMachine.SetDatasAsync(MachineDataType.Address, dic1);

            var ans = await _siemensTcpMachine.GetDatasAsync(MachineDataType.Address);
            var ans2 = await _siemensPpiMachine.GetDatasAsync(MachineDataType.Address);

            Assert.AreEqual(ans.Datas["M 0.0"].DeviceValue, dic1["M 0"]);
            Assert.AreEqual(ans2.Datas["M 0.0"].DeviceValue, dic1["M 0"]);
        }

        [TestMethod]
        public async Task SiemensMSingleBool()
        {
            Random r = new Random();

            var addresses = new List<AddressUnit>
            {
                new AddressUnit
                {
                    Id = "0",
                    Area = "M",
                    Address = 0,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(bool)
                }
            };

            _siemensTcpMachine!.GetAddresses = addresses;
            _siemensPpiMachine!.GetAddresses = addresses;

            var dic1 = new Dictionary<string, double>()
            {
                {
                    "M 0.0", r.Next(0, 2)
                }
            };

            await _siemensTcpMachine.SetDatasAsync(MachineDataType.Address, dic1);
            await _siemensPpiMachine.SetDatasAsync(MachineDataType.Address, dic1);

            var ans = await _siemensTcpMachine.GetDatasAsync(MachineDataType.Address);
            var ans2 = await _siemensPpiMachine.GetDatasAsync(MachineDataType.Address);

            Assert.AreEqual(ans.Datas["M 0.0"].DeviceValue, dic1["M 0.0"]);
            Assert.AreEqual(ans2.Datas["M 0.0"].DeviceValue, dic1["M 0.0"]);
        }

        [TestMethod]
        public async Task SiemensDbSingle()
        {
            Random r = new Random();

            var addresses = new List<AddressUnit>
            {
                new AddressUnit
                {
                    Id = "0",
                    Area = "DB1",
                    Address = 0,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(ushort)
                }
            };

            var dic1 = new Dictionary<string, double>()
            {
                {
                    "DB1 0.0", r.Next(0, UInt16.MaxValue)
                }
            };

            _siemensTcpMachine!.GetAddresses = addresses;
            _siemensPpiMachine!.GetAddresses = addresses;

            await _siemensTcpMachine.SetDatasAsync(MachineDataType.Address, dic1);
            await _siemensPpiMachine.SetDatasAsync(MachineDataType.Address, dic1);

            var ans = await _siemensTcpMachine.GetDatasAsync(MachineDataType.Address);
            var ans2 = await _siemensPpiMachine.GetDatasAsync(MachineDataType.Address);

            Assert.AreEqual(ans.Datas["DB1 0.0"].DeviceValue, dic1["DB1 0.0"]);
            Assert.AreEqual(ans2.Datas["DB1 0.0"].DeviceValue, dic1["DB1 0.0"]);
        }

        [TestMethod]
        public async Task SiemensDbMultiple()
        {
            Random r = new Random();

            var addresses = new List<AddressUnit>
            {
                new AddressUnit
                {
                    Id = "0",
                    Area = "DB1",
                    Address = 2,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Id = "1",
                    Area = "DB1",
                    Address = 4,
                    SubAddress = 0,
                    CommunicationTag = "A2",
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Id = "2",
                    Area = "DB1",
                    Address = 6,
                    SubAddress = 0,
                    CommunicationTag = "A3",
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Id = "3",
                    Area = "DB1",
                    Address = 8,
                    SubAddress = 0,
                    CommunicationTag = "A4",
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Id = "4",
                    Area = "DB1",
                    Address = 10,
                    SubAddress = 0,
                    CommunicationTag = "A5",
                    DataType = typeof(uint)
                },
                new AddressUnit
                {
                    Id = "5",
                    Area = "DB1",
                    Address = 14,
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

            _siemensTcpMachine!.GetAddresses = addresses;
            _siemensPpiMachine!.GetAddresses = addresses;

            await _siemensTcpMachine.SetDatasAsync(MachineDataType.CommunicationTag, dic1);
            await _siemensPpiMachine.SetDatasAsync(MachineDataType.CommunicationTag, dic1);

            var ans = await _siemensTcpMachine.GetDatasAsync(MachineDataType.CommunicationTag);
            var ans2 = await _siemensPpiMachine.GetDatasAsync(MachineDataType.CommunicationTag);

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
        }

        [TestCleanup]
        public void MachineClean()
        {
            _siemensTcpMachine!.Disconnect();

            _siemensPpiMachine!.Disconnect();
        }
    }
}
