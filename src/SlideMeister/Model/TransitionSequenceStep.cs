using System;

namespace SlideMeister.Model
{
    public class TransitionSequenceStep
    {
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(1.0);

        public TransitionSet Transitions { get; set; }

        public override string ToString()
        {
            return $"Transition Step with duration: {Duration.TotalMilliseconds} ms";
        }
    }
}