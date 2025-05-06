using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class MaterialPaletteAsset : Asset
{
    public List<ContentReference<Material>> materials;

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        if (p.data.TryGetList("Materials", out var node))
        {
            materials = node.ParseListOfStructs<ContentReference<Material>>();
        }
    }

    public MaterialPaletteAsset()
    {
        materials = new List<ContentReference<Material>>();
    }
}
