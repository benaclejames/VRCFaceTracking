using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using ViveSR;
using ViveSR.anipal;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using VRCFaceTracking.Assets.UI;
using VRCFaceTracking.Params;

namespace VRCFaceTracking.SRanipal
{
    public class SRanipalExtTrackingInterface : ExtTrackingModule
    {
        LipData_v2 lipData = default;
        EyeData_v2 eyeData = default;

        private static CancellationTokenSource _cancellationToken;
        
        public override (bool SupportsEye, bool SupportsExpressions) Supported => (true, true);

        public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eye, bool lip)
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
                    
                    UnifiedTrackingData.LatestExpressionData.EyeImageData.SupportsImage = true;
                    UnifiedTrackingData.LatestExpressionData.EyeImageData.ImageSize = (200, 100);
                }
            }
            
            if (lipEnabled)
            {
                UnifiedTrackingData.LatestExpressionData.LipImageData.SupportsImage = true;
                UnifiedTrackingData.LatestExpressionData.LipImageData.ImageSize = (SRanipal_Lip_v2.ImageWidth, SRanipal_Lip_v2.ImageHeight);
                UnifiedTrackingData.LatestExpressionData.LipImageData.ImageData = new byte[UnifiedTrackingData.LatestExpressionData.LipImageData.ImageSize.x *
                                                                       UnifiedTrackingData.LatestExpressionData.LipImageData.ImageSize.y];
                lipData.image = Marshal.AllocCoTaskMem(UnifiedTrackingData.LatestExpressionData.LipImageData.ImageSize.x *
                                                       UnifiedTrackingData.LatestExpressionData.LipImageData.ImageSize.x);
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

            UpdateEyeParameters(ref UnifiedTrackingData.LatestExpressionData.LatestData, eyeData.verbose_data);
            UpdateEyeExpressions(ref UnifiedTrackingData.LatestExpressionData.LatestData, eyeData.expression_data);

            UnifiedTrackingData.LatestExpressionData.UpdateData();
            
            if (!MainWindow.IsEyePageVisible || _processHandle == IntPtr.Zero || !UnifiedTrackingData.LatestExpressionData.EyeImageData.SupportsImage) return updateResult;
            
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
            UnifiedTrackingData.LatestExpressionData.EyeImageData.ImageData = imageBytes;

            return updateResult;
        }

        private void UpdateEyeParameters(ref UnifiedExpressionsData data, VerboseData external)
        {
            data.Eye.Left.GazeNormalized = external.left.gaze_direction_normalized;
            data.Eye.Right.GazeNormalized = external.right.gaze_direction_normalized;

            data.Eye.Left.Openness = external.left.eye_openness;
            data.Eye.Right.Openness = external.right.eye_openness;

            data.Eye.Left.PupilDiameter_MM = external.left.pupil_diameter_mm;
            data.Eye.Right.PupilDiameter_MM = external.right.pupil_diameter_mm;
        }

        private void UpdateEyeExpressions(ref UnifiedExpressionsData data, EyeExpression external)
        {
            data.Shapes[(int)UnifiedExpressions.EyeWideLeft] = external.left.eye_wide;
            data.Shapes[(int)UnifiedExpressions.EyeWideRight] = external.right.eye_wide;

            data.Shapes[(int)UnifiedExpressions.EyeSquintLeft] = external.left.eye_squeeze;
            data.Shapes[(int)UnifiedExpressions.EyeSquintRight] = external.right.eye_squeeze;

            // Emulator expressions for Unified Expressions. These are essentially already baked into Legacy eye expressions (SRanipal)

            float browOuterOffset = 0.66f;

            data.Shapes[(int)UnifiedExpressions.BrowInnerUpLeft] = external.left.eye_wide;
            data.Shapes[(int)UnifiedExpressions.BrowOuterUpLeft] = external.left.eye_wide;

            data.Shapes[(int)UnifiedExpressions.BrowInnerUpRight] = external.right.eye_wide;
            data.Shapes[(int)UnifiedExpressions.BrowOuterUpRight] = external.right.eye_wide;

            data.Shapes[(int)UnifiedExpressions.BrowInnerDownLeft] = external.left.eye_squeeze;
            data.Shapes[(int)UnifiedExpressions.BrowOuterDownLeft] = external.left.eye_squeeze * browOuterOffset;

            data.Shapes[(int)UnifiedExpressions.BrowInnerDownRight] = external.right.eye_squeeze;
            data.Shapes[(int)UnifiedExpressions.BrowOuterDownRight] = external.right.eye_squeeze * browOuterOffset;
        }

        private Error UpdateMouth()
        {
            var updateResult = SRanipal_Lip_API.GetLipData_v2(ref lipData);

            UpdateMouthExpressions(ref UnifiedTrackingData.LatestExpressionData.LatestData, lipData.prediction_data);

            UnifiedTrackingData.LatestExpressionData.UpdateData();
            
            if (!MainWindow.IsLipPageVisible || lipData.image == IntPtr.Zero || !UnifiedTrackingData.LatestExpressionData.LipImageData.SupportsImage) return updateResult;
            
            Marshal.Copy(lipData.image, UnifiedTrackingData.LatestExpressionData.LipImageData.ImageData, 0, UnifiedTrackingData.LatestExpressionData.LipImageData.ImageSize.x *
                UnifiedTrackingData.LatestExpressionData.LipImageData.ImageSize.y);

            return updateResult;
        }

        private void UpdateMouthExpressions(ref UnifiedExpressionsData data, PredictionData_v2 external)
        {
            unsafe
            {
                // Direct Legacy map to SRanipal
                for (int i = 0; i < data.LegacyShapes.Length; i++)
                {
                    data.LegacyShapes[i] = external.blend_shape_weight[i];
                }

                // Mapping to UnifiedExpression

                #region Direct Jaw

                data.Shapes[(int)UnifiedExpressions.JawOpen] = external.blend_shape_weight[(int)LipShape_v2.JawOpen];
                data.Shapes[(int)UnifiedExpressions.JawLeft] = external.blend_shape_weight[(int)LipShape_v2.JawLeft];
                data.Shapes[(int)UnifiedExpressions.JawRight] = external.blend_shape_weight[(int)LipShape_v2.JawRight];
                data.Shapes[(int)UnifiedExpressions.JawForward] = external.blend_shape_weight[(int)LipShape_v2.JawForward];
                data.Shapes[(int)UnifiedExpressions.MouthApeShape] = external.blend_shape_weight[(int)LipShape_v2.MouthApeShape];

                #endregion

                #region Direct Mouth and Lip

                // These shapes have overturns subtracting from them, as we are expecting the new standard to have Upper Up / Lower Down baked into the funneller shapes below these.
                data.Shapes[(int)UnifiedExpressions.MouthUpperUpLeft] = external.blend_shape_weight[(int)LipShape_v2.MouthUpperUpLeft] - external.blend_shape_weight[(int)LipShape_v2.MouthUpperOverturn];
                data.Shapes[(int)UnifiedExpressions.MouthUpperUpRight] = external.blend_shape_weight[(int)LipShape_v2.MouthUpperUpRight] - external.blend_shape_weight[(int)LipShape_v2.MouthUpperOverturn];
                data.Shapes[(int)UnifiedExpressions.MouthLowerDownLeft] = external.blend_shape_weight[(int)LipShape_v2.MouthLowerDownLeft] - external.blend_shape_weight[(int)LipShape_v2.MouthLowerOverturn];
                data.Shapes[(int)UnifiedExpressions.MouthLowerDownRight] = external.blend_shape_weight[(int)LipShape_v2.MouthLowerDownRight] - external.blend_shape_weight[(int)LipShape_v2.MouthLowerOverturn];


                data.Shapes[(int)UnifiedExpressions.LipFunnelTopLeft] = external.blend_shape_weight[(int)LipShape_v2.MouthUpperOverturn];
                data.Shapes[(int)UnifiedExpressions.LipFunnelTopRight] = external.blend_shape_weight[(int)LipShape_v2.MouthUpperOverturn];
                data.Shapes[(int)UnifiedExpressions.LipFunnelBottomLeft] = external.blend_shape_weight[(int)LipShape_v2.MouthUpperOverturn];
                data.Shapes[(int)UnifiedExpressions.LipFunnelBottomRight] = external.blend_shape_weight[(int)LipShape_v2.MouthUpperOverturn];


                data.Shapes[(int)UnifiedExpressions.LipSuckTopLeft] = external.blend_shape_weight[(int)LipShape_v2.MouthUpperInside];
                data.Shapes[(int)UnifiedExpressions.LipSuckTopRight] = external.blend_shape_weight[(int)LipShape_v2.MouthUpperInside];
                data.Shapes[(int)UnifiedExpressions.LipSuckBottomLeft] = external.blend_shape_weight[(int)LipShape_v2.MouthLowerInside];
                data.Shapes[(int)UnifiedExpressions.LipSuckBottomRight] = external.blend_shape_weight[(int)LipShape_v2.MouthLowerInside];

                data.Shapes[(int)UnifiedExpressions.MouthTopLeft] = external.blend_shape_weight[(int)LipShape_v2.MouthUpperLeft];
                data.Shapes[(int)UnifiedExpressions.MouthTopRight] = external.blend_shape_weight[(int)LipShape_v2.MouthUpperRight];
                data.Shapes[(int)UnifiedExpressions.MouthBottomLeft] = external.blend_shape_weight[(int)LipShape_v2.MouthLowerLeft];
                data.Shapes[(int)UnifiedExpressions.MouthBottomRight] = external.blend_shape_weight[(int)LipShape_v2.MouthLowerRight];

                data.Shapes[(int)UnifiedExpressions.MouthSmileLeft] = external.blend_shape_weight[(int)LipShape_v2.MouthSmileLeft];
                data.Shapes[(int)UnifiedExpressions.MouthSmileRight] = external.blend_shape_weight[(int)LipShape_v2.MouthSmileRight];
                data.Shapes[(int)UnifiedExpressions.MouthFrownLeft] = external.blend_shape_weight[(int)LipShape_v2.MouthSadLeft];
                data.Shapes[(int)UnifiedExpressions.MouthFrownRight] = external.blend_shape_weight[(int)LipShape_v2.MouthSadRight];

                data.Shapes[(int)UnifiedExpressions.MouthRaiserUpper] = external.blend_shape_weight[(int)LipShape_v2.MouthLowerOverlay] - external.blend_shape_weight[(int)LipShape_v2.MouthUpperInside];
                data.Shapes[(int)UnifiedExpressions.MouthRaiserLower] = external.blend_shape_weight[(int)LipShape_v2.MouthLowerOverlay];

                #endregion

                #region Direct Cheek

                data.Shapes[(int)UnifiedExpressions.CheekPuffLeft] = external.blend_shape_weight[(int)LipShape_v2.CheekPuffLeft];
                data.Shapes[(int)UnifiedExpressions.CheekPuffRight] = external.blend_shape_weight[(int)LipShape_v2.CheekPuffRight];

                data.Shapes[(int)UnifiedExpressions.CheekSuckLeft] = external.blend_shape_weight[(int)LipShape_v2.CheekSuck];
                data.Shapes[(int)UnifiedExpressions.CheekSuckRight] = external.blend_shape_weight[(int)LipShape_v2.CheekSuck];

                #endregion

                #region Direct Tongue

                data.Shapes[(int)UnifiedExpressions.TongueOut] = (external.blend_shape_weight[(int)LipShape_v2.TongueLongStep1] + external.blend_shape_weight[(int)LipShape_v2.TongueLongStep2]) / 2.0f;
                data.Shapes[(int)UnifiedExpressions.TongueUp] = external.blend_shape_weight[(int)LipShape_v2.TongueUp];
                data.Shapes[(int)UnifiedExpressions.TongueDown] = external.blend_shape_weight[(int)LipShape_v2.TongueDown];
                data.Shapes[(int)UnifiedExpressions.TongueLeft] = external.blend_shape_weight[(int)LipShape_v2.TongueLeft];
                data.Shapes[(int)UnifiedExpressions.TongueRight] = external.blend_shape_weight[(int)LipShape_v2.TongueRight];
                data.Shapes[(int)UnifiedExpressions.TongueRoll] = external.blend_shape_weight[(int)LipShape_v2.TongueRoll];

                #endregion

                // These shapes are not tracked at all by SRanipal, but instead are being treated as enhancements to driving the shapes above.

                #region Emulated Unified Mapping

                data.Shapes[(int)UnifiedExpressions.CheekSquintLeft] = external.blend_shape_weight[(int)LipShape_v2.MouthSmileLeft];
                data.Shapes[(int)UnifiedExpressions.CheekSquintRight] = external.blend_shape_weight[(int)LipShape_v2.MouthSmileRight];

                data.Shapes[(int)UnifiedExpressions.MouthDimpleLeft] = external.blend_shape_weight[(int)LipShape_v2.MouthSmileLeft];
                data.Shapes[(int)UnifiedExpressions.MouthDimpleRight] = external.blend_shape_weight[(int)LipShape_v2.MouthSmileRight];

                data.Shapes[(int)UnifiedExpressions.MouthStretchLeft] = external.blend_shape_weight[(int)LipShape_v2.MouthSadRight];
                data.Shapes[(int)UnifiedExpressions.MouthStretchRight] = external.blend_shape_weight[(int)LipShape_v2.MouthSadRight];

                #endregion
            }
        }
    }
}