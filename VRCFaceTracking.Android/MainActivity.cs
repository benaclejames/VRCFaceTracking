using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Avalonia;
using Avalonia.Android;
using ReactiveUI.Avalonia;

namespace VRCFaceTracking.Android;

[Activity(
    Label = "VRCFaceTracking",
    Theme = "@style/VrcftTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    private const int NotificationPermissionRequestCode = 1;

    protected override AppBuilder CreateAppBuilder()
    {
        // Host.CreateDefaultBuilder watches appsettings.json for changes by default. There is no such file on Android
        // and a FileSystemWatcher on the process' working directory is wasteful at best, so turn it off.
        System.Environment.SetEnvironmentVariable("DOTNET_hostBuilder__reloadConfigOnChange", "false");

        return AppBuilder.Configure(() => new App(new AndroidPlatformServices()))
            .UseAndroid()
            .UseReactiveUI();
    }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu &&
            CheckSelfPermission(Manifest.Permission.PostNotifications) != Permission.Granted)
        {
            RequestPermissions([Manifest.Permission.PostNotifications], NotificationPermissionRequestCode);
        }
    }

    protected override void OnDestroy()
    {
        // Only tear down tracking when the user actually closes the app, not on configuration changes
        if (IsFinishing && Avalonia.Application.Current is App app)
        {
            // Give the mutator a chance to persist settings; Android may kill the process right after this
            app.ShutdownAsync().Wait(TimeSpan.FromSeconds(2));
        }

        base.OnDestroy();
    }
}
