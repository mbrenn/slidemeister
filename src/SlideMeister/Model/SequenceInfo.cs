using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using SlideMeister.Annotations;
using SlideMeister.Helper;
using SlideMeisterLib.Logic;
using SlideMeisterLib.Model;

namespace SlideMeister.Model
{
    public class SequenceInfo: INotifyPropertyChanged
    {
        public TransitionSequence Sequence { get; }
        public TransitionNavigation Navigation { get; }

        public SequenceInfo(MainWindow window, TransitionSequence sequence)
        {
            Sequence = sequence;

            Navigation = new TransitionNavigation(window.Machine, Sequence);

            Initialize = new ActionCommand(() =>
            {
                Navigation.Initialize();
                OnPropertyChanged1(nameof(Transition));
                window.SlideCanvas.UpdateStates();
            });

            Previous = new ActionCommand(() =>
            {
                Navigation.NavigateToPrevious();
                OnPropertyChanged1(nameof(Transition));
                window.SlideCanvas.UpdateStates();
            });

            Next = new ActionCommand(() =>
            {
                Navigation.NavigateToNext();
                OnPropertyChanged1(nameof(Transition));
                window.SlideCanvas.UpdateStates();
            });

            Play = new ActionCommand(() =>
            {
                window.CurrentSequence?.Cancel();

                window.CurrentSequence = new CancellationTokenSource();
                try
                {
                    window.StartAutomaticSequence(this, window.CurrentSequence.Token);
                }
                catch (OperationCanceledException)
                {
                }
            });

            Sequence.PropertyChanged += (x, y) =>
            {
                if (y.PropertyName == "Name")
                {
                    OnPropertyChanged1(nameof(Name));
                }
            };
        }

        public string Name => Sequence.Name;

        public string Transition => Navigation?.CurrentStep == null ? "None" : Navigation.CurrentStep.Name;

        public ActionCommand Initialize { get; }
        public ActionCommand Previous { get; }
        public ActionCommand Next { get; }
        public ActionCommand Play { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged1([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Called, when an external event has performed a transition. 
        /// The PropertyChanged event is thrown
        /// </summary>
        public void TransitionOccured()
        {
            OnPropertyChanged1(nameof(Transition));
        }

        public override string ToString()
        {
            return Sequence.Name;
        }
    }
}