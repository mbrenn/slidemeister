using System.Windows;
using System.Windows.Controls;
using SlideMeisterLib.Model;

namespace SlideMeister.ViewModels
{
    /// <summary>
    /// Stores the information within the item
    /// </summary>
    public class ItemView
    {
        public OverlayItem Item { get; set; }

        /// <summary>
        /// Gets or sets the image being associated to the given item
        /// </summary>
        public Image Image { get; set; } 

        /// <summary>
        /// Gets or sets the ui element hosting the item as a complete element
        /// </summary>
        public UIElement UiElement { get; set; }

        public Button StateButton { get; set; }
    }
}