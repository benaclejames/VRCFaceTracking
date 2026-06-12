using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using VRCFaceTracking.Services;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public partial class OutputPage : UserControl
{
    private OutputViewModel ViewModel => (OutputViewModel)DataContext!;

    public OutputPage()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<OutputViewModel>();

        // Auto-scroll when new log lines arrive
        OutputPageLogger.FilteredLogs.CollectionChanged += OnLogsChanged;
    }

    private void OnLogsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        LogScroller.ScrollToEnd();
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
