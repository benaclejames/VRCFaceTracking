﻿using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.ViewModels;

public class OscViewModel : ObservableRecipient
{
    private readonly IOSCService _oscService;
    
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

            _oscService.InPort = value;
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
            
            _oscService.OutPort = value;
            SetProperty(ref _outPort, value);
        }
    }

    public string Address
    {
        get => _address;
        set
        {
            _oscService.Address = value;
            SetProperty(ref _address, value);
        }
    }
    
    public OscViewModel(IOSCService osc)
    {
        _oscService = osc;
        
        _inPort = _oscService.InPort;
        _outPort = _oscService.OutPort;
        _address = _oscService.Address;
        
        PropertyChanged += async (sender, args) =>
        {
            // If the property changed is either the in port, out port, or the address, then we need to save the settings
            if (args.PropertyName is not (nameof(RecvPort) or nameof(SendPort) or nameof(Address)))
                return;

            await _oscService.SaveSettings();
            await _oscService.InitializeAsync();
        };
    }
}