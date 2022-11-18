using System;
using UnityEngine;

namespace SDG.Framework.Landscapes;

public struct HeightmapCoord : IEquatable<HeightmapCoord>
{
    public int x;

    public int y;

    public static bool operator ==(HeightmapCoord a, HeightmapCoord b)
    {
        if (a.x == b.x)
        {
            return a.y == b.y;
        }
        return false;
    }

    public static bool operator !=(HeightmapCoord a, HeightmapCoord b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        HeightmapCoord heightmapCoord = (HeightmapCoord)obj;
        if (x == heightmapCoord.x)
        {
            return y == heightmapCoord.y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return x ^ y;
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ")";
    }

    public bool Equals(HeightmapCoord other)
    {
        if (x == other.x)
        {
            return y == other.y;
        }
        return false;
    }

    public HeightmapCoord(int new_x, int new_y)
    {
        x = new_x;
        y = new_y;
    }

    public HeightmapCoord(LandscapeCoord tileCoord, Vector3 worldPosition)
    {
        x = Mathf.Clamp(Mathf.RoundToInt((worldPosition.z - (float)tileCoord.y * Landscape.TILE_SIZE) / Landscape.TILE_SIZE * (float)Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE), 0, Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE);
        y = Mathf.Clamp(Mathf.RoundToInt((worldPosition.x - (float)tileCoord.x * Landscape.TILE_SIZE) / Landscape.TILE_SIZE * (float)Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE), 0, Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE);
    }
}
