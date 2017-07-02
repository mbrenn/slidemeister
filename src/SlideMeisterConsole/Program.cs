using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlideMeister.Model;

namespace SlideMeisterConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var machine = new Machine();
           
            var led= new OverlayType("LED");
            led.AddState(new OverlayState("On", "on.png"));
            led.AddState(new OverlayState("Off", "off.png"));

            var firstLed = new OverlayItem(led);
            var secondLed = new OverlayItem(led);

            machine.AddItem(firstLed);
            machine.AddItem(secondLed);
        }
    }
}
