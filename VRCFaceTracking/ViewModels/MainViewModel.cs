using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using VRCFaceTracking.Core.Contracts.Services;
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
        LibManager = App.GetService<ILibManager>();
        ParameterOutputService = App.GetService<ParameterOutputService>();
        var moduleDataService = App.GetService<IModuleDataService>();
        var installedNewModules = moduleDataService.GetInstalledModules();
        var installedLegacyModules = moduleDataService.GetLegacyModules().Count();
        NoModulesInstalled = !installedNewModules.Any() && installedLegacyModules == 0;
        
        // We now start 2 new threads to count both the send rate and recv rate of the osc service over 1 second intervals at a time
        // This is done in a separate thread to not block the UI thread
        // We also use a timer to update the UI every 1 second
        ParameterOutputService.OnMessageReceived += _ => { _messagesRecvd++; };
        var inTimer = new DispatcherTimer();
        inTimer.Interval = TimeSpan.FromSeconds(1);
        inTimer.Tick += (sender, args) =>
        {
            MessagesInPerSec = _messagesRecvd;
            _messagesRecvd = 0;
        };
        inTimer.Start();
        
        ParameterOutputService.OnMessageDispatched += () => { _messagesSent++; };
        var outTimer = new DispatcherTimer();
        outTimer.Interval = TimeSpan.FromSeconds(1);
        outTimer.Tick += (sender, args) =>
        {
            MessagesOutPerSec = _messagesSent;
            _messagesSent = 0;
        };
        outTimer.Start();
    }
}
