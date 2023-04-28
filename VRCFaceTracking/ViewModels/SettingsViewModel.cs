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
    
    private readonly IOSCService _oscService;
    private int _inPort, _outPort;
    private string _address;

    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set => SetProperty(ref _elementTheme, value);
    }

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public int RecvPort
    {
        get => _inPort;
        set
        {
            _oscService.InPort = value;
            SetProperty(ref _inPort, value);
        }
    }

    public int SendPort
    {
        get => _outPort;
        set
        {
            _oscService.OutPort = value;
            SetProperty(ref _outPort, value);
        }
    }

    public string Address
    {
        get => _address;
        set
        {
            _oscService.Address = value;
            SetProperty(ref _address, value);
        }
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, IOSCService oscService)
    {
        _themeSelectorService = themeSelectorService;
        _oscService = oscService;

        _inPort = _oscService.InPort;
        _outPort = _oscService.OutPort;
        _address = _oscService.Address;

        PropertyChanged += async (sender, args) =>
        {
            // If the property changed is either the in port, out port, or the address, then we need to save the settings
            if (args.PropertyName is not (nameof(RecvPort) or nameof(SendPort) or nameof(Address)))
                return;

            await _oscService.SaveSettings();
            await _oscService.InitializeAsync();
        };

        _elementTheme = _themeSelectorService.Theme;

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
}
