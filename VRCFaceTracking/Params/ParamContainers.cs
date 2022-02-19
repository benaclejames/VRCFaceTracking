using System;
using System.Collections.Generic;
using ParamLib;
using ViveSR.anipal.Lip;

namespace VRCFaceTracking.Params
{
    public class FloatParameter : FloatBaseParam, IParameter
    {
        public FloatParameter(Func<EyeTrackingData, Dictionary<LipShape_v2, float>, float?> getValueFunc,
            string paramName, bool wantsPriority = false)
            : base(paramName, wantsPriority) =>
            UnifiedTrackingData.OnUnifiedParamsUpdated += (eye, lip) =>
            {
                //if (!UnifiedLibManager.EyeEnabled && !UnifiedLibManager.LipEnabled) return;
                var value = getValueFunc.Invoke(eye, lip);
                if (value.HasValue)
                    ParamValue = value.Value;
            };

        public string[] GetName() => new[] {ParamName};
        public BaseParam[] GetBase()
        {
            return new[] {this};
        }
    }

    public class XYParameter : XYParam, IParameter
    {
        public XYParameter(Func<EyeTrackingData, Dictionary<LipShape_v2, float>, Vector2?> getValueFunc, string xParamName, string yParamName)
            : base(new FloatBaseParam(xParamName, true), new FloatBaseParam(yParamName, true)) =>
            UnifiedTrackingData.OnUnifiedParamsUpdated += (eye, lip) =>
            {
                if (!UnifiedLibManager.EyeEnabled && !UnifiedLibManager.LipEnabled) return;
                var value = getValueFunc.Invoke(eye, lip);
                if (value.HasValue)
                    ParamValue = value.Value;
            };

        public XYParameter(Func<EyeTrackingData, Vector2> getValueFunc, string xParamName, string yParamName)
            : this((eye, lip) => getValueFunc.Invoke(eye), xParamName, yParamName)
        {
        }

        public string[] GetName() => new[] {X.ParamName, Y.ParamName};

        public void ResetParam() => ResetParams();

        public void ZeroParam() => ZeroParams();
        public BaseParam[] GetBase()
        {
            return new[] {X, Y};
        }
    }

    public class BoolParameter : BoolBaseParam, IParameter
    {
        public BoolParameter(Func<EyeTrackingData, Dictionary<LipShape_v2, float>, bool?> getValueFunc,
            string paramName) : base(paramName) =>
            UnifiedTrackingData.OnUnifiedParamsUpdated += (eye, lip) =>
            {
                #if DLL
                if (!UnifiedLibManager.EyeEnabled && !UnifiedLibManager.LipEnabled) return;
                var value = getValueFunc.Invoke(eye, lip);
                if (value.HasValue)
                    ParamValue = value.Value;
                #endif
            };

        public BoolParameter(Func<EyeTrackingData, bool> getValueFunc, string paramName) : this(
            (eye, lip) => getValueFunc.Invoke(eye), paramName)
        {
        }

        public string[] GetName() => new [] {ParamName};
        public BaseParam[] GetBase()
        {
            return new BaseParam[] {this};
        }
    }

    public class BinaryParameter : BinaryBaseParameter, IParameter
    {
        public BinaryParameter(Func<EyeTrackingData, Dictionary<LipShape_v2, float>, float?> getValueFunc,
            string paramName) : base(paramName)
        {
            #if DLL
            UnifiedTrackingData.OnUnifiedParamsUpdated += (eye, lipFloats, lip) =>
            {
                if (!UnifiedLibManager.EyeEnabled && !UnifiedLibManager.LipEnabled) return;
                var value = getValueFunc.Invoke(eye, lip);
                if (value.HasValue)
                    ParamValue = value.Value;
            };
#endif
        }

        public BinaryParameter(Func<EyeTrackingData, float> getValueFunc, string paramName) : this((eye, lip) => getValueFunc.Invoke(eye), paramName)
        {
        }

        public BaseParam[] GetBase()
        {
            return new BaseParam[] {this};
        }
    }

    // EverythingParam, or EpicParam. You choose!
    // Contains a bool, float and binary parameter, all in one class with IParameter implemented.
    public class EParam : IParameter
    {
        private readonly BaseParam[] _parameter;
        private readonly string Name;

        public EParam(Func<EyeTrackingData, Dictionary<LipShape_v2, float>, float?> getValueFunc, string paramName, float minBoolThreshold = 0.5f, bool skipBinaryParamCreation = false)
        {
            var paramLiterals = new List<BaseParam>
            {
                new BoolParameter((eye, lip) => getValueFunc.Invoke(eye, lip) < minBoolThreshold, paramName),
                new FloatParameter(getValueFunc, paramName, true),
            };
            
            if (!skipBinaryParamCreation)
             paramLiterals.Add(new BinaryParameter(getValueFunc, paramName));

            Name = paramName;
            _parameter = paramLiterals.ToArray();
            Logger.Msg("Constructing EParam name " + Name);
        }

        public EParam(Func<EyeTrackingData, float> getValueFunc, string paramName,
            float minBoolThreshold = 0.5f) : this((eye, lip) => getValueFunc.Invoke(eye), paramName, minBoolThreshold)
        {
        }

        public string[] GetName()
        {
            var names = new List<string>();
            foreach (var param in _parameter)
                names.Add(param.ParamName);
            return names.ToArray();
        }

        public void ResetParam()
        {
            foreach (var param in _parameter)
                param.ResetParam();
        }

        public void ZeroParam()
        {
            foreach (var param in _parameter)
                param.ZeroParam();
        }

        public BaseParam[] GetBase()
        {
            // Log name and whether _param is null
            return _parameter;
        }
    }
}