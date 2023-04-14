using System.Runtime.InteropServices;

namespace VRCFaceTracking.Core.Types;

[StructLayout(LayoutKind.Sequential)]
public struct Vector4
{
    public float w;
    public float x;
    public float y;
    public float z;
    
    public Vector4(float w, float x, float y, float z)
    {
        this.w = w;
        this.x = x;
        this.y = y;
        this.z = z;
    }
}