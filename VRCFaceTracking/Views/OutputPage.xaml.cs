using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging.EventSource;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking_Next.Services;
using VRCFaceTracking_Next.ViewModels;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage;
using System.Globalization;

namespace VRCFaceTracking_Next.Views;

public sealed partial class OutputPage : Page
{
    public OutputViewModel ViewModel
    {
        get;
    }

    public ObservableCollection<string> Log => LoggingService.Logs;

    public OutputPage()
    {
        ViewModel = App.GetService<OutputViewModel>();
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

            // write to file
            var textBox = ((sender as Button).Parent as StackPanel)
            .FindName("FileContentTextBox") as TextBox;
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                using (var tw = new StreamWriter(stream))
                {
                    tw.WriteLine(textBox?.Text);
                }
            }
            // Another way to write a string to the file is to use this instead:
            // await FileIO.WriteTextAsync(file, "Example file contents.");

            // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
            // Completing updates may require Windows to ask for user input.
            FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
            /*if (status == FileUpdateStatus.Complete)
            {
                SaveFileOutputTextBlock.Text = "File " + file.Name + " was saved.";
            }
            else if (status == FileUpdateStatus.CompleteAndRenamed)
            {
                SaveFileOutputTextBlock.Text = "File " + file.Name + " was renamed and saved.";
            }
            else
            {
                SaveFileOutputTextBlock.Text = "File " + file.Name + " couldn't be saved.";
            }*/
        }
        else
        {
            //SaveFileOutputTextBlock.Text = "Operation cancelled.";
        }

    }
}
