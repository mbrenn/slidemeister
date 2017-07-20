using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SlideMeisterLib.Annotations;

namespace SlideMeisterLib.Model
{
    public class TransitionSet : INotifyPropertyChanged
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
        /// <param name="name">Name of the transition set</param>
        /// <param name="transitions">Array for transitions</param>
        public TransitionSet(string name, params Transition[] transitions)
        {
            Name = name;
            foreach (var transition in transitions)
            {
                Transitions.Add(transition);
            }
        }


        public override string ToString()
        {
            return Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}