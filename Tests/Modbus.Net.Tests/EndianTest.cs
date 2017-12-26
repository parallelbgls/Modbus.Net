using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Modbus;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class EndianTest
    {
        private BaseMachine _modbusTcpMachine;

        private BaseMachine _modbusTcpMachine2;

        [TestInitialize]
        public void Init()
        {
            _modbusTcpMachine = new ModbusMachine("1", ModbusType.Tcp, "127.0.0.1", null, true, 1, 0);

            _modbusTcpMachine2 = new ModbusMachine("2", ModbusType.Tcp, "127.0.0.1", null, true, 1, 0, Endian.LittleEndianLsb);
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

            _modbusTcpMachine.GetAddresses = addresses;
            _modbusTcpMachine2.GetAddresses = addresses;
            await _modbusTcpMachine.SetDatasAsync(MachineSetDataType.Address, dic1);
            var ans = await _modbusTcpMachine.GetDatasAsync(MachineGetDataType.Address);
            var ans2 = await _modbusTcpMachine2.GetDatasAsync(MachineGetDataType.Address);
            Assert.AreEqual(ans["4X 1.0"].PlcValue, dic1["4X 1"]);
            Assert.AreEqual(ans2["4X 1.0"].PlcValue, (ushort)dic1["4X 1"] % 256 * 256 + (ushort)dic1["4X 1"] / 256);
        }
    }
}
