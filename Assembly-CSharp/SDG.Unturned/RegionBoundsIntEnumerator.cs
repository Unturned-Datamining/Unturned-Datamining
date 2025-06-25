using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Allows foreach loop to iterate Vector2Int within RegionBoundsInt.
/// </summary>
public struct RegionBoundsIntEnumerator : IEnumerator<Vector2Int>, IEnumerator, IDisposable
{
    private Vector2Int min;

    private Vector2Int max;

    private Vector2Int coord;

    public Vector2Int Current => coord;

    object IEnumerator.Current => Current;

    public RegionBoundsIntEnumerator(RegionBoundsInt bounds)
    {
        min = bounds.min;
        max = bounds.max;
        coord = new Vector2Int(min.x - 1, min.y);
    }

    public void Dispose()
    {
    }

    public bool MoveNext()
    {
        coord = new Vector2Int(coord.x + 1, coord.y);
        if (coord.x > max.x)
        {
            coord = new Vector2Int(min.x, coord.y + 1);
            if (coord.y > max.y)
            {
                return false;
            }
        }
        return true;
    }

    public void Reset()
    {
        coord = new Vector2Int(min.x - 1, min.y);
    }
}
