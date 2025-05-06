using System.Collections.Generic;

namespace SDG.Unturned;

public interface IBlueprintOwner
{
    Asset GetBlueprintOwnerAsset();

    List<Blueprint> GetBlueprints();
}
