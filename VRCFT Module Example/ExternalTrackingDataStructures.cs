using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCFT_Module_Example.Structures
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
}
