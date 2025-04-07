using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using VRCFaceTracking.ViewModels;
using Windows.System;
using Microsoft.UI.Xaml.Media.Imaging;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Params.Data;
using VrcftImage = VRCFaceTracking.Core.Types.Image;

namespace VRCFaceTracking.Views;

public sealed partial class SettingsPage : Page
{
    public string Version
    {
        get
        {
            var version = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version;
            return version != null ? $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}" : "Unknown";
        }
    }

    public SettingsViewModel ViewModel
    {
        get;
    }
    
    public IOscTarget OscTarget
    {
        get;
    }
    
    public RiskySettingsViewModel RiskySettingsViewModel
    {
        get;
    }

    public WriteableBitmap UpperImageSource => _upperImageStream;
    public WriteableBitmap LowerImageSource => _lowerImageStream;

    private WriteableBitmap _upperImageStream, _lowerImageStream;
    private Stream _upperStream, _lowerStream;
    
    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        RiskySettingsViewModel = App.GetService<RiskySettingsViewModel>();
        OscTarget = App.GetService<IOscTarget>();

        // Initialize hardware debug streams for upper and lower face tracking
        InitializeHardwareDebugStream(UnifiedTracking.EyeImageData, ref _upperImageStream, ref _upperStream);
        InitializeHardwareDebugStream(UnifiedTracking.LipImageData, ref _lowerImageStream, ref _lowerStream);

        Loaded += OnPageLoaded;
        
        UnifiedTracking.OnUnifiedDataUpdated += _ => DispatcherQueue?.TryEnqueue(OnTrackingDataUpdated);
        InitializeComponent();
    }

    private void InitializeHardwareDebugStream(VrcftImage image, ref WriteableBitmap bitmap, ref Stream targetStream)
    {
        var imageSize = image.ImageSize;

        if ( imageSize is { x: > 0, y: > 0 } )
        {
            bitmap = new WriteableBitmap(imageSize.x, imageSize.y);
            targetStream = bitmap.PixelBuffer.AsStream();
        }
    }

    private async void OnTrackingDataUpdated()
    {
        // Handle eye tracking

        var upperData = UnifiedTracking.EyeImageData.ImageData;
        if ( upperData != null )
        {
            // Handle device connected
            if ( _upperStream == null )
            {
                InitializeHardwareDebugStream(UnifiedTracking.EyeImageData, ref _upperImageStream, ref _upperStream);
            }
            // Handle device is valid and is providing data
            if ( _upperStream.CanWrite )
            {
                _upperStream.Position = 0;
                await _upperStream.WriteAsync(upperData, 0, upperData.Length);

                _upperImageStream.Invalidate();
            }
        }
        else
        {
            // Handle device getting unplugged / destroyed / disabled
            // Device is connected
            if ( _upperStream != null || _upperImageStream != null )
            {
                await _upperStream.DisposeAsync();
                _upperImageStream = null;
                _upperStream = null;
            }
        }

        // Handle lip tracking

        var lowerData = UnifiedTracking.LipImageData.ImageData;
        if ( lowerData != null )
        {
            // Handle device connected
            if ( _lowerStream == null )
            {
                InitializeHardwareDebugStream(UnifiedTracking.LipImageData, ref _lowerImageStream, ref _lowerStream);
            }
            // Handle device is valid and is providing data
            if ( _lowerStream.CanWrite )
            {
                _lowerStream.Position = 0;
                await _lowerStream.WriteAsync(lowerData, 0, lowerData.Length);

                _lowerImageStream.Invalidate();
            }
        }
        else
        {
            // Handle device getting unplugged / destroyed / disabled
            // Device is connected
            if ( _lowerStream != null || _lowerImageStream != null )
            {
                await _lowerStream.DisposeAsync();
                _lowerImageStream = null;
                _lowerStream = null;
            }
        }

        if ( _lowerStream == null || _upperStream == null )
        {
            HardwareDebugSeparator.Visibility = Visibility.Collapsed;
        }
        else
        {
            HardwareDebugSeparator.Visibility = Visibility.Visible;
        }
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        var currentTheme = ViewModel.ElementTheme;
        switch (currentTheme)
        {
            case ElementTheme.Light:
                themeMode.SelectedIndex = 0; break;
            case ElementTheme.Dark:
                themeMode.SelectedIndex = 1; break;
            case ElementTheme.Default:
                themeMode.SelectedIndex = 2; break;
        }
    }

    private async void bugRequestCard_Click(object sender, RoutedEventArgs e)
    => await Launcher.LaunchUriAsync(new Uri("https://github.com/benaclejames/VRCFaceTracking/issues/new/choose"));
    
    private async void privacyPolicyCard_Click(object sender, RoutedEventArgs e) 
     => await Launcher.LaunchUriAsync(new Uri("https://github.com/benaclejames/VRCFaceTracking/blob/master/PRIVACY.md"));

    private void themeMode_SelectionChanged(object sender, RoutedEventArgs e)
    {
        var selectedTheme = ((ComboBoxItem)themeMode.SelectedItem)?.Tag?.ToString();

        if (selectedTheme != null)
        {
            var themeEnum = App.GetEnum<ElementTheme>(selectedTheme);
            if (themeEnum == ElementTheme.Default)
            {
                themeEnum = Application.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
            }

            ViewModel.SwitchThemeCommand.Execute(themeEnum);
        }
    }

    private void AcceptToggle_OnToggled(object sender, RoutedEventArgs e)
    {
        RiskySettingsViewModel.Enabled = dangerAcceptToggle.IsOn;
        if (RiskySettingsViewModel.Enabled)
        {
            // Enable cards
            allParamsRelevant.IsEnabled = true;
            resetVRCFT.IsEnabled = true;
            resetAvatarConfig.IsEnabled = true;
            forceReInit.IsEnabled = true;

            // Enable toggles/buttons
            allParamsRelevantToggle.IsEnabled = true;
            resetVRCFTButton.IsEnabled = true;
            resetVRCAvatarConf.IsEnabled = true;
            forceReInitButton.IsEnabled = true;
        }
        else
        {
            // Disable cards
            allParamsRelevant.IsEnabled = false;
            resetVRCFT.IsEnabled = false;
            resetAvatarConfig.IsEnabled = false;
            forceReInit.IsEnabled = false;

            // Disable toggles/buttons and set them to off
            allParamsRelevantToggle.IsEnabled = false;
            allParamsRelevantToggle.IsOn = false;
            resetVRCFTButton.IsEnabled = false;
            resetVRCAvatarConf.IsEnabled = false;
            forceReInitButton.IsEnabled = false;
        }
    }

    private void forceReInitButton_OnClick(object sender, RoutedEventArgs e) => RiskySettingsViewModel.ForceReInit();

    private void resetVRCFTButton_OnClick(object sender, RoutedEventArgs e) => RiskySettingsViewModel.ResetVRCFT();

    private void resetVRCAvatarConf_OnClick(object sender, RoutedEventArgs e) => RiskySettingsViewModel.ResetAvatarOscManifests();
}
