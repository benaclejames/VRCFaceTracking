using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.Contracts.Services;
using VRCFaceTracking.Helpers;

namespace VRCFaceTracking;

public sealed partial class MainWindow : WindowEx
{
    private bool _isClosing = false;

    private readonly IApplicationLifecycleService _lifecycleService;

    public MainWindow()
    {
        InitializeComponent();
        
        AppWindow.Closing += async (window, args) =>
        {
            // Prevent multiple close attempts
            if (_isClosing)
            {
                return;
            }

            _isClosing = true;
            args.Cancel = true;

            try
            {
                // Show shutdown
                var shutdownContent = new Grid();
                var progressRing = new ProgressRing { IsActive = true, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                var textBlock = new TextBlock { Text = "Shutting down...", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 60, 0, 0) };
                shutdownContent.Children.Add(progressRing);
                shutdownContent.Children.Add(textBlock);
                Content = shutdownContent;

                // Use the ApplicationLifecycleService to ensure proper shutdown sequence
                var lifecycleService = App.GetService<IApplicationLifecycleService>();

                // Register for progress updates if needed
                lifecycleService.ShutdownProgress += (sender, args) =>
                {
                    // Update UI with progress if desired
                    if (textBlock != null)
                    {
                        textBlock.Text = $"Shutting down: {args.Message} ({args.ProgressPercentage}%)";
                    }
                };

                await lifecycleService.ShutdownAsync();
            }
            catch (Exception ex)
            {
                // Log final error but continue with shutdown
                App.GetService<ILogger<MainWindow>>()?.LogError(ex, "Error during shutdown process");
            }
            finally
            {
                Close();
            }
        };

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
        Content = null;
        Title = "AppDisplayName".GetLocalized();
    }
}
