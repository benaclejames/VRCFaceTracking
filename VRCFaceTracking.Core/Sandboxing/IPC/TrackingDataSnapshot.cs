using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.Core.Sandboxing.IPC;

public sealed class TrackingDataSnapshot
{
    private readonly object _dataLock = new();
    private readonly UnifiedTrackingData _data = new();

    public void Publish(UnifiedTrackingData source)
    {
        lock (_dataLock)
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
        lock (_dataLock)
        {
            ReplyUpdatePacket.Capture(_data, destination);
        }
    }
}
