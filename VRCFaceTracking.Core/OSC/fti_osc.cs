using System.Runtime.InteropServices;

namespace VRCFaceTracking.Core.OSC;

public enum OscValueType : byte
{
    Null = 0,
    Int = 1,
    Float = 2,
    Bool = 3,
    String = 4,
    ArrayBegin = 5,
    ArrayEnd = 6,
}

[StructLayout(LayoutKind.Sequential)]
public struct OscValue
{
    public OscValueType Type;
    [MarshalAs(UnmanagedType.I4)]
    public int IntValue;
    [MarshalAs(UnmanagedType.R4)]
    public float FloatValue;
    [MarshalAs(UnmanagedType.I1)]
    public bool BoolValue;
    [MarshalAs(UnmanagedType.LPUTF8Str)]
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
public struct OscMessageMeta
{
    [MarshalAs(UnmanagedType.LPUTF8Str)]
    public string Address;

    [MarshalAs(UnmanagedType.U4)]
    public int ValueLength;

    [MarshalAs(UnmanagedType.SysUInt)]
    public IntPtr Value;
}

// Simple Rust OSC Lib wrapper
public static class fti_osc
{
    private const string DllName = "fti_osc";

    /// <summary>
    /// Parses a byte buffer of specified length into a single pointer to an osc message
    /// </summary>
    /// <param name="buffer">The target byte buffer to parse osc from</param>
    /// <param name="bufferLength">The length of <paramref name="buffer"/></param>
    /// <param name="byteIndex">The index of the first byte of the message. This is modified after a message is parsed
    /// This way we can sequentially read messages by passing in the value this int was last modified to be</param>
    /// <returns>Pointer to an OscMessageMeta struct</returns>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr parse_osc(byte[] buffer, int bufferLength, ref int byteIndex);

    /// <summary>
    /// Serializes a pointer to an OscMessageMeta struct into a 4096 length byte buffer
    /// </summary>
    /// <param name="buf">Target write buffer</param>
    /// <param name="osc_template">Target OscMessageMeta to serialize</param>
    /// <returns>Amount of bytes written to <paramref name="buf"/></returns>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int create_osc_message([MarshalAs(UnmanagedType.LPArray, SizeConst = 4096)] byte[] buf, ref OscMessageMeta osc_template);

    /// <summary>
    /// Serializes a pointer to an array of OscMessageMeta structs to a byte array of size 4096
    /// </summary>
    /// <param name="buf">Target byte array</param>
    /// <param name="messages">Array of messages to be contained within the bundle</param>
    /// <param name="len">Length of <paramref name="messages"/></param>
    /// <param name="messageIndex">Index of the last message written to <paramref name="buf"/> before it was filled</param>
    /// <returns></returns>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int create_osc_bundle(
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 4096)] byte[] buf,
        [MarshalAs(UnmanagedType.LPArray)] OscMessageMeta[] messages,
        int len,
        ref int messageIndex);

    /// <summary>
    /// Free memory allocated to OscMessageMeta by fti_osc lib
    /// </summary>
    /// <param name="oscMessage">Target message pointer</param>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void free_osc_message(IntPtr oscMessage);
}
