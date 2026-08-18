using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.ViewModels;

public partial class ModuleRegistryViewModel : ObservableRecipient
{
    private readonly IModuleDataService _moduleDataService;
    [ObservableProperty] private InstallTrackedTrackingModule? _selected;
    [ObservableProperty] private string _searchQuery = string.Empty;

    public ObservableCollection<InstallTrackedTrackingModule> ModuleInfos { get; } = new();
    public ObservableCollection<InstallTrackedTrackingModule> FilteredModuleInfos { get; } = new();

    public ModuleRegistryViewModel(IModuleDataService moduleDataService)
    {
        _moduleDataService = moduleDataService;
        ModuleInfos.CollectionChanged += (_, _) => ApplyFilter();
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        FilteredModuleInfos.Clear();
        var query = SearchQuery?.Trim();
        var filtered = string.IsNullOrEmpty(query)
            ? ModuleInfos
            : ModuleInfos.Where(m =>
                (m.TrackingModuleMetadata.ModuleName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (m.TrackingModuleMetadata.AuthorName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
        foreach (var m in filtered)
            FilteredModuleInfos.Add(m);
    }

    public async Task OnNavigatedTo()
    {
        ModuleInfos.Clear();

        var data = await _moduleDataService.GetRemoteModules();
        
        // Lay out our list as we normally would with ordering and all, and assume all are uninstalled
        ModuleInfos.AddRange(new ObservableCollection<InstallTrackedTrackingModule>(data
            .OrderByDescending(x => x.AuthorName == "VRCFT Team").ThenBy(x => x.ModuleName).Select(x =>
                new InstallTrackedTrackingModule {TrackingModuleMetadata = x, InstallationState = InstallState.NotInstalled})));
        
        // Now comes the tricky bit, we get all locally installed modules and add them to the list.
        // If any of the IDs match a remote module and the other data contained within does not match,
        // then we need to set the local module install state to outdated. If everything matches then we need to set the install state to installed.
        var installedModules = _moduleDataService.GetInstalledModules().Concat(_moduleDataService.GetLegacyModules());
        foreach (var installedModule in installedModules)
        {
            var remoteModule = ModuleInfos.FirstOrDefault(x => x.TrackingModuleMetadata.ModuleId == installedModule.ModuleId);
            if (remoteModule == null)   // If this module is completely missing from the remote list, then we need to add it to the list.
            {
                // This module is installed but not in the remote list, so we need to add it to the list at the top
                ModuleInfos.Insert(0, new InstallTrackedTrackingModule {TrackingModuleMetadata = installedModule, InstallationState = InstallState.Installed});
            }
            else
            {
                // This module is installed and in the remote list, so we need to update the remote module's install state.
                remoteModule.InstallationState = remoteModule.TrackingModuleMetadata.Version != installedModule.Version
                    ? InstallState.Outdated
                    : InstallState.Installed;
                ModuleInfos.Move(ModuleInfos.IndexOf(remoteModule), 0);
            }
        }
    }
}
