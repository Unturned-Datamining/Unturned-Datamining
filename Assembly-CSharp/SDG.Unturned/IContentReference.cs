namespace SDG.Unturned;

public interface IContentReference
{
    /// <summary>
    /// Name of the asset bundle.
    /// </summary>
    /// <example>core.content</example>
    string name { get; set; }

    /// <summary>
    /// Path within the asset bundle.
    /// </summary>
    string path { get; set; }

    bool isValid { get; }
}
