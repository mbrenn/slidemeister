using System.Collections.Generic;

namespace SlideMeister.Model
{
    /// <summary>
    /// Defines the machine which contains items with different states
    /// </summary>
    public class Machine
    {
        /// <summary>
        /// Gets or sets the name of the machine
        /// </summary>
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