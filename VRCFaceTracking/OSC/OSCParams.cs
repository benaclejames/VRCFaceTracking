using System;
using System.Collections.Generic;
using System.Linq;

namespace VRCFaceTracking.OSC
{
    public class OSCParams
    {
        public class BaseParam
        {
            public string ParamName;

            public byte[] ParamValue
            {
                get => _paramValue;
                set
                {
                    if (Relevant)
                    {
                        _paramValue = value;
                    }
                }
            }

            private byte[] _paramValue;
            public char ParamType;
            public bool Relevant;

            public BaseParam(string name, char type)
            {
                ParamName = name;
                ParamType = type;
            }

            public virtual void ResetParam(ConfigParser.Parameter[] newParams)
            {
            }
        }

        public class FloatBaseParam : BaseParam
        {
            public FloatBaseParam(string name) : base(name, 'f')
            {
            }

            public new float ParamValue
            {
                set
                {
                    var valueArr = BitConverter.GetBytes(value);
                    Array.Reverse(valueArr);
                    base.ParamValue = valueArr;
                }
            }

            public override void ResetParam(ConfigParser.Parameter[] newParams)
            {
                Relevant = newParams.Any(param => param.name == ParamName && param.input.Type == typeof(float));
            }
        }
        
        public class BoolBaseParam : BaseParam
        {
            public BoolBaseParam(string name) : base(name, 'F')
            {
            }

            public new bool ParamValue
            {
                set
                {
                    if (Relevant)
                        ParamType = value ? 'T' : 'F';
                }
            }
            
            public override void ResetParam(ConfigParser.Parameter[] newParams)
            {
                Relevant = newParams.Any(param => param.name == ParamName && param.input.Type == typeof(bool));
            }
        }
        
        public class BinaryBaseParameter : FloatBaseParam
    {
        public new double ParamValue
        {
            set
            {
                // If the value is negative, make it positive
                if (!_negativeParam.Relevant &&
                    value < 0) // If the negative parameter isn't set, cut the negative values
                    return;
                        
                // Ensure value going into the bitwise shifts is between 0 and 1
                var adjustedValue = Math.Abs(value);

                var bigValue = (int) (adjustedValue * (_maxPossibleBinaryInt - 1));
                
                foreach (var boolChild in _params)
                    boolChild.Value.ParamValue = ((bigValue >> boolChild.Key) & 1) == 1;

                _negativeParam.ParamValue = value < 0;
            }
        }

        protected readonly Dictionary<int, BoolBaseParam> _params = new Dictionary<int, BoolBaseParam>(); // Int represents binary steps
        protected readonly BoolBaseParam _negativeParam;
        private int _maxPossibleBinaryInt;
        private readonly string _paramName;

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
        public override void ResetParam(ConfigParser.Parameter[] newParams)
        {
            _params.Clear();
            _negativeParam.ResetParam(newParams);
        
            // Get all parameters starting with this parameter's name, and of type bool
            var boolParams = newParams.Where(p => p.input.Type == typeof(bool) && p.name.StartsWith(_paramName));

            var paramsToCreate = new Dictionary<string, int>();
            foreach (var param in boolParams)
            {
                // Cut the parameter name to get the index
                if (!int.TryParse(param.name.Substring(_paramName.Length), out var index)) continue;
                // Get the shift steps
                var binaryIndex = GetBinarySteps(index);
                // If this index has a shift step, create the parameter
                if (binaryIndex.HasValue)
                    paramsToCreate.Add(param.name, binaryIndex.Value);
            }

            if (paramsToCreate.Count == 0) return;
            
            // Calculate the highest possible binary number
            _maxPossibleBinaryInt = (int)Math.Pow(2, paramsToCreate.Values.Count);
            foreach (var param in paramsToCreate)
            {
                var newBool = new BoolBaseParam(param.Key);
                newBool.ResetParam(newParams);
                _params.Add(param.Value, newBool);
            }
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
        
        public BinaryBaseParameter(string paramName) : base(paramName)
        {
            _paramName = paramName;
            _negativeParam = new BoolBaseParam(paramName + "Negative");
        }
    }
    }
}