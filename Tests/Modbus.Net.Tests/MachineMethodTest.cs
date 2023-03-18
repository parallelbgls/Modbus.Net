using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Modbus;
using System.Reflection;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class MachineMethodTest
    {
        [TestMethod]
        public void GetUtility()
        {
            BaseMachine<int, int> baseMachine = new ModbusMachine<int, int>(1, ModbusType.Tcp, "192.168.3.12", null, true, 2, 0);
            var utility = baseMachine.GetUtility<IUtilityMethodTime>();
            var methods = utility.GetType().GetRuntimeMethods();
            Assert.AreEqual(methods.FirstOrDefault(method => method.Name == "GetTimeAsync") != null, true);
            Assert.AreEqual(methods.FirstOrDefault(method => method.Name == "SetTimeAsync") != null, true);
            baseMachine.Disconnect();
        }

        [TestMethod]
        public async Task InvokeUtility()
        {
            BaseMachine<int, int> baseMachine = new ModbusMachine<int, int>(1, ModbusType.Tcp, "192.168.3.12", null, true, 2, 0);
            var success = await baseMachine.BaseUtility.GetUtilityMethods<IUtilityMethodTime>().SetTimeAsync(DateTime.Now);
            Assert.AreEqual(success, true);
            var time = await baseMachine.BaseUtility.GetUtilityMethods<IUtilityMethodTime>().GetTimeAsync();
            Assert.AreEqual((time.Datas.ToUniversalTime() - DateTime.Now.ToUniversalTime()).Seconds < 10, true);
            baseMachine.Disconnect();
        }

        [TestMethod]
        public async Task InvokeMachine()
        {
            BaseMachine<int, int> baseMachine = new ModbusMachine<int, int>(1, ModbusType.Tcp, "192.168.3.10", new List<AddressUnit<int>>
            {
                new AddressUnit<int>
                {
                    Id = 0,
                    Area = "0X",
                    Address = 1,
                    SubAddress = 0,
                    CommunicationTag = "A1",
                    DataType = typeof(bool)
                }
            }, true, 2, 0);
            var success = await baseMachine.GetMachineMethods<IMachineMethodData>().SetDatasAsync(
                MachineDataType.Address,
                new Dictionary<string, double>
                {
                    {
                        "0X 1.0", 1
                    }
                });
            Assert.AreEqual(success, true);
            var datas = await baseMachine.GetMachineMethods<IMachineMethodData>().GetDatasAsync(MachineDataType.Address);
            Assert.AreEqual(datas.Datas["0X 1.0"].DeviceValue, 1);
            success = await baseMachine.GetMachineMethods<IMachineMethodData>().SetDatasAsync(
                MachineDataType.Address,
                new Dictionary<string, double>
                {
                    {
                        "0X 1.0", 0
                    }
                });
            Assert.AreEqual(success, true);
            baseMachine.Disconnect();
        }
    }
}
