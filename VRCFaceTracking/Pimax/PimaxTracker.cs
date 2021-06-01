using System.Runtime.InteropServices;
using UnityEngine;

namespace VRCFaceTracking.Pimax
{
	public class Ai1EyeData
	{
		public EyeState Left = new EyeState(Eye.Left);
		public EyeState Right = new EyeState(Eye.Right);
		public EyeState Recommended = new EyeState(PimaxTracker.GetRecommendedEye());
	}
	
	
	// Mad props to NGenesis for these bindings <3
	
	public enum EyeParameter {
		GazeX, // Gaze point on the X axis (not working!)
		GazeY, // Gaze point on then Y axis (not working!)
		GazeRawX, // Gaze point on the X axis before smoothing is applied (not working!)
		GazeRawY, // Gaze point on the Y axis before smoothing is applied (not working!)
		GazeSmoothX, // Gaze point on the X axis after smoothing is applied (not working!)
		GazeSmoothY, // Gaze point on the Y axis after smoothing is applied (not working!)
		GazeOriginX, // Pupil gaze origin on the X axis
		GazeOriginY, // Pupil gaze origin on the Y axis
		GazeOriginZ, // Pupil gaze origin on the Z axis
		GazeDirectionX, // Gaze vector on the X axis (not working!)
		GazeDirectionY, // Gaze vector on the Y axis (not working!)
		GazeDirectionZ, // Gaze vector on the Z axis (not working!)
		GazeReliability, // Gaze point reliability (not working!)
		PupilCenterX, // Pupil center on the X axis, normalized between 0 and 1
		PupilCenterY, // Pupil center on the Y axis, normalized between 0 and 1
		PupilDistance, // Distance between pupil and camera lens, measured in millimeters
		PupilMajorDiameter, // Pupil major axis diameter, normalized between 0 and 1
		PupilMajorUnitDiameter, // Pupil major axis diameter, measured in millimeters
		PupilMinorDiameter, // Pupil minor axis diameter, normalized between 0 and 1
		PupilMinorUnitDiameter, // Pupil minor axis diameter, measured in millimeters
		Blink, // Blink state (not working!)
		Openness, // How open the eye is - 100 (closed), 50 (partially open, unreliable), 0 (open)
        UpperEyelid, // Upper eyelid state (not working!)
		LowerEyelid // Lower eyelid state (not working!)
	}

	public enum EyeExpression {
		PupilCenterX, // Pupil center on the X axis, smoothed and normalized between -1 (looking left) ... 0 (looking forward) ... 1 (looking right)
		PupilCenterY, // Pupil center on the Y axis, smoothed and normalized between -1 (looking up) ... 0 (looking forward) ... 1 (looking down)
		Openness, // How open the eye is, smoothed and normalized between 0 (fully closed) ... 1 (fully open)
		Blink // Blink, 0 (not blinking) or 1 (blinking)
	}

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

	public readonly struct EyeExpressionState
	{
		private readonly Vector2 _pupilCenter;
		private readonly float _openness;
		private readonly bool _blink;

		public EyeExpressionState(Eye eyeType)
		{
			_pupilCenter = new Vector2(PimaxTracker.GetEyeExpression(eyeType, EyeExpression.PupilCenterX),
				PimaxTracker.GetEyeExpression(eyeType, EyeExpression.PupilCenterY));
			_openness = PimaxTracker.GetEyeExpression(eyeType, EyeExpression.Openness);
			_blink = PimaxTracker.GetEyeExpression(eyeType, EyeExpression.Blink) != 0.0f;
		}
	}

