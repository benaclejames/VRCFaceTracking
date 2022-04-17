using System;
using System.Diagnostics;
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
        private static CancellationTokenSource _cancellationToken;

        public override (bool SupportsEye, bool SupportsLip) Supported => (true, true);
        public override (bool UtilizingEye, bool UtilizingLip) Utilizing { get; set; }

        public override (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip)
        {
            _cancellationToken?.Cancel();
            
            Error eyeError = Error.UNDEFINED, lipError = Error.UNDEFINED;

            if (eye && SRanipal_Eye_API.IsViveProEye())
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
                            offset = module.BaseAddress + (_process.MainModule.FileVersionInfo.FileVersion == "1.3.2.0" ? 0x19190 : 0x19100);
                    
                    UnifiedTrackingData.LatestEyeData.SupportsImage = true;
                    UnifiedTrackingData.LatestEyeData.ImageSize = (200, 100);
                }
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
            
            if (UnifiedLibManager.EyeEnabled) SRanipal_API.Release(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2);
            if (UnifiedLibManager.LipEnabled) SRanipal_API.Release(SRanipal_Lip_v2.ANIPAL_TYPE_LIP_V2);
            
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
                    Update();
                    //Thread.Sleep(10);
                }
            };
        }

        public override void Update()
        {
            if (Utilizing.UtilizingLip)
                UpdateMouth();
            
            if (Utilizing.UtilizingEye)
                UpdateEye();
        }
        
        private const int PROCESS_VM_OPERATION = 0x0008;
        private const int PROCESS_VM_READ = 0x0010;
        private const int PROCESS_VM_WRITE = 0x0020;
        private IntPtr offset;

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(int hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
        
        private static Process _process;
        private static IntPtr _processHandle;

        private static bool Attach()
        {
            if (Process.GetProcessesByName("sr_runtime").Length <= 0) return false;
            _process = Process.GetProcessesByName("sr_runtime")[0];
            _processHandle =
                OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE,
                    false, _process.Id);
            return true;
        }

        private static byte[] ReadMemory(IntPtr offset, int size) {
            var buffer = new byte[size];

            var bytesRead = 0;
            ReadProcessMemory((int) _processHandle, offset, buffer, size, ref bytesRead);

            return bytesRead != size ? null : buffer;
        }
        
        private void UpdateEye()
        {
            EyeData_v2 eyeData = default;
            SRanipal_Eye_API.GetEyeData_v2(ref eyeData);
            UnifiedTrackingData.LatestEyeData.UpdateData(eyeData);
        }

        private void UpdateMouth() => SRanipal_Lip_v2.GetLipWeightingsAndImage(out UnifiedTrackingData.LatestLipShapes, out UnifiedTrackingData.Image);

        #endregion
    }
}