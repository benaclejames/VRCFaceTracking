using System.Runtime.InteropServices;

namespace VRCFaceTracking.Core.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        
        // Make a method that multiplies the vector by a scalar
        public Vector3 FlipXCoordinates()
        {
            x *= -1;
            
            return this;
        }
        public static Vector3 operator *(Vector3 a, float d)
        => new Vector3(a.x * d, a.y * d, a.z * d);

        public static Vector3 operator /(Vector3 a, float d)
        => new Vector3(a.x / d, a.y / d, a.z / d);

        public static Vector3 operator +(Vector3 a, Vector3 b)
        => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);

        public static Vector3 operator -(Vector3 a, Vector3 b)
        => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);

        // Tobii normalized eye value is r = 1. Used by UnifiedEyeData gaze data.
        public Vector3 PolarTo2DCartesian(float r = 1)
        => new Vector3(r * (float)Math.Cos(x), r * (float)Math.Sin(y), z);

        public static Vector3 zero => new Vector3(0, 0, 0);
    }
}