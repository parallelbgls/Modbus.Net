using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Siemens;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class SiemensTest
    {
        private BaseMachine? _siemensTcpMachine;

        [TestInitialize]
        public void Init()
        {
            _siemensTcpMachine = new SiemensMachine("1", SiemensType.Tcp, "192.168.3.10", SiemensMachineModel.S7_1200, null, true, 2, 0);
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

            var ans = await _siemensTcpMachine.GetDatasAsync(MachineDataType.Address);
            Assert.AreEqual(ans.Datas["Q 0.0"].DeviceValue, dic1["Q 0.0"]);
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
            var ans = await _siemensTcpMachine.GetDatasAsync(MachineDataType.Address);
            Assert.AreEqual(ans.Datas["I 0.0"].DeviceValue, 0);
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

            var dic1 = new Dictionary<string, double>()
            {
                {
                    "M 0", r.Next(0, UInt16.MaxValue)
                }
            };

            await _siemensTcpMachine.SetDatasAsync(MachineDataType.Address, dic1);
            var ans = await _siemensTcpMachine.GetDatasAsync(MachineDataType.Address);
            Assert.AreEqual(ans.Datas["M 0.0"].DeviceValue, dic1["M 0"]);
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

            var dic1 = new Dictionary<string, double>()
            {
                {
                    "M 0.0", r.Next(0, 2)
                }
            };

            await _siemensTcpMachine.SetDatasAsync(MachineDataType.Address, dic1);

            var ans = await _siemensTcpMachine.GetDatasAsync(MachineDataType.Address);
            Assert.AreEqual(ans.Datas["M 0.0"].DeviceValue, dic1["M 0.0"]);
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
                    Area = "DB2",
                    Address = 0,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(ushort)
                }
            };

            var dic1 = new Dictionary<string, double>()
            {
                {
                    "DB2 0.0", r.Next(0, UInt16.MaxValue)
                }
            };

            _siemensTcpMachine!.GetAddresses = addresses;
            await _siemensTcpMachine.SetDatasAsync(MachineDataType.Address, dic1);

            var ans = await _siemensTcpMachine.GetDatasAsync(MachineDataType.Address);
            Assert.AreEqual(ans.Datas["DB2 0.0"].DeviceValue, dic1["DB2 0.0"]);
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
                    Area = "DB2",
                    Address = 2,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Id = "1",
                    Area = "DB2",
                    Address = 4,
                    SubAddress = 0,
                    CommunicationTag = "A2",
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Id = "2",
                    Area = "DB2",
                    Address = 6,
                    SubAddress = 0,
                    CommunicationTag = "A3",
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Id = "3",
                    Area = "DB2",
                    Address = 8,
                    SubAddress = 0,
                    CommunicationTag = "A4",
                    DataType = typeof(ushort)
                },
                new AddressUnit
                {
                    Id = "4",
                    Area = "DB2",
                    Address = 10,
                    SubAddress = 0,
                    CommunicationTag = "A5",
                    DataType = typeof(uint)
                },
                new AddressUnit
                {
                    Id = "5",
                    Area = "DB2",
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
            await _siemensTcpMachine.SetDatasAsync(MachineDataType.CommunicationTag, dic1);
            var ans = await _siemensTcpMachine.GetDatasAsync(MachineDataType.CommunicationTag);
            Assert.AreEqual(ans.Datas["A1"].DeviceValue, dic1["A1"]);
            Assert.AreEqual(ans.Datas["A2"].DeviceValue, dic1["A2"]);
            Assert.AreEqual(ans.Datas["A3"].DeviceValue, dic1["A3"]);
            Assert.AreEqual(ans.Datas["A4"].DeviceValue, dic1["A4"]);
            Assert.AreEqual(ans.Datas["A5"].DeviceValue, dic1["A5"]);
            Assert.AreEqual(ans.Datas["A6"].DeviceValue, dic1["A6"]);
        }

        [TestCleanup]
        public void MachineClean()
        {
            _siemensTcpMachine!.Disconnect();
        }
    }
}
