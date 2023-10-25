using System.Collections.Generic;

namespace SDG.Framework.Modules;

/// <summary>
/// Sorts modules by dependencies.
/// </summary>
public class ModuleComparer : IComparer<ModuleConfig>
{
    public int Compare(ModuleConfig x, ModuleConfig y)
    {
        for (int i = 0; i < y.Dependencies.Count; i++)
        {
            if (y.Dependencies[i].Name == x.Name)
            {
                return -1;
            }
        }
        for (int j = 0; j < x.Dependencies.Count; j++)
        {
            if (x.Dependencies[j].Name == y.Name)
            {
                return 1;
            }
        }
        return x.Dependencies.Count - y.Dependencies.Count;
    }
}
