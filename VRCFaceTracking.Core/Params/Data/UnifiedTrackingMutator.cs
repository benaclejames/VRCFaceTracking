using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;
using VRCFaceTracking.Mutators;

namespace VRCFaceTracking
{
    /// <summary>
    /// Container of all functions and structures retaining to mutating the incoming Expression Data to be usable for output parameters.
    /// </summary>
    public class UnifiedTrackingMutator : INotifyPropertyChanged
    {
        public UnifiedTrackingData trackingDataPrevious = new();
        public UnifiedTrackingData trackingDataBuffer = new();
        public UnifiedMutationConfig mutatorConfig = new();
        public List<IUnifiedMutation> mutations = new();

        private float _calibrationWeight;
        public float CalibrationWeight
        {
            get => _calibrationWeight;
            set => SetField(ref _calibrationWeight, value);
        }

        private bool _continuousCalibration;
        public bool ContinuousCalibration
        {
            get => _continuousCalibration;
            set => SetField(ref _continuousCalibration, value);
        }

        private bool _enabled;
        public bool Enabled
        {
            get => _enabled;
            set => SetField(ref _enabled, value);
        }

        private readonly ILogger<UnifiedTrackingMutator> _logger;
        private readonly IDispatcherService _dispatcherService;
        private readonly ILocalSettingsService _localSettingsService;

        public UnifiedTrackingMutator(ILogger<UnifiedTrackingMutator> logger, IDispatcherService dispatcherService, ILocalSettingsService localSettingsService)
        {
            UnifiedTracking.Mutator = this;
            trackingDataPrevious.CopyPropertiesOf(UnifiedTracking.Data);
            trackingDataBuffer.CopyPropertiesOf(UnifiedTracking.Data);

            mutations.Add(new CalibrationMutator());
            //mutations.Add(new SmoothingMutator());

            foreach (var item in mutations)
                item.Initialize();

            _logger = logger;
            _dispatcherService = dispatcherService;
            _localSettingsService = localSettingsService;

            Enabled = false;
            ContinuousCalibration = true;
            CalibrationWeight = 0.2f;
        }

        /// <summary>
        /// Takes in the latest expression data and applies mutations.
        /// </summary>
        /// <returns> Mutated Expression Data. </returns>
        public UnifiedTrackingData MutateData(UnifiedTrackingData input)
        {
            if (!Enabled) 
                return input;

            trackingDataBuffer.CopyPropertiesOf(input);

            foreach (var item in mutations)
                item.Mutate(ref trackingDataBuffer, trackingDataPrevious, _logger);

            trackingDataPrevious.CopyPropertiesOf(trackingDataBuffer);
            return trackingDataBuffer;
        }

        public async Task SaveMutations()
        {
            _logger.LogDebug("Saving mutation data.");

            foreach (IUnifiedMutation mut in mutations)
                mutatorConfig.MutationInfo.Add(new UnifiedMutationInfo 
                { 
                    MutationName = mut.Name,
                    Properties = mut.GetProperties()
                });

            await _localSettingsService.SaveSettingAsync("CalibrationEnabled", Enabled);
            //await _localSettingsService.SaveSettingAsync("CalibrationWeight", CalibrationWeight);
            //await _localSettingsService.SaveSettingAsync("ContinuousCalibrationEnabled", ContinuousCalibration);
            await _localSettingsService.SaveSettingAsync("MutationConfig", mutatorConfig, true);
        }

        public async void LoadMutations()
        {
            // Try to load config and propogate data into Unified if they exist.
            _logger.LogDebug("Reading mutation data...");
            Enabled = await _localSettingsService.ReadSettingAsync<bool>("CalibrationEnabled");
            //CalibrationWeight = await _localSettingsService.ReadSettingAsync<float>("CalibrationWeight", 0.2f);
            //ContinuousCalibration = await _localSettingsService.ReadSettingAsync<bool>("ContinuousCalibrationEnabled", true);
            mutatorConfig = await _localSettingsService.ReadSettingAsync<UnifiedMutationConfig>("MutationConfig", new());

            foreach (IUnifiedMutation mut in mutations)
                foreach (UnifiedMutationInfo info in mutatorConfig.MutationInfo)
                    if (info.MutationName == mut.Name)
                    {
                        mut.SetProperties(info.Properties);
                        _logger.LogDebug("Successfully applied config to mutation {mut}", mut.Name);
                    }

            _logger.LogDebug("Mutation data loaded.");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            _dispatcherService.Run(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public void InitializeCalibration()
        {
            foreach (var item in mutations)
            {
                _logger.LogInformation("Activating mutation: {mut}.", item.Name);
                item.Mutable = true;
            }
            //foreach(IUnifiedMutation mutation in mutatorConfig.Mutations)
            //    mutation.Reset();
        }
    }
}
