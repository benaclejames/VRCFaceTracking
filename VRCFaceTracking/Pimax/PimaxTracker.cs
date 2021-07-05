using System.Runtime.InteropServices;

namespace VRCFaceTracking.Pimax
{
	public class Ai1EyeData
	{
		public EyeExpressionState Left = PimaxTracker.GetEyeData(Eye.Left);
		public EyeExpressionState Right = PimaxTracker.GetEyeData(Eye.Right);
		public EyeExpressionState Recommended = PimaxTracker.GetEyeData(Eye.Any);
	}
	
	
	// Mad props to NGenesis for these bindings <3

	public enum Eye {
		Any,
		Left,
		Right
	}

	public enum CallbackType {
		Start,
		Stop,
		Update
	}

	public struct EyeExpressionState
	{
		public float PupilCenterX;
		public float PupilCenterY;
		public float Openness;
	};

	public delegate void EyeTrackerEventHandler();
	
    public static class PimaxTracker
    {
	    /// <summary>
	    /// Registers callbacks for the tracker to notify when it's finished initializing, when it has new data available and when the module is stopped.
	    /// </summary>
        [DllImport("PimaxEyeTracker", EntryPoint = "RegisterCallback")] public static extern void RegisterCallback(CallbackType type, EyeTrackerEventHandler callback);
	    
	    /// <summary>
	    /// Initializes the module.
	    /// </summary>
	    /// <returns>Initialization Successful</returns>
		[DllImport("PimaxEyeTracker", EntryPoint = "Start")] public static extern bool Start();
	    
	    /// <summary>
	    /// Stops the eye tracking module and disconnects the server
	    /// </summary>
		[DllImport("PimaxEyeTracker", EntryPoint = "Stop")] public static extern void Stop();
	    [DllImport("PimaxEyeTracker", EntryPoint = "GetEyeData")] public static extern EyeExpressionState GetEyeData(Eye eye);
    }
}