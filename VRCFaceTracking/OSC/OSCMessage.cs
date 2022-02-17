using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VRCFaceTracking.OSC
{
    public class OscMessage
    {
        public readonly List<byte> Data = new List<byte>();

        private OscMessage(string name, char typeIdentifier)
        {
            var nameBytes = new List<byte>(Encoding.ASCII.GetBytes(name)) {0};
            nameBytes.EnsureCompliance();
            
            var valueIdentBytes = new List<byte>(Encoding.ASCII.GetBytes(","+typeIdentifier)) {0};
            valueIdentBytes.EnsureCompliance();
            
            Data.AddRange(nameBytes.Concat(valueIdentBytes));
        }

        public OscMessage(string name, int value) : this(name, 'i')
        {
            var valueArr = BitConverter.GetBytes(value);
            Array.Reverse(valueArr);

            Data.AddRange(valueArr);
        }
        
        public OscMessage(string name, double value) : this(name, 'f')
        {
            float floatVal = (float)value;
            var valueArr = BitConverter.GetBytes(floatVal);
            Array.Reverse(valueArr);

            Data .AddRange(valueArr);
        }
    }
}