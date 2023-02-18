using CrossLamp.Models;
using CrossLampControl.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Modbus.Net;
using Modbus.Net.Modbus;
using System.Diagnostics;

namespace CrossLamp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        private static BaseUtility? _utility = null;

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult?> GetLamp()
        {
            try
            {
                if (_utility == null)
                {
                    _utility = new ModbusUtility(ModbusType.Tcp, "192.168.0.161", 2, 0);
                    await _utility.ConnectAsync();
                }
                Lamp light = new Lamp();
                object[] lampsbyte = await _utility.GetDatasAsync("0X 1", new KeyValuePair<Type, int>(typeof(bool), 7));
                bool[] lamps = BigEndianValueHelper.Instance.ObjectArrayToDestinationArray<bool>(lampsbyte);
                if (lamps[0])
                {
                    light.MainLamp = LightLamp.Red.ToString();
                }
                else if (lamps[1])
                {
                    light.MainLamp = LightLamp.Yellow.ToString();
                }
                else
                {
                    light.MainLamp = LightLamp.Green.ToString();
                }
                if (lamps[3])
                {
                    light.SubLamp = LightLamp.Red.ToString();
                }
                else if (lamps[4])
                {
                    light.SubLamp = LightLamp.Yellow.ToString();
                }
                else
                {
                    light.SubLamp = LightLamp.Green.ToString();
                }
                light.SetStart(lamps[6]);
                return Json(light);
            }
            catch (Exception)
            {
                return null;
            }
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