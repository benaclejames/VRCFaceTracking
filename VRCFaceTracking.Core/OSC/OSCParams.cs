using System.Text.RegularExpressions;
using VRCFaceTracking.Core.OSC;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.OSC
{
    public class OSCParams
    {
        private const string DefaultPrefix = "/avatar/parameters/";
        public const string CurrentVersionPrefix = "v2/";
        
        public static readonly List<OscMessageMeta> SendQueue = new();
        public static bool AlwaysRelevantDebug = false;
        
        public class BaseParam<T> : IParameter where T : struct
        {
            private readonly Func<UnifiedTrackingData, T> _getValueFunc;

            private readonly string _paramName;
            private readonly Regex _regex;
            
            private bool _relevant;
            private bool _sendOnLoad;

            public bool Relevant
            {
                get => _relevant;
                protected set
                {
                    if (_sendOnLoad && value) Enqueue();
                    
                    // If we're irrelevant or we don't have a getValueFunc, we don't need to do anything
                    if (_relevant == value) return;
                    
                    _relevant = value;

                    if (_getValueFunc == null) return;

                    if (value)
                        UnifiedTracking.OnUnifiedDataUpdated += Process;
                    else
                        UnifiedTracking.OnUnifiedDataUpdated -= Process;
                }
            }

            public T ParamValue
            {
                get => (T)OscMessage.Value;
                set
                {
                    // Ensure that the value is different
                    if (Equals(OscMessage.CachedValue, value)) return;
                    
                    OscMessage.Value = value;
                    Enqueue();
                }
            }

            private void Enqueue() => SendQueue.Add(OscMessage._meta);

            protected readonly OscMessage OscMessage;

            public BaseParam(string name, Func<UnifiedTrackingData, T> getValueFunc, bool sendOnLoad = false)
            {
                _paramName = name;
                _regex = new Regex(@"(?<!(v\d+))(/" + _paramName + @")$|^(" + _paramName + @")$");
                _getValueFunc = getValueFunc;
                OscMessage = new OscMessage(DefaultPrefix+name, typeof(T));
                _sendOnLoad = sendOnLoad;
            }

            public virtual IParameter[] ResetParam(ConfigParser.Parameter[] newParams)
            {
                if (AlwaysRelevantDebug)
                {
                    Relevant = true;
                    OscMessage.Address = DefaultPrefix+_paramName;
                    
                    return new IParameter[] { this };
                }

                var compatibleParam = newParams.FirstOrDefault(param =>
                    _regex.IsMatch(param.name)
                    && param.input.Type == typeof(T));

                if (compatibleParam != null)
                {
                    Relevant = true;
                    OscMessage.Address = compatibleParam.input.address;
                }
                else
                {
                    Relevant = false;
                    OscMessage.Address = DefaultPrefix+_paramName;
                }
                
                return Relevant ? new IParameter[] {this} : Array.Empty<IParameter>();
            }

            public (string, IParameter)[] GetParamNames() => new[] {(_paramName, (IParameter) this)};

            public bool Deprecated => !_paramName.StartsWith(CurrentVersionPrefix);

            protected virtual void Process(UnifiedTrackingData data) => ParamValue = _getValueFunc.Invoke(data);
            
            ~BaseParam()
            {   
                // Not sure if this is actually needed, but it's good practice
               UnifiedTracking.OnUnifiedDataUpdated -= Process;
            }
        }

        public class BinaryBaseParameter : IParameter
        {
            private readonly List<BaseParam<bool>> _params = new(); // Int represents binary steps

            private readonly BaseParam<bool> _negativeParam;
            private int _maxPossibleBinaryInt;
            private readonly string _paramName;
            private readonly Func<UnifiedTrackingData, float> _getValueFunc;
            private readonly Regex _regex;

            private bool ProcessBinary(UnifiedTrackingData data, int binaryIndex)
            {
                var value = _getValueFunc.Invoke(data);
                if (!_negativeParam.Relevant &&
                    value < 0) // If the negative parameter isn't set, cut the negative values
                    return false;
                var adjustedValue = Math.Abs(value);
                var bigValue = (int) (adjustedValue * (_maxPossibleBinaryInt - 1));
                return ((bigValue >> binaryIndex) & 1) == 1;
            }

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
            public IParameter[] ResetParam(ConfigParser.Parameter[] newParams)
            {
                _params.Clear();
                var negativeRelevancy = _negativeParam.ResetParam(newParams);

                var boolParams = newParams.Where(p => 
                    p.input.Type == typeof(bool) && _regex.IsMatch(p.name));

                var paramsToCreate = new Dictionary<string, int>();
                foreach (var param in boolParams)
                {
                    var tempName = param.name;
                    if (!int.TryParse(String.Concat(tempName.Replace(_paramName, "").ToArray().Reverse().TakeWhile(char.IsNumber).Reverse()), out var index)) continue;
                    // Get the shift steps
                    var binaryIndex = GetBinarySteps(index);
                    // If this index has a shift step, create the parameter
                    if (binaryIndex.HasValue)
                        paramsToCreate.Add(_paramName+index, binaryIndex.Value);
                }

                if (paramsToCreate.Count == 0) return negativeRelevancy;

                // Calculate the highest possible binary number
                _maxPossibleBinaryInt = (int) Math.Pow(2, paramsToCreate.Values.Count);
                var parameters = new List<IParameter>(negativeRelevancy);
                foreach (var newBool in paramsToCreate
                             .Select(param => new BaseParam<bool>(param.Key, data => ProcessBinary(data, param.Value), true)))
                {
                    parameters.AddRange(newBool.ResetParam(newParams));
                    _params.Add(newBool);
                }

                return parameters.ToArray();
            }

            public (string, IParameter)[] GetParamNames() => _params.SelectMany(p => p.GetParamNames()).Concat(_negativeParam.GetParamNames()).ToArray();

            public bool Deprecated => false;    // Handled by our children

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

            protected BinaryBaseParameter(string paramName, Func<UnifiedTrackingData, float> getValueFunc)
            {
                _paramName = paramName;
                _regex = new Regex(@"(?<!(v\d+))/" + _paramName + @"\d+$|^" + _paramName + @"\d+$");
                _getValueFunc = getValueFunc;
                _negativeParam = new BaseParam<bool>(paramName + "Negative", data => getValueFunc.Invoke(data) < 0, true);
            }
        }
    }
}