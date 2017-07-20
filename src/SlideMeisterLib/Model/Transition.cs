using System.ComponentModel;
using System.Runtime.CompilerServices;
using SlideMeisterLib.Annotations;

namespace SlideMeisterLib.Model
{
    public class Transition : INotifyPropertyChanged
    {
        public OverlayItem Item { get; set; }

        public OverlayState State { get; set; }

        public Transition(OverlayItem item, OverlayState state)
        {
            Item = item;
            State = state;
        }

        public override string ToString()
        {
            return $"{Item.Name} -> {State.Name}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}