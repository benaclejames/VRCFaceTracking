using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models.ParameterDefinition;
using VRCFaceTracking.Core.OSC.Query.mDNS;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking.Core.OSC;

public partial class OscQueryService : ObservableObject
{
    // Services
    private readonly ILogger _logger;
    private readonly OscQueryConfigParser _oscQueryConfigParser;
    private readonly QueryRegistrar _queryRegistrar;

    [ObservableProperty] private IAvatarInfo _avatarInfo = new NullAvatarDef("Loading...", "Loading...");
    [ObservableProperty] private List<Parameter> _avatarParameters;

    private readonly IOscTarget _oscTarget;
 
    // Local vars
    private readonly OscRecvService _recvService;
    private HttpHandler _httpHandler;
    private readonly ILocalSettingsService _settingsService;
    
    public OscQueryService(
        ILogger<OscQueryService> logger,
        OscQueryConfigParser oscQueryConfigParser,
        IOscTarget oscTarget,
        OscRecvService recvService,
        ILocalSettingsService settingsService
    )
    {
        _oscQueryConfigParser = oscQueryConfigParser;
        _logger = logger;
        _recvService = recvService;
        _recvService.OnMessageReceived = HandleNewMessage;
        _queryRegistrar = new QueryRegistrar();
        _oscTarget = oscTarget;
        _settingsService = settingsService;
    }

    public async Task InitializeAsync()
    {
        _logger.LogDebug("OSC Service Initializing");
            
        await _settingsService.Load(_oscTarget);
        
        (bool listenerSuccess, bool senderSuccess) result = (false, false);

        if (string.IsNullOrWhiteSpace(_oscTarget.DestinationAddress))
        {
            return;  // Return both false as we cant bind to anything without an address
        }

        InitOscQuery();
            

        QueryRegistrar.OnVrcClientDiscovered += FirstClientDiscovered;
            
        _logger.LogDebug("OSC Service Initialized with result {0}", result);
        await Task.CompletedTask;
        return;

        void FirstClientDiscovered()
        {
            QueryRegistrar.OnVrcClientDiscovered -= FirstClientDiscovered;
                
            HandleNewAvatar();
        }
    }
    
    private void InitOscQuery()
    {
        _httpHandler?.Dispose();
        _httpHandler = new HttpHandler(6970);
        
        // Advertise our OSC JSON and OSC endpoints (OSC JSON to display the silly lil popup in-game)
        _queryRegistrar.Advertise("_oscjson._tcp", "VRCFT", 6970, IPAddress.Loopback);
        _queryRegistrar.Advertise("_osc._udp", "VRCFT", 6969, IPAddress.Loopback);
        
        _queryRegistrar.QueryForVRChat();
    }

    private async void HandleNewAvatar()
    {
        var newAvatar = await _oscQueryConfigParser.ParseNewAvatar(_queryRegistrar.VrchatClientEndpoint);
        if (newAvatar.HasValue)
        {
            AvatarInfo = newAvatar.Value.avatarInfo;
            AvatarParameters = newAvatar.Value.relevantParameters;
        }
    }
        
    private void HandleNewMessage(OscMessage msg)
    {
        switch (msg.Address)
        {
            case "/avatar/change":
                HandleNewAvatar();
                break;
            case "/vrcft/settings/forceRelevant":   // Endpoint for external tools to force vrcft to send all parameters
                if (msg.Value is bool relevancy)
                {
                    ParameterSenderService.AllParametersRelevant = relevancy;
                }

                break;
            /*
            case "/avatar/parameters/EyeTrackingActive":
                if (UnifiedLibManager.EyeStatus != ModuleState.Uninitialized)
                {
                    if (!msg.Value.BoolValue)
                        UnifiedLibManager.EyeStatus = ModuleState.Idle;
                    else UnifiedLibManager.EyeStatus = ModuleState.Active;
                }
                break;
            case "/avatar/parameters/LipTrackingActive":
            case "/avatar/parameters/ExpressionTrackingActive":
                {
                    if (!msg.Value.BoolValue)
                if (UnifiedLibManager.ExpressionStatus != ModuleState.Uninitialized)
                        UnifiedLibManager.ExpressionStatus = ModuleState.Idle;
                    else UnifiedLibManager.ExpressionStatus = ModuleState.Active;
                }
                break;
            */
        }
    }

    partial void OnAvatarParametersChanged(List<Parameter> discoveredParameters)
    {
        _logger.LogDebug("Configuration loaded. Checking for native tracking parameters...");
        var hasLoadedNative = discoveredParameters.Any(p => p.GetParamNames().Any(t => t.paramName.StartsWith("/tracking/")));
        if (hasLoadedNative)
        {
            _logger.LogWarning("Native tracking parameters detected.");
        }

        var deprecatedParams = discoveredParameters.Count(p => p.Deprecated);

        _logger.LogInformation(discoveredParameters.Count + " parameters loaded.");
        if (deprecatedParams > 0)
        {
            _logger.LogWarning(
                deprecatedParams +
                " Legacy parameters detected. " +
                "Please consider updating the avatar to use the latest documented parameters.");
        }
    }
}