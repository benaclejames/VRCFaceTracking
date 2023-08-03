﻿using Windows.Storage.Pickers;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.Core.Services;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public sealed partial class ModuleRegistryPage : Page
{
    public ModuleRegistryViewModel ViewModel
    {
        get;
    }
    
    private ModuleInstaller ModuleInstaller
    {
        get;
    }

    private ILibManager LibManager
    {
        get;
    }

    public ModuleRegistryPage()
    {
        ViewModel = App.GetService<ModuleRegistryViewModel>();
        ModuleInstaller = App.GetService<ModuleInstaller>();
        LibManager = App.GetService<ILibManager>();
        InitializeComponent();
    }

    private void OnViewStateChanged(object sender, ListDetailsViewState e)
    {
        if (e == ListDetailsViewState.Both)
        {
            ViewModel.EnsureItemSelected();
        }
    }

    private async void InstallCustomModule_OnClick(object sender, RoutedEventArgs e)
    {
        CustomInstallStatus.Text = "";

        // Create a file picker
        var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var window = App.MainWindow;
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // Initialize the file picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your file picker
        openPicker.ViewMode = PickerViewMode.Thumbnail;
        openPicker.FileTypeFilter.Add("*");

        // Open the picker for the user to pick a file
        var file = await openPicker.PickSingleFileAsync();
        if (file != null)
        {
            var path = await ModuleInstaller.InstallLocalModule(file.Path);
            if (path != null)
            {
                CustomInstallStatus.Text = "Successfully installed module.";
                App.MainWindow.DispatcherQueue.TryEnqueue(() => LibManager.Initialize());
            }
            else
            {
                CustomInstallStatus.Text = "Failed to install module. Check logs for more information.";
            }
        }
        else
        {
            CustomInstallStatus.Text = "Operation cancelled.";
        }
    }
}
