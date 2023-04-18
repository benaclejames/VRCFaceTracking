using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

using VRCFaceTracking.Contracts.ViewModels;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Models;

namespace VRCFaceTracking.ViewModels;

public class ModuleRegistryViewModel : ObservableRecipient, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;
    private RemoteTrackingModule? _selected;

    public RemoteTrackingModule? Selected
    {
        get => _selected;
        set => SetProperty(ref _selected, value);
    }

    public ObservableCollection<RemoteTrackingModule> SampleItems { get; private set; } = new ObservableCollection<RemoteTrackingModule>();

    public ModuleRegistryViewModel(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        SampleItems.Clear();

        // TODO: Replace with real data.
        var data = await _sampleDataService.GetListDetailsDataAsync();

        foreach (var item in data)
        {
            SampleItems.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    public void EnsureItemSelected()
    {
        if (Selected == null)
        {
            Selected = SampleItems.First();
        }
    }
}
