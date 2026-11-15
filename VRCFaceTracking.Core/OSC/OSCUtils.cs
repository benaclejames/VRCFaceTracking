using VRCFaceTracking.Core.Types;

namespace VRCFaceTracking.OSC
{
    public static class OscUtils
    {
        public static readonly Dictionary<(Type, char[] typeChar), string> TypeConversions =
            new()
            {
                {(typeof(bool), new[]{'T', 'F'}), "Bool"},
                {(typeof(float), new[]{'f'}), "Float"},
                {(typeof(int), new[]{'i'}), "Int"},
                {(typeof(string), new[]{'s'}), "String"},
            };
    }
}