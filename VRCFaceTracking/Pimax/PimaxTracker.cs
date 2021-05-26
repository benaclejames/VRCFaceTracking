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

	public struct EyeExpressionState
	{
		public Eye Eye { get; }
		public Vector2 PupilCenter { get; }
		public float Openness { get; }
		public bool Blink { get; }

		public EyeExpressionState(Eye eyeType)
		{
			Eye = eyeType;
			PupilCenter = new Vector2(PimaxTracker.GetEyeExpression(Eye, EyeExpression.PupilCenterX),
				PimaxTracker.GetEyeExpression(Eye, EyeExpression.PupilCenterY));
			Openness = PimaxTracker.GetEyeExpression(Eye, EyeExpression.Openness);
			Blink = PimaxTracker.GetEyeExpression(Eye, EyeExpression.Blink) != 0.0f;
		}
	}

	public struct EyeState
	{
		public Eye Eye { get; }
		public Vector2 Gaze { get; }
		public Vector2 GazeRaw { get; }
		public Vector2 GazeSmooth { get; }
		public Vector3 GazeOrigin { get; }
		public Vector3 GazeDirection { get; }
		public float GazeReliability { get; }
		public Vector2 PupilCenter { get; }
		public float PupilDistance { get; }
		public float PupilMajorDiameter { get; }
		public float PupilMajorUnitDiameter { get; }
		public float PupilMinorDiameter { get; }
		public float PupilMinorUnitDiameter { get; }
		public float Blink { get; }
		public float Openness { get; }
		public float UpperEyelid { get; }
		public float LowerEyelid { get; }
		public EyeExpressionState Expression { get; }

		public EyeState(Eye eyeType)
		{
			Eye = eyeType;
			Gaze = new Vector2(PimaxTracker.GetEyeParameter(Eye, EyeParameter.GazeX),
				PimaxTracker.GetEyeParameter(Eye, EyeParameter.GazeY));
			GazeRaw = new Vector2(PimaxTracker.GetEyeParameter(Eye, EyeParameter.GazeRawX),
				PimaxTracker.GetEyeParameter(Eye, EyeParameter.GazeRawY));
			GazeSmooth = new Vector2(PimaxTracker.GetEyeParameter(Eye, EyeParameter.GazeSmoothX),
				PimaxTracker.GetEyeParameter(Eye, EyeParameter.GazeSmoothY));
			GazeOrigin = new Vector3(PimaxTracker.GetEyeParameter(Eye, EyeParameter.GazeOriginX),
				PimaxTracker.GetEyeParameter(Eye, EyeParameter.GazeOriginY),
				PimaxTracker.GetEyeParameter(Eye, EyeParameter.GazeOriginZ));
			GazeDirection = new Vector3(PimaxTracker.GetEyeParameter(Eye, EyeParameter.GazeDirectionX),
				PimaxTracker.GetEyeParameter(Eye, EyeParameter.GazeDirectionY),
				PimaxTracker.GetEyeParameter(Eye, EyeParameter.GazeDirectionZ));
			GazeReliability = PimaxTracker.GetEyeParameter(Eye, EyeParameter.GazeReliability);
			PupilDistance = PimaxTracker.GetEyeParameter(Eye, EyeParameter.PupilDistance);
			PupilMajorDiameter = PimaxTracker.GetEyeParameter(Eye, EyeParameter.PupilMajorDiameter);
			PupilMajorUnitDiameter = PimaxTracker.GetEyeParameter(Eye, EyeParameter.PupilMajorUnitDiameter);
			PupilMinorDiameter = PimaxTracker.GetEyeParameter(Eye, EyeParameter.PupilMinorDiameter);
			PupilMinorUnitDiameter = PimaxTracker.GetEyeParameter(Eye, EyeParameter.PupilMinorUnitDiameter);
			Blink = PimaxTracker.GetEyeParameter(Eye, EyeParameter.Blink);
			UpperEyelid = PimaxTracker.GetEyeParameter(Eye, EyeParameter.UpperEyelid);
			LowerEyelid = PimaxTracker.GetEyeParameter(Eye, EyeParameter.LowerEyelid);
			Openness = PimaxTracker.GetEyeParameter(Eye, EyeParameter.Openness);
			PupilCenter = new Vector2(PimaxTracker.GetEyeParameter(Eye, EyeParameter.PupilCenterX),
				PimaxTracker.GetEyeParameter(Eye, EyeParameter.PupilCenterY));
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
	    
	    
		[DllImport("PimaxEyeTracker", EntryPoint = "IsActive")] private static extern bool IsActive();
		[DllImport("PimaxEyeTracker", EntryPoint = "GetTimestamp")] private static extern System.Int64 GetTimestamp();
		[DllImport("PimaxEyeTracker", EntryPoint = "GetRecommendedEye")] public static extern Eye GetRecommendedEye();
		[DllImport("PimaxEyeTracker", EntryPoint = "GetEyeParameter")] public static extern float GetEyeParameter(Eye eye, EyeParameter param);
		[DllImport("PimaxEyeTracker", EntryPoint = "GetEyeExpression")] public static extern float GetEyeExpression(Eye eye, EyeExpression expression);
    }
}