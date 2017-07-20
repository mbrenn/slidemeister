using System.ComponentModel;
using System.Runtime.CompilerServices;
using SlideMeisterLib.Annotations;

namespace SlideMeisterLib.Model
{
    public class Transition : INotifyPropertyChanged
    {
        private OverlayItem _item;
        private OverlayState _state;

        public OverlayItem Item
        {
            get => _item;
            set
            {
                if (Equals(value, _item)) return;
                _item = value;
                OnPropertyChanged();
            }
        }

        public OverlayState State
        {
            get => _state;
            set
            {
                if (Equals(value, _state)) return;
                _state = value;
                OnPropertyChanged();
            }
        }

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