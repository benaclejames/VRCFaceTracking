using System;
using System.Threading;
using VRCFaceTracking;
using VRCFaceTracking.Params;
using VRCFT_Module_Example.Structures;

namespace VRCFT_Module_Example
{ 
    // This class contains the overrides for any VRCFT Tracking Data struct functions
    public static class TrackingData
    {

        // This function parses the external module's full-data data into UnifiedExpressions' eye structure.
        public static void UpdateEye(ref UnifiedEyeData data, ExampleExternalTrackingDataStruct external)
        {
            if (external.left_eye.eye_validity) 
            {
                data.Left.Openness = external.left_eye.eye_lid_openness;
                data.Left.Gaze = new Vector2(external.left_eye.eye_x, external.left_eye.eye_y);
            }

            if (external.right_eye.eye_validity)
            {
                data.Right.Openness = external.right_eye.eye_lid_openness;
                data.Right.Gaze = new Vector2(external.right_eye.eye_x, external.right_eye.eye_y);
            }
        }

        // This function parses the external module's full-data data into the UnifiedExpressions' Shapes
        public static void UpdateExpressions(ref UnifiedTrackingData data, ExampleExternalTrackingDataExpressions external)
        {
            data.Shapes[(int)UnifiedExpressions.JawOpen].Weight = external.jaw_open;
            data.Shapes[(int)UnifiedExpressions.TongueOut].Weight = external.tongue_out;
        }
    }
    
    public class ExternalExtTrackingModule : ExtTrackingModule
    {
        // Example of data coming from the outside tracking interface.
        ExampleExternalTrackingDataStruct external_eye = new ExampleExternalTrackingDataStruct();
        ExampleExternalTrackingDataExpressions external_expressions = new ExampleExternalTrackingDataExpressions();

        // Lets Unified Library Manager know what type of data to expect.
        public override (bool SupportsEye, bool SupportsExpressions) Supported => (true, true);

        // Synchronous module initialization. Take as much time as you need to initialize any external modules. This runs in the init-thread
        public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeAvailable, bool expressionAvailable)
        {
            Logger.Msg("Initializing inside external module");

            // Use the incoming parameters to determine if the tracking interface can initialize into any available module slots.
            // Then let the library manager know if your tracking interface has initialized properly

            var moduleState = (eyeAvailable, expressionAvailable);

            return moduleState;
        }

        // This will be run in the tracking thread. This is exposed so you can control when and if the tracking data is updated down to the lowest level.
        public override Action GetUpdateThreadFunc()
        {
            return () =>
            {
                while (true)
                {
                    Update();

                    // Have the update function work in-tandem with your tracking's update
                    Thread.Sleep(10);
                }
            };
        }

        // The update function needs to be defined separately in case the user is running with the --vrcft-nothread launch parameter
        public void Update()
        {
            Logger.Msg("Updating inside external module.");

            if (Status.EyeState == ModuleState.Active)
            {
                Logger.Msg("Eye data is being utilized.");
                TrackingData.UpdateEye(ref UnifiedTracking.Data.Eye, external_eye);
            }
            if (Status.ExpressionState == ModuleState.Active)
            {
                Logger.Msg("Expression data is being utilized.");
                TrackingData.UpdateExpressions(ref UnifiedTracking.Data, external_expressions);
            }
        }

        public override void Teardown()
        {
            Logger.Msg("Teardown");
        }
    }
}