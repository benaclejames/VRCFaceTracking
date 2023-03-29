using Microsoft.Extensions.Logging.EventSource;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;

using VRCFaceTracking_Next.ViewModels;

namespace VRCFaceTracking_Next.Views;

public sealed partial class OutputPage : Page
{
    public OutputViewModel ViewModel
    {
        get;
    }

    private EventSourceLoggerProvider provider;

    public OutputPage()
    {
        ViewModel = App.GetService<OutputViewModel>();
        InitializeComponent();
    }
}
