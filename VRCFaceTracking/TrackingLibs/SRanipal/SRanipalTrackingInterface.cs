using System;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Runtime.InteropServices;
using VRCFaceTracking;
using VRCFaceTracking.Params;
using VRCFaceTracking.Params.Lip;

using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace VRCFaceTracking.SRanipal
{
    // Treating this as a static class instead of an enum so we don't need to cast
    public static class FBExpression
    {
        public const int Brow_Lowerer_L = 0;
        public const int Brow_Lowerer_R = 1;
        public const int Cheek_Puff_L = 2;
        public const int Cheek_Puff_R = 3;
        public const int Cheek_Raiser_L = 4;
        public const int Cheek_Raiser_R = 5;
        public const int Cheek_Suck_L = 6;
        public const int Cheek_Suck_R = 7;
        public const int Chin_Raiser_B = 8;
        public const int Chin_Raiser_T = 9;
        public const int Dimpler_L = 10;
        public const int Dimpler_R = 11;
        public const int Eyes_Closed_L = 12;
        public const int Eyes_Closed_R = 13;
        public const int Eyes_Look_Down_L = 14;
        public const int Eyes_Look_Down_R = 15;
        public const int Eyes_Look_Left_L = 16;
        public const int Eyes_Look_Left_R = 17;
        public const int Eyes_Look_Right_L = 18;
        public const int Eyes_Look_Right_R = 19;
        public const int Eyes_Look_Up_L = 20;
        public const int Eyes_Look_Up_R = 21;
        public const int Inner_Brow_Raiser_L = 22;
        public const int Inner_Brow_Raiser_R = 23;
        public const int Jaw_Drop = 24;
        public const int Jaw_Sideways_Left = 25;
        public const int Jaw_Sideways_Right = 26;
        public const int Jaw_Thrust = 27;
        public const int Lid_Tightener_L = 28;
        public const int Lid_Tightener_R = 29;
        public const int Lip_Corner_Depressor_L = 30;
        public const int Lip_Corner_Depressor_R = 31;
        public const int Lip_Corner_Puller_L = 32;
        public const int Lip_Corner_Puller_R = 33;
        public const int Lip_Funneler_LB = 34;
        public const int Lip_Funneler_LT = 35;
        public const int Lip_Funneler_RB = 36;
        public const int Lip_Funneler_RT = 37;
        public const int Lip_Pressor_L = 38;
        public const int Lip_Pressor_R = 39;
        public const int Lip_Pucker_L = 40;
        public const int Lip_Pucker_R = 41;
        public const int Lip_Stretcher_L = 42;
        public const int Lip_Stretcher_R = 43;
        public const int Lip_Suck_LB = 44;
        public const int Lip_Suck_LT = 45;
        public const int Lip_Suck_RB = 46;
        public const int Lip_Suck_RT = 47;
        public const int Lip_Tightener_L = 48;
        public const int Lip_Tightener_R = 49;
        public const int Lips_Toward = 50;
        public const int Lower_Lip_Depressor_L = 51;
        public const int Lower_Lip_Depressor_R = 52;
        public const int Mouth_Left = 53;
        public const int Mouth_Right = 54;
        public const int Nose_Wrinkler_L = 55;
        public const int Nose_Wrinkler_R = 56;
        public const int Outer_Brow_Raiser_L = 57;
        public const int Outer_Brow_Raiser_R = 58;
        public const int Upper_Lid_Raiser_L = 59;
        public const int Upper_Lid_Raiser_R = 60;
        public const int Upper_Lip_Raiser_L = 61;
        public const int Upper_Lip_Raiser_R = 62;
        public const int Max = 63;
    }

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

    public class SRanipalExtTrackingInterface : ExtTrackingModule
    {
        public IPAddress localAddr;
        public int PORT = 13191;

        private TcpClient client;
        private NetworkStream stream;
        private bool connected = false;

        private const int expressionsSize = 63;
        private byte[] rawExpressions = new byte[expressionsSize * 4 + (8 * 2 * 4)];
        private float[] expressions = new float[expressionsSize + (8 * 2)];

        private double pitch_L, yaw_L, pitch_R, yaw_R; // eye rotations

        public override (bool SupportsEye, bool SupportsLip) Supported => (true, true);

        public override (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip)
        {
            string configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "questProIP.txt");
            if (!File.Exists(configPath))
            {
                Logger.Msg("Failed to find config JSON! Please maker sure it is present in the same directory as the DLL.");
                return (false, false);
            }

            string text = File.ReadAllText(configPath).Trim();

            if (!IPAddress.TryParse(text, out localAddr))
            {
                Logger.Error("The IP provided in questProIP.txt is not valid. Please check the file and try again.");
                return (false, false);
            }

            ConnectToTCP();

            Logger.Msg("ALXR handshake successful! Data will be broadcast to VRCFaceTracking.");
            return (true, true);
        }

        private bool ConnectToTCP()
        {
            try
            {
                client = new TcpClient();
                Logger.Msg($"Trying to establish a Quest Pro connection at {localAddr}:{PORT}...");

                client.Connect(localAddr, PORT);
                Logger.Msg("Connected to Quest Pro!");

                stream = client.GetStream();
                connected = true;

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return false;
            }
        }

        public override Action GetUpdateThreadFunc()
        {
            return () =>
            {
                while (true)
                {
                    Update();
                    //Thread.Sleep(10); // blocked by IO
                }
            };
        }

        private void Update()
        {
            try
            {
                // Attempt reconnection if needed
                if (!connected || stream == null)
                {
                    ConnectToTCP();
                }

                if (stream == null)
                {
                    Logger.Warning("Can't read from network stream just yet! Trying again soon...");
                    return;
                }

                if (!stream.CanRead)
                {
                    Logger.Warning("Can't read from network stream just yet! Trying again soon...");
                    return;
                }

                int offset = 0;
                int readBytes;
                do
                {
                    readBytes = stream.Read(rawExpressions, offset, rawExpressions.Length - offset);
                    offset += readBytes;
                }
                while (readBytes > 0 && offset < rawExpressions.Length);

                if (offset < rawExpressions.Length && connected)
                {
                    // TODO Reconnect to the server if we lose connection
                    Logger.Warning("End of stream! Reconnecting...");
                    Thread.Sleep(1000);
                    connected = false;
                    try
                    {
                        stream.Close();
                    }
                    catch (SocketException e)
                    {
                        Logger.Error(e.Message);
                        Thread.Sleep(1000);
                    }
                }

                // We receive information from the stream as a byte array 63*4 bytes long, since floats are 32 bits long and we have 63 expressions.
                // We then need to convert this byte array to a float array. Thankfully, this can all be done in a single line of code.
                Buffer.BlockCopy(rawExpressions, 0, expressions, 0, expressionsSize * 4 + (8 * 2 * 4));

                
                // temp
                //Logger.Msg(" ");
                //Logger.Msg("Inner_Brow_Raiser_R: " + expressions[FBExpression.Inner_Brow_Raiser_R].ToString());
                //Logger.Msg("Outer_Brow_Raiser_R: " + expressions[FBExpression.Outer_Brow_Raiser_R].ToString());
                //Logger.Msg("Brow_Lowerer_R: " + expressions[FBExpression.Brow_Lowerer_R].ToString());
                //Logger.Msg(expressions[FBExpression.Brow_Lowerer_R].ToString());

                //Logger.Msg("DownPre: " + expressions[FBExpression.Eyes_Look_Down_L].ToString());
                //Logger.Msg("UpPre: " + expressions[FBExpression.Eyes_Look_Up_L].ToString());
                //Logger.Msg("ClosedPre: " + expressions[FBExpression.Eyes_Closed_L].ToString());

                double q_x = expressions[64];
                double q_y = expressions[65];
                double q_z = expressions[66];
                double q_w = expressions[67];

                double yaw = Math.Atan2(2.0 * (q_y * q_z + q_w * q_x), q_w * q_w - q_x * q_x - q_y * q_y + q_z * q_z);
                double pitch = Math.Asin(-2.0 * (q_x * q_z - q_w * q_y));

                pitch_L = (180.0 / Math.PI) * pitch; // from radians
                yaw_L = (180.0 / Math.PI) * yaw;

                q_x = expressions[72];
                q_y = expressions[73];
                q_z = expressions[74];
                q_w = expressions[75];

                yaw = Math.Atan2(2.0 * (q_y * q_z + q_w * q_x), q_w * q_w - q_x * q_x - q_y * q_y + q_z * q_z);
                pitch = Math.Asin(-2.0 * (q_x * q_z - q_w * q_y));

                pitch_R = (180.0 / Math.PI) * pitch; // from radians
                yaw_R = (180.0 / Math.PI) * yaw;

                PrepareUpdate();
                UpdateExpressions();
            }
            catch (SocketException e)
            {
                Logger.Error(e.Message);
                Thread.Sleep(1000);
            }
        }


        // Preprocess our expressions per the Meta Documentation
        private void PrepareUpdate()
        {
            if(expressions[FBExpression.Eyes_Look_Down_L] == expressions[FBExpression.Eyes_Look_Up_L] && expressions[FBExpression.Eyes_Closed_L] > 0.25f)
            { // wonky eyelid case, eyes are actually closed now
                expressions[FBExpression.Eyes_Closed_L] = 0; //0.9f - (expressions[FBExpression.Lid_Tightener_L] * 3);
            }
            else
            {
                expressions[FBExpression.Eyes_Closed_L] = 0.9f - ((expressions[FBExpression.Eyes_Closed_L] * 3) / (1 + expressions[FBExpression.Eyes_Look_Down_L] * 3));
            }

            if (expressions[FBExpression.Eyes_Look_Down_R] == expressions[FBExpression.Eyes_Look_Up_R] && expressions[FBExpression.Eyes_Closed_R] > 0.25f)
            { // wonky eyelid case, eyes are actually closed now
                expressions[FBExpression.Eyes_Closed_R] = 0; //0.9f - (expressions[FBExpression.Lid_Tightener_R] * 3);
            }
            else
            {
                expressions[FBExpression.Eyes_Closed_R] = 0.9f - ((expressions[FBExpression.Eyes_Closed_R] * 3) / (1 + expressions[FBExpression.Eyes_Look_Down_R] * 3));
            }

            if (1 - expressions[FBExpression.Eyes_Closed_L] < expressions[FBExpression.Lid_Tightener_L])
                expressions[FBExpression.Lid_Tightener_L] = (1 - expressions[FBExpression.Eyes_Closed_L]) - 0.01f;

            if (1 - expressions[FBExpression.Eyes_Closed_R] < expressions[FBExpression.Lid_Tightener_R])
                expressions[FBExpression.Lid_Tightener_R] = (1 - expressions[FBExpression.Eyes_Closed_R]) - 0.01f;

            expressions[FBExpression.Upper_Lid_Raiser_L] = Math.Max(0, expressions[FBExpression.Upper_Lid_Raiser_L] );
            expressions[FBExpression.Upper_Lid_Raiser_R] = Math.Max(0, expressions[FBExpression.Upper_Lid_Raiser_R] );

            expressions[FBExpression.Lid_Tightener_L] = Math.Max(0, expressions[FBExpression.Lid_Tightener_L] );
            expressions[FBExpression.Lid_Tightener_R] = Math.Max(0, expressions[FBExpression.Lid_Tightener_R] );

            expressions[FBExpression.Inner_Brow_Raiser_L] = Math.Min(1, expressions[FBExpression.Inner_Brow_Raiser_L] );
            expressions[FBExpression.Brow_Lowerer_L] = Math.Min(1, expressions[FBExpression.Brow_Lowerer_L] );
            expressions[FBExpression.Outer_Brow_Raiser_L] = Math.Min(1, expressions[FBExpression.Outer_Brow_Raiser_L] );

            expressions[FBExpression.Inner_Brow_Raiser_R] = Math.Min(1, expressions[FBExpression.Inner_Brow_Raiser_R] );
            expressions[FBExpression.Brow_Lowerer_R] = Math.Min(1, expressions[FBExpression.Brow_Lowerer_R] );
            expressions[FBExpression.Outer_Brow_Raiser_R] = Math.Min(1, expressions[FBExpression.Outer_Brow_Raiser_R] );
            
            //expressions[FBExpression.Eyes_Look_Up_L] = expressions[FBExpression.Eyes_Look_Up_L] * 0.55f;
            //expressions[FBExpression.Eyes_Look_Up_R] = expressions[FBExpression.Eyes_Look_Up_R] * 0.55f;
            //expressions[FBExpression.Eyes_Look_Down_L] = expressions[FBExpression.Eyes_Look_Down_L] * 1.5f;
            //expressions[FBExpression.Eyes_Look_Down_R] = expressions[FBExpression.Eyes_Look_Down_R] * 1.5f;

            //expressions[FBExpression.Eyes_Look_Left_L] = expressions[FBExpression.Eyes_Look_Left_L] * 0.85f;
            //expressions[FBExpression.Eyes_Look_Right_L] = expressions[FBExpression.Eyes_Look_Right_L] * 0.85f;
            //expressions[FBExpression.Eyes_Look_Left_R] = expressions[FBExpression.Eyes_Look_Left_R] * 0.85f;
            //expressions[FBExpression.Eyes_Look_Right_R] = expressions[FBExpression.Eyes_Look_Right_R] * 0.85f;

            // hack: turn rots to looks
            // pitch = 47(left)-- > -47(right)
            // yaw = -55(down)-- > 43(up)
            // Eye look angle (degrees) limits calibrated to SRanipal eye tracking
            float eyeLookUpLimit = 43;
            float eyeLookDownLimit = 55;
            float eyeLookOutLimit = 47;
            float eyeLookInLimit = 47;      
            if (pitch_L > 0)
            {
                expressions[FBExpression.Eyes_Look_Left_L] = Math.Min(1, (float)(pitch_L / eyeLookOutLimit));
                expressions[FBExpression.Eyes_Look_Right_L] = 0;
            }
            else
            {
                expressions[FBExpression.Eyes_Look_Left_L] = 0;
                expressions[FBExpression.Eyes_Look_Right_L] = Math.Min(1, (float)((-pitch_L) / eyeLookInLimit));
            }
            if(yaw_L > 0)
            {
                expressions[FBExpression.Eyes_Look_Up_L] = Math.Min(1, (float)(yaw_L / eyeLookUpLimit));
                expressions[FBExpression.Eyes_Look_Down_L] = 0;
            }
            else
            {
                expressions[FBExpression.Eyes_Look_Up_L] = 0;
                expressions[FBExpression.Eyes_Look_Down_L] = Math.Min(1, (float)((-yaw_L) / eyeLookDownLimit));
            }

            if (pitch_R > 0)
            {
                expressions[FBExpression.Eyes_Look_Left_R] = Math.Min(1, (float)(pitch_R / eyeLookInLimit));
                expressions[FBExpression.Eyes_Look_Right_R] = 0;
            }
            else
            {
                expressions[FBExpression.Eyes_Look_Left_R] = 0;
                expressions[FBExpression.Eyes_Look_Right_R] = Math.Min(1, (float)((-pitch_R) / eyeLookOutLimit));
            }
            if (yaw_R > 0)
            {
                expressions[FBExpression.Eyes_Look_Up_R] = Math.Min(1, (float)(yaw_R / eyeLookUpLimit));
                expressions[FBExpression.Eyes_Look_Down_R] = 0;
            }
            else
            {
                expressions[FBExpression.Eyes_Look_Up_R] = 0;
                expressions[FBExpression.Eyes_Look_Down_R] = Math.Min(1, (float)((-yaw_R) / eyeLookDownLimit));
            }
        }
        
        
        public static class TrackingSensitivity
        {
            // Tracking Sensitivity Multipliers
            public const float EyeLid = 1.0f;
            public const float EyeSquint = 1.0f;
            public const float EyeWiden = 1.0f;
            public const float BrowInnerUp = 1.0f;
            public const float BrowOuterUp = 1.0f;
            public const float BrowDown = 1.0f;
            public const float CheekPuff = 1.13f;
            public const float CheekSuck = 2.72f;
            public const float CheekRaiser = 1.1f;
            public const float JawDrop = 1.1f;
            public const float LipTowards = 2.0f;
            public const float JawX = 1.0f;
            public const float JawThrust = 1.0f;
            public const float LipPucker = 1.21f;
            public const float MouthX = 1.0f;
            public const float LipCornerPuller = 1.22f;
            public const float LipCornerDepressor = 1.1f;
            public const float LipFunnelTop = 1.13f;
            public const float LipFunnelBottom = 8.0f;
            public const float LipSuckTop = 1.0f;
            public const float LipSuckBottom = 1.0f;
            public const float ChinRaiserTop = 0.75f;
            public const float ChinRaiserBottom = 1.0f;
            public const float LowerLipDepressor = 2.87f;
            public const float UpperLipRaiser = 1.75f;
            public const float Dimpler = 4.3f;
            public const float LipStretcher = 3.0f;
            public const float LipPressor = 10f;
            public const float LipTightener = 2.13f;
            public const float NoseWrinkler = 3.16f;


        }
        // Thank you @adjerry on the VRCFT discord for these conversions! https://docs.google.com/spreadsheets/d/118jo960co3Mgw8eREFVBsaJ7z0GtKNr52IB4Bz99VTA/edit#gid=0
        private void UpdateExpressions()
        {           
            UnifiedTrackingData.LatestEyeData.Left = MakeEye
            (
                LookLeft: expressions[FBExpression.Eyes_Look_Left_L],
                LookRight: expressions[FBExpression.Eyes_Look_Right_L],
                LookUp: expressions[FBExpression.Eyes_Look_Up_L],
                LookDown: expressions[FBExpression.Eyes_Look_Down_L],
                Openness: expressions[FBExpression.Eyes_Closed_L] ,
                Squint: Math.Min(1, expressions[FBExpression.Lid_Tightener_L] * TrackingSensitivity.EyeSquint),
                Squeeze: Math.Min(1, expressions[FBExpression.Lid_Tightener_L] * TrackingSensitivity.EyeSquint),
                Widen: Math.Min(1, expressions[FBExpression.Upper_Lid_Raiser_L] * TrackingSensitivity.EyeWiden),
                InnerUp: Math.Min(1, expressions[FBExpression.Inner_Brow_Raiser_L] * TrackingSensitivity.BrowInnerUp),
                InnerDown: Math.Min(1, expressions[FBExpression.Brow_Lowerer_L] * TrackingSensitivity.BrowDown),
                OuterUp: Math.Min(1, expressions[FBExpression.Outer_Brow_Raiser_L] * TrackingSensitivity.BrowOuterUp),
                OuterDown: Math.Min(1, expressions[FBExpression.Brow_Lowerer_L] * TrackingSensitivity.BrowDown)
            );

            UnifiedTrackingData.LatestEyeData.Right = MakeEye
            (
                LookLeft: expressions[FBExpression.Eyes_Look_Left_R],
                LookRight: expressions[FBExpression.Eyes_Look_Right_R],
                LookUp: expressions[FBExpression.Eyes_Look_Up_R],
                LookDown: expressions[FBExpression.Eyes_Look_Down_R],
                Openness: expressions[FBExpression.Eyes_Closed_R],
                Squint: Math.Min(1, expressions[FBExpression.Lid_Tightener_R] * TrackingSensitivity.EyeSquint),
                Squeeze: Math.Min(1, expressions[FBExpression.Lid_Tightener_R] * TrackingSensitivity.EyeSquint),
                Widen: Math.Min(1, expressions[FBExpression.Upper_Lid_Raiser_R] * TrackingSensitivity.EyeWiden),
                InnerUp: Math.Min(1, expressions[FBExpression.Inner_Brow_Raiser_R] * TrackingSensitivity.BrowInnerUp),
                InnerDown: Math.Min(1, expressions[FBExpression.Brow_Lowerer_R] * TrackingSensitivity.BrowDown),
                OuterUp: Math.Min(1, expressions[FBExpression.Outer_Brow_Raiser_R] * TrackingSensitivity.BrowOuterUp),
                OuterDown: Math.Min(1, expressions[FBExpression.Brow_Lowerer_R] * TrackingSensitivity.BrowDown)
            );

            UnifiedTrackingData.LatestEyeData.Combined = MakeEye
            (
                LookLeft: (expressions[FBExpression.Eyes_Look_Left_L]+expressions[FBExpression.Eyes_Look_Left_R]) / 2.0f,
                LookRight: (expressions[FBExpression.Eyes_Look_Right_L]+expressions[FBExpression.Eyes_Look_Right_R]) / 2.0f,
                LookUp: (expressions[FBExpression.Eyes_Look_Up_L]+expressions[FBExpression.Eyes_Look_Up_R]) / 2.0f,
                LookDown: (expressions[FBExpression.Eyes_Look_Down_L]+expressions[FBExpression.Eyes_Look_Down_R]) / 2.0f,
                Openness: (expressions[FBExpression.Eyes_Closed_L]+expressions[FBExpression.Eyes_Closed_R]) / 2.0f,
                Squint: Math.Min(1, TrackingSensitivity.EyeSquint * (expressions[FBExpression.Lid_Tightener_L]+expressions[FBExpression.Lid_Tightener_R]) / 2.0f),
                Squeeze: Math.Min(1, TrackingSensitivity.EyeSquint * (expressions[FBExpression.Lid_Tightener_L]+expressions[FBExpression.Lid_Tightener_R]) / 2.0f),
                Widen: Math.Min(1, TrackingSensitivity.EyeWiden *(expressions[FBExpression.Upper_Lid_Raiser_L]+expressions[FBExpression.Upper_Lid_Raiser_R]) / 2.0f),
                InnerUp: Math.Min(1, TrackingSensitivity.BrowInnerUp * (expressions[FBExpression.Inner_Brow_Raiser_L]+expressions[FBExpression.Inner_Brow_Raiser_R]) / 2.0f),
                InnerDown: Math.Min(1, TrackingSensitivity.BrowDown * (expressions[FBExpression.Brow_Lowerer_L]+expressions[FBExpression.Brow_Lowerer_R]) / 2.0f),
                OuterUp: Math.Min(1, TrackingSensitivity.BrowOuterUp * (expressions[FBExpression.Outer_Brow_Raiser_L] + expressions[FBExpression.Outer_Brow_Raiser_R]) / 2.0f),
                OuterDown: Math.Min(1, TrackingSensitivity.BrowDown * (expressions[FBExpression.Brow_Lowerer_L] + expressions[FBExpression.Brow_Lowerer_R]) / 2.0f)
            );

            UnifiedTrackingData.LatestEyeData.EyesDilation = 0.5f;
            UnifiedTrackingData.LatestEyeData.EyesPupilDiameter = 0.0035f;

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.CheekPuffLeft] = Math.Min(1, TrackingSensitivity.CheekPuff * expressions[FBExpression.Cheek_Puff_L]);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.CheekPuffRight] = Math.Min(1, TrackingSensitivity.CheekPuff * expressions[FBExpression.Cheek_Puff_R]);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.CheekSuck] = Math.Min(1, TrackingSensitivity.CheekSuck * (expressions[FBExpression.Cheek_Suck_L] + expressions[FBExpression.Cheek_Suck_R]) / 2);
            
            //Mouth Ape Shape is not shape by itself and is combinaiton with JawOpen
            //TODO - allow compatiblity
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthApeShape] = Math.Min(1, expressions[FBExpression.Lips_Toward] * TrackingSensitivity.LipTowards);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.JawOpen] = Math.Min(1, expressions[FBExpression.Jaw_Drop] * TrackingSensitivity.JawDrop);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.JawLeft] = Math.Min(1, expressions[FBExpression.Jaw_Sideways_Left] * TrackingSensitivity.JawX);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.JawRight] = Math.Min(1, expressions[FBExpression.Jaw_Sideways_Right] * TrackingSensitivity.JawX);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.JawForward] = Math.Min(1, expressions[FBExpression.Jaw_Thrust] * TrackingSensitivity.JawThrust);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthPout] = Math.Min(1, TrackingSensitivity.LipPucker * (expressions[FBExpression.Lip_Pucker_L] + expressions[FBExpression.Lip_Pucker_R]) / 2);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthUpperLeft] = Math.Min(1, expressions[FBExpression.Mouth_Left] * TrackingSensitivity.MouthX);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerLeft] = Math.Min(1, expressions[FBExpression.Mouth_Left] * TrackingSensitivity.MouthX);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthUpperRight] = Math.Min(1, expressions[FBExpression.Mouth_Right] * TrackingSensitivity.MouthX);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerRight] = Math.Min(1, expressions[FBExpression.Mouth_Right] * TrackingSensitivity.MouthX);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileLeft] = Math.Min(1, expressions[FBExpression.Lip_Corner_Puller_L] * TrackingSensitivity.LipCornerPuller);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileRight] = Math.Min(1, expressions[FBExpression.Lip_Corner_Puller_R] * TrackingSensitivity.LipCornerPuller);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadLeft] = Math.Min(1, expressions[FBExpression.Lip_Corner_Depressor_L] * TrackingSensitivity.LipCornerDepressor);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadRight] = Math.Min(1, expressions[FBExpression.Lip_Corner_Depressor_R] * TrackingSensitivity.LipCornerDepressor);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthUpperOverturn] = Math.Min(1, TrackingSensitivity.LipFunnelTop * (expressions[FBExpression.Lip_Funneler_LT] + expressions[FBExpression.Lip_Funneler_RT]) / 2);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerOverturn] = Math.Min(1, TrackingSensitivity.LipFunnelBottom * (expressions[FBExpression.Lip_Funneler_LB] + expressions[FBExpression.Lip_Funneler_RB]) / 2); 

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthUpperInside] = Math.Min(1, TrackingSensitivity.LipSuckTop * (expressions[FBExpression.Lip_Suck_LT] + expressions[FBExpression.Lip_Suck_RT]) / 2); 
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerInside] = Math.Min(1, TrackingSensitivity.LipSuckBottom * (expressions[FBExpression.Lip_Suck_LB] + expressions[FBExpression.Lip_Suck_RB]) / 2); 

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerOverlay] = Math.Min(1, + expressions[FBExpression.Chin_Raiser_T] * TrackingSensitivity.ChinRaiserTop);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerDownLeft] = Math.Min(1, expressions[FBExpression.Lower_Lip_Depressor_L] * TrackingSensitivity.LowerLipDepressor);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerDownRight] = Math.Min(1, expressions[FBExpression.Lower_Lip_Depressor_R] * TrackingSensitivity.LowerLipDepressor);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthUpperUpLeft] = Math.Min(1, expressions[FBExpression.Upper_Lip_Raiser_L] * TrackingSensitivity.UpperLipRaiser);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthUpperUpRight] = Math.Min(1, expressions[FBExpression.Upper_Lip_Raiser_R] * TrackingSensitivity.UpperLipRaiser);

            // solve issue with smilesad combined blendshape
            if (UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileLeft] > UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadLeft])
                UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadLeft] /= 1 + UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileLeft];
            else if (UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileLeft] < UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadLeft])
                UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileLeft] /= 1 + UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadLeft];

            if (UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileRight] > UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadRight])
                UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadRight] /= 1 + UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileRight];
            else if (UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileRight] < UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadRight])
                UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileRight] /= 1 + UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadRight];

            // FACS Unique Shapes
            // Lip funnel bottom not sensitive
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipFunnelerLeftBottom] = Math.Min(1, expressions[FBExpression.Lip_Funneler_LB] * TrackingSensitivity.LipFunnelBottom);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipFunnelerRightBottom] = Math.Min(1, expressions[FBExpression.Lip_Funneler_RB] * TrackingSensitivity.LipFunnelBottom);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipFunnelerLeftTop] = Math.Min(1, expressions[FBExpression.Lip_Funneler_LT] * TrackingSensitivity.LipFunnelTop);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipFunnelerRightTop] = Math.Min(1, expressions[FBExpression.Lip_Funneler_RT] * TrackingSensitivity.LipFunnelTop);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.CheekRaiserLeft] = Math.Min(1, expressions[FBExpression.Cheek_Raiser_L] * TrackingSensitivity.CheekRaiser);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.CheekRaiserRight] = Math.Min(1, expressions[FBExpression.Cheek_Raiser_R] * TrackingSensitivity.CheekRaiser);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.ChinRaiserBottom] = Math.Min(1, expressions[FBExpression.Chin_Raiser_B] * TrackingSensitivity.ChinRaiserBottom);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.ChinRaiserTop] = Math.Min(1, expressions[FBExpression.Chin_Raiser_T] * TrackingSensitivity.ChinRaiserTop);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.DimplerLeft] = Math.Min(1, expressions[FBExpression.Dimpler_L] * TrackingSensitivity.Dimpler);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.DimplerRight] = Math.Min(1, expressions[FBExpression.Dimpler_R] * TrackingSensitivity.Dimpler);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipPuckerLeft] = Math.Min(1, expressions[FBExpression.Lip_Pucker_L] * TrackingSensitivity.LipPucker);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipPuckerRight] = Math.Min(1, expressions[FBExpression.Lip_Pucker_R] * TrackingSensitivity.LipPucker);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipStretcherLeft] = Math.Min(1, expressions[FBExpression.Lip_Stretcher_L] * TrackingSensitivity.LipStretcher);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipStretcherRight] = Math.Min(1, expressions[FBExpression.Lip_Stretcher_R] * TrackingSensitivity.LipStretcher);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipPressorLeft] = Math.Min(1, expressions[FBExpression.Lip_Pressor_L] * TrackingSensitivity.LipPressor);  
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipPressorRight] = Math.Min(1, expressions[FBExpression.Lip_Pressor_R] * TrackingSensitivity.LipPressor);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipSuckLeftBottom] = Math.Min(1, expressions[FBExpression.Lip_Suck_LB] * TrackingSensitivity.LipSuckBottom);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipSuckRightBottom] = Math.Min(1, expressions[FBExpression.Lip_Suck_RB] * TrackingSensitivity.LipSuckBottom);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipSuckRightTop] = Math.Min(1, expressions[FBExpression.Lip_Suck_RT] * TrackingSensitivity.LipSuckTop);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipSuckLeftTop] = Math.Min(1, expressions[FBExpression.Lip_Suck_LT] * TrackingSensitivity.LipSuckTop);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipTightenerLeft] = Math.Min(1, expressions[FBExpression.Lip_Tightener_L] * TrackingSensitivity.LipTightener);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipTightenerRight] = Math.Min(1, expressions[FBExpression.Lip_Tightener_R] * TrackingSensitivity.LipTightener);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LowerLipDepressorLeft] = Math.Min(1, expressions[FBExpression.Lower_Lip_Depressor_L] * TrackingSensitivity.LowerLipDepressor);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LowerLipDepressorRight] = Math.Min(1, expressions[FBExpression.Lower_Lip_Depressor_R] * TrackingSensitivity.LowerLipDepressor);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.NoseWrinklerLeft] = Math.Min(1, expressions[FBExpression.Nose_Wrinkler_L] * TrackingSensitivity.NoseWrinkler);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.NoseWrinklerRight] = Math.Min(1, expressions[FBExpression.Nose_Wrinkler_R] * TrackingSensitivity.NoseWrinkler);

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipsTowards] = Math.Min(1, (expressions[FBExpression.Lips_Toward]) * TrackingSensitivity.LipTowards);

        }

        private Eye MakeEye(float LookLeft, float LookRight, float LookUp, float LookDown, float Openness, float Squint, float Squeeze, float Widen, float InnerUp, float InnerDown, float OuterUp, float OuterDown)
        {
            return new Eye()
            {
                Look = new Vector2(LookRight - LookLeft, LookUp - LookDown),
                Openness = Openness,
                Squeeze = Squeeze,
                Squint = Squint,
                Widen = Widen,
                Brow = new Eye.EyeBrow()
                {
                    InnerUp = InnerUp,
                    InnerDown = InnerDown,
                    OuterUp = OuterUp,
                    OuterDown = OuterDown,
                }
            };
        }

        public override void Teardown()
        {

        }
    }
}
