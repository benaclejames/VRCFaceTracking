using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public sealed partial class ParametersPage : Page
{
    public ParametersViewModel ViewModel { get; }
    
    public IMainService MainService
    {
        get;
    }

    private ObservableCollection<ParameterDebugUserControl> _trackedParameters = new();

    public ParametersPage()
    {
        ViewModel = App.GetService<ParametersViewModel>();
        MainService = App.GetService<IMainService>();
        var dispatcher = App.GetService<IDispatcherService>();
        _trackedParameters.Add(new ParameterDebugUserControl()
        {
            ViewModel =
            {
                ParameterName = "REEE",
                ParameterValue = 0.5f
            }
        });
        
        this.DataContext = this;
        InitializeComponent();

        MainService.ParameterUpdate += (addr, val) => dispatcher.Run(() => OnParameterSend(addr, val));
    }

    private void OnParameterSend(string address, float value)
    {
        // Gotta extract the name from the address by getting the last split /
        var name = address.Split('/').Last();

        // First we check to see if we already have a control for this parameter
        var existingControl = _trackedParameters.FirstOrDefault(x => x.ViewModel.ParameterName == name);
        
        // If we don't, add one
        if (existingControl == null)
        {
            var newControl = new ParameterDebugUserControl();
            _trackedParameters.Add(newControl);
            newControl.ViewModel.ParameterName = name;
            newControl.ViewModel.ParameterValue = value;
        }
        
        // If we do, update the value
        else
        {
            existingControl.ViewModel.ParameterValue = value;
        }
    }
}
