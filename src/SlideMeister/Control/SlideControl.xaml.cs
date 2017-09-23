using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SlideMeister.ViewModels;
using SlideMeisterLib.Model;

namespace SlideMeister.Control
{
    /// <summary>
    /// Interaktionslogik für SlideControl.xaml
    /// </summary>
    public partial class SlideControl : UserControl
    {
        public SlideControl()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Stores the machine to be shown
        /// </summary>
        public Machine Machine { get; set; }

        private double _ratioWidthToHeight;

        /// <summary>
        /// Stores a dictionary of images which are loaded by the states
        /// </summary>
        private readonly Dictionary<OverlayState, UiElementFactory> _imagesForStates = new Dictionary<OverlayState, UiElementFactory>();

        /// <summary>
        /// Stores the background image being used for the presentation
        /// </summary>
        private Image _backgroundImage;

        /// <summary>
        /// Gets the size of the background image
        /// </summary>
        public Size BackgroundSize => new Size(
            _backgroundImage.Width,
            _backgroundImage.Height);

        /// <summary>
        /// Gets or sets the size of the bitmal which was loaded for the bitmap
        /// It represents the original image and not the sized drawing area
        /// </summary>
        public Size OriginalBackgroundSize { get; set; }

        /// <summary>
        /// Stores the dictionary between overlay items and their corresponding image
        /// </summary>
        public List<ItemView> ItemViews { get; } = new List<ItemView>();


        private void GetSizeAndOffsetOfRenderedBackground(out double newHeight, out double newWidth, out double offsetX, out double offsetY)
        {
            var totalSize = new Size(
                BackgroundCanvas.ActualWidth,
                BackgroundCanvas.ActualHeight);

            newHeight = Math.Min(totalSize.Height, totalSize.Width / _ratioWidthToHeight);
            newWidth = newHeight * _ratioWidthToHeight;

            offsetX = (totalSize.Width - newWidth) / 2;
            offsetY = (totalSize.Height - newHeight) / 2;
        }

        /// <summary>
        /// Scales the position of the element
        /// </summary>
        /// <param name="x">X-Coordinate to be scaled</param>
        /// <param name="y">Y-Coordinate to be scaled</param>
        /// <returns></returns>
        public (double resultX, double resultY) ScalePosition(DoubleWithUnit x, DoubleWithUnit y)
        {
            GetSizeAndOffsetOfRenderedBackground(out var newHeight, out var newWidth, out var offsetX, out var offsetY);

            var scaledX =
                x.Unit == Units.Percentage
                    ? newWidth * x.Value + offsetX
                    : newWidth * x.Value / OriginalBackgroundSize.Width + offsetX;
            var scaledY =
                y.Unit == Units.Percentage
                    ? newHeight * y.Value + offsetY
                    : newHeight * y.Value / OriginalBackgroundSize.Height + offsetY;

            return (scaledX, scaledY);
        }


        /// <summary>
        /// Scales the size of the element
        /// </summary>
        /// <param name="x">X-Coordinate to be scaled</param>
        /// <param name="y">Y-Coordinate to be scaled</param>
        /// <returns>Scaled size without offset change</returns>
        public (double resultX, double resultY) ScaleSize(DoubleWithUnit x, DoubleWithUnit y)
        {
            GetSizeAndOffsetOfRenderedBackground(out var newHeight, out var newWidth, out var _, out var _);

            var scaledX =
                x.Unit == Units.Percentage
                    ? newWidth * x.Value
                    : newWidth * x.Value / OriginalBackgroundSize.Width;
            var scaledY =
                y.Unit == Units.Percentage
                    ? newHeight * y.Value
                    : newHeight * y.Value / OriginalBackgroundSize.Height;

            return (scaledX, scaledY);
        }


        /// <summary>
        /// Scales the coordinates of absolute value to a rectangle in which the ratio is defined as in _ratioWidthToHeight
        /// </summary>
        /// <param name="x">X coordinate in relative position (0.0-1.0)</param>
        /// <param name="y">Y coordinate in relative position (0.0-1.0)</param>
        /// <param name="width">Width in relative size</param>
        /// <param name="height">Height in relative size</param>
        /// <returns>Rectangle containing the absolute coordinate in pixels for WPF</returns>
        public Rect ScaleToRect(DoubleWithUnit x, DoubleWithUnit y, DoubleWithUnit width, DoubleWithUnit height)
        {
            if (Math.Abs(_ratioWidthToHeight) < 1E-7 || ActualWidth < 1E-7 || ActualHeight < 1E-7)
            {
                return new Rect(0, 0, 0, 0);
            }


            GetSizeAndOffsetOfRenderedBackground(out var newHeight, out var newWidth, out var offsetX, out var offsetY);

            var scaledX =
                x.Unit == Units.Percentage
                    ? newWidth * x.Value + offsetX
                    : newWidth * x.Value / OriginalBackgroundSize.Width + offsetX;
            var scaledY =
                y.Unit == Units.Percentage
                    ? newHeight * y.Value + offsetY
                    : newHeight * y.Value / OriginalBackgroundSize.Height + offsetY;
            var scaledWidth =
                width.Unit == Units.Percentage
                    ? newWidth * width.Value
                    : newWidth * width.Value / OriginalBackgroundSize.Width;
            var scaledHeight =
                height.Unit == Units.Percentage
                    ? newHeight * height.Value
                    : newHeight * height.Value / OriginalBackgroundSize.Height;

            return new Rect(
                scaledX,
                scaledY,
                scaledWidth,
                scaledHeight
            );
        }

        /// <summary>
        /// Makes and scales the actual size to fit into the surrounding box
        /// </summary>
        /// <param name="surroundingBox">The surrounding box</param>
        /// <param name="actualSize">The size of the element that needs to fit into the surrounding box</param>
        /// <returns>The scaled size</returns>
        public static Size ScaleSize(Size surroundingBox, Size actualSize)
        {
            var scale = Math.Min(
                surroundingBox.Width / actualSize.Width,
                surroundingBox.Height / actualSize.Height);

            return new Size(
                actualSize.Width * scale,
                actualSize.Height * scale);
        }

        /// <summary>
        /// Loads the images from the data
        /// </summary>
        public void CreateView()
        {
            // If machine is null, creates the view
            if (Machine == null)
            {
                return;
            }
            
            BackgroundCanvas.Children.Clear();
            _imagesForStates.Clear();
            ItemViews.Clear();

            // Removes the existing items
            _backgroundImage = null;

            if (!string.IsNullOrEmpty(Machine.BackgroundImageUrl))
            {
                var path = System.IO.Path.Combine(Environment.CurrentDirectory, Machine.BackgroundImageUrl);
                var bitmap = new BitmapImage(new Uri(path));
                _ratioWidthToHeight = bitmap.Width / bitmap.Height;
                OriginalBackgroundSize = new Size(bitmap.PixelWidth, bitmap.PixelHeight);

                var states = Machine.Items.SelectMany(x => x.Type.States).Distinct();
                foreach (var state in states)
                {
                    _imagesForStates[state] = new UiElementFactory(state);
                }

                // Creates the actual image
                _backgroundImage = new Image { Source = bitmap };
                BackgroundCanvas.Children.Add(_backgroundImage);

                foreach (var item in Machine.Items)
                {
                    var border = new Border
                    {
                        BorderBrush = Brushes.Red,
                        BorderThickness = new Thickness(0.0)
                    };

                    var viewbox = new Viewbox
                    {
                        Stretch = Stretch.Uniform
                    };

                    border.Child = viewbox;

                    BackgroundCanvas.Children.Add(border);
                    var itemView = new ItemView
                    {
                        Item = item,
                        UiElement = border
                    };
                    ItemViews.Add(itemView);
                }
            }

            UpdateStates();
            UpdateView(PositionFlag.OnlyBackground);
        }

        public enum PositionFlag
        {
            /// <summary>
            /// Defines that not only the background, but also the items will be positioned
            /// </summary>
            AlsoItems,

            /// <summary>
            /// Defines that only the background shall be positioned
            /// </summary>
            OnlyBackground
        }

        /// <summary>
        /// Updates all the images as given by the state of each item
        /// </summary>
        public void UpdateStates()
        {
            foreach (var pair in ItemViews)
            {
                UpdateState(pair);
            }
        }

        /// <summary>
        /// Updates the state and the button text
        /// </summary>
        /// <param name="itemView">Pair to be updated</param>
        public void UpdateState(ItemView itemView)
        {
            var state = itemView.Item.CurrentState;
            if (_imagesForStates.TryGetValue(state, out var elementFactory))
            {
                // Sets the factory being used for the element
                itemView.Factory = elementFactory;

                var border = (Border) itemView.UiElement;
                ((Viewbox) border.Child).Child = elementFactory.CreateUiElement(this);
            }

            UpdateView(itemView);
        }

        /// <summary>
        /// Updates all positions when the window is resized
        /// </summary>
        public void UpdateView(PositionFlag flag = PositionFlag.AlsoItems)
        {
            if (_backgroundImage != null)
            {
                var backgroundPosition = ScaleToRect(
                    DoubleWithUnit.ToPercentage(0), 
                    DoubleWithUnit.ToPercentage(0),
                    DoubleWithUnit.ToPercentage(1.0),
                    DoubleWithUnit.ToPercentage(1.0));
                Canvas.SetLeft(_backgroundImage, backgroundPosition.Left);
                Canvas.SetTop(_backgroundImage, backgroundPosition.Top);
                _backgroundImage.Width = backgroundPosition.Width;
                _backgroundImage.Height = backgroundPosition.Height;
            }

            if (flag == PositionFlag.AlsoItems)
            {
                foreach (var pair in ItemViews)
                {
                    UpdateView(pair);
                }
            }
        }

        /// <summary>
        /// Updates the view of the given item 
        /// </summary>
        /// <param name="pair">Item whose position shall be updated</param>
        private void UpdateView(ItemView pair)
        {
            var width = pair.Item.Position.Width;
            var height = pair.Item.Position.Height;

            if (Math.Abs(width.Value) < 1E-7)
            {
                width = DoubleWithUnit.ToPixel(pair.Factory.NativeSize.Width);
            }
            if (Math.Abs(height.Value) < 1E-7)
            {
                height = DoubleWithUnit.ToPixel(pair.Factory.NativeSize.Height);
            }

            var rect = ScaleToRect(
                pair.Item.Position.X,
                pair.Item.Position.Y,
                width,
                height);

            Canvas.SetLeft(pair.UiElement, rect.Left);
            Canvas.SetTop(pair.UiElement, rect.Top);
            pair.UiElement.Width = rect.Width;
            pair.UiElement.Height = rect.Height;
            pair.UiElement.HorizontalAlignment = HorizontalAlignment.Center;
            pair.UiElement.VerticalAlignment = VerticalAlignment.Center;


            pair.UiElement.RenderTransform = new RotateTransform(
                pair.Item.Rotation,
                rect.Width / 2,
                rect.Height / 2);
        }

        /// <summary>
        /// Called, when the user or another application changed the size of the element
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments being used for sizing</param>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateView();
        }
    }
}
