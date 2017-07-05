using System.Windows.Controls;
using SlideMeisterLib.Model;

namespace SlideMeister.ViewModels
{
    public class ItemView
    {
        public OverlayItem Item { get; set; }
        public Image Image { get; set; }

        public Button StateButton { get; set; }
    }
}