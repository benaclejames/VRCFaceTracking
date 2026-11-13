namespace VRCFaceTracking.Core.Params.Data.Mutation;

internal class NativeGazeRange : TrackingMutation
{
    public override string Name => "Native Eye Gaze Range";
    public override string Description => "VRChat native face tracking avatars only. Gaze angle that turns the avatar's eyes fully (its Eye Look rotation). Lower = eyes move more.";
    public override MutationPriority Step => MutationPriority.Postprocessor;

    [MutationProperty("Full eye turn at (degrees)", true, 10f, 60f)]
    public float maxGazeAngle = 30f;

    // Read by UnifiedEyeData.CopyTo when it writes the native EyeLook bytes.
    // shortcut: switching this off keeps the last angle until restart; make CopyTo read the mutation if users need "off" = default
    internal static float MaxAngle = 30f;

    public NativeGazeRange()
    {
        _isActive = true;
    }

    public override void MutateData(ref UnifiedTrackingData data)
    {
        // The slider's 10..60 only bounds the UI; a hand-edited or restored settings value (<= 0, NaN) would mirror, snap or freeze the eyes.
        MaxAngle = float.IsFinite(maxGazeAngle) ? Math.Clamp(maxGazeAngle, 10f, 60f) : 30f;
    }
}
