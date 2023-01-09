using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VRCFaceTracking.OSC
{
    public class OscMessage
    {
        public readonly byte[] Data;
        public readonly string Address;
        public readonly object Value;

        private OscMessage(string name, char typeIdentifier)
        {
            Address = name;
            
            var nameBytes = Encoding.ASCII.GetBytes(name);
            nameBytes = nameBytes.EnsureCompliance();

            var valueIdentBytes = Encoding.ASCII.GetBytes("," + typeIdentifier);
            valueIdentBytes = valueIdentBytes.EnsureCompliance();

            Data = new byte[nameBytes.Length + valueIdentBytes.Length];
            Array.Copy(nameBytes, Data, nameBytes.Length);
            Array.Copy(valueIdentBytes, 0, Data, nameBytes.Length, valueIdentBytes.Length);
        }

        public OscMessage(string name, int value) : this(name, 'i')
        {
            var valueArr = BitConverter.GetBytes(value);
            Array.Reverse(valueArr);

            var newFullArr = new byte[Data.Length+valueArr.Length];
            Array.Copy(Data, newFullArr, Data.Length);
            Array.Copy(valueArr, 0, newFullArr, Data.Length, valueArr.Length);
            Data = newFullArr;
        }
        
        public OscMessage(string name, double value) : this(name, 'f')
        {
            var valueArr = BitConverter.GetBytes((float)value);
            Array.Reverse(valueArr);

            var newFullArr = new byte[Data.Length+valueArr.Length];
            Array.Copy(Data, newFullArr, Data.Length);
            Array.Copy(valueArr, 0, newFullArr, Data.Length, valueArr.Length);
            Data = newFullArr;
        }
        
        public OscMessage(string name, bool value) : this(name, value ? 'T' : 'F') {}

        public OscMessage(string name, char type, byte[] valueBytes) : this(name, type)
        {
            if (valueBytes == null) return;
            var newFullArr = new byte[Data.Length+valueBytes.Length];
            Array.Copy(Data, newFullArr, Data.Length);
            Array.Copy(valueBytes, 0, newFullArr, Data.Length, valueBytes.Length);
            Data = newFullArr;
        }

        public OscMessage(byte[] bytes)
        {
            int iter = 0;

            var addressBytes = new List<byte>();
            for (; iter < bytes.Length; iter++)
            {
                if (bytes[iter] == 0)
                    break;

                addressBytes.Add(bytes[iter]);
            }

            Address = Encoding.ASCII.GetString(addressBytes.ToArray());
            
            // Currently we're at a null terminator for the string,
            // We need to ensure we're at the beginning of the type identifier which will be at the next multiple of 4
            // We need to pad the iterator to the next multiple of 4
            iter = (iter + 4) & ~3;


            // Ensure the next two bytes are zero and a comma
            if (bytes[iter++] != ',')
                throw new Exception("Invalid OSC message: No comma after address");

            byte type = bytes[iter];
            iter += 2; // Next multiple of 4

            switch (type)
            {
                case 105:
                    var intBytes = new byte[4];
                    Array.Copy(bytes, iter, intBytes, 0, 4);
                    Array.Reverse(intBytes);
                    Value = BitConverter.ToInt32(intBytes, 0);
                    break;
                case 102:
                    var floatBytes = new byte[4];
                    Array.Copy(bytes, iter, floatBytes, 0, 4);
                    Array.Reverse(floatBytes);
                    Value = BitConverter.ToSingle(floatBytes, 0);
                    break;
                case 115:
                    var stringBytes = new List<byte>();
                    for (iter++; iter < bytes.Length; iter++)
                    {
                        if (bytes[iter] == 0)
                            break;

                        stringBytes.Add(bytes[iter]);
                    }

                    Value = Encoding.ASCII.GetString(stringBytes.ToArray());
                    break;
                case 70:
                    Value = false;
                    break;
                case 84:
                    Value = true;
                    break;
                default:
                    throw new Exception("Unknown type identifier: " + type + " for name " + Address);
            }
        }
    }
}