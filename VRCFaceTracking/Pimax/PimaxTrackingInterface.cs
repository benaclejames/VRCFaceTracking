using System;
using System.Threading;
using MelonLoader;

namespace VRCFaceTracking.Pimax
{
    public class PimaxTrackingInterface
    {
        public static readonly Thread PimaxWorker = new Thread(() => Update(CancellationToken.Token));
        private static readonly CancellationTokenSource CancellationToken = new CancellationTokenSource();
        
        private static bool _needsUpdate;
        
        public static bool Initialize()
        {
            PimaxTracker.RegisterCallback(CallbackType.Update, () => _needsUpdate = true);
            return PimaxTracker.Start();
        }

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