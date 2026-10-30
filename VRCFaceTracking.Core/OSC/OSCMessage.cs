﻿using System.Reflection;
using FastOSC;
using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Core.OSC;

public class OscMessage
{
    private readonly object[] _arguments;
    private readonly Action<object> _valueSetter;

    public string Address { get; set; }

    public object Value
    {
        get => _arguments.Length == 0 ? null : _arguments[0];
        set => _valueSetter?.Invoke(value);
    }

    public OscMessage(string address, Type type)
    {
        Address = address;

        if (OscUtils.TypeConversions.Any(conv => conv.Key.Item1 == type))
        {
            _arguments = [DefaultValue(type)];
            _valueSetter = value => _arguments[0] = value;
        }
        else    // If we don't have the type, we assume it's a struct and serialize it using reflection
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            _arguments = fields.Select(field => DefaultValue(field.FieldType)).ToArray();
            _valueSetter = value =>
            {
                for (var j = 0; j < fields.Length; j++)
                {
                    _arguments[j] = fields[j].GetValue(value);
                }
            };
        }
    }

    private OscMessage(string address, object[] arguments)
    {
        Address = address;
        _arguments = arguments;
    }

    // Creates an OscMessage that references a caller-owned buffer. The caller may rewrite its contents
    // in place between sends; the buffer is read when the message is encoded.
    public static OscMessage CreateBlob(string address, byte[] buffer) => new(address, [buffer]);

    /// <summary>
    /// Flattens a decoded packet (a single message or a possibly nested bundle) into messages
    /// </summary>
    public static IEnumerable<OscMessage> FromPacket(IOSCPacket packet) => packet switch
    {
        OSCMessage message => [new OscMessage(message.Address, message.Arguments)],
        OSCBundle bundle => bundle.Packets.SelectMany(FromPacket),
        _ => []
    };

    /// <summary>
    /// Snapshots the current address and value(s) into a FastOSC message ready for encoding
    /// </summary>
    public OSCMessage ToPacket() => new(Address, (object[])_arguments.Clone());

    private static object DefaultValue(Type type) => type == typeof(string) ? string.Empty : Activator.CreateInstance(type);
}
