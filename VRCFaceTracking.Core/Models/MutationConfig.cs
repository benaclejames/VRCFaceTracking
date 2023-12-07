namespace VRCFaceTracking.Core.Models;

public struct MutationConfig
{
    public string Name;
    public float Ceil; // The maximum that the parameter reaches.
    public float Floor; // the minimum that the parameter reaches.
    //public float SigmoidMult; // How much should this parameter be affected by the sigmoid function. This makes the parameter act more like a toggle.
    //public float LogitMult; // How much should this parameter be affected by the logit (inverse of sigmoid) function. This makes the parameter act more within the normalized range.
    public float SmoothnessMult; // How much should this parameter be affected by the smoothing function.
}