using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Types;

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
                {typeof(string), (OscValueType.String, "String")},
                {typeof(Vector2), (OscValueType.Vector2, "Vector2")},
                {typeof(Vector3), (OscValueType.Vector3, "Vector3")},
                {typeof(Vector4), (OscValueType.Vector4, "Vector4")},
            };
    }
}