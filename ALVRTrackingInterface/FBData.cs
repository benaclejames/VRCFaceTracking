using System.Runtime.InteropServices;

namespace DefaultNamespace
{
    public class FBData
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct AllData
        {
            public EyeData eyeData;
            public FaceData faceData;

            public override string ToString()
            {
                return "EyeData: " + eyeData.ToString() + " FaceData: " + faceData.ToString();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EyeData
        {
            public Eye leftEye;
            public Eye rightEye;

            public override string ToString()
            {
                return "LeftEye: " + leftEye.ToString() + " RightData: " + rightEye.ToString();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Eye
        {
            public float confidence;
            public Vec3 position;
            public Quat rotation;

            public override string ToString()
            {
                return "Confidence: " + confidence + ", Position: " + position.ToString() + ", Rotation: " + rotation.ToString();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Vec3
        {
            public float x;
            public float y;
            public float z;

            public override string ToString()
            {
                return $"Vector3: X: {x}, Y: {y}, Z: {z})";
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Quat
        {
            public float x;
            public float y;
            public float z;
            public float w;

            public override string ToString()
            {
                return $"Quaternion: X: {x}, Y: {y}, Z: {z}, W: {w})";
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FaceData
        {
            public float FaceRegionConfidenceLower;
            public float FaceRegionConfidenceUpper;
            public float BrowLowererL;
            public float BrowLowererR;
            public float CheekPuffL;
            public float CheekPuffR;
            public float CheekRaiserL;
            public float CheekRaiserR;
            public float CheekSuckL;
            public float CheekSuckR;
            public float ChinRaiserB;
            public float ChinRaiserT;
            public float DimplerL;
            public float DimplerR;
            public float EyesClosedL;
            public float EyesClosedR;
            public float EyesLookDownL;
            public float EyesLookDownR;
            public float EyesLookLeftL;
            public float EyesLookLeftR;
            public float EyesLookRightL;
            public float EyesLookRightR;
            public float EyesLookUpL;
            public float EyesLookUpR;
            public float InnerBrowRaiserL;
            public float InnerBrowRaiserR;
            public float JawDrop;
            public float JawSidewaysLeft;
            public float JawSidewaysRight;
            public float JawThrust;
            public float LidTightenerL;
            public float LidTightenerR;
            public float LipCornerDepressorL;
            public float LipCornerDepressorR;
            public float LipCornerPullerL;
            public float LipCornerPullerR;
            public float LipFunnelerLB;
            public float LipFunnelerLT;
            public float LipFunnelerRB;
            public float LipFunnelerRT;
            public float LipPressorL;
            public float LipPressorR;
            public float LipPuckerL;
            public float LipPuckerR;
            public float LipStretcherL;
            public float LipStretcherR;
            public float LipSuckLB;
            public float LipSuckLT;
            public float LipSuckRB;
            public float LipSuckRT;
            public float LipTightenerL;
            public float LipTightenerR;
            public float LipsToward;
            public float LowerLipDepressorL;
            public float LowerLipDepressorR;
            public float MouthLeft;
            public float MouthRight;
            public float NoseWrinklerL;
            public float NoseWrinklerR;
            public float OuterBrowRaiserL;
            public float OuterBrowRaiserR;
            public float UpperLidRaiserL;
            public float UpperLidRaiserR;
            public float UpperLipRaiserL;
            public float UpperLipRaiserR;

            public override string ToString()
            {
                return
                     "FaceData: " +
                     "\nFaceRegionConfidenceLower: " + FaceRegionConfidenceLower +
                     "\nFaceRegionConfidenceUpper: " + FaceRegionConfidenceUpper +
                     "\nBrowLowererL: " + BrowLowererL +
                     "\nBrowLowererR: " + BrowLowererR +
                     "\nCheekPuffL: " + CheekPuffL +
                     "\nCheekPuffR: " + CheekPuffR +
                     "\nCheekRaiserL: " + CheekRaiserL +
                     "\nCheekRaiserR: " + CheekRaiserR +
                     "\nCheekSuckL: " + CheekSuckL +
                     "\nCheekSuckR: " + CheekSuckR +
                     "\nChinRaiserB: " + ChinRaiserB +
                     "\nChinRaiserT: " + ChinRaiserT +
                     "\nDimplerL: " + DimplerL +
                     "\nDimplerR: " + DimplerR +
                     "\nEyesClosedL: " + EyesClosedL +
                     "\nEyesClosedR: " + EyesClosedR +
                     "\nEyesLookDownL: " + EyesLookDownL +
                     "\nEyesLookDownR: " + EyesLookDownR +
                     "\nEyesLookLeftL: " + EyesLookLeftL +
                     "\nEyesLookLeftR: " + EyesLookLeftR +
                     "\nEyesLookRightL: " + EyesLookRightL +
                     "\nEyesLookRightR: " + EyesLookRightR +
                     "\nEyesLookUpL: " + EyesLookUpL +
                     "\nEyesLookUpR: " + EyesLookUpR +
                     "\nInnerBrowRaiserL: " + InnerBrowRaiserL +
                     "\nInnerBrowRaiserR: " + InnerBrowRaiserR +
                     "\nJawDrop: " + JawDrop +
                     "\nJawSidewaysLeft: " + JawSidewaysLeft +
                     "\nJawSidewaysRight: " + JawSidewaysRight +
                     "\nJawThrust: " + JawThrust +
                     "\nLidTightenerL: " + LidTightenerL +
                     "\nLidTightenerR: " + LidTightenerR +
                     "\nLipCornerDepressorL: " + LipCornerDepressorL +
                     "\nLipCornerDepressorR: " + LipCornerDepressorR +
                     "\nLipCornerPullerL: " + LipCornerPullerL +
                     "\nLipCornerPullerR: " + LipCornerPullerR +
                     "\nLipFunnelerLB: " + LipFunnelerLB +
                     "\nLipFunnelerLT: " + LipFunnelerLT +
                     "\nLipFunnelerRB: " + LipFunnelerRB +
                     "\nLipFunnelerRT: " + LipFunnelerRT +
                     "\nLipPressorL: " + LipPressorL +
                     "\nLipPressorR: " + LipPressorR +
                     "\nLipPuckerL: " + LipPuckerL +
                     "\nLipPuckerR: " + LipPuckerR +
                     "\nLipStretcherL: " + LipStretcherL +
                     "\nLipStretcherR: " + LipStretcherR +
                     "\nLipSuckLB: " + LipSuckLB +
                     "\nLipSuckLT: " + LipSuckLT +
                     "\nLipSuckRB: " + LipSuckRB +
                     "\nLipSuckRT: " + LipSuckRT +
                     "\nLipTightenerL: " + LipTightenerL +
                     "\nLipTightenerR: " + LipTightenerR +
                     "\nLipsToward: " + LipsToward +
                     "\nLowerLipDepressorL: " + LowerLipDepressorL +
                     "\nLowerLipDepressorR: " + LowerLipDepressorR +
                     "\nMouthLeft: " + MouthLeft +
                     "\nMouthRight: " + MouthRight +
                     "\nNoseWrinklerL: " + NoseWrinklerL +
                     "\nNoseWrinklerR: " + NoseWrinklerR +
                     "\nOuterBrowRaiserL: " + OuterBrowRaiserL +
                     "\nOuterBrowRaiserR: " + OuterBrowRaiserR +
                     "\nUpperLidRaiserL: " + UpperLidRaiserL +
                     "\nUpperLidRaiserR: " + UpperLidRaiserR +
                     "\nUpperLipRaiserL: " + UpperLipRaiserL +
                     "\nUpperLipRaiserR: " + UpperLipRaiserR;
            }
        }
    }
}