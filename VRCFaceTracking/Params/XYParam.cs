using ParamLib;

namespace VRCFaceTracking.Params
{
    public class XYParam
    {
        public FloatBaseParam X, Y;

        protected Vector2 ParamValue
        {
            set
            {
                X.ParamValue = value.x;
                Y.ParamValue = value.y;
            }
        }

        protected XYParam(FloatBaseParam x, FloatBaseParam y)
        {
            X = x;
            Y = y;
        }

        protected void ResetParams()
        {
            X.ResetParam();
            Y.ResetParam();
        }

        protected void ZeroParams()
        {
            X.ZeroParam();
            Y.ZeroParam();
        }
    }
}