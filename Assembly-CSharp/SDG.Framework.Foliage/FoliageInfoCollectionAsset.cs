using System.Collections.Generic;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage;

public class FoliageInfoCollectionAsset : Asset
{
    public struct FoliageInfoCollectionElement : IDatParseable
    {
        public AssetReference<FoliageInfoAsset> asset;

        public float weight;

        public bool TryParse(IDatNode node)
        {
            if (node is DatDictionary datDictionary)
            {
                asset = datDictionary.ParseStruct<AssetReference<FoliageInfoAsset>>("Asset");
                weight = datDictionary.ParseFloat("Weight", 1f);
                return true;
            }
            return false;
        }
    }

    public List<FoliageInfoCollectionElement> elements;

    public virtual void bakeFoliage(FoliageBakeSettings bakeSettings, IFoliageSurface surface, Bounds bounds, float weight)
    {
        foreach (FoliageInfoCollectionElement element in elements)
        {
            Assets.find(element.asset)?.bakeFoliage(bakeSettings, surface, bounds, weight, element.weight);
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        if (data.TryGetList("Foliage", out var node))
        {
            elements = node.ParseListOfStructs<FoliageInfoCollectionElement>();
        }
    }

    public FoliageInfoCollectionAsset()
    {
        elements = new List<FoliageInfoCollectionElement>();
    }
}
