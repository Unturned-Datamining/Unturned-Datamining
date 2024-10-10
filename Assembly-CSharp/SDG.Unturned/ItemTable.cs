using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class ItemTable
{
    private List<ItemTier> _tiers;

    private Color _color;

    public string name;

    public ushort tableID;

    public List<ItemTier> tiers => _tiers;

    public Color color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
            for (byte b = 0; b < Regions.WORLD_SIZE; b++)
            {
                for (byte b2 = 0; b2 < Regions.WORLD_SIZE; b2++)
                {
                    for (ushort num = 0; num < LevelItems.spawns[b, b2].Count; num++)
                    {
                        ItemSpawnpoint itemSpawnpoint = LevelItems.spawns[b, b2][num];
                        if (itemSpawnpoint.type == EditorSpawns.selectedItem)
                        {
                            itemSpawnpoint.node.GetComponent<Renderer>().material.color = color;
                        }
                    }
                    EditorSpawns.itemSpawn.GetComponent<Renderer>().material.color = color;
                }
            }
        }
    }

    public void addTier(string name)
    {
        if (tiers.Count == 255)
        {
            return;
        }
        for (int i = 0; i < tiers.Count; i++)
        {
            if (tiers[i].name == name)
            {
                return;
            }
        }
        if (tiers.Count == 0)
        {
            tiers.Add(new ItemTier(new List<ItemSpawn>(), name, 1f));
        }
        else
        {
            tiers.Add(new ItemTier(new List<ItemSpawn>(), name, 0f));
        }
    }

    public void removeTier(int tierIndex)
    {
        updateChance(tierIndex, 0f);
        tiers.RemoveAt(tierIndex);
    }

    public void addItem(byte tierIndex, ushort id)
    {
        tiers[tierIndex].addItem(id);
    }

    public void removeItem(byte tierIndex, byte itemIndex)
    {
        tiers[tierIndex].removeItem(itemIndex);
    }

    public ushort getItem()
    {
        if (tableID != 0)
        {
            return SpawnTableTool.ResolveLegacyId(tableID, EAssetType.ITEM, OnGetSpawnTableErrorContext);
        }
        float value = Random.value;
        if (tiers.Count == 0)
        {
            return 0;
        }
        for (int i = 0; i < tiers.Count; i++)
        {
            if (value < tiers[i].chance)
            {
                ItemTier itemTier = tiers[i];
                if (itemTier.table.Count > 0)
                {
                    return itemTier.table[Random.Range(0, itemTier.table.Count)].item;
                }
                return 0;
            }
        }
        ItemTier itemTier2 = tiers[Random.Range(0, tiers.Count)];
        if (itemTier2.table.Count > 0)
        {
            return itemTier2.table[Random.Range(0, itemTier2.table.Count)].item;
        }
        return 0;
    }

    public void buildTable()
    {
        List<ItemTier> list = new List<ItemTier>();
        for (int i = 0; i < tiers.Count; i++)
        {
            if (list.Count == 0)
            {
                list.Add(tiers[i]);
                continue;
            }
            bool flag = false;
            for (int j = 0; j < list.Count; j++)
            {
                if (tiers[i].chance < list[j].chance)
                {
                    flag = true;
                    list.Insert(j, tiers[i]);
                    break;
                }
            }
            if (!flag)
            {
                list.Add(tiers[i]);
            }
        }
        float num = 0f;
        for (int k = 0; k < list.Count; k++)
        {
            num += list[k].chance;
            list[k].chance = num;
        }
        _tiers = list;
    }

    public void updateChance(int tierIndex, float chance)
    {
        float num = chance - tiers[tierIndex].chance;
        tiers[tierIndex].chance = chance;
        if (tiers.Count < 2)
        {
            return;
        }
        float num2 = Mathf.Abs(num);
        while (num2 > 0.001f)
        {
            int num3 = 0;
            for (int i = 0; i < tiers.Count; i++)
            {
                if (((num < 0f && tiers[i].chance < 1f) || (num > 0f && tiers[i].chance > 0f)) && i != tierIndex)
                {
                    num3++;
                }
            }
            if (num3 == 0)
            {
                break;
            }
            float num4 = num2 / (float)num3;
            for (int j = 0; j < tiers.Count; j++)
            {
                if (((!(num < 0f) || !(tiers[j].chance < 1f)) && (!(num > 0f) || !(tiers[j].chance > 0f))) || j == tierIndex)
                {
                    continue;
                }
                if (num > 0f)
                {
                    if (tiers[j].chance >= num4)
                    {
                        num2 -= num4;
                        tiers[j].chance -= num4;
                    }
                    else
                    {
                        num2 -= tiers[j].chance;
                        tiers[j].chance = 0f;
                    }
                }
                else if (tiers[j].chance <= 1f - num4)
                {
                    num2 -= num4;
                    tiers[j].chance += num4;
                }
                else
                {
                    num2 -= 1f - tiers[j].chance;
                    tiers[j].chance = 1f;
                }
            }
        }
        float num5 = 0f;
        for (int k = 0; k < tiers.Count; k++)
        {
            num5 += tiers[k].chance;
        }
        for (int l = 0; l < tiers.Count; l++)
        {
            tiers[l].chance /= num5;
        }
    }

    public ItemTable(string newName)
    {
        _tiers = new List<ItemTier>();
        _color = Color.white;
        name = newName;
        tableID = 0;
    }

    public ItemTable(List<ItemTier> newTiers, Color newColor, string newName, ushort newTableID)
    {
        _tiers = newTiers;
        _color = newColor;
        name = newName;
        tableID = newTableID;
    }

    private string OnGetSpawnTableErrorContext()
    {
        return "\"" + Level.info.name + "\" item table \"" + name + "\"";
    }

    internal string OnGetSpawnTableValidationErrorContext()
    {
        return "\"" + Level.info.name + " item table \"" + name + "\" validation";
    }
}
