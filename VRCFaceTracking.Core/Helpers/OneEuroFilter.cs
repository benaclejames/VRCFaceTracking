using System.ComponentModel;
using System.Runtime.CompilerServices;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.Core.Helpers;

public class OneEuroFilter : IFilterService
{
    private const double DEFAULT_ALPHA = 0.1f;
    private const double DEFAULT_BETA = 0.1f;
    private const double DEFAULT_FREQUENCY = 1.0f;

    public bool Enabled { get; set; } 
    public double Alpha { get; set; }
    public double Beta { get; set; }
    public double Frequency { get; set; }

    private double _prevTimestamp;
    private float _rawValue;
    private float _filteredValue;

    private readonly IDispatcherService _dispatcherService;
    private readonly ILocalSettingsService _localSettingsService;
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

    public OneEuroFilter(IDispatcherService dispatcherService, ILocalSettingsService localSettingsService)
    {
        _dispatcherService = dispatcherService;
        _localSettingsService = localSettingsService;

        Enabled = false;
        Alpha = DEFAULT_ALPHA;
        Beta = DEFAULT_BETA;
        Frequency = DEFAULT_FREQUENCY;
        _rawValue = 0f;
        _filteredValue = 0f;
    }

    public async Task SaveCalibration()
    {
        await _localSettingsService.SaveSettingAsync("FilterEnabled", Enabled);
        await _localSettingsService.SaveSettingAsync("FilterAlpha", Alpha);
        await _localSettingsService.SaveSettingAsync("FilterBeta", Beta);
        await _localSettingsService.SaveSettingAsync("FilterFrequency", Frequency);
        await Task.CompletedTask;
    }

    public async Task LoadCalibration()
    {
        Enabled = await _localSettingsService.ReadSettingAsync("FilterEnabled", false);
        Alpha = await _localSettingsService.ReadSettingAsync("FilterAlpha", DEFAULT_ALPHA);
        Beta = await _localSettingsService.ReadSettingAsync("FilterBeta", DEFAULT_BETA);
        Frequency = await _localSettingsService.ReadSettingAsync("FilterFrequency", DEFAULT_FREQUENCY);
        await Task.CompletedTask;
    }

    public void Filter()
    {
        if (!Enabled) return;

        FilterEye(UnifiedTracking.Data.Eye.Left);
        FilterEye(UnifiedTracking.Data.Eye.Right);

        // Modifying a collection while iterating over it is a bad idea
        // Should we create its own array for this?
        for (var i = 0; i < UnifiedTracking.Data.Shapes.Length; i++)
        {
            UnifiedTracking.Data.Shapes[i].Weight = FilterSingle(UnifiedTracking.Data.Shapes[i].Weight);
        }
    }

    private void FilterEye(UnifiedSingleEyeData eye)
    {
        eye.Gaze.x = FilterSingle(eye.Gaze.x);
        eye.Gaze.y = FilterSingle(eye.Gaze.y);
        eye.PupilDiameter_MM = FilterSingle(eye.PupilDiameter_MM);
        eye.Openness = FilterSingle(eye.Openness);
    }

    private float FilterSingle(float rawValue)
    {
        if (_prevTimestamp == 0) // Initial case
        {
            _rawValue = rawValue;
            _filteredValue = rawValue;
        }
        else
        {
            // Calculate the delta time
            var deltaTime = TimeTracker.ElapsedMilliseconds - _prevTimestamp;

            // Calculate the derivative of the raw signal
            var derivative = (rawValue - _rawValue) / deltaTime;

            // Update the cutoff frequency based on the derivative
            var cutoff = 1.0f / (1.0f + Math.Exp(-Beta * (Math.Abs(derivative) - Frequency)));

            // Exponential smoothing for raw and filtered signals
            var alphaDt = Math.Exp(-Alpha * deltaTime * cutoff);
            _filteredValue = (float)(alphaDt * rawValue + (1 - alphaDt) * _filteredValue);
        }

        // Update previous values
        _prevTimestamp = TimeTracker.ElapsedMilliseconds;
        _rawValue = rawValue;

        return _filteredValue;

    }
}
