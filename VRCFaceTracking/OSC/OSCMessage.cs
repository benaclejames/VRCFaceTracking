using System;
using System.Text;

namespace VRCFaceTracking.OSC
{
    public class OscMessage
    {
        public readonly byte[] Data;

        private OscMessage(string name, char typeIdentifier)
        {
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
    }
}