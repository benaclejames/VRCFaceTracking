using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using VRCFaceTracking_Next.Contracts.Services;
using VRCFaceTracking_Next.Helpers;

using Windows.ApplicationModel;
using VRCFaceTracking_Next.Core.Contracts.Services;

namespace VRCFaceTracking_Next.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private ElementTheme _elementTheme;
    private string _versionDescription;
    private readonly IOSCService _oscService;
    private int _recvPort, _sendPort;
    private string _address;

    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set => SetProperty(ref _elementTheme, value);
    }

    public string VersionDescription
    {
        get => _versionDescription;
        set => SetProperty(ref _versionDescription, value);
    }

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public int RecvPort
    {
        get => _recvPort;
        set 
        {
            _oscService.InPort = value;
            SetProperty(ref _recvPort, value);
        }
    }

    public int SendPort
    {
        get => _sendPort;
        set
        {
            _oscService.OutPort = value;
            SetProperty(ref _sendPort, value);
        }
    }

    public Task<(bool, bool)> ReInitOsc() => _oscService.InitializeAsync();

    public SettingsViewModel(IThemeSelectorService themeSelectorService, IOSCService oscService)
    {
        _themeSelectorService = themeSelectorService;
        _oscService = oscService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
