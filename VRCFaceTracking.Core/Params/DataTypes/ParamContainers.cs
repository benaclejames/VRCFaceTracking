using VRCFaceTracking.Core.OSC.DataTypes;
using VRCFaceTracking.Core.Params.Data;
using VRCFaceTracking.Core.Types;
using VRCFaceTracking.OSC;

namespace VRCFaceTracking.Core.Params.DataTypes
{
    public class AlwaysRelevantParameter<T> : BaseParam<T>, IParameter where T : struct
    {
        public AlwaysRelevantParameter(Func<UnifiedTrackingData, T> getValueFunc,
            string paramAddress)
            : base(CurrentVersionPrefix, getValueFunc)
        {
            OscMessage.Address = paramAddress;
            Relevant = true;
        }

        public new (string, IParameter)[] GetParamNames() => new[] { (OscMessage.Address, (IParameter)this) };
        
        public override IParameter[] ResetParam((string paramName, string paramAddress, Type paramType)[] newParams)
        {
            Relevant = true;
            return new IParameter[] { this };
        }
    }

    public class NativeParameter<T> : AlwaysRelevantParameter<T>, IParameter where T : struct
    {
        private readonly Func<(string paramName, string paramAddress, Type paramType)[], bool> _condition;
        
        public NativeParameter(Func<UnifiedTrackingData, T> getValueFunc, Func<(string paramName, string paramAddress, Type paramType)[], bool> condition, string paramAddress) : base(getValueFunc, paramAddress)
        {
            _condition = condition;
        }
        
        public override IParameter[] ResetParam((string paramName, string paramAddress, Type paramType)[] newParams)
        {
            if (!_condition.Invoke(newParams))
            {
                Relevant = false;
                return Array.Empty<IParameter>();
            }
            
            return base.ResetParam(newParams);
        }
        
        public new (string, IParameter)[] GetParamNames() => new (string, IParameter)[] { (OscMessage.Address, this) };
    }

    // This parameter type will only update parameter 1 if parameter 2 is true
    public class ConditionalBoolParameter : BaseParam<bool>
    {
        private readonly Func<UnifiedTrackingData, (bool, bool)> _conditionalValueFunc;
        
        public ConditionalBoolParameter(Func<UnifiedTrackingData, (bool, bool)> getValueFunc, string paramName) :
            base(paramName, exp => getValueFunc.Invoke(exp).Item1)
        => _conditionalValueFunc = getValueFunc;

        protected override void Process(UnifiedTrackingData exp)
        {
            if (_conditionalValueFunc.Invoke(exp).Item2)
                base.Process(exp);
        }
    }

    // EverythingParam, or EpicParam. You choose!
    // Contains a bool, float and binary parameter, all in one class with IParameter implemented.
    public class EParam : IParameter
    {
        private readonly IParameter[] _parameter;

        public EParam(string paramName, Func<UnifiedTrackingData, float> getValueFunc, float minBoolThreshold = 0.5f,
            bool skipBinaryParamCreation = false)
        {
            if (skipBinaryParamCreation)
            {
                _parameter = new IParameter[]
                {
                    new BaseParam<bool>(paramName, exp => getValueFunc.Invoke(exp) < minBoolThreshold),
                    new BaseParam<float>(paramName, getValueFunc),
                };
            }
            else
            {
                _parameter = new IParameter[]
                {
                    new BaseParam<bool>(paramName, exp => getValueFunc.Invoke(exp) < minBoolThreshold),
                    new BaseParam<float>(paramName, getValueFunc),
                    new BinaryBaseParameter(paramName, getValueFunc)
                };
            }
        }

        public EParam(string paramName, Func<UnifiedTrackingData, Vector2> getValueFunc, float minBoolThreshold = 0.5f)
        {
            _parameter = new IParameter[]
            {
                new BaseParam<bool>(paramName + "X", exp => getValueFunc.Invoke(exp).x < minBoolThreshold),
                new BaseParam<float>(paramName + "X", exp => getValueFunc.Invoke(exp).x),
                new BinaryBaseParameter(paramName + "X", exp => getValueFunc.Invoke(exp).x),

                new BaseParam<bool>(paramName + "Y", exp => getValueFunc.Invoke(exp).y < minBoolThreshold),
                new BaseParam<float>(paramName + "Y", exp => getValueFunc.Invoke(exp).y),
                new BinaryBaseParameter(paramName + "Y", exp => getValueFunc.Invoke(exp).y)
            };
        }

        public IParameter[] ResetParam((string paramName, string paramAddress, Type paramType)[] newParams) => _parameter.SelectMany(param => param.ResetParam(newParams)).ToArray();

        public (string, IParameter)[] GetParamNames() => _parameter.SelectMany(param => param.GetParamNames()).ToArray();
        
        public bool Deprecated => false;    // False as our children will handle this
    }
}