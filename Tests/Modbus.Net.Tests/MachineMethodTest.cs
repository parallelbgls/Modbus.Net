using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modbus.Net.Modbus;
using System.Reflection;

namespace Modbus.Net.Tests
{
    [TestClass]
    public class MachineMethodTest
    {
        private string _machineIp = "10.10.18.251";

        [TestMethod]
        public void GetUtility()
        {
            BaseMachine<int, int> baseMachine = new ModbusMachine<int, int>(1, ModbusType.Tcp, _machineIp, null, true, 2, 0);
            var utility = baseMachine.GetUtilityMethods<IUtilityMethodDatas>();
            var methods = utility.GetType().GetRuntimeMethods();
            Assert.AreEqual(methods.FirstOrDefault(method => method.Name == "GetDatasAsync") != null, true);
            Assert.AreEqual(methods.FirstOrDefault(method => method.Name == "SetDatasAsync") != null, true);
            baseMachine.Disconnect();
        }

        [TestMethod]
        public async Task InvokeUtility()
        {
            BaseMachine<int, int> baseMachine = new ModbusMachine<int, int>(1, ModbusType.Tcp, _machineIp, null, true, 2, 0);
            await baseMachine.BaseUtility.ConnectAsync();
            var success = await baseMachine.BaseUtility.GetUtilityMethods<IUtilityMethodDatas>().SetDatasAsync("4X 1", new object[] { (byte)11 });
            Assert.AreEqual(success.IsSuccess, true);
            var datas = await baseMachine.BaseUtility.GetUtilityMethods<IUtilityMethodDatas>().GetDatasAsync("4X 1", 1);
            Assert.AreEqual(datas.Datas[0], 11);
            baseMachine.Disconnect();
        }

        [TestMethod]
        public async Task InvokeMachine()
        {
            BaseMachine<int, int> baseMachine = new ModbusMachine<int, int>(1, ModbusType.Tcp, _machineIp, new List<AddressUnit<int>>
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
            var success = await baseMachine.GetMachineMethods<IMachineMethodDatas>().SetDatasAsync(
                MachineDataType.Address,
                new Dictionary<string, double>
                {
                    {
                        "0X 1.0", 1
                    }
                });
            Assert.AreEqual(success.IsSuccess, true);
            var datas = await baseMachine.GetMachineMethods<IMachineMethodDatas>().GetDatasAsync(MachineDataType.Address);
            Assert.AreEqual(datas.Datas["0X 1.0"].DeviceValue, 1);
            success = await baseMachine.GetMachineMethods<IMachineMethodDatas>().SetDatasAsync(
                MachineDataType.Address,
                new Dictionary<string, double>
                {
                    {
                        "0X 1.0", 0
                    }
                });
            Assert.AreEqual(success.IsSuccess, true);
            baseMachine.Disconnect();
        }
    }
}
