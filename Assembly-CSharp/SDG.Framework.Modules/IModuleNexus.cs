namespace SDG.Framework.Modules;

/// <summary>
/// ModuleHook looks for module entry/exit points, then calls <see cref="M:SDG.Framework.Modules.IModuleNexus.initialize" /> when enabled and <see cref="M:SDG.Framework.Modules.IModuleNexus.shutdown" /> when disabled.
/// </summary>
public interface IModuleNexus
{
    /// <summary>
    /// Register components of this module.
    /// </summary>
    void initialize();

    /// <summary>
    /// Cleanup after this module.
    /// </summary>
    void shutdown();
}
