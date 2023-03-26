using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VRCFaceTracking.Params;

namespace VRCFaceTracking.OSC
{
    public class OSCParams
    {
        private const string DefaultPrefix = "/avatar/parameters/";
        
        public static readonly List<OscMessageMeta> SendQueue = new List<OscMessageMeta>();
        
        public class BaseParam<T> : IParameter
        {
            protected Func<UnifiedTrackingData, T> getValueFunc;

            private readonly string _paramName;
            
            private bool _relevant;

            public bool Relevant
            {
                get => _relevant;
                private set
                {
                    // If we're irrelevant or we don't have a getValueFunc, we don't need to do anything
                    if (_relevant == value) return;
                    
                    _relevant = value;

                    if (getValueFunc == null) return;
                    
                    if (value)
                        UnifiedTracking.OnUnifiedDataUpdated += Process;
                    else
                        UnifiedTracking.OnUnifiedDataUpdated -= Process;
                }
            }

            public virtual T ParamValue
            {
                get => (T)_oscMessage.Value;
                set
                {
                    // Ensure that the value is different
                    if (Equals(ParamValue, value)) return;
                    
                    _oscMessage.Value = value;
                    NeedsSend = true;
                }
            }

            private bool NeedsSend
            {
                set
                {
                    if (!value)
                        return;
                    
                    SendQueue.Add(_oscMessage._meta);
                }
            }

            private readonly OscMessage _oscMessage;

            protected BaseParam(string name, Func<UnifiedTrackingData, T> getValueFunc) : this(name) => this.getValueFunc = getValueFunc;

            public BaseParam(string name)
            {
                _paramName = name;
                _oscMessage = new OscMessage(DefaultPrefix+name, typeof(T));
            }

            public virtual IParameter[] ResetParam(ConfigParser.Parameter[] newParams)
            {
                Regex regex = new Regex(@"(?<!(v\d+))(/" + _paramName + @")$|^(" + _paramName + @")$");

                var compatibleParam = newParams.FirstOrDefault(param =>
                    regex.IsMatch(param.name)
                    && param.input.Type == typeof(T));

                if (compatibleParam != null)
                {
                    Relevant = true;
                    _oscMessage.Address = compatibleParam.input.address;
                }
                else
                {
                    Relevant = false;
                    _oscMessage.Address = DefaultPrefix+_paramName;
                }
                
                return Relevant ? new IParameter[] {this} : Array.Empty<IParameter>();
            }

            //public ParameterState[] GetSelfAndChildren() => new ParameterState[] {this};

            protected virtual void Process(UnifiedTrackingData data) => ParamValue = getValueFunc.Invoke(data);
        }

        public class BinaryBaseParameter : BaseParam<float>, IParameter
        {
            public override float ParamValue
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

            private readonly Dictionary<int, BaseParam<bool>>
                _params = new(); // Int represents binary steps

            private readonly BaseParam<bool> _negativeParam;
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
            public override IParameter[] ResetParam(ConfigParser.Parameter[] newParams)
            {
                _params.Clear();
                IParameter[] negativeRelevancy = _negativeParam.ResetParam(newParams);

                // Get all parameters starting with this parameter's name, and of type bool
                Regex regex = new Regex(@"(?<!(v\d+))/" + _paramName + @"\d+$|^" + _paramName + @"\d+$");

                var boolParams = newParams.Where(p => 
                    p.input.Type == typeof(bool) && regex.IsMatch(p.name));

                var paramsToCreate = new Dictionary<string, int>();
                foreach (var param in boolParams)
                {
                    var _name = param.name;
                    if (!int.TryParse(String.Concat(_name.Replace(_paramName, "").ToArray().Reverse().TakeWhile(char.IsNumber).Reverse()), out var index)) continue;
                    // Get the shift steps
                    var binaryIndex = GetBinarySteps(index);
                    // If this index has a shift step, create the parameter
                    if (binaryIndex.HasValue)
                        paramsToCreate.Add(param.name, binaryIndex.Value);
                }

                if (paramsToCreate.Count == 0) return negativeRelevancy;

                // Calculate the highest possible binary number
                _maxPossibleBinaryInt = (int) Math.Pow(2, paramsToCreate.Values.Count);
                List<IParameter> parameters = new List<IParameter>();
                parameters.AddRange(negativeRelevancy);
                foreach (var param in paramsToCreate)
                {
                    var newBool = new BaseParam<bool>(param.Key);
                    parameters.AddRange(newBool.ResetParam(newParams));
                    _params.Add(param.Value, newBool);
                }

                return parameters.ToArray();
            }

            public new bool Relevant => false;

            // This serves both as a test to make sure this index is in the binary sequence, but also returns how many bits we need to shift to find it
            private static int? GetBinarySteps(int index)
            {
                var currSeqItem = 1;
                for (var i = 0; i < index; i++)
                {
                    if (currSeqItem == index)
                        return i;
                    currSeqItem *= 2;
                }

                return null;
            }

            protected BinaryBaseParameter(string paramName, Func<UnifiedTrackingData, float> getValueFunc) : base(paramName, getValueFunc)
            {
                _paramName = paramName;
                _negativeParam = new BaseParam<bool>(paramName + "Negative");
            }
        }
    }
}