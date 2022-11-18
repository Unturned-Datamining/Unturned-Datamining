using Newtonsoft.Json;

namespace SDG.Framework.Modules;

public class ModuleDependency
{
    public string Name;

    public string Version;

    [JsonIgnore]
    public uint Version_Internal;

    public ModuleDependency()
    {
        Name = string.Empty;
        Version = "1.0.0.0";
    }
}
