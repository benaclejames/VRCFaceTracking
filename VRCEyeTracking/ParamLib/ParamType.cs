namespace VRCEyeTracking.ParamLib
{
    public class ParamType
    {
        protected ParamType(string paramName)
        {
            _paramIndex = ParamLib.GetParamIndex(paramName);
            ParamName = paramName;
        }
        
        public double ParamValue
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
}