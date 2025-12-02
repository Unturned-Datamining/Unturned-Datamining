using System;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Common base for barricades and structures.
/// 2023-01-16: not ideal to be adding this so late in development, but at least it is a step in the right direction.
/// </summary>
public class ItemPlaceableAsset : ItemAsset, IArmorFalloff
{
    private CachingAssetRef _salvageItemRef;

    private CachingAssetRef _itemDroppedOnDestroyRef;

    public float ArmorFalloffMaxRange { get; set; }

    public float ArmorFalloffRange { get; set; }

    public float ArmorFalloffMultiplier { get; set; }

    /// <summary>
    /// If true, this item is eligible for zombies to detect and attack when stuck.
    /// Defaults to true.
    /// </summary>
    public bool CanZombiesTarget { get; protected set; }

    /// <summary>
    /// Item or spawn table recovered when picked up below 100% health.
    /// </summary>
    public CachingAssetRef SalvageItemRef
    {
        get
        {
            return _salvageItemRef;
        }
        set
        {
            _salvageItemRef = value;
        }
    }

    /// <summary>
    /// Minimum number of items to recover when salvaged.
    /// </summary>
    public int MinItemsRecoveredOnSalvage { get; protected set; }

    /// <summary>
    /// Maximum number of items to recover when salvaged.
    /// </summary>
    public int MaxItemsRecoveredOnSalvage { get; set; }

    /// <summary>
    /// Minimum number of items to drop when destroyed.
    /// </summary>
    public int minItemsDroppedOnDestroy { get; protected set; }

    /// <summary>
    /// Maximum number of items to drop when destroyed.
    /// </summary>
    public int maxItemsDroppedOnDestroy { get; protected set; }

    /// <summary>
    /// Item or spawn table dropped when destroyed.
    /// </summary>
    public CachingAssetRef ItemDroppedOnDestroyRef
    {
        get
        {
            return _itemDroppedOnDestroyRef;
        }
        set
        {
            _itemDroppedOnDestroyRef = value;
        }
    }

    /// <summary>
    /// If non-null, this asset provides the listed crafting tags to nearby players.
    /// </summary>
    public CachingAssetRef[] PlaceableProvidedCraftingTags { get; protected set; }

    public EPlaceableExplosionEffectFlags ExplosionEffectFlags { get; set; }

    [Obsolete("Replaced by SalvageItemRef which supports spawn tables as well")]
    public AssetReference<ItemAsset> salvageItemRef => new AssetReference<ItemAsset>(SalvageItemRef.Guid);

    [Obsolete("Replaced by ItemDroppedOnDestroyRef which supports items as well")]
    public AssetReference<SpawnAsset> ItemDroppedOnDestroy => new AssetReference<SpawnAsset>(_itemDroppedOnDestroyRef.Guid);

    /// <summary>
    /// Note: this assumes SalvageItemRef points to an ItemAsset.
    /// </summary>
    public ItemAsset FindSalvageItemAsset()
    {
        if (SalvageItemRef.IsAssigned)
        {
            return SalvageItemRef.Get<ItemAsset>();
        }
        return FindDefaultSalvageItemAsset();
    }

    /// <summary>
    /// By default a crafting ingredient is salvaged.
    /// </summary>
    public ItemAsset FindDefaultSalvageItemAsset()
    {
        foreach (Blueprint blueprint in base.blueprints)
        {
            if (blueprint.outputs.Length == 1 && blueprint.outputs[0].IsItem(this))
            {
                return blueprint.supplies[UnityEngine.Random.Range(0, blueprint.supplies.Length)].FindItemAsset();
            }
        }
        return null;
    }

    public void GrantSalvageItems(Player player)
    {
        int value = UnityEngine.Random.Range(MinItemsRecoveredOnSalvage, MaxItemsRecoveredOnSalvage + 1);
        value = Mathf.Clamp(value, 0, 100);
        if (value < 1)
        {
            return;
        }
        Asset asset = _salvageItemRef.Get();
        if (asset is SpawnAsset spawnAsset)
        {
            for (int i = 0; i < value; i++)
            {
                ItemAsset itemAsset = SpawnTableTool.Resolve<ItemAsset>(spawnAsset, EAssetType.ITEM, OnGetItemRecoveredOnSalvageSpawnTableErrorContext);
                if (itemAsset != null)
                {
                    player.inventory.forceAddItem(new Item(itemAsset, EItemOrigin.NATURE), auto: true);
                }
            }
            return;
        }
        ItemAsset itemAsset2 = asset as ItemAsset;
        if (itemAsset2 == null)
        {
            itemAsset2 = FindDefaultSalvageItemAsset();
            if (itemAsset2 == null)
            {
                return;
            }
        }
        for (int j = 0; j < value; j++)
        {
            player.inventory.forceAddItem(new Item(itemAsset2, EItemOrigin.NATURE), auto: true);
        }
    }

    public bool DoesAnyPlaceableProvidedCraftingTagNameContainText(string text)
    {
        if (PlaceableProvidedCraftingTags != null && PlaceableProvidedCraftingTags.Length != 0)
        {
            for (int i = 0; i < PlaceableProvidedCraftingTags.Length; i++)
            {
                TagAsset tagAsset = PlaceableProvidedCraftingTags[i].Get<TagAsset>();
                if (tagAsset != null && !string.IsNullOrEmpty(tagAsset.PlainTextName) && tagAsset.PlainTextName.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    internal void SpawnItemDropsOnDestroy(Vector3 position)
    {
        int value = UnityEngine.Random.Range(minItemsDroppedOnDestroy, maxItemsDroppedOnDestroy + 1);
        value = Mathf.Clamp(value, 0, 100);
        if (value < 1)
        {
            return;
        }
        Asset asset = _itemDroppedOnDestroyRef.Get();
        if (asset is SpawnAsset spawnAsset)
        {
            for (int i = 0; i < value; i++)
            {
                ItemAsset itemAsset = SpawnTableTool.Resolve<ItemAsset>(spawnAsset, EAssetType.ITEM, OnGetItemDroppedOnDestroySpawnTableErrorContext);
                if (itemAsset != null)
                {
                    ItemManager.dropItem(new Item(itemAsset, EItemOrigin.NATURE), position + new Vector3(UnityEngine.Random.Range(-2f, 2f), 2f, UnityEngine.Random.Range(-2f, 2f)), playEffect: false, Dedicator.IsDedicatedServer, wideSpread: true);
                }
            }
        }
        else if (asset is ItemAsset asset2)
        {
            for (int j = 0; j < value; j++)
            {
                ItemManager.dropItem(new Item(asset2, EItemOrigin.NATURE), position + new Vector3(UnityEngine.Random.Range(-2f, 2f), 2f, UnityEngine.Random.Range(-2f, 2f)), playEffect: false, Dedicator.IsDedicatedServer, wideSpread: true);
            }
        }
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized) || PlaceableProvidedCraftingTags == null || PlaceableProvidedCraftingTags.Length == 0)
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
        if (p.data.TryParseInt32("Items_Recovered_On_Salvage", out var value))
        {
            MinItemsRecoveredOnSalvage = value;
            MaxItemsRecoveredOnSalvage = value;
        }
        else
        {
            MinItemsRecoveredOnSalvage = p.data.ParseInt32("Min_Items_Recovered_On_Salvage", 1);
            MaxItemsRecoveredOnSalvage = p.data.ParseInt32("Max_Items_Recovered_On_Salvage", 1);
        }
        if (!p.data.TryParseAssetRef("SalvageItem", out _salvageItemRef) && string.Equals(p.data.GetString("SalvageItem"), "this", StringComparison.InvariantCultureIgnoreCase))
        {
            _salvageItemRef = this;
        }
        if (p.data.TryParseInt32("Items_Dropped_On_Destroy", out var value2))
        {
            minItemsDroppedOnDestroy = value2;
            maxItemsDroppedOnDestroy = value2;
        }
        else
        {
            minItemsDroppedOnDestroy = p.data.ParseInt32("Min_Items_Dropped_On_Destroy");
            maxItemsDroppedOnDestroy = p.data.ParseInt32("Max_Items_Dropped_On_Destroy");
        }
        if (!p.data.TryParseAssetRef("Item_Dropped_On_Destroy", out _itemDroppedOnDestroyRef) && string.Equals(p.data.GetString("Item_Dropped_On_Destroy"), "this", StringComparison.InvariantCultureIgnoreCase))
        {
            _itemDroppedOnDestroyRef = this;
        }
        PlaceableProvidedCraftingTags = p.data.ParseArrayOfStructs<CachingAssetRef>("PlaceableProvidesCraftingTags");
        if (p.data.ParseBool("ExplosionEffect_CopyModelPosition"))
        {
            ExplosionEffectFlags |= EPlaceableExplosionEffectFlags.CopyModelPosition;
        }
        if (p.data.ParseBool("ExplosionEffect_CopyModelRotation"))
        {
            ExplosionEffectFlags |= EPlaceableExplosionEffectFlags.CopyModelRotation;
        }
        this.PopulateArmorFalloff(in p);
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Placeable");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Can_Zombies_Target", CanZombiesTarget);
        orAddDeclaration.Append("SalvageItem", SalvageItemRef);
        orAddDeclaration.Append("Min_Items_Dropped_On_Destroy", minItemsDroppedOnDestroy);
        orAddDeclaration.Append("Max_Items_Dropped_On_Destroy", maxItemsDroppedOnDestroy);
        orAddDeclaration.Append("Item_Dropped_On_Destroy", ItemDroppedOnDestroyRef);
    }

    private string OnGetItemDroppedOnDestroySpawnTableErrorContext()
    {
        return FriendlyName + " items dropped on destroy";
    }

    private string OnGetItemRecoveredOnSalvageSpawnTableErrorContext()
    {
        return FriendlyName + " items recovered on salvage";
    }
}
