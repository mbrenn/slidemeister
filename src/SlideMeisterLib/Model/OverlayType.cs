﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using SlideMeisterLib.Annotations;

namespace SlideMeisterLib.Model
{
    public class OverlayType : INotifyPropertyChanged
    {
        private OverlayState _defaultState;

        public string Name { get; set; }

        public List<OverlayState> States { get; set; } = new List<OverlayState>();

        public OverlayType(string name)
        {
            Name = name;
        }

        public void AddState(OverlayState state)
        {
            States.Add(state);
        }

        public OverlayState DefaultState
        {
            get => _defaultState ?? States.FirstOrDefault();
            set => _defaultState = value;
        }

        public override string ToString()
        {
            return $"{Name}";
        }

        public OverlayState GetNextState(OverlayState itemCurrentState)
        {
            var pos = States.IndexOf(itemCurrentState);
            if (pos == -1 || pos == States.Count - 1)
            {
                pos = -1;
            }

            return States[pos + 1];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}