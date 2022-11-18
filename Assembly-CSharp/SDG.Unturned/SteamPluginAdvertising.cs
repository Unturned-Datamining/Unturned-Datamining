using System.Collections.Generic;
using Steamworks;

namespace SDG.Unturned;

internal class SteamPluginAdvertising : IPluginAdvertising
{
    private bool isGameServerReady;

    private HashSet<string> pluginNames = new HashSet<string>();

    private string pluginFrameworkName = string.Empty;

    private static SteamPluginAdvertising instance;

    public string PluginFrameworkName
    {
        get
        {
            return pluginFrameworkName;
        }
        set
        {
            if (isGameServerReady)
            {
                UnturnedLog.warn("Cannot change advertised plugin framework after server startup");
                return;
            }
            pluginFrameworkName = value;
            if (string.IsNullOrEmpty(pluginFrameworkName))
            {
                PluginFrameworkTag = null;
                return;
            }
            if (pluginFrameworkName.Equals("rocket"))
            {
                PluginFrameworkTag = "rm";
                return;
            }
            if (pluginFrameworkName.Equals("openmod"))
            {
                PluginFrameworkTag = "om";
                return;
            }
            PluginFrameworkTag = null;
            UnturnedLog.warn("Cannot advertise unknown plugin framework name \"{0}\"", pluginFrameworkName);
        }
    }

    public string PluginFrameworkTag { get; private set; }

    public static SteamPluginAdvertising Get()
    {
        if (instance == null)
        {
            instance = new SteamPluginAdvertising();
        }
        return instance;
    }

    public void AddPlugin(string name)
    {
        if (pluginNames.Add(name))
        {
            UpdateKeyValue();
        }
    }

    public void AddPlugins(IEnumerable<string> names)
    {
        int count = pluginNames.Count;
        pluginNames.UnionWith(names);
        if (pluginNames.Count > count)
        {
            UpdateKeyValue();
        }
    }

    public void RemovePlugin(string name)
    {
        if (pluginNames.Remove(name))
        {
            UpdateKeyValue();
        }
    }

    public void RemovePlugins(IEnumerable<string> names)
    {
        int count = pluginNames.Count;
        pluginNames.ExceptWith(names);
        if (pluginNames.Count < count)
        {
            UpdateKeyValue();
        }
    }

    public IEnumerable<string> GetPluginNames()
    {
        return pluginNames;
    }

    public void NotifyGameServerReady()
    {
        isGameServerReady = true;
        UpdateKeyValue();
    }

    public void UpdateKeyValue()
    {
        if (isGameServerReady)
        {
            SteamGameServer.SetKeyValue("rocketplugins", string.Join(",", pluginNames));
        }
    }
}
