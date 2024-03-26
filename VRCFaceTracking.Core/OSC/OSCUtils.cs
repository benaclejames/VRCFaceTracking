using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Types;

namespace VRCFaceTracking.OSC
{
    public static class OscUtils
    {
        public static readonly Dictionary<(Type, char[] typeChar), (OscValueType oscType, string configType)> TypeConversions =
            new()
            {
                {(typeof(bool), new[]{'T', 'F'}), (OscValueType.Bool, "Bool")},
                {(typeof(float), new[]{'f'}), (OscValueType.Float, "Float")},
                {(typeof(int), new[]{'i'}), (OscValueType.Int, "Int")},
                {(typeof(string), new[]{'s'}), (OscValueType.String, "String")},
            };
    }
}