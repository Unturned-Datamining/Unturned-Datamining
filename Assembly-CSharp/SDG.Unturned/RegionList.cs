using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal class RegionList<T>
{
    private List<T>[,] grid;

    private const int GRID_SIZE = 512;

    private const int CELL_SIZE = 16;

    public void Add(Vector3 position, T item)
    {
        grid[GetCellIndex(position.x), GetCellIndex(position.z)].Add(item);
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

    public List<T> GetList(Vector3 position)
    {
        return grid[GetCellIndex(position.x), GetCellIndex(position.z)];
    }

    public IEnumerable<List<T>> EnumerateListsInSquare(Vector3 position, float radius)
    {
        int cellIndex = GetCellIndex(position.x - radius);
        int max_x = GetCellIndex(position.x + radius);
        int min_y = GetCellIndex(position.z - radius);
        int max_y = GetCellIndex(position.z + radius);
        int x = cellIndex;
        while (x <= max_x)
        {
            int num;
            for (int y = min_y; y <= max_y; y = num)
            {
                yield return grid[x, y];
                num = y + 1;
            }
            num = x + 1;
            x = num;
        }
    }

    public IEnumerable<T> EnumerateItemsInSquare(Vector3 position, float radius)
    {
        int cellIndex = GetCellIndex(position.x - radius);
        int max_x = GetCellIndex(position.x + radius);
        int min_y = GetCellIndex(position.z - radius);
        int max_y = GetCellIndex(position.z + radius);
        int x = cellIndex;
        while (x <= max_x)
        {
            int num;
            for (int y = min_y; y <= max_y; y = num)
            {
                foreach (T item in grid[x, y])
                {
                    yield return item;
                }
                num = y + 1;
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
        for (int i = 0; i < 512; i++)
        {
            for (int j = 0; j < 512; j++)
            {
                grid[i, j] = new List<T>();
            }
        }
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
