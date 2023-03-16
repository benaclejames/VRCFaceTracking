using System;
using System.Collections.Generic;
using System.Text;

namespace VRCFaceTracking.OSC
{
    public class OscMessage<T>
    {
        public readonly string Address;
        private readonly byte[] _addressBytes;
        
        private readonly byte[] _valueIdentBytes = {44, 0, 0, 0};

        public char TypeIdentifier
        {
            get => (char)_valueIdentBytes[1];
            set => _valueIdentBytes[1] = (byte)value;
        }
        
        public T Value;
        private byte[] _data;

        public byte[] SerializationCache;

        private OscMessage(string name, char typeIdentifier)
        {
            // Set constant address
            Address = name;
            _addressBytes = Encoding.ASCII.GetBytes(name).EnsureCompliance();

            TypeIdentifier = typeIdentifier;
        }
        
        public OscMessage(string name, Type type) : this(name, OscUtils.TypeConversions[type].oscType) {}

        public void SetValue(T newValue)
        {
            Value = newValue;
            
            if (typeof(T) == typeof(int))
                SetInt((int)(object)newValue);
            
            if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
                SetFloat((double)(object)newValue);
            
            if (typeof(T) == typeof(bool))
                SetBool((bool)(object)newValue);
        }

        private void SetInt(int newValue)
        {
            _data = BitConverter.GetBytes(newValue);
            
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_data);
        }
        
        private void SetFloat(double newValue)
        {
            _data = BitConverter.GetBytes(newValue);
            
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_data);
        }
        
        private void SetBool(bool newValue) => TypeIdentifier = newValue ? 'T' : 'F';

        public byte[] Serialize()
        {
            SerializationCache = new byte[_addressBytes.Length + _valueIdentBytes.Length + _data.Length];
            Array.Copy(_addressBytes, SerializationCache, _addressBytes.Length);
            Array.Copy(_valueIdentBytes, 0, SerializationCache, _addressBytes.Length, _valueIdentBytes.Length);
            Array.Copy(_data, 0, SerializationCache, _addressBytes.Length + _valueIdentBytes.Length, _data.Length);
            return SerializationCache;
        }

        public OscMessage(byte[] bytes)
        {
            var iter = 0;

            var addressBytes = new List<byte>();
            for (; iter < bytes.Length; iter++)
            {
                if (bytes[iter] == 0)
                    break;

                addressBytes.Add(bytes[iter]);
            }

            Address = Encoding.ASCII.GetString(addressBytes.ToArray());

            // Increase iter until we find the type identifier
            for (; iter < bytes.Length; iter++)
            {
                if (bytes[iter] != ',') continue;
                
                iter++;
                break;
            }

            iter += 2; // Next multiple of 4

            switch (bytes[iter])
            {
                case 105: // OSC Type tag: 'i' ; int32
                    var intBytes = new byte[4];
                    Array.Copy(bytes, iter, intBytes, 0, 4);
                    Array.Reverse(intBytes);
                    Value = (T)(object)BitConverter.ToInt32(intBytes, 0);
                    break;
                case 102: // OSC Type tag: 'f' ; float32
                    var floatBytes = new byte[4];
                    Array.Copy(bytes, iter, floatBytes, 0, 4);
                    Array.Reverse(floatBytes);
                    Value = (T)(object)BitConverter.ToSingle(floatBytes, 0);
                    break;
                case 115: // OSC Type tag: 's' ; OSC-string
                    var stringBytes = new List<byte>();
                    for (iter++; iter < bytes.Length; iter++)
                    {
                        if (bytes[iter] == 0)
                            break;

                        stringBytes.Add(bytes[iter]);
                    }

                    Value = (T)(object)Encoding.ASCII.GetString(stringBytes.ToArray());
                    break;

                case 70: // OSC Type tag: 'T' ; Represents true, No extra data
                    Value = (T)(object)false;
                    break;
                case 84: // OSC Type tag: 'F' ; Represents false, No extra data
                    Value = (T)(object)true;
                    break;
                case 78: // OSC Type tag: 'N' ; Represents NIL (zero), No extra data
                    Value = (T)(object)0;
                    break;
                case 73: // OSC Type tag: 'I' ; Represents Infinitum (endlessly infinite), No extra data. Capping to 1.
                    Value = (T)(object)int.MaxValue;
                    break;
                case 91: // OSC Type tag: '[' ; Represents start of an array.
                    goto default;
                case 93: // OSC Type tag: ']' ; Represents end of an array.
                    goto default;

                default:
                    break;
            }
        }
    }
}