using Android.Content;
using Android.Net.Wifi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AndroidApplication = Android.App.Application;

namespace VRCFaceTracking.Android.Services;

// Android filters inbound multicast to save battery unless an app holds a MulticastLock.
// Without it MulticastDnsService never sees VRChat's OSCQuery advertisements.
public class MulticastLockService(ILogger<MulticastLockService> logger) : IHostedService
{
    private WifiManager.MulticastLock? _lock;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var wifi = (WifiManager?)AndroidApplication.Context.GetSystemService(Context.WifiService);
        if (wifi == null)
        {
            logger.LogWarning("WifiManager unavailable; mDNS discovery of VRChat may not work.");
            return Task.CompletedTask;
        }

        _lock = wifi.CreateMulticastLock("vrcft-mdns");
        _lock.SetReferenceCounted(false);
        _lock.Acquire();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_lock is { IsHeld: true })
            _lock.Release();
        return Task.CompletedTask;
    }
}
