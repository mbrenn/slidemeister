using System;
using SlideMeisterLib.Model;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SlideMeister.Control;
using SlideMeister.Model;
using SlideMeisterLib.Logic;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace SlideMeister
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Stores the current action
        /// </summary>
        public CancellationTokenSource CurrentSequence;

        private void SequenceSelection_SelectionChanged(
            object sender, 
            System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SequenceSelection.SelectedItem is SequenceInfo selectedItem)
            {
                selectedItem.Initialize.Execute(null);
                CurrentSequenceText.Text = selectedItem.Transition;
            }
        }

        private void SequenceResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (SequenceSelection.SelectedItem is SequenceInfo selectedItem)
            {
                selectedItem.Initialize.Execute(null);
                CurrentSequenceText.Text = selectedItem.Transition;
            }
        }

        private void SequencePreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (SequenceSelection.SelectedItem is SequenceInfo selectedItem)
            {
                selectedItem.Previous.Execute(null);
                CurrentSequenceText.Text = selectedItem.Transition;
            }
        }

        private void SequenceNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (SequenceSelection.SelectedItem is SequenceInfo selectedItem)
            {
                selectedItem.Next.Execute(null);
                CurrentSequenceText.Text = selectedItem.Transition;
            }
        }

        private void SequencePlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (SequenceSelection.SelectedItem is SequenceInfo selectedItem)
            {
                selectedItem.Play.Execute(null);
            }
        }

        /// <summary>
        /// Starts the automatic sequence by clicking the Start button
        /// </summary>
        /// <param name="info">The sequence information for the chosen element</param>
        /// <param name="token">The cancellation token being used to interrupt in case of a cancellation</param>
        internal async void StartAutomaticSequence(SequenceInfo info, CancellationToken token)
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
                        CurrentSequenceText.Text = info.Transition;
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
            SequenceView.ItemsSource = Machine.Sequences.Select(x => new SequenceInfo(this, x)).ToList();

            SequenceSelection.ItemsSource = SequenceView.ItemsSource;
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
            var slideControl = CreateVisualForExport();

            SaveToPng(slideControl, filename);
        }

        private SlideControl CreateVisualForExport()
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
            return slideControl;
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

        private void CopyToClipboard_OnClick(object sender, RoutedEventArgs e)
        {
            var visual = CreateVisualForExport();
            var bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            Clipboard.SetImage(bitmap);

            MessageBox.Show("Copied to clipbard", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
