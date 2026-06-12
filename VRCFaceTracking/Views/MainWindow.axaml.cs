using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace VRCFaceTracking.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        try
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "WindowIcon.ico");
            if (File.Exists(iconPath))
            {
                using var stream = File.OpenRead(iconPath);
                Icon = new WindowIcon(stream);
            }
        }
        catch { }
    }
}
