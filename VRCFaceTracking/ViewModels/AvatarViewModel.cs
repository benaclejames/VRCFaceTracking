using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking_Next.Core.Contracts.Services;

namespace VRCFaceTracking_Next.ViewModels;

public class AvatarViewModel : ObservableRecipient, IAvatarInfo
{
    private string _name;
    private string _id;

    private int _currentParameters;
    private int _currentParametersLegacy; // How many parameters are we using that are legacy
    
    public string Name
    {
        get => _name;
        set => App.MainWindow.DispatcherQueue.TryEnqueue(() => SetProperty(ref _name, value));
    }
    
    public string Id
    {
        get => _id;
        set => App.MainWindow.DispatcherQueue.TryEnqueue(() => SetProperty(ref _id, value));    // Literally suck on my penis windows
    }
    
    public int CurrentParameters
    {
        get => _currentParameters;
        set => App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            SetProperty(ref _currentParameters, value);
            OnPropertyChanged(nameof(CurrentParametersText));
        });
    }
    
    public int CurrentParametersLegacy
    {
        get => _currentParametersLegacy;
        set => App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            SetProperty(ref _currentParametersLegacy, value);
            OnPropertyChanged(nameof(CurrentParametersLegacyText));
            OnPropertyChanged(nameof(HasAnyLegacy));
        });
    }
    
    public string CurrentParametersText => $"{CurrentParameters} Parameters";
    public string CurrentParametersLegacyText => $"{CurrentParametersLegacy} Legacy Parameters";
    public bool HasAnyLegacy => CurrentParametersLegacy > 0;
    
    public AvatarViewModel()
    {
        _name = "Loading...";
        _id = "Loading...";
    }
}