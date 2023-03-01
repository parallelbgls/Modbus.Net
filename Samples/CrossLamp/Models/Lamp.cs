public enum LightLamp { Red, Yellow, Green }
namespace CrossLampControl.WebApi.Models
{
    public class Lamp
    {
        public string? MainLamp { get; set; }
        public string? SubLamp { get; set; }
        public string? StartPause { get; set; }

        public Lamp()
        {
            MainLamp = LightLamp.Red.ToString();
            SubLamp = LightLamp.Red.ToString();
        }

        public void SetLamp(LightLamp mLamp, LightLamp sLamp)
        {
            MainLamp = mLamp.ToString();
            SubLamp = sLamp.ToString();
        }

        public void SetStart(bool isStart)
        {
            StartPause = isStart ? "Start" : "Pause";
        }
    }
}