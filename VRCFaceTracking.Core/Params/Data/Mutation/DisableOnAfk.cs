using System;
using System.Collections.Generic;
using System.Text;

namespace VRCFaceTracking.Core.Params.Data.Mutation;

internal class DisableOnAfk : TrackingMutation
{
    public override string Name => "Disable FT on AFK";

    public override string Description => "When enabled, stops sending FT messages to VRChat when AFK is detected.";

    public override MutationPriority Step => MutationPriority.Postprocessor; // post-process because we're completely overriding the data with 0s.

    public override bool IsSaved => base.IsSaved;


    public static volatile bool IsAfk; // probably a better way of doing this
    private readonly UnifiedTrackingData _blank = new();

    public override void MutateData(ref UnifiedTrackingData data)
    {
        if(IsAfk)
        {
            data.CopyPropertiesOf(_blank);
        }
    }
}
