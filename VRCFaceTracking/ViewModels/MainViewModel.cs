using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.ViewModels;

public class MainViewModel : ObservableRecipient
{
    public IAvatarInfo AvatarInfo
    {
        get;
    }

    public ILibManager LibManager
    {
        get;
    }
    
    public IOSCService OscService
    {
        get;
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
    
    private bool _isRecvConnected;
    public bool IsRecvConnected
    {
        get => _isRecvConnected;
        set => SetProperty(ref _isRecvConnected, value);
    }

    public MainViewModel()
    {
        AvatarInfo = App.GetService<IAvatarInfo>();
        LibManager = App.GetService<ILibManager>();
        OscService = App.GetService<IOSCService>();
        var moduleDataService = App.GetService<IModuleDataService>();
        var installedNewModules = moduleDataService.GetInstalledModules();
        var installedLegacyModules = moduleDataService.GetLegacyModules().Count();
        NoModulesInstalled = installedNewModules.Count() == 0 && installedLegacyModules == 0;
        
        // We now start 2 new threads to count both the send rate and recv rate of the osc service over 1 second intervals at a time
        // This is done in a separate thread to not block the UI thread
        // We also use a timer to update the UI every 1 second
        OscService.OnMessageReceived += _ => { _messagesRecvd++; };
        var inTimer = new DispatcherTimer();
        inTimer.Interval = TimeSpan.FromSeconds(1);
        inTimer.Tick += (sender, args) =>
        {
            MessagesInPerSec = _messagesRecvd;
            _messagesRecvd = 0;
        };
        inTimer.Start();
        
        OscService.OnMessageDispatched += () => { _messagesSent++; };
        var outTimer = new DispatcherTimer();
        outTimer.Interval = TimeSpan.FromSeconds(1);
        outTimer.Tick += (sender, args) =>
        {
            MessagesOutPerSec = _messagesSent;
            _messagesSent = 0;
        };
        outTimer.Start();
        
        IsRecvConnected = OscService.IsConnected;
        OscService.OnConnectedDisconnected += b => { IsRecvConnected = b; };
    }
}
