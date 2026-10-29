using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using FluentAvalonia.UI.Controls;
using VRCFaceTracking.Contracts;
using VRCFaceTracking.Core.Contracts.Services;

namespace VRCFaceTracking.Views;

public partial class ShellPage : UserControl
{
    private readonly MainPage _mainPage = new();
    private readonly OutputPage _outputPage = new();
    private readonly ModuleRegistryPage _moduleRegistryPage = new();
    private readonly MutatorPage _mutatorPage = new();
    private readonly SettingsPage _settingsPage = new();

    private readonly ILibManager _libManager;
    private Control? _currentPage;
    private object? _lastSelectedItem;
    private bool _suppressSelection;

    public ShellPage()
    {
        InitializeComponent();

        _libManager = Ioc.Default.GetRequiredService<ILibManager>();
        _libManager.ReloadPendingChanged += (_, _) => Dispatcher.UIThread.Post(UpdateReloadItem);

        // The NavigationView footer does not re-layout when an item's IsVisible flips, so the
        // reload item is added to / removed from the footer collection instead.
        NavView.FooterMenuItems.Remove(ReloadNavItem);
        UpdateReloadItem();

        PageContainer.Children.Add(_mainPage);
        PageContainer.Children.Add(_outputPage);
        PageContainer.Children.Add(_moduleRegistryPage);
        PageContainer.Children.Add(_mutatorPage);
        PageContainer.Children.Add(_settingsPage);

        foreach (var child in PageContainer.Children)
            child.IsVisible = false;

        // Select the first nav item and show the main page
        NavView.SelectedItem = NavView.MenuItems[0];
        _lastSelectedItem = NavView.SelectedItem;
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

    private async void OnNavigationSelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (_suppressSelection)
            return;

        if (e.IsSettingsSelected)
        {
            _lastSelectedItem = e.SelectedItem;
            ShowPage(_settingsPage);
            return;
        }

        if (e.SelectedItem is NavigationViewItem { Tag: string tag })
        {
            if (tag == "ReloadModules")
            {
                await ReloadModulesAsync();

                _suppressSelection = true;
                NavView.SelectedItem = _lastSelectedItem;
                _suppressSelection = false;
                return;
            }

            _lastSelectedItem = e.SelectedItem;
            ShowPage(tag switch
            {
                "Main" => (Control)_mainPage,
                "Output" => _outputPage,
                "ModuleRegistry" => _moduleRegistryPage,
                "Mutator" => _mutatorPage,
                _ => _mainPage
            });
        }
    }

    private void UpdateReloadItem()
    {
        var items = NavView.FooterMenuItems;
        var present = items.Contains(ReloadNavItem);

        if (_libManager.IsReloadPending && !present)
            items.Add(ReloadNavItem);
        else if (!_libManager.IsReloadPending && present)
            items.Remove(ReloadNavItem);
    }

    private async Task ReloadModulesAsync()
    {
        ReloadNavItem.IsEnabled = false;
        try
        {
            await _libManager.ReloadModulesAsync();

            // Re-read applied module states so the registry view reflects the reload
            if (_currentPage is INotifyNavigated notifyNavigated)
                notifyNavigated.OnNavigatedTo();
        }
        finally
        {
            ReloadNavItem.IsEnabled = true;
        }
    }
}

