using System;
using System.Collections.Generic;
using System.Reflection;
using SDG.Unturned;
using Unturned.SystemEx;

namespace SDG.Framework.Modules;

/// <summary>
/// Wraps module assembly and handles initialization.
/// </summary>
public class Module
{
    /// <summary>
    /// True when config is enabled and dependencies are enabled.
    /// </summary>
    protected bool _isEnabled;

    private List<IModuleNexus> nexii;

    public bool isEnabled
    {
        get
        {
            return _isEnabled;
        }
        set
        {
            if (isEnabled != value)
            {
                _isEnabled = value;
                if (isEnabled)
                {
                    load();
                    initialize();
                }
                else
                {
                    shutdown();
                }
            }
        }
    }

    /// <summary>
    /// Metadata.
    /// </summary>
    public ModuleConfig config { get; protected set; }

    /// <summary>
    /// Assembly files loaded.
    /// </summary>
    public Assembly[] assemblies { get; protected set; }

    /// <summary>
    /// Types in the assemblies of this module. Refer to this for types rather than the assemblies to avoid exception and garbage.
    /// </summary>
    public Type[] types { get; protected set; }

    /// <summary>
    /// How far along the initialization to shutdown lifecycle this module is.
    /// </summary>
    public EModuleStatus status { get; protected set; }

    public event ModuleLoaded onModuleLoaded;

    public event ModuleInitialized onModuleInitialized;

    public event ModuleShutdown onModuleShutdown;

    protected void register()
    {
        if (config == null)
        {
            return;
        }
        foreach (ModuleAssembly assembly in config.Assemblies)
        {
            ModuleHook.registerAssemblyPath(config.DirectoryPath + assembly.Path, assembly.Load_As_Byte_Array);
        }
    }

    protected void load()
    {
        if (config == null || assemblies != null || !config.IsEnabled)
        {
            return;
        }
        List<Type> list = new List<Type>();
        assemblies = new Assembly[config.Assemblies.Count];
        for (int i = 0; i < config.Assemblies.Count; i++)
        {
            Assembly assembly = ModuleHook.resolveAssemblyPath(config.DirectoryPath + config.Assemblies[i].Path);
            assemblies[i] = assembly;
            Type[] array;
            try
            {
                array = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                array = ex.Types;
            }
            if (array == null)
            {
                continue;
            }
            for (int j = 0; j < array.Length; j++)
            {
                if (!(array[j] == null))
                {
                    list.Add(array[j]);
                }
            }
        }
        types = list.ToArray();
        this.onModuleLoaded?.Invoke(this);
    }

    protected void initialize()
    {
        if (config == null || assemblies == null || (status != 0 && status != EModuleStatus.Shutdown))
        {
            return;
        }
        nexii.Clear();
        Type typeFromHandle = typeof(IModuleNexus);
        for (int i = 0; i < types.Length; i++)
        {
            Type type = types[i];
            try
            {
                if (!type.IsAbstract && typeFromHandle.TryIsAssignableFrom(type))
                {
                    IModuleNexus moduleNexus = Activator.CreateInstance(type) as IModuleNexus;
                    try
                    {
                        moduleNexus.initialize();
                    }
                    catch (Exception e)
                    {
                        UnturnedLog.error("Caught exception while initializing module \"" + config.Name + "\" entry point \"" + type.Name + "\":");
                        UnturnedLog.exception(e);
                    }
                    nexii.Add(moduleNexus);
                }
            }
            catch (Exception e2)
            {
                UnturnedLog.exception(e2, "Caught exception while searching for entry points in module \"" + config.Name + "\" type \"" + type.Name + "\"");
            }
        }
        status = EModuleStatus.Initialized;
        UnturnedLog.info("Initialized module \"" + config.Name + "\"");
        this.onModuleInitialized?.Invoke(this);
    }

    protected void shutdown()
    {
        if (config == null || assemblies == null || status != EModuleStatus.Initialized)
        {
            return;
        }
        for (int i = 0; i < nexii.Count; i++)
        {
            try
            {
                nexii[i].shutdown();
            }
            catch (Exception e)
            {
                UnturnedLog.error("Caught exception while shutting down module \"" + config.Name + "\":");
                UnturnedLog.exception(e);
            }
        }
        nexii.Clear();
        status = EModuleStatus.Shutdown;
        UnturnedLog.info("Shutdown module \"" + config.Name + "\"");
        this.onModuleShutdown?.Invoke(this);
    }

    public Module(ModuleConfig newConfig)
    {
        config = newConfig;
        isEnabled = false;
        status = EModuleStatus.None;
        nexii = new List<IModuleNexus>();
        register();
    }
}
