using System;
using System.Collections.Generic;
using System.Reflection;
using SDG.Unturned;

namespace SDG.Framework.Modules;

public class Module
{
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

    public ModuleConfig config { get; protected set; }

    public Assembly[] assemblies { get; protected set; }

    public Type[] types { get; protected set; }

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
        if (this.onModuleLoaded != null)
        {
            this.onModuleLoaded(this);
        }
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
            if (!type.IsAbstract && typeFromHandle.IsAssignableFrom(type))
            {
                IModuleNexus moduleNexus = Activator.CreateInstance(type) as IModuleNexus;
                try
                {
                    moduleNexus.initialize();
                }
                catch (Exception e)
                {
                    UnturnedLog.error("Failed to initialize nexus!");
                    UnturnedLog.exception(e);
                }
                nexii.Add(moduleNexus);
            }
        }
        status = EModuleStatus.Initialized;
        if (this.onModuleInitialized != null)
        {
            this.onModuleInitialized(this);
        }
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
                UnturnedLog.error("Failed to shutdown nexus!");
                UnturnedLog.exception(e);
            }
        }
        nexii.Clear();
        status = EModuleStatus.Shutdown;
        if (this.onModuleShutdown != null)
        {
            this.onModuleShutdown(this);
        }
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
