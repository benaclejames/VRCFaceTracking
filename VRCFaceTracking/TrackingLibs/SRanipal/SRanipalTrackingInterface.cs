using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;

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

            if (Status.EyeState > ModuleState.Uninitialized)
            {
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
                // Same for lips
                var killThread = new Thread(() => SRanipal_API.Release(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2));
                killThread.Start();
                if (!killThread.Join(new TimeSpan(0,0,5)))
                    killThread.Abort();
            }
            
            _cancellationToken.Dispose();
        }

        #region Update

        
        public override Action GetUpdateThreadFunc()
        {
            _cancellationToken = new CancellationTokenSource();
            return () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    if (Status.LipState == ModuleState.Active)
                        UpdateMouth();
            
                    if (Status.EyeState == ModuleState.Active)
                        UpdateEye();
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
        
        private void UpdateEye()
        {
            SRanipal_Eye_API.GetEyeData_v2(ref eyeData);
            UnifiedTrackingData.LatestEyeData.UpdateData(eyeData);
            
            if (_processHandle == IntPtr.Zero || !UnifiedTrackingData.LatestEyeData.SupportsImage) return;
            
            // Read 20000 image bytes from the predefined offset. 10000 bytes per eye.
            var imageBytes = ReadMemory(_offset, 20000);
            
            // Concatenate the two images side by side instead of one after the other
            var leftEye = imageBytes.Take(10000).ToList();
            var rightEye = imageBytes.Skip(10000).ToList();
            var concatImage = new List<byte>();
            for (var i = 0; i < 100; i++)
            {
                concatImage.AddRange(leftEye.Take(100));
                concatImage.AddRange(rightEye.Take(100));
                leftEye.RemoveRange(0, 100);
                rightEye.RemoveRange(0, 100);
            }

            // Write the image to the latest eye data
            UnifiedTrackingData.LatestEyeData.ImageData = concatImage.ToArray();
        }

        private void UpdateMouth()
        {
            SRanipal_Lip_API.GetLipData_v2(ref lipData);
            UnifiedTrackingData.LatestLipData.UpdateData(lipData);
            
            if (lipData.image == IntPtr.Zero || !UnifiedTrackingData.LatestLipData.SupportsImage) return;
            
            Marshal.Copy(lipData.image, UnifiedTrackingData.LatestLipData.ImageData, 0, UnifiedTrackingData.LatestLipData.ImageSize.x *
                UnifiedTrackingData.LatestLipData.ImageSize.y);
        }

        #endregion
    }
}