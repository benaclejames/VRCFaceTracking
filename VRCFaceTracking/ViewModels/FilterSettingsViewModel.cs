using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.Contracts;

namespace VRCFaceTracking.ViewModels;
public class FilterSettingsViewModel : ObservableObject
{
    private readonly IFilterService _filter;

    private bool _enabled;
    private double _alpha; // Exponential smoothing factor for raw signal
    private double _beta; // Exponential smoothing factor for filtered signal
    private double _frequency; // Cutoff frequency for filtering

    public bool Enabled
    {
        get => _enabled;
        set 
        {
            _filter.Enabled = value;
            SetProperty(ref _enabled, value);
        }
    }

    public double Alpha
    {
        get => _alpha;
        set
        {
            if (value < 0.001f || value > 1.0f)
            {
                value = 0.001f;
            }

            _filter.Alpha = value;
            SetProperty(ref _alpha, value);
        }
    }

    public double Beta
    {
        get => _beta;
        set
        {
            if (value < 0.001f || value > 1.0f)
            {
                value = 0.001f;
            }

            _filter.Beta = value;
            SetProperty(ref _beta, value);
        }
    }

    public double Frequency
    {
        get => _frequency;
        set
        {
            if (value < 1.0f || value > 3.0f)
            {
                value = 1f;
            }

            _filter.Frequency = value;
            SetProperty(ref _frequency, value);
        }
    }

    public FilterSettingsViewModel(IFilterService newFilter)
    {
        if (_filter == null) // First time
        {
            _filter = newFilter;
            Task.Run(_filter.LoadCalibration).Wait(); // Hacky
            Enabled = _filter.Enabled;
            Alpha = _filter.Alpha;
            Beta = _filter.Beta;
            Frequency = _filter.Frequency;
        }
        else
        {
            _filter = newFilter;
        }

        PropertyChanged += async (sender, args) =>
        {
            if (args.PropertyName is not (nameof(Enabled) 
                or nameof(Alpha) 
                or nameof(Beta)
                or nameof(Frequency))) return;

            await _filter.SaveCalibration();
        };
    }
}
