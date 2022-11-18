using System.Collections.Generic;
using Newtonsoft.Json;

namespace SDG.Framework.Modules;

public class ModuleConfig
{
    public bool IsEnabled;

    [JsonIgnore]
    public string DirectoryPath;

    [JsonIgnore]
    public string FilePath;

    public string Name;

    public string Version;

    [JsonIgnore]
    public uint Version_Internal;

    public List<ModuleDependency> Dependencies;

    public List<ModuleAssembly> Assemblies;

    public ModuleConfig()
    {
        IsEnabled = true;
        Name = string.Empty;
        Version = "1.0.0.0";
        Dependencies = new List<ModuleDependency>(0);
        Assemblies = new List<ModuleAssembly>(0);
    }
}
