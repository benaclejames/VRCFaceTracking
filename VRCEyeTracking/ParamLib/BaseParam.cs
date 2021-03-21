using System.Collections;
using MelonLoader;
using UnityEngine;

namespace VRCEyeTracking.ParamLib
{
    public class BaseParam
    {
        protected BaseParam(string paramName)
        {
            ParamIndex = ParamLib.GetParamIndex(paramName);
            _paramName = paramName;
        }

        public void ResetParams() => ParamIndex = ParamLib.GetParamIndex(_paramName);

        protected double ParamValue
        {
            get => _paramValue;
            set
            {
                if (ParamLib.SetParameter(ParamIndex, (float) value))
                    _paramValue = value;
            }
        }

        public int? ParamIndex;
        
        private readonly string _paramName;
        protected double _paramValue;
        
    }

    
    
    public class FloatBaseParam : BaseParam
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

        private bool Prioritised
        {
            get => _prioritised;
            set
            {
                if (value)
                    ParamLib.PrioritizeParameter(ParamIndex);
                
                _prioritised = value;
            }
        }
        private bool _prioritised;

        public FloatBaseParam(string paramName, bool prioritised = false) : base(paramName)
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
                ParamLib.PrioritizeParameter(ParamIndex);
            }
        }
    }

    public class XYParam
    {
        private FloatBaseParam X, Y;

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
            X.ResetParams();
            Y.ResetParams();
        }

        protected void ZeroParams()
        {
            X.ParamIndex = null;
            Y.ParamIndex = null;
        }
    }
}