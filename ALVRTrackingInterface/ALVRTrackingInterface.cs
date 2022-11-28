using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using VRCFaceTracking;
using VRCFaceTracking.Params;
using VRCFaceTracking.Params.Lip;

namespace ALVRTrackingInterface
{
    public class ALVRTrackingInterface : ExtTrackingModule
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

        public override (bool SupportsEye, bool SupportsExpressions) Supported => (true, true);

        public override (bool eyeSuccess, bool expressionSuccess) Initialize()
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

            LoadQuestProSensitivity();
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

        private bool LoadQuestProSensitivity()
        {
            try
            {
                Dictionary<String, float> multipliers = LoadMultiplierDict();
                
                Logger.Msg("Successful loading Quest Pro custom sensitivity:");

                Logger.Msg("Loaded custom Quest Pro multipliers:");
                foreach (string s in multipliers.Keys)
                {
                    var field = typeof(TrackingSensitivity).GetField(s, BindingFlags.Static | BindingFlags.Public);
                    if (field == null)
                        Logger.Msg("Invalid sensitivity name: " + s);
                    else
                    {
                        if ((float)field.GetValue(s) != multipliers[s])
                        {
                            Logger.Msg(s + ": " + field.GetValue(s) + " >> " + multipliers[s]);
                            field.SetValue(null, multipliers[s]);
                        }
                                               
                    }                                               
                }

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

                UpdateEye(ref UnifiedTracking.AllData.LatestExpressionData.Eye, ref expressions);
                UpdateExpressions(ref UnifiedTracking.AllData.LatestExpressionData.Shapes, ref expressions);

                UnifiedTracking.AllData.UpdateData();
            }
            catch (SocketException e)
            {
                Logger.Error(e.Message);
                Thread.Sleep(1000);
            }
        }

        private Dictionary<String, float> LoadMultiplierDict()
        {
            
            Dictionary<String, float> multipliers = new Dictionary<String, float>();
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "questProSensitivity.ini");
            string text = File.ReadAllText(filePath).Trim();
            if (File.Exists(filePath))
            {              
                foreach (string line in text.Split('\n'))
                {
                    if (line.Length > 3 && !line.Contains("#") && !line.Contains("[") && !line.Contains(";"))
                    {
                        string cleaned = line.Replace(" ", "").Trim(); // trim spaces for safer parsing (We dont need them anyway)
                        string[] pair = cleaned.Split('=');

                        multipliers[pair[0]] = float.Parse(pair[1]);
                    }
                }
            }

            return multipliers;
        }

        public class TrackingSensitivity
        {
            // Tracking Sensitivity Multipliers
            public static float EyeLid = 1.0f;
            public static float EyeSquint = 1.0f;
            public static float EyeWiden = 1.0f;
            public static float BrowInnerUp = 1.0f;
            public static float BrowOuterUp = 1.0f;
            public static float BrowDown = 1.0f;
            public static float CheekPuff = 1.0f;
            public static float CheekSuck = 1.0f;
            public static float CheekRaiser = 1.0f;
            public static float JawOpen = 1.0f;
            public static float MouthApeShape = 1.0f;
            public static float JawX = 1.0f;
            public static float JawForward = 1.0f;
            public static float LipPucker = 1.0f;
            public static float MouthX = 1.0f;
            public static float MouthSmile = 1.0f;
            public static float MouthFrown = 1.0f;
            public static float LipFunnelTop = 1.0f;
            public static float LipFunnelBottom = 1.0f;  //VERY NOT SENSITIVE
            public static float LipSuckTop = 1.0f;
            public static float LipSuckBottom = 1.0f;
            public static float ChinRaiserTop = 1.0f;
            public static float ChinRaiserBottom = 1.0f;
            public static float MouthLowerDown = 1.0f;
            public static float MouthUpperUp = 1.0f;
            public static float MouthDimpler = 1.0f;
            public static float MouthStretch = 1.0f;
            public static float MouthPress = 1.0f;        //VERY NOT SENSITIVE
            public static float MouthTightener = 1.0f;
            public static float NoseSneer = 1.0f;
        }

        // Preprocess our expressions per the Meta Documentation
        private void UpdateEye(ref UnifiedEyeData eye, ref float[] expressions)
        {
            #region Eye Data parsing

            // Recover true eye closed values; as you look down the eye closes.
            // from FaceTrackingSystem.CS from Movement Aura Scene in https://github.com/oculus-samples/Unity-Movement
            float eyeClosedL = Math.Min(1, expressions[(int)FBExpression.Eyes_Closed_L] + expressions[(int)FBExpression.Eyes_Look_Down_L] * 0.5f);
            float eyeClosedR = Math.Min(1, expressions[(int)FBExpression.Eyes_Closed_R] + expressions[(int)FBExpression.Eyes_Look_Down_R] * 0.5f);

            // Add Lid tightener to eye lid close to help get value closed
            // eyeClosedL = Math.Min(1, eyeClosedL + expressions[(int)FBExpression.Lid_Tightener_L] * 0.5f);
            // eyeClosedR = Math.Min(1, eyeClosedR + expressions[(int)FBExpression.Lid_Tightener_R] * 0.5f);

            // Convert from Eye Closed to Eye Openness and limit from going negative. Set the max higher than normal to offset the eye lid to help keep eye lid open.
            eye.Left.Openness = Math.Min(1, Math.Max(0, 1.1f - eyeClosedL * TrackingSensitivity.EyeLid));
            eye.Right.Openness = Math.Min(1, Math.Max(0, 1.1f - eyeClosedR * TrackingSensitivity.EyeLid));

            // As eye opens there is an issue flickering between eye wide and eye not fully open with the combined eye lid parameters. Need to reduce the eye widen value until openess is closer to value of 1. When not fully open will do constant value to reduce the eye widen.
            float eyeWidenL = Math.Max(0, expressions[(int)FBExpression.Upper_Lid_Raiser_L] * TrackingSensitivity.EyeWiden - 3.0f * (1 - eye.Left.Openness));
            float eyeWidenR = Math.Max(0, expressions[(int)FBExpression.Upper_Lid_Raiser_R] * TrackingSensitivity.EyeWiden - 3.0f * (1 - eye.Right.Openness));

            // Feedback eye widen to openess, this will help drive the openness value higher from eye widen values
            eye.Left.Openness += eyeWidenL;
            eye.Right.Openness += eyeWidenR;

            // Lid Tightener is not tracked the same as SRanipal eye squeeze. This causes problems with combined parameters. The lid tightener has more controls the fine state of closing the eye while the eye lid is more of control blinking.
            // Eye close is non-linear and seems to be based on the confidence of that eye blink is detected. Lid tightener will be used to control the eye state thus squeeze will be disabled for now for the Quest Pro mapping.

            // Subtract eye close
            //float squeezeL = Math.Max(0, expressions[(int)FBExpression.Lid_Tightener_L] - expressions[(int)FBExpression.Eyes_Closed_L] * 1.0f);
            //float squeezeR = Math.Max(0, expressions[(int)FBExpression.Lid_Tightener_R] - expressions[(int)FBExpression.Eyes_Closed_R] * 1.0f);
            //float squeezeL = 0;
            //float squeezeR = 0;

            #endregion

            #region Eye Gaze parsing

            // pitch = 47(left)-- > -47(right)
            // yaw = -55(down)-- > 43(up)
            // Eye look angle (degrees) limits calibrated to SRanipal eye tracking

            float eyeLookUpLimit = 43;
            float eyeLookDownLimit = 55;
            float eyeLookOutLimit = 47;
            float eyeLookInLimit = 47;    
            
            if (pitch_L > 0)
            {
                expressions[(int)FBExpression.Eyes_Look_Left_L] = Math.Min(1, (float)(pitch_L / eyeLookOutLimit));
                expressions[(int)FBExpression.Eyes_Look_Right_L] = 0;
            }
            else
            {
                expressions[(int)FBExpression.Eyes_Look_Left_L] = 0;
                expressions[(int)FBExpression.Eyes_Look_Right_L] = Math.Min(1, (float)((-pitch_L) / eyeLookInLimit));
            }
            if(yaw_L > 0)
            {
                expressions[(int)FBExpression.Eyes_Look_Up_L] = Math.Min(1, (float)(yaw_L / eyeLookUpLimit));
                expressions[(int)FBExpression.Eyes_Look_Down_L] = 0;
            }
            else
            {
                expressions[(int)FBExpression.Eyes_Look_Up_L] = 0;
                expressions[(int)FBExpression.Eyes_Look_Down_L] = Math.Min(1, (float)((-yaw_L) / eyeLookDownLimit));
            }

            if (pitch_R > 0)
            {
                expressions[(int)FBExpression.Eyes_Look_Left_R] = Math.Min(1, (float)(pitch_R / eyeLookInLimit));
                expressions[(int)FBExpression.Eyes_Look_Right_R] = 0;
            }
            else
            {
                expressions[(int)FBExpression.Eyes_Look_Left_R] = 0;
                expressions[(int)FBExpression.Eyes_Look_Right_R] = Math.Min(1, (float)((-pitch_R) / eyeLookOutLimit));
            }
            if (yaw_R > 0)
            {
                expressions[(int)FBExpression.Eyes_Look_Up_R] = Math.Min(1, (float)(yaw_R / eyeLookUpLimit));
                expressions[(int)FBExpression.Eyes_Look_Down_R] = 0;
            }
            else
            {
                expressions[(int)FBExpression.Eyes_Look_Up_R] = 0;
                expressions[(int)FBExpression.Eyes_Look_Down_R] = Math.Min(1, (float)((-yaw_R) / eyeLookDownLimit));
            }

            #endregion

            #region Eye Data to UnifiedEye

            //Porting of eye tracking parameters
            eye.Left = MakeEye
            (
                LookLeft: expressions[(int)FBExpression.Eyes_Look_Left_L],
                LookRight: expressions[(int)FBExpression.Eyes_Look_Right_L],
                LookUp: expressions[(int)FBExpression.Eyes_Look_Up_L],
                LookDown: expressions[(int)FBExpression.Eyes_Look_Down_L],
                Openness: Math.Min(1, eye.Left.Openness)
            );

            eye.Right = MakeEye
            (
                LookLeft: expressions[(int)FBExpression.Eyes_Look_Left_R],
                LookRight: expressions[(int)FBExpression.Eyes_Look_Right_R],
                LookUp: expressions[(int)FBExpression.Eyes_Look_Up_R],
                LookDown: expressions[(int)FBExpression.Eyes_Look_Down_R],
                Openness: Math.Min(1, eye.Right.Openness)
            );

            // Eye dilation code, automated process maybe?
            eye.Left.PupilDiameter_MM  = 0.0035f;
            eye.Right.PupilDiameter_MM = 0.0035f;

            #endregion
        }

        // Thank you @adjerry on the VRCFT discord for these conversions! https://docs.google.com/spreadsheets/d/118jo960co3Mgw8eREFVBsaJ7z0GtKNr52IB4Bz99VTA/edit#gid=0
        private void UpdateExpressions(ref UnifiedExpressionShape[] unifiedExpressions, ref float[] expressions)
        {

            // Mapping to existing parameters

            #region Eye Expressions Set

            unifiedExpressions[(int)UnifiedExpressions.EyeWideLeft].Weight = expressions[(int)FBExpression.Upper_Lid_Raiser_L] * TrackingSensitivity.EyeWiden;
            unifiedExpressions[(int)UnifiedExpressions.EyeWideRight].Weight = expressions[(int)FBExpression.Upper_Lid_Raiser_R] * TrackingSensitivity.EyeWiden;

            unifiedExpressions[(int)UnifiedExpressions.EyeSquintLeft].Weight = expressions[(int)FBExpression.Lid_Tightener_L] * TrackingSensitivity.EyeSquint;
            unifiedExpressions[(int)UnifiedExpressions.EyeSquintRight].Weight = expressions[(int)FBExpression.Lid_Tightener_R] * TrackingSensitivity.EyeSquint;

            #endregion

            #region Base Face Expressions Set

            // Mouth Ape Shape is combination of shapes. The shape by itself and is combination with Lips Towards and Lip Corner Depressors.              
            unifiedExpressions[(int)UnifiedExpressions.MouthClosed].Weight = expressions[(int)FBExpression.Lips_Toward] * TrackingSensitivity.MouthApeShape;
            // Subtract ApeShapeShape as Jaw Open will go towards zero as ape shape increase.
            unifiedExpressions[(int)UnifiedExpressions.JawOpen].Weight = expressions[(int)FBExpression.Jaw_Drop] * TrackingSensitivity.JawOpen;
            
            //UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.JawDrop] = Math.Min(1, expressions[(int)FBExpression.Jaw_Drop] * TrackingSensitivity.JawDrop); //TESTING MOUTH APE CONTROL - NON COMPENSATED VALUE

            unifiedExpressions[(int)UnifiedExpressions.JawLeft].Weight = expressions[(int)FBExpression.Jaw_Sideways_Left] * TrackingSensitivity.JawX;
            unifiedExpressions[(int)UnifiedExpressions.JawRight].Weight = expressions[(int)FBExpression.Jaw_Sideways_Right] * TrackingSensitivity.JawX;
            unifiedExpressions[(int)UnifiedExpressions.JawForward].Weight = expressions[(int)FBExpression.Jaw_Thrust] * TrackingSensitivity.JawForward;
            unifiedExpressions[(int)UnifiedExpressions.LipPuckerRight].Weight = expressions[(int)FBExpression.Lip_Pucker_R] * TrackingSensitivity.LipPucker;
            unifiedExpressions[(int)UnifiedExpressions.LipPuckerLeft].Weight = expressions[(int)FBExpression.Lip_Pucker_L] * TrackingSensitivity.LipPucker;

            // Cheek puff can be triggered by low values of lip pucker (around 0.1), subtract cheek puff with mouth pout values.
            unifiedExpressions[(int)UnifiedExpressions.CheekPuffLeft].Weight = 1.0f - expressions[(int)FBExpression.Cheek_Puff_L] * TrackingSensitivity.CheekPuff;
            unifiedExpressions[(int)UnifiedExpressions.CheekPuffRight].Weight = 1.0f - expressions[(int)FBExpression.Cheek_Puff_R] * TrackingSensitivity.CheekPuff;
            
            unifiedExpressions[(int)UnifiedExpressions.CheekSuckLeft].Weight = expressions[(int)FBExpression.Cheek_Suck_L] * TrackingSensitivity.CheekSuck;
            unifiedExpressions[(int)UnifiedExpressions.CheekSuckRight].Weight = expressions[(int)FBExpression.Cheek_Suck_R] * TrackingSensitivity.CheekSuck;
            unifiedExpressions[(int)UnifiedExpressions.MouthTopLeft].Weight = expressions[(int)FBExpression.Mouth_Left] * TrackingSensitivity.MouthX;
            unifiedExpressions[(int)UnifiedExpressions.MouthBottomLeft].Weight = expressions[(int)FBExpression.Mouth_Left] * TrackingSensitivity.MouthX;
            unifiedExpressions[(int)UnifiedExpressions.MouthTopRight].Weight = expressions[(int)FBExpression.Mouth_Right] * TrackingSensitivity.MouthX;
            unifiedExpressions[(int)UnifiedExpressions.MouthBottomRight].Weight = expressions[(int)FBExpression.Mouth_Right] * TrackingSensitivity.MouthX;
            unifiedExpressions[(int)UnifiedExpressions.MouthSmileLeft].Weight = expressions[(int)FBExpression.Lip_Corner_Puller_L] * TrackingSensitivity.MouthSmile;
            unifiedExpressions[(int)UnifiedExpressions.MouthSmileRight].Weight = expressions[(int)FBExpression.Lip_Corner_Puller_R] * TrackingSensitivity.MouthSmile;

            // Lip corner depressors are part of mouth ape shape, will subtract the current value of mouthApeShape from lip corner depressor to compensate
            unifiedExpressions[(int)UnifiedExpressions.MouthFrownLeft].Weight = expressions[(int)FBExpression.Lip_Corner_Depressor_L] * TrackingSensitivity.MouthFrown;
            unifiedExpressions[(int)UnifiedExpressions.MouthFrownRight].Weight = expressions[(int)FBExpression.Lip_Corner_Depressor_R] * TrackingSensitivity.MouthFrown;
            
            unifiedExpressions[(int)UnifiedExpressions.LipFunnelTopLeft].Weight = expressions[(int)FBExpression.Lip_Funneler_LT] * TrackingSensitivity.LipFunnelTop;
            unifiedExpressions[(int)UnifiedExpressions.LipFunnelTopRight].Weight = expressions[(int)FBExpression.Lip_Funneler_RT] * TrackingSensitivity.LipFunnelTop;
            unifiedExpressions[(int)UnifiedExpressions.LipFunnelBottomLeft].Weight = expressions[(int)FBExpression.Lip_Funneler_LB] * TrackingSensitivity.LipFunnelBottom;
            unifiedExpressions[(int)UnifiedExpressions.LipFunnelBottomRight].Weight = expressions[(int)FBExpression.Lip_Funneler_RB] * TrackingSensitivity.LipFunnelBottom;
            unifiedExpressions[(int)UnifiedExpressions.LipSuckTopLeft].Weight = expressions[(int)FBExpression.Lip_Suck_LT] * TrackingSensitivity.LipSuckTop;
            unifiedExpressions[(int)UnifiedExpressions.LipSuckTopRight].Weight = expressions[(int)FBExpression.Lip_Suck_RT] * TrackingSensitivity.LipSuckTop;
            unifiedExpressions[(int)UnifiedExpressions.LipSuckBottomLeft].Weight = expressions[(int)FBExpression.Lip_Suck_LB] * TrackingSensitivity.LipSuckBottom;
            unifiedExpressions[(int)UnifiedExpressions.LipSuckBottomRight].Weight = expressions[(int)FBExpression.Lip_Suck_RB] * TrackingSensitivity.LipSuckBottom;
            unifiedExpressions[(int)UnifiedExpressions.MouthLowerDownLeft].Weight = expressions[(int)FBExpression.Lower_Lip_Depressor_L] * TrackingSensitivity.MouthLowerDown;
            unifiedExpressions[(int)UnifiedExpressions.MouthLowerDownRight].Weight = expressions[(int)FBExpression.Lower_Lip_Depressor_R] * TrackingSensitivity.MouthLowerDown;
            unifiedExpressions[(int)UnifiedExpressions.MouthUpperUpLeft].Weight = expressions[(int)FBExpression.Upper_Lip_Raiser_L] * TrackingSensitivity.MouthUpperUp;
            unifiedExpressions[(int)UnifiedExpressions.MouthUpperUpRight].Weight = expressions[(int)FBExpression.Upper_Lip_Raiser_R] * TrackingSensitivity.MouthUpperUp;

            #endregion

            // Mapping of Quest Pro FACS to VRCFT Unique Shapes

            #region Brow Expressions Set

            unifiedExpressions[(int)UnifiedExpressions.BrowInnerUpLeft].Weight = expressions[(int)FBExpression.Inner_Brow_Raiser_L] * TrackingSensitivity.BrowInnerUp;
            unifiedExpressions[(int)UnifiedExpressions.BrowInnerUpRight].Weight = expressions[(int)FBExpression.Inner_Brow_Raiser_R] * TrackingSensitivity.BrowInnerUp;
            unifiedExpressions[(int)UnifiedExpressions.BrowOuterUpLeft].Weight = expressions[(int)FBExpression.Outer_Brow_Raiser_L] * TrackingSensitivity.BrowOuterUp;
            unifiedExpressions[(int)UnifiedExpressions.BrowOuterUpRight].Weight = expressions[(int)FBExpression.Outer_Brow_Raiser_R] * TrackingSensitivity.BrowOuterUp;
            unifiedExpressions[(int)UnifiedExpressions.BrowOuterDownLeft].Weight = expressions[(int)FBExpression.Brow_Lowerer_L] * TrackingSensitivity.BrowDown;
            unifiedExpressions[(int)UnifiedExpressions.BrowInnerDownLeft].Weight = expressions[(int)FBExpression.Brow_Lowerer_L] * TrackingSensitivity.BrowDown;
            unifiedExpressions[(int)UnifiedExpressions.BrowOuterDownRight].Weight = expressions[(int)FBExpression.Brow_Lowerer_R] * TrackingSensitivity.BrowDown;
            unifiedExpressions[(int)UnifiedExpressions.BrowInnerDownRight].Weight = expressions[(int)FBExpression.Brow_Lowerer_R] * TrackingSensitivity.BrowDown;

            #endregion

            #region Additonal Eye Tracking Expressions Set

            unifiedExpressions[(int)UnifiedExpressions.EyeSquintLeft].Weight = expressions[(int)FBExpression.Lid_Tightener_L] * TrackingSensitivity.EyeSquint;
            unifiedExpressions[(int)UnifiedExpressions.EyeSquintRight].Weight = expressions[(int)FBExpression.Lid_Tightener_R] * TrackingSensitivity.EyeSquint;

            #endregion

            #region Additional Face Tracking Expressions Set  

            unifiedExpressions[(int)UnifiedExpressions.CheekSquintLeft].Weight = expressions[(int)FBExpression.Cheek_Raiser_L] * TrackingSensitivity.CheekRaiser;
            unifiedExpressions[(int)UnifiedExpressions.CheekSquintRight].Weight = expressions[(int)FBExpression.Cheek_Raiser_R] * TrackingSensitivity.CheekRaiser;
            unifiedExpressions[(int)UnifiedExpressions.MouthRaiserUpper].Weight = 1.0f - expressions[(int)FBExpression.Chin_Raiser_T] * TrackingSensitivity.ChinRaiserBottom;
            unifiedExpressions[(int)UnifiedExpressions.MouthRaiserLower].Weight = 1.0f - expressions[(int)FBExpression.Chin_Raiser_B] * TrackingSensitivity.ChinRaiserTop;
            unifiedExpressions[(int)UnifiedExpressions.MouthDimpleLeft].Weight = expressions[(int)FBExpression.Dimpler_L] * TrackingSensitivity.MouthDimpler;
            unifiedExpressions[(int)UnifiedExpressions.MouthDimpleRight].Weight = expressions[(int)FBExpression.Dimpler_R] * TrackingSensitivity.MouthDimpler;
            unifiedExpressions[(int)UnifiedExpressions.LipFunnelBottomLeft].Weight = expressions[(int)FBExpression.Lip_Funneler_LB] * TrackingSensitivity.LipFunnelBottom;
            unifiedExpressions[(int)UnifiedExpressions.LipFunnelBottomRight].Weight = expressions[(int)FBExpression.Lip_Funneler_RB] * TrackingSensitivity.LipFunnelBottom;
            unifiedExpressions[(int)UnifiedExpressions.LipFunnelTopLeft].Weight = expressions[(int)FBExpression.Lip_Funneler_LT] * TrackingSensitivity.LipFunnelTop;
            unifiedExpressions[(int)UnifiedExpressions.LipFunnelTopRight].Weight = expressions[(int)FBExpression.Lip_Funneler_RT] * TrackingSensitivity.LipFunnelTop;
            unifiedExpressions[(int)UnifiedExpressions.MouthPressLeft].Weight = expressions[(int)FBExpression.Lip_Pressor_L] * TrackingSensitivity.MouthPress;  
            unifiedExpressions[(int)UnifiedExpressions.MouthPressRight].Weight = expressions[(int)FBExpression.Lip_Pressor_R] * TrackingSensitivity.MouthPress;
            unifiedExpressions[(int)UnifiedExpressions.LipPuckerLeft].Weight = expressions[(int)FBExpression.Lip_Pucker_L] * TrackingSensitivity.LipPucker;
            unifiedExpressions[(int)UnifiedExpressions.LipPuckerRight].Weight = expressions[(int)FBExpression.Lip_Pucker_R] * TrackingSensitivity.LipPucker;
            unifiedExpressions[(int)UnifiedExpressions.MouthStretchLeft].Weight = expressions[(int)FBExpression.Lip_Stretcher_L] * TrackingSensitivity.MouthStretch;
            unifiedExpressions[(int)UnifiedExpressions.MouthStretchRight].Weight = expressions[(int)FBExpression.Lip_Stretcher_R] * TrackingSensitivity.MouthStretch;
            unifiedExpressions[(int)UnifiedExpressions.LipSuckBottomLeft].Weight = expressions[(int)FBExpression.Lip_Suck_LB] * TrackingSensitivity.LipSuckBottom;
            unifiedExpressions[(int)UnifiedExpressions.LipSuckBottomRight].Weight = expressions[(int)FBExpression.Lip_Suck_RB] * TrackingSensitivity.LipSuckBottom;
            unifiedExpressions[(int)UnifiedExpressions.LipSuckTopRight].Weight = expressions[(int)FBExpression.Lip_Suck_RT] * TrackingSensitivity.LipSuckTop;
            unifiedExpressions[(int)UnifiedExpressions.LipSuckTopLeft].Weight = expressions[(int)FBExpression.Lip_Suck_LT] * TrackingSensitivity.LipSuckTop;
            unifiedExpressions[(int)UnifiedExpressions.MouthTightenerLeft].Weight = expressions[(int)FBExpression.Lip_Tightener_L] * TrackingSensitivity.MouthTightener;
            unifiedExpressions[(int)UnifiedExpressions.MouthTightenerRight].Weight = expressions[(int)FBExpression.Lip_Tightener_R] * TrackingSensitivity.MouthTightener;            
            unifiedExpressions[(int)UnifiedExpressions.NoseSneerLeft].Weight = expressions[(int)FBExpression.Nose_Wrinkler_L] * TrackingSensitivity.NoseSneer;
            unifiedExpressions[(int)UnifiedExpressions.NoseSneerRight].Weight = expressions[(int)FBExpression.Nose_Wrinkler_R] * TrackingSensitivity.NoseSneer;

            //UnifiedTrackingData.LatestLipData.LatestShapes[(int)UnifiedExpression.MouthClosed] = Math.Min(1, (expressions[(int)FBExpression.Lips_Toward]) * TrackingSensitivity.MouthTowards);

            #endregion
        }

        private UnifiedSingleEyeData MakeEye(float LookLeft, float LookRight, float LookUp, float LookDown, float Openness)
        {
            return new UnifiedSingleEyeData()
            {
                Gaze = new Vector2(LookRight - LookLeft, LookUp - LookDown),
                Openness = Openness
            };
        }

        public override void Teardown()
        {

        }
    }
}