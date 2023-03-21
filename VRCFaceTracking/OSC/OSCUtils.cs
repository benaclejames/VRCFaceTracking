using System;
using System.Collections.Generic;

namespace VRCFaceTracking.OSC
{
    public static class OscUtils
    {
        public static readonly Dictionary<Type, (OscValueType oscType, string configType)> TypeConversions =
            new()
            {
                {typeof(bool), (OscValueType.Bool, "Bool")},
                {typeof(float), (OscValueType.Float, "Float")},
                {typeof(int), (OscValueType.Int, "Int")},
            };
    }
}