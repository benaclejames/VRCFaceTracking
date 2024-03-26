namespace VRCFaceTracking;

public struct ModuleMetadata
{
    public delegate void ActiveChange(bool state);
    public ActiveChange OnActiveChange;

    public List<Stream> StaticImages { get; set; }
    public string Name { get; set; }
    private bool _active;

    public bool Active
    {
        get => _active;
        set
        {
            _active = value;
            OnActiveChange?.Invoke(value);
        }
    }
    
    //Temporary for the menu display
    private bool _usingEye;
    private bool _usingExpression;
    
    public bool UsingEye
    {
        get => _usingEye;
        set => _usingEye = value;
    }
    
    public bool UsingExpression
    {
        get => _usingExpression;
        set => _usingExpression = value;
    }
}