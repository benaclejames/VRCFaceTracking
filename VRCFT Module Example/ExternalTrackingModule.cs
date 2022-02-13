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
    }
    
    // Example "full-data" response from the external tracking system.
    public struct ExampleExternalTrackingDataStruct
    {
        public ExampleExternalTrackingDataEye left_eye;
        public ExampleExternalTrackingDataEye right_eye;
    }   
    
    
    // This class contains the overrides for any VRCFT Tracking Data struct functions
    public static class TrackingData
    {
        // This function parses the external module's single-eye data into a VRCFT-Parseable format
        public static void Update(ref Eye data, ExampleExternalTrackingDataEye external)
        {
            data.Look = new Vector2(external.eye_x, external.eye_y);
            data.Openness = external.eye_lid_openness;
        }

        // This function parses the external module's full-data data into multiple VRCFT-Parseable single-eye structs
        public static void Update(ref EyeTrackingData data, ExampleExternalTrackingDataStruct external)
        {
            Update(ref data.Right, external.left_eye);
            Update(ref data.Left, external.right_eye);
        }
    }
    
    public class ExternalTrackingModule : ITrackingModule
    {
        // Synchronous module initialization. Take as much time as you need to initialize any external modules. This runs in the init-thread
        public (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip)
        {
            Console.WriteLine("Initializing inside external module");
            return (true, false);
        }

        // This will be run in the tracking thread. This is exposed so you can control when and if the tracking data is updated down to the lowest level.
        public Action GetUpdateThreadFunc()
        {
            return () =>
            {
                while (true)
                {
                    Update();
                    Thread.Sleep(10);
                }
            };
        }

        // The update function needs to be defined separately in case the user is running with the --vrcft-nothread launch parameter
        public void Update()
        {
            Console.WriteLine("Updating inside external module");
        }

        // A chance to de-initialize everything. This runs synchronously inside main game thread. Do not touch any Unity objects here.
        public void Teardown()
        {
            Console.WriteLine("Teardown");
        }

        public bool SupportsEye => true;
        public bool SupportsLip => true;
    }
}