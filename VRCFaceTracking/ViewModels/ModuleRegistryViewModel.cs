using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using VRCFaceTracking.Contracts.ViewModels;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.ViewModels;

public class ModuleRegistryViewModel : ObservableRecipient, INavigationAware
{
    private readonly IModuleDataService _moduleDataService;
    private RemoteTrackingModule? _selected;

    public RemoteTrackingModule? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public ObservableCollection<RemoteTrackingModule> ModuleInfos { get; private set; } = new ObservableCollection<RemoteTrackingModule>();
    
    public ModuleRegistryViewModel(IModuleDataService moduleDataService)
    {
        _moduleDataService = moduleDataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        ModuleInfos.Clear();

        var data = await _moduleDataService.GetListDetailsDataAsync();
        
        // Now comes the tricky bit, we get all locally installed modules and add them to the list.
        // If any of the IDs match a remote module and the other data contained within does not match,
        // then we need to set the local module install state to outdated. If everything matches then we need to set the install state to installed.
        var installedModules = await _moduleDataService.GetInstalledModulesAsync();
        foreach (var installedModule in installedModules)
        {
            var remoteModule = data.FirstOrDefault(x => x.ModuleId == installedModule.ModuleId);
            if (remoteModule == null)   // If this module is completely missing from the remote list, then we need to add it to the list.
            {
                // This module is installed but not in the remote list, so we need to add it to the list.
                data = data.Append(installedModule);
            }
            else
            {
                // This module is installed and in the remote list, so we need to update the remote module's install state.
                if (remoteModule.Version != installedModule.Version)
                {
                    remoteModule.InstallationState = InstallState.Outdated;
                }
                else
                {
                    remoteModule.InstallationState = InstallState.Installed;
                }
            }
        }

        // Sort our data so that installed or outdated modules are at the top of the list.
        data = data.OrderByDescending(x => x.InstallationState);
        
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
        if (Selected == null)
        {
            Selected = ModuleInfos.First();
        }
    }
}
