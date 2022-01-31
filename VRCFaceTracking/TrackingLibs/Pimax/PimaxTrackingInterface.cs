using System;
using System.Threading;
using VRCFaceTracking.Pimax;

namespace VRCFaceTracking.TrackingLibs.Pimax
{ 
    public class PimaxTrackingInterface //: ITrackingModule
    {
        private static readonly Ai1EyeData PimaxEyeData = new Ai1EyeData();
        private static readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();
        
        private static bool _needsUpdate;
        
        public bool SupportsEye => true;
        public bool SupportsLip => false;

        public (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip)
        {
            PimaxTracker.RegisterCallback(CallbackType.Update, () => _needsUpdate = true);

            var success = PimaxTracker.Start();
            return (success, false);
        }

        public Action GetUpdateThreadFunc()
        {
            return () =>
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    if (_needsUpdate)
                        Update();

                    Thread.Sleep(10);
                }
            };
        }

        public void Teardown()
        {
            CancellationToken.Cancel();
            PimaxTracker.Stop();
        }

        public void Update()
        {
            if (!UnifiedLibManager.EyeEnabled) return;
            
            PimaxEyeData.Update();
            UnifiedTrackingData.LatestEyeData.UpdateData(PimaxEyeData);
        }
    }
}