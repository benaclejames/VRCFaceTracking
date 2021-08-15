using System;
using System.Threading;
using UnhollowerBaseLib;

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
            PimaxTracker.RegisterCallback(CallbackType.Update, () => _needsUpdate = true);

            var success = PimaxTracker.Start();
            if (success && !PimaxWorker.IsAlive) PimaxWorker.Start();
            return (success, false);
        }

        public void StartThread()
        {
            PimaxWorker = new Thread(() =>
            {
                IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
                while (!CancellationToken.IsCancellationRequested)
                {
                    if (_needsUpdate)
                        Update();
                        
                    Thread.Sleep(10);
                }
            });
            PimaxWorker.Start();
        }

        public void Teardown() => PimaxTracker.Stop();

        public void Update()
        {
            if (!UnifiedLibManager.EyeEnabled) return;
            
            PimaxEyeData.Update();
            UnifiedTrackingData.LatestEyeData.UpdateData(PimaxEyeData);
        }
    }
}