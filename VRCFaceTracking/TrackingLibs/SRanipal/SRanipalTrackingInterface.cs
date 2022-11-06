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
                //double roll = Math.Atan2(2.0 * (q_x * q_y + q_w * q_z), q_w * q_w + q_x * q_x - q_y * q_y - q_z * q_z);

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

                //Logger.Msg(expressions[63].ToString()); // confidence
                //Logger.Msg(((180.0 / Math.PI) * pitch).ToString());
                //Logger.Msg(((180.0 / Math.PI) * yaw).ToString());
                //Logger.Msg(((180.0 / Math.PI) * roll).ToString());

                //Logger.Msg(expressions[68].ToString());
                //Logger.Msg(expressions[69].ToString());
                //Logger.Msg(expressions[70].ToString());

                //Logger.Msg(pitch_L.ToString());
                //Logger.Msg(yaw_L.ToString());

                //Logger.Msg(pitch_R.ToString());
                //Logger.Msg(yaw_R.ToString());

                //Logger.Msg(expressions[69].ToString());
                //Logger.Msg(expressions[70].ToString());
                //Logger.Msg(expressions[71].ToString());


                //Logger.Msg(expressions[63].ToString());
                //Logger.Msg(expressions[64].ToString());
                //Logger.Msg(expressions[65].ToString());
                //Logger.Msg(expressions[66].ToString());
                //Logger.Msg(expressions[67].ToString());
                //Logger.Msg(expressions[68].ToString());
                //Logger.Msg(expressions[69].ToString());

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
            //expressions[FBExpression.Eyes_Closed_L] = ((
            //    (expressions[FBExpression.Eyes_Closed_L] -
            //        Math.Min(expressions[FBExpression.Eyes_Look_Down_L],
            //        expressions[FBExpression.Eyes_Look_Down_R])))); // temporary
            //expressions[FBExpression.Eyes_Closed_R] = ((
            //    (expressions[FBExpression.Eyes_Closed_R] -
            //        Math.Min(expressions[FBExpression.Eyes_Look_Down_L],
            //        expressions[FBExpression.Eyes_Look_Down_R])))); // temporary

            if(expressions[FBExpression.Eyes_Look_Down_L] == expressions[FBExpression.Eyes_Look_Up_L] && expressions[FBExpression.Eyes_Closed_L] > 0.25f)
            { // wonky eyelid case, eyes are actually closed now
                expressions[FBExpression.Eyes_Closed_L] = 0; //0.9f - (expressions[FBExpression.Lid_Tightener_L] * 3);
            }
            else
            {
                expressions[FBExpression.Eyes_Closed_L] = 0.9f - ((expressions[FBExpression.Eyes_Closed_L] * 3) / (1+ expressions[FBExpression.Eyes_Look_Down_L] * 3));
            }

            if (expressions[FBExpression.Eyes_Look_Down_R] == expressions[FBExpression.Eyes_Look_Up_R] && expressions[FBExpression.Eyes_Closed_R] > 0.25f)
            { // wonky eyelid case, eyes are actually closed now
                expressions[FBExpression.Eyes_Closed_R] = 0; //0.9f - (expressions[FBExpression.Lid_Tightener_R] * 3);
            }
            else
            {
                expressions[FBExpression.Eyes_Closed_R] = 0.9f - ((expressions[FBExpression.Eyes_Closed_R] * 3) / (1 + expressions[FBExpression.Eyes_Look_Down_R] * 3));
            }

            //expressions[FBExpression.Lid_Tightener_L] = 0.8f-expressions[FBExpression.Eyes_Closed_L]; // sad: fix combined param instead
            //expressions[FBExpression.Lid_Tightener_R] = 0.8f-expressions[FBExpression.Eyes_Closed_R]; // sad: fix combined param instead

            if (1 - expressions[FBExpression.Eyes_Closed_L] < expressions[FBExpression.Lid_Tightener_L])
                expressions[FBExpression.Lid_Tightener_L] = (1 - expressions[FBExpression.Eyes_Closed_L]) - 0.01f;

            if (1 - expressions[FBExpression.Eyes_Closed_R] < expressions[FBExpression.Lid_Tightener_R])
                expressions[FBExpression.Lid_Tightener_R] = (1 - expressions[FBExpression.Eyes_Closed_R]) - 0.01f;

            //expressions[FBExpression.Lid_Tightener_L] = Math.Max(0, expressions[FBExpression.Lid_Tightener_L] - 0.15f);
            //expressions[FBExpression.Lid_Tightener_R] = Math.Max(0, expressions[FBExpression.Lid_Tightener_R] - 0.15f);

            expressions[FBExpression.Upper_Lid_Raiser_L] = Math.Max(0, expressions[FBExpression.Upper_Lid_Raiser_L] - 0.5f);
            expressions[FBExpression.Upper_Lid_Raiser_R] = Math.Max(0, expressions[FBExpression.Upper_Lid_Raiser_R] - 0.5f);

            expressions[FBExpression.Lid_Tightener_L] = Math.Max(0, expressions[FBExpression.Lid_Tightener_L] - 0.5f);
            expressions[FBExpression.Lid_Tightener_R] = Math.Max(0, expressions[FBExpression.Lid_Tightener_R] - 0.5f);

            expressions[FBExpression.Inner_Brow_Raiser_L] = Math.Min(1, expressions[FBExpression.Inner_Brow_Raiser_L] );// * 4;
            expressions[FBExpression.Brow_Lowerer_L] = Math.Min(1, expressions[FBExpression.Brow_Lowerer_L] * 3f);// * 4;
            expressions[FBExpression.Outer_Brow_Raiser_L] = Math.Min(1, expressions[FBExpression.Outer_Brow_Raiser_L] );// * 4;

            expressions[FBExpression.Inner_Brow_Raiser_R] = Math.Min(1, expressions[FBExpression.Inner_Brow_Raiser_R] );// * 4;
            expressions[FBExpression.Brow_Lowerer_R] = Math.Min(1, expressions[FBExpression.Brow_Lowerer_R] * 3f);// * 4;
            expressions[FBExpression.Outer_Brow_Raiser_R] = Math.Min(1, expressions[FBExpression.Outer_Brow_Raiser_R] );// * 4;

            
            expressions[FBExpression.Eyes_Look_Up_L] = expressions[FBExpression.Eyes_Look_Up_L] * 0.55f;
            expressions[FBExpression.Eyes_Look_Up_R] = expressions[FBExpression.Eyes_Look_Up_R] * 0.55f;
            expressions[FBExpression.Eyes_Look_Down_L] = expressions[FBExpression.Eyes_Look_Down_L] * 1.5f;
            expressions[FBExpression.Eyes_Look_Down_R] = expressions[FBExpression.Eyes_Look_Down_R] * 1.5f;

            expressions[FBExpression.Eyes_Look_Left_L] = expressions[FBExpression.Eyes_Look_Left_L] * 0.85f;
            expressions[FBExpression.Eyes_Look_Right_L] = expressions[FBExpression.Eyes_Look_Right_L] * 0.85f;
            expressions[FBExpression.Eyes_Look_Left_R] = expressions[FBExpression.Eyes_Look_Left_R] * 0.85f;
            expressions[FBExpression.Eyes_Look_Right_R] = expressions[FBExpression.Eyes_Look_Right_R] * 0.85f;




            // hack: turn rots to looks
            //pitch = 29(left)-- > -29(right)
            //yaw = -27(down)-- > 27(up)

            //Angle Degree Limits calibrated to SRanipal eye tracking
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

        // Thank you @adjerry on the VRCFT discord for these conversions! https://docs.google.com/spreadsheets/d/118jo960co3Mgw8eREFVBsaJ7z0GtKNr52IB4Bz99VTA/edit#gid=0
        private void UpdateExpressions()
        {
            UnifiedTrackingData.LatestEyeData.Left = MakeEye
            (
                LookLeft: expressions[FBExpression.Eyes_Look_Left_L],
                LookRight: expressions[FBExpression.Eyes_Look_Right_L],
                LookUp: expressions[FBExpression.Eyes_Look_Up_L],
                LookDown: expressions[FBExpression.Eyes_Look_Down_L],
                Openness: expressions[FBExpression.Eyes_Closed_L],
                Squint: expressions[FBExpression.Lid_Tightener_L],
                Squeeze: expressions[FBExpression.Lid_Tightener_L],
                Widen: expressions[FBExpression.Upper_Lid_Raiser_L],
                InnerUp: expressions[FBExpression.Inner_Brow_Raiser_L],
                InnerDown: expressions[FBExpression.Brow_Lowerer_L],
                OuterUp: expressions[FBExpression.Outer_Brow_Raiser_L],
                OuterDown: expressions[FBExpression.Brow_Lowerer_L]
            );

            UnifiedTrackingData.LatestEyeData.Right = MakeEye
            (
                LookLeft: expressions[FBExpression.Eyes_Look_Left_R],
                LookRight: expressions[FBExpression.Eyes_Look_Right_R],
                LookUp: expressions[FBExpression.Eyes_Look_Up_R],
                LookDown: expressions[FBExpression.Eyes_Look_Down_R],
                Openness: expressions[FBExpression.Eyes_Closed_R],
                Squint: expressions[FBExpression.Lid_Tightener_R],
                Squeeze: expressions[FBExpression.Lid_Tightener_R],
                Widen: expressions[FBExpression.Upper_Lid_Raiser_R],
                InnerUp: expressions[FBExpression.Inner_Brow_Raiser_R],
                InnerDown: expressions[FBExpression.Brow_Lowerer_R],
                OuterUp: expressions[FBExpression.Outer_Brow_Raiser_R],
                OuterDown: expressions[FBExpression.Brow_Lowerer_R]
            );



            UnifiedTrackingData.LatestEyeData.Combined = MakeEye
            (
                LookLeft: (expressions[FBExpression.Eyes_Look_Left_R]+expressions[FBExpression.Eyes_Look_Left_R])/2.0f,
                LookRight: (expressions[FBExpression.Eyes_Look_Right_R]+expressions[FBExpression.Eyes_Look_Right_R]) / 2.0f,
                LookUp: (expressions[FBExpression.Eyes_Look_Up_R]+expressions[FBExpression.Eyes_Look_Up_R]) / 2.0f,
                LookDown: (expressions[FBExpression.Eyes_Look_Down_R]+expressions[FBExpression.Eyes_Look_Down_R]) / 2.0f,
                Openness: (expressions[FBExpression.Eyes_Closed_R]+expressions[FBExpression.Eyes_Closed_R]) / 2.0f,
                Squint: (expressions[FBExpression.Lid_Tightener_R]+expressions[FBExpression.Lid_Tightener_R]) / 2.0f,
                Squeeze: (expressions[FBExpression.Lid_Tightener_R]+expressions[FBExpression.Lid_Tightener_R]) / 2.0f,
                Widen: (expressions[FBExpression.Upper_Lid_Raiser_R]+expressions[FBExpression.Upper_Lid_Raiser_R]) / 2.0f,
                InnerUp: (expressions[FBExpression.Inner_Brow_Raiser_R]+expressions[FBExpression.Inner_Brow_Raiser_R]) / 2.0f,
                InnerDown: (expressions[FBExpression.Brow_Lowerer_R]+expressions[FBExpression.Brow_Lowerer_R]) / 2.0f,
                OuterUp: (expressions[FBExpression.Outer_Brow_Raiser_R]+expressions[FBExpression.Outer_Brow_Raiser_R]) / 2.0f,
                OuterDown: (expressions[FBExpression.Brow_Lowerer_R]+expressions[FBExpression.Brow_Lowerer_R]) / 2.0f
            );

            UnifiedTrackingData.LatestEyeData.EyesDilation = 0.5f;
            UnifiedTrackingData.LatestEyeData.EyesPupilDiameter = 0.0035f;

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.CheekPuffLeft] = expressions[FBExpression.Cheek_Puff_L];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.CheekPuffRight] = expressions[FBExpression.Cheek_Puff_R];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.CheekSuck] = (expressions[FBExpression.Cheek_Suck_L] + expressions[FBExpression.Cheek_Suck_R]) / 2;
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.JawOpen] = expressions[FBExpression.Jaw_Drop];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.JawLeft] = expressions[FBExpression.Jaw_Sideways_Left];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.JawRight] = expressions[FBExpression.Jaw_Sideways_Right];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.JawForward] = expressions[FBExpression.Jaw_Thrust];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthPout] = (expressions[FBExpression.Lip_Pucker_L] + expressions[FBExpression.Lip_Pucker_R]) / 3;
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthUpperLeft] = expressions[FBExpression.Mouth_Left];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerLeft] = (expressions[FBExpression.Mouth_Left] + expressions[FBExpression.Lip_Funneler_LB]) / 2;
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthUpperRight] = expressions[FBExpression.Mouth_Right];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerRight] = (expressions[FBExpression.Mouth_Right] + expressions[FBExpression.Lip_Funneler_RB]) / 2;
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileLeft] = Math.Min(1, expressions[FBExpression.Lip_Corner_Puller_L] * 1.2f);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileRight] = Math.Min(1, expressions[FBExpression.Lip_Corner_Puller_R] * 1.2f);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadLeft] = Math.Min(1, (expressions[FBExpression.Lip_Corner_Depressor_L] + expressions[FBExpression.Lip_Stretcher_L]) * 0.75f);//Math.Min(1, (expressions[FBExpression.Lip_Corner_Depressor_L]) * 1.5f);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadRight] = Math.Min(1, (expressions[FBExpression.Lip_Corner_Depressor_R] + expressions[FBExpression.Lip_Stretcher_R]) * 0.75f);//Math.Min(1, (expressions[FBExpression.Lip_Corner_Depressor_R]) * 1.5f);
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthUpperOverturn] = (expressions[FBExpression.Lips_Toward] + expressions[FBExpression.Lip_Funneler_LT] + expressions[FBExpression.Lip_Funneler_RT]) / 3;
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerOverturn] = (expressions[FBExpression.Lips_Toward] + expressions[FBExpression.Lip_Funneler_LB] + expressions[FBExpression.Lip_Funneler_RB]) / 3;


            // solve issue with smilesad combined blendshape
            if (UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileLeft] > UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadLeft])
                UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadLeft] /= 1 + UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileLeft];
            else if (UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileLeft] < UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadLeft])
                UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileLeft] /= 1 + UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadLeft];

            if (UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileRight] > UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadRight])
                UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadRight] /= 1 + UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileRight];
            else if (UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileRight] < UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadRight])
                UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSmileRight] /= 1 + UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthSadRight];

            //UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthUpperUpLeft] = (expressions[FBExpression.Upper_Lip_Raiser_L] + expressions[FBExpression.Lip_Funneler_LT]) / 2;
            //UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthUpperUpRight] = (expressions[FBExpression.Upper_Lip_Raiser_R] + expressions[FBExpression.Lip_Funneler_RT]) / 2;

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipFunnelerLeftBottom] = expressions[FBExpression.Lip_Funneler_LB];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipFunnelerRightBottom] = expressions[FBExpression.Lip_Funneler_RB];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipFunnelerLeftTop] = expressions[FBExpression.Lip_Funneler_LT];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipFunnelerRightTop] = expressions[FBExpression.Lip_Funneler_RT];

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.CheekRaiserLeft] = expressions[FBExpression.Cheek_Raiser_L];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.CheekRaiserRight] = expressions[FBExpression.Cheek_Raiser_R];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.ChinRaiserBottom] = expressions[FBExpression.Chin_Raiser_B];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.ChinRaiserTop] = expressions[FBExpression.Chin_Raiser_T];

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.DimplerLeft] = expressions[FBExpression.Dimpler_L];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.DimplerRight] = expressions[FBExpression.Dimpler_R];

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipPressorLeft] = expressions[FBExpression.Lip_Pressor_L];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipPressorRight] = expressions[FBExpression.Lip_Pressor_R];

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipPuckerLeft] = expressions[FBExpression.Lip_Pucker_L];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipPuckerRight] = expressions[FBExpression.Lip_Pucker_R];

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipStretcherLeft] = expressions[FBExpression.Lip_Stretcher_L];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipStretcherRight] = expressions[FBExpression.Lip_Stretcher_R];

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipSuckLeftBottom] = expressions[FBExpression.Lip_Suck_LB];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipSuckLeftTop] = expressions[FBExpression.Lip_Suck_LT];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipSuckRightBottom] = expressions[FBExpression.Lip_Suck_RB];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipSuckRightTop] = expressions[FBExpression.Lip_Suck_RT];

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipTightenerLeft] = expressions[FBExpression.Lip_Tightener_L];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LipTightenerRight] = expressions[FBExpression.Lip_Tightener_R];

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LowerLipDepressorLeft] = expressions[FBExpression.Lower_Lip_Depressor_L];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.LowerLipDepressorRight] = expressions[FBExpression.Lower_Lip_Depressor_R];

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.NoseWrinklerLeft] = expressions[FBExpression.Nose_Wrinkler_L];
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.NoseWrinklerRight] = expressions[FBExpression.Nose_Wrinkler_R];

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthApeShape] = Math.Min(1, expressions[FBExpression.Lips_Toward] * 1.4f * (0.5f+expressions[FBExpression.Jaw_Drop]));

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthUpperInside] = (expressions[FBExpression.Lip_Suck_LT] + expressions[FBExpression.Lip_Suck_RT]) / 2;
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerInside] = (expressions[FBExpression.Lip_Suck_LB] + expressions[FBExpression.Lip_Suck_RB]) / 2;

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerDownLeft] = Math.Min(1, (expressions[FBExpression.Lip_Funneler_LB] + expressions[FBExpression.Lower_Lip_Depressor_L]));
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerDownRight] = Math.Min(1, (expressions[FBExpression.Lip_Funneler_RB] + expressions[FBExpression.Lower_Lip_Depressor_R]));

            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthUpperUpLeft] = Math.Min(1, (expressions[FBExpression.Lip_Funneler_LT] + expressions[FBExpression.Upper_Lip_Raiser_L]*2));
            UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthUpperUpRight] = Math.Min(1, (expressions[FBExpression.Lip_Funneler_RT] + expressions[FBExpression.Upper_Lip_Raiser_R]*2));


            // Possible matches
            //UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerOverlay = FBData.faceData.ChinRaiserB;
            //UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthUpperInside = FBData.faceData.ChinRaiserB;
            //UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerInside = FBData.faceData.ChinRaiserT;
            //UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerDownLeft = FBData.faceData.LowerLipDepressorL;
            //UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthLowerDownRight = FBData.faceData.LowerLipDepressorR;

            // Expressions reported by @aero that look the best - would be best to get a side by side comparison
            //LeftEyeX = data[E["EYESLOOKRIGHTL"] - data[E["EYESLOOKLEFTL"]]
            //RightEyeX = data[E["EYESLOOKRIGHTR"] - data[E["EYESLOOKLEFTR"]]
            //EyesY = (((data[E["EYESLOOKUPL"] + data[E["EYESLOOKUPR"]) / 1.6) - (data[E["EYESLOOKDOWNL"] + data[E["EYESLOOKDOWNR"])) / 2
            //LeftEyeLid = data[E["EYESCLOSEDL"] + min(data[E["EYESLOOKDOWNL"], data[E["EYESLOOKDOWNR"])
            //RightEyeLid = data[E["EYESCLOSEDR"] + min(data[E["EYESLOOKDOWNL"], data[E["EYESLOOKDOWNR"])
            //JawOpen = data[E["JAWDROP"]]
            //MouthPout = (data[E["LIPPUCKERL"] + data[E["LIPPUCKERR"]) / 2
            //JawX = (data[E["MOUTHRIGHT"] - data[E["MOUTHLEFT"]) * 2
            //JawForward = data[E["JAWTHRUST"]]
            //CheekPuffLeft = data[E["CHEEKPUFFL"] * 4
            //CheekPuffRight = data[E["CHEEKPUFFR"] * 4
            //SmileSadLeft = (data[E["DIMPLERL"] + (data[E["UPPERLIPRAISERL"] * 2)) - ((data[E["LOWERLIPDEPRESSORL"]) * 4)
            //SmileSadRight = (data[E["DIMPLERR"] + (data[E["UPPERLIPRAISERR"] * 2)) - ((data[E["LOWERLIPDEPRESSORR"]) * 4)
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
