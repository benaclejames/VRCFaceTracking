using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Params
{
    public class XYParam
    {
        public OSCBaseParams.FloatBaseParam X, Y;

        protected Vector2 ParamValue
        {
            set
            {
                X.ParamValue = value.x;
                Y.ParamValue = value.y;
            }
        }

        protected XYParam(OSCBaseParams.FloatBaseParam x, OSCBaseParams.FloatBaseParam y)
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