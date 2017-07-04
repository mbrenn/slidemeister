using System.Collections.Generic;

namespace SlideMeisterLib.Model
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

        /// <summary>
        /// Initializes a new instance of the TransitionSet
        /// </summary>
        /// <param name="transitions">Array for transitions</param>
        public TransitionSet(params Transition[] transitions)
        {
            foreach (var transition in transitions)
            {
                Transitions.Add(transition);
            }
        }


        public override string ToString()
        {
            return Name;
        }
    }
}