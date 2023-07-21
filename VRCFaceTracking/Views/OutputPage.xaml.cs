using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using System.Globalization;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Controls;

using VRCFaceTracking.ViewModels;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage;
using VRCFaceTracking.Services;

namespace VRCFaceTracking.Views;

public sealed partial class OutputPage : Page
{
    public OutputViewModel ViewModel
    {
        get;
    }

    public ObservableCollection<string> FilteredLog => OutputPageLogger.FilteredLogs;
    public ObservableCollection<string> AllLog => OutputPageLogger.AllLogs;

    public OutputPage()
    {
        ViewModel = App.GetService<OutputViewModel>();
        InitializeComponent();
    }
    
    private void ScrollToBottom() => LogScroller.ChangeView(null, LogScroller.ScrollableHeight, null);

    private async void SaveToFile_OnClick(object sender, RoutedEventArgs e)
    {
        // Create a file picker
        FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var window = App.MainWindow;
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // Initialize the file picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

        // Set options for your file picker
        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
        DateTime now = DateTime.Now;
        string format = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + "-" + CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
        var dateFormatted = now.ToString(format);
        savePicker.SuggestedFileName = $"VRCFaceTracking.txt-{dateFormatted}";

        // Open the picker for the user to pick a file
        StorageFile file = await savePicker.PickSaveFileAsync();
        if (file != null)
        {
            CachedFileManager.DeferUpdates(file);

            // write to file
            var logString = AllLog.Aggregate("", (current, log) => current + log + "\n");
            await FileIO.AppendTextAsync(file, logString);

            FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
            if (status == FileUpdateStatus.Complete)
            {
                SaveStatus.Text = "File " + file.Name + " was saved.";
            }
            else if (status == FileUpdateStatus.CompleteAndRenamed)
            {
                SaveStatus.Text = "File " + file.Name + " was renamed and saved.";
            }
            else
            {
                SaveStatus.Text = "File " + file.Name + " couldn't be saved.";
            }
        }
        else
        {
            SaveStatus.Text = "Operation cancelled.";
        }
    }

    private void CopyToClipboard_OnClick(object sender, RoutedEventArgs e)
    {
        var logString = AllLog.Aggregate("", (current, log) => current + log + "\n");
        var package = new DataPackage();
        package.SetText(logString);
        Clipboard.SetContent(package);
        SaveStatus.Text = "Copied to clipboard.";
    }

    private void LogScroller_OnLoaded(object sender, RoutedEventArgs e)
    {
        ScrollToBottom();
        
        // We need to subscribe to the observablecollection onchanged event to scroll to the bottom. Note that we need a small delay because windows.
        // If we don't then we'll be scrolling a line too short.
        FilteredLog.CollectionChanged += (sender, args) =>
        {
            // Start a timer for 1ms to scroll to the bottom
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                ScrollToBottom();
            };
            timer.Start();
        };
    }
}
