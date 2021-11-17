using System;
using System.Linq;
using ParamLib;
using UnityEngine;

namespace VRCFaceTracking.Params.Eye
{
    public class FloatEyeParameter : FloatBaseParam, IParameter
    {
        public FloatEyeParameter(Func<EyeTrackingData, float> getValueFunc, string paramName, bool prioritised = false)
            : base(paramName, prioritised) =>
            UnifiedTrackingData.OnUnifiedParamsUpdated += (eye, lip, floats) => ParamValue = getValueFunc.Invoke(eye);

        public string[] GetName() => new[] {ParamName};
    }

    public class XYParameter : XYParam, IParameter
    {
        public XYParameter(Func<EyeTrackingData, Vector2?> getValueFunc, string xParamName, string yParamName)
            : base(new FloatBaseParam(xParamName, true), new FloatBaseParam(yParamName, true))
        {
            UnifiedTrackingData.OnUnifiedParamsUpdated += (eye, lip, floats) =>
            {
                var newValue = getValueFunc.Invoke(eye);
                if (newValue.HasValue) ParamValue = newValue.Value;
            };
        }

        void IParameter.ResetParam() => ResetParams();
        public void ZeroParam() => ZeroParams();
        public string[] GetName() => new[] {X.ParamName, Y.ParamName};

    }

    public class BoolEyeParameter : BoolBaseParam, IParameter
    {
        public BoolEyeParameter(Func<EyeTrackingData, bool> getValueFunc, string paramName) : base(paramName) =>
            UnifiedTrackingData.OnUnifiedParamsUpdated += (eye, lip, floats) => ParamValue = getValueFunc.Invoke(eye);

        public string[] GetName() => new [] {ParamName};
    }

    public class BinaryEyeParameter : IParameter
    {
        private readonly BoolEyeParameter[] _params = new BoolEyeParameter[4];
        
        public void ResetParam()
        {
            foreach (var param in _params)
                param.ResetParam();
        }

        public void ZeroParam()
        {
            foreach (var param in _params)
                param.ParamIndex = null;
        }

        public string[] GetName() => _params.Select(p => p.ParamName).Distinct().ToArray();

        public BinaryEyeParameter(Func<EyeTrackingData, float> getValueFunc, string paramName)
        {
            _params[0] = new BoolEyeParameter(data => (int) (getValueFunc.Invoke(data) * 15 + .5) % 2 == 1, paramName + "1");
            _params[1] = new BoolEyeParameter(data => (int) (getValueFunc.Invoke(data) * 15 + .5)/2 % 2 == 1, paramName + "2");
            _params[2] = new BoolEyeParameter(data => (int) (getValueFunc.Invoke(data) * 15 + .5)/4 % 2 == 1, paramName + "4");
            _params[3] = new BoolEyeParameter(data => (int) (getValueFunc.Invoke(data) * 15 + .5)/8 % 2 == 1, paramName + "8");
        }
    }
}