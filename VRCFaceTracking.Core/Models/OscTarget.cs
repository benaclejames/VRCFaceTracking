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

    /// Validates whether the provided address is a valid IP address or resolvable hostname.
    private static bool IsValidAddress(string address) =>
        !string.IsNullOrWhiteSpace(address) &&
        IPAddress.TryParse(address, out _);
}
