using System;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using ParamLib;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace VRCFaceTracking.Params.Eye
{
    public class FloatEyeParameter : FloatBaseParam, IParameter
    {
        public FloatEyeParameter(Func<EyeTrackingData, float> getValueFunc, string paramName, bool wantsPriority = false)
            : base(paramName, wantsPriority) =>
            UnifiedTrackingData.OnUnifiedParamsUpdated += (eye, lip, floats) =>
            {
                if (!UnifiedLibManager.EyeEnabled) return;
                ParamValue = getValueFunc.Invoke(eye);
            };

        public string[] GetName() => new[] {ParamName};
    }

    public class XYParameter : XYParam, IParameter
    {
        public XYParameter(Func<EyeTrackingData, Vector2> getValueFunc, string xParamName, string yParamName)
            : base(new FloatBaseParam(xParamName, true), new FloatBaseParam(yParamName, true))
        {
            UnifiedTrackingData.OnUnifiedParamsUpdated += (eye, lip, floats) =>
            {
                if (!UnifiedLibManager.EyeEnabled) return;
                ParamValue = getValueFunc.Invoke(eye);
            };
        }

        void IParameter.ResetParam() => ResetParams();
        public void ZeroParam() => ZeroParams();
        public string[] GetName() => new[] {X.ParamName, Y.ParamName};

    }

    public class BoolEyeParameter : BoolBaseParam, IParameter
    {
        public BoolEyeParameter(Func<EyeTrackingData, bool> getValueFunc, string paramName) : base(paramName) =>
            UnifiedTrackingData.OnUnifiedParamsUpdated += (eye, lip, floats) =>
            {
                if (!UnifiedLibManager.EyeEnabled) return;
                ParamValue = getValueFunc.Invoke(eye);
            };

        public string[] GetName() => new [] {ParamName};
    }

    public class BinaryEyeParameter : IParameter
    {
        private readonly List<BoolEyeParameter> _params = new List<BoolEyeParameter>();
        private readonly string _paramName;
        private readonly Func<EyeTrackingData, float> _getValueFunc;

        /* Pretty complicated, but let me try to explain...
         * As with other ResetParam functions, the purpose of this function is to reset all the parameters.
         * Since we don't actually know what parameters we'll be needing for this new avatar, nor do we know if the parameters we currently have are valid
         * it's just easier to just reset everything.
         *
         * Step 1) Find all valid parameters on the new avatar that start with the name of this binary param, and end with a number.
         * 
         * Step 2) Find the binary steps for that number. That's the number of shifts we need to do. That number could be 8, and it's steps would be 3 as it's 3 steps away from zero in binary
         * This also makes sure the number is a valid base2-compatible number
         *
         * Step 3) Calculate the maximum possible value for the discovered binary steps, then subtract 1 since we count from 0.
         *
         * Step 4) Create each parameter literal that'll be responsible for actually changing parameters. It's output data will be multiplied by the highest possible
         * binary number since we can safely assume the highest possible input float will be 1.0. Then we bitwise shift by the binary steps discovered in step 2.
         * Finally, we use a combination of bitwise AND to get whether the designated index for this param is 1 or 0.
         */
        public void ResetParam()
        {
            // Get all parameters starting with this parameter's name, and of type bool
            var boolParams = ParamLib.ParamLib.GetLocalParams().Where(p => p.valueType == VRCExpressionParameters.ValueType.Bool && p.name.StartsWith(_paramName)).ToArray();

            var paramsToCreate = new Dictionary<string, int>();
            foreach (var param in boolParams)
            {
                // Cut the parameter name to get the index
                var index = int.Parse(param.name.Substring(_paramName.Length));
                // Get the shift steps
                var binaryIndex = GetBinarySteps(index);
                // If this index has a shift step, create the parameter
                if (binaryIndex.HasValue)
                    paramsToCreate.Add(param.name, binaryIndex.Value);
            }

            if (paramsToCreate.Count == 0) return;
            
            // Calculate the highest possible binary number
            var maxPossibleBinaryInt = Math.Pow(2, paramsToCreate.Values.Count)-1;
            foreach (var param in paramsToCreate)
                // Create the parameter literal. Calculate the 
                _params.Add(new BoolEyeParameter(
                    data => (((int) (_getValueFunc.Invoke(data) * maxPossibleBinaryInt) >> param.Value) & 1) == 1,
                    param.Key));
        }
        
        // This serves both as a test to make sure this index is in the binary sequence, but also returns how many bits we need to shift to find it
        private static int? GetBinarySteps(int index)
        {
            var currSeqItem = 1;
            for (var i = 0; i < index; i++)
            {
                if (currSeqItem == index)
                    return i;
                currSeqItem*=2;
            }
            return null;
        }

        public void ZeroParam()
        {
            foreach (var param in _params)
                param.ZeroParam();
            _params.Clear();
        }

        public string[] GetName() =>
            // If we have no parameters, return a single value array containing the paramName. If we have values, return the names of all the parameters
            _params.Count == 0 ? new[] {_paramName} : _params.Select(p => p.ParamName).ToArray();

        public BinaryEyeParameter(Func<EyeTrackingData, float> getValueFunc, string paramName)
        {
            _paramName = paramName;
            _getValueFunc = getValueFunc;
        }
    }

    // EverythingParam, or EpicParam. You choose!
    // Contains a bool, float and binary parameter, all in one class with IParameter implemented.
    public class EParam : IParameter
    {
        private readonly IParameter[] _parameter;

        public EParam(Func<EyeTrackingData, float> getValueFunc, string paramName, float minBoolThreshold = 0.5f)
        {
            var boolParam = new BoolEyeParameter(eye => getValueFunc.Invoke(eye) < minBoolThreshold, paramName);
            var floatParam = new FloatEyeParameter(getValueFunc, paramName, true);
            var binaryParam = new BinaryEyeParameter(getValueFunc, paramName);
            
            _parameter = new IParameter[] {boolParam, floatParam, binaryParam};
        }

        public string[] GetName()
        {
            var names = new List<string>();
            foreach (var param in _parameter)
                names.AddRange(param.GetName());
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
    }
}