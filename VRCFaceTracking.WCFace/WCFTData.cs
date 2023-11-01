namespace VRCFaceTracking.WCFace;

public class WCFTData
{
    public bool IsFaceTracking = false; // If the Webcam can see the person's face
    public float LeftEyeBlink = 0, RightEyeBlink = 0; // Left-Right
    public float MouthOpen = 0, MouthWide = 0; // MouthOpen is obvious, MouthWide is like smile
    public float LeftEyebrowUpDown = 0, RightEyebrowUpDown = 0; // Left-Right
    public float LookUpDown = 0, LookLeftRight = 0; // Combined (a Fork of NeosWCFaceTrack could change this)
    public float EyebrowSteepness = 0; // Combined (a Fork of NeosWCFaceTrack could change this)
}