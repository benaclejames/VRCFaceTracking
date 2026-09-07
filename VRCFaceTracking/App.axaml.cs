using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using Avalonia.Data.Core.Plugins;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.Styling;
using Microsoft.Extensions.Hosting;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Views;

namespace VRCFaceTracking
{
    public partial class App : Application
    {
        private ILogger? _logger;

        public static MainWindow? MainWindow { get; private set; }
        private IHost? _host;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            // gsettings breaks gamescope on linux and system theme is unreliable on plasma anyways
            if (!OperatingSystem.IsLinux())
            {
                var faTheme = Styles.OfType<FluentAvaloniaTheme>().FirstOrDefault();
                if (faTheme is not null)
                    faTheme.PreferSystemTheme = true;
            }

#if DEBUG
            var loggerFactory = LoggerFactory.Create(b => b
                .SetMinimumLevel(LogLevel.Information)
                .AddConsole());

            this.AttachDeveloperTools(o =>
            {
                o.AddMicrosoftLoggerObservable(loggerFactory);
            });

            _logger = loggerFactory.CreateLogger("App");
#endif
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // Remove duplicate Avalonia/CommunityToolkit data validation
            BindingPlugins.DataValidators.RemoveAt(0);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>services.AddCommonServices())
                .Build();
            Ioc.Default.ConfigureServices(_host.Services);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                MainWindow = new MainWindow();
                desktop.MainWindow = MainWindow;
                desktop.Exit += (_, _) =>
                {
                    Task.Run(async () =>
                    {
                        await Ioc.Default.GetRequiredService<IMainService>().Teardown();
                        await _host.StopAsync();
                    }).GetAwaiter().GetResult();
                };
            }

            HandleResetFile();

            Core.Utils.KillAllProcessesOfName("VRCFaceTracking");
            Core.Utils.KillAllProcessesOfName("VRCFaceTracking.ModuleProcess");

            _ = Task.Run(() => _host.StartAsync());
            Ioc.Default.GetRequiredService<IActivationService>().ActivateAsync(null);

            base.OnFrameworkInitializationCompleted();
        }

        private void HandleResetFile()
        {
            var resetFile = Path.Combine(VRCFaceTracking.Core.Utils.PersistentDataDirectory, "reset");
            if (!File.Exists(resetFile)) return;

            try
            {
                foreach (var file in Directory.EnumerateFiles(
                             VRCFaceTracking.Core.Utils.PersistentDataDirectory, "*", SearchOption.AllDirectories))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to clean directory during reset");
            }
            finally
            {
                try { File.Delete(resetFile); }
                catch {}
            }
        }
    }
}
