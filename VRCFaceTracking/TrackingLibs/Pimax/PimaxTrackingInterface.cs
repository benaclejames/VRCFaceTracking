using System;
using System.Threading;
using VRCFaceTracking.Pimax;

namespace VRCFaceTracking.TrackingLibs.Pimax
{ 
    public class PimaxTrackingInterface : ExtTrackingModule
    {
        private static readonly Ai1EyeData PimaxEyeData = new Ai1EyeData();
        private static readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();
        
        private static bool _needsUpdate;
        
        public override (bool SupportsEye, bool SupportsLip) Supported => (true, false);
        public override (bool UtilizingEye, bool UtilizingLip) Utilizing { get; set; }

        public override (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip)
        {
            PimaxTracker.RegisterCallback(CallbackType.Update, () => _needsUpdate = true);

            var success = PimaxTracker.Start();
            return (success, false);
        }

        public override Action GetUpdateThreadFunc()
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

        public override void Teardown()
        {
            CancellationToken.Cancel();
            PimaxTracker.Stop();
        }

        public override void Update()
        {
            if (!Utilizing.UtilizingEye) return;
            
            PimaxEyeData.Update();
            UnifiedTrackingData.LatestEyeData.UpdateData(PimaxEyeData);
        }
    }
}