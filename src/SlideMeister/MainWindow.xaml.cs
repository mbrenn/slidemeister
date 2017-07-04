using SlideMeisterLib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        /// <summary>
        /// Stores a dictionary of images which are loaded by the states
        /// </summary>
        private readonly Dictionary<OverlayState, Image> _imagesForStates = new Dictionary<OverlayState, Image>();

        /// <summary>
        /// Initializes a new instance of the MainWindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            CreateMachine();

            LoadImages();
        }

        public void LoadImages()
        {
            var path = System.IO.Path.Combine(Environment.CurrentDirectory, _machine.BackgroundImageUrl);
            var bitmap = new BitmapImage(new Uri(path));
            
            BackgroundCanvas.Children.Add(new Image { Source = bitmap });

            var states = _machine.Items.SelectMany(x => x.Type.States).Distinct();
            foreach (var state in states)
            {
                path = System.IO.Path.Combine(Environment.CurrentDirectory, _machine.BackgroundImageUrl);
                bitmap = new BitmapImage(new Uri(path));

                var image = new Image { Source = bitmap };
                _imagesForStates[state] = image;
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

            var firstLed = new OverlayItem(led);
            var secondLed = new OverlayItem(led);

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

    }
}
