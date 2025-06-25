using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal class RegionDictionary<T>
{
    internal Dictionary<Vector2Int, List<T>> data;

    public List<T> GetListOrNull(Vector2Int coord)
    {
        data.TryGetValue(coord, out var value);
        return value;
    }

    public List<T> GetOrAddList(Vector2Int coord)
    {
        if (!data.TryGetValue(coord, out var value))
        {
            value = new List<T>();
            data[coord] = value;
        }
        return value;
    }

    public List<T> GetOrAddList(byte x, byte y)
    {
        return GetOrAddList(new Vector2Int(x, y));
    }

    public void ReleaseListIfEmpty(Vector2Int coord)
    {
        if (data.TryGetValue(coord, out var value) && value != null && value.IsEmpty())
        {
            data.Remove(coord);
        }
    }

    public void GatherAllItems(List<T> results)
    {
        foreach (List<T> value in data.Values)
        {
            if (value != null && value.Count > 0)
            {
                results.AddRange(value);
            }
        }
    }

    public RegionDictionary()
    {
        data = new Dictionary<Vector2Int, List<T>>();
    }
}
