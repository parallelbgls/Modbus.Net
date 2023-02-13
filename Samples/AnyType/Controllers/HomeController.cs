using AnyType.Models;
using Microsoft.AspNetCore.Mvc;
using Modbus.Net;
using Modbus.Net.Modbus;
using System.Diagnostics;

namespace AnyType.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private static IEnumerable<TaskViewModel> values = new List<TaskViewModel>();
        private static bool taskStart = false;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<ActionResult> Index()
        {
            if (taskStart == false)
            {
                await StartTask();
            }
            return View(values.ToList());
        }

        private async Task StartTask()
        {

            //增加需要通信的PLC地址
            List<AddressUnit> addressUnits = new List<AddressUnit>
        {
            new AddressUnit() {Id = "d0", Name="Variable 1", Area = "4X", Address = 1, CommunicationTag = "D1", DataType = typeof (ushort), Zoom = 0.01, DecimalPos = 2},
            new AddressUnit() {Id = "d1", Name="Variable 2", Area = "4X", Address = 2, SubAddress = 0, CommunicationTag = "D2", DataType = typeof (bool)},
            new AddressUnit() {Id = "d2", Name="Variable 3", Area = "4X", Address = 2, SubAddress = 1, CommunicationTag = "D3", DataType = typeof (bool)},
            new AddressUnit() {Id = "d3", Name="Variable 4", Area = "4X", Address = 2, SubAddress = 8, CommunicationTag = "D4", DataType = typeof (byte)},
            new AddressUnit() {Id = "d4", Name="Variable 5", Area = "4X", Address = 3, CommunicationTag = "D5", DataType = typeof (int)},
            new AddressUnit() {Id = "d5", Name="Variable 6", Area = "4X", Address = 5, CommunicationTag = "D6", Zoom=0.1, DecimalPos = 1, DataType = typeof (float)},
            new AddressUnit() {Id = "d6", Name="Variable 7", Area = "4X", Address = 7, CommunicationTag = "D7", DecimalPos = 1, DataType = typeof (float)},
            new AddressUnit() {Id = "d7", Name="Variable 8", Area = "4X", Address = 9, CommunicationTag = "D8", DataType = typeof (short)},
        };

            values = from unitValue in addressUnits
                        select
                            new TaskViewModel()
                            {
                                Id = unitValue.Id,
                                Name = unitValue.Name,
                                Address = unitValue.Address + "." + unitValue.SubAddress,
                                Value = 0,
                                Type = unitValue.DataType.Name
                            };
            var machine = new ModbusMachine("1", ModbusType.Tcp, "192.168.0.172:502", addressUnits, true, 2, 0);
            //启动任务
            await MachineJobSchedulerCreator.CreateScheduler("Trigger1", -1, 1).Result.From(machine.Id, machine, MachineDataType.CommunicationTag).Result.Query("Query1",
                returnValues =>
                {
                    //唯一的参数包含返回值，是一个唯一标识符（machine的第二个参数），返回值（类型ReturnUnit）的键值对。
                    if (returnValues.ReturnValues != null)
                    {
                        lock (values)
                        {
                            var unitValues = from val in returnValues.ReturnValues
                                             select
                                             new Tuple<AddressUnit, double?>(
                                                 addressUnits.FirstOrDefault(p => p.CommunicationTag == val.Key)!, val.Value.DeviceValue);
                            values = from unitValue in unitValues
                                     select
                                     new TaskViewModel()
                                     {
                                         Id = unitValue.Item1.Id,
                                         Name = unitValue.Item1.Name,
                                         Address = unitValue.Item1.Address + "." + unitValue.Item1.SubAddress,
                                         Value = unitValue.Item2 ?? 0,
                                         Type = unitValue.Item1.DataType.Name
                                     };
                        }
                    }
                    else
                    {
                        _logger.LogError($"ip {returnValues.MachineId} not return value");
                    }
                    return null;
                }).Result.Run();
            taskStart = true;
        }

        [HttpGet]
        public JsonResult Get()
        {
            List<TaskViewModel> ans;
            lock (values)
            {
                ans = values.ToList();
            }
            return Json(ans);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}