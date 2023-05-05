using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.ViewModels;

public class CalibrationSettingsViewModel : ObservableObject, ICalibrationSettings
{
    private bool _enabled;
    public bool Enabled
    {
        get => _enabled;
        set => SetProperty(ref _enabled, value);
    }

    private bool _continuousCalibration;
    public bool ContinuousCalibration
    {
        get => _continuousCalibration;
        set => SetProperty(ref _continuousCalibration, value);
    }

    private float _influence;
    public float Influence
    {
        get => _influence;
        set => SetProperty(ref _influence, value);
    }
    
    public CalibrationSettingsViewModel()
    {
        Enabled = true;
        ContinuousCalibration = true;
    }
}