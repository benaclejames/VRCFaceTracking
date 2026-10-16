using Microsoft.Extensions.DependencyInjection;

namespace VRCFaceTracking.Contracts.Services;

/// <summary>
/// Implemented once by each platform head (Desktop, Android) and handed to <see cref="App"/> at startup.
/// The shared project registers portable defaults for every platform contract
/// (<see cref="INotificationService"/>, <see cref="IVrRuntimeService"/>, ...). A head swaps in its own
/// implementation with <c>services.Replace(...)</c>, since this runs after the shared registrations.
/// </summary>
public interface IPlatformServices
{
    void ConfigureServices(IServiceCollection services);
}
