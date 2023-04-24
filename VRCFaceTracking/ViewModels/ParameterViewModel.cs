using CommunityToolkit.Mvvm.ComponentModel;

namespace VRCFaceTracking.ViewModels;

public class ParameterViewModel : ObservableRecipient
{
    public string? ParameterName { get; set; }

    private float _parameterValue;
    public float ParameterValue
    {
        get => _parameterValue;
        set => SetProperty(ref _parameterValue, value);
    }
    
    public bool CanBeNegative { get; set; }
    public float MinValue => CanBeNegative ? -1 : 0;
}