using System.Diagnostics;
using Avalonia;
using ReactiveUI.Avalonia;

namespace VRCFaceTracking.Desktop;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Single-instance model: take over from any previous VRCFT and its orphaned module sandboxes
        Core.Utils.KillAllProcessesOfName(Process.GetCurrentProcess().ProcessName);
        Core.Utils.KillAllProcessesOfName("VRCFaceTracking.ModuleProcess");

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Also used by the XAML previewer
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure(() => new App(new DesktopPlatformServices()))
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}
