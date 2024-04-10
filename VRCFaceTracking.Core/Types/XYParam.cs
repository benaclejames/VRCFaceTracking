using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.OSC.DataTypes;

namespace VRCFaceTracking.Core.Types
{
    [Obsolete("Use Vector2 instead")]
    public class XYParam
    {
        public BaseParam<float> X, Y;

        protected Vector2 ParamValue
        {
            set
            {
                X.ParamValue = value.x;
                Y.ParamValue = value.y;
            }
        }

        protected XYParam(BaseParam<float> x, BaseParam<float> y)
        {
            X = x;
            Y = y;
        }

        protected void ResetParams(IParameterDefinition[] newParams)
        {
            X.ResetParam(newParams);
            Y.ResetParam(newParams);
        }
    }
}