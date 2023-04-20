using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Modbus;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class ModbusMultiStationTest
    {
        private BaseMachine<string, string>? _modbusRtuMachine1;

        private BaseMachine<string, string>? _modbusRtuMachine2;

        private string _machineCom = "COM1";

        [TestInitialize]
        public void Init()
        {
            _modbusRtuMachine1 = new ModbusMachine<string, string>("1", ModbusType.Rtu, _machineCom, null, true, 1, 0);
            _modbusRtuMachine2 = new ModbusMachine<string, string>("2", ModbusType.Rtu, _machineCom, null, true, 2, 0);
        }

        [TestMethod]
        public async Task MultiStation()
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

            _modbusRtuMachine1!.GetAddresses = addresses.ToList();
            _modbusRtuMachine2!.GetAddresses = addresses.ToList();

            Random r = new Random();
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

            var dic2 = new Dictionary<string, double>()
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

            await _modbusRtuMachine1.SetDatasAsync(MachineDataType.CommunicationTag, dic1);
            await _modbusRtuMachine2.SetDatasAsync(MachineDataType.CommunicationTag, dic2);

            var ans = await _modbusRtuMachine1.GetDatasAsync(MachineDataType.CommunicationTag);
            var ans2 = await _modbusRtuMachine2.GetDatasAsync(MachineDataType.CommunicationTag);

            _modbusRtuMachine1.Disconnect();
            _modbusRtuMachine2.Disconnect();

            Assert.AreEqual(ans.Datas["A1"].DeviceValue, dic1["A1"]);
            Assert.AreEqual(ans2.Datas["A1"].DeviceValue, dic2["A1"]);
            Assert.AreEqual(ans.Datas["A2"].DeviceValue, dic1["A2"]);
            Assert.AreEqual(ans2.Datas["A2"].DeviceValue, dic2["A2"]);
            Assert.AreEqual(ans.Datas["A3"].DeviceValue, dic1["A3"]);
            Assert.AreEqual(ans2.Datas["A3"].DeviceValue, dic2["A3"]);
            Assert.AreEqual(ans.Datas["A4"].DeviceValue, dic1["A4"]);
            Assert.AreEqual(ans2.Datas["A4"].DeviceValue, dic2["A4"]);
            Assert.AreEqual(ans.Datas["A5"].DeviceValue, dic1["A5"]);
            Assert.AreEqual(ans2.Datas["A5"].DeviceValue, dic2["A5"]);
            Assert.AreEqual(ans.Datas["A6"].DeviceValue, dic1["A6"]);
            Assert.AreEqual(ans2.Datas["A6"].DeviceValue, dic2["A6"]);
        }
    }
}
