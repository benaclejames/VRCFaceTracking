using System.Net;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Library;
using VRCFaceTracking.Core.mDNS;
using VRCFaceTracking.Core.Models.ParameterDefinition;
using VRCFaceTracking.Core.OSC.Query.mDNS;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking.Core.OSC;

public partial class OscQueryService : ObservableObject
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    
    // Services
    private readonly ILogger _logger;
    private readonly OscQueryConfigParser _oscQueryConfigParser;
    private readonly MulticastDnsService _multicastDnsService;
    private static readonly Random Random = new ();
    
    [ObservableProperty] private IAvatarInfo _avatarInfo = new NullAvatarDef("Loading...", "Loading...");
    [ObservableProperty] private List<Parameter> _avatarParameters;

    private readonly IOscTarget _oscTarget;
 
    // Local vars
    private readonly OscRecvService _recvService;
    private readonly ILocalSettingsService _settingsService;
    private readonly HttpHandler _httpHandler;
    
    public OscQueryService(
        ILogger<OscQueryService> logger,
        OscQueryConfigParser oscQueryConfigParser,
        IOscTarget oscTarget,
        MulticastDnsService multicastDnsService,
        OscRecvService recvService,
        ILocalSettingsService settingsService,
        HttpHandler httpHandler
    )
    {
        _oscQueryConfigParser = oscQueryConfigParser;
        _logger = logger;
        _recvService = recvService;
        _recvService.OnMessageReceived = HandleNewMessage;
        _multicastDnsService = multicastDnsService;
        _oscTarget = oscTarget;
        _settingsService = settingsService;
        _httpHandler = httpHandler;
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

        _multicastDnsService.OnVrcClientDiscovered += FirstClientDiscovered;
        
        _multicastDnsService.SendQuery("_oscjson._tcp.local");
            
        _logger.LogDebug("OSC Service Initialized with result {0}", result);
        await Task.CompletedTask;
    }
    
    private void FirstClientDiscovered()
    {
        _multicastDnsService.OnVrcClientDiscovered -= FirstClientDiscovered;
        
        _logger.LogInformation("OSCQuery detected. Setting port negotiation to autopilot.");
        
        var randomStr = new string(Enumerable.Repeat(chars, 6).Select(s => s[Random.Next(s.Length)]).ToArray());
        _httpHandler.OnHostInfoQueried += HandleNewAvatar;
        
        var recvEndpoint = _recvService.UpdateTarget(new IPEndPoint(IPAddress.Parse(_oscTarget.DestinationAddress), 0));
        if (recvEndpoint == null)
        {
            _logger.LogError("Very strange. We were unable to bind to a random port.");
            recvEndpoint = new IPEndPoint(IPAddress.Parse(_oscTarget.DestinationAddress), _oscTarget.InPort);
        }
        
        // TODO: Move this somewhere more appropriate
        var listener = new TcpListener(IPAddress.Any, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        _httpHandler.SetAppName("VRCFT-" + randomStr);
        _httpHandler.BindTo($"http://127.0.0.1:{port}/", recvEndpoint.Port);
        
        // Advertise our OSC JSON and OSC endpoints (OSC JSON to display the silly lil popup in-game)
        _multicastDnsService.Advertise("_oscjson._tcp", "VRCFT-"+randomStr, port, IPAddress.Loopback);
        _multicastDnsService.Advertise("_osc._udp", "VRCFT-"+randomStr, recvEndpoint.Port, IPAddress.Loopback);
        
        HandleNewAvatar();
    }

    private async void HandleNewAvatar()
    {
        _httpHandler.OnHostInfoQueried -= HandleNewAvatar;
        var newAvatar = await _oscQueryConfigParser.ParseAvatar("");
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
            case "/vrcft/settings/forceRelevant": // Endpoint for external tools to force vrcft to send all parameters
                if (msg.Value is bool relevancy)
                {
                    ParameterSenderService.AllParametersRelevant = relevancy;
                }

                break;
            case "/avatar/parameters/EyeTrackingActive":
                if (UnifiedLibManager.EyeStatus != ModuleState.Uninitialized && msg.Value is bool eyeValue)
                {
                    UnifiedLibManager.EyeStatus = eyeValue ? ModuleState.Active : ModuleState.Idle;
                }
                break;
            case "/avatar/parameters/LipTrackingActive":
            case "/avatar/parameters/ExpressionTrackingActive":
                if (UnifiedLibManager.ExpressionStatus != ModuleState.Uninitialized &&
                    msg.Value is bool expressionValue)
                {
                    UnifiedLibManager.ExpressionStatus = expressionValue ? ModuleState.Active : ModuleState.Idle;
                }
                break;
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

        var deprecatedParams = discoveredParameters.Where(p => p.Deprecated).ToList();

        _logger.LogInformation(discoveredParameters.Count + " parameters loaded.");
        if (deprecatedParams.Any())
        {
            _logger.LogWarning(
                deprecatedParams.Count +
                " Legacy parameters detected. " +
                "Please consider updating the avatar to use the latest documented parameters.");
            _logger.LogDebug($"Loaded deprecated parameters: {string.Join(", ", deprecatedParams.SelectMany(x => x.GetParamNames()))}");
        }
    }
}