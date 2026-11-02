using Avalonia;
using ReactiveUI.Avalonia;

namespace VRCFaceTracking;

internal class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
    {
        var builder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();

        /*if (OperatingSystem.IsLinux() && Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") is not null)
        {
            builder = builder.UseWayland();
        }*/

        return builder;
    }
}