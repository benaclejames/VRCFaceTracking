using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using VRCFaceTracking.Contracts;

namespace VRCFaceTracking.Views;

public partial class ShellPage : UserControl
{
    private readonly MainPage _mainPage = new();
    private readonly OutputPage _outputPage = new();
    private readonly ModuleConfigPage _moduleConfigPage = new();
    private readonly ModuleRegistryPage _moduleRegistryPage = new();
    private readonly MutatorPage _mutatorPage = new();
    private readonly SettingsPage _settingsPage = new();

    private Control? _currentPage;

    public ShellPage()
    {
        InitializeComponent();

        PageContainer.Children.Add(_mainPage);
        PageContainer.Children.Add(_outputPage);
        PageContainer.Children.Add(_moduleConfigPage);
        PageContainer.Children.Add(_moduleRegistryPage);
        PageContainer.Children.Add(_mutatorPage);
        PageContainer.Children.Add(_settingsPage);

        foreach (var child in PageContainer.Children)
            child.IsVisible = false;

        // Select the first nav item and show the main page
        NavView.SelectedItem = NavView.MenuItems[0];
        ShowPage(_mainPage);
    }

    private void ShowPage(Control page)
    {
        if (_currentPage != null)
            _currentPage.IsVisible = false;

        page.IsVisible = true;
        _currentPage = page;

        // Notify module registry when navigated to so it loads data
        if (page is INotifyNavigated notifyNavigated)
            notifyNavigated.OnNavigatedTo();
    }

    private void OnNavigationSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.IsSettingsSelected)
        {
            ShowPage(_settingsPage);
            return;
        }

        if (e.SelectedItem is NavigationViewItem { Tag: string tag })
        {
            ShowPage(tag switch
            {
                "Main" => (Control)_mainPage,
                "Output" => _outputPage,
                "ModuleConfig" => _moduleConfigPage,
                "ModuleRegistry" => _moduleRegistryPage,
                "Mutator" => _mutatorPage,
                _ => _mainPage
            });
        }
    }
}