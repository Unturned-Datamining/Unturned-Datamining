using SDG.Framework.Devkit;

namespace SDG.Unturned;

public abstract class VolumeBase : DevkitHierarchyWorldItem
{
    public virtual ISleekElement CreateMenu()
    {
        return null;
    }
}
