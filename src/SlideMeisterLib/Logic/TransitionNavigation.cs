using System;
using System.Linq;
using SlideMeisterLib.Model;

namespace SlideMeisterLib.Logic
{
    public class TransitionNavigation
    {
        /// <summary>
        /// Stores the machine to be applied
        /// </summary>
        private readonly Machine _machine;
    
    
        /// <summary>
        /// Stores the transitionsequence being applied to the machine
        /// </summary>
        private readonly TransitionSequence _sequence;

        /// <summary>
        /// Stores the machine logic
        /// </summary>
        private readonly MachineLogic _machineLogic;

        /// <summary>
        /// Gets the current step of the transitions
        /// </summary>
        public TransitionSequenceStep CurrentStep { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TransitionNavigation
        /// </summary>
        /// <param name="machine">Machine to be set</param>
        /// <param name="sequence">The sequence being used to navigate</param>
        public TransitionNavigation(Machine machine, TransitionSequence sequence)
        {
            _machine = machine;
            _sequence = sequence;

            _machineLogic = new MachineLogic(_machine);
        }

        /// <summary>
        /// Initializes the machine by its default values
        /// </summary>
        public void Initialize()
        {
            // Goes through each item to initialize the machine and the current step
            foreach (var item in _machine.Items)
            {
                item.CurrentState = item.DefaultState;
            }

            // Switches to the first item and applies the transition
            CurrentStep = _sequence.Steps.FirstOrDefault();
            if (CurrentStep == null)
            {
                throw new InvalidOperationException("The given sequence has no steps, so navigation is not possible.");
            }

            foreach (var transition in CurrentStep.Transitions)
            {
                _machineLogic.ApplyTransition(transition);
            }
        }

        /// <summary>
        /// Sets the next step
        /// </summary>
        public bool NavigateToNext()
        {
            var pos = _sequence.Steps.IndexOf(CurrentStep);
            if (pos == -1)
            {
                pos = 0;
            }
            else
            {

                if (pos >= _sequence.Steps.Count - 1)
                {
                    return false;
                }

                pos++;
            }


            CurrentStep = _sequence.Steps[pos];

            foreach (var transition in CurrentStep.Transitions)
            {
                _machineLogic.ApplyTransition(transition);
            }

            return true;
        }


        /// <summary>
        /// Sets the previous step
        /// </summary>
        public bool NavigateToPrevious()
        {
            var pos = _sequence.Steps.IndexOf(CurrentStep);
            if (pos == -1)
            {
                pos = 0;
            }
            else
            {
                if (pos <= 0)
                {
                    return false;
                }

                pos--;
            }

            CurrentStep = _sequence.Steps[pos];

            foreach (var transition in CurrentStep.Transitions)
            {
                _machineLogic.ApplyTransition(transition);
            }

            return true;

        }
    }
}