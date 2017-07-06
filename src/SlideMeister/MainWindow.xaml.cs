using SlideMeisterLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SlideMeister.ViewModels;
using SlideMeisterLib.Logic;

namespace SlideMeister
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Stores the machine to be shown
        /// </summary>
        private Machine _machine;

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
        private readonly List<ItemView> _items = new List<ItemView>();

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
        /// Initializes a new instance of the MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            CreateMachine();

            LoadImages();
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
        /// Loads the images from the data
        /// </summary>
        public void LoadImages()
        {
            // Removes the existing items
            BackgroundCanvas.Children.Clear();
            _backgroundImage = null;


            if (!string.IsNullOrEmpty(_machine.BackgroundImageUrl))
            {
                var path = System.IO.Path.Combine(Environment.CurrentDirectory, _machine.BackgroundImageUrl);
                var bitmap = new BitmapImage(new Uri(path));
                _ratioWidthToHeight = bitmap.Width / bitmap.Height;

                var states = _machine.Items.SelectMany(x => x.Type.States).Distinct();
                foreach (var state in states)
                {
                    path = System.IO.Path.Combine(Environment.CurrentDirectory, state.ImageUrl);
                    var itemBitmap = new BitmapImage(new Uri(path));

                    _imagesForStates[state] = itemBitmap;
                }


                // Creates the actuatl imagesImages
                _backgroundImage = new Image {Source = bitmap};
                BackgroundCanvas.Children.Add(_backgroundImage);

                foreach (var item in _machine.Items)
                {
                    var imageForItem = new Image();
                    BackgroundCanvas.Children.Add(imageForItem);


                    var button = new Button
                    {
                        Content = $"{item.Name}: {item.CurrentState?.Name ?? "Not known"}"
                    };

                    var itemView = new ItemView
                    {
                        Image = imageForItem,
                        Item = item,
                        StateButton = button
                    };

                    _items.Add(itemView);

                    button.Click += (x, y) =>
                    {
                        OnStateButtonClick(itemView);
                    };



                    StateButtons.Children.Add(button);
                }
            }

            UpdateStates();

            UpdatePositions();
        }

        private void OnStateButtonClick(ItemView item)
        {
            item.Item.CurrentState = item.Item.Type.GetNextState(item.Item.CurrentState);
            item.StateButton.Content = $"{item.Item.Name}: {item.Item.CurrentState}";
            
            UpdateStates();
        }

        /// <summary>
        /// Updates all the images as given by the state of each item
        /// </summary>
        public void UpdateStates()
        {
            foreach (var pair in _items)
            {
                var state = pair.Item.CurrentState;
                if (_imagesForStates.TryGetValue(state, out BitmapImage source))
                {
                    pair.Image.Source = source;
                }

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


            foreach (var pair in _items)
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

        public void CreateMachine()
        {
            _machine = new Machine {BackgroundImageUrl = "examples/leds/leds.png"};

            var led = new OverlayType("LED");
            var onState = new OverlayState("On", "examples/leds/on.png");
            var offState = new OverlayState("Off", "examples/leds/off.png");
            led.AddState(onState);
            led.AddState(offState);

            var firstLed = new OverlayItem(led)
            {
                Name = "Top",
                Position = new SlideMeisterLib.Model.Rectangle(0.3, 0.1, 0.4, 0.4)
            };

            var secondLed = new OverlayItem(led)
            {
                Name = "Bottom",
                Position = new SlideMeisterLib.Model.Rectangle(0.3, 0.5, 0.4, 0.4),
                CurrentState = offState
            };

            _machine.AddItem(firstLed);
            _machine.AddItem(secondLed);

            var sequence = new TransitionSequence();
            sequence.Steps.Add(
                new TransitionSequenceStep(
                    new Transition(firstLed, onState),
                    new Transition(secondLed, offState)));
            sequence.Steps.Add(
                new TransitionSequenceStep(
                    new Transition(firstLed, offState),
                    new Transition(secondLed, onState)));

            _machine.Sequences.Add(sequence);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePositions();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = true,
                Filter = "SlideMeister Files |*.json;*.slidemeister"
            };

            if (dlg.ShowDialog(this) == true)
            {
                // Load file
                using (var stream = dlg.OpenFile())
                {
                    _machine = Loader.LoadMachine(stream);
                    LoadImages();

                }
                
            }
        }
    }
}
