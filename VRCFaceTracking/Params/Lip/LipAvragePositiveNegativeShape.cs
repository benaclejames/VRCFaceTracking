using System.Collections.Generic;
using ViveSR.anipal.Lip;
using System;

namespace VRCFaceTracking.Params.Lip
{
    public class AvragePositiveNegativeShape : ILipMerger
    {
        private readonly LipShape_v2[] positiveShapes;
        private readonly LipShape_v2[] negativeShapes;
        public AvragePositiveNegativeShape(LipShape_v2[] positiveShapes, LipShape_v2[] negativeShapes)
        {
            this.positiveShapes = positiveShapes;
            this.negativeShapes = negativeShapes;

            if (positiveShapes.Length == 0 | negativeShapes.Length == 0)
                throw new Exception("an AvragePositiveNegativeShape must have atleast 1 positive and 1 negative shape");
        }

        public float GetBlendedLipShape(Dictionary<LipShape_v2, float> inputMap)
        {
            return AvrageUtils.GetAvrageOfShapes(positiveShapes, inputMap) + (AvrageUtils.GetAvrageOfShapes(negativeShapes, inputMap) * -1);
        }
    }
}