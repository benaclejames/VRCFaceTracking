using System;
using System.Threading;
using MelonLoader;

namespace VRCFaceTracking.Pimax
{ 
    public class PimaxTrackingInterface : ITrackingModule
    {
        private static readonly Thread PimaxWorker = new Thread(() => Update(CancellationToken.Token));
        private static readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();
        
        private static bool _needsUpdate;


        public bool SupportsEye => true;
        public bool SupportsLip => false;

        public (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip)
        {
            PimaxTracker.RegisterCallback(CallbackType.Update, () => _needsUpdate = true);

            var success = PimaxTracker.Start();
            if (success && !PimaxWorker.IsAlive) PimaxWorker.Start();
            return (success, false);
        }

        public void Teardown() => PimaxTracker.Stop();

        private static void Update(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_needsUpdate)
                {
                    try
                    {
                        if (UnifiedLibManager.EyeEnabled) UnifiedTrackingData.LatestEyeData = new Ai1EyeData();
                    }
                    catch (Exception e)
                    {
                        if (e.InnerException.GetType() != typeof(ThreadAbortException))
                            MelonLogger.Error("Threading error occured in PimaxTrackingInterface Update: " + e + ": " +
                                              e.InnerException);
                    }
                }

                Thread.Sleep(10);
            }
        }
    }
}