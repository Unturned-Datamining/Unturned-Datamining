using UnityEngine;

namespace SDG.Unturned;

public class ItemPlaceableAsset : ItemAsset
{
    public AssetReference<ItemAsset> salvageItemRef { get; protected set; }

    public int minItemsDroppedOnDestroy { get; protected set; }

    public int maxItemsDroppedOnDestroy { get; protected set; }

    public AssetReference<SpawnAsset> itemDroppedOnDestroy { get; protected set; }

    public ItemAsset FindSalvageItemAsset()
    {
        if (salvageItemRef.isValid)
        {
            return salvageItemRef.Find();
        }
        return FindDefaultSalvageItemAsset();
    }

    internal ItemAsset FindDefaultSalvageItemAsset()
    {
        foreach (Blueprint blueprint in base.blueprints)
        {
            if (blueprint.outputs.Length == 1 && blueprint.outputs[0].id == id)
            {
                BlueprintSupply blueprintSupply = blueprint.supplies[Random.Range(0, blueprint.supplies.Length)];
                return Assets.find(EAssetType.ITEM, blueprintSupply.id) as ItemAsset;
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
            ushort num = SpawnTableTool.resolve(spawnAsset.id);
            if (num > 0)
            {
                ItemManager.dropItem(new Item(num, EItemOrigin.NATURE), position + new Vector3(Random.Range(-2f, 2f), 2f, Random.Range(-2f, 2f)), playEffect: false, Dedicator.IsDedicatedServer, wideSpread: true);
            }
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        salvageItemRef = data.readAssetReference<ItemAsset>("SalvageItem");
        minItemsDroppedOnDestroy = data.ParseInt32("Min_Items_Dropped_On_Destroy");
        maxItemsDroppedOnDestroy = data.ParseInt32("Max_Items_Dropped_On_Destroy");
        itemDroppedOnDestroy = data.readAssetReference<SpawnAsset>("Item_Dropped_On_Destroy");
    }
}
