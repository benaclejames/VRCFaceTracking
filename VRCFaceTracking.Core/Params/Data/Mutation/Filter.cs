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
    [MutationProperty("Minimum Cutoff", true, 0f, 2f)]
    public float minCutoff = 1f;
    [MutationProperty("Beta", true)]
    public float beta = 0.5f;
    [MutationProperty("Derivative Cutoff", true)]
    public float dCutoff = 0.1f;
    private const float hz = 10f;

    public class EuroFilter
    {
        public const float PI = (float)Math.PI;

        float xPrev;
        float dxPrev;

        public float Filter(float x, float minCutoff, float beta, float dCutoff)
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

    public Filter()
    {
        _isActive = false;
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
            data.Shapes[i].Weight = ApplyFilter(shapes[i], data.Shapes[i].Weight);

        data.Eye.Left.Openness = ApplyFilter(opennessLeft, data.Eye.Left.Openness);
        data.Eye.Left.PupilDiameter_MM = ApplyFilter(pupilLeft, data.Eye.Left.PupilDiameter_MM);
        data.Eye.Left.Gaze.x = ApplyFilter(gazeLeftX, data.Eye.Left.Gaze.x);
        data.Eye.Left.Gaze.y = ApplyFilter(gazeLeftY, data.Eye.Left.Gaze.y);

        data.Eye.Right.Openness = ApplyFilter(opennessRight, data.Eye.Right.Openness);
        data.Eye.Right.PupilDiameter_MM = ApplyFilter(pupilRight, data.Eye.Right.PupilDiameter_MM);
        data.Eye.Right.Gaze.x = ApplyFilter(gazeRightX, data.Eye.Right.Gaze.x);
        data.Eye.Right.Gaze.y = ApplyFilter(gazeRightY, data.Eye.Right.Gaze.y);

        data.Head.HeadYaw = ApplyFilter(headYaw, data.Head.HeadYaw);
        data.Head.HeadPitch = ApplyFilter(headPitch, data.Head.HeadPitch);
        data.Head.HeadRoll = ApplyFilter(headRoll, data.Head.HeadRoll);
        data.Head.HeadPosX = ApplyFilter(headPosX, data.Head.HeadPosX);
        data.Head.HeadPosY = ApplyFilter(headPosY, data.Head.HeadPosY);
        data.Head.HeadPosZ = ApplyFilter(headPosZ, data.Head.HeadPosZ);
    }

    private float ApplyFilter(EuroFilter filter, float x) => filter.Filter(x, minCutoff, beta, dCutoff);
}
