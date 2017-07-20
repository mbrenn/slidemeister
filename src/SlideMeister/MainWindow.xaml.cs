using System;
using System.Collections.Generic;
using System.ComponentModel;
using SlideMeisterLib.Model;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SlideMeister.Annotations;
using SlideMeister.Control;
using SlideMeister.Helper;
using SlideMeister.ViewModels;
using SlideMeisterLib.Logic;
using Button = System.Windows.Controls.Button;
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
        public class StateInfo : INotifyPropertyChanged
        {
            private readonly ItemView _view;

            public StateInfo(MainWindow window, ItemView view)
            {
                _view = view;
                NextState = new ActionCommand(() =>
                {
                    _view.Item.CurrentState = _view.Item.Type.GetNextState(_view.Item.CurrentState);

                    window.SlideCanvas.UpdateState(_view);
                });

                _view.Item.PropertyChanged += (x, y) =>
                {
                    if (y.PropertyName == "CurrentState")
                    {
                        OnPropertyChanged1(nameof(State));
                    }
                };

                _view.Item.PropertyChanged += (x, y) =>
                {
                    if (y.PropertyName == "Name")
                    {
                        OnPropertyChanged1(nameof(Name));
                    }
                };
            }

            public string Name => _view?.Item?.Name;

            public string State => _view?.Item?.CurrentState?.Name;

            public ActionCommand NextState { get; }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged1([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class TransitionInfo : INotifyPropertyChanged
        {
            private readonly TransitionSet _transitionSet;

            public TransitionInfo(MainWindow window, TransitionSet transitionSet)
            {
                _transitionSet = transitionSet;

                SwitchTo = new ActionCommand(() =>
                {
                    var logic = new MachineLogic(window.Machine);
                    logic.ApplyTransition(_transitionSet);
                    window.SlideCanvas.UpdateStates();
                });

                _transitionSet.PropertyChanged += (x, y) =>
                {
                    if (y.PropertyName == "Name")
                    {
                        OnPropertyChanged1(nameof(Name));
                    }
                };
            }

            public string Name => _transitionSet.Name;

            public ActionCommand SwitchTo { get; }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged1([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class SequenceInfo: INotifyPropertyChanged
        {
            private TransitionSequence _sequence;
            private TransitionNavigation _navigation;

            public SequenceInfo(MainWindow window, TransitionSequence transitionSet)
            {
                _sequence = transitionSet;

                _navigation = new TransitionNavigation(window.Machine, _sequence);

                Initialize = new ActionCommand(() =>
                {
                    _navigation.Initialize();
                    OnPropertyChanged1(nameof(Transition));
                    window.SlideCanvas.UpdateStates();
                });

                Previous = new ActionCommand(() =>
                {
                    _navigation.NavigateToPrevious();
                    OnPropertyChanged1(nameof(Transition));
                    window.SlideCanvas.UpdateStates();
                });

                Next = new ActionCommand(() =>
                {
                    _navigation.NavigateToNext();
                    OnPropertyChanged1(nameof(Transition));
                    window.SlideCanvas.UpdateStates();
                });

                _sequence.PropertyChanged += (x, y) =>
                {
                    if (y.PropertyName == "Name")
                    {
                        OnPropertyChanged1(nameof(Name));
                    }
                };
            }

            public string Name => _sequence.Name;

            public string Transition => _navigation?.CurrentStep == null ? "None" : _navigation.CurrentStep.Transitions.Name;

            public ActionCommand Initialize { get; }
            public ActionCommand Previous { get; }
            public ActionCommand Next { get; }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
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
            
            Title = $"SlideMeister - {Machine.Name} - {Machine.Version}";

            StateButtonsView.ItemsSource = SlideCanvas.ItemViews.Select(x => new StateInfo(this, x)).ToList();
            TransitionView.ItemsSource = Machine.Transitions.Select(x => new TransitionInfo(this, x)).ToList();
            SequenceView.ItemsSource = Machine.Sequences.Select(x => new SequenceInfo(this, x));

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
