using Microsoft.Extensions.Logging;
using VRCFaceTracking.Contracts.Services;

namespace VRCFaceTracking.Services;

/// <summary>
/// Service responsible for managing the application lifecycle.
/// Acts as a facade for the ShutdownManager and other lifecycle-related components.
/// </summary>
public class ApplicationLifecycleService : IApplicationLifecycleService
{
    private readonly ILogger<ApplicationLifecycleService> _logger;
    private readonly IShutdownManager _shutdownManager;

    public ApplicationLifecycleService(
        ILogger<ApplicationLifecycleService> logger,
        IShutdownManager shutdownManager)
    {
        _logger = logger;
        _shutdownManager = shutdownManager;

        // Forward shutdown progress events
        _shutdownManager.ShutdownProgress += (sender, args) => ShutdownProgress?.Invoke(this, args);
    }

    /// <summary>
    /// Event raised when shutdown begins, can be used to update UI
    /// </summary>
    public event EventHandler<ShutdownProgressEventArgs>? ShutdownProgress;

    /// <summary>
    /// Initiates the application shutdown sequence.
    /// This is the single entry point for all shutdown operations.
    /// </summary>
    /// <returns>A task representing the shutdown operation</returns>
    public async Task ShutdownAsync()
    {
        _logger.LogInformation("Application shutdown requested");

        try
        {
            // Delegate to the shutdown manager
            await _shutdownManager.PerformShutdownAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during application shutdown");
            // Continue with shutdown despite errors
        }
    }
}