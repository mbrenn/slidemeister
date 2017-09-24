using System;
using System.Linq;
using SlideMeisterLib.Model;

namespace SlideMeisterLib.Logic
{
    public class Example
    {
        /// <summary>
        /// Creates an example machine
        /// </summary>
        /// <returns>Machine to be created</returns>
        public static Machine CreateMachine()
        {
            var machine = new Machine
            {
                BackgroundImageUrl = "examples/leds/leds.png",
                Name = "Two LEDs Preloaded",
                Version = "0.1"
            };

            var led = new OverlayType("LED");
            var onState = new OverlayState("On", "examples/leds/on.png");
            var offState = new OverlayState("Off", "examples/leds/off.png");
            led.AddState(onState);
            led.AddState(offState);

            var firstLed = new OverlayItem(led)
            {
                Name = "Top",
                Position = new Rectangle(0.3, 0.1, 0.4, 0.4)
            };

            var secondLed = new OverlayItem(led)
            {
                Name = "Bottom",
                Position = new Rectangle(0.3, 0.5, 0.4, 0.4),
                CurrentState = offState
            };

            machine.AddItem(firstLed);
            machine.AddItem(secondLed);

            var sequence = new TransitionSequence("Traffic Light");
            sequence.Steps.Add(
                new TransitionSequenceStep(
                    "Top",
                    TimeSpan.FromSeconds(1.0),
                    new Transition(firstLed, onState),
                    new Transition(secondLed, offState)));
            sequence.Steps.Add(
                new TransitionSequenceStep(
                    "Bottom",
                    TimeSpan.FromSeconds(1.0),
                    new Transition(firstLed, offState),
                    new Transition(secondLed, onState)));

            machine.Sequences.Add(sequence);

            foreach (var step in sequence.Steps)
            {
                machine.Transitions.Add(step.Transitions.First());
            }

            return machine;
        }
    }
}