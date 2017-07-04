using System.Collections.Generic;

namespace SlideMeister.Model
{
    public class TransitionSet
    {
        /// <summary>
        /// Gets or sets the name 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the list of transition for the transition set. Each of the transition will be
        /// applied to the machine
        /// </summary>
        public List<Transition> Transitions { get; set; } = new List<Transition>();

        public override string ToString()
        {
            return Name;
        }
    }
}