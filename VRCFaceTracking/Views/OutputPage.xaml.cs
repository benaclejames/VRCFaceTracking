using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging.EventSource;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking_Next.Services;
using VRCFaceTracking_Next.ViewModels;

namespace VRCFaceTracking_Next.Views;

public sealed partial class OutputPage : Page
{
    public OutputViewModel ViewModel
    {
        get;
    }

    public ObservableCollection<string> Log => LoggingService.Logs;

    public OutputPage()
    {
        ViewModel = App.GetService<OutputViewModel>();
        InitializeComponent();
    }
}
