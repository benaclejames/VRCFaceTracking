using System;
using System.Collections.Generic;
using MelonLoader;
using ViveSR.anipal.Lip;

namespace VRCFaceTracking.Params.Lip
{
    public class AvrageShape : ILipMerger
    {
        private readonly LipShape_v2[] shapes;

        public AvrageShape(params LipShape_v2[] shapes)
        {
            this.shapes = shapes;
            if (shapes.Length == 0)
                throw new Exception("an LipAvrageShape must have atleast 1 shape");
        }

        public float GetBlendedLipShape(Dictionary<LipShape_v2, float> inputMap)
        {
            return AvrageUtils.GetAvrageOfShapes(shapes, inputMap);
        }
    }

}
