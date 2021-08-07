using System;
using System.Threading;
using MelonLoader;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;

namespace VRCFaceTracking.SRanipal
{
    public class SRanipalTrackingInterface : ITrackingModule
    {
        public static float MaxDilation;
        public static float MinDilation = 999;

        private static readonly Thread SRanipalWorker = new Thread(() => Update(CancellationToken.Token));
        
        private static readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();

        public bool SupportsEye => true;
        public bool SupportsLip => true;

        public (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip)
        {
            Error eyeError = Error.UNDEFINED, lipError = Error.UNDEFINED;

            if (eye)
                eyeError = SRanipal_API.Initial(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2, IntPtr.Zero);

            if (lip)
                lipError = SRanipal_API.Initial(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2, IntPtr.Zero);

            var (eyeEnabled, lipEnabled) = HandleSrErrors(eyeError, lipError);
            
            if ((eyeEnabled || lipEnabled) && !SRanipalWorker.IsAlive) SRanipalWorker.Start();
            
            return (eyeEnabled, lipEnabled);
        }

        private static (bool eyeSuccess, bool lipSuccess) HandleSrErrors(Error eyeError, Error lipError)
        {
            bool eyeEnabled = false, lipEnabled = false;
            
            if (eyeError == Error.WORK)
                eyeEnabled = true;

            if (lipError == Error.FOXIP_SO)
                while (lipError == Error.FOXIP_SO)
                    lipError = SRanipal_API.Initial(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2, IntPtr.Zero);
            
            if (lipError == Error.WORK)
                lipEnabled = true;

            return (eyeEnabled, lipEnabled);
        }
        
        public void Teardown()
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
                    if (UnifiedLibManager.LipEnabled) UpdateMouth();
                    if (UnifiedLibManager.EyeEnabled) UpdateEye();
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

        private static EyeData_v2 _eyeData;
        private static void UpdateEye()
        {
            SRanipal_Eye_API.GetEyeData_v2(ref _eyeData);
            UnifiedTrackingData.LatestEyeData.UpdateData(_eyeData);;
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

        #endregion

        public static void ResetTrackingThresholds()
        {
            MinDilation = 999;
            MaxDilation = 0;
        }
    }
}