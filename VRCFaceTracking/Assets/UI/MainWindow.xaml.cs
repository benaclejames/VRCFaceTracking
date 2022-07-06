using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VRCFaceTracking.Assets.UI
{
    public partial class MainWindow
    {
        public static readonly NotifyIcon TrayIcon = new NotifyIcon
        {
            Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("VRCFaceTracking.Assets.Images.VRCFT.ico")), 
            Text = "VRCFaceTracking",
            Visible = true,
        };

        private void ShowWindow(object sender, EventArgs args)
        { 
            Show();
            WindowState = WindowState.Normal;
        }
        
        public MainWindow()
        {
            InitializeComponent();
            // If --min is passed as a command line arg, hide the window immediately
            if (Environment.GetCommandLineArgs().Length > 1 && Environment.GetCommandLineArgs().Any(arg => arg == "--min"))
            {
                WindowState = WindowState.Minimized;
                Hide();
            }

            DataContext = this;

            ConsoleOutput.CollectionChanged += (sender, args) =>
                Dispatcher.BeginInvoke(new ThreadStart(() => Scroller.ScrollToVerticalOffset(Scroller.ExtentHeight)));


            // use the application icon as the icon for the tray
            TrayIcon.DoubleClick += ShowWindow;
            TrayIcon.ContextMenu = new ContextMenu(new[]
            {
                new MenuItem("Exit", (sender, args) => MainStandalone.Teardown()),
                new MenuItem("Show", ShowWindow)
            });
            
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
                while (!MainStandalone.MasterCancellationTokenSource.IsCancellationRequested)
                {
                    Thread.Sleep(10);
                    Dispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        UpdateEyeImage();
                        UpdateLipImage();
                    }));
                }
            }).Start();
        }

        void UpdateLipImage()
        {
            if (TabController.SelectedIndex != 2 || UnifiedTrackingData.LatestLipData.ImageData == null)   // If the image is not initialized
                return;

            var bitmap = LipImage.Source;
            if (bitmap == null || bitmap.GetType() != typeof(WriteableBitmap))
            {
                bitmap = new WriteableBitmap(UnifiedTrackingData.LatestLipData.ImageSize.x,
                    UnifiedTrackingData.LatestLipData.ImageSize.y, 96, 96, PixelFormats.Gray8, null);
            }
            ((WriteableBitmap)bitmap).WritePixels(new Int32Rect(0, 0, UnifiedTrackingData.LatestLipData.ImageSize.x,
                UnifiedTrackingData.LatestLipData.ImageSize.y), UnifiedTrackingData.LatestLipData.ImageData, 800, 0);
            
            // Set the WPF image name LipImage 
            LipImage.Source = bitmap;
        }
        
        void UpdateEyeImage()
        {
            if (TabController.SelectedIndex != 1 || UnifiedTrackingData.LatestEyeData.ImageData == null)   // If the image is not initialized
                return;
            
            var bitmap = EyeImage.Source;
            if (bitmap == null || bitmap.GetType() != typeof(WriteableBitmap))
            {
                bitmap = new WriteableBitmap(UnifiedTrackingData.LatestEyeData.ImageSize.x,
                    UnifiedTrackingData.LatestEyeData.ImageSize.y, 96, 96, PixelFormats.Gray8, null);
            }
            
            ((WriteableBitmap)bitmap).WritePixels(new Int32Rect(0, 0, UnifiedTrackingData.LatestEyeData.ImageSize.x, 
                UnifiedTrackingData.LatestEyeData.ImageSize.y), UnifiedTrackingData.LatestEyeData.ImageData, UnifiedTrackingData.LatestEyeData.ImageSize.x, 0);
            
            // Set the WPF image name EyeImage 
            EyeImage.Source = bitmap;
        }

        public ObservableCollection<Tuple<string, string>> ConsoleOutput => Logger.ConsoleOutput;

        private void AvatarInfoUpdate(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Nothing should go here AFAIK
        }

        private void ReinitializeClick(object sender, RoutedEventArgs e)
        {
            Logger.Msg("Reinitializing...");
            UnifiedLibManager.Initialize();
        }

        private void PauseClickEyes(object sender, RoutedEventArgs e)
        {
            if (UnifiedLibManager.EyeStatus == ModuleState.Uninitialized)   // We don't wanna change states of an inactive module
                return;
            
            UnifiedLibManager.EyeStatus = UnifiedLibManager.EyeStatus == ModuleState.Idle ? ModuleState.Active : ModuleState.Idle;
        }

        private void PauseClickMouth(object sender, RoutedEventArgs e)
        {
            if (UnifiedLibManager.LipStatus == ModuleState.Uninitialized)   // We don't wanna change states of an inactive module
                return;
            
            UnifiedLibManager.LipStatus = UnifiedLibManager.LipStatus == ModuleState.Idle ? ModuleState.Active : ModuleState.Idle;
        }

        private void UpdateLogo(ModuleState eyeState, ModuleState lipState)
        {
            VRCFTLogoTop.Source =
                new BitmapImage(new Uri(@"../Images/LogoIndicators/" + eyeState + "/Top.png",
                    UriKind.Relative));
            VRCFTLogoBottom.Source =
                new BitmapImage(new Uri(@"../Images/LogoIndicators/" + lipState + "/Bottom.png",
                    UriKind.Relative));
        }

        private void MainWindow_OnSizeChanged(object sender, EventArgs eventArgs)
        {
            if (this.WindowState == WindowState.Minimized)  
            {  
                Hide();                 
            } 
        }
    }
}