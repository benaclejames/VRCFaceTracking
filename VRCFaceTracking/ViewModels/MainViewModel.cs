using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models.ParameterDefinition;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Params;

namespace VRCFaceTracking.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    public ILibManager LibManager { get; }
    public OscQueryService ParameterOutputService { get; }

    [ObservableProperty] private IAvatarInfo _currentlyLoadedAvatar;

    [ObservableProperty] private List<Parameter>? _currentParameters;

    private int _messagesRecvd;
    [ObservableProperty] private int _messagesInPerSec;

    private int _messagesSent;
    [ObservableProperty] private int _messagesOutPerSec;

    [ObservableProperty] private bool _noModulesInstalled;
    
    [ObservableProperty] private bool _oscWasDisabled;

    public MainViewModel(
        ILibManager libManager,
        OscQueryService parameterOutputService,
        IModuleDataService moduleDataService,
        IDispatcherService dispatcherService
        )
    {
        //Services
        LibManager = libManager;
        ParameterOutputService = parameterOutputService;
        
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
        ParameterOutputService.OnMessagesDispatched += msgCount => { _messagesSent += msgCount; };
        var messageTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        messageTimer.Tick += (_, _) =>
        {
            MessagesInPerSec = _messagesRecvd;
            _messagesRecvd = 0;
            
            MessagesOutPerSec = _messagesSent;
            _messagesSent = 0;
        };
        messageTimer.Start();
    }
}
