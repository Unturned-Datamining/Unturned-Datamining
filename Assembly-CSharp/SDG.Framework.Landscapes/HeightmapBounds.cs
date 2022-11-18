using UnityEngine;

namespace SDG.Framework.Landscapes;

public struct HeightmapBounds
{
    public HeightmapCoord min;

    public HeightmapCoord max;

    public override string ToString()
    {
        return "[" + min.ToString() + ", " + max.ToString() + "]";
    }

    public HeightmapBounds(HeightmapCoord newMin, HeightmapCoord newMax)
    {
        min = newMin;
        max = newMax;
    }

    public HeightmapBounds(LandscapeCoord tileCoord, Bounds worldBounds)
    {
        int new_x = Mathf.Clamp(Mathf.FloorToInt((worldBounds.min.z - (float)tileCoord.y * Landscape.TILE_SIZE) / Landscape.TILE_SIZE * (float)Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE), 0, Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE);
        int new_x2 = Mathf.Clamp(Mathf.CeilToInt((worldBounds.max.z - (float)tileCoord.y * Landscape.TILE_SIZE) / Landscape.TILE_SIZE * (float)Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE), 0, Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE);
        int new_y = Mathf.Clamp(Mathf.FloorToInt((worldBounds.min.x - (float)tileCoord.x * Landscape.TILE_SIZE) / Landscape.TILE_SIZE * (float)Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE), 0, Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE);
        int new_y2 = Mathf.Clamp(Mathf.CeilToInt((worldBounds.max.x - (float)tileCoord.x * Landscape.TILE_SIZE) / Landscape.TILE_SIZE * (float)Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE), 0, Landscape.HEIGHTMAP_RESOLUTION_MINUS_ONE);
        min = new HeightmapCoord(new_x, new_y);
        max = new HeightmapCoord(new_x2, new_y2);
    }
}
