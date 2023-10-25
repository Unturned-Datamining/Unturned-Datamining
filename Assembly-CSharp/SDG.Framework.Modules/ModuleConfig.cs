using System.Collections.Generic;
using Newtonsoft.Json;

namespace SDG.Framework.Modules;

/// <summary>
/// Holds module configuration.
/// </summary>
public class ModuleConfig
{
    /// <summary>
    /// Whether to load assemblies.
    /// </summary>
    public bool IsEnabled;

    /// <summary>
    /// Directory containing Module file, set when loading.
    /// </summary>
    [JsonIgnore]
    public string DirectoryPath;

    /// <summary>
    /// Path to the Module file, set when loading.
    /// </summary>
    [JsonIgnore]
    public string FilePath;

    /// <summary>
    /// Used for module dependencies.
    /// </summary>
    public string Name;

    /// <summary>
    /// Nicely formatted version, converted into <see cref="F:SDG.Framework.Modules.ModuleConfig.Version_Internal" />.
    /// </summary>
    public string Version;

    /// <summary>
    /// Used for module dependencies.
    /// </summary>
    [JsonIgnore]
    public uint Version_Internal;

    /// <summary>
    /// Modules that must be loaded before this module.
    /// </summary>
    public List<ModuleDependency> Dependencies;

    /// <summary>
    /// Relative file paths of .dlls to load.
    /// </summary>
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
