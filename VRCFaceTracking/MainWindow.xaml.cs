using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Helpers;

namespace VRCFaceTracking;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();
        
        AppWindow.Closing += (window, args) =>
        {
            App.GetService<IMainService>().Teardown();
        };

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();
    }
}
