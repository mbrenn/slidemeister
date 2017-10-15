using System.ComponentModel;
using System.Runtime.CompilerServices;
using SlideMeister.Annotations;
using SlideMeister.Helper;
using SlideMeisterLib.Logic;
using SlideMeisterLib.Model;

namespace SlideMeister.Model
{
    public class TransitionInfo : INotifyPropertyChanged
    {
        private readonly TransitionSet _transitionSet;

        public TransitionInfo(MainWindow window, TransitionSet transitionSet)
        {
            _transitionSet = transitionSet;

            SwitchTo = new ActionCommand(() =>
            {
                var logic = new MachineLogic(window.Machine);
                logic.ApplyTransition(_transitionSet);
                window.SlideCanvas.UpdateStates();
            });

            _transitionSet.PropertyChanged += (x, y) =>
            {
                if (y.PropertyName == "Name")
                {
                    OnPropertyChanged1(nameof(Name));
                }
            };
        }

        public string Name => _transitionSet.Name;

        public ActionCommand SwitchTo { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged1([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}