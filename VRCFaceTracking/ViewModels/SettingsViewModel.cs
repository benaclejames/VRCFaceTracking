using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Models;
using VRCFaceTracking.Services;

namespace VRCFaceTracking.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    [ObservableProperty] private ElementTheme _elementTheme;
    [ObservableProperty] private List<GithubContributor> _contributors;
    
    public ICommand SwitchThemeCommand
    {
        get;
    }

    private GithubService GithubService
    {
        get;
        set;
    }

    private async void LoadContributors()
    {
        Contributors = await GithubService.GetContributors("benaclejames/VRCFaceTracking");
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, GithubService githubService)
    {
        _themeSelectorService = themeSelectorService;
        GithubService = githubService;

        _elementTheme = _themeSelectorService.Theme;

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });
        
        LoadContributors();
    }
}
