//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ViveSR
{
    namespace anipal
    {
        namespace Eye
        {
            public static class SRanipal_Eye_v2
            {
                public const int ANIPAL_TYPE_EYE_V2 = 2;
                public delegate void CallbackBasic(ref EyeData_v2 data);

                /// <summary>
                /// Register a callback function to receive eye camera related data when the module has new outputs.
                /// </summary>
                /// <param name="callback">function pointer of callback</param>
                /// <returns>error code. please refer Error in ViveSR_Enums.h.</returns>
                public static int WrapperRegisterEyeDataCallback(System.IntPtr callback)
                {
                    return SRanipal_Eye_API.RegisterEyeDataCallback_v2(callback);
                }

                /// <summary>
                /// Unregister a callback function to receive eye camera related data when the module has new outputs.
                /// </summary>
                /// <param name="callback">function pointer of callback</param>
                /// <returns>error code. please refer Error in ViveSR_Enums.h.</returns>
                public static int WrapperUnRegisterEyeDataCallback(System.IntPtr callback)
                {
                    return SRanipal_Eye_API.UnregisterEyeDataCallback_v2(callback);
                }

                public const int WeightingCount = (int)EyeShape_v2.Max;
                private static EyeData_v2 EyeData_ = new EyeData_v2();
                private static int LastUpdateFrame = -1;
                private static Error LastUpdateResult = Error.FAILED;
                private static Dictionary<EyeShape_v2, float> Weightings;

                static SRanipal_Eye_v2()
                {
                    Weightings = new Dictionary<EyeShape_v2, float>();
                    for (int i = 0; i < WeightingCount; ++i) Weightings.Add((EyeShape_v2)i, 0.0f);
                }
                private static bool UpdateData()
                {
                    if (Time.frameCount == LastUpdateFrame) return LastUpdateResult == Error.WORK;
                    else LastUpdateFrame = Time.frameCount;
                    LastUpdateResult = SRanipal_Eye_API.GetEyeData_v2(ref EyeData_);

                    //Debug.Log("[EYE V2] LeftWide = " + EyeData_.verbose_data.left.eye_wide);
                    //Debug.Log("[EYE V2] RightWide = " + EyeData_.verbose_data.right.eye_wide);

                    return LastUpdateResult == Error.WORK;
                }

                /// <summary>
                /// Gets the VerboseData of anipal's Eye module when enable eye callback function.
                /// </summary>
                /// <param name="data">ViveSR.anipal.Eye.VerboseData</param>
                /// <param name="data">ViveSR.anipal.Eye.EyeData_v2</param>
                /// <returns>Indicates whether the data received is new.</returns>
                public static bool GetVerboseData(out VerboseData data, EyeData_v2 eye_data)
                {
                    data = eye_data.verbose_data;
                    return true;
                }

                /// <summary>
                /// Gets the VerboseData of anipal's Eye module.
                /// </summary>
                /// <param name="data">ViveSR.anipal.Eye.VerboseData</param>
                /// <returns>Indicates whether the data received is new.</returns>
                public static bool GetVerboseData(out VerboseData data)
                {
                    UpdateData();
                    return GetVerboseData(out data, EyeData_);
                }

                /// <summary>
                /// Gets the openness value of an eye when enable eye callback function.
                /// </summary>
                /// <param name="eye">The index of an eye.</param>
                /// <param name="openness">The openness value of an eye, clamped between 0 (fully closed) and 1 (fully open). </param>
                /// <param name="eye_data">ViveSR.anipal.Eye.EyeData_v2. </param>
                /// <returns>Indicates whether the openness value received is valid.</returns>
                public static bool GetEyeOpenness(EyeIndex eye, out float openness, EyeData_v2 eye_data)
                {
                    if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING)
                    {
                        SingleEyeData eyeData = eye == EyeIndex.LEFT ? eye_data.verbose_data.left : eye_data.verbose_data.right;
                        bool valid = eyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY);
                        openness = valid ? eyeData.eye_openness : 0;
                    }
                    else
                    {
                        // If not support eye tracking, set default to open.
                        openness = 1;
                    }
                    return true;
                }

                /// <summary>
                /// Gets the openness value of an eye.
                /// </summary>
                /// <param name="eye">The index of an eye.</param>
                /// <param name="openness">The openness value of an eye, clamped between 0 (fully closed) and 1 (fully open). </param>
                /// <returns>Indicates whether the openness value received is valid.</returns>
                public static bool GetEyeOpenness(EyeIndex eye, out float openness)
                {
                    UpdateData();
                    return GetEyeOpenness(eye, out openness, EyeData_);
                }

                /// <summary>
                /// Gets weighting values from anipal's Eye module when enable eye callback function.
                /// </summary>
                /// <param name="shapes">Weighting values obtained from anipal's Eye module.</param>
                /// <param name="eye_data">ViveSR.anipal.Eye.EyeData_v2. </param>
                /// <returns>Indicates whether the values received are new.</returns>\
                /// 
                public static bool GetEyeWeightings(out Dictionary<EyeShape_v2, float> shapes, EyeData_v2 eye_data)
                {
                    float[] openness = new float[2];
                    bool[] valid = new bool[2];
                    Vector2[] pupilPosition = new Vector2[2];

                    foreach (EyeIndex index in (EyeIndex[])Enum.GetValues(typeof(EyeIndex)))
                    {
                        GetEyeOpenness(index, out openness[(int)index], eye_data);
                        valid[(int)index] = GetPupilPosition(index, out pupilPosition[(int)index]);
                    }

                    float[] weighting_up = new float[3] { Mathf.Max(pupilPosition[(int)GazeIndex.LEFT].y, 0f), Mathf.Max(pupilPosition[(int)GazeIndex.RIGHT].y, 0f), 0 };
                    float[] weighting_down = new float[3] { Mathf.Max(-pupilPosition[(int)GazeIndex.LEFT].y, 0f), Mathf.Max(-pupilPosition[(int)GazeIndex.RIGHT].y, 0f), 0 };
                    float[] weighting_left = new float[3] { Mathf.Max(-pupilPosition[(int)GazeIndex.LEFT].x, 0f), Mathf.Max(-pupilPosition[(int)GazeIndex.RIGHT].x, 0f), 0 };
                    float[] weighting_right = new float[3] { Mathf.Max(pupilPosition[(int)GazeIndex.LEFT].x, 0f), Mathf.Max(pupilPosition[(int)GazeIndex.RIGHT].x, 0f), 0 };
                    weighting_up[(int)GazeIndex.COMBINE] = (weighting_up[(int)GazeIndex.LEFT] + weighting_up[(int)GazeIndex.RIGHT]) / 2;
                    weighting_down[(int)GazeIndex.COMBINE] = (weighting_down[(int)GazeIndex.LEFT] + weighting_down[(int)GazeIndex.RIGHT]) / 2;
                    weighting_left[(int)GazeIndex.COMBINE] = (weighting_left[(int)GazeIndex.LEFT] + weighting_left[(int)GazeIndex.RIGHT]) / 2;
                    weighting_right[(int)GazeIndex.COMBINE] = (weighting_right[(int)GazeIndex.LEFT] + weighting_right[(int)GazeIndex.RIGHT]) / 2;

                    foreach (EyeShape_v2 index in (EyeShape_v2[])Enum.GetValues(typeof(EyeShape_v2)))
                    {
                        Weightings[index] = 0;
                    }
                    Weightings[EyeShape_v2.Eye_Left_Blink] = 1 - openness[(int)EyeIndex.LEFT];
                    Weightings[EyeShape_v2.Eye_Right_Blink] = 1 - openness[(int)EyeIndex.RIGHT];
                    Weightings[EyeShape_v2.Eye_Left_Wide] = EyeData_.expression_data.left.eye_wide;
                    Weightings[EyeShape_v2.Eye_Right_Wide] = EyeData_.expression_data.right.eye_wide;

                    Weightings[EyeShape_v2.Eye_Left_Squeeze] =  EyeData_.expression_data.left.eye_squeeze;
                    Weightings[EyeShape_v2.Eye_Right_Squeeze] = EyeData_.expression_data.right.eye_squeeze;

                    if (valid[(int)EyeIndex.LEFT] && valid[(int)EyeIndex.RIGHT])
                    {
                        Ray gaze_ray = new Ray();
                        GetGazeRay(GazeIndex.COMBINE, out gaze_ray, eye_data);
                        Vector3 gaze_direction = gaze_ray.direction - gaze_ray.origin;
                        gaze_direction.x = 0.0f;
                        Vector3 gaze_direction_normalized = gaze_direction.normalized;
                        Vector3 gaze_axis_z = Vector3.forward;
                        float y_weight = Mathf.Acos(Vector3.Dot(gaze_direction_normalized, gaze_axis_z));

                        Weightings[EyeShape_v2.Eye_Left_Up] = EyeData_.expression_data.left.eye_wide;
                        Weightings[EyeShape_v2.Eye_Left_Down] = gaze_direction_normalized.y < 0 ? y_weight : 0;
                        Weightings[EyeShape_v2.Eye_Left_Left] = weighting_left[(int)GazeIndex.COMBINE];
                        Weightings[EyeShape_v2.Eye_Left_Right] = weighting_right[(int)GazeIndex.COMBINE];

                        Weightings[EyeShape_v2.Eye_Right_Up] = EyeData_.expression_data.right.eye_wide;
                        Weightings[EyeShape_v2.Eye_Right_Down] = gaze_direction_normalized.y < 0 ? y_weight : 0;
                        Weightings[EyeShape_v2.Eye_Right_Left] = weighting_left[(int)GazeIndex.COMBINE];
                        Weightings[EyeShape_v2.Eye_Right_Right] = weighting_right[(int)GazeIndex.COMBINE];
                    }
                    else if (valid[(int)EyeIndex.LEFT])
                    {
                        Ray gaze_ray = new Ray();
                        GetGazeRay(GazeIndex.LEFT, out gaze_ray, eye_data);
                        Vector3 gaze_direction = gaze_ray.direction - gaze_ray.origin;
                        gaze_direction.x = 0.0f;
                        Vector3 gaze_direction_normalized = gaze_direction.normalized;
                        Vector3 gaze_axis_z = Vector3.forward;
                        float y_weight = Mathf.Acos(Vector3.Dot(gaze_direction_normalized, gaze_axis_z));

                        Weightings[EyeShape_v2.Eye_Left_Up] = EyeData_.expression_data.left.eye_wide;
                        Weightings[EyeShape_v2.Eye_Left_Down] = gaze_direction_normalized.y < 0 ? y_weight : 0;
                        Weightings[EyeShape_v2.Eye_Left_Left] = weighting_left[(int)GazeIndex.LEFT];
                        Weightings[EyeShape_v2.Eye_Left_Right] = weighting_right[(int)GazeIndex.LEFT];
                    }
                    else if (valid[(int)EyeIndex.RIGHT])
                    {
                        Ray gaze_ray = new Ray();
                        GetGazeRay(GazeIndex.RIGHT, out gaze_ray, eye_data);
                        Vector3 gaze_direction = gaze_ray.direction - gaze_ray.origin;
                        gaze_direction.x = 0.0f;
                        Vector3 gaze_direction_normalized = gaze_direction.normalized;
                        Vector3 gaze_axis_z = Vector3.forward;
                        float y_weight = Mathf.Acos(Vector3.Dot(gaze_direction_normalized, gaze_axis_z));

                        Weightings[EyeShape_v2.Eye_Right_Up] = EyeData_.expression_data.right.eye_wide;
                        Weightings[EyeShape_v2.Eye_Right_Down] = gaze_direction_normalized.y < 0 ? y_weight : 0;
                        Weightings[EyeShape_v2.Eye_Right_Left] = weighting_left[(int)GazeIndex.RIGHT];
                        Weightings[EyeShape_v2.Eye_Right_Right] = weighting_right[(int)GazeIndex.RIGHT];
                    }
                    shapes = Weightings;
                    return true;

                }

                /// <summary>
                /// Gets weighting values from anipal's Eye module.
                /// </summary>
                /// <param name="shapes">Weighting values obtained from anipal's Eye module.</param>
                /// <returns>Indicates whether the values received are new.</returns>\
                public static bool GetEyeWeightings(out Dictionary<EyeShape_v2, float> shapes)
                {
                    UpdateData();
                    return GetEyeWeightings(out shapes, EyeData_);
                }

                /// <summary>
                /// Tests eye gaze data when enable eye callback function.
                /// </summary>
                /// <param name="validity">A type of eye gaze data to test with.</param>
                /// <param name="gazeIndex">The index of a source of eye gaze data.</param>
                /// <param name="eye_data">ViveSR.anipal.Eye.EyeData_v2. </param>
                /// <returns>Indicates whether a source of eye gaze data is found.</returns>
                public static bool TryGaze(SingleEyeDataValidity validity, out GazeIndex gazeIndex, EyeData_v2 eye_data)
                {
                    bool[] valid = new bool[(int)GazeIndex.COMBINE + 1] { eye_data.verbose_data.left.GetValidity(validity),
                                                                          eye_data.verbose_data.right.GetValidity(validity),
                                                                          eye_data.verbose_data.combined.eye_data.GetValidity(validity)};
                    gazeIndex = GazeIndex.COMBINE;
                    for (int i = (int)GazeIndex.COMBINE; i >= 0; --i)
                    {
                        if (valid[i])
                        {
                            gazeIndex = (GazeIndex)i;
                            return true;
                        }
                    }
                    return false;
                }


                /// <summary>
                /// Tests eye gaze data.
                /// </summary>
                /// <param name="validity">A type of eye gaze data to test with.</param>
                /// <param name="gazeIndex">The index of a source of eye gaze data.</param>
                /// <returns>Indicates whether a source of eye gaze data is found.</returns>
                public static bool TryGaze(SingleEyeDataValidity validity, out GazeIndex gazeIndex)
                {
                    UpdateData();
                    return TryGaze(validity, out gazeIndex, EyeData_);
                }

                /// <summary>
                /// Gets the gaze ray of a source of eye gaze data when enable eye callback function.
                /// </summary>
                /// <param name="gazeIndex">The index of a source of eye gaze data.</param>
                /// <param name="origin">The starting point of the ray in local coordinates.</param>
                /// <param name="direction">Tthe direction of the ray.</param>
                /// <param name="eye_data">ViveSR.anipal.Eye.EyeData_v2. </param>
                /// <returns>Indicates whether the eye gaze data received is valid.</returns>
                public static bool GetGazeRay(GazeIndex gazeIndex, out Vector3 origin, out Vector3 direction, EyeData_v2 eye_data)
                {
                    bool valid = false;
                    origin = Vector3.zero;
                    direction = Vector3.forward;
                    if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING)
                    {
                        origin = Camera.main.transform.position;
                        valid = true;
                    }
                    else
                    {
                        SingleEyeData[] eyesData = new SingleEyeData[(int)GazeIndex.COMBINE + 1];
                        eyesData[(int)GazeIndex.LEFT] = eye_data.verbose_data.left;
                        eyesData[(int)GazeIndex.RIGHT] = eye_data.verbose_data.right;
                        eyesData[(int)GazeIndex.COMBINE] = eye_data.verbose_data.combined.eye_data;

                        if (gazeIndex == GazeIndex.COMBINE)
                        {
                            valid = eyesData[(int)GazeIndex.COMBINE].GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY);
                            if (valid)
                            {
                                origin = eyesData[(int)GazeIndex.COMBINE].gaze_origin_mm * 0.001f;
                                direction = eyesData[(int)GazeIndex.COMBINE].gaze_direction_normalized;
                                direction.x *= -1;
                            }
                        }
                        else if (gazeIndex == GazeIndex.LEFT || gazeIndex == GazeIndex.RIGHT)
                        {
                            valid = eyesData[(int)gazeIndex].GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY);
                            if (valid)
                            {
                                origin = eyesData[(int)gazeIndex].gaze_origin_mm * 0.001f;
                                direction = eyesData[(int)gazeIndex].gaze_direction_normalized;
                                origin.x *= -1;
                                direction.x *= -1;
                            }
                        }
                    }
                    return valid;
                }

                /// <summary>
                /// Gets the gaze ray of a source of eye gaze data.
                /// </summary>
                /// <param name="gazeIndex">The index of a source of eye gaze data.</param>
                /// <param name="origin">The starting point of the ray in local coordinates.</param>
                /// <param name="direction">Tthe direction of the ray.</param>
                /// <returns>Indicates whether the eye gaze data received is valid.</returns>
                public static bool GetGazeRay(GazeIndex gazeIndex, out Vector3 origin, out Vector3 direction)
                {
                    UpdateData();
                    return GetGazeRay(gazeIndex, out origin, out direction, EyeData_);
                }

                /// <summary>
                /// Gets the gaze ray data of a source eye gaze data when enable eye callback function.
                /// </summary>
                /// <param name="gazeIndex">The index of a source of eye gaze data.</param>
                /// <param name="ray">The starting point and direction of the ray.</param>
                /// <param name="eye_data">ViveSR.anipal.Eye.EyeData_v2. </param>
                /// <returns>Indicates whether the gaze ray data received is valid.</returns>
                public static bool GetGazeRay(GazeIndex gazeIndex, out Ray ray, EyeData_v2 eye_data)
                {
                    Vector3 origin = Vector3.zero;
                    Vector3 direction = Vector3.forward;
                    bool valid = false;
                    if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING)
                    {
                        valid = GetGazeRay(gazeIndex, out origin, out direction, eye_data);
                    }
                    else if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
                    {
                        origin = Camera.main.transform.position;
                        valid = true;
                    }
                    ray = new Ray(origin, direction);
                    return valid;
                }

                /// <summary>
                /// Gets the gaze ray data of a source eye gaze data.
                /// </summary>
                /// <param name="gazeIndex">The index of a source of eye gaze data.</param>
                /// <param name="ray">The starting point and direction of the ray.</param>
                /// <returns>Indicates whether the gaze ray data received is valid.</returns>
                public static bool GetGazeRay(GazeIndex gazeIndex, out Ray ray)
                {
                    UpdateData();
                    return GetGazeRay(gazeIndex, out ray, EyeData_);
                }

                /// <summary>
                /// Casts a ray against all colliders when enable eye callback function.
                /// </summary>
                /// <param name="index">A source of eye gaze data.</param>
                /// <param name="ray">The starting point and direction of the ray.</param>
                /// <param name="focusInfo">Information about where the ray focused on.</param>
                /// <param name="radius">The radius of the gaze ray</param>
                /// <param name="maxDistance">The max length of the ray.</param>
                /// <param name="focusableLayer">A layer id that is used to selectively ignore object.</param>
                /// <param name="eye_data">ViveSR.anipal.Eye.EyeData_v2. </param>
                /// <returns>Indicates whether the ray hits a collider.</returns>
                public static bool Focus(GazeIndex index, out Ray ray, out FocusInfo focusInfo, float radius, float maxDistance, int focusableLayer, EyeData_v2 eye_data)
                {
                    bool valid = GetGazeRay(index, out ray, eye_data);
                    if (valid)
                    {
                        Ray rayGlobal = new Ray(Camera.main.transform.position, Camera.main.transform.TransformDirection(ray.direction));
                        RaycastHit hit;
                        if (radius == 0) valid = Physics.Raycast(rayGlobal, out hit, maxDistance, focusableLayer);
                        else valid = Physics.SphereCast(rayGlobal, radius, out hit, maxDistance, focusableLayer);
                        focusInfo = new FocusInfo
                        {
                            point = hit.point,
                            normal = hit.normal,
                            distance = hit.distance,
                            collider = hit.collider,
                            rigidbody = hit.rigidbody,
                            transform = hit.transform
                        };
                    }
                    else
                    {
                        focusInfo = new FocusInfo();
                    }
                    return valid;
                }

                /// <summary>
                /// Casts a ray against all colliders.
                /// </summary>
                /// <param name="index">A source of eye gaze data.</param>
                /// <param name="ray">The starting point and direction of the ray.</param>
                /// <param name="focusInfo">Information about where the ray focused on.</param>
                /// <param name="radius">The radius of the gaze ray</param>
                /// <param name="maxDistance">The max length of the ray.</param>
                /// <param name="focusableLayer">A layer id that is used to selectively ignore object.</param>
                /// <returns>Indicates whether the ray hits a collider.</returns>
                public static bool Focus(GazeIndex index, out Ray ray, out FocusInfo focusInfo, float radius, float maxDistance, int focusableLayer)
                {
                    UpdateData();
                    return Focus(index, out ray, out focusInfo, radius, maxDistance, focusableLayer, EyeData_);
                }

                /// <summary>
                /// Casts a ray against all colliders.
                /// </summary>
                /// <param name="index">A source of eye gaze data.</param>
                /// <param name="ray">The starting point and direction of the ray.</param>
                /// <param name="focusInfo">Information about where the ray focused on.</param>
                /// <param name="radius">The radius of the gaze ray</param>
                /// <param name="maxDistance">The max length of the ray.</param>
                /// <param name="eye_data">ViveSR.anipal.Eye.EyeData_v2. </param>
                /// <returns>Indicates whether the ray hits a collider.</returns>
                public static bool Focus(GazeIndex index, out Ray ray, out FocusInfo focusInfo, float radius, float maxDistance, EyeData_v2 eye_data)
                {
                    return Focus(index, out ray, out focusInfo, radius, maxDistance, -1, eye_data);
                }

                /// <summary>
                /// Casts a ray against all colliders.
                /// </summary>
                /// <param name="index">A source of eye gaze data.</param>
                /// <param name="ray">The starting point and direction of the ray.</param>
                /// <param name="focusInfo">Information about where the ray focused on.</param>
                /// <param name="radius">The radius of the gaze ray</param>
                /// <param name="maxDistance">The max length of the ray.</param>
                /// <returns>Indicates whether the ray hits a collider.</returns>
                public static bool Focus(GazeIndex index, out Ray ray, out FocusInfo focusInfo, float radius, float maxDistance)
                {
                    UpdateData();
                    return Focus(index, out ray, out focusInfo, radius, maxDistance, EyeData_);
                }

                /// <summary>
                /// Casts a ray against all colliders when enable eye callback function.
                /// </summary>
                /// <param name="index">A source of eye gaze data.</param>
                /// <param name="ray">The starting point and direction of the ray.</param>
                /// <param name="focusInfo">Information about where the ray focused on.</param>
                /// <param name="maxDistance">The max length of the ray.</param>
                /// <param name="eye_data">ViveSR.anipal.Eye.EyeData_v2. </param>
                /// <returns>Indicates whether the ray hits a collider.</returns>
                public static bool Focus(GazeIndex index, out Ray ray, out FocusInfo focusInfo, float maxDistance, EyeData_v2 eye_data)
                {
                    return Focus(index, out ray, out focusInfo, 0, float.MaxValue, -1, eye_data);
                }

                /// <summary>
                /// Casts a ray against all colliders.
                /// </summary>
                /// <param name="index">A source of eye gaze data.</param>
                /// <param name="ray">The starting point and direction of the ray.</param>
                /// <param name="focusInfo">Information about where the ray focused on.</param>
                /// <param name="maxDistance">The max length of the ray.</param>
                /// <returns>Indicates whether the ray hits a collider.</returns>
                public static bool Focus(GazeIndex index, out Ray ray, out FocusInfo focusInfo, float maxDistance)
                {
                    UpdateData();
                    return Focus(index, out ray, out focusInfo, maxDistance, EyeData_);
                }

                /// <summary>
                /// Casts a ray against all colliders when enable eye callback function.
                /// </summary>
                /// <param name="index">A source of eye gaze data.</param>
                /// <param name="ray">The starting point and direction of the ray.</param>
                /// <param name="focusInfo">Information about where the ray focused on.</param>
                /// <param name="eye_data">ViveSR.anipal.Eye.EyeData_v2. </param>
                /// <returns>Indicates whether the ray hits a collider.</returns>
                public static bool Focus(GazeIndex index, out Ray ray, out FocusInfo focusInfo, EyeData_v2 eye_data)
                {
                    return Focus(index, out ray, out focusInfo, 0, float.MaxValue, -1, eye_data);
                }

                /// <summary>
                /// Casts a ray against all colliders.
                /// </summary>
                /// <param name="index">A source of eye gaze data.</param>
                /// <param name="ray">The starting point and direction of the ray.</param>
                /// <param name="focusInfo">Information about where the ray focused on.</param>
                /// <returns>Indicates whether the ray hits a collider.</returns>
                public static bool Focus(GazeIndex index, out Ray ray, out FocusInfo focusInfo)
                {
                    UpdateData();
                    return Focus(index, out ray, out focusInfo, EyeData_);
                }

                /// <summary>
                /// Gets the 2D position of a selected pupil when enable eye callback function.
                /// </summary>
                /// <param name="eye">The index of an eye.</param>
                /// <param name="postion">The 2D position of a selected pupil clamped between -1 and 1.
                /// Position (0, 0) indicates that the pupil is looking forward;
                /// position (1, 1) up-rightward; and
                /// position (-1, -1) left-downward.</param>
                /// <param name="eye_data">ViveSR.anipal.Eye.EyeData_v2. </param>
                /// <returns></returns>
                public static bool GetPupilPosition(EyeIndex eye, out Vector2 postion, EyeData_v2 eye_data)
                {
                    bool valid = false;
                    if (SRanipal_Eye_Framework.Status == SRanipal_Eye_Framework.FrameworkStatus.WORKING)
                    {
                        SingleEyeData eyeData = eye == EyeIndex.LEFT ? eye_data.verbose_data.left : eye_data.verbose_data.right;
                        valid = eyeData.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY);
                        postion = valid ? postion = new Vector2(eyeData.pupil_position_in_sensor_area.x * 2 - 1,
                                                                eyeData.pupil_position_in_sensor_area.y * -2 + 1) : Vector2.zero;

                    }
                    else
                    {
                        // If not support eye tracking, set default in middle.
                        postion = Vector2.zero;
                        valid = true;
                    }
                    return valid;
                }

                /// <summary>
                /// Gets the 2D position of a selected pupil.
                /// </summary>
                /// <param name="eye">The index of an eye.</param>
                /// <param name="postion">The 2D position of a selected pupil clamped between -1 and 1.
                /// Position (0, 0) indicates that the pupil is looking forward;
                /// position (1, 1) up-rightward; and
                /// position (-1, -1) left-downward.</param>
                /// <returns></returns>
                public static bool GetPupilPosition(EyeIndex eye, out Vector2 postion)
                {
                    UpdateData();
                    return GetPupilPosition(eye, out postion, EyeData_);
                }


                /// <summary>
                /// Launches anipal's Eye Calibration feature (an overlay program).
                /// </summary>
                /// <returns>Indicates the resulting ViveSR.Error status of this method.</returns>
                public static bool LaunchEyeCalibration()
                {
                    int result = SRanipal_Eye_API.LaunchEyeCalibration(IntPtr.Zero);
                    return result == (int)Error.WORK;
                }
            }
        }
    }
}