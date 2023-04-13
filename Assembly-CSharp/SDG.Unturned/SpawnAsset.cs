using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class SpawnAsset : Asset
{
    private static SpawnTableWeightAscendingComparator comparator = new SpawnTableWeightAscendingComparator();

    protected List<SpawnTable> _roots;

    protected List<SpawnTable> _tables;

    public List<SpawnTable> insertRoots { get; protected set; }

    public List<SpawnTable> roots => _roots;

    public List<SpawnTable> tables => _tables;

    public override EAssetType assetCategory => EAssetType.SPAWN;

    public bool hasBeenOverridden { get; protected set; }

    public bool areTablesDirty { get; protected set; }

    public void markOverridden()
    {
        if (hasBeenOverridden)
        {
            return;
        }
        hasBeenOverridden = true;
        foreach (SpawnTable table in tables)
        {
            if (!table.isOverride)
            {
                table.weight = 0;
            }
        }
    }

    public void resolve(out ushort id, out bool isSpawn)
    {
        id = 0;
        isSpawn = false;
        if (tables.Count < 1)
        {
            UnturnedLog.warn("Spawn table {0} '{1}' resolved while empty", base.id, name);
            return;
        }
        if (areTablesDirty)
        {
            UnturnedLog.warn("Spawn table {0} '{1}' resolved while dirty", base.id, name);
            sortAndNormalizeWeights();
        }
        float value = Random.value;
        for (int i = 0; i < tables.Count; i++)
        {
            if (value < tables[i].chance || i == tables.Count - 1)
            {
                if (tables[i].spawnID != 0)
                {
                    id = tables[i].spawnID;
                    isSpawn = true;
                    break;
                }
                if (tables[i].assetID != 0)
                {
                    id = tables[i].assetID;
                    isSpawn = false;
                    break;
                }
            }
        }
    }

    public void sortAndNormalizeWeights()
    {
        if (!areTablesDirty)
        {
            return;
        }
        areTablesDirty = false;
        tables.Sort(comparator);
        float num = 0f;
        foreach (SpawnTable table in tables)
        {
            num += (float)table.weight;
        }
        float num2 = 0f;
        foreach (SpawnTable table2 in tables)
        {
            num2 += (float)table2.weight;
            table2.chance = num2 / num;
        }
    }

    public void markTablesDirty()
    {
        areTablesDirty = true;
    }

    public void addSpawnTable(SpawnAsset other)
    {
        SpawnTable spawnTable = new SpawnTable();
        spawnTable.spawnID = id;
        spawnTable.isLink = true;
        other.roots.Add(spawnTable);
        SpawnTable spawnTable2 = new SpawnTable();
        spawnTable2.spawnID = other.id;
        tables.Add(spawnTable2);
        markTablesDirty();
    }

    public void addAssetTable(ushort newID)
    {
        SpawnTable spawnTable = new SpawnTable();
        spawnTable.assetID = newID;
        tables.Add(spawnTable);
        markTablesDirty();
    }

    public void removeRootAtIndex(int rootIndex)
    {
        SpawnTable spawnTable = roots[rootIndex];
        if (spawnTable.spawnID != 0 && Assets.find(EAssetType.SPAWN, spawnTable.spawnID) is SpawnAsset spawnAsset)
        {
            for (int i = 0; i < spawnAsset.tables.Count; i++)
            {
                if (spawnAsset.tables[i].spawnID == id)
                {
                    spawnAsset.tables.RemoveAt(i);
                    spawnAsset.markTablesDirty();
                    break;
                }
            }
        }
        roots.RemoveAt(rootIndex);
    }

    public void removeTableAtIndex(int tableIndex)
    {
        SpawnTable spawnTable = tables[tableIndex];
        if (spawnTable.spawnID != 0 && Assets.find(EAssetType.SPAWN, spawnTable.spawnID) is SpawnAsset spawnAsset)
        {
            for (int i = 0; i < spawnAsset.roots.Count; i++)
            {
                if (spawnAsset.roots[i].spawnID == id)
                {
                    spawnAsset.roots.RemoveAt(i);
                    break;
                }
            }
        }
        tables.RemoveAt(tableIndex);
        markTablesDirty();
    }

    public void setTableWeightAtIndex(int tableIndex, int weight)
    {
        tables[tableIndex].weight = weight;
        markTablesDirty();
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        int num = data.ParseInt32("Roots");
        insertRoots = new List<SpawnTable>(num);
        for (int i = 0; i < num; i++)
        {
            SpawnTable spawnTable = new SpawnTable();
            spawnTable.spawnID = data.ParseUInt16("Root_" + i + "_Spawn_ID", 0);
            spawnTable.isOverride = data.ContainsKey("Root_" + i + "_Override");
            spawnTable.weight = data.ParseInt32("Root_" + i + "_Weight", spawnTable.isOverride ? 1 : 0);
            spawnTable.chance = 0f;
            if (spawnTable.spawnID == 0 && spawnTable.assetID == 0)
            {
                Assets.reportError(this, "root " + i + " has neither a spawnID nor an assetID!");
            }
            if (spawnTable.weight <= 0)
            {
                Assets.reportError(this, "root " + i + " has no weight!");
            }
            insertRoots.Add(spawnTable);
        }
        _roots = new List<SpawnTable>(num);
        int num2 = data.ParseInt32("Tables");
        _tables = new List<SpawnTable>(num2);
        for (int j = 0; j < num2; j++)
        {
            SpawnTable spawnTable2 = new SpawnTable();
            spawnTable2.assetID = data.ParseUInt16("Table_" + j + "_Asset_ID", 0);
            spawnTable2.spawnID = data.ParseUInt16("Table_" + j + "_Spawn_ID", 0);
            spawnTable2.weight = data.ParseInt32("Table_" + j + "_Weight");
            spawnTable2.chance = 0f;
            if (spawnTable2.spawnID == 0 && spawnTable2.assetID == 0)
            {
                Assets.reportError(this, "table " + j + " has neither a spawnID nor an assetID!");
            }
            if (spawnTable2.weight <= 0)
            {
                Assets.reportError(this, "table " + j + " has no weight!");
            }
            tables.Add(spawnTable2);
        }
        areTablesDirty = true;
    }
}
