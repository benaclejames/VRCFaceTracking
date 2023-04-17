using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.Views;

public sealed partial class ModuleRegistryDetailControl : UserControl
{
    public RemoteTrackingModule? ListDetailsMenuItem
    {
        get => GetValue(ListDetailsMenuItemProperty) as RemoteTrackingModule;
        set => SetValue(ListDetailsMenuItemProperty, value);
    }

    public static readonly DependencyProperty ListDetailsMenuItemProperty = DependencyProperty.Register("ListDetailsMenuItem", typeof(RemoteTrackingModule), typeof(ModuleRegistryDetailControl), new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

    public ModuleRegistryDetailControl()
    {
        InitializeComponent();
    }

    private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ModuleRegistryDetailControl control)
        {
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }

    private void Install_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}
