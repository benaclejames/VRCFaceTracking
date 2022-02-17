#if DLL
using VRC.UI;
#endif

namespace VRCFaceTracking.Params
{
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
        
        #if DLL
        public static implicit operator Vector3(UnityEngine.Vector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
        
        public static implicit operator UnityEngine.Vector3(Vector3 v)
        {
            return new UnityEngine.Vector3(v.x, v.y, v.z);
        }
#endif
    }
}