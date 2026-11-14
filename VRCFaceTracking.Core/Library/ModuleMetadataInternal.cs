using System.ComponentModel;

namespace VRCFaceTracking;

// Internal version of ModuleMetadata, done to not break compat
public class ModuleMetadataInternal : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public List<Stream> StaticImages { get; set; }
    public string Name { get; set; }
    private bool _active;

    public bool Active
    {
        get => _active;
        set
        {
            _active = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active)));
        }
    }

    //Temporary for the menu display
    private bool _usingEye;
    private bool _usingExpression;
    public bool InitializedEye { get; set; }
    public bool InitializedExpression { get; set; }
    public int Priority { get; set; }


    public bool UsingEye
    {
        get => _usingEye;
        set
        {
            _usingEye = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UsingEye)));
        }
    }

    public bool UsingExpression
    {
        get => _usingExpression;
        set
        {
            _usingExpression = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UsingExpression)));
        }
    }
}