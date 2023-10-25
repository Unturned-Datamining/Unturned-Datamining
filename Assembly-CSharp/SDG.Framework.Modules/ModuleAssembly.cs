using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SDG.Framework.Modules;

public class ModuleAssembly
{
    public string Path;

    [JsonConverter(typeof(StringEnumConverter))]
    public EModuleRole Role;

    /// <summary>
    /// Requested by Trojaner. LoadFile locks the file while in use which prevents OpenMod from updating itself.
    /// </summary>
    public bool Load_As_Byte_Array;

    public ModuleAssembly()
    {
        Path = string.Empty;
        Role = EModuleRole.None;
        Load_As_Byte_Array = false;
    }
}
