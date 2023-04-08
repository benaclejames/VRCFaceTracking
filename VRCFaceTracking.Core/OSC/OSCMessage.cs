using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace VRCFaceTracking.OSC;

public enum OscValueType : byte {
    Int = 0,
    Float = 1,
    Bool = 2,
    String = 3,
    Vector3 = 4,
    Vector2 = 5,
}

[StructLayout(LayoutKind.Sequential)]
public struct OscValue {
    [MarshalAs(UnmanagedType.I4)]
    public int IntValue;
    [MarshalAs(UnmanagedType.R4)]
    public float FloatValue;
    [MarshalAs(UnmanagedType.I1)]
    public bool BoolValue;
    [MarshalAs(UnmanagedType.LPStr)]
    public string StringValue; // Use IntPtr for pointer types
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public float[] Vector3;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public float[] Vector2;
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
    public static extern int parse_osc(byte[] buffer, int bufferLength, int offset, ref OscMessageMeta message);
    
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

    public object Value
    {
        get
        {
            switch (TypeIdentifier)
            {
                case OscValueType.Bool:
                    return _meta.Value.BoolValue;
                case OscValueType.Float:
                    return _meta.Value.FloatValue;
                case OscValueType.Int:
                    return _meta.Value.IntValue;
                case OscValueType.String:
                    return _meta.Value.StringValue;
                default:
                    return null;
            }
        }
        set
        {
            // Unbox time
            switch (value)
            {
                case int intValue:
                    TypeIdentifier = OscValueType.Int;
                    _meta.Value.IntValue = intValue;
                    break;
                case float floatValue:
                    TypeIdentifier = OscValueType.Float;
                    _meta.Value.FloatValue = floatValue;
                    break;
                case bool boolValue:
                    TypeIdentifier = OscValueType.Bool;
                    _meta.Value.BoolValue = boolValue;
                    break;
            }
        }
    }

    private OscMessage(string address, OscValueType typeIdentifier)
    {
        Address = address;
        TypeIdentifier = typeIdentifier;
    }
    
    public OscMessage(string address, Type type) : this(address, OscUtils.TypeConversions[type].oscType) {}

    public OscMessage(byte[] bytes) => SROSCLib.parse_osc(bytes, bytes.Length, 0, ref _meta);
}