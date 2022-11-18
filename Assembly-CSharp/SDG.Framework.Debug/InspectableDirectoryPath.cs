namespace SDG.Framework.Debug;

public struct InspectableDirectoryPath : IInspectablePath
{
    public string absolutePath { get; set; }

    public bool isValid => !string.IsNullOrEmpty(absolutePath);

    public override string ToString()
    {
        return absolutePath;
    }
}
