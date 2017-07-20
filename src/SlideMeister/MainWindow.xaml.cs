using System;
using System.Collections.Generic;
using System.ComponentModel;
using SlideMeisterLib.Model;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SlideMeister.Control;
using SlideMeister.ViewModels;
using SlideMeisterLib.Logic;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Orientation = System.Windows.Controls.Orientation;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace SlideMeister
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class Messenger : ICommand
        {
            private readonly StateInfo _stateInfo;

            public Messenger(StateInfo stateInfo)
            {
                _stateInfo = stateInfo;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                System.Diagnostics.Debug.WriteLine("Button pressed for: " + _stateInfo.Name);
            }
        }

        public class StateInfo : INotifyPropertyChanged
        {
            private readonly ItemView _view;

            public StateInfo(ItemView view)
            {
                _view = view;
                NextState = new Messenger(this);

            }

            public string Name => _view?.Item?.Name;

            public string State => _view?.Item?.CurrentState?.Name;

            public Messenger NextState { get; }

            public event PropertyChangedEventHandler PropertyChanged;
            
            protected virtual void OnPropertyChanged1([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }


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

            var stateInfos = new List<StateInfo>();

            var row = 0;
            foreach (var itemView in SlideCanvas.ItemViews)
            {
                var rowDefinition = new RowDefinition();
                StateButtons.RowDefinitions.Add(rowDefinition);

                var itemBlock = new TextBlock
                {
                    Text = itemView.Item.Name,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(itemBlock, 0);
                Grid.SetRow(itemBlock, row);
                StateButtons.Children.Add(itemBlock);

                var stateBlock = new TextBlock
                {
                    Text = itemView.Item.CurrentState?.Name ?? string.Empty,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(stateBlock, 1);
                Grid.SetRow(stateBlock, row);
                StateButtons.Children.Add(stateBlock);


                var button = new Button();
                button.Content = "Next State";


                button.Click += (x, y) =>
                {
                    itemView.Item.CurrentState = itemView.Item.Type.GetNextState(itemView.Item.CurrentState);

                    SlideCanvas.UpdateState(itemView, stateBlock);

                };

                Grid.SetColumn(button, 2);
                Grid.SetRow(button, row);
                StateButtons.Children.Add(button);

                SlideCanvas.UpdateState(itemView, stateBlock);
                row++;

                stateInfos.Add(new StateInfo(itemView));

            }

            StateButtonsView.ItemsSource = stateInfos;
            
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
                var filename = dlg.FileName;

                StoreCurrentMachineIntoPng(filename);
            }
        }

        private void SaveSequences_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog();


            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var sequence in Machine.Sequences)
                {
                    var navigation = new TransitionNavigation(Machine, sequence);
                    navigation.Initialize();

                    var directory = dlg.SelectedPath;

                    do
                    {
                        var filename = Path.Combine(
                            directory ?? ".",
                            $"{sequence.Name} - {navigation.CurrentStep.Transitions.Name}.png");

                        StoreCurrentMachineIntoPng(filename);


                        if (!navigation.NavigateToNext())
                        {
                            break;
                        }

                    } while (true);
                }
            }
        }

        /// <summary>
        /// Stores the current machine into a png. 
        /// </summary>
        /// <param name="filename">Filename to be stored</param>
        private void StoreCurrentMachineIntoPng(string filename)
        {
            var size = new Size(SlideCanvas.BackgroundSize.Width, SlideCanvas.BackgroundSize.Height);

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

            SaveToPng(slideControl, filename);
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
