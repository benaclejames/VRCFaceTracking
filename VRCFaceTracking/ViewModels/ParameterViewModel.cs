using CommunityToolkit.Mvvm.ComponentModel;

namespace VRCFaceTracking.ViewModels;

public partial class ParameterViewModel : ObservableRecipient
{
    public string? ParameterName { get; set; }

    [ObservableProperty] private float _parameterValue;
    
    public bool CanBeNegative { get; set; }
    public float MinValue => CanBeNegative ? -1 : 0;
}