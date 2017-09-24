using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SlideMeister.Control;
using SlideMeisterLib.Model;

namespace SlideMeister.ViewModels
{
    public class UiElementFactory
    {
        /// <summary>
        /// Gets or sets the overlay state of the ui factory being used to create the element
        /// </summary>
        private OverlayState _state;

        public BitmapImage _bitmapImage;

        /// <summary>
        /// Gets the native size of the element according to the definition and/or image. 
        /// This information is used, when there is no explicit information in the state
        /// </summary>
        public Size NativeSize { get; }

        public UiElementFactory(OverlayState state)
        {
            _state = state;
            var path = System.IO.Path.Combine(Environment.CurrentDirectory, _state.ImageUrl);
            _bitmapImage = new BitmapImage(new Uri(path));

            NativeSize = new Size(
                _bitmapImage.PixelWidth, _bitmapImage.PixelHeight);
        }

        public UIElement CreateUiElement(SlideControl element)
        {
            var image = new Image { Source = _bitmapImage };
            return image;
        }

    }
}