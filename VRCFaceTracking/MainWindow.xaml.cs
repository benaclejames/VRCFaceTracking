using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace VRCFaceTracking
{
    public partial class MainWindow
    {
        public bool ShouldEyesPause = false;
        public bool ShouldMouthPause = false;
        public bool ShouldReinitialize = false;
        // public UDP IP = 127.0.0.1;
        public int port = 9000;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            ConsoleOutput.CollectionChanged += (sender, args) => 
                Dispatcher.BeginInvoke(new ThreadStart(() => Scroller.ScrollToVerticalOffset(Scroller.ExtentHeight)));

            
            // Is this running as admin?
            // If not, disable the re-int button
            if (!Utils.HasAdmin)
            {
                // Apparently, windows form buttons don't allow for text wrapping
                ReinitializeButton.Content = "Reinitialization is enabled \n only when the application \n is running as administrator.";
                ReinitializeButton.FontSize = 10f;
                ReinitializeButton.IsEnabled = false;
            }
        }


        public ObservableCollection<Tuple<string, string>> ConsoleOutput => Logger.ConsoleOutput;

        private void AvatarInfoUpdate(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Nothing should go here AFAIK
        }

        private void IPInputUpdate(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void PortInputUpdate(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void ReinitializeClick(object sender, RoutedEventArgs e)
        {
            ShouldReinitialize = !ShouldReinitialize;
            Logger.Msg("Reinitializing...");
        }

        private void PauseClickEyes(object sender, RoutedEventArgs e)
        {
            ShouldEyesPause = !ShouldEyesPause;
            Logger.Msg(ShouldEyesPause ? "Eyes Paused" : "Eyes Unpaused");
        }

        private void PauseClickMouth(object sender, RoutedEventArgs e)
        {
            ShouldMouthPause = !ShouldMouthPause;
            Logger.Msg(ShouldMouthPause ? "Mouth Paused" : "Mouth Unpaused");
        }
    }
}