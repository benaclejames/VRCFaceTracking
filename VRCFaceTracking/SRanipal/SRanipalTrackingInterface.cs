using System;
using System.Threading;
using MelonLoader;
using UnityEngine;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCFaceTracking.Params.LipMerging;

namespace VRCFaceTracking.SRanipal
{
    public static class SRanipalTrackingInterface
    {
        public static float MaxDilation;
        public static float MinDilation = 999;
        
        public static readonly Thread SRanipalWorker = new Thread(() => Update(CancellationToken.Token));
        
        private static readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();

        public static bool IsRealError(this Error error) => error != Error.WORK && error != Error.UNDEFINED && error != Error.FOXIP_SO;

        public static (Error eyeError, Error lipError) Initialize(bool eye = true, bool lip = true)
        {
            MelonLogger.Msg($"Initializing VRCFaceTracking...");

            Error eyeError = Error.UNDEFINED, lipError = Error.UNDEFINED;

            if (eye)
            {
                if (UnifiedLibManager.EyeEnabled)
                {
                    MelonLogger.Msg("Releasing previously initialized eye module...");
                    SRanipal_API.Release(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2);
                    Thread.Sleep(5000); // Give SRanipal a chance to finish restarting
                }

                eyeError = SRanipal_API.Initial(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2, IntPtr.Zero);
            }

            if (lip)
            {
                if (UnifiedLibManager.LipEnabled)
                {
                    MelonLogger.Msg("Releasing previously initialized lip module...");
                    SRanipal_API.Release(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2);
                    Thread.Sleep(5000); // Give SRanipal a chance to finish restarting
                }
                
                lipError = SRanipal_API.Initial(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2, IntPtr.Zero);
            }

            return (eyeError, lipError);
        }

        public static void Stop()
        {
            CancellationToken.Cancel();
            
            if (UnifiedLibManager.EyeEnabled) SRanipal_API.Release(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2);
            if (UnifiedLibManager.LipEnabled) SRanipal_API.Release(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2);
            
            CancellationToken.Dispose();
        }

        private static void Update(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (UnifiedLibManager.EyeEnabled) UpdateEye();
                    if (UnifiedLibManager.LipEnabled) UpdateMouth();
                }
                catch (Exception e)
                {
                    if (e.InnerException.GetType() != typeof(ThreadAbortException))
                        MelonLogger.Error("Threading error occured in SRanipalTrackingInterface Update: "+e+": "+e.InnerException);
                }
                Thread.Sleep(10);
            }
        }
        
        #region EyeUpdate

        private static void UpdateEye()
        {
            EyeData_v2 eyeData = default;
            
            SRanipal_Eye_API.GetEyeData_v2(ref eyeData);
            
            UnifiedTrackingData.LatestEyeData = eyeData;
        }

        public static void UpdateMinMaxDilation(float readDilation)
        {
            if (readDilation > MaxDilation)
                MaxDilation = readDilation;
            if (readDilation < MinDilation)
                MinDilation = readDilation;
        }
        
        #endregion

        #region MouthUpdate

        private static void UpdateMouth()
        {
            LipData_v2 lipData = default;

            SRanipal_Lip_API.GetLipData_v2(ref lipData);
            SRanipal_Lip_v2.GetLipWeightings(out var lipWeightings);

            UnifiedTrackingData.LatestLipData = lipData;
            UnifiedTrackingData.LatestLipShapes = lipWeightings;
        }

        public static Texture2D UpdateLipTexture()
        {
            var lipTexture = new Texture2D(800, 400, TextureFormat.Alpha8, false);
            return SRanipal_Lip_v2.GetLipImage(ref lipTexture) ? lipTexture : null;
        }

        #endregion

        public static void ResetTrackingThresholds()
        {
            MinDilation = 999;
            MaxDilation = 0;
            
            LipShapeMerger.ResetLipShapeMinMaxThresholds();
        }
    }
}