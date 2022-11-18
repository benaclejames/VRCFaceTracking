using System;
using System.Threading;
using VRCFaceTracking;
using VRCFaceTracking.Params;

namespace VRCFT_Module_Example
{ 
    // Example "single-eye" data response.    
    public struct ExampleExternalTrackingDataEye
    {
        public float eye_lid_openness;
        public float eye_x;
        public float eye_y;

        public bool eye_validity;
    }
    
    // Example "full-data" response from the external tracking system.
    public struct ExampleExternalTrackingDataStruct
    {
        public ExampleExternalTrackingDataEye left_eye;
        public ExampleExternalTrackingDataEye right_eye;
    }

    // Example expression response from external tracking system.
    public struct ExampleExternalTrackingDataExpressions
    {
        public float jaw_open;
        public float tongue_out;
    }

    // This class contains the overrides for any VRCFT Tracking Data struct functions
    public static class TrackingData
    {

        // This function parses the external module's full-data data into UnifiedExpressions' eye structure.
        public static void UpdateEye(ref UnifiedEyeData data, ExampleExternalTrackingDataStruct external)
        {
            data.Right.Openness = external.left_eye.eye_lid_openness;
            data.Right.Openness = external.right_eye.eye_lid_openness;

            data.Left.GazeNormalized = new Vector2(external.left_eye.eye_x, external.left_eye.eye_y);
            data.Right.GazeNormalized = new Vector2(external.right_eye.eye_x, external.right_eye.eye_y);

            // This will tell VRCFaceTracking if the eye data is accurate/usable or not.
            data.Left.Valid = external.left_eye.eye_validity;
            data.Right.Valid = external.right_eye.eye_validity;
        }

        // This function parses the external module's full-data data into the UnifiedExpressions' Shapes
        public static void UpdateExpressions(ref UnifiedExpressionsData data, ExampleExternalTrackingDataExpressions external)
        {
            // Map to Shapes from the External structure the UnifiedExpressionData structure to access UnifiedExpression shapes.
            data.Shapes[(int)UnifiedExpressions.JawOpen] = external.jaw_open;
            data.Shapes[(int)UnifiedExpressions.TongueOut] = external.tongue_out;

            // Map to LegacyShapes within the UnifiedExpressionData to access SRanipal shapes. This may become obsolete.
            data.LegacyShapes[(int)SRanipal_LipShape_v2.JawOpen] = external.jaw_open;
            data.LegacyShapes[(int)SRanipal_LipShape_v2.TongueLongStep1] = external.tongue_out;
            data.LegacyShapes[(int)SRanipal_LipShape_v2.TongueLongStep2] = external.tongue_out;
        }
    }
    
    public class ExternalExtTrackingModule : ExtTrackingModule
    {
        // Example of data coming from the outside tracking interface.
        ExampleExternalTrackingDataStruct external_eye = new ExampleExternalTrackingDataStruct();
        ExampleExternalTrackingDataExpressions external_expressions = new ExampleExternalTrackingDataExpressions();
        bool external_tracking_state;

        // Synchronous module initialization. Take as much time as you need to initialize any external modules. This runs in the init-thread
        public override (bool SupportsEye, bool SupportsExpressions) Supported => (true, true);

        public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eye, bool exp)
        {
            Console.WriteLine("Initializing inside external module");
            return (true, false);
        }

        // This will be run in the tracking thread. This is exposed so you can control when and if the tracking data is updated down to the lowest level.
        public override Action GetUpdateThreadFunc()
        {
            return () =>
            {
                while (external_tracking_state)
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
            Console.WriteLine("Updating inside external module.");

            if (Status.EyeState == ModuleState.Active)
            {
                Console.WriteLine("Eye data is being utilized.");
                TrackingData.UpdateEye(ref UnifiedTracking.AllData.LatestExpressionData.Eye, external_eye);
            }
            if (Status.ExpressionState == ModuleState.Active)
            {
                Console.WriteLine("Expression data is being utilized.");
                TrackingData.UpdateExpressions(ref UnifiedTracking.AllData.LatestExpressionData, external_expressions);
            }

            // Updates the parameter internally in VRCFT
            UnifiedTracking.AllData.UpdateData();
        }

        // A chance to de-initialize everything. This runs synchronously inside main game thread. Do not touch any Unity objects here.
        public override void Teardown()
        {
            Console.WriteLine("Teardown");
        }
    }
}