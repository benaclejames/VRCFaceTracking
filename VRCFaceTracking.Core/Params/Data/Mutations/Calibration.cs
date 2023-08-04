using Microsoft.Extensions.Logging;
using VRCFaceTracking.Core.Models;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Params.Expressions;

namespace VRCFaceTracking.Mutators;

public struct CalibrationData
{
    public float ceil;
    public float ceilPrev;
    public float ceilBuffer;
    public float confidence;
    public float lifeTime;
}

public class CalibrationMutator : IUnifiedMutation
{
    public string Name => "Calibration";
    public int Order => -100;
    public bool Mutable { get; set; }

    public CalibrationData[] cal = new CalibrationData[(int)UnifiedExpressions.Max];

    public float calibrationWeight = 0.6f;

    private const float LIFETIME_MIN = 0.8f;
    private const float LIFETIME_MAX = 1f;
    private const float LIFETIME_MULT = 1.002f;
    private const float LIFETIME_CONF_INFL = 0.003f;

    private const float ABS_CEIL = 1f;
    private const float ABS_FLOOR = 0f;

    private const float CONF_VALUE = 2f;
    private const float CONF_CURVE = .2f;

    static float SimpleLerp(float from, float to, float t) => to * t + from * (1f - t);

    public void Mutate(ref UnifiedTrackingData data, UnifiedTrackingData buffer, ILogger<UnifiedTrackingMutator> _logger)
    {
        if (calibrationWeight <= 0.0f) 
            return;
        if (cal == null)
            return;

        for (int i = 0; i < (int)UnifiedExpressions.Max; i++)
        {
            if (calibrationWeight > 0.0f && data.Shapes[i].Weight > cal[i].ceil)
            {
                var confidenceCurve = 1f/(float)(Math.Pow(CONF_VALUE, (float)Math.Abs(cal[i].ceil - cal[i].ceilPrev)));
                cal[i].confidence = SimpleLerp(cal[i].confidence, confidenceCurve, 1f-calibrationWeight);
                cal[i].ceilPrev = cal[i].ceil;
                cal[i].ceil = data.Shapes[i].Weight;
                cal[i].lifeTime *= (LIFETIME_MULT + (LIFETIME_CONF_INFL * cal[i].confidence));
               
                /*
                _logger.LogInformation("\n" + ((UnifiedExpressions)i).ToString() +
                                       "\nRaw: " + data.Shapes[i].Weight +
                                       "\nCalibrated: " + data.Shapes[i].Weight / cal[i].ceilBuffer + " Clamp Error: " + (1f - (data.Shapes[i].Weight / cal[i].ceilBuffer)) +
                                       "\nCeil: " + cal[i].ceil +
                                       "\nCeilPrev: " + cal[i].ceilPrev +
                                       "\nConfidence: " + cal[i].confidence +
                                       "\nCeilBuffer: " + cal[i].ceilBuffer +
                                       "\nConfidenceApplied: " + (1f/(1f+(float)Math.Pow(Math.E, (-60 * cal[i].confidence)+52f))) +
                                       "\nLifeTime: " + cal[i].lifeTime);
                */
            }

            var confidenceApplied = 1f / (1f + (float)Math.Pow(Math.E, (-60 * cal[i].confidence) + 52f));
            cal[i].ceilBuffer = SimpleLerp(ABS_CEIL, cal[i].ceil, confidenceApplied);

            // decay function
            if (cal[i].lifeTime < LIFETIME_MAX)
                cal[i].ceil = SimpleLerp(0f, cal[i].ceil, cal[i].lifeTime);

            data.Shapes[i].Weight = Math.Max(ABS_FLOOR, Math.Min(ABS_CEIL, data.Shapes[i].Weight * cal[i].ceil));
        }
    }

    private void ResetCalibration()
    {
        for (int i = 0; i < cal.Length; i++)
        {
            cal[i].ceil = 0f;
            cal[i].ceilBuffer = ABS_CEIL;
            cal[i].lifeTime = LIFETIME_MIN;
        }
    }

    public void Initialize() => Reset();
    public void Reset() => ResetCalibration();
    public object GetProperties() => calibrationWeight;
    public void SetProperties(object data) => 
        calibrationWeight = Convert.ToSingle(data);
}
