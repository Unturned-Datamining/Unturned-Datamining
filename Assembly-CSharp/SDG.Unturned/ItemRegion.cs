using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class ItemRegion
{
    private List<ItemDrop> _drops;

    public List<ItemData> items;

    public bool isNetworked;

    internal bool isPendingDestroy;

    public ushort despawnItemIndex;

    public ushort respawnItemIndex;

    public float lastRespawn;

    public List<ItemDrop> drops => _drops;

    [Obsolete("Renamed to DestroyAll")]
    public void destroy()
    {
        DestroyAll();
    }

    internal void DestroyTail()
    {
        UnityEngine.Object.Destroy(drops.GetAndRemoveTail().model.gameObject);
    }

    public void DestroyAll()
    {
        for (ushort num = 0; num < drops.Count; num = (ushort)(num + 1))
        {
            UnityEngine.Object.Destroy(drops[num].model.gameObject);
        }
        drops.Clear();
    }

    public ItemRegion()
    {
        _drops = new List<ItemDrop>();
        items = new List<ItemData>();
        isNetworked = false;
        isPendingDestroy = false;
        lastRespawn = Time.realtimeSinceStartup;
        despawnItemIndex = 0;
        respawnItemIndex = 0;
    }
}
