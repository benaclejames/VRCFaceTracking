using System;
using System.Threading.Tasks;
using VRCFaceTracking.Contracts.Services;

namespace VRCFaceTracking.Services;

/// <summary>
/// Interface for the shutdown manager that coordinates the application shutdown sequence.
/// </summary>
public interface IShutdownManager
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
    Task PerformShutdownAsync();
}