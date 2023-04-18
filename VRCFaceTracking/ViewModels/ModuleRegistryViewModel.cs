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

        // TODO: Replace with real data.
        var data = await _moduleDataService.GetListDetailsDataAsync();

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
