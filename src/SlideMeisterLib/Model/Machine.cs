using System.Collections.Generic;

namespace SlideMeisterLib.Model
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

        /// <summary>
        /// Gets or sets the url of the background image
        /// </summary>
        public string BackgroundImageUrl { get; set; }

        public string Version { get; set; }

        public List<OverlayItem> Items { get; set; } = new List<OverlayItem>();

        public List<TransitionSequence> Sequences { get; set; } = new List<TransitionSequence>();

        public List<TransitionSet> Transitions { get; set; } = new List<TransitionSet>();

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