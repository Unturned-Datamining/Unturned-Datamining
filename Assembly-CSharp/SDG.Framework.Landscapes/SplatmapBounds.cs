using UnityEngine;

namespace SDG.Framework.Landscapes;

public struct SplatmapBounds
{
    public SplatmapCoord min;

    public SplatmapCoord max;

    public override string ToString()
    {
        return "[" + min.ToString() + ", " + max.ToString() + "]";
    }

    public SplatmapBounds(SplatmapCoord newMin, SplatmapCoord newMax)
    {
        min = newMin;
        max = newMax;
    }

    public SplatmapBounds(LandscapeCoord tileCoord, Bounds worldBounds)
    {
        int new_x = Mathf.Clamp(Mathf.FloorToInt((worldBounds.min.z - (float)tileCoord.y * Landscape.TILE_SIZE) / Landscape.TILE_SIZE * (float)Landscape.SPLATMAP_RESOLUTION), 0, Landscape.SPLATMAP_RESOLUTION_MINUS_ONE);
        int new_x2 = Mathf.Clamp(Mathf.FloorToInt((worldBounds.max.z - (float)tileCoord.y * Landscape.TILE_SIZE) / Landscape.TILE_SIZE * (float)Landscape.SPLATMAP_RESOLUTION), 0, Landscape.SPLATMAP_RESOLUTION_MINUS_ONE);
        int new_y = Mathf.Clamp(Mathf.FloorToInt((worldBounds.min.x - (float)tileCoord.x * Landscape.TILE_SIZE) / Landscape.TILE_SIZE * (float)Landscape.SPLATMAP_RESOLUTION), 0, Landscape.SPLATMAP_RESOLUTION_MINUS_ONE);
        int new_y2 = Mathf.Clamp(Mathf.FloorToInt((worldBounds.max.x - (float)tileCoord.x * Landscape.TILE_SIZE) / Landscape.TILE_SIZE * (float)Landscape.SPLATMAP_RESOLUTION), 0, Landscape.SPLATMAP_RESOLUTION_MINUS_ONE);
        min = new SplatmapCoord(new_x, new_y);
        max = new SplatmapCoord(new_x2, new_y2);
    }
}
