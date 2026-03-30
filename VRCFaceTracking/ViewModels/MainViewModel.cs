using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    public ILibManager LibManager { get; }
    public OscQueryService ParameterOutputService { get; }
    public OscRecvService OscRecvService { get; }
    public OscSendService OscSendService { get; }
    public IOscTarget OscTarget { get; }

    private int _messagesRecvd;
    [ObservableProperty] private int _messagesInPerSec;

    private int _messagesSent;
    [ObservableProperty] private int _messagesOutPerSec;

    [ObservableProperty] private bool _noModulesInstalled;
    
    [ObservableProperty] private bool _oscWasDisabled;

    public bool IsAprilFoolsDay => DateTime.Now.Month == 4 && DateTime.Now.Day == 1;

    private const string AprilFoolsSettingKey = "AprilFoolsEnabled";
    private const string AprilFoolsResetYearKey = "AprilFoolsResetYear";
    private readonly ILocalSettingsService _localSettingsService;

    public bool AprilFoolsEnabled
    {
        get => UnifiedExpressionsParameters.AprilFoolsEnabled;
        set
        {
            UnifiedExpressionsParameters.AprilFoolsEnabled = value;
            OnPropertyChanged();
            _ = _localSettingsService.SaveSettingAsync(AprilFoolsSettingKey, value);
        }
    }

    private DispatcherTimer msgCounterTimer;

    public MainViewModel(
        ILibManager libManager,
        OscQueryService parameterOutputService,
        IModuleDataService moduleDataService,
        IOscTarget oscTarget,
        OscRecvService oscRecvService,
        OscSendService oscSendService,
        ILocalSettingsService localSettingsService
        )
    {
        //Services
        LibManager = libManager;
        ParameterOutputService = parameterOutputService;
        OscTarget = oscTarget;
        OscRecvService = oscRecvService;
        OscSendService = oscSendService;
        _localSettingsService = localSettingsService;

        // On the first boot of each April, reset the toggle to on
        LoadAprilFoolsSettingAsync();

        
        // Modules
        var installedNewModules = moduleDataService.GetInstalledModules();
        var installedLegacyModules = moduleDataService.GetLegacyModules().Count();
        NoModulesInstalled = !installedNewModules.Any() && installedLegacyModules == 0;
        
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
    }

    private async void LoadAprilFoolsSettingAsync()
    {
        var now = DateTime.Now;
        var isApril = now.Month == 4;
        var lastResetYear = await _localSettingsService.ReadSettingAsync(AprilFoolsResetYearKey, 0);

        bool enabled;
        if (isApril && lastResetYear != now.Year)
        {
            // First boot this April — turn it on and record the year
            enabled = true;
            await _localSettingsService.SaveSettingAsync(AprilFoolsResetYearKey, now.Year);
            await _localSettingsService.SaveSettingAsync(AprilFoolsSettingKey, true);
        }
        else
        {
            enabled = await _localSettingsService.ReadSettingAsync(AprilFoolsSettingKey, false);
        }

        UnifiedExpressionsParameters.AprilFoolsEnabled = enabled;
        OnPropertyChanged(nameof(AprilFoolsEnabled));
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
