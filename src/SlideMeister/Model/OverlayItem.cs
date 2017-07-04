namespace SlideMeister.Model
{
    public class OverlayItem
    {
        /// <summary>
        /// Gets the name of the overlayitem
        /// </summary>
        public string Name { get; set; }

        public Rectangle Position { get; set; }

        public OverlayType Type { get; set; }

        public OverlayState CurrentState { get; set; }

        /// <summary>
        /// Initializes a new instance of the OverlayItem
        /// </summary>
        /// <param name="type"></param>
        public OverlayItem(OverlayType type)
        {
            Type = type;
            CurrentState = Type.DefaultState;
        }

        public override string ToString()
        {
            return $"{Name} of Type {Type.Name} ({CurrentState.Name})";
        }
    }
}