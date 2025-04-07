using System;
using System.Threading.Tasks;

namespace VRCFaceTracking.Contracts.Services;

/// <summary>
/// Defines methods for managing the application lifecycle.
/// </summary>
public interface IApplicationLifecycleService
{
    /// <summary>
    /// Event raised when shutdown begins, can be used to update UI
    /// </summary>
    event EventHandler<ShutdownProgressEventArgs>? ShutdownProgress;

    /// <summary>
    /// Initiates the application shutdown sequence.
    /// This is the single entry point for all shutdown operations.
    /// </summary>
    /// <returns>A task representing the shutdown operation</returns>
    Task ShutdownAsync();
}

/// <summary>
/// Event arguments for shutdown progress updates
/// </summary>
public class ShutdownProgressEventArgs : EventArgs
{
    public ShutdownProgressEventArgs(string message, int progressPercentage)
    {
        Message = message;
        ProgressPercentage = progressPercentage;
    }

    /// <summary>
    /// A message describing the current shutdown step
    /// </summary>
    public string Message
    {
        get;
    }

    /// <summary>
    /// The progress percentage (0-100), or -1 for error
    /// </summary>
    public int ProgressPercentage
    {
        get;
    }
}