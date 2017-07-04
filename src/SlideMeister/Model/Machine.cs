using System.Collections.Generic;

namespace SlideMeister.Model
{
    public class Machine
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public List<OverlayItem> Items { get; set; } = new List<OverlayItem>();

        public List<TransitionSequence> Sequences { get; set; } = new List<TransitionSequence>();

        public void AddItem(OverlayItem item)
        {
            Items.Add(item);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}