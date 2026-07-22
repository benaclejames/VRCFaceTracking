using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public partial class OutputPage : UserControl
{
    private OutputViewModel ViewModel => (OutputViewModel)DataContext!;
    private const double StickThreshold = 40;
    private bool _stickToBottom = true;

    public OutputPage()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<OutputViewModel>();

        LogItems.LayoutUpdated += OnLogItemsLayoutUpdated;
        LogItems.AddHandler(ScrollViewer.ScrollChangedEvent, OnLogItemsScrollChanged, RoutingStrategies.Bubble);
    }

    private void OnLogItemsLayoutUpdated(object? sender, EventArgs e)
    {
        if (!_stickToBottom || LogItems.Scroll is not { } scroll)
            return;

        var target = scroll.Extent.Height - scroll.Viewport.Height;
        if (target > 0 && scroll.Offset.Y < target)
            scroll.Offset = scroll.Offset.WithY(target);
    }

    private void OnLogItemsScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (e.OffsetDelta.Y == 0 || LogItems.Scroll is not { } scroll)
            return;

        var target = scroll.Extent.Height - scroll.Viewport.Height;
        _stickToBottom = target <= 0 || target - scroll.Offset.Y <= StickThreshold;
    }

    private async void CopyToClipboard_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var text = ViewModel.AllLogsText;
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(text);
            StatusText.Text = "Copied to clipboard.";
        }
    }

    private async void SaveToFile_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Log",
            SuggestedFileName = $"vrcft-log-{DateTime.Now:yyyyMMdd-HHmmss}.txt",
            FileTypeChoices = [new FilePickerFileType("Text") { Patterns = ["*.txt"] }]
        });

        if (file != null)
        {
            await using var stream = await file.OpenWriteAsync();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(ViewModel.AllLogsText);
            StatusText.Text = "Log saved.";
        }
    }
}
