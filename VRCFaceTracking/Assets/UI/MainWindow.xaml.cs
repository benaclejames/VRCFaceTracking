using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VRCFaceTracking.Params;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;
using Timer = System.Threading.Timer;

namespace VRCFaceTracking.Assets.UI
{
    public partial class MainWindow
    {
        public static bool IsLipPageVisible { get; private set; }
        public static bool IsEyePageVisible { get; private set; }

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
            /*
            if (!Utils.HasAdmin)
            {
                // Apparently, windows form buttons don't allow for text wrapping
                ReinitializeButton.Content =
                    "Reinitialization is enabled \n only when the application \n is running as administrator.";
                ReinitializeButton.FontSize = 10f;
                ReinitializeButton.IsEnabled = false;
            }
            */

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
            if (!IsLipPageVisible || UnifiedTracking.LipImageData.ImageData == null)   // If the image is not initialized
                return;

            var bitmap = LipImage.Source;
            if (bitmap == null || bitmap.GetType() != typeof(WriteableBitmap))
            {
                bitmap = new WriteableBitmap(UnifiedTracking.LipImageData.ImageSize.x,
                    UnifiedTracking.LipImageData.ImageSize.y, 96, 96, PixelFormats.Gray8, null);
            }
            ((WriteableBitmap)bitmap).WritePixels(new Int32Rect(0, 0, UnifiedTracking.LipImageData.ImageSize.x,
                UnifiedTracking.LipImageData.ImageSize.y), UnifiedTracking.LipImageData.ImageData, 800, 0);
            
            // Set the WPF image name LipImage 
            LipImage.Source = bitmap;
        }
        
        void UpdateEyeImage()
        {
            if (!IsEyePageVisible || UnifiedTracking.EyeImageData.ImageData == null)   // If the image is not initialized
                return;
            
            var bitmap = EyeImage.Source;
            if (bitmap == null || bitmap.GetType() != typeof(WriteableBitmap))
            {
                bitmap = new WriteableBitmap(UnifiedTracking.EyeImageData.ImageSize.x,
                    UnifiedTracking.EyeImageData.ImageSize.y, 96, 96, PixelFormats.Gray8, null);
            }
            
            ((WriteableBitmap)bitmap).WritePixels(new Int32Rect(0, 0, UnifiedTracking.EyeImageData.ImageSize.x, 
                UnifiedTracking.EyeImageData.ImageSize.y), UnifiedTracking.EyeImageData.ImageData, 800, 0);
            
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
            Logger.Msg("Initializing modules in selected order.");

            UnifiedLibManager.Initialize();
        }

        private void ReloadModulesClick(object sender, RoutedEventArgs e)
        {
            Logger.Msg("Reloading available modules.");

            UnifiedLibManager.ReloadModules();
            moduleListBox.ItemsSource = UnifiedLibManager.AvailableModules;
        }

        private void PauseClickEyes(object sender, RoutedEventArgs e)
        {
            if (UnifiedLibManager.EyeStatus == ModuleState.Uninitialized)   // We don't wanna change states of an inactive module
                return;
            
            UnifiedLibManager.EyeStatus = UnifiedLibManager.EyeStatus == ModuleState.Idle ? ModuleState.Active : ModuleState.Idle;
        }

        private void PauseClickMouth(object sender, RoutedEventArgs e)
        {
            if (UnifiedLibManager.ExpressionStatus == ModuleState.Uninitialized)   // We don't wanna change states of an inactive module
                return;
            
            UnifiedLibManager.ExpressionStatus = UnifiedLibManager.ExpressionStatus == ModuleState.Idle ? ModuleState.Active : ModuleState.Idle;
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

        private void MainWindow_Loaded(object sender, EventArgs eventArgs)
        {
            moduleListBox.ItemsSource = UnifiedLibManager.AvailableModules;
            UseCalibration.IsChecked = UnifiedTracking.Mutator.CalibratorMode == UnifiedTrackingMutator.CalibratorState.Inactive ? false : true;
            EnableSmoothing.IsChecked = UnifiedTracking.Mutator.SmoothingMode ? true : false;
        }

        private void TabController_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IsEyePageVisible = TabController.SelectedIndex == 1;
            IsLipPageVisible = TabController.SelectedIndex == 2;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UnifiedLibManager.RequestedModules = (((System.Windows.Controls.ListBox)sender).SelectedItems.Cast<Assembly>().ToList());
        }

        private void CalibrationClick(object sender, RoutedEventArgs e)
        {
            UseCalibration.IsChecked = true;

            Thread _thread = new Thread(() =>
            {
                Logger.Msg("Initialized calibration.");

                UnifiedTracking.Mutator.SetCalibration();

                UnifiedTracking.Mutator.CalibrationWeight = 0.75f;
                UnifiedTracking.Mutator.CalibratorMode = UnifiedTrackingMutator.CalibratorState.Calibrating;

                Logger.Msg("Calibrating deep normalization for 30s.");
                Thread.Sleep(30000);

                UnifiedTracking.Mutator.CalibrationWeight = 0.2f;
                Logger.Msg("Fine-tuning normalization. Values will be saved on exit.");
                
            });
            _thread.Start();
        }

        private void EnableSmoothing_Checked(object sender, RoutedEventArgs e)
        {                   
            UnifiedTracking.Mutator.SmoothingMode = true;
        }

        private void EnableSmoothing_Unchecked(object sender, RoutedEventArgs e)
        {
            UnifiedTracking.Mutator.SmoothingMode = false;
        }

        private void UseCalibration_Checked(object sender, RoutedEventArgs e)
        {
            Logger.Msg("Enabled fine-tune calibration and using existing calibrated values.");
            UnifiedTracking.Mutator.CalibrationWeight = 0.25f;
            UnifiedTracking.Mutator.CalibratorMode = UnifiedTrackingMutator.CalibratorState.Calibrating;
        }

        private void UseCalibration_Unchecked(object sender, RoutedEventArgs e)
        {
            Logger.Msg("Disabled calibration.");
            UnifiedTracking.Mutator.CalibratorMode = UnifiedTrackingMutator.CalibratorState.Inactive;
        }

        private void Smooth_Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UnifiedTracking.Mutator.SetSmoothness((float)e.NewValue);
        }
    }
}