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
        /// Gets or sets the ui element hosting the item as a complete element
        /// </summary>
        public FrameworkElement UiElement { get; set; }

        /// <summary>
        /// Gets or sets the factory being used for the element
        /// </summary>
        public UiElementFactory Factory { get; set; }
    }
}