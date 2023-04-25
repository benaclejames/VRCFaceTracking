using System.Runtime.InteropServices;
using VRCFaceTracking.Core.Types;
using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Core.OSC;

public enum OscValueType : byte {
    Int = 0,
    Float = 1,
    Bool = 2,
    String = 3,
    Vector2 = 4,
    Vector3 = 5,
    Vector4 = 6,
    Vector6 = 7,
}

[StructLayout(LayoutKind.Sequential)]
public struct OscValue {
    [MarshalAs(UnmanagedType.I4)]
    public int IntValue;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public float[] FloatValues;
    [MarshalAs(UnmanagedType.I1)]
    public bool BoolValue;
    [MarshalAs(UnmanagedType.LPStr)]
    public string StringValue; // Use IntPtr for pointer types
}

[StructLayout(LayoutKind.Sequential)]
public struct OscMessageMeta {
    [MarshalAs(UnmanagedType.LPStr)]
    public string Address;
    
    public OscValueType Type;
    
    public OscValue Value;
}

// Simple Rust OSC Lib wrapper
public static class SROSCLib
{
    [DllImport("fti_osc.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool parse_osc(byte[] buffer, int bufferLength, ref OscMessageMeta message);
    
    [DllImport("fti_osc.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int create_osc_message([MarshalAs(UnmanagedType.LPArray, SizeConst = 4096)] byte[] buf, ref OscMessageMeta osc_template);

    [DllImport("fti_osc.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int create_osc_bundle([MarshalAs(UnmanagedType.LPArray, SizeConst = 4096)] byte[] buf, 
        [MarshalAs(UnmanagedType.LPArray)]  OscMessageMeta[] messages, int len, ref int messageIndex);
}

public class OscMessage
{
    public OscMessageMeta _meta = new();

    public string Address
    {
        get => _meta.Address;
        set => _meta.Address = value;
    }

    public OscValueType TypeIdentifier
    {
        get => _meta.Type;
        set => _meta.Type = value;
    }

    public object CachedValue
    {
        get;
        private set;
    }
    public object Value
    {
        get
        {
            switch (TypeIdentifier)
            {
                case OscValueType.Bool:
                    return _meta.Value.BoolValue;
                case OscValueType.Float:
                    return _meta.Value.FloatValues[0];
                case OscValueType.Int:
                    return _meta.Value.IntValue;
                case OscValueType.String:
                    return _meta.Value.StringValue;
                case OscValueType.Vector2:
                    return new Vector2(_meta.Value.FloatValues[0], _meta.Value.FloatValues[1]);
                case OscValueType.Vector3:
                    return new Vector3(_meta.Value.FloatValues[0], _meta.Value.FloatValues[1], _meta.Value.FloatValues[2]);
                case OscValueType.Vector4:
                    return new Vector4(_meta.Value.FloatValues[0], _meta.Value.FloatValues[1], _meta.Value.FloatValues[2], _meta.Value.FloatValues[3]);
                default:
                    return null;
            }
        }
        set
        {
            switch (TypeIdentifier)
            {
                case OscValueType.Int:
                    _meta.Value.IntValue = (int)value;
                    break;
                case OscValueType.Float:
                    _meta.Value.FloatValues = new []{(float)value, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f};
                    break;
                case OscValueType.Bool:
                    _meta.Value.BoolValue = (bool)value;
                    break;
                case OscValueType.String:
                    _meta.Value.StringValue = (string)value;
                    break;
                case OscValueType.Vector2:
                    var val = (Vector2)value;
                    _meta.Value.FloatValues = new []{val.x, val.y, 0.0f, 0.0f, 0.0f, 0.0f};
                    break;
                case OscValueType.Vector3:
                    var val3 = (Vector3)value;
                    _meta.Value.FloatValues = new []{val3.x, val3.y, val3.z, 0.0f, 0.0f, 0.0f};
                    break;
                case OscValueType.Vector4:
                    var val4 = (Vector4)value;
                    _meta.Value.FloatValues = new []{val4.x, val4.y, val4.z, val4.w, 0.0f, 0.0f};
                    break;
            }
            
            CachedValue = value;
        }
    }

    private OscMessage(string address, OscValueType typeIdentifier)
    {
        Address = address;
        TypeIdentifier = typeIdentifier;
    }
    
    public OscMessage(string address, Type type) : this(address, OscUtils.TypeConversions[type].oscType) {}

    public OscMessage(byte[] bytes, int len) => SROSCLib.parse_osc(bytes, len, ref _meta);
}