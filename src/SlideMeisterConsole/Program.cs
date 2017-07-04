using System;
using SlideMeisterLib.Logic;
using SlideMeisterLib.Model;

namespace SlideMeisterConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var machine = new Machine();

            var led = new OverlayType("LED");
            var onState = new OverlayState("On", "on.png");
            var offState = new OverlayState("Off", "off.png");
            led.AddState(onState);
            led.AddState(offState);

            var firstLed = new OverlayItem(led);
            var secondLed = new OverlayItem(led);

            machine.AddItem(firstLed);
            machine.AddItem(secondLed);

            var sequence = new TransitionSequence();
            sequence.Steps.Add(
                new TransitionSequenceStep(
                    new Transition(firstLed, onState),
                    new Transition(secondLed, offState)));
            sequence.Steps.Add(
                new TransitionSequenceStep(
                    new Transition(firstLed, offState),
                    new Transition(secondLed, onState)));

            var transitionLogic = new TransitionNavigation(machine, sequence);
            transitionLogic.Initialize();

            Console.WriteLine("First:");
            Console.WriteLine(machine.ConvertToString());

            transitionLogic.NavigateToNext();
            Console.WriteLine("Next:");
            Console.WriteLine(machine.ConvertToString());

            transitionLogic.NavigateToPrevious();
            Console.WriteLine("First again:");
            Console.WriteLine(machine.ConvertToString());

            Console.ReadKey();
        }
    }
}
