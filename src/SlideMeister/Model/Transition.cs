namespace SlideMeister.Model
{
    public class Transition
    {
        public OverlayItem Item { get; set; }

        public OverlayState State { get; set; }

        public Transition(OverlayItem item, OverlayState state)
        {
            Item = item;
            State = state;
        }
    }
}