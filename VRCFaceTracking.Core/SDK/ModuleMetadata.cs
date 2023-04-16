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
        internal set
        {
            _active = value;
            OnActiveChange?.Invoke(value);
        }
    }
}