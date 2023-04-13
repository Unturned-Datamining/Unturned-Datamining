using System;
using System.Collections.Generic;

namespace SDG.Unturned;

public class ItemCurrencyAsset : Asset
{
    public struct Entry
    {
        public AssetReference<ItemAsset> item;

        public uint value;

        public bool isVisibleInVendorMenu;
    }

    private static List<InventorySearch> search = new List<InventorySearch>();

    private static ItemCurrencyComparer valueComparer = new ItemCurrencyComparer();

    public string valueFormat { get; protected set; }

    public string defaultConditionFormat { get; protected set; }

    public Entry[] entries { get; protected set; }

    public uint getInventoryValue(Player player)
    {
        uint num = 0u;
        Entry[] array = entries;
        for (int i = 0; i < array.Length; i++)
        {
            Entry entry = array[i];
            AssetReference<ItemAsset> item = entry.item;
            ItemAsset itemAsset = item.Find();
            if (itemAsset == null)
            {
                continue;
            }
            search.Clear();
            player.inventory.search(search, itemAsset.id, findEmpty: false, findHealthy: true);
            foreach (InventorySearch item2 in search)
            {
                num += item2.jar.item.amount * entry.value;
            }
        }
        return num;
    }

    public bool canAfford(Player player, uint value)
    {
        return getInventoryValue(player) >= value;
    }

    public void grantValue(Player player, uint requiredValue)
    {
        if (requiredValue < 1)
        {
            return;
        }
        for (int num = entries.Length - 1; num >= 0; num--)
        {
            Entry entry = entries[num];
            ItemAsset itemAsset = entry.item.Find();
            if (itemAsset != null && requiredValue >= entry.value)
            {
                uint num2 = requiredValue / entry.value;
                ItemTool.tryForceGiveItem(player, itemAsset.id, (byte)num2);
                requiredValue -= num2 * entry.value;
                if (requiredValue == 0)
                {
                    break;
                }
            }
        }
    }

    public bool spendValue(Player player, uint requiredValue)
    {
        if (!canAfford(player, requiredValue))
        {
            return false;
        }
        uint num = 0u;
        Entry[] array = entries;
        for (int i = 0; i < array.Length; i++)
        {
            Entry entry = array[i];
            AssetReference<ItemAsset> item = entry.item;
            ItemAsset itemAsset = item.Find();
            if (itemAsset == null)
            {
                continue;
            }
            uint num2 = (requiredValue - num - 1) / entry.value + 1;
            List<InventorySearch> list = new List<InventorySearch>();
            player.inventory.search(list, itemAsset.id, findEmpty: false, findHealthy: true);
            foreach (InventorySearch item2 in list)
            {
                uint num3 = item2.deleteAmount(player, num2);
                num2 -= num3;
                num += num3 * entry.value;
                if (num2 == 0)
                {
                    break;
                }
            }
            if (num >= requiredValue)
            {
                break;
            }
        }
        if (num > requiredValue)
        {
            uint requiredValue2 = num - requiredValue;
            grantValue(player, requiredValue2);
        }
        return true;
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        valueFormat = data.GetString("ValueFormat");
        defaultConditionFormat = data.GetString("DefaultConditionFormat");
        if (string.IsNullOrEmpty(defaultConditionFormat) && !string.IsNullOrEmpty(valueFormat))
        {
            defaultConditionFormat = valueFormat + " / " + valueFormat.Replace("{0", "{1");
        }
        if (data.TryGetList("Entries", out var node))
        {
            int count = node.Count;
            entries = new Entry[count];
            for (int i = 0; i < count; i++)
            {
                Entry entry = default(Entry);
                if (node[i] is DatDictionary datDictionary)
                {
                    entry.item = datDictionary.ParseStruct<AssetReference<ItemAsset>>("Item");
                    entry.value = datDictionary.ParseUInt32("Value");
                    if (datDictionary.ContainsKey("Is_Visible_In_Vendor_Menu"))
                    {
                        entry.isVisibleInVendorMenu = datDictionary.ParseBool("Is_Visible_In_Vendor_Menu");
                    }
                    else
                    {
                        entry.isVisibleInVendorMenu = true;
                    }
                }
                entries[i] = entry;
            }
        }
        else
        {
            entries = new Entry[0];
        }
        Array.Sort(entries, valueComparer);
    }
}
