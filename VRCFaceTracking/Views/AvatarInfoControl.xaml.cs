using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Params;

namespace VRCFaceTracking.Views;

public sealed partial class AvatarInfoControl : UserControl
{
    public AvatarInfoControl()
    {
        InitializeComponent();
    }
    
    public void OnAvatarSwitched(IAvatarInfo newInfo, List<Parameter> parameters)
    {
        AvatarName.Text = newInfo.Name;
        AvatarId.Text = newInfo.Id;
        
        var deprecatedParams = parameters.Count(p => p.Deprecated);

        CurrentParametersCount.Text = parameters.Count.ToString();
        LegacyParametersCount.Text = deprecatedParams.ToString();

        LegacyParamsWarning.Visibility = deprecatedParams > 0 ? Visibility.Visible : Visibility.Collapsed;
        LocalTestWarning.Visibility = newInfo.Id.StartsWith("local:") ? Visibility.Visible : Visibility.Collapsed;
    }
}