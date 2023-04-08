using VRCFaceTracking_Next.Core.Types;
using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Params
{
    public class XYParam
    {
        public OSCParams.BaseParam<float> X, Y;

        protected Vector2 ParamValue
        {
            set
            {
                X.ParamValue = value.x;
                Y.ParamValue = value.y;
            }
        }

        protected XYParam(OSCParams.BaseParam<float> x, OSCParams.BaseParam<float> y)
        {
            X = x;
            Y = y;
        }

        protected void ResetParams(ConfigParser.Parameter[] newParams)
        {
            X.ResetParam(newParams);
            Y.ResetParam(newParams);
        }
    }
}