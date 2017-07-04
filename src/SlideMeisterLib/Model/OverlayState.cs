namespace SlideMeisterLib.Model
{
    public class OverlayState
    {
        /// <summary>
        /// Gets or sets the name of the state
        /// </summary>
        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public OverlayState(string name, string imageUrl = "")
        {
            Name = name;
            ImageUrl = imageUrl;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}