using System.Runtime.InteropServices;

namespace VRCFaceTracking.Params
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
        public Vector3 Invert()
        {
            x *= -1;
            
            return this;
        }
    }
}