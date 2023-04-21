using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Helpers;

using Windows.ApplicationModel;

namespace VRCFaceTracking.ViewModels;

public class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private ElementTheme _elementTheme;
    private string _versionDescription;
    private readonly IOSCService _oscService;
    private int _inPort, _outPort;
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
        get => _inPort;
        private set => SetProperty(ref _inPort, value);
    }

    public int SendPort
    {
        get => _outPort;
        private set => SetProperty(ref _outPort, value);
    }

    public string Address
    {
        get => _address;
        private set => SetProperty(ref _address, value);
    }

    public async Task SetRecvPort(int port)
    {
        if (RecvPort == port)
            return;

        RecvPort = port;

        _oscService.InPort = port;
        await _oscService.SaveSettings();
        await _oscService.InitializeAsync();
    }

    public async Task SetSendPort(int port)
    {
        if (SendPort == port)
            return;

        SendPort = port;

        _oscService.OutPort = port;
        await _oscService.SaveSettings();
        await _oscService.InitializeAsync();
    }

    public async Task SetAddress(string address)
    {
        if (Address == address)
            return;

        Address = address;

        _oscService.Address = address;
        await _oscService.SaveSettings();
        await _oscService.InitializeAsync();
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, IOSCService oscService)
    {
        _themeSelectorService = themeSelectorService;
        _oscService = oscService;

        _inPort = _oscService.InPort;
        _outPort = _oscService.OutPort;
        _address = _oscService.Address;

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
