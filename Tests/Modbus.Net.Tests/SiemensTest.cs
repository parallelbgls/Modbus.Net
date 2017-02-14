using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Modbus;
using Modbus.Net.Siemens;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class SiemensTest
    {
        private BaseMachine _siemensTcpMachine;

        [TestInitialize]
        public void Init()
        {
            _siemensTcpMachine = new SiemensMachine(SiemensType.Tcp, "192.168.3.10", SiemensMachineModel.S7_1200, null, true, 2, 0);
        }

        [TestMethod]
        public async Task SiemensCoilSingle()
        {
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

            _siemensTcpMachine.GetAddresses = addresses;
            await _siemensTcpMachine.SetDatasAsync(MachineSetDataType.Address, new Dictionary<string, double>()
            {
                {
                    "Q 0.0", 1
                }
            });
            var ans = await _siemensTcpMachine.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans["Q 0.0"].PlcValue, 1);
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

            _siemensTcpMachine.GetAddresses = addresses;
            var ans = await _siemensTcpMachine.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans["I 0.0"].PlcValue, 0);
        }

        [TestMethod]
        public async Task SiemensMSingle()
        {
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

            _siemensTcpMachine.GetAddresses = addresses;

            await _siemensTcpMachine.SetDatasAsync(MachineSetDataType.Address, new Dictionary<string, double>()
            {
                {
                    "M 0", 31125
                }
            });
            var ans = await _siemensTcpMachine.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans["M 0.0"].PlcValue, 31125);
        }

        [TestMethod]
        public async Task SiemensMSingleBool()
        {
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

            _siemensTcpMachine.GetAddresses = addresses;

            await _siemensTcpMachine.SetDatasAsync(MachineSetDataType.Address, new Dictionary<string, double>()
            {
                {
                    "M 0.0", 1
                }
            });
            var ans = await _siemensTcpMachine.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans["M 0.0"].PlcValue, 1);
        }

        [TestMethod]
        public async Task SiemensDbSingle()
        {
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

            _siemensTcpMachine.GetAddresses = addresses;
            await _siemensTcpMachine.SetDatasAsync(MachineSetDataType.Address, new Dictionary<string, double>()
            {
                {
                    "DB2 0.0", 31125
                }
            });
            var ans = await _siemensTcpMachine.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans["DB2 0.0"].PlcValue, 31125);
        }

        [TestMethod]
        public async Task SiemensDbMultiple()
        {
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

            _siemensTcpMachine.GetAddresses = addresses;
            await _siemensTcpMachine.SetDatasAsync(MachineSetDataType.CommunicationTag, new Dictionary<string, double>()
            {
                {
                    "A1", 70
                },
                {
                    "A2", 71
                },
                {
                    "A3", 72
                },
                {
                    "A4", 73
                },
                {
                    "A5", 717870
                },
                {
                    "A6", 717871
                },
            });
            var ans = await _siemensTcpMachine.GetDatasAsync(MachineGetDataType.CommunicationTag);
            Assert.AreEqual(ans["A1"].PlcValue, 70);
            Assert.AreEqual(ans["A2"].PlcValue, 71);
            Assert.AreEqual(ans["A3"].PlcValue, 72);
            Assert.AreEqual(ans["A4"].PlcValue, 73);
            Assert.AreEqual(ans["A5"].PlcValue, 717870);
            Assert.AreEqual(ans["A6"].PlcValue, 717871);
        }
    }
}
