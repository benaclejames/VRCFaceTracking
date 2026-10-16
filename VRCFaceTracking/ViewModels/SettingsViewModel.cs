using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Models;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Services;

namespace VRCFaceTracking.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    [ObservableProperty] private List<GithubContributor> _contributors = [];

    public IOscTarget OscTarget { get; }
    public RiskySettingsViewModel RiskySettings { get; }

    private readonly IVrRuntimeService _vrRuntimeService;

    public bool AutoStart
    {
        get => _vrRuntimeService.AutoStart;
        set
        {
            _vrRuntimeService.AutoStart = value;
            OnPropertyChanged();
        }
    }

    public bool IsOpenVREnabled => _vrRuntimeService.IsInitialized;

    public SettingsViewModel(
        GithubService githubService,
        IVrRuntimeService vrRuntimeService,
        IOscTarget oscTarget,
        RiskySettingsViewModel riskySettingsViewModel)
    {
        _vrRuntimeService = vrRuntimeService;
        OscTarget = oscTarget;
        RiskySettings = riskySettingsViewModel;

        _vrRuntimeService.InitIfNotAlready();
        LoadContributors(githubService);
    }

    private async void LoadContributors(GithubService githubService)
    {
        try
        {
            Contributors = await githubService.GetContributors("benaclejames/VRCFaceTracking");
        }
        catch
        {
        }
    }
}
