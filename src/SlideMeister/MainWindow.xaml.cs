using System;
using SlideMeisterLib.Model;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using SlideMeister.Control;
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
        public Machine Machine
        {
            get => SlideCanvas.Machine;
            set => SlideCanvas.Machine = value;
        }

        /// <summary>
        /// Initializes a new instance of the MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Machine = Example.CreateMachine();

            CreateView();
        }

        /// <summary>
        /// Loads the images from the data
        /// </summary>
        public void CreateView()
        {
            SlideCanvas.CreateView();

            // If machine is null, creates the view
            if (Machine == null)
            {
                return;
            }

            StateButtons.Children.Clear();
            TransitionButtons.Children.Clear();
            SequenceButtons.Children.Clear();
            Title = $"SlideMeister - {Machine.Name} - {Machine.Version}";

            foreach (var itemView in SlideCanvas.ItemViews)
            {
                var button = new Button();
                itemView.StateButton = button;

                button.Click += (x, y) =>
                {
                    OnStateButtonClick(itemView);
                };

                StateButtons.Children.Add(button);

                SlideCanvas.UpdateState(itemView);

            }


            // Creates the buttons for the transition
            foreach (var transition in Machine.Transitions)
            {
                var button = new Button
                {
                    Content = transition.Name
                };
                button.Click += (x, y) =>
                {
                    var logic = new MachineLogic(Machine);
                    logic.ApplyTransition(transition);
                    SlideCanvas.UpdateStates();
                };

                TransitionButtons.Children.Add(button);
            }


            // Creates the buttons for the transition
            var n = 0;
            foreach (var sequence in Machine.Sequences)
            {
                SequenceButtons.RowDefinitions.Add(new RowDefinition());

                var navigation = new TransitionNavigation(Machine, sequence);

                var title = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var titleText = new TextBlock {Text = sequence.Name};
                var stateText = new TextBlock {Text = string.Empty};
                title.Children.Add(titleText);
                title.Children.Add(stateText);

                Grid.SetRow(title, n);
                Grid.SetColumn(title, 0);

                var initButton = new Button
                {
                    Content = "Initialize"
                };
                initButton.Click += (x, y) =>
                {
                    navigation.Initialize();
                    stateText.Text = navigation.CurrentStep.Transitions.Name;
                    SlideCanvas.UpdateStates();
                };
                Grid.SetRow(initButton, n);
                Grid.SetColumn(initButton, 1);

                var prevButton = new Button
                {
                    Content = "Previous"
                };
                prevButton.Click += (x, y) =>
                {
                    navigation.NavigateToPrevious();
                    stateText.Text = navigation.CurrentStep.Transitions.Name;
                    SlideCanvas.UpdateStates();
                };
                Grid.SetRow(prevButton, n);
                Grid.SetColumn(prevButton, 2);


                var nextButton = new Button
                {
                    Content = "Next"
                };
                nextButton.Click += (x, y) =>
                {
                    navigation.NavigateToNext();
                    stateText.Text = navigation.CurrentStep.Transitions.Name;
                    SlideCanvas.UpdateStates();
                };
                Grid.SetRow(nextButton, n);
                Grid.SetColumn(nextButton, 3);

                SequenceButtons.Children.Add(title);
                SequenceButtons.Children.Add(initButton);
                SequenceButtons.Children.Add(prevButton);
                SequenceButtons.Children.Add(nextButton);
                n++;
            }
        }

        private void OnStateButtonClick(ItemView item)
        {
            item.Item.CurrentState = item.Item.Type.GetNextState(item.Item.CurrentState);


            SlideCanvas.UpdateState(item);
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
                Filter = "SlideMeister Files (*.json, *.slidemeister) |*.json;*.slidemeister"
            };

            if (dlg.ShowDialog(this) == true)
            {
                try
                {
                    // Load file
                    Machine = Loader.LoadMachine(dlg.FileName);
                    CreateView();
                }
                catch (Exception exc)
                {
                    MessageBox.Show(this, $"Exception occured: \r\n\r\n{exc.Message}");
                }
            }
        }

        private void SaveImage_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                AddExtension = true,
                Filter = "PNG-File (*.png)|*.png;*.slidemeister"
            };

            if (dlg.ShowDialog(this) == true)
            {
                var size = new Size(1024, 768);
                // Load file

                var slideControl = new SlideControl
                {
                    Width = size.Width,
                    Height = size.Height,
                    Machine = Machine
                };


                slideControl.Measure(size);
                slideControl.Arrange(new Rect(size));
                slideControl.CreateView();
                slideControl.BackgroundCanvas.Measure(size);
                slideControl.BackgroundCanvas.Arrange(new Rect(size));

                SaveToPng(slideControl, dlg.FileName);
                
            }
        }

        private void SaveSequences_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                AddExtension = true,
                Filter = "PNG-File (*.png)|*.png;*.slidemeister"
            };


            if (dlg.ShowDialog(this) == true)
            {
                // Load file
                var visual = new SlideControl
                {
                    Width = 1024,
                    Height = 768
                };

                foreach (var sequence in Machine.Sequences)
                {
                    var navigation = new TransitionNavigation(Machine, sequence);
                    navigation.Initialize();

                    var directory = Path.GetDirectoryName(dlg.FileName);

                    do
                    {
                        var size = new Size(1024, 768);
                        // Load file

                        var slideControl = new SlideControl
                        {
                            Width = size.Width,
                            Height = size.Height,
                            Machine = Machine
                        };


                        slideControl.Measure(size);
                        slideControl.Arrange(new Rect(size));
                        slideControl.CreateView();
                        slideControl.BackgroundCanvas.Measure(size);
                        slideControl.BackgroundCanvas.Arrange(new Rect(size));

                        SaveToPng(
                            slideControl,
                            Path.Combine(
                                directory ?? ".",
                                $"{sequence.Name} - {navigation.CurrentStep.Transitions.Name}.png"));


                        if (!navigation.NavigateToNext())
                        {
                            break;
                        }
                    } while (true);
                }
            }
        }

        void SaveToPng(FrameworkElement visual, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            SaveUsingEncoder(visual, fileName, encoder);
        }

        // and so on for other encoders (if you want)


        void SaveUsingEncoder(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);

            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }
    }
}
