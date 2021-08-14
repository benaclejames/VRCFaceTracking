using System;
using System.Threading;
using UnhollowerBaseLib;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;

namespace VRCFaceTracking.SRanipal
{
    public class SRanipalTrackingInterface : ITrackingModule
    {
        private Thread _updateThread;
        private static readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();

        public bool SupportsEye => true;
        public bool SupportsLip => true;

        public (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip)
        {
            Error eyeError = Error.UNDEFINED, lipError = Error.UNDEFINED;

            if (eye && SRanipal_Eye_API.IsViveProEye()) // Only try to init if we're actually using the only headset that supports SRanipal eye tracking
                eyeError = SRanipal_API.Initial(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2, IntPtr.Zero);

            if (lip)
                lipError = SRanipal_API.Initial(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2, IntPtr.Zero);

            var (eyeEnabled, lipEnabled) = HandleSrErrors(eyeError, lipError);
            
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

        public void Update(bool threaded = false)
        {
            if (!threaded)
            {
                if (UnifiedLibManager.EyeEnabled) UpdateEye();
                if (UnifiedLibManager.LipEnabled) UpdateMouth();
            }
            else
            {
                _updateThread = new Thread(() =>
                {
                    IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
                    while (!CancellationToken.IsCancellationRequested)
                    {
                        Update();
                        Thread.Sleep(10);
                    }
                });
                _updateThread.Start();
            }
        }
        
        #region EyeUpdate

        private void UpdateEye()
        {
            EyeData_v2 eyeData = default;
            SRanipal_Eye_API.GetEyeData_v2(ref eyeData);
            UnifiedTrackingData.LatestEyeData.UpdateData(eyeData);
        }

        #endregion

        #region MouthUpdate

        private void UpdateMouth()
        {
            LipData_v2 lipData = default;

            SRanipal_Lip_API.GetLipData_v2(ref lipData);
            SRanipal_Lip_v2.GetLipWeightings(out var lipWeightings);

            UnifiedTrackingData.LatestLipData = lipData;
            UnifiedTrackingData.LatestLipShapes = lipWeightings;
        }

        #endregion
    }
}