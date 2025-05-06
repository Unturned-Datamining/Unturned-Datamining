using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Common base for barricades and structures.
/// 2023-01-16: not ideal to be adding this so late in development, but at least it is a step in the right direction.
/// </summary>
public class ItemPlaceableAsset : ItemAsset
{
    /// <summary>
    /// If true, this item is eligible for zombies to detect and attack when stuck.
    /// Defaults to true.
    /// </summary>
    public bool CanZombiesTarget { get; protected set; }

    /// <summary>
    /// Item recovered when picked up below 100% health.
    /// </summary>
    public AssetReference<ItemAsset> salvageItemRef { get; protected set; }

    /// <summary>
    /// Minimum number of items to drop when destroyed.
    /// </summary>
    public int minItemsDroppedOnDestroy { get; protected set; }

    /// <summary>
    /// Maximum number of items to drop when destroyed.
    /// </summary>
    public int maxItemsDroppedOnDestroy { get; protected set; }

    /// <summary>
    /// Spawn table for items dropped when destroyed.
    /// </summary>
    public AssetReference<SpawnAsset> itemDroppedOnDestroy { get; protected set; }

    /// <summary>
    /// If non-null, this asset provides the listed crafting tags to nearby players.
    /// </summary>
    public CachingAssetRef[] PlaceableProvidedCraftingTags { get; protected set; }

    public ItemAsset FindSalvageItemAsset()
    {
        if (salvageItemRef.isValid)
        {
            return salvageItemRef.Find();
        }
        return FindDefaultSalvageItemAsset();
    }

    /// <summary>
    /// By default a crafting ingredient is salvaged.
    /// </summary>
    internal ItemAsset FindDefaultSalvageItemAsset()
    {
        foreach (Blueprint blueprint in base.blueprints)
        {
            if (blueprint.outputs.Length == 1 && blueprint.outputs[0].IsItem(this))
            {
                return blueprint.supplies[Random.Range(0, blueprint.supplies.Length)].FindItemAsset();
            }
        }
        return null;
    }

    internal void SpawnItemDropsOnDestroy(Vector3 position)
    {
        int value = Random.Range(minItemsDroppedOnDestroy, maxItemsDroppedOnDestroy + 1);
        value = Mathf.Clamp(value, 0, 100);
        if (value < 1)
        {
            return;
        }
        SpawnAsset spawnAsset = itemDroppedOnDestroy.Find();
        if (spawnAsset == null)
        {
            return;
        }
        for (int i = 0; i < value; i++)
        {
            ushort num = SpawnTableTool.ResolveLegacyId(spawnAsset, EAssetType.ITEM, OnGetItemDroppedOnDestroySpawnTableErrorContext);
            if (num > 0)
            {
                ItemManager.dropItem(new Item(num, EItemOrigin.NATURE), position + new Vector3(Random.Range(-2f, 2f), 2f, Random.Range(-2f, 2f)), playEffect: false, Dedicator.IsDedicatedServer, wideSpread: true);
            }
        }
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (builder.shouldRestrictToLegacyContent || PlaceableProvidedCraftingTags == null || PlaceableProvidedCraftingTags.Length == 0)
        {
            return;
        }
        Local localization = PlayerDashboardInventoryUI.localization;
        int num = 25000;
        builder.Append(localization.format("ItemDescription_ProvidesCraftingTags"), ++num);
        for (int i = 0; i < PlaceableProvidedCraftingTags.Length; i++)
        {
            TagAsset tagAsset = PlaceableProvidedCraftingTags[i].Get<TagAsset>();
            if (tagAsset != null)
            {
                builder.Append(localization.format("ItemDescription_ListItem", tagAsset.RichTextName), ++num);
            }
        }
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        CanZombiesTarget = p.data.ParseBool("Can_Zombies_Target", defaultValue: true);
        salvageItemRef = p.data.readAssetReference<ItemAsset>("SalvageItem");
        minItemsDroppedOnDestroy = p.data.ParseInt32("Min_Items_Dropped_On_Destroy");
        maxItemsDroppedOnDestroy = p.data.ParseInt32("Max_Items_Dropped_On_Destroy");
        itemDroppedOnDestroy = p.data.readAssetReference<SpawnAsset>("Item_Dropped_On_Destroy");
        PlaceableProvidedCraftingTags = p.data.ParseArrayOfStructs<CachingAssetRef>("PlaceableProvidesCraftingTags");
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Placeable");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Can_Zombies_Target", CanZombiesTarget);
        orAddDeclaration.Append("SalvageItem", salvageItemRef);
        orAddDeclaration.Append("Min_Items_Dropped_On_Destroy", minItemsDroppedOnDestroy);
        orAddDeclaration.Append("Max_Items_Dropped_On_Destroy", maxItemsDroppedOnDestroy);
        orAddDeclaration.Append("Item_Dropped_On_Destroy", itemDroppedOnDestroy);
    }

    private string OnGetItemDroppedOnDestroySpawnTableErrorContext()
    {
        return FriendlyName + " items dropped on destroy";
    }
}
