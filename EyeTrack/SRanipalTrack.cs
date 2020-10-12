using ViveSR.anipal.Eye;
using MelonLoader;
using UnityEngine;
using ViveSR.anipal;
using IEnumerator = System.Collections.IEnumerator;
using IntPtr = System.IntPtr;

public class SRanipalTrack
{

    private readonly SRanipal_Eye_Framework _framework = new SRanipal_Eye_Framework();

    public Vector3 GazeDirectionCombinedLocal;
    public float LeftOpenness;
    public float RightOpenness;
    public float Diameter;
    public float CombinedWiden;

    public float MaxOpen;
    public float MinOpen = 999;
    
    private EyeData_v2 _latestEyeData;

    public void Start()
    {
        _framework.EnableEye = true;
        _framework.EnableEyeDataCallback = false;
        _framework.EnableEyeVersion = SRanipal_Eye_Framework.SupportedEyeVersion.version1;
        _framework.StartFramework();
        SRanipal_API.Initial(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2, IntPtr.Zero);
        MelonCoroutines.Start(Update());
    }

    public void Stop()
    {
        MelonCoroutines.Stop(Update());
        _framework.StopFramework();
    }

    public IEnumerator Update()
    {
        for (;;)
        {
            if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
                yield return new WaitForSeconds(0.05f);

            SRanipal_Eye_API.GetEyeData_v2(ref _latestEyeData);

            Vector3 unused;
            /*Vector3 tempDirection;
            if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out unused, out tempDirection)) { }
            else if (SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out unused, out tempDirection)) { }
            else if (SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out unused, out tempDirection)) { }*/
            Vector3 reverseGaze = new Vector3(-1f, 1f, 1f);
           // GazeDirectionCombinedLocal = _latestEyeData.verbose_data.combined.eye_data.gaze_direction_normalized;
           SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out unused, out GazeDirectionCombinedLocal);

            LeftOpenness = _latestEyeData.verbose_data.left.eye_openness;
            RightOpenness = _latestEyeData.verbose_data.right.eye_openness;

            if (_latestEyeData.verbose_data.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
            {
                Diameter = _latestEyeData.verbose_data.right.pupil_diameter_mm;
                if (_latestEyeData.verbose_data.right.eye_openness >= 1f)
                    UpdateMinMaxDilation(Diameter);
            } else if (_latestEyeData.verbose_data.left.GetValidity(SingleEyeDataValidity
                .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY))
            {
                Diameter = _latestEyeData.verbose_data.left.pupil_diameter_mm;
                if (_latestEyeData.verbose_data.left.eye_openness >= 1f)
                    UpdateMinMaxDilation(Diameter);
            }

            CombinedWiden = _latestEyeData.expression_data.left.eye_wide > _latestEyeData.expression_data.right.eye_wide
                ? _latestEyeData.expression_data.left.eye_wide
                : _latestEyeData.expression_data.right.eye_wide;

            yield return new WaitForSeconds(0.005f);
        }
    }

    private void UpdateMinMaxDilation(float readDilation)
    {

        if (readDilation > MaxOpen)
            MaxOpen = readDilation;
        if (readDilation < MinOpen)
            MinOpen = readDilation;
    }
}