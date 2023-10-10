using System;
using System.Collections;
using System.Collections.Generic;

namespace SDG.Unturned;

public struct SpawnTableRewardEnumerator : IEnumerable<ushort>, IEnumerable, IEnumerator<ushort>, IEnumerator, IDisposable
{
    public ushort tableID;

    public ushort assetID;

    public int count;

    public int index;

    public ushort Current => assetID;

    object IEnumerator.Current => assetID;

    public SpawnTableRewardEnumerator(ushort tableID, int count)
    {
        this.tableID = tableID;
        assetID = 0;
        this.count = count;
        index = -1;
    }

    public void Dispose()
    {
    }

    public IEnumerator<ushort> GetEnumerator()
    {
        return this;
    }

    public bool MoveNext()
    {
        while (++index < count)
        {
            assetID = SpawnTableTool.ResolveLegacyId(tableID, EAssetType.ITEM, OnGetSpawnTableErrorContext);
            if (assetID != 0)
            {
                return true;
            }
        }
        return false;
    }

    public void Reset()
    {
        index = -1;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this;
    }

    private string OnGetSpawnTableErrorContext()
    {
        return "consumable item";
    }
}
