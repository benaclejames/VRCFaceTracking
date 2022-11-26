using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCFaceTracking;
using VRCFaceTracking.Assets.UI;
using VRCFaceTracking.Params;

namespace SRanipalExtTrackingInterface
{
    public class SRanipalExtTrackingInterface : ExtTrackingModule
    {
        LipData_v2 lipData = default;
        EyeData_v2 eyeData = default;

        private static CancellationTokenSource _cancellationToken;
        
        public override (bool SupportsEye, bool SupportsExpressions) Supported => (true, true);

        public override (bool eyeSuccess, bool expressionSuccess) Initialize()
        {
            // Look for SRanipal assemblies here.
            Directory.SetCurrentDirectory(Utils.PersistentDataDirectory + "/CustomLibs/ModuleLibs");

            Error eyeError = Error.UNDEFINED, lipError = Error.UNDEFINED;

            // Only try to init if we're actually using the only headset that supports SRanipal eye tracking
            eyeError = SRanipal_API.Initial(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2, IntPtr.Zero);

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
                    
                    UnifiedTracking.AllData.EyeImageData.SupportsImage = true;
                    UnifiedTracking.AllData.EyeImageData.ImageSize = (200, 100);
                }
            }
            
            if (lipEnabled)
            {
                UnifiedTracking.AllData.LipImageData.SupportsImage = true;
                UnifiedTracking.AllData.LipImageData.ImageSize = (SRanipal_Lip_v2.ImageWidth, SRanipal_Lip_v2.ImageHeight);
                UnifiedTracking.AllData.LipImageData.ImageData = new byte[UnifiedTracking.AllData.LipImageData.ImageSize.x *
                                                                       UnifiedTracking.AllData.LipImageData.ImageSize.y];
                lipData.image = Marshal.AllocCoTaskMem(UnifiedTracking.AllData.LipImageData.ImageSize.x *
                                                       UnifiedTracking.AllData.LipImageData.ImageSize.x);
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

            if (Status.ExpressionState > ModuleState.Uninitialized)
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
                    if (Status.ExpressionState == ModuleState.Active && UpdateMouth() != Error.WORK)
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

        #endregion

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

            UpdateEyeParameters(ref UnifiedTracking.AllData.LatestExpressionData.Eye, eyeData.verbose_data);
            UpdateEyeExpressions(ref UnifiedTracking.AllData.LatestExpressionData.Shapes, eyeData.expression_data);

            UnifiedTracking.AllData.UpdateData();
            
            if (!MainWindow.IsEyePageVisible || _processHandle == IntPtr.Zero || !UnifiedTracking.AllData.EyeImageData.SupportsImage) return updateResult;
            
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
            UnifiedTracking.AllData.EyeImageData.ImageData = imageBytes;

            return updateResult;
        }

        private void UpdateEyeParameters(ref UnifiedEyeData data, VerboseData external)
        {
            data.Left.GazeNormalized = external.left.gaze_direction_normalized.FlipXCoordinates();
            data.Right.GazeNormalized = external.right.gaze_direction_normalized.FlipXCoordinates();

            data.Left.Openness = external.left.eye_openness;
            data.Right.Openness = external.right.eye_openness;

            data.Left.PupilDiameter_MM = external.left.pupil_diameter_mm;
            data.Right.PupilDiameter_MM = external.right.pupil_diameter_mm;
        }

        private void UpdateEyeExpressions(ref UnifiedExpressionShape[] data, EyeExpression external)
        {
            data[(int)UnifiedExpressions.EyeWideLeft].weight = external.left.eye_wide;
            data[(int)UnifiedExpressions.EyeWideRight].weight = external.right.eye_wide;

            data[(int)UnifiedExpressions.EyeSquintLeft].weight = external.left.eye_squeeze;
            data[(int)UnifiedExpressions.EyeSquintRight].weight = external.right.eye_squeeze;

            // Emulator expressions for Unified Expressions. These are essentially already baked into Legacy eye expressions (SRanipal)

            float browOuterOffset = 0.66f;

            data[(int)UnifiedExpressions.BrowInnerUpLeft].weight = external.left.eye_wide;
            data[(int)UnifiedExpressions.BrowOuterUpLeft].weight = external.left.eye_wide;

            data[(int)UnifiedExpressions.BrowInnerUpRight].weight = external.right.eye_wide;
            data[(int)UnifiedExpressions.BrowOuterUpRight].weight = external.right.eye_wide;

            data[(int)UnifiedExpressions.BrowInnerDownLeft].weight = external.left.eye_squeeze;
            data[(int)UnifiedExpressions.BrowOuterDownLeft].weight = external.left.eye_squeeze * browOuterOffset;

            data[(int)UnifiedExpressions.BrowInnerDownRight].weight = external.right.eye_squeeze;
            data[(int)UnifiedExpressions.BrowOuterDownRight].weight = external.right.eye_squeeze * browOuterOffset;
        }

