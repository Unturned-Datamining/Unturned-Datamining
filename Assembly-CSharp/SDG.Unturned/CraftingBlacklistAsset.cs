using System.Collections.Generic;

namespace SDG.Unturned;

public class CraftingBlacklistAsset : Asset
{
    protected struct BlacklistedBlueprint : IDatParseable
    {
        public AssetReference<ItemAsset> assetRef;

        public int index;

        public bool TryParse(IDatNode node)
        {
            if (!(node is DatDictionary datDictionary))
            {
                return false;
            }
            assetRef = datDictionary.ParseStruct<AssetReference<ItemAsset>>("Item");
            index = datDictionary.ParseInt32("Blueprint");
            if (assetRef.isValid)
            {
                return index >= 0;
            }
            return false;
        }

        public BlacklistedBlueprint(AssetReference<ItemAsset> assetRef, int index)
        {
            this.assetRef = assetRef;
            this.index = index;
        }
    }

    protected List<AssetReference<ItemAsset>> inputItems = new List<AssetReference<ItemAsset>>();

    protected List<AssetReference<ItemAsset>> outputItems = new List<AssetReference<ItemAsset>>();

    protected List<ushort> resolvedInputItems;

    protected List<ushort> resolvedOutputItems;

    protected List<BlacklistedBlueprint> blueprints = new List<BlacklistedBlueprint>();

    public bool isBlueprintBlacklisted(Blueprint blueprint)
    {
        foreach (BlacklistedBlueprint blueprint2 in blueprints)
        {
            if (blueprint2.index == blueprint.id)
            {
                AssetReference<ItemAsset> assetRef = blueprint2.assetRef;
                if (assetRef.isReferenceTo(blueprint.sourceItem))
                {
                    return true;
                }
            }
        }
        if (resolvedInputItems == null && inputItems != null)
        {
            resolvedInputItems = new List<ushort>(inputItems.Count);
            foreach (AssetReference<ItemAsset> inputItem in inputItems)
            {
                ItemAsset itemAsset = inputItem.Find();
                if (itemAsset != null)
                {
                    resolvedInputItems.Add(itemAsset.id);
                }
            }
        }
        if (resolvedInputItems != null)
        {
            BlueprintSupply[] supplies = blueprint.supplies;
            foreach (BlueprintSupply blueprintSupply in supplies)
            {
                if (resolvedInputItems.Contains(blueprintSupply.id))
                {
                    return true;
                }
            }
        }
        if (resolvedOutputItems == null && outputItems != null)
        {
            resolvedOutputItems = new List<ushort>(outputItems.Count);
            foreach (AssetReference<ItemAsset> outputItem in outputItems)
            {
                ItemAsset itemAsset2 = outputItem.Find();
                if (itemAsset2 != null)
                {
                    resolvedOutputItems.Add(itemAsset2.id);
                }
            }
        }
        if (resolvedOutputItems != null)
        {
            BlueprintOutput[] outputs = blueprint.outputs;
            foreach (BlueprintOutput blueprintOutput in outputs)
            {
                if (resolvedOutputItems.Contains(blueprintOutput.id))
                {
                    return true;
                }
            }
        }
        return false;
    }

    protected void readList(DatDictionary reader, List<AssetReference<ItemAsset>> list, string key)
    {
        if (!reader.TryGetList(key, out var node))
        {
            return;
        }
        foreach (IDatNode item in node)
        {
            if (item.TryParseStruct<AssetReference<ItemAsset>>(out var value) && value.isValid)
            {
                list.Add(value);
            }
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        readList(data, inputItems, "Input_Items");
        readList(data, outputItems, "Output_Items");
        if (data.TryGetList("Blueprints", out var node))
        {
            blueprints = node.ParseListOfStructs<BlacklistedBlueprint>();
        }
    }
}
