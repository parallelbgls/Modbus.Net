using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Modbus;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class ModbusMultiStationTest
    {
        private BaseMachine _modbusRtuMachine1;

        private BaseMachine _modbusRtuMachine2;

        [TestInitialize]
        public void Init()
        {
            _modbusRtuMachine1 = new ModbusMachine(ModbusType.Rtu, "COM1", null, true, 1, 0);
            _modbusRtuMachine2 = new ModbusMachine(ModbusType.Rtu, "COM1", null, true, 2, 0);
        }

        [TestMethod]
        public async Task Get()
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

            _modbusRtuMachine1.GetAddresses = addresses.ToList();
            _modbusRtuMachine2.GetAddresses = addresses.ToList();

            await _modbusRtuMachine1.SetDatasAsync(MachineSetDataType.CommunicationTag, new Dictionary<string, double>()
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
            await _modbusRtuMachine2.SetDatasAsync(MachineSetDataType.CommunicationTag, new Dictionary<string, double>()
            {
                {
                    "A1", 74
                },
                {
                    "A2", 75
                },
                {
                    "A3", 76
                },
                {
                    "A4", 77
                },
                {
                    "A5", 717873
                },
                {
                    "A6", 717874
                },
            });
            var ans = await _modbusRtuMachine1.GetDatasAsync(MachineGetDataType.CommunicationTag);
            var ans2 = await _modbusRtuMachine2.GetDatasAsync(MachineGetDataType.CommunicationTag);

            _modbusRtuMachine1.Disconnect();
            _modbusRtuMachine2.Disconnect();

            Assert.AreEqual(ans["A1"].PlcValue, 70);
            Assert.AreEqual(ans2["A1"].PlcValue, 74);
        }
    }
}
