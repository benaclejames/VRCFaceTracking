using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Services;
using VRCFaceTracking.ViewModels;
using WinRT;

namespace VRCFaceTracking.Views;

public sealed partial class OutputPage : Page
{
    public OutputViewModel ViewModel
    {
        get;
    }

    private IFileService FileService
    {
        get;
    }

    public ObservableCollection<string> Log => LoggingService.Logs;

    public OutputPage()
    {
        ViewModel = App.GetService<OutputViewModel>();
        FileService = App.GetService<IFileService>();
        InitializeComponent();
    }

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
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
            // write to file
            var logString = Log.Aggregate("", (current, log) => current + log + "\n");
            await FileIO.AppendTextAsync(file, logString);
        }
        else
        {
            Log.Add("Operation cancelled.");
        }
    }

    /* Create a file picker
        FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var window = App.MainWindow;
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // Initialize the file picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);

        // Set options for your file picker
        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        // Dropdown of file types the user can save the file as
        savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
        DateTime now = DateTime.Now;
        string format = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + "-" + CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
        var dateFormatted = now.ToString(format);
        savePicker.SuggestedFileName = $"VRCFaceTracking-{dateFormatted}.txt";

        // Open the picker for the user to pick a file
        StorageFile file = await savePicker.PickSaveFileAsync();
        if (file != null)
        {
            // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
            CachedFileManager.DeferUpdates(file);

            var logString = Log.Aggregate("", (current, log) => current + log + "\n");
            await FileIO.WriteTextAsync(file, logString);

            // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
            // Completing updates may require Windows to ask for user input.
            FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
            if (status == FileUpdateStatus.Complete)
            {
                //SaveFileOutputTextBlock.Text = "File " + file.Name + " was saved.";
            }
            else if (status == FileUpdateStatus.CompleteAndRenamed)
            {
                //SaveFileOutputTextBlock.Text = "File " + file.Name + " was renamed and saved.";
            }
            else
            {
                //SaveFileOutputTextBlock.Text = "File " + file.Name + " couldn't be saved.";
            }

        }
        else
        {
            //SaveFileOutputTextBlock.Text = "Operation cancelled.";
        }*/
}
