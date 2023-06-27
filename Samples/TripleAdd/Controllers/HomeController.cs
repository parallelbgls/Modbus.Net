using Microsoft.AspNetCore.Mvc;
using Modbus.Net;
using Modbus.Net.Modbus;
using System.Diagnostics;
using TripleAdd.Models;

namespace TripleAdd.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        private static IUtility? utility;
        private static BaseMachine<string, string>? machine;

        public ActionResult Index()
        {
            return RedirectToAction("Machine");
        }

        public async Task<ActionResult> Machine()
        {
            var data = await GetMachineEnter();
            ViewBag.Method = "SetMachine";
            return View("Index", data);
        }

        public async Task<ActionResult> Utility()
        {
            var data = await GetUtilityEnter();
            ViewBag.Method = "SetUtility";
            return View("Index", data);
        }

        private async Task<TripleAddViewModel> GetUtilityEnter()
        {
            if (utility == null)
            {
                utility = new ModbusUtility(ModbusType.Tcp, "10.10.18.251", 2, 0);
                utility.AddressTranslator = new AddressTranslatorModbus();
                await utility.ConnectAsync();
            }
            object[] getNum = (await utility.GetDatasAsync("4X 1", new KeyValuePair<Type, int>(typeof(ushort), 4))).Datas;
            ushort[] getNumUshorts = BigEndianLsbValueHelper.Instance.ObjectArrayToDestinationArray<ushort>(getNum);
            return SetValue(getNumUshorts);
        }

        private async Task<TripleAddViewModel> GetMachineEnter()
        {
            if (machine == null)
            {
                machine = new ModbusMachine<string, string>("1", ModbusType.Tcp, "10.10.18.251", new List<AddressUnit>()
                {
                    new AddressUnit() {Id = "1", Area = "4X", Address = 1, CommunicationTag = "Add1", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                    new AddressUnit() {Id = "2", Area = "4X", Address = 2, CommunicationTag = "Add2", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                    new AddressUnit() {Id = "3", Area = "4X", Address = 3, CommunicationTag = "Add3", DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                    new AddressUnit() {Id = "4", Area = "4X", Address = 4, CommunicationTag = "Ans",  DataType = typeof(ushort), Zoom = 1, DecimalPos = 0},
                }, 2, 0);
                machine.AddressCombiner = new AddressCombinerContinus<string>(machine.AddressTranslator, 100000);
                machine.AddressCombinerSet = new AddressCombinerContinus<string>(machine.AddressTranslator, 100000);
            }
            var resultFormat = (await machine.GetDatasAsync(MachineDataType.CommunicationTag)).Datas.MapGetValuesToSetValues();
            return SetValue(new ushort[4] { (ushort)resultFormat["Add1"], (ushort)resultFormat["Add2"], (ushort)resultFormat["Add3"], (ushort)resultFormat["Ans"] });
        }

        private TripleAddViewModel SetValue(ushort[] getNum)
        {
            return new TripleAddViewModel()
            {
                Add1 = getNum[0],
                Add2 = getNum[1],
                Add3 = getNum[2],
                Ans = getNum[3],
            };
        }

        [HttpPost]
        public async Task<ActionResult> SetUtility(TripleAddViewModel model)
        {
            ushort add1 = model.Add1, add2 = model.Add2, add3 = model.Add3;
            await utility!.SetDatasAsync("4X 1", new object[] { add1, add2, add3 });
            return RedirectToAction("Utility");
        }

        [HttpPost]
        public async Task<ActionResult> SetMachine(TripleAddViewModel model)
        {
            ushort add1 = model.Add1, add2 = model.Add2, add3 = model.Add3;
            var setDic = new Dictionary<string, double> { { "Add1", add1 }, { "Add2", add2 }, { "Add3", add3 } };
            await machine!.SetDatasAsync(MachineDataType.CommunicationTag, setDic);
            return RedirectToAction("Machine");
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