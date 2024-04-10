using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Core.Params;

namespace VRCFaceTracking.Views;

public sealed partial class AvatarInfoControl : UserControl
{
    public IAvatarInfo AvatarInfo
    {
        get => (IAvatarInfo)GetValue(AvatarInfoProperty);
        set => SetValue(AvatarInfoProperty, value);
    }

    public List<Parameter> AvatarParameters
    {
        get => (List<Parameter>)GetValue(AvatarParametersProperty);
        set => SetValue(AvatarParametersProperty, value);
    }

    public static readonly DependencyProperty AvatarInfoProperty =
        DependencyProperty.Register(
            nameof(AvatarInfo),
            typeof(IAvatarInfo),
            typeof(AvatarInfoControl),
            new PropertyMetadata(null));
    
    public static readonly DependencyProperty AvatarParametersProperty = 
        DependencyProperty.Register(
            nameof(AvatarParameters),
            typeof(List<Parameter>),
            typeof(AvatarInfoControl),
            new PropertyMetadata(null));

    public AvatarInfoControl()
    {
        InitializeComponent();
        
        DataContext = this;
        RegisterPropertyChangedCallback(AvatarInfoProperty, OnNewAvatarInfo);
        RegisterPropertyChangedCallback(AvatarParametersProperty, OnNewAvatarParameters);
    }

    private void OnNewAvatarInfo(DependencyObject d, DependencyProperty dp)
    {
        var avatar = (IAvatarInfo)GetValue(dp);
        LocalTestWarning.Visibility = avatar.Id.StartsWith("local:") ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnNewAvatarParameters(DependencyObject d, DependencyProperty dp)
    {
        var parameters = (List<Parameter>)GetValue(dp);
        var deprecatedParams = parameters.Count(p => p.Deprecated);

        CurrentParametersCount.Text = parameters.Count.ToString();
        LegacyParametersCount.Text = deprecatedParams.ToString();

        LegacyParamsWarning.Visibility = deprecatedParams > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
}