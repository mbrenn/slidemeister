using System;
using System.Collections.Generic;
using System.ComponentModel;
using SlideMeisterLib.Model;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
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
    public partial class MainWindow
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

        /// <summary>
        /// Stores the current action
        /// </summary>
        public CancellationTokenSource CurrentSequence;

        public class SequenceInfo: INotifyPropertyChanged
        {
            public TransitionSequence Sequence { get; }
            public TransitionNavigation Navigation { get; }

            public SequenceInfo(MainWindow window, TransitionSequence sequence)
            {
                Sequence = sequence;

                Navigation = new TransitionNavigation(window.Machine, Sequence);

                Initialize = new ActionCommand(() =>
                {
                    Navigation.Initialize();
                    OnPropertyChanged1(nameof(Transition));
                    window.SlideCanvas.UpdateStates();
                });

                Previous = new ActionCommand(() =>
                {
                    Navigation.NavigateToPrevious();
                    OnPropertyChanged1(nameof(Transition));
                    window.SlideCanvas.UpdateStates();
                });

                Next = new ActionCommand(() =>
                {
                    Navigation.NavigateToNext();
                    OnPropertyChanged1(nameof(Transition));
                    window.SlideCanvas.UpdateStates();
                });

                Play = new ActionCommand(() =>
                {
                    window.CurrentSequence?.Cancel();

                    window.CurrentSequence = new CancellationTokenSource();
                    try
                    {
                        window.StartAutomaticSequence(this, window.CurrentSequence.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                });

                Sequence.PropertyChanged += (x, y) =>
                {
                    if (y.PropertyName == "Name")
                    {
                        OnPropertyChanged1(nameof(Name));
                    }
                };
            }

            public string Name => Sequence.Name;

            public string Transition => Navigation?.CurrentStep == null ? "None" : Navigation.CurrentStep.Name;

            public ActionCommand Initialize { get; }
            public ActionCommand Previous { get; }
            public ActionCommand Next { get; }
            public ActionCommand Play { get; }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged1([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            /// <summary>
            /// Called, when an external event has performed a transition. 
            /// The PropertyChanged event is thrown
            /// </summary>
            public void TransitionOccured()
            {
                OnPropertyChanged1(nameof(Transition));
            }
        }

        /// <summary>
        /// Starts the automatic sequence by clicking the Start button
        /// </summary>
        /// <param name="info">The sequence information for the chosen element</param>
        /// <param name="token">The cancellation token being used to interrupt in case of a cancellation</param>
        private async void StartAutomaticSequence(SequenceInfo info, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                info.Navigation.Initialize();
                SlideCanvas.UpdateStates();

                while (true)
                {
                    info.TransitionOccured();
                    await Task.Delay(info.Navigation.CurrentStep.Duration, token);
                    if (info.Navigation.NavigateToNext())
                    {
                        SlideCanvas.UpdateStates();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (TaskCanceledException)
            {
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

            var args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                var fileToBeLoaded = args[1];
                LoadMachineFile(fileToBeLoaded);
            }
            else
            {
                Machine = Example.CreateMachine();
            }

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
                var filename = dlg.FileName;
                LoadMachineFile(filename);
            }
        }

        /// <summary>
        /// Loads the machinefile as specified in the filename. 
        /// Will show a messagebox in case of an exception
        /// </summary>
        /// <param name="filename">File to be loaded</param>
        private void LoadMachineFile(string filename)
        {
            try
            {
                // Load file
                Machine = Loader.LoadMachine(filename);
                CreateView();
            }
            catch (Exception exc)
            {
                MessageBox.Show(this, $"Exception occured: \r\n\r\n{exc.Message}");
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


            MessageBox.Show(this, "Image saved.");
        }

        private void SaveSequences_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var total = 0;
                foreach (var sequence in Machine.Sequences)
                {
                    var navigation = new TransitionNavigation(Machine, sequence);
                    navigation.Initialize();

                    var directory = dlg.SelectedPath;

                    var n = 1;
                    do
                    {
                        var filename = $"{sequence.Name} - {n} - {navigation.CurrentStep.Name}.png";
                        var validFilename = new string(filename.Where(ch => !InvalidFileNameChars.Contains(ch)).ToArray());
                        var filePath = Path.Combine(
                            directory ?? ".",
                            validFilename);

                        StoreCurrentMachineIntoPng(filePath);


                        if (!navigation.NavigateToNext())
                        {
                            break;
                        }

                        n++;
                        total++;

                    } while (true);
                }

                MessageBox.Show(this, $"{total} images saved.");
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

        static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        void SaveUsingEncoder(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            var bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            var frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);

            
            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutDialog
            {
                Owner = this
            };
            about.ShowDialog();
        }

        private void FileOpenExample_Click(object sender, RoutedEventArgs e)
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "examples");
            if (!Directory.Exists(path))
            {
                MessageBox.Show(this,
                    "For whatever reason, no example directory is given within the application folder");
            }

            var dlg = new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = true,
                Filter = "SlideMeister Files (*.json, *.slidemeister) |*.json;*.slidemeister",
                InitialDirectory = path
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
    }
}
