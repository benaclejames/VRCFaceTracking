using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace VRCFaceTracking.Views;

public sealed partial class WarningBlock : UserControl
{
    public static readonly DependencyProperty IsVisibleProperty =
        DependencyProperty.Register("IsVisible", typeof(bool), typeof(WarningBlock), new PropertyMetadata(true));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(WarningBlock), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register("Description", typeof(string), typeof(WarningBlock), new PropertyMetadata(string.Empty));
    
    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register("ButtonText", typeof(string), typeof(WarningBlock), new PropertyMetadata(string.Empty));

    public bool IsVisible
    {
        get { return (bool)GetValue(IsVisibleProperty); }
        set { SetValue(IsVisibleProperty, value); }
    }

    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public string Description
    {
        get { return (string)GetValue(DescriptionProperty); }
        set { SetValue(DescriptionProperty, value); }
    }
    
    public string ButtonText
    {
        get { return (string)GetValue(ButtonTextProperty); }
        set { SetValue(ButtonTextProperty, value); }
    }

    public event RoutedEventHandler LinkClickHandler;

    public WarningBlock()
    {
        InitializeComponent();
    }

    private void HyperlinkButton_OnClick(object sender, RoutedEventArgs e)
    {
        LinkClickHandler?.Invoke(this, e);
    }

    private void DismissButton_OnClick(object sender, RoutedEventArgs e)
    {
        IsVisible = false;
    }
}