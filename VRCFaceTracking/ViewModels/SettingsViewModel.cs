using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking.Core.Contracts;
using VRCFaceTracking.Models;
using VRCFaceTracking.Services;

namespace VRCFaceTracking.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    [ObservableProperty] private List<GithubContributor> _contributors = [];

    public IOscTarget OscTarget { get; }
    public RiskySettingsViewModel RiskySettings { get; }

    private readonly OpenVRService _openVRService;

    public bool AutoStart
    {
        get => _openVRService.AutoStart;
        set
        {
            _openVRService.AutoStart = value;
            OnPropertyChanged();
        }
    }

    public bool IsOpenVREnabled => _openVRService.IsInitialized;

    public SettingsViewModel(
        GithubService githubService,
        OpenVRService openVRService,
        IOscTarget oscTarget,
        RiskySettingsViewModel riskySettingsViewModel)
    {
        _openVRService = openVRService;
        OscTarget = oscTarget;
        RiskySettings = riskySettingsViewModel;

        _openVRService.InitIfNotAlready();
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
