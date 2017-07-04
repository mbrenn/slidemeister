using System;

namespace SlideMeister.Model
{
    public class TransitionSequenceStep
    {
        /// <summary>
        /// Gets or sets the duration of the current step
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the transitions within the step
        /// </summary>
        public TransitionSet Transitions { get; set; }

        public TransitionSequenceStep()
        {
            Duration = TimeSpan.FromSeconds(1.0);
        }

        public TransitionSequenceStep(TimeSpan duration)
        {
            Duration = duration;
        }

        public TransitionSequenceStep(params Transition[] transition)
        {
            Transitions = new TransitionSet(transition);
        }

        public TransitionSequenceStep(TimeSpan duration, params Transition[] transition)
        {
            Duration = duration;
            Transitions = new TransitionSet(transition);
        }

        public TransitionSequenceStep(TimeSpan duration, TransitionSet set)
        {
            Duration = duration;
            Transitions = set;
        }

        public override string ToString()
        {
            return $"Transition Step with duration: {Duration.TotalMilliseconds} ms";
        }
    }
}