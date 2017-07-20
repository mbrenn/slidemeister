using System.ComponentModel;
using System.Runtime.CompilerServices;
using SlideMeisterLib.Annotations;

namespace SlideMeisterLib.Model
{
    public class OverlayState : INotifyPropertyChanged
    {
        private string _name;
        private string _imageUrl;

        /// <summary>
        /// Gets or sets the name of the state
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

        public string ImageUrl
        {
            get => _imageUrl;
            set
            {
                if (value == _imageUrl) return;
                _imageUrl = value;
                OnPropertyChanged();
            }
        }

        public OverlayState(string name, string imageUrl = "")
        {
            Name = name;
            ImageUrl = imageUrl;
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