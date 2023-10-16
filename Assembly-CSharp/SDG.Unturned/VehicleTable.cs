using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class VehicleTable
{
    private List<VehicleTier> _tiers;

    private Color _color;

    public string name;

    public ushort tableID;

    public List<VehicleTier> tiers => _tiers;

    public Color color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
            for (ushort num = 0; num < LevelVehicles.spawns.Count; num++)
            {
                VehicleSpawnpoint vehicleSpawnpoint = LevelVehicles.spawns[num];
                if (vehicleSpawnpoint.type == EditorSpawns.selectedVehicle)
                {
                    vehicleSpawnpoint.node.GetComponent<Renderer>().material.color = color;
                    vehicleSpawnpoint.node.Find("Arrow").GetComponent<Renderer>().material.color = color;
                }
            }
            EditorSpawns.vehicleSpawn.GetComponent<Renderer>().material.color = color;
            EditorSpawns.vehicleSpawn.Find("Arrow").GetComponent<Renderer>().material.color = color;
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
            tiers.Add(new VehicleTier(new List<VehicleSpawn>(), name, 1f));
        }
        else
        {
            tiers.Add(new VehicleTier(new List<VehicleSpawn>(), name, 0f));
        }
    }

    public void removeTier(int tierIndex)
    {
        updateChance(tierIndex, 0f);
        tiers.RemoveAt(tierIndex);
    }

    public void addVehicle(byte tierIndex, ushort id)
    {
        tiers[tierIndex].addVehicle(id);
    }

    public void removeVehicle(byte tierIndex, byte vehicleIndex)
    {
        tiers[tierIndex].removeVehicle(vehicleIndex);
    }

    public ushort getVehicle()
    {
        if (tableID != 0)
        {
            return SpawnTableTool.ResolveLegacyId(tableID, EAssetType.VEHICLE, OnGetSpawnTableErrorContext);
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
                VehicleTier vehicleTier = tiers[i];
                if (vehicleTier.table.Count > 0)
                {
                    return vehicleTier.table[Random.Range(0, vehicleTier.table.Count)].vehicle;
                }
                return 0;
            }
        }
        VehicleTier vehicleTier2 = tiers[Random.Range(0, tiers.Count)];
        if (vehicleTier2.table.Count > 0)
        {
            return vehicleTier2.table[Random.Range(0, vehicleTier2.table.Count)].vehicle;
        }
        return 0;
    }

    public void buildTable()
    {
        List<VehicleTier> list = new List<VehicleTier>();
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

    public VehicleTable(string newName)
    {
        _tiers = new List<VehicleTier>();
        _color = Color.white;
        name = newName;
        tableID = 0;
    }

    public VehicleTable(List<VehicleTier> newTiers, Color newColor, string newName, ushort newTableID)
    {
        _tiers = newTiers;
        _color = newColor;
        name = newName;
        tableID = newTableID;
    }

    private string OnGetSpawnTableErrorContext()
    {
        return "\"" + Level.info.name + "\" vehicle table \"" + name + "\"";
    }

    internal string OnGetSpawnTableValidationErrorContext()
    {
        return "\"" + Level.info.name + "\" vehicle table \"" + name + "\" validation";
    }
}
