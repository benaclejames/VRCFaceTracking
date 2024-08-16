using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.SDK;

namespace VRCFaceTracking.Core.Params.Data;

/// <summary>
/// Container of all functions and structures retaining to mutating the incoming Expression Data to be usable for output parameters.
/// </summary>
public partial class UnifiedTrackingMutator : ObservableObject
{
    [ObservableProperty]
    [property: SavedSetting("MutatorEnabled", true)]
    private bool _enabled;

    private readonly ILogger<UnifiedTrackingMutator> _logger;
    private readonly ILocalSettingsService _localSettingsService;
    private UnifiedTrackingData _inputBuffer;
    public TrackingMutation[] _mutations = new TrackingMutation[0];

    public UnifiedTrackingMutator(ILogger<UnifiedTrackingMutator> logger, ILocalSettingsService localSettingsService)
    {
        UnifiedTracking.Mutator = this;
        _logger = logger;
        _localSettingsService = localSettingsService;
            
        Enabled = false;
        _inputBuffer = new UnifiedTrackingData();
        _mutations = TrackingMutation.GetImplementingMutations(true);
    }

    /// <summary>
    /// Takes in the latest base expression Weight data from modules and mutates into the Weight data for output parameters.
    /// </summary>
    /// <returns> Mutated Expression Data. </returns>
    public UnifiedTrackingData MutateData(UnifiedTrackingData input)
    {
        if (!Enabled)
        {
            return input;
        }

        _inputBuffer.CopyPropertiesOf(input);

        foreach (var mutator in _mutations) 
        { 
            if (mutator.IsActive)
                mutator.MutateData(ref _inputBuffer);
        }

        return _inputBuffer;
    }

    public async Task Save()
    {
        _logger.LogDebug("Saving mutation configuration...");
        await _localSettingsService.Save(this);
        foreach (var mutation in _mutations)
        {
            _logger.LogInformation($"Saving {mutation.Name} data.");
            await mutation.SaveData(_localSettingsService);
        }
        _logger.LogDebug("Mutation data saved.");
    }

    public async Task<Task> Initialize()
    {
        // Try to load config and propogate data into Unified if they exist.
        _logger.LogDebug("Initializing mutations...");
        foreach (var mutation in _mutations)
        {
            _logger.LogInformation($"Initializing {mutation.Name}");
            await mutation.Initialize(UnifiedTracking.Data);
            mutation.IsActive = true;
        }
        _logger.LogDebug("Mutations initialized successfully.");
        return Task.CompletedTask;
    }

    public async void Load()
    {
        // Try to load config and propogate data into Unified if they exist.
        _logger.LogDebug("Loading mutation data...");
        await _localSettingsService.Load(this);
        foreach (var mutation in _mutations)
        {
            _logger.LogInformation($"Loading {mutation.Name}");
            mutation.Logger = _logger;
            await mutation.LoadData(_localSettingsService);
        }
        _logger.LogDebug("Mutation data loaded.");
    }
}