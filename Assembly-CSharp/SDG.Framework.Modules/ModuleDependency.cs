using Newtonsoft.Json;

namespace SDG.Framework.Modules;

public class ModuleDependency
{
    public string Name;

    /// <summary>
    /// Nicely formatted version, converted into <see cref="F:SDG.Framework.Modules.ModuleDependency.Version_Internal" />.
    /// </summary>
    public string Version;

    /// <summary>
    /// Used for module dependencies.
    /// </summary>
    [JsonIgnore]
    public uint Version_Internal;

    public ModuleDependency()
    {
        Name = string.Empty;
        Version = "1.0.0.0";
    }
}
