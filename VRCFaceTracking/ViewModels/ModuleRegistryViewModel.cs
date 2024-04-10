using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using VRCFaceTracking.Contracts.ViewModels;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.ViewModels;

public partial class ModuleRegistryViewModel : ObservableRecipient, INavigationAware
{
    private readonly IModuleDataService _moduleDataService;
    [ObservableProperty] private InstallableTrackingModule? _selected;

    public ObservableCollection<InstallableTrackingModule> ModuleInfos { get; } = new();
    
    public ModuleRegistryViewModel(IModuleDataService moduleDataService)
    {
        _moduleDataService = moduleDataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        ModuleInfos.Clear();

        var data = await _moduleDataService.GetRemoteModules();
        
        // Now comes the tricky bit, we get all locally installed modules and add them to the list.
        // If any of the IDs match a remote module and the other data contained within does not match,
        // then we need to set the local module install state to outdated. If everything matches then we need to set the install state to installed.
        var installedModules = _moduleDataService.GetInstalledModules().Concat(_moduleDataService.GetLegacyModules());
        var localModules = new List<InstallableTrackingModule>();    // dw about it
        foreach (var installedModule in installedModules)
        {
            installedModule.InstallationState = InstallState.Installed;
            var remoteModule = data.FirstOrDefault(x => x.ModuleId == installedModule.ModuleId);
            if (remoteModule == null)   // If this module is completely missing from the remote list, then we need to add it to the list.
            {
                // This module is installed but not in the remote list, so we need to add it to the list.
                localModules.Add(installedModule);
            }
            else
            {
                // This module is installed and in the remote list, so we need to update the remote module's install state.
                remoteModule.InstallationState = remoteModule.Version != installedModule.Version ? InstallState.Outdated : InstallState.Installed;
            }
        }

        // Sort our data by name, then place any modules with the author name VRCFT Team at the top of the list. (unbiased)
        data = data.OrderByDescending(x => x.InstallationState == InstallState.Installed)
            .ThenByDescending(x => x.AuthorName == "VRCFT Team")
            .ThenBy(x => x.ModuleName);
        
        // Then prepend the local modules to the list.
        data = localModules.Concat(data);

        foreach (var item in data)
        {
            ModuleInfos.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    public void EnsureItemSelected()
    {
        if (Selected == null && ModuleInfos.Any())
        {
            Selected = ModuleInfos.First();
        }
    }
}
