using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models.ParameterDefinition;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking.ViewModels;

public class MainViewModel : ObservableRecipient
{
    public ILibManager LibManager
    {
        get;
    }
    
    public ParameterOutputService ParameterOutputService
    {
        get;
    }

    private IAvatarInfo _currentlyLoadedAvatar;
    public IAvatarInfo CurrentlyLoadedAvatar
    {
        get => _currentlyLoadedAvatar;
        private set => SetProperty(ref _currentlyLoadedAvatar, value);
    }

    private List<Parameter> _currentParameters;

    public List<Parameter> CurrentParameters
    {
        get => _currentParameters;
        private set => SetProperty(ref _currentParameters, value);
    }

    private int _messagesRecvd;
    private int _messagesInPerSec;
    public int MessagesInPerSec
    {
        get => _messagesInPerSec;
        set => SetProperty(ref _messagesInPerSec, value);
    }

    private int _messagesSent;
    private int _messagesOutPerSec;
    public int MessagesOutPerSec
    {
        get => _messagesOutPerSec;
        set => SetProperty(ref _messagesOutPerSec, value);
    }

    private bool _noModulesInstalled;
    public bool NoModulesInstalled
    {
        get => _noModulesInstalled;
        set => SetProperty(ref _noModulesInstalled, value);
    }
    
    private bool _oscWasDisabled;
    public bool OscWasDisabled
    {
        get => true;
        set => SetProperty(ref _oscWasDisabled, value);
    }

    public MainViewModel()
    {
        //Services
        LibManager = App.GetService<ILibManager>();
        ParameterOutputService = App.GetService<ParameterOutputService>();
        var moduleDataService = App.GetService<IModuleDataService>();
        var dispatcherService = App.GetService<IDispatcherService>();
        
        // Modules
        var installedNewModules = moduleDataService.GetInstalledModules();
        var installedLegacyModules = moduleDataService.GetLegacyModules().Count();
        NoModulesInstalled = !installedNewModules.Any() && installedLegacyModules == 0;
        
        // Avatar Info
        CurrentlyLoadedAvatar = new NullAvatarDef("Loading...", "Loading...");
        ParameterOutputService.OnAvatarLoaded += (info, list) => dispatcherService.Run(() =>
        {
            CurrentlyLoadedAvatar = info;
            CurrentParameters = list;
        });
        
        // Message Timer
        ParameterOutputService.OnMessageReceived += _ => { _messagesRecvd++; };
        ParameterOutputService.OnMessageDispatched += () => { _messagesSent++; };
        var messageTimer = new DispatcherTimer();
        messageTimer.Interval = TimeSpan.FromSeconds(1);
        messageTimer.Tick += (sender, args) =>
        {
            MessagesInPerSec = _messagesRecvd;
            _messagesRecvd = 0;
            
            MessagesOutPerSec = _messagesSent;
            _messagesSent = 0;
        };
        messageTimer.Start();
    }
}
