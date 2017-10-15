using System.ComponentModel;
using System.Runtime.CompilerServices;
using SlideMeister.Annotations;
using SlideMeister.Helper;
using SlideMeister.ViewModels;

namespace SlideMeister.Model
{
    public class StateInfo : INotifyPropertyChanged
    {
        private readonly ItemView _view;

        public StateInfo(MainWindow window, ItemView view)
        {
            _view = view;
            NextState = new ActionCommand(() =>
            {
                _view.Item.CurrentState = _view.Item.Type.GetNextState(_view.Item.CurrentState);

                window.SlideCanvas.UpdateState(_view);
            });

            _view.Item.PropertyChanged += (x, y) =>
            {
                if (y.PropertyName == "CurrentState")
                {
                    OnPropertyChanged1(nameof(State));
                }
            };

            _view.Item.PropertyChanged += (x, y) =>
            {
                if (y.PropertyName == "Name")
                {
                    OnPropertyChanged1(nameof(Name));
                }
            };
        }

        public string Name => _view?.Item?.Name;

        public string State => _view?.Item?.CurrentState?.Name;

        public ActionCommand NextState { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged1([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}