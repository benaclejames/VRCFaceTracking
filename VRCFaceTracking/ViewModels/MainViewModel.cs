using System.Runtime.InteropServices.Swift;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Contracts;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    public ILibManager LibManager { get; }
    public OscQueryService ParameterOutputService { get; }
    public OscRecvService OscRecvService { get; }
    public OscSendService OscSendService { get; }
    public IOscTarget OscTarget { get; }
    private readonly IModuleDataService _moduleDataService;

    private int _messagesRecvd;
    [ObservableProperty] private int _messagesInPerSec;

    private int _messagesSent;
    [ObservableProperty] private int _messagesOutPerSec;

    [ObservableProperty] private bool _noModulesInstalled;
    
    [ObservableProperty] private bool _oscWasDisabled;

    private DispatcherTimer msgCounterTimer;

    public MainViewModel(
        ILibManager libManager,
        OscQueryService parameterOutputService,
        IModuleDataService moduleDataService,
        IOscTarget oscTarget,
        OscRecvService oscRecvService,
        OscSendService oscSendService
        )
    {
        //Services
        LibManager = libManager;
        ParameterOutputService = parameterOutputService;
        OscTarget = oscTarget;
        OscRecvService = oscRecvService;
        OscSendService = oscSendService;
        _moduleDataService = moduleDataService;
        
        // Message Timer
        OscRecvService.OnMessageReceived += MessageReceived;
        OscSendService.OnMessagesDispatched += MessageDispatched;
        msgCounterTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        msgCounterTimer.Tick += (_, _) =>
        {
            MessagesInPerSec = _messagesRecvd;
            _messagesRecvd = 0;
            
            MessagesOutPerSec = _messagesSent;
            _messagesSent = 0;
        };
        msgCounterTimer.Start();
        
        OnNavigatedTo();
    }

    public void OnNavigatedTo()
    {
        // Modules
        var installedNewModules = _moduleDataService.GetInstalledModules();
        var installedLegacyModules = _moduleDataService.GetLegacyModules().Count();
        NoModulesInstalled = !installedNewModules.Any() && installedLegacyModules == 0;
    }
    
    private void MessageReceived(OscMessage msg) => _messagesRecvd++;
    private void MessageDispatched(int msgCount) => _messagesSent += msgCount;

    ~MainViewModel()
    {
        OscRecvService.OnMessageReceived -= MessageReceived;
        OscSendService.OnMessagesDispatched -= MessageDispatched;
        
        msgCounterTimer.Stop();
    }
}
