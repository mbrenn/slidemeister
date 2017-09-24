﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SlideMeisterLib.Annotations;

namespace SlideMeisterLib.Model
{
    public class TransitionSequenceStep : INotifyPropertyChanged
    {
        private string _name;
        private TimeSpan _duration;
        private TransitionSet _transitions;

        public string Name
        {
            get => _name;
            set
            {
                if (value.Equals(_name)) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the duration of the current step
        /// </summary>
        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (value.Equals(_duration)) return;
                _duration = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the transitions within the step
        /// </summary>
        public TransitionSet Transitions
        {
            get => _transitions;
            set
            {
                if (Equals(value, _transitions)) return;
                _transitions = value;
                OnPropertyChanged();
            }
        }

        public TransitionSequenceStep()
        {
            Duration = TimeSpan.FromSeconds(1.0);
        }

        public TransitionSequenceStep(TimeSpan duration)
        {
            Duration = duration;
        }

        public TransitionSequenceStep(string name, params Transition[] transition)
        {
            Name = name;
            Transitions = new TransitionSet(name, transition);
        }

        public TransitionSequenceStep(string name, TimeSpan duration, params Transition[] transition)
        {
            Name = name;
            Duration = duration;
            Transitions = new TransitionSet(name, transition);
        }

        public TransitionSequenceStep(string name, TimeSpan duration, TransitionSet set)
        {
            Name = name;
            Duration = duration;
            Transitions = set;
        }

        public override string ToString()
        {
            return $"Transition Step with duration: {Duration.TotalMilliseconds} ms";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}