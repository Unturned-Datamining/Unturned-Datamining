using System;
using System.Collections.Generic;
using Unturned.SystemEx;

namespace SDG.Unturned;

/// <summary>
/// Restricts which items can be crafted.
/// </summary>
public class CraftingBlacklistAsset : Asset
{
    protected struct BlacklistedBlueprint
    {
        public int index;

        /// <summary>
        /// If null, use index instead.
        /// </summary>
        public string blueprintName;
    }

    /// <summary>
    /// Restrict blueprints that consume these items.
    /// </summary>
    protected HashSet<Guid> inputItems = new HashSet<Guid>();

    /// <summary>
    /// Restrict blueprints that generate these items.
    /// </summary>
    protected HashSet<Guid> outputItems = new HashSet<Guid>();

    /// <summary>
    /// If false, blueprints on vanilla/core/built-in items are not allowed. Defaults to true.
    /// </summary>
    protected bool allowCoreBlueprints = true;

    /// <summary>
    /// Restrict specific blueprints.
    /// </summary>
    protected Dictionary<Guid, List<BlacklistedBlueprint>> blueprints = new Dictionary<Guid, List<BlacklistedBlueprint>>();

    public bool isBlueprintBlacklisted(Blueprint blueprint)
    {
        Asset ownerAsset = blueprint.GetOwnerAsset();
        if (!allowCoreBlueprints && ownerAsset.origin == Assets.coreOrigin)
        {
            return true;
        }
        if (blueprints.TryGetValue(ownerAsset.GUID, out var value))
        {
            foreach (BlacklistedBlueprint item in value)
            {
                if (!string.IsNullOrEmpty(item.blueprintName))
                {
                    if (string.Equals(item.blueprintName, blueprint.Name, StringComparison.InvariantCulture))
                    {
                        return true;
                    }
                }
                else if (item.index == blueprint.Index)
                {
                    return true;
                }
            }
        }
        if (inputItems != null)
        {
            BlueprintSupply[] supplies = blueprint.supplies;
            for (int i = 0; i < supplies.Length; i++)
            {
                ItemAsset itemAsset = supplies[i].FindItemAsset();
                if (itemAsset != null && inputItems.Contains(itemAsset.GUID))
                {
                    return true;
                }
            }
        }
        if (outputItems != null)
        {
            BlueprintOutput[] outputs = blueprint.outputs;
            for (int i = 0; i < outputs.Length; i++)
            {
                ItemAsset itemAsset2 = outputs[i].FindItemAsset();
                if (itemAsset2 != null && outputItems.Contains(itemAsset2.GUID))
                {
                    return true;
                }
            }
        }
        return false;
    }

    protected void readList(IDatDictionary reader, HashSet<Guid> list, string key)
    {
        if (!reader.TryGetList(key, out var node))
        {
            return;
        }
        foreach (IDatNode item in node)
        {
            if (item.TryParseStruct<AssetReference<ItemAsset>>(out var value) && value.isValid)
            {
                list.Add(value.GUID);
            }
        }
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        readList(p.data, inputItems, "Input_Items");
        readList(p.data, outputItems, "Output_Items");
        if (p.data.TryGetList("Blueprints", out var node))
        {
            foreach (IDatNode item in node)
            {
                if (!(item is IDatDictionary dictionary))
                {
                    continue;
                }
                AssetReference<Asset> assetReference = dictionary.ParseStruct<AssetReference<Asset>>("Item");
                if (assetReference.isValid)
                {
                    int num = dictionary.ParseInt32("Blueprint");
                    string @string = dictionary.GetString("BlueprintName");
                    if (num >= 0 || !string.IsNullOrEmpty(@string))
                    {
                        blueprints.GetOrAddNew(assetReference.GUID).Add(new BlacklistedBlueprint
                        {
                            index = num,
                            blueprintName = @string
                        });
                    }
                }
            }
        }
        allowCoreBlueprints = p.data.ParseBool("Allow_Core_Blueprints", defaultValue: true);
    }
}
