using CommunityToolkit.WinUI.UI.Controls;

using Microsoft.UI.Xaml.Controls;

using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public sealed partial class ModuleRegistryPage : Page
{
    public ModuleRegistryViewModel ViewModel
    {
        get;
    }

    public ModuleRegistryPage()
    {
        ViewModel = App.GetService<ModuleRegistryViewModel>();
        InitializeComponent();
    }

    private void OnViewStateChanged(object sender, ListDetailsViewState e)
    {
        if (e == ListDetailsViewState.Both)
        {
            ViewModel.EnsureItemSelected();
        }
    }
}
