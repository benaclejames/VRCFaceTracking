using CommunityToolkit.Mvvm.ComponentModel;
using VRCFaceTracking_Next.Core.Contracts.Services;

namespace VRCFaceTracking_Next.ViewModels;

public class HomeViewModel : ObservableRecipient
{
    private IMainService MainService
    {
        get;
    }

    private bool eyeEnabled = true, expressionEnabled = true;

    public bool EyeEnabled
    {
        get => eyeEnabled;
        set
        {
            MainService.SetEnabled(value, ExpressionEnabled);
            SetProperty(ref eyeEnabled, value);
        }
    }

    public bool ExpressionEnabled
    {
        get => expressionEnabled;
        set
        {
            MainService.SetEnabled(value, ExpressionEnabled);
            SetProperty(ref expressionEnabled, value);
        }
    }

    public HomeViewModel(IMainService mainService)
    {
        MainService = mainService;
    }
}
