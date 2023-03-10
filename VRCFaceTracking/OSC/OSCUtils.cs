using System;
using System.Collections.Generic;

namespace VRCFaceTracking.OSC
{
    public static class OscUtils
    {
        public static readonly Dictionary<Type, (char oscType, string configType)> TypeConversions =
            new Dictionary<Type, (char oscType, string configType)>
            {
                {typeof(bool), ('F', "Bool")},
                {typeof(float), ('f', "Float")},
                {typeof(int), ('i', "Int")},
            };
        
        public static byte[] EnsureCompliance(this byte[] inputArr)
        {
            var nullTerm = new byte[inputArr.Length + 1];
            Array.Copy(inputArr, nullTerm, inputArr.Length);

            int n = nullTerm.Length + 3;
            int m = n % 4;
            int closestMult = n - m;
            int multDiff = closestMult - nullTerm.Length;

            // Construct new array of zeros with the correct length + 1 for null terminator
            byte[] newArr = new byte[nullTerm.Length + multDiff];
            Array.Copy(nullTerm, newArr, nullTerm.Length);
            return newArr;
        }
    }
}