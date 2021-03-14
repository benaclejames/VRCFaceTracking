using UnityEngine;

namespace VRCEyeTracking.ParamLib
{
    public class ParamType
    {
        protected ParamType(string paramName)
        {
            _paramIndex = ParamLib.GetParamIndex(paramName);
            ParamName = paramName;
        }

        public void ResetParam() => ParamLib.GetParamIndex(ParamName);

        protected double ParamValue
        {
            get => _paramValue;
            set
            {
                if (ParamLib.SetParameter(_paramIndex, (float) value))
                    _paramValue = value;
            }
        }

        public int ParamIndex => _paramIndex;

        internal string ParamName;
        protected double _paramValue;
        protected int _paramIndex;
    }

    
    
    public class FloatParam : ParamType
    {
        public new float ParamValue
        {
            get => (float) _paramValue;
            set
            {
                base.ParamValue = value;
                _paramValue = value;
            }
        }

        public bool Prioritised
        {
            get => _prioritised;
            set
            {
                if (value)
                    ParamLib.PrioritizeParameter(_paramIndex);
                
                _prioritised = value;
            }
        }
        private bool _prioritised;

        public FloatParam(string paramName, bool prioritised = false) : base(paramName) => Prioritised = prioritised;
    }

    public class XYParam
    {
        private FloatParam X, Y;

        protected Vector2 ParamValue
        {
            set
            {
                X.ParamValue = value.x;
                Y.ParamValue = value.y;
            }
        }

        protected XYParam(FloatParam x, FloatParam y)
        {
            X = x;
            Y = y;
        }

        protected void ResetParams()
        {
            X.ResetParam();
            Y.ResetParam();
        }
    }
}