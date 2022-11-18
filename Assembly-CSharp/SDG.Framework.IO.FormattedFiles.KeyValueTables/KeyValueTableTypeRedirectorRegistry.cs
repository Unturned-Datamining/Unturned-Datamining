using System.Collections.Generic;
using SDG.Framework.Devkit;

namespace SDG.Framework.IO.FormattedFiles.KeyValueTables;

public static class KeyValueTableTypeRedirectorRegistry
{
    private static Dictionary<string, string> redirects;

    public static string chase(string assemblyQualifiedName)
    {
        if (redirects.TryGetValue(assemblyQualifiedName, out var value))
        {
            return chase(value);
        }
        return assemblyQualifiedName;
    }

    public static void add(string oldAssemblyQualifiedName, string newAssemblyQualifiedName)
    {
        redirects.Add(oldAssemblyQualifiedName, newAssemblyQualifiedName);
    }

    public static void remove(string oldAssemblyQualifiedName)
    {
        redirects.Remove(oldAssemblyQualifiedName);
    }

    static KeyValueTableTypeRedirectorRegistry()
    {
        redirects = new Dictionary<string, string>();
        add("SDG.Framework.Landscapes.PlayerClipVolume, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", typeof(PlayerClipVolume).AssemblyQualifiedName);
        add("SDG.Framework.Foliage.KillVolume, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", typeof(KillVolume).AssemblyQualifiedName);
    }
}
