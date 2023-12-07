using System.Reflection;
using System.Runtime.InteropServices;
using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Core.OSC;

public enum OscValueType : byte {
    Null = 0,
    Int = 1,
    Float = 2,
    Bool = 3,
    String = 4,
    ArrayBegin = 5,
    ArrayEnd = 6,
}

[StructLayout(LayoutKind.Sequential)]
public struct OscValue {
    public OscValueType Type;
    [MarshalAs(UnmanagedType.I4)]
    public int IntValue;
    [MarshalAs(UnmanagedType.R4)]
    public float FloatValue;
    [MarshalAs(UnmanagedType.I1)]
    public bool BoolValue;
    [MarshalAs(UnmanagedType.LPStr)]
    public string StringValue; // Use IntPtr for pointer types

    public object Value
    {
        get => Type switch
        {
            OscValueType.Int => IntValue,
            OscValueType.Float => FloatValue,
            OscValueType.Bool => BoolValue,
            OscValueType.String => StringValue,
            _ => null
        };
        set
        {
            switch (Type)
            {
                case OscValueType.Int:
                    IntValue = (int)value;
                    break;
                case OscValueType.Float:
                    FloatValue = (float)value;
                    break;
                case OscValueType.Bool:
                    BoolValue = (bool)value;
                    break;
                case OscValueType.String:
                    StringValue = (string)value;
                    break;
            }
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct OscMessageMeta {
    [MarshalAs(UnmanagedType.LPStr)]
    public string Address;
    
    [MarshalAs(UnmanagedType.I4)]
    public int ValueLength;
    
    public IntPtr Value;
}

// Simple Rust OSC Lib wrapper
public static class SROSCLib
{
    [DllImport("fti_osc.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr parse_osc(byte[] buffer, int bufferLength, ref int messageIndex);
    
    [DllImport("fti_osc.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int create_osc_message([MarshalAs(UnmanagedType.LPArray, SizeConst = 4096)] byte[] buf, ref OscMessageMeta osc_template);

    [DllImport("fti_osc.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int create_osc_bundle([MarshalAs(UnmanagedType.LPArray, SizeConst = 4096)] byte[] buf, 
        [MarshalAs(UnmanagedType.LPArray)]  OscMessageMeta[] messages, int len, ref int messageIndex);

    [DllImport("fti_osc.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void free_osc_message(IntPtr oscMessage);
}

public class OscMessage
{
    public OscMessageMeta _meta;
    protected IntPtr _metaPtr;

    public string Address
    {
        get => _meta.Address;
        set => _meta.Address = value;
    }
    
    private Action<object> _valueSetter;

    public object Value
    {
        get
        {
            var values = new OscValue[_meta.ValueLength];
            var ptr = _meta.Value;
            for (var i = 0; i < _meta.ValueLength; i++)
            {
                values[i] = Marshal.PtrToStructure<OscValue>(ptr);
                ptr += Marshal.SizeOf<OscValue>();
            }

            return values[0].Value;
        }
        set => _valueSetter(value);
    }

    public OscMessage(string address, Type type)
    {
        Address = address;
        
        if (OscUtils.TypeConversions.TryGetValue(type, out var oscType))
        {
            _meta.ValueLength = 1;
            _meta.Value = Marshal.AllocHGlobal(Marshal.SizeOf<OscValue>() * _meta.ValueLength);
            var oscValue = new OscValue
            {
                Type = oscType.oscType,
            };
            _valueSetter = value =>
            {
                oscValue.Value = value;
                Marshal.StructureToPtr(oscValue, _meta.Value, false);
            };
        }
        else    // If we don't have the type, we assume it's a struct and serialize it using reflection
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            _meta.ValueLength = fields.Length;
            _meta.Value = Marshal.AllocHGlobal(Marshal.SizeOf<OscValue>() * _meta.ValueLength);
            var values = new OscValue[_meta.ValueLength];
            for (var i = 0; i < _meta.ValueLength; i++)
            {
                values[i] = new OscValue
                {
                    Type = OscUtils.TypeConversions[fields[i].FieldType].oscType,
                };
            }
            _valueSetter = value =>
            {
                for (var j = 0; j < _meta.ValueLength; j++)
                {
                    values[j].Value = fields[j].GetValue(value);
                    Marshal.StructureToPtr(values[j], _meta.Value + Marshal.SizeOf<OscValue>() * j, false);
                }
            };
        }
    }

    public static OscMessage TryParseOsc(byte[] bytes, int len, ref int messageIndex)
    {
        var msg = new OscMessage(bytes, len, ref messageIndex);
        if (msg._metaPtr == IntPtr.Zero)
        {
            return null;
        }

        return msg;
    }
    
    public OscMessage(byte[] bytes, int len, ref int messageIndex)
    {
        _metaPtr = SROSCLib.parse_osc(bytes, len, ref messageIndex);
        if (_metaPtr != IntPtr.Zero)
        {
            _meta = Marshal.PtrToStructure<OscMessageMeta>(_metaPtr);
        }
    }

    public OscMessage(OscMessageMeta meta) => _meta = meta;
    
    ~OscMessage()
    {
        SROSCLib.free_osc_message(_metaPtr);
    }
}