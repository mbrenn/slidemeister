namespace SlideMeister.Model
{
    public class OverlayState
    {
        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public OverlayState(string name, string imageUrl = "")
        {
            Name = name;
            ImageUrl = imageUrl;
        }
    }
}