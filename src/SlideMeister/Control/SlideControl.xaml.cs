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
        private readonly Dictionary<OverlayState, BitmapImage> _imagesForStates = new Dictionary<OverlayState, BitmapImage>();

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

        /// <summary>
        /// Gets the size of the window for the architecture
        /// </summary>
        /// <returns></returns>
        private Size GetArchitectureSize()
        {
            return new Size(
                BackgroundCanvas.ActualWidth,
                BackgroundCanvas.ActualHeight);
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

            var totalSize = GetArchitectureSize();

            var newHeight = Math.Min(totalSize.Height, totalSize.Width / _ratioWidthToHeight);
            var newWidth = newHeight * _ratioWidthToHeight;

            var offsetX = (totalSize.Width - newWidth) / 2;
            var offsetY = (totalSize.Height - newHeight) / 2;

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
                OriginalBackgroundSize = new Size(bitmap.Width, bitmap.Height);

                var states = Machine.Items.SelectMany(x => x.Type.States).Distinct();
                foreach (var state in states)
                {
                    path = System.IO.Path.Combine(Environment.CurrentDirectory, state.ImageUrl);
                    var itemBitmap = new BitmapImage(new Uri(path));

                    _imagesForStates[state] = itemBitmap;
                }

                // Creates the actuatl imagesImages
                _backgroundImage = new Image { Source = bitmap };
                BackgroundCanvas.Children.Add(_backgroundImage);

                foreach (var item in Machine.Items)
                {
                    var border = new Border
                    {
                        BorderThickness = new Thickness(0.0),
                        BorderBrush = Brushes.Red
                    };

                    var imageForItem = new Image();
                    border.Child = imageForItem;

                    BackgroundCanvas.Children.Add(border);
                    var itemView = new ItemView
                    {
                        Item = item,
                        UiElement = border,
                        Image = imageForItem
                    };
                    ItemViews.Add(itemView);
                }
            }

            UpdateStates();
            UpdatePositions(PositionFlag.OnlyBackground);
        }

        public enum PositionFlag
        {
            All,
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
            if (_imagesForStates.TryGetValue(state, out BitmapImage source))
            {
                itemView.Image.Source = source;
            }

            UpdatePosition(itemView);
        }

        /// <summary>
        /// Updates all positions when the window is resized
        /// </summary>
        public void UpdatePositions(PositionFlag flag = PositionFlag.All)
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

            if (flag == PositionFlag.All)
            {
                foreach (var pair in ItemViews)
                {
                    UpdatePosition(pair);
                }
            }
        }

        /// <summary>
        /// Updates the position of the given item 
        /// </summary>
        /// <param name="pair">Item whose position shall be updated</param>
        private void UpdatePosition(ItemView pair)
        {
            var width = pair.Item.Position.Width;
            var height = pair.Item.Position.Height;

            if (Math.Abs(width.Value) < 1E-7)
            {
                width = DoubleWithUnit.ToPixel(pair.Image.Source.Width);
            }
            if (Math.Abs(height.Value) < 1E-7)
            {
                height = DoubleWithUnit.ToPixel(pair.Image.Source.Height);
            }

            var rect = ScaleToRect(
                pair.Item.Position.X,
                pair.Item.Position.Y,
                width,
                height);

            Canvas.SetLeft(pair.UiElement, rect.Left);
            Canvas.SetTop(pair.UiElement, rect.Top);
            pair.Image.Width = rect.Width;
            pair.Image.Height = rect.Height;
            pair.Image.HorizontalAlignment = HorizontalAlignment.Center;
            pair.Image.VerticalAlignment = VerticalAlignment.Center;

            var size = ScaleSize(
                new Size(pair.Image.Width, pair.Image.Height),
                new Size(pair.Image.Source.Width, pair.Image.Source.Height));


            pair.Image.RenderTransform = new RotateTransform(
                pair.Item.Rotation,
                size.Width / 2,
                size.Height / 2);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePositions();
        }
    }
}
