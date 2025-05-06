using System.Collections.Generic;

namespace SDG.Unturned;

public class CraftingAsset : Asset, IBlueprintOwner
{
    private List<Blueprint> _blueprints;

    public Asset GetBlueprintOwnerAsset()
    {
        return this;
    }

    public List<Blueprint> GetBlueprints()
    {
        return _blueprints;
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        if (p.data.TryGetList("Blueprints", out var node))
        {
            _blueprints = ItemAsset.PopulateBlueprintsV2(node, p.localization, this);
        }
        else
        {
            ReportAssetError("missing Blueprints list");
        }
        if (_blueprints == null)
        {
            _blueprints = new List<Blueprint>();
        }
        else if (_blueprints.Count > 255)
        {
            ReportAssetError($"has more than {byte.MaxValue} Blueprints which breaks some assumptions");
        }
    }
}
