using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class MaterialPaletteAsset : Asset
{
    public List<ContentReference<Material>> materials;

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        if (data.TryGetList("Materials", out var node))
        {
            materials = node.ParseListOfStructs<ContentReference<Material>>();
        }
    }

    public MaterialPaletteAsset()
    {
        materials = new List<ContentReference<Material>>();
    }
}
