using System.Collections.Generic;

namespace SDG.Unturned;

public interface IPluginAdvertising
{
    string PluginFrameworkName { get; set; }

    void AddPlugin(string name);

    void AddPlugins(IEnumerable<string> names);

    void RemovePlugin(string name);

    void RemovePlugins(IEnumerable<string> names);

    IEnumerable<string> GetPluginNames();
}
