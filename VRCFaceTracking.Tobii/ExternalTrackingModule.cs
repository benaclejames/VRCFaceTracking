// #define TOBIIDEBUG

/* Define TOBIIDEBUG symbol if debug logs are needed, Release configuration causes issues with undefined reference. 
 * Either due to pointer/memory handling, race condition/multithreading differences etc.
 * Due to this, stay on Debug configuration.
*/

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Tobii.StreamEngine;
using VRCFaceTracking;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Types;

namespace VRCFT_Module_TobiiXR;

// Action to provide debug logs only if TOBIIDEBUG is set.
class TobiiXRLogger
{
    public static Action<string> Log = delegate (string x)
    {
        #if TOBIIDEBUG
            Logger.Msg(x);
        #endif

    };
}

public struct TobiiXRExternalTrackingDataEye
{
    public float eye_lid_openness;
    public float eye_x;
    public float eye_y;
}

public struct TobiiXRExternalTrackingDataStruct
{
    public TobiiXRExternalTrackingDataEye left_eye;
    public TobiiXRExternalTrackingDataEye right_eye;
}   

public static class TrackingData
{
    // Convert eye X/Y data from 1 to 0 format into 1 to -1 format and then normalise to ensure max values are either 1 or -1 respectively.
    // TODO : Callibration tool to gather max values for each axis/directions for better accuracy here.
    private static float NormalisationX = (float)0.5;
    private static float NormalisationY = (float)0.5;

    private static float ParseEyeData(float EyeData, bool XorY = true)
    {
        if (XorY)
            return ((EyeData * 2) - 1) / NormalisationX;
        else
            return ((EyeData * -2) + 1) / NormalisationY;
    }

    // This function parses the external module's single-eye data into a VRCFT-Parseable format
    public static void Update(ref UnifiedSingleEyeData data, TobiiXRExternalTrackingDataEye external)
    {
        data.Gaze = new Vector2(ParseEyeData(external.eye_x), ParseEyeData(external.eye_y, false));
        data.Openness = external.eye_lid_openness;
    }

    // This function parses the external module's full-data data into multiple VRCFT-Parseable single-eye structs
    public static void Update(ref UnifiedEyeData data, TobiiXRExternalTrackingDataStruct external)
    {
        Update(ref data.Left, external.right_eye);
        Update(ref data.Right, external.left_eye);
    }
}

public class ExternalExtTrackingModule : ExtTrackingModule
{
    public override (bool SupportsEye, bool SupportsExpression) Supported => (true, false);

    private IntPtr apiContext = Marshal.AllocHGlobal(1024);
    private IntPtr deviceContext = Marshal.AllocHGlobal(1024);
    private tobii_error_t result;
    private Thread thread;
    private bool _loop;

    TobiiXRExternalTrackingDataStruct ParsedtrackingData;

    public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eyeSuccess, bool expressionSuccess)
    {
        ExtractDLLs();
        result = Interop.tobii_api_create(out apiContext, null);
        result = Interop.tobii_enumerate_local_device_urls(apiContext, out var urls);
        if (urls.Count == 0) return (false, false);

        result = Interop.tobii_device_create(apiContext, urls[0], Interop.tobii_field_of_use_t.TOBII_FIELD_OF_USE_STORE_OR_TRANSFER_FALSE, out deviceContext);
        TobiiXRLogger.Log(result.ToString());

        _loop = true;
        thread = new Thread(GetUpdateThreadFunc);
        thread.Start();

        return (true, false);
    }

    private void GetUpdateThreadFunc()
    {   
        result = Interop.tobii_gaze_point_subscribe(deviceContext, TobiiUpdate);
        TobiiXRLogger.Log(result.ToString());

        while (_loop)
        {
            Interop.tobii_wait_for_callbacks(new[] { deviceContext });
            Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR || result == tobii_error_t.TOBII_ERROR_TIMED_OUT);

            Interop.tobii_device_process_callbacks(deviceContext);
            Debug.Assert(result == tobii_error_t.TOBII_ERROR_NO_ERROR);

            //Thread.Sleep(10);
        }
    }

    public void TobiiUpdate(ref tobii_gaze_point_t consumerData, IntPtr userData)
    {
        if (Status == VRCFaceTracking.Core.Library.ModuleState.Active)
        {
            ParsedtrackingData.left_eye.eye_lid_openness = 1f;
            ParsedtrackingData.right_eye.eye_lid_openness = 1f;

            ParsedtrackingData.left_eye.eye_x = consumerData.position.x;
            ParsedtrackingData.left_eye.eye_y = consumerData.position.y;

            ParsedtrackingData.right_eye.eye_x = consumerData.position.x;
            ParsedtrackingData.right_eye.eye_y = consumerData.position.y;

            TrackingData.Update(ref UnifiedTracking.Data.Eye, ParsedtrackingData);
        }
    }

    public override void Teardown()
    {
        _loop = false;
        thread.Join();
        Interop.tobii_gaze_point_unsubscribe(deviceContext);
        Interop.tobii_device_destroy(deviceContext);
        Interop.tobii_api_destroy(apiContext);
        Marshal.FreeHGlobal(deviceContext);
        Marshal.FreeHGlobal(apiContext);
    }

    public override void Update() { }

    private static void ExtractDLLs()
    {
        // Extract the Embedded DLL
        var dirName = Path.Combine(Utils.PersistentDataDirectory, "CustomLibs", "TobiiDeps");
        if (!Directory.Exists(dirName))
            Directory.CreateDirectory(dirName);

        var dllPath = Path.Combine(dirName, "tobii_stream_engine.dll");

        // TODO embed this in this assembly
        using var stm = Assembly.GetExecutingAssembly()
                   .GetManifestResourceStream("VRCFaceTracking.Tobii.tobii_stream_engine.dll");
        try
        {
            using (Stream outFile = File.Create(dllPath))
            {
                const int sz = 4096;
                var buf = new byte[sz];
                while (true)
                {
                    if (stm == null) continue;
                    var nRead = stm.Read(buf, 0, sz);
                    if (nRead < 1)
                        break;
                    outFile.Write(buf, 0, nRead);
                }
            }

            // Load the DLL
            LoadLibrary(dllPath);
        }
        catch (Exception) { }
    }

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string lpFileName);
}