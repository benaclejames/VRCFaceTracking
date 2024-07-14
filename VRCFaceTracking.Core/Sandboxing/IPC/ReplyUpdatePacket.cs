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

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = EXPRESSION_COUNT)]
        internal float[] Expression_Shapes;
    }

    private UpdateDataContiguous _contiguousUnifiedData = new ()
    {
        Expression_Shapes = new float[EXPRESSION_COUNT]
    };

    public override PacketType GetPacketType() => PacketType.ReplyUpdate;

    // We send a challenge to the vrcft host, and if we receive a reply with the same data, we consider the connection successfully ACKed.
    // In other words, this packet is the handshake begin and ACK packet.
    public override byte[] GetBytes()
    {
        // Build handshake packet

        byte[] packetTypeBytes = BitConverter.GetBytes((uint)GetPacketType());

        int packetSize = SIZE_PACKET_MAGIC + SIZE_PACKET_TYPE;

        // Update the internal data structure to match the current state of unified tracking
        _contiguousUnifiedData.Eye_Left_GazeX               = UnifiedTracking.Data.Eye.Left.Gaze.x;
        _contiguousUnifiedData.Eye_Left_GazeY               = UnifiedTracking.Data.Eye.Left.Gaze.y;
        _contiguousUnifiedData.Eye_Left_PupilDiameter_MM    = UnifiedTracking.Data.Eye.Left.PupilDiameter_MM;
        _contiguousUnifiedData.Eye_Left_Openness            = UnifiedTracking.Data.Eye.Left.Openness;
        
        _contiguousUnifiedData.Eye_Right_GazeX              = UnifiedTracking.Data.Eye.Right.Gaze.x;
        _contiguousUnifiedData.Eye_Right_GazeY              = UnifiedTracking.Data.Eye.Right.Gaze.y;
        _contiguousUnifiedData.Eye_Right_PupilDiameter_MM   = UnifiedTracking.Data.Eye.Right.PupilDiameter_MM;
        _contiguousUnifiedData.Eye_Right_Openness           = UnifiedTracking.Data.Eye.Right.Openness;

        _contiguousUnifiedData.Eye_MaxDilation              = UnifiedTracking.Data.Eye._maxDilation;
        _contiguousUnifiedData.Eye_MinDilation              = UnifiedTracking.Data.Eye._minDilation;

        // Copy face tracking
        for ( int i = 0; i < _contiguousUnifiedData.Expression_Shapes.Length; i++ )
        {
            _contiguousUnifiedData.Expression_Shapes[i] = UnifiedTracking.Data.Shapes[i].Weight;
        }

        // Convert _contiguousUnifiedData to bytes
        int sizeStruct = Marshal.SizeOf<UpdateDataContiguous>();
        byte[] sizeStructBytes = BitConverter.GetBytes(sizeStruct);
        byte[] arr = new byte[sizeStruct];

        var ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(sizeStruct);
            Marshal.StructureToPtr(_contiguousUnifiedData, ptr, false);
            Marshal.Copy(ptr, arr, 0, sizeStruct);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        packetSize = packetSize + sizeof(int) + sizeStruct;

        // Prepare buffer
        byte[] finalDataStream = new byte[packetSize];
        Buffer.BlockCopy(HANDSHAKE_MAGIC,   0, finalDataStream, 0,  SIZE_PACKET_MAGIC);     // Magic
        Buffer.BlockCopy(packetTypeBytes,   0, finalDataStream, 4,  SIZE_PACKET_TYPE);      // Packet Type
        Buffer.BlockCopy(sizeStructBytes,   0, finalDataStream, 8,  sizeof(int));           // Struct.Length
        Buffer.BlockCopy(arr,               0, finalDataStream, 12, sizeStruct);            // Data

        return finalDataStream;
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
        // If the eye state is valid

        // If dilation parameters are invalid
        if (_contiguousUnifiedData.Eye_MaxDilation < _contiguousUnifiedData.Eye_MinDilation)
        {
            return;
        }

        // Update the unified tracking to match our data structure
        UnifiedTracking.Data.Eye.Left.Gaze.x                = _contiguousUnifiedData.Eye_Left_GazeX;
        UnifiedTracking.Data.Eye.Left.Gaze.y                = _contiguousUnifiedData.Eye_Left_GazeY;
        UnifiedTracking.Data.Eye.Left.PupilDiameter_MM      = _contiguousUnifiedData.Eye_Left_PupilDiameter_MM;
        UnifiedTracking.Data.Eye.Left.Openness              = _contiguousUnifiedData.Eye_Left_Openness;
        
        UnifiedTracking.Data.Eye.Right.Gaze.x               = _contiguousUnifiedData.Eye_Right_GazeX;
        UnifiedTracking.Data.Eye.Right.Gaze.y               = _contiguousUnifiedData.Eye_Right_GazeY;
        UnifiedTracking.Data.Eye.Right.PupilDiameter_MM     = _contiguousUnifiedData.Eye_Right_PupilDiameter_MM;
        UnifiedTracking.Data.Eye.Right.Openness             = _contiguousUnifiedData.Eye_Right_Openness;

        UnifiedTracking.Data.Eye._maxDilation               = _contiguousUnifiedData.Eye_MaxDilation;
        UnifiedTracking.Data.Eye._minDilation               = _contiguousUnifiedData.Eye_MinDilation;
    }

    public void UpdateGlobalExpressionState()
    {
        // Copy face tracking
        for ( int i = 0; i < _contiguousUnifiedData.Expression_Shapes.Length; i++ )
        {
            UnifiedTracking.Data.Shapes[i].Weight = _contiguousUnifiedData.Expression_Shapes[i];
        }
    }
}