	public readonly struct EyeState
	{
		public readonly Vector2 Gaze, GazeRaw, GazeSmooth, GazeOrigin, GazeDirection, PupilCenter;
		
		public readonly float PupilDistance,
			PupilMajorDiameter,
			PupilMajorUnitDiameter,
			PupilMinorDiameter,
			PupilMinorUnitDiameter,
			GazeReliability,
			Blink,
			UpperEyelid,
			LowerEyelid,
			Openness;

		public readonly EyeExpressionState Expression;

		public EyeState(Eye eyeType)
		{
			Gaze = new Vector2(PimaxTracker.GetEyeParameter(eyeType, EyeParameter.GazeX),
				PimaxTracker.GetEyeParameter(eyeType, EyeParameter.GazeY));
			GazeRaw = new Vector2(PimaxTracker.GetEyeParameter(eyeType, EyeParameter.GazeRawX),
				PimaxTracker.GetEyeParameter(eyeType, EyeParameter.GazeRawY));
			GazeSmooth = new Vector2(PimaxTracker.GetEyeParameter(eyeType, EyeParameter.GazeSmoothX),
				PimaxTracker.GetEyeParameter(eyeType, EyeParameter.GazeSmoothY));
			GazeOrigin = new Vector3(PimaxTracker.GetEyeParameter(eyeType, EyeParameter.GazeOriginX),
				PimaxTracker.GetEyeParameter(eyeType, EyeParameter.GazeOriginY),
				PimaxTracker.GetEyeParameter(eyeType, EyeParameter.GazeOriginZ));
			GazeDirection = new Vector3(PimaxTracker.GetEyeParameter(eyeType, EyeParameter.GazeDirectionX),
				PimaxTracker.GetEyeParameter(eyeType, EyeParameter.GazeDirectionY),
				PimaxTracker.GetEyeParameter(eyeType, EyeParameter.GazeDirectionZ));
			GazeReliability = PimaxTracker.GetEyeParameter(eyeType, EyeParameter.GazeReliability);
			PupilDistance = PimaxTracker.GetEyeParameter(eyeType, EyeParameter.PupilDistance);
			PupilMajorDiameter = PimaxTracker.GetEyeParameter(eyeType, EyeParameter.PupilMajorDiameter);
			PupilMajorUnitDiameter = PimaxTracker.GetEyeParameter(eyeType, EyeParameter.PupilMajorUnitDiameter);
			PupilMinorDiameter = PimaxTracker.GetEyeParameter(eyeType, EyeParameter.PupilMinorDiameter);
			PupilMinorUnitDiameter = PimaxTracker.GetEyeParameter(eyeType, EyeParameter.PupilMinorUnitDiameter);
			Blink = PimaxTracker.GetEyeParameter(eyeType, EyeParameter.Blink);
			UpperEyelid = PimaxTracker.GetEyeParameter(eyeType, EyeParameter.UpperEyelid);
			LowerEyelid = PimaxTracker.GetEyeParameter(eyeType, EyeParameter.LowerEyelid);
			Openness = PimaxTracker.GetEyeParameter(eyeType, EyeParameter.Openness);
			PupilCenter = new Vector2(PimaxTracker.GetEyeParameter(eyeType, EyeParameter.PupilCenterX),
				PimaxTracker.GetEyeParameter(eyeType, EyeParameter.PupilCenterY));
			Expression = new EyeExpressionState(eyeType);

			// Convert range from 0...1 to -1...1, defaulting eyes to center (0) when unavailable
			//float x = eyeTracker.GetEyeParameter(this.Eye, EyeParameter.PupilCenterX);  
			//float y = eyeTracker.GetEyeParameter(this.Eye, EyeParameter.PupilCenterY);
			//this.PupilCenter = new Vector2(x <= float.Epsilon ? 0.0f : (x * 2.0f - 1.0f), y <= float.Epsilon ? 0.0f : (y * 2.0f - 1.0f));
			//this.Openness = (x <= float.Epsilon && y <= float.Epsilon) ? 0.0f : 1.0f;
		}
	}
	
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
	    
	    /// <summary>
	    /// Query aSeeVR for the eye it's most confident tracking
	    /// </summary>
		[DllImport("PimaxEyeTracker", EntryPoint = "GetRecommendedEye")] public static extern Eye GetRecommendedEye();
	    
		[DllImport("PimaxEyeTracker", EntryPoint = "GetEyeParameter")] public static extern float GetEyeParameter(Eye eye, EyeParameter param);
		[DllImport("PimaxEyeTracker", EntryPoint = "GetEyeExpression")] public static extern float GetEyeExpression(Eye eye, EyeExpression expression);
    }
}