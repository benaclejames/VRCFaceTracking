using System.Collections;
using MelonLoader;
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

        public void ResetParam() => _paramIndex = ParamLib.GetParamIndex(ParamName);

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

        public FloatParam(string paramName, bool prioritised = false) : base(paramName)
        {
            if (!prioritised) return;
            
            Prioritised = true;
            MelonCoroutines.Start(KeepParamPrioritised());
        } 
        
        private IEnumerator KeepParamPrioritised()
        {
            for (;;)
            {
                yield return new WaitForSeconds(5);
                if (!Prioritised) continue;
                ParamLib.PrioritizeParameter(_paramIndex);
            }
        }
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