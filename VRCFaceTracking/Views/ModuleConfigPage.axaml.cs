using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.DependencyInjection;
using VRCFaceTracking.Core.Contracts.Services;
using VRCFaceTracking.ViewModels;

namespace VRCFaceTracking.Views;

public partial class ModuleConfigPage : UserControl
{
    private readonly ILibManager _libManager;
    private Task? _reloadTask;
    
    public ModuleConfigPage()
    {
        InitializeComponent();
        DataContext = Ioc.Default.GetRequiredService<ModuleConfigViewModel>();
        _libManager = Ioc.Default.GetRequiredService<ILibManager>();
    }

    private void ReloadButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (_reloadTask is not null && !_reloadTask.IsCompleted)
        {
            return;
        }
        
        _reloadTask = Task.Run(async () =>
        {
            await _libManager.TeardownAllModules();
            await _libManager.Initialize();
        });
    }
}
