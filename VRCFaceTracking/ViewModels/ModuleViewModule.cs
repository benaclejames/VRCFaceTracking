using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace VRCFaceTracking_Next.ViewModels;

public class ModuleViewModule : ObservableRecipient
{
    private List<ImageSource> images;
    private string name;
    private bool active;

    public List<ImageSource> Images
    {
        get => images;
        set => SetProperty(ref images, value);
    }
    
    public string Name 
    {
        get => name;
        set => SetProperty(ref name, value);
    }
    
    public bool Active
    {
        get => active;
        set => SetProperty(ref active, value);
    }
    
    public ModuleViewModule()
    {
        images = new List<ImageSource>();
        active = true;
        name = "Module";
    }
}