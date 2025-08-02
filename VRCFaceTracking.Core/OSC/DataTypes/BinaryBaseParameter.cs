using System.Text.RegularExpressions;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Params;
using VRCFaceTracking.Core.Params.Data;

namespace VRCFaceTracking.Core.OSC.DataTypes;

public class BinaryBaseParameter : Parameter
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
        
        value = Math.Abs(value);

        if (value > 0.99999f)
        {
            return true; // Platform independent fix for floor 1 and bigger
        }

        var bigValue = (int)(value * _maxPossibleBinaryInt);

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
    public override Parameter[] ResetParam(IParameterDefinition[] newParams)
    {
        _params.Clear();
        var negativeRelevancy = _negativeParam.ResetParam(newParams);

        var boolParams = newParams.Where(p =>
            p.Type == typeof(bool) && _regex.IsMatch(p.Address));

        var paramsToCreate = new Dictionary<string, int>();
        foreach (var param in boolParams)
        {
            var tempName = param.Name;
            if (!int.TryParse(
                    String.Concat(tempName.Replace(_paramName, "").ToArray().Reverse().TakeWhile(char.IsNumber)
                        .Reverse()), out var index)) continue;
            // Get the shift steps
            var binaryIndex = GetBinarySteps(index);
            // If this index has a shift step, create the parameter
            if (binaryIndex.HasValue)
                paramsToCreate.Add(_paramName + index, binaryIndex.Value);
        }

        if (paramsToCreate.Count == 0) return negativeRelevancy;

        // Calculate the highest possible binary number
        _maxPossibleBinaryInt = (int)Math.Pow(2, paramsToCreate.Values.Count);
        var parameters = new List<Parameter>(negativeRelevancy);
        foreach (var newBool in paramsToCreate
                     .Select(param =>
                         new BaseParam<bool>(param.Key, data => ProcessBinary(data, param.Value), true)))
        {
            parameters.AddRange(newBool.ResetParam(newParams));
            _params.Add(newBool);
        }

        return parameters.ToArray();
    }

    public override (string, Parameter)[] GetParamNames() =>
        _params.SelectMany(p => p.GetParamNames()).Concat(_negativeParam.GetParamNames()).ToArray();

    public new bool Deprecated => false; // Handled by our children

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

    public BinaryBaseParameter(string paramName, Func<UnifiedTrackingData, float> getValueFunc)
    {
        _paramName = paramName;
        _regex = new Regex(@"(?<!(v\d+))/" + _paramName + @"\d+$|^" + _paramName + @"\d+$");
        _getValueFunc = getValueFunc;
        _negativeParam =
            new BaseParam<bool>(paramName + "Negative", data => getValueFunc.Invoke(data) < 0, true);
    }
}