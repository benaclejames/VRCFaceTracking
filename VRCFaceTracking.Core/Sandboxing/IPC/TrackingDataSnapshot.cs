using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.Core.Sandboxing.IPC;

/// <summary>Reusable storage for the last completed module update.</summary>
public sealed class TrackingDataSnapshot
{
    private readonly object _lock = new();
    private readonly UnifiedTrackingData _data = new();

    /// <summary>Publishes raw data after the producer has finished updating it.</summary>
    public void Publish(UnifiedTrackingData source)
    {
        lock (_lock)
        {
            var eye = source.Eye;
            _data.Eye.Left = eye.Left;
            _data.Eye.Right = eye.Right;
            _data.Eye._maxDilation = eye._maxDilation;
            _data.Eye._minDilation = eye._minDilation;
            _data.Head = source.Head;
            Array.Copy(source.Shapes, _data.Shapes, _data.Shapes.Length);
        }
    }

    internal void CopyTo(ReplyUpdatePacket.UpdateDataContiguous destination)
    {
        lock (_lock)
        {
            ReplyUpdatePacket.Capture(_data, destination);
        }
    }
}
