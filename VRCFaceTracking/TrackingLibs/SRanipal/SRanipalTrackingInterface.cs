using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCFaceTracking.Assets.UI;

namespace VRCFaceTracking.SRanipal
{
    public class SRanipalExtTrackingInterface : ExtTrackingModule
    {
        LipData_v2 lipData = default;
        EyeData_v2 eyeData = default;
        private static CancellationTokenSource _cancellationToken;
        
        public override (bool SupportsEye, bool SupportsLip) Supported => (true, true);

        public override (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip)
        {
            Error eyeError = Error.UNDEFINED, lipError = Error.UNDEFINED;

            if (eye)
                // Only try to init if we're actually using the only headset that supports SRanipal eye tracking
                eyeError = SRanipal_API.Initial(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2, IntPtr.Zero);
            
            if (lip)
                lipError = SRanipal_API.Initial(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2, IntPtr.Zero);

            var (eyeEnabled, lipEnabled) = HandleSrErrors(eyeError, lipError);

            if (eyeEnabled && Utils.HasAdmin)
            {
                var found = false;
                int tries = 0;
                while (!found && tries < 15)
                {
                    tries++;
                    found = Attach();
                    Thread.Sleep(250);
                }

                if (found)
                {
                    // Find the EyeCameraDevice.dll module inside sr_runtime, get it's offset and add hex 19190 to it for the image stream.
                    foreach (ProcessModule module in _process.Modules)
                        if (module.ModuleName == "EyeCameraDevice.dll")
                            _offset = module.BaseAddress + (_process.MainModule.FileVersionInfo.FileVersion == "1.3.2.0" ? 0x19190 : 0x19100);
                    
                    UnifiedTrackingData.LatestEyeData.SupportsImage = true;
                    UnifiedTrackingData.LatestEyeData.ImageSize = (200, 100);
                }
            }
            
            if (lipEnabled)
            {
                UnifiedTrackingData.LatestLipData.SupportsImage = true;
                UnifiedTrackingData.LatestLipData.ImageSize = (SRanipal_Lip_v2.ImageWidth, SRanipal_Lip_v2.ImageHeight);
                UnifiedTrackingData.LatestLipData.ImageData = new byte[UnifiedTrackingData.LatestLipData.ImageSize.x *
                                                                       UnifiedTrackingData.LatestLipData.ImageSize.y];
                lipData.image = Marshal.AllocCoTaskMem(UnifiedTrackingData.LatestLipData.ImageSize.x *
                                                       UnifiedTrackingData.LatestLipData.ImageSize.y);
            }

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
        
        public override void Teardown()
        {
            _cancellationToken.Cancel();
            _cancellationToken.Dispose();
            
            Thread.Sleep(2000);

            if (Status.EyeState > ModuleState.Uninitialized)
            {
                Logger.Msg("Teardown: Releasing Eye");
                // Attempt to release this module and give up after 10 seconds because Vive Moment
                var killThread = new Thread(() => SRanipal_API.Release(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2));
                killThread.Start();
                if (!killThread.Join(new TimeSpan(0, 0, 5)))
                {
                    killThread.Abort();
                    if (killThread.IsAlive)
                        killThread.Interrupt();
                }
            }

            if (Status.LipState > ModuleState.Uninitialized)
            {
                Logger.Msg("Teardown: Releasing Lip");
                // Same for lips
                var killThread = new Thread(() => SRanipal_API.Release(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2));
                killThread.Start();
                if (!killThread.Join(new TimeSpan(0,0,5)))
                    killThread.Abort();
            }
        }

        #region Update

        
        public override Action GetUpdateThreadFunc()
        {
            _cancellationToken = new CancellationTokenSource();
            return () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    if (Status.LipState == ModuleState.Active && UpdateMouth() != Error.WORK)
                    {
                        Logger.Msg("An error occured while getting lip data. This might be a wireless crash.");
                        Logger.Msg("Waiting 30 seconds before reinitializing to account for wireless users.");
                        Thread.Sleep(30000);
                        UnifiedLibManager.Initialize();
                        return;
                    }

                    if (Status.EyeState == ModuleState.Active && UpdateEye() != Error.WORK)
                    {
                        Logger.Msg("An error occured while getting eye data. This might be a wireless crash.");
                        Logger.Msg("Waiting 30 seconds before reinitializing to account for wireless users.");
                        Thread.Sleep(30000);
                        UnifiedLibManager.Initialize();
                        return;
                    }
                    
                    Thread.Sleep(10);
                }
            };
        }

        private static Process _process;
        private static IntPtr _processHandle;
        private IntPtr _offset;

        private static bool Attach()
        {
            if (Process.GetProcessesByName("sr_runtime").Length <= 0) return false;
            _process = Process.GetProcessesByName("sr_runtime")[0];
            _processHandle =
                Utils.OpenProcess(Utils.PROCESS_VM_READ,
                    false, _process.Id);
            return true;
        }

        private static byte[] ReadMemory(IntPtr offset, int size) {
            var buffer = new byte[size];

            var bytesRead = 0;
            Utils.ReadProcessMemory((int) _processHandle, offset, buffer, size, ref bytesRead);

            return bytesRead != size ? null : buffer;
        }
        
        private Error UpdateEye()
        {
            var updateResult = SRanipal_Eye_API.GetEyeData_v2(ref eyeData);
            UnifiedTrackingData.LatestEyeData.UpdateData(eyeData);
            
            if (!MainWindow.IsEyePageVisible || _processHandle == IntPtr.Zero || !UnifiedTrackingData.LatestEyeData.SupportsImage) return updateResult;
            
            // Read 20000 image bytes from the predefined offset. 10000 bytes per eye.
            var imageBytes = ReadMemory(_offset, 20000);
            
            // Concatenate the two images side by side instead of one after the other
            byte[] leftEye = new byte[10000];
            Array.Copy(imageBytes, 0, leftEye, 0, 10000);
            byte[] rightEye = new byte[10000];
            Array.Copy(imageBytes, 10000, rightEye, 0, 10000);
            
            for (var i = 0; i < 100; i++)   // 100 lines of 200 bytes
            {
                // Add 100 bytes from the left eye to the left side of the image
                int leftIndex = i * 100 * 2;
                Array.Copy(leftEye,i*100, imageBytes, leftIndex, 100);

                // Add 100 bytes from the right eye to the right side of the image
                Array.Copy(rightEye, i*100, imageBytes, leftIndex + 100, 100);
            }

            // Write the image to the latest eye data
            UnifiedTrackingData.LatestEyeData.ImageData = imageBytes;

            return updateResult;
        }

        private Error UpdateMouth()
        {
            var updateResult = SRanipal_Lip_API.GetLipData_v2(ref lipData);
            UnifiedTrackingData.LatestLipData.UpdateData(lipData);
            
            if (!MainWindow.IsLipPageVisible || lipData.image == IntPtr.Zero || !UnifiedTrackingData.LatestLipData.SupportsImage) return updateResult;
            
            Marshal.Copy(lipData.image, UnifiedTrackingData.LatestLipData.ImageData, 0, UnifiedTrackingData.LatestLipData.ImageSize.x *
                UnifiedTrackingData.LatestLipData.ImageSize.y);

            return updateResult;
        }

        #endregion
    }
}