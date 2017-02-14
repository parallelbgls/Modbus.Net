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

        [TestInitialize]
        public void Init()
        {
            _modbusTcpMachine = new ModbusMachine(ModbusType.Tcp, "192.168.3.10", null, true, 2, 0);
        }

        [TestMethod]
        public async Task ModbusCoilSingle()
        {
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

            _modbusTcpMachine.GetAddresses = addresses;
            await _modbusTcpMachine.SetDatasAsync(MachineSetDataType.Address, new Dictionary<string, double>()
            {
                {
                    "0X 1.0", 1
                }
            });
            var ans = await _modbusTcpMachine.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans["0X 1.0"].PlcValue, 1);
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
            var ans = await _modbusTcpMachine.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans["1X 1.0"].PlcValue, 0);
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
            var ans = await _modbusTcpMachine.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans["3X 1.0"].PlcValue, 0);
        }

        [TestMethod]
        public async Task ModbusRegSingle()
        {
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

            _modbusTcpMachine.GetAddresses = addresses;
            await _modbusTcpMachine.SetDatasAsync(MachineSetDataType.Address, new Dictionary<string, double>()
            {
                {
                    "4X 1", 31125
                }
            });
            var ans = await _modbusTcpMachine.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans["4X 1.0"].PlcValue, 31125);
        }

        [TestMethod]
        public async Task ModbusRegMultiple()
        {
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

            _modbusTcpMachine.GetAddresses = addresses;
            await _modbusTcpMachine.SetDatasAsync(MachineSetDataType.CommunicationTag, new Dictionary<string, double>()
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
            var ans = await _modbusTcpMachine.GetDatasAsync(MachineGetDataType.CommunicationTag);
            Assert.AreEqual(ans["A1"].PlcValue, 70);
            Assert.AreEqual(ans["A2"].PlcValue, 71);
            Assert.AreEqual(ans["A3"].PlcValue, 72);
            Assert.AreEqual(ans["A4"].PlcValue, 73);
            Assert.AreEqual(ans["A5"].PlcValue, 717870);
            Assert.AreEqual(ans["A6"].PlcValue, 717871);
        }
    }
}
