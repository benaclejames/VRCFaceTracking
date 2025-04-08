using Microsoft.Extensions.Logging;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.OSC.Query.mDNS;
using VRCFaceTracking.Core.Services;

namespace VRCFaceTracking.Services;

/// <summary>
///     Centralized manager for application shutdown operations.
///     Handles coordinating the shutdown sequence across all components.
/// </summary>
public class ShutdownManager : IShutdownManager
{
    private readonly HttpHandler _httpHandler;
    private readonly ILibManager _libManager;
    private readonly ILogger<ShutdownManager> _logger;
    private readonly IMainService _mainService;
    private readonly OscQueryService _oscQueryService;
    private readonly OscRecvService _oscRecvService;
    private readonly ParameterSenderService _parameterSenderService;
    private readonly SemaphoreSlim _shutdownLock = new(1, 1);

    private bool _isShuttingDown;

    public ShutdownManager(
        ILogger<ShutdownManager> logger,
        IMainService mainService,
        ILibManager libManager,
        HttpHandler httpHandler,
        OscQueryService oscQueryService,
        ParameterSenderService parameterSenderService,
        OscRecvService oscRecvService)
    {
        _logger = logger;
        _mainService = mainService;
        _libManager = libManager;
        _httpHandler = httpHandler;
        _oscQueryService = oscQueryService;
        _parameterSenderService = parameterSenderService;
        _oscRecvService = oscRecvService;
    }

    /// <summary>
    ///     Event raised when shutdown begins, can be used to update UI
    /// </summary>
    public event EventHandler<ShutdownProgressEventArgs>? ShutdownProgress;

    /// <summary>
    ///     Initiates the application shutdown sequence.
    ///     This is the single entry point for all shutdown operations.
    /// </summary>
    /// <returns>A task representing the shutdown operation</returns>
    public async Task PerformShutdownAsync()
    {
        // Ensure we only run shutdown once
        await _shutdownLock.WaitAsync();
        try
        {
            if (_isShuttingDown)
            {
                _logger.LogInformation("Shutdown already in progress, ignoring duplicate request");
                return;
            }

            _isShuttingDown = true;

            _logger.LogInformation("Beginning application shutdown sequence");
            RaiseProgressEvent("Beginning shutdown sequence", 0);

            // First stop services that might be using modules
            await StopServicesAsync();
            RaiseProgressEvent("Services stopped", 25);

            // Then teardown modules
            await TeardownModulesAsync();
            RaiseProgressEvent("Modules unloaded", 50);

            // Cleanup main service
            await FinalCleanupAsync();
            RaiseProgressEvent("Cleanup completed", 75);

            // Flush logs
            await FlushLogsAsync();
            RaiseProgressEvent("Application shutdown complete", 100);

            _logger.LogInformation("Shutdown sequence completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during shutdown sequence");
            RaiseProgressEvent($"Error during shutdown: {ex.Message}", -1);
            // Continue with shutdown despite errors
        }
        finally
        {
            _shutdownLock.Release();
        }
    }

    /// <summary>
    ///     Flushes any pending logs
    /// </summary>
    private async Task FlushLogsAsync()
    {
        _logger.LogDebug("Stopping host and flushing logs");

        try
        {
            // Ensure Sentry logs are flushed
            await SentrySdk.FlushAsync(TimeSpan.FromSeconds(3));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during host shutdown or log flushing");
        }
    }

    private async Task StopServicesAsync()
    {
        _logger.LogDebug("Stopping application services");

        try
        {
            // Use Task.Run to avoid deadlocks if these are blocking operations
            await Task.Run(() =>
            {
                _parameterSenderService?.StopAsync(CancellationToken.None).Wait(TimeSpan.FromSeconds(3));
                _oscRecvService?.StopAsync(CancellationToken.None).Wait(TimeSpan.FromSeconds(3));
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error stopping services, continuing shutdown");
        }
    }

    private async Task TeardownModulesAsync()
    {
        _logger.LogDebug("Tearing down modules");

        try
        {
            // Use Task.Run for potentially blocking operations
            await Task.Run(() =>
            {
                _libManager?.TeardownAllAndResetAsync();
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error tearing down modules, continuing shutdown");
        }
    }

    private async Task FinalCleanupAsync()
    {
        _logger.LogDebug("Performing final cleanup");

        try
        {
            // Give each component a limited time to clean up
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            await Task.Run(async () =>
            {
                try
                {
                    // Dispose HTTP handler
                    _httpHandler?.Dispose();

                    // Finally, teardown main service
                    await _mainService?.Teardown();
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Cleanup timed out, forcing shutdown");
                }
            }, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during final cleanup, forcing shutdown");
        }
    }

    /// <summary>
    ///     Raises the ShutdownProgress event with the provided message and progress percentage
    /// </summary>
    private void RaiseProgressEvent(string message, int progressPercentage) =>
        ShutdownProgress?.Invoke(this, new ShutdownProgressEventArgs(message, progressPercentage));
}
