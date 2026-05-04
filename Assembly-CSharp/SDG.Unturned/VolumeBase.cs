using SDG.Framework.Devkit;

namespace SDG.Unturned;

public abstract class VolumeBase : DevkitHierarchyWorldItem
{
    internal bool inDynamicVolumesList;

    public virtual ISleekElement CreateMenu()
    {
        return null;
    }
}
