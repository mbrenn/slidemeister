namespace SlideMeister.Model
{
    public class OverlayItem
    {
        public string Name { get; set; }

        public Rectangle Position { get; set; }

        public OverlayType Type { get; set; }

        public OverlayState CurrentState { get; set; }

        public OverlayItem(OverlayType type)
        {
            Type = type;
            CurrentState = Type.DefaultState;
        }

        public override string ToString()
        {
            return $"{Name} of Type {Type.Name} ({CurrentState})";
        }
    }
}