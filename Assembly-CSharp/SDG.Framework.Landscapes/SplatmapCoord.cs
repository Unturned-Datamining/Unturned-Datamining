using System;
using UnityEngine;

namespace SDG.Framework.Landscapes;

public struct SplatmapCoord : IEquatable<SplatmapCoord>
{
    public int x;

    public int y;

    public static bool operator ==(SplatmapCoord a, SplatmapCoord b)
    {
        if (a.x == b.x)
        {
            return a.y == b.y;
        }
        return false;
    }

    public static bool operator !=(SplatmapCoord a, SplatmapCoord b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        SplatmapCoord splatmapCoord = (SplatmapCoord)obj;
        if (x.Equals(splatmapCoord.x))
        {
            return y.Equals(splatmapCoord.y);
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

    public bool Equals(SplatmapCoord other)
    {
        if (x == other.x)
        {
            return y == other.y;
        }
        return false;
    }

    public SplatmapCoord(int new_x, int new_y)
    {
        x = new_x;
        y = new_y;
    }

    public SplatmapCoord(LandscapeCoord tileCoord, Vector3 worldPosition)
    {
        x = Mathf.Clamp(Mathf.FloorToInt((worldPosition.z - (float)tileCoord.y * Landscape.TILE_SIZE) / Landscape.TILE_SIZE * (float)Landscape.SPLATMAP_RESOLUTION), 0, Landscape.SPLATMAP_RESOLUTION_MINUS_ONE);
        y = Mathf.Clamp(Mathf.FloorToInt((worldPosition.x - (float)tileCoord.x * Landscape.TILE_SIZE) / Landscape.TILE_SIZE * (float)Landscape.SPLATMAP_RESOLUTION), 0, Landscape.SPLATMAP_RESOLUTION_MINUS_ONE);
    }
}
