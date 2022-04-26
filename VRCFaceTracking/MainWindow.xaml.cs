using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VRCFaceTracking
{
    public partial class MainWindow
    {
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
                ReinitializeButton.Content =
                    "Reinitialization is enabled \n only when the application \n is running as administrator.";
                ReinitializeButton.FontSize = 10f;
                ReinitializeButton.IsEnabled = false;
            }

            UnifiedLibManager.OnTrackingStateUpdate += (eye, lip) =>
                Dispatcher.BeginInvoke(new ThreadStart(() => UpdateLogo(eye, lip)));
            
            // Start a new thread to update the lip image
            new Thread(() =>
            {
                while (!MainStandalone.MainToken.IsCancellationRequested)
                {
                    Thread.Sleep(10);
                    Dispatcher.BeginInvoke(new ThreadStart(() => UpdateLipImage()));
                }
            }).Start();
        }

        void UpdateLipImage()
        {
            if (UnifiedTrackingData.Image == IntPtr.Zero)   // If the image is not initialized
                return;
            
            byte[] managedArray = new byte[800*400];
            Marshal.Copy(UnifiedTrackingData.Image, managedArray, 0, 800*400);
            var bitmap = new WriteableBitmap(800, 400, 96, 96, PixelFormats.Gray8, null);
            bitmap.WritePixels(new Int32Rect(0, 0, 800, 400), managedArray, 800, 0);
            
            // Set the WPF image name LipImage 
            LipImage.Source = bitmap;
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
            UnifiedLibManager.CheckRuntimeSanity();
            Logger.Msg("Reinitializing...");
        }

        private void PauseClickEyes(object sender, RoutedEventArgs e)
        {
            if (UnifiedLibManager.EyeStatus == ModuleState.Inactive)   // We don't wanna change states of an inactive module
                return;
            
            UnifiedLibManager.EyeStatus = UnifiedLibManager.EyeStatus == ModuleState.Idle ? ModuleState.Active : ModuleState.Idle;
            Logger.Msg(UnifiedLibManager.EyeStatus == ModuleState.Idle ? "Eyes Paused" : "Eyes Unpaused");
        }

        private void PauseClickMouth(object sender, RoutedEventArgs e)
        {
            if (UnifiedLibManager.LipStatus == ModuleState.Inactive)   // We don't wanna change states of an inactive module
                return;
            
            UnifiedLibManager.LipStatus = UnifiedLibManager.LipStatus == ModuleState.Idle ? ModuleState.Active : ModuleState.Idle;
            Logger.Msg(UnifiedLibManager.LipStatus == ModuleState.Idle ? "Mouth Paused" : "Mouth Unpaused");
        }

        private void UpdateLogo(ModuleState eyeState, ModuleState lipState)
        {
            VRCFTLogoTop.Source =
                new BitmapImage(new Uri(@"Images/LogoIndicators/" + eyeState + "/Top.png",
                    UriKind.Relative));
            VRCFTLogoBottom.Source =
                new BitmapImage(new Uri(@"Images/LogoIndicators/" + lipState + "/Bottom.png",
                    UriKind.Relative));
            
            // Construct a new bitmap image from the alpha bytes in UnifiedTrackingData.Image
            
        }
    }
}