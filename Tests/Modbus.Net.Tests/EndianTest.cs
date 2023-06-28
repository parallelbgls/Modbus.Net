using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Modbus;
using AddressUnit = Modbus.Net.AddressUnit<string, int, int>;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class EndianTest
    {
        private BaseMachine<string, string>? _modbusTcpMachine;

        private BaseMachine<string, string>? _modbusTcpMachine2;

        private string _machineIp = "10.10.18.251";

        [TestInitialize]
        public void Init()
        {
            _modbusTcpMachine = new ModbusMachine<string, string>("1", ModbusType.Tcp, _machineIp, null, true, 1, 0, Endian.BigEndianLsb);

            _modbusTcpMachine2 = new ModbusMachine<string, string>("2", ModbusType.Tcp, _machineIp, null, true, 1, 0, Endian.LittleEndianLsb);
        }

        [TestMethod]
        public async Task ModbusEndianSingle()
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
            _modbusTcpMachine2!.GetAddresses = addresses;
            await _modbusTcpMachine.SetDatasAsync(MachineDataType.Address, dic1);
            var ans = await _modbusTcpMachine.GetDatasAsync(MachineDataType.Address);
            _modbusTcpMachine.Disconnect();
            var ans2 = await _modbusTcpMachine2.GetDatasAsync(MachineDataType.Address);
            Assert.AreEqual(ans.Datas["4X 1.0"].DeviceValue, dic1["4X 1"]);
            Assert.AreEqual(ans2.Datas["4X 1.0"].DeviceValue, (ushort)dic1["4X 1"] % 256 * 256 + (ushort)dic1["4X 1"] / 256);
        }
    }
}
