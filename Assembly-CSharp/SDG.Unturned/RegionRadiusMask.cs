using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Circular mask for 2D distances in meters on a 2D cell grid.
/// Includes a cell if the meters distance between the center cell
/// and the closest point on test cell is within radius.
/// </summary>
public class RegionRadiusMask
{
    private List<Vector2Int> offsets = new List<Vector2Int>();

    private float _radius = -1f;

    private int _cellSize = 128;

    private bool isDirty = true;

    public List<Vector2Int> Offsets
    {
        get
        {
            if (isDirty)
            {
                isDirty = false;
                RebuildOffsets();
            }
            return offsets;
        }
    }

    /// <summary>
    /// World space distance in meters.
    /// </summary>
    public float Radius
    {
        get
        {
            return _radius;
        }
        set
        {
            if (_radius != value)
            {
                _radius = value;
                isDirty = true;
            }
        }
    }

    /// <summary>
    /// Region cell size in meters.
    /// </summary>
    public int CellSize
    {
        get
        {
            return _cellSize;
        }
        set
        {
            if (_cellSize != value)
            {
                _cellSize = value;
                isDirty = true;
            }
        }
    }

    public void DebugDumpToStringBuilder(StringBuilder sb)
    {
        Vector2Int lhs = Vector2Int.zero;
        Vector2Int lhs2 = Vector2Int.zero;
        foreach (Vector2Int offset in Offsets)
        {
            lhs = Vector2Int.Min(lhs, offset);
            lhs2 = Vector2Int.Max(lhs2, offset);
        }
        for (int i = lhs.y; i <= lhs2.y; i++)
        {
            for (int j = lhs.x; j <= lhs2.x; j++)
            {
                Vector2Int item = new Vector2Int(j, i);
                if (Offsets.Contains(item))
                {
                    sb.Append('X');
                }
                else
                {
                    sb.Append('O');
                }
            }
            sb.AppendLine();
        }
    }

    public string DebugDumpToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        DebugDumpToStringBuilder(stringBuilder);
        return stringBuilder.ToString();
    }

    private void RebuildOffsets()
    {
        Offsets.Clear();
        Offsets.Add(Vector2Int.zero);
        if (_radius < Mathf.Epsilon)
        {
            return;
        }
        int num = Mathf.CeilToInt(_radius / (float)_cellSize);
        for (int i = 1; i <= num; i++)
        {
            Offsets.Add(new Vector2Int(-i, 0));
            Offsets.Add(new Vector2Int(i, 0));
            Offsets.Add(new Vector2Int(0, -i));
            Offsets.Add(new Vector2Int(0, i));
        }
        float num2 = _radius * _radius;
        for (int j = 1; j <= num; j++)
        {
            for (int k = 1; k <= num; k++)
            {
                int num3 = (j - 1) * _cellSize;
                int num4 = (k - 1) * _cellSize;
                if ((float)(num3 * num3 + num4 * num4) <= num2)
                {
                    Offsets.Add(new Vector2Int(j, k));
                    Offsets.Add(new Vector2Int(-j, k));
                    Offsets.Add(new Vector2Int(j, -k));
                    Offsets.Add(new Vector2Int(-j, -k));
                }
            }
        }
    }
}
