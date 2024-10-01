using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Params.Data.Mutation;

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
    public ObservableCollection<TrackingMutation> _mutations = new();

    public UnifiedTrackingMutator(ILogger<UnifiedTrackingMutator> logger, ILocalSettingsService localSettingsService)
    {
        UnifiedTracking.Mutator = this;
        _logger = logger;
        _localSettingsService = localSettingsService;
            
        Enabled = false;
        _inputBuffer = new UnifiedTrackingData();
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
            await _localSettingsService.SaveSettingAsync(mutation.Name, mutation, true);
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
        var mutations = TrackingMutation.GetImplementingMutations(true);
        await _localSettingsService.Load(this);

        for (int i = 0; i < mutations.Length; i++)
        {
            var mutation = mutations[i];
            try
            {
                _logger.LogInformation($"Loading {mutation.Name}");

                Type mutationType = mutation.GetType();

                MethodInfo method = typeof(ILocalSettingsService)
                    .GetMethod("ReadSettingAsync")
                    .MakeGenericMethod(mutationType);

                var task = (Task)method.Invoke(_localSettingsService, new object[] { mutation.Name, mutation, true });
                await task.ConfigureAwait(false);

                PropertyInfo resultProperty = task.GetType().GetProperty("Result");
                var typedMutation = resultProperty.GetValue(task);

                mutation = (TrackingMutation)typedMutation;

                mutation.Logger = _logger;
                mutation.CreateProperties();
                _mutations.Add(mutation);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Creating new mutation data. {ex.Message}");
                mutation.CreateProperties();
            }
        }
        _logger.LogDebug("Mutation data loaded.");
    }
}