        private Error UpdateMouth()
        {
            var updateResult = SRanipal_Lip_API.GetLipData_v2(ref lipData);

            UpdateMouthExpressions(ref UnifiedTracking.AllData.LatestExpressionData, lipData.prediction_data);

            UnifiedTracking.AllData.UpdateData();
            
            if (!MainWindow.IsLipPageVisible || lipData.image == IntPtr.Zero || !UnifiedTracking.AllData.LipImageData.SupportsImage) return updateResult;
            
            Marshal.Copy(lipData.image, UnifiedTracking.AllData.LipImageData.ImageData, 0, UnifiedTracking.AllData.LipImageData.ImageSize.x *
                UnifiedTracking.AllData.LipImageData.ImageSize.y);

            return updateResult;
        }

        private void UpdateMouthExpressions(ref UnifiedExpressionsData data, PredictionData_v2 external)
        {
            unsafe
            {
                #region Direct Jaw

                data.Shapes[(int)UnifiedExpressions.JawOpen].weight = external.blend_shape_weight[(int)LipShape_v2.JawOpen] - external.blend_shape_weight[(int)LipShape_v2.MouthApeShape];
                data.Shapes[(int)UnifiedExpressions.JawLeft].weight = external.blend_shape_weight[(int)LipShape_v2.JawLeft];
                data.Shapes[(int)UnifiedExpressions.JawRight].weight = external.blend_shape_weight[(int)LipShape_v2.JawRight];
                data.Shapes[(int)UnifiedExpressions.JawForward].weight = external.blend_shape_weight[(int)LipShape_v2.JawForward];
                data.Shapes[(int)UnifiedExpressions.MouthClosed].weight = external.blend_shape_weight[(int)LipShape_v2.MouthApeShape];

                #endregion

                #region Direct Mouth and Lip

                // These shapes have overturns subtracting from them, as we are expecting the new standard to have Upper Up / Lower Down baked into the funneller shapes below these.
                data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft].weight = external.blend_shape_weight[(int)LipShape_v2.MouthUpperUpLeft] - external.blend_shape_weight[(int)LipShape_v2.MouthUpperOverturn];
                data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight].weight = external.blend_shape_weight[(int)LipShape_v2.MouthUpperUpRight] - external.blend_shape_weight[(int)LipShape_v2.MouthUpperOverturn];
                data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft].weight = external.blend_shape_weight[(int)LipShape_v2.MouthLowerDownLeft] - external.blend_shape_weight[(int)LipShape_v2.MouthLowerOverturn];
                data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight].weight = external.blend_shape_weight[(int)LipShape_v2.MouthLowerDownRight] - external.blend_shape_weight[(int)LipShape_v2.MouthLowerOverturn];

                data.Shapes[(int)UnifiedExpressions.LipPuckerLeft].weight = external.blend_shape_weight[(int)LipShape_v2.MouthPout];
                data.Shapes[(int)UnifiedExpressions.LipPuckerRight].weight = external.blend_shape_weight[(int)LipShape_v2.MouthPout];

                data.Shapes[(int)UnifiedExpressions.LipFunnelTopLeft].weight = external.blend_shape_weight[(int)LipShape_v2.MouthUpperOverturn];
                data.Shapes[(int)UnifiedExpressions.LipFunnelTopRight].weight = external.blend_shape_weight[(int)LipShape_v2.MouthUpperOverturn];
                data.Shapes[(int)UnifiedExpressions.LipFunnelBottomLeft].weight = external.blend_shape_weight[(int)LipShape_v2.MouthUpperOverturn];
                data.Shapes[(int)UnifiedExpressions.LipFunnelBottomRight].weight = external.blend_shape_weight[(int)LipShape_v2.MouthUpperOverturn];

                data.Shapes[(int)UnifiedExpressions.LipSuckTopLeft].weight = external.blend_shape_weight[(int)LipShape_v2.MouthUpperInside];
                data.Shapes[(int)UnifiedExpressions.LipSuckTopRight].weight = external.blend_shape_weight[(int)LipShape_v2.MouthUpperInside];
                data.Shapes[(int)UnifiedExpressions.LipSuckBottomLeft].weight = external.blend_shape_weight[(int)LipShape_v2.MouthLowerInside];
                data.Shapes[(int)UnifiedExpressions.LipSuckBottomRight].weight = external.blend_shape_weight[(int)LipShape_v2.MouthLowerInside];

                data.Shapes[(int)UnifiedExpressions.MouthTopLeft].weight = external.blend_shape_weight[(int)LipShape_v2.MouthUpperLeft];
                data.Shapes[(int)UnifiedExpressions.MouthTopRight].weight = external.blend_shape_weight[(int)LipShape_v2.MouthUpperRight];
                data.Shapes[(int)UnifiedExpressions.MouthBottomLeft].weight = external.blend_shape_weight[(int)LipShape_v2.MouthLowerLeft];
                data.Shapes[(int)UnifiedExpressions.MouthBottomRight].weight = external.blend_shape_weight[(int)LipShape_v2.MouthLowerRight];

                data.Shapes[(int)UnifiedExpressions.MouthSmileLeft].weight = external.blend_shape_weight[(int)LipShape_v2.MouthSmileLeft];
                data.Shapes[(int)UnifiedExpressions.MouthSmileRight].weight = external.blend_shape_weight[(int)LipShape_v2.MouthSmileRight];
                data.Shapes[(int)UnifiedExpressions.MouthFrownLeft].weight = external.blend_shape_weight[(int)LipShape_v2.MouthSadLeft];
                data.Shapes[(int)UnifiedExpressions.MouthFrownRight].weight = external.blend_shape_weight[(int)LipShape_v2.MouthSadRight];

                data.Shapes[(int)UnifiedExpressions.MouthRaiserUpper].weight = external.blend_shape_weight[(int)LipShape_v2.MouthLowerOverlay] - external.blend_shape_weight[(int)LipShape_v2.MouthUpperInside];
                data.Shapes[(int)UnifiedExpressions.MouthRaiserLower].weight = external.blend_shape_weight[(int)LipShape_v2.MouthLowerOverlay];

                #endregion

                #region Direct Cheek

                data.Shapes[(int)UnifiedExpressions.CheekPuffLeft].weight = external.blend_shape_weight[(int)LipShape_v2.CheekPuffLeft];
                data.Shapes[(int)UnifiedExpressions.CheekPuffRight].weight = external.blend_shape_weight[(int)LipShape_v2.CheekPuffRight];

                data.Shapes[(int)UnifiedExpressions.CheekSuckLeft].weight = external.blend_shape_weight[(int)LipShape_v2.CheekSuck];
                data.Shapes[(int)UnifiedExpressions.CheekSuckRight].weight = external.blend_shape_weight[(int)LipShape_v2.CheekSuck];

                #endregion

                #region Direct Tongue

                data.Shapes[(int)UnifiedExpressions.TongueOut].weight = (external.blend_shape_weight[(int)LipShape_v2.TongueLongStep1] + external.blend_shape_weight[(int)LipShape_v2.TongueLongStep2]) / 2.0f;
                data.Shapes[(int)UnifiedExpressions.TongueUp].weight = external.blend_shape_weight[(int)LipShape_v2.TongueUp];
                data.Shapes[(int)UnifiedExpressions.TongueDown].weight = external.blend_shape_weight[(int)LipShape_v2.TongueDown];
                data.Shapes[(int)UnifiedExpressions.TongueLeft].weight = external.blend_shape_weight[(int)LipShape_v2.TongueLeft];
                data.Shapes[(int)UnifiedExpressions.TongueRight].weight = external.blend_shape_weight[(int)LipShape_v2.TongueRight];
                data.Shapes[(int)UnifiedExpressions.TongueRoll].weight = external.blend_shape_weight[(int)LipShape_v2.TongueRoll];

                #endregion

                // These shapes are not tracked at all by SRanipal, but instead are being treated as enhancements to driving the shapes above.

                #region Emulated Unified Mapping

                data.Shapes[(int)UnifiedExpressions.CheekSquintLeft].weight = external.blend_shape_weight[(int)LipShape_v2.MouthSmileLeft];
                data.Shapes[(int)UnifiedExpressions.CheekSquintRight].weight = external.blend_shape_weight[(int)LipShape_v2.MouthSmileRight];

                data.Shapes[(int)UnifiedExpressions.MouthDimpleLeft].weight = external.blend_shape_weight[(int)LipShape_v2.MouthSmileLeft];
                data.Shapes[(int)UnifiedExpressions.MouthDimpleRight].weight = external.blend_shape_weight[(int)LipShape_v2.MouthSmileRight];

                data.Shapes[(int)UnifiedExpressions.MouthStretchLeft].weight = external.blend_shape_weight[(int)LipShape_v2.MouthSadRight];
                data.Shapes[(int)UnifiedExpressions.MouthStretchRight].weight = external.blend_shape_weight[(int)LipShape_v2.MouthSadRight];

                #endregion
            }
        }
    }
}