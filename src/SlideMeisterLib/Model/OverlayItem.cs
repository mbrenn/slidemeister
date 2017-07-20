using System.ComponentModel;
using System.Runtime.CompilerServices;
using SlideMeisterLib.Annotations;

namespace SlideMeisterLib.Model
{
    public class OverlayItem : INotifyPropertyChanged
    {
        private string _name;
        private Rectangle _position;
        private OverlayType _type;
        private OverlayState _currentState;

        /// <summary>
        /// Gets the name of the overlayitem
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public Rectangle Position
        {
            get => _position;
            set
            {
                if (Equals(value, _position)) return;
                _position = value;
                OnPropertyChanged();
            }
        }

        public OverlayType Type
        {
            get => _type;
            set
            {
                if (Equals(value, _type)) return;
                _type = value;
                OnPropertyChanged();
            }
        }

        public OverlayState CurrentState
        {
            get => _currentState;
            set
            {
                if (Equals(value, _currentState)) return;
                _currentState = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Defines the rotation of the elements
        /// </summary>
        public double Rotation { get; set; }

        /// <summary>
        /// Initializes a new instance of the OverlayItem
        /// </summary>
        /// <param name="type"></param>
        public OverlayItem(OverlayType type)
        {
            Type = type;
            CurrentState = Type.DefaultState;
        }

        public override string ToString()
        {
            return $"{Name} of Type {Type.Name} ({CurrentState.Name})";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}