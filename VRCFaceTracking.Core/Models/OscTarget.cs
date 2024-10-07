using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Contracts.Services;
using System.Net;

namespace VRCFaceTracking.Core.Models;

public partial class OscTarget : ObservableObject, IOscTarget
{
    [ObservableProperty] private bool _isConnected;

    [ObservableProperty] [property: SavedSetting("OSCInPort", 9001)]

    private int _inPort;

    [ObservableProperty] [property: SavedSetting("OSCOutPort", 9000)]

    private int _outPort;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InPort))] [NotifyPropertyChangedFor(nameof(OutPort))]

    [property: SavedSetting("OSCAddress", "127.0.0.1")]
    private string _destinationAddress;

    private const string DefaultAddress = "127.0.0.1";
    private const int DefaultInPort = 9001;
    private const int DefaultOutPort = 9000;
    private bool _isReverting = false;

    public OscTarget(ILocalSettingsService localSettingsService)
    {
        PropertyChanged += (_, _) => localSettingsService.Save(this);
    }

    /// Called after the DestinationAddress property has changed.
    /// It validates the new address and reverts it to the default if invalid.
    partial void OnDestinationAddressChanged(string oldValue, string newValue)
    {
        // Prevent re-entrancy if we're already reverting
        if (_isReverting)
        {
            return;
        }

        _isReverting = true;

        // Check if the new address is valid and revert if invalid
        if (!IsValidAddress(newValue))
        {
            DestinationAddress = DefaultAddress;
        }

        _isReverting = false;
    }

    /// Called after the InPort property has changed.
    /// It checks if the InPort is valid and reverts to the default if it's invalid (less than 1).
    partial void OnInPortChanged(int oldValue, int newValue)
    {
        if (newValue < 1)
        {
            InPort = DefaultInPort;
        }
    }

    /// Called after the OutPort property has changed.
    /// It checks if the OutPort is valid and reverts to the default if it's invalid (less than 1).
    partial void OnOutPortChanged(int oldValue, int newValue)
    {
        if (newValue < 1)
        {
            OutPort = DefaultOutPort;
        }
    }

    /// Validates whether the provided address is a valid IP address or resolvable hostname.
    private static bool IsValidAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return false;
        }

        // Check if it's a valid IP address
        if (IPAddress.TryParse(address, out _))
        {
            return true;
        }

        // Check if it's a resolvable hostname
        try
        {
            var hostEntry = Dns.GetHostEntry(address);
            return true;
        }
        catch
        {
            // DNS resolution failed
            return false;
        }
    }
}
