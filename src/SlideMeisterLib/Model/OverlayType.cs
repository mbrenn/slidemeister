using System.Collections.Generic;
using System.Linq;

namespace SlideMeisterLib.Model
{
    public class OverlayType
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
    }
}