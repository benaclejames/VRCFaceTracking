using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.Core.Sandboxing.IPC;
public class ReplyUpdatePacket : IpcPacket
{
    const int EXPRESSION_COUNT = (int)UnifiedExpressions.Max + 1;
    const float INVALID_FLOAT = 0xFFFFFFFF;

    [StructLayout(LayoutKind.Sequential)]
    internal class UpdateDataContiguous
    {
        internal float Eye_MaxDilation;
        internal float Eye_MinDilation;

        internal float Eye_Left_GazeX;
        internal float Eye_Left_GazeY;
        internal float Eye_Left_PupilDiameter_MM;
        internal float Eye_Left_Openness;

        internal float Eye_Right_GazeX;
        internal float Eye_Right_GazeY;
        internal float Eye_Right_PupilDiameter_MM;
        internal float Eye_Right_Openness;

        internal float Head_Yaw;
        internal float Head_Pitch;
        internal float Head_Roll;

        internal float Head_PosX;
        internal float Head_PosY;
        internal float Head_PosZ;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EXPRESSION_COUNT)]
        internal float[] Expression_Shapes;
    }

    private UpdateDataContiguous _contiguousUnifiedData = new ()
    {
        Expression_Shapes = new float[EXPRESSION_COUNT]
    };

    private readonly TrackingDataSnapshot? _snapshot;

    public ReplyUpdatePacket()
    {
    }

    public ReplyUpdatePacket(TrackingDataSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _snapshot = snapshot;
    }

    public override PacketType GetPacketType() => PacketType.ReplyUpdate;

    public override byte[] GetBytes()
    {
        if (_snapshot is null)
            Capture(UnifiedTracking.Data, _contiguousUnifiedData);
        else
            _snapshot.CopyTo(_contiguousUnifiedData);

        const int sizeStruct = (16 + EXPRESSION_COUNT) * sizeof(float);
        byte[] finalDataStream = new byte[12 + sizeStruct];
        HANDSHAKE_MAGIC.CopyTo(finalDataStream, 0);
        BitConverter.TryWriteBytes(finalDataStream.AsSpan(4), (uint)GetPacketType());
        BitConverter.TryWriteBytes(finalDataStream.AsSpan(8), sizeStruct);

        var data = _contiguousUnifiedData;
        Span<float> fields = stackalloc float[]
        {
            data.Eye_MaxDilation, data.Eye_MinDilation,
            data.Eye_Left_GazeX, data.Eye_Left_GazeY, data.Eye_Left_PupilDiameter_MM, data.Eye_Left_Openness,
            data.Eye_Right_GazeX, data.Eye_Right_GazeY, data.Eye_Right_PupilDiameter_MM, data.Eye_Right_Openness,
            data.Head_Yaw, data.Head_Pitch, data.Head_Roll, data.Head_PosX, data.Head_PosY, data.Head_PosZ,
        };
        MemoryMarshal.AsBytes(fields).CopyTo(finalDataStream.AsSpan(12));
        MemoryMarshal.AsBytes(data.Expression_Shapes.AsSpan()).CopyTo(finalDataStream.AsSpan(12 + fields.Length * sizeof(float)));

        return finalDataStream;
    }

    internal static void Capture(UnifiedTrackingData source, UpdateDataContiguous destination)
    {
        var eye = source.Eye;
        destination.Eye_Left_GazeX               = eye.Left.Gaze.x;
        destination.Eye_Left_GazeY               = eye.Left.Gaze.y;
        destination.Eye_Left_PupilDiameter_MM    = eye.Left.PupilDiameter_MM;
        destination.Eye_Left_Openness            = eye.Left.Openness;

        destination.Eye_Right_GazeX              = eye.Right.Gaze.x;
        destination.Eye_Right_GazeY              = eye.Right.Gaze.y;
        destination.Eye_Right_PupilDiameter_MM   = eye.Right.PupilDiameter_MM;
        destination.Eye_Right_Openness           = eye.Right.Openness;

        destination.Eye_MaxDilation              = eye._maxDilation;
        destination.Eye_MinDilation              = eye._minDilation;

        var head = source.Head;
        destination.Head_Yaw                     = head.HeadYaw;
        destination.Head_Pitch                   = head.HeadPitch;
        destination.Head_Roll                    = head.HeadRoll;
        destination.Head_PosX                    = head.HeadPosX;
        destination.Head_PosY                    = head.HeadPosY;
        destination.Head_PosZ                    = head.HeadPosZ;

        var shapes = source.Shapes;
        for (int i = 0; i < destination.Expression_Shapes.Length; i++)
            destination.Expression_Shapes[i] = shapes[i].Weight;
    }

    public override void Decode(in byte[] data)
    {
        int structSize = BitConverter.ToInt32(data, 8);
        
        var ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(structSize);
            Marshal.Copy(data, 12, ptr, structSize);
            Marshal.PtrToStructure<UpdateDataContiguous>(ptr, _contiguousUnifiedData);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    public void UpdateGlobalEyeState()
    {
        lock (UnifiedTracking.DataLock)
        {
            ApplyEyeState();
        }
    }

    private void ApplyEyeState()
    {
        // If the eye state is valid

        // If dilation parameters are invalid
        if ( _contiguousUnifiedData.Eye_MaxDilation != INVALID_FLOAT &&
            _contiguousUnifiedData.Eye_MinDilation != INVALID_FLOAT &&
            _contiguousUnifiedData.Eye_MaxDilation < _contiguousUnifiedData.Eye_MinDilation )
        {
            return;
        }

        // Update the unified tracking to match our data structure
        if ( _contiguousUnifiedData.Eye_Left_GazeX != INVALID_FLOAT )
            UnifiedTracking.Data.Eye.Left.Gaze.x = _contiguousUnifiedData.Eye_Left_GazeX;
        if ( _contiguousUnifiedData.Eye_Left_GazeY != INVALID_FLOAT )
            UnifiedTracking.Data.Eye.Left.Gaze.y = _contiguousUnifiedData.Eye_Left_GazeY;
        if ( _contiguousUnifiedData.Eye_Left_PupilDiameter_MM != INVALID_FLOAT )
            UnifiedTracking.Data.Eye.Left.PupilDiameter_MM = _contiguousUnifiedData.Eye_Left_PupilDiameter_MM;
        if ( _contiguousUnifiedData.Eye_Left_Openness != INVALID_FLOAT )
            UnifiedTracking.Data.Eye.Left.Openness = _contiguousUnifiedData.Eye_Left_Openness;

        if ( _contiguousUnifiedData.Eye_Right_GazeX != INVALID_FLOAT )
            UnifiedTracking.Data.Eye.Right.Gaze.x = _contiguousUnifiedData.Eye_Right_GazeX;
        if ( _contiguousUnifiedData.Eye_Right_GazeY != INVALID_FLOAT )
            UnifiedTracking.Data.Eye.Right.Gaze.y = _contiguousUnifiedData.Eye_Right_GazeY;
        if ( _contiguousUnifiedData.Eye_Right_PupilDiameter_MM != INVALID_FLOAT )
            UnifiedTracking.Data.Eye.Right.PupilDiameter_MM = _contiguousUnifiedData.Eye_Right_PupilDiameter_MM;
        if ( _contiguousUnifiedData.Eye_Right_Openness != INVALID_FLOAT )
            UnifiedTracking.Data.Eye.Right.Openness = _contiguousUnifiedData.Eye_Right_Openness;

        if ( _contiguousUnifiedData.Eye_MaxDilation != INVALID_FLOAT )
            UnifiedTracking.Data.Eye._maxDilation = _contiguousUnifiedData.Eye_MaxDilation;
        if ( _contiguousUnifiedData.Eye_MinDilation != INVALID_FLOAT )
            UnifiedTracking.Data.Eye._minDilation = _contiguousUnifiedData.Eye_MinDilation;

        // Eye-derived expression shapes
        for ( int i = (int)UnifiedExpressions.EyeSquintRight; i <= (int)UnifiedExpressions.BrowOuterUpLeft; i++ )
        {
            if ( _contiguousUnifiedData.Expression_Shapes[i] != INVALID_FLOAT )
                UnifiedTracking.Data.Shapes[i].Weight = _contiguousUnifiedData.Expression_Shapes[i];
        }
    }

    public void UpdateHeadState()
    {
        if ( _contiguousUnifiedData.Head_Yaw != INVALID_FLOAT )
            UnifiedTracking.Data.Head.HeadYaw = _contiguousUnifiedData.Head_Yaw;
        if ( _contiguousUnifiedData.Head_Pitch != INVALID_FLOAT )
            UnifiedTracking.Data.Head.HeadPitch = _contiguousUnifiedData.Head_Pitch;
        if ( _contiguousUnifiedData.Head_Roll != INVALID_FLOAT )
            UnifiedTracking.Data.Head.HeadRoll = _contiguousUnifiedData.Head_Roll;
            
        if ( _contiguousUnifiedData.Head_PosX != INVALID_FLOAT )
            UnifiedTracking.Data.Head.HeadPosX = _contiguousUnifiedData.Head_PosX;
        if ( _contiguousUnifiedData.Head_PosY != INVALID_FLOAT )
            UnifiedTracking.Data.Head.HeadPosY = _contiguousUnifiedData.Head_PosY;
        if ( _contiguousUnifiedData.Head_PosZ != INVALID_FLOAT )
            UnifiedTracking.Data.Head.HeadPosZ = _contiguousUnifiedData.Head_PosZ;
    }

    public void UpdateGlobalExpressionState()
    {
        // Copy face tracking
        for ( int i = (int)UnifiedExpressions.BrowOuterUpLeft; i < _contiguousUnifiedData.Expression_Shapes.Length; i++ )
        {
            if ( _contiguousUnifiedData.Expression_Shapes[i] != INVALID_FLOAT)
                UnifiedTracking.Data.Shapes[i].Weight = _contiguousUnifiedData.Expression_Shapes[i];
        }
    }
}
