using System;
using System.Threading;
using MelonLoader;

namespace VRCFaceTracking.Pimax
{ 
    public class PimaxTrackingInterface : ITrackingModule
    {
        private static Thread PimaxWorker;
        private static readonly Ai1EyeData PimaxEyeData = new Ai1EyeData();
        private static readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();
        
        private static bool _needsUpdate;
        
        public bool SupportsEye => true;
        public bool SupportsLip => false;

        public (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip)
        {
            MelonLogger.Msg("Init Pimax");
            PimaxTracker.RegisterCallback(CallbackType.Update, () => _needsUpdate = true);

            var success = PimaxTracker.Start();
            if (success && !PimaxWorker.IsAlive) PimaxWorker.Start();
            return (success, false);
        }

        public void Teardown() => PimaxTracker.Stop();

        public void Update(bool threaded = false)
        {
            if (!threaded && UnifiedLibManager.EyeEnabled)
            {
                PimaxEyeData.Update();
                UnifiedTrackingData.LatestEyeData.UpdateData(PimaxEyeData);
            }
            else
            {
                PimaxWorker = new Thread(() =>
                {
                    while (!CancellationToken.IsCancellationRequested)
                    {
                        if (_needsUpdate)
                            Update();
                        
                        Thread.Sleep(10);
                    }
                });
                PimaxWorker.Start();
            }
        }
    }
}