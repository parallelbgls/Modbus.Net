using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public enum LightLamp{Red,Yellow,Green}
namespace CrossLampControl.WebApi.Models
{
    public class Lamp
    {
        public string MainLamp { get; set; }
        public string SubLamp { get; set; }

        public Lamp()
        {
            MainLamp = LightLamp.Red.ToString();
            SubLamp = LightLamp.Red.ToString();
        }

        public Lamp(LightLamp mLamp, LightLamp sLamp)
        {
            MainLamp = mLamp.ToString();
            SubLamp = sLamp.ToString();
        }
    }
}