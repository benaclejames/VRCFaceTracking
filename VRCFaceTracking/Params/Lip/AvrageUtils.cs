using System.Collections.Generic;
using ViveSR.anipal.Lip;
using System;
using MelonLoader;

namespace VRCFaceTracking.Params.Lip
{
    class AvrageUtils
    {
        public static float GetAvrageOfShapes(LipShape_v2[] Shapes, Dictionary<LipShape_v2, float> inputMap)
        {
            int ProcessedShapes = 0;
            float buffer = 0f;
            for (int i = 0; i < Shapes.Length; i++)
            {
                LipShape_v2 key = Shapes[i];
                if (inputMap.ContainsKey(key))
                {
                    ProcessedShapes++;
                    buffer += inputMap[key];
                }
                else
                {
                    MelonLogger.Error("Shape not found: " + key);
                }
            }

            try
            {
                buffer /= ProcessedShapes;
                return buffer;
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
                return buffer;
            }
        }
    }
}
