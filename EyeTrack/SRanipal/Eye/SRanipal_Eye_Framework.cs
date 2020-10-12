//========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ViveSR
{
    namespace anipal
    {
        namespace Eye
        {
            public class SRanipal_Eye_Framework : MonoBehaviour
            {
                public enum FrameworkStatus { STOP, START, WORKING, ERROR, NOT_SUPPORT }
                /// <summary>
                /// The status of the anipal engine.
                /// </summary>
                public static FrameworkStatus Status { get; protected set; }

                /// <summary>
                /// Currently supported lip motion prediction engine's version.
                /// </summary>
                public enum SupportedEyeVersion { version1, version2 }

                /// <summary>
                /// Whether to enable anipal's Eye module.
                /// </summary>
                public bool EnableEye = true;

                /// <summary>
                /// Whether to use callback to get data.
                /// </summary>
                public bool EnableEyeDataCallback = false;

                /// <summary>
                /// Which version of eye prediction engine will be used, default is version 1.
                /// </summary>
                public SupportedEyeVersion EnableEyeVersion = SupportedEyeVersion.version1;
                private static SRanipal_Eye_Framework Mgr = null;
                public static SRanipal_Eye_Framework Instance
                {
                    get
                    {
                        if (Mgr == null)
                        {
                            Mgr = FindObjectOfType<SRanipal_Eye_Framework>();
                        }
                        if (Mgr == null)
                        {
                            Debug.LogError("SRanipal_Eye_Framework not found");
                        }
                        return Mgr;
                    }
                }

                void Start()
                {
                    StartFramework();
                }

                void OnDestroy()
                {
                    StopFramework();
                }

                public void StartFramework()
                {
                    if (!EnableEye) return;
                    if (Status == FrameworkStatus.WORKING || Status == FrameworkStatus.NOT_SUPPORT) return;

                    if (EnableEyeVersion == SupportedEyeVersion.version1)
                    {
                        Error result = SRanipal_API.Initial(SRanipal_Eye.ANIPAL_TYPE_EYE, IntPtr.Zero);
                        if (result == Error.WORK)
                        {
                            Status = FrameworkStatus.WORKING;
                            Debug.Log("[SRanipal] Initial Eye success!");
                        }
                        else
                        {
                            if (result == Error.NOT_SUPPORT_EYE_TRACKING)
                            {
                                Status = FrameworkStatus.NOT_SUPPORT;
                                EnableEyeDataCallback = false;
                                Debug.Log("[SRanipal] Current HMD do not support eye tracking!");
                            }
                            else
                            {
                                Status = FrameworkStatus.ERROR;
                                Debug.LogError("[SRanipal] Initial Eye : " + result);
                            }
                        }
                    }
                    else
                    {
                        Error result = SRanipal_API.Initial(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2, IntPtr.Zero);
                        if (result == Error.WORK)
                        {
                            Status = FrameworkStatus.WORKING;
                            Debug.Log("[SRanipal] Initial Eye v2 success!");
                        }
                        else
                        {
                            if (result == Error.NOT_SUPPORT_EYE_TRACKING)
                            {
                                Status = FrameworkStatus.NOT_SUPPORT;
                                EnableEyeDataCallback = false;
                                Debug.Log("[SRanipal] Current HMD do not support eye tracking!");
                            }
                            else
                            {
                                Status = FrameworkStatus.ERROR;
                                Debug.LogError("[SRanipal] Initial Eye v2: " + result);
                            }
                        }
                    }
                }

                public void StopFramework()
                {
                    if (Status != FrameworkStatus.NOT_SUPPORT)
                    {
                        if (Status != FrameworkStatus.STOP)
                        {
                            if (EnableEyeVersion == SupportedEyeVersion.version1)
                            {
                                Error result = SRanipal_API.Release(SRanipal_Eye.ANIPAL_TYPE_EYE);
                                if (result == Error.WORK) Debug.Log("[SRanipal] Release Eye : " + result);
                                else Debug.LogError("[SRanipal] Release Eye : " + result);
                            }
                            else
                            {
                                Error result = SRanipal_API.Release(SRanipal_Eye_v2.ANIPAL_TYPE_EYE_V2);
                                if (result == Error.WORK) Debug.Log("[SRanipal] Release Eye v2: " + result);
                                else Debug.LogError("[SRanipal] Release Eye v2: " + result);
                            }
                        }
                        else
                        {
                            Debug.Log("[SRanipal] Stop Framework : module not on");
                        }
                    }
                    Status = FrameworkStatus.STOP;
                }
            }
        }
    }
}