using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SlideMeisterLib.Annotations;

namespace SlideMeisterLib.Model
{
    /// <summary>
    /// Defines the machine which contains items with different states
    /// </summary>
    public class Machine : INotifyPropertyChanged
    {
        private string _name;
        private string _backgroundImageUrl;
        private string _version;
        private List<OverlayItem> _items = new List<OverlayItem>();
        private List<TransitionSet> _transitions = new List<TransitionSet>();
        private List<TransitionSequence> _sequences = new List<TransitionSequence>();

        /// <summary>
        /// Gets or sets the name of the machine
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

        /// <summary>
        /// Gets or sets the url of the background image
        /// </summary>
        public string BackgroundImageUrl
        {
            get => _backgroundImageUrl;
            set
            {
                if (value == _backgroundImageUrl) return;
                _backgroundImageUrl = value;
                OnPropertyChanged();
            }
        }

        public string Version
        {
            get => _version;
            set
            {
                if (value == _version) return;
                _version = value;
                OnPropertyChanged();
            }
        }

        public List<OverlayItem> Items
        {
            get => _items;
            set
            {
                if (Equals(value, _items)) return;
                _items = value;
                OnPropertyChanged();
            }
        }

        public List<TransitionSequence> Sequences
        {
            get => _sequences;
            set
            {
                if (Equals(value, _sequences)) return;
                _sequences = value;
                OnPropertyChanged();
            }
        }

        public List<TransitionSet> Transitions
        {
            get => _transitions;
            set
            {
                if (Equals(value, _transitions)) return;
                _transitions = value;
                OnPropertyChanged();
            }
        }

        public void AddItem(OverlayItem item)
        {
            Items.Add(item);
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