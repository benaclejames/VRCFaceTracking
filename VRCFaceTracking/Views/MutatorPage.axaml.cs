using Avalonia.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public partial class MutatorPage : UserControl
{
    public MutatorPage()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<MutatorViewModel>();
    }
}
