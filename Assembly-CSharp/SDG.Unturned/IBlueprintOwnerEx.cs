using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public static class IBlueprintOwnerEx
{
    public static Blueprint FindBlueprintByName(this IBlueprintOwner blueprintOwner, string name)
    {
        List<Blueprint> blueprints = blueprintOwner.GetBlueprints();
        if (blueprints == null || string.IsNullOrEmpty(name))
        {
            return null;
        }
        foreach (Blueprint item in blueprints)
        {
            if (string.Equals(item.Name, name, StringComparison.InvariantCulture))
            {
                return item;
            }
        }
        return null;
    }

    public static Blueprint GetBlueprintByIndex(this IBlueprintOwner blueprintOwner, int index)
    {
        List<Blueprint> blueprints = blueprintOwner.GetBlueprints();
        if (blueprints != null && index >= 0 && index < blueprints.Count)
        {
            return blueprints[index];
        }
        return null;
    }
}
