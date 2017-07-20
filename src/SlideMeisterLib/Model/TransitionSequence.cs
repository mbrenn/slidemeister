using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SlideMeisterLib.Annotations;

namespace SlideMeisterLib.Model
{
    public class TransitionSequence : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public List<TransitionSequenceStep> Steps { get; set; }
            = new List<TransitionSequenceStep>();

        public override string ToString()
        {
            return Name;
        }

        public TransitionSequence()
        {
        }

        public TransitionSequence(string name)
        {
            Name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}