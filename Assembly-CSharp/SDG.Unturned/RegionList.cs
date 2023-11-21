using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal class RegionList<T>
{
    private List<T>[,] grid;

    private List<List<T>> listPool;

    private const int GRID_SIZE = 512;

    private const int CELL_SIZE = 16;

    /// <summary>
    /// Number of Lists to preallocate in batches.
    /// (GRID_SIZE * GRID_SIZE) % LIST_POOL_SIZE should be zero leftover.
    /// Reduces constructor performance cost. (public issue #4209)
    /// </summary>
    private const int LIST_POOL_SIZE = 1024;

    public void Add(Vector3 position, T item)
    {
        int cellIndex = GetCellIndex(position.x);
        int cellIndex2 = GetCellIndex(position.z);
        GetOrAddList(cellIndex, cellIndex2).Add(item);
    }

    public bool RemoveFast(Vector3 position, T item, float tolerance)
    {
        foreach (List<T> item2 in EnumerateListsInSquare(position, tolerance))
        {
            if (item2.RemoveFast(item))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Can be null if nothing has been added at position.
    /// </summary>
    public List<T> GetList(Vector3 position)
    {
        return grid[GetCellIndex(position.x), GetCellIndex(position.z)];
    }

    public IEnumerable<T> EnumerateAllItems()
    {
        List<T>[,] array = grid;
        foreach (List<T> list in array)
        {
            if (list == null)
            {
                continue;
            }
            foreach (T item in list)
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Does not add new lists to empty cells.
    /// </summary>
    public IEnumerable<List<T>> EnumerateListsInSquare(Vector3 position, float radius)
    {
        int cellIndex = GetCellIndex(position.x - radius);
        int max_x = GetCellIndex(position.x + radius);
        int min_z = GetCellIndex(position.z - radius);
        int max_z = GetCellIndex(position.z + radius);
        int x = cellIndex;
        while (x <= max_x)
        {
            int num;
            for (int z = min_z; z <= max_z; z = num)
            {
                if (grid[x, z] != null)
                {
                    yield return grid[x, z];
                }
                num = z + 1;
            }
            num = x + 1;
            x = num;
        }
    }

    public IEnumerable<T> EnumerateItemsInSquare(Vector3 position, float radius)
    {
        int cellIndex = GetCellIndex(position.x - radius);
        int max_x = GetCellIndex(position.x + radius);
        int min_z = GetCellIndex(position.z - radius);
        int max_z = GetCellIndex(position.z + radius);
        int x = cellIndex;
        while (x <= max_x)
        {
            int num;
            for (int z = min_z; z <= max_z; z = num)
            {
                List<T> list = grid[x, z];
                if (list != null)
                {
                    foreach (T item in list)
                    {
                        yield return item;
                    }
                }
                num = z + 1;
            }
            num = x + 1;
            x = num;
        }
    }

    public void DrawGrid(Vector3 cameraPosition, Color color)
    {
        cameraPosition.x = (float)(GetCellIndex(cameraPosition.x) * 16) - 4096f + 8f;
        cameraPosition.y = (float)Mathf.FloorToInt(cameraPosition.y * 0.1f) * 10f - 5f;
        cameraPosition.z = (float)(GetCellIndex(cameraPosition.z) * 16) - 4096f + 8f;
        RuntimeGizmos.Get().GridXZ(cameraPosition, 80f, 5, color);
    }

    public RegionList()
    {
        grid = new List<T>[512, 512];
        listPool = new List<List<T>>(1024);
        for (int i = 0; i < 1024; i++)
        {
            listPool.Add(new List<T>());
        }
    }

    private List<T> GetOrAddList(int x, int z)
    {
        List<T> list = grid[x, z];
        if (list != null)
        {
            return list;
        }
        if (listPool.IsEmpty())
        {
            for (int i = 0; i < 1024; i++)
            {
                listPool.Add(new List<T>());
            }
        }
        list = listPool.GetAndRemoveTail();
        grid[x, z] = list;
        return list;
    }

    private int GetCellIndex(float position)
    {
        if (position <= -4096f)
        {
            return 0;
        }
        if (position >= 4096f)
        {
            return 511;
        }
        return Mathf.FloorToInt((position + 4096f) / 16f);
    }
}
