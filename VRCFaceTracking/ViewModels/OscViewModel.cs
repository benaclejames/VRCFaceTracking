using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking.ViewModels;

public class OscViewModel : ObservableRecipient
{
    private readonly ParameterOutputService _parameterOutputService;
    
    private int _inPort, _outPort;
    private string _address;
    
    public int RecvPort
    {
        get => _inPort;
        set
        { 
            // Ensure we're within the valid range
            if (value < 0 || value > 65535)
            {
                // If we're not, then just set to default which is 9001, This won't be reflected in the UI but saves us crashing
                value = 9001;
            }

            _parameterOutputService.InPort = value;
            SetProperty(ref _inPort, value);
        }
    }

    public int SendPort
    {
        get => _outPort;
        set
        {
            if (value < 0 || value > 65535)
            {
                value = 9000;
            }
            
            _parameterOutputService.OutPort = value;
            SetProperty(ref _outPort, value);
        }
    }

    public string Address
    {
        get => _address;
        set
        {
            _parameterOutputService.DestinationAddress = value;
            SetProperty(ref _address, value);
        }
    }
    
    public OscViewModel(ParameterOutputService parameterOutput)
    {
        _parameterOutputService = parameterOutput;
        
        _inPort = _parameterOutputService.InPort;
        _outPort = _parameterOutputService.OutPort;
        _address = _parameterOutputService.DestinationAddress;
        
        PropertyChanged += async (sender, args) =>
        {
            // If the property changed is either the in port, out port, or the address, then we need to save the settings
            if (args.PropertyName is not (nameof(RecvPort) or nameof(SendPort) or nameof(Address)))
                return;

            await _parameterOutputService.SaveSettings();
            await _parameterOutputService.InitializeAsync();
        };
    }
}