using System.Collections.Generic;
using SDG.Framework.IO.FormattedFiles;

namespace SDG.Unturned;

public class CraftingBlacklistAsset : Asset
{
    protected struct BlacklistedBlueprint
    {
        public AssetReference<ItemAsset> assetRef;

        public int index;

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

    protected void readList(IFormattedFileReader reader, List<AssetReference<ItemAsset>> list, string key)
    {
        int num = reader.readArrayLength(key);
        for (int i = 0; i < num; i++)
        {
            AssetReference<ItemAsset> item = reader.readValue<AssetReference<ItemAsset>>(i);
            if (item.isValid)
            {
                list.Add(item);
            }
        }
    }

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        readList(reader, inputItems, "Input_Items");
        readList(reader, outputItems, "Output_Items");
        int num = reader.readArrayLength("Blueprints");
        for (int i = 0; i < num; i++)
        {
            IFormattedFileReader formattedFileReader = reader.readObject(i);
            AssetReference<ItemAsset> assetRef = formattedFileReader.readValue<AssetReference<ItemAsset>>("Item");
            int num2 = formattedFileReader.readValue<int>("Blueprint");
            if (assetRef.isValid && num2 >= 0)
            {
                blueprints.Add(new BlacklistedBlueprint(assetRef, num2));
            }
        }
    }

    public CraftingBlacklistAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
    }
}
