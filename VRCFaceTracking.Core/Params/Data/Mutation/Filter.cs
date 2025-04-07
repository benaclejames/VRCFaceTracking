using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.Core.Params.Data.Mutation;
public class Filter : TrackingMutation
{
    [MutationProperty("Minimum Cutoff", 0f, 2f)]
    public static float minCutoff = 1f;
    [MutationProperty("Beta", 0f, 1f)]
    public static float beta = 0.5f;
    [MutationProperty("Derivative Cutoff")]
    public static float dCutoff = 0.1f;
    private const float hz = 10f;

    public class EuroFilter
    {
        public const float PI = (float)Math.PI;

        float xPrev;
        float dxPrev;

        public float Filter(float x)
        {
            if (float.IsNaN(x))
            {
                return 0f;
            }
            var dx = (x - xPrev) * hz;

            var edx = LowPass(ref dxPrev, dx, Alpha(hz, dCutoff));
            var cutoff = minCutoff + beta * (float)Math.Abs(edx);

            return LowPass(ref xPrev, x, Alpha(hz, cutoff));
        }

        private float Alpha(float hz, float cutoff)
        {
            var tau = 1.0f / (2 * PI * cutoff);
            var te = 1.0f / hz;
            return 1.0f / (1.0f + tau / te);
        }

        private float LowPass(ref float hatXPrev, float x, float alpha)
        {
            var hatX = alpha * x + (1 - alpha) * hatXPrev;
            hatXPrev = hatX;
            return hatX;
        }
    }

    EuroFilter[] shapes;
    EuroFilter gazeLeftX;
    EuroFilter gazeLeftY;
    EuroFilter gazeRightX;
    EuroFilter gazeRightY;
    EuroFilter pupilLeft;
    EuroFilter pupilRight;
    EuroFilter opennessLeft;
    EuroFilter opennessRight;

    EuroFilter headYaw;
    EuroFilter headPitch;
    EuroFilter headRoll;
    EuroFilter headPosX;
    EuroFilter headPosY;
    EuroFilter headPosZ;

    public override string Name => "Data Filter";

    public override string Description => "Default data filtering for VRCFaceTracking expressions.";

    public override MutationPriority Step => MutationPriority.Postprocessor;

    public override bool IsSaved => true;
    public override bool IsActive { get; set; } = false;

    public Filter()
    {
        shapes = new EuroFilter[(int)UnifiedExpressions.Max];
        gazeLeftX = new();
        gazeLeftY = new();
        gazeRightX = new();
        gazeRightY = new();
        pupilLeft = new();
        pupilRight = new();
        opennessLeft = new();
        opennessRight = new();
        headYaw = new();
        headPitch = new();
        headRoll = new();
        headPosX = new();
        headPosY = new();
        headPosZ = new();

        for (int i = 0; i < (int)UnifiedExpressions.Max; i++)
        {
            shapes[i] = new EuroFilter();
        }
    }

    public override void MutateData(ref UnifiedTrackingData data)
    {
        for (int i = 0; i < (int)UnifiedExpressions.Max; i++)
            data.Shapes[i].Weight = shapes[i].Filter(data.Shapes[i].Weight);

        data.Eye.Left.Openness = opennessLeft.Filter(data.Eye.Left.Openness);
        data.Eye.Left.PupilDiameter_MM = pupilLeft.Filter(data.Eye.Left.PupilDiameter_MM);
        data.Eye.Left.Gaze.x = gazeLeftX.Filter(data.Eye.Left.Gaze.x);
        data.Eye.Left.Gaze.y = gazeLeftY.Filter(data.Eye.Left.Gaze.y);

        data.Eye.Right.Openness = opennessRight.Filter(data.Eye.Right.Openness);
        data.Eye.Right.PupilDiameter_MM = pupilRight.Filter(data.Eye.Right.PupilDiameter_MM);
        data.Eye.Right.Gaze.x = gazeRightX.Filter(data.Eye.Right.Gaze.x);
        data.Eye.Right.Gaze.y = gazeRightY.Filter(data.Eye.Right.Gaze.y);

        data.Head.HeadYaw = headYaw.Filter(data.Head.HeadYaw);
        data.Head.HeadPitch = headPitch.Filter(data.Head.HeadPitch);
        data.Head.HeadRoll = headRoll.Filter(data.Head.HeadRoll);
        data.Head.HeadPosX = headPosX.Filter(data.Head.HeadPosX);
        data.Head.HeadPosY = headPosY.Filter(data.Head.HeadPosY);
        data.Head.HeadPosZ = headPosZ.Filter(data.Head.HeadPosZ);
    }
}
