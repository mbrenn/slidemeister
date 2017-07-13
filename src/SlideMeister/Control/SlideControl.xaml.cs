using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        public Rect ScaleToRect(double x, double y, double width, double height)
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


            return new Rect(
                newWidth * x + offsetX,
                newHeight * y + offsetY,
                newWidth * width,
                newHeight * height
            );
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

            if (itemView.StateButton != null)
            {
                itemView.StateButton.Content = $"{itemView.Item.Name}: {itemView.Item.CurrentState}";
            }
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

            var dict = new Dictionary<OverlayItem, Image>();

            if (!string.IsNullOrEmpty(Machine.BackgroundImageUrl))
            {
                var path = System.IO.Path.Combine(Environment.CurrentDirectory, Machine.BackgroundImageUrl);
                var bitmap = new BitmapImage(new Uri(path));
                _ratioWidthToHeight = bitmap.Width / bitmap.Height;

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
                    var imageForItem = new Image();
                    BackgroundCanvas.Children.Add(imageForItem);
                    dict[item] = imageForItem;
                }


                foreach (var item in Machine.Items)
                {

                    var itemView = new ItemView
                    {
                        Image = dict[item],
                        Item = item
                    };

                    ItemViews.Add(itemView);
                }
            }

            UpdateStates();

            UpdatePositions();
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
        /// Updates all positions when the window is resized
        /// </summary>
        public void UpdatePositions()
        {
            if (_backgroundImage != null)
            {
                var backgroundPosition = ScaleToRect(0, 0, 1.0, 1.0);
                Canvas.SetLeft(_backgroundImage, backgroundPosition.Left);
                Canvas.SetTop(_backgroundImage, backgroundPosition.Top);
                _backgroundImage.Width = backgroundPosition.Width;
                _backgroundImage.Height = backgroundPosition.Height;
            }


            foreach (var pair in ItemViews)
            {
                var rect = ScaleToRect(
                    pair.Item.Position.X,
                    pair.Item.Position.Y,
                    pair.Item.Position.Width,
                    pair.Item.Position.Height);

                Canvas.SetLeft(pair.Image, rect.Left);
                Canvas.SetTop(pair.Image, rect.Top);
                pair.Image.Width = rect.Width;
                pair.Image.Height = rect.Height;
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePositions();
        }
    }
}
