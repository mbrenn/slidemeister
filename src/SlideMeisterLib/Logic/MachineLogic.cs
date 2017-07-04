using System.Diagnostics;
using System.Linq;
using SlideMeisterLib.Model;

namespace SlideMeisterLib.Logic
{
    public class MachineLogic
    {
        /// <summary>
        /// Gets the machine to be handled by the machine logic
        /// </summary>
        private readonly Machine _machine;

        public MachineLogic(Machine machine)
        {
            _machine = machine;
        }

        /// <summary>
        /// Applies the given transition to the machine
        /// </summary>
        /// <param name="transition">Transition to be applied</param>
        public void ApplyTransition(Transition transition)
        {
            var foundItem = _machine.Items.FirstOrDefault(x => x == transition.Item);
            if (foundItem == null)
            {
                Debug.WriteLine("ApplyTransition: Item not found");
                return;
            }

            foundItem.CurrentState = transition.State;
        }
        
        /// <summary>
        /// Applies the complete transition set to the given machine
        /// </summary>
        /// <param name="set">The transition to be set</param>
        public void ApplyTransition(TransitionSet set)
        {
            foreach (var transition in set.Transitions)
            {
                ApplyTransition(transition);
            }
            
        }
    }
}