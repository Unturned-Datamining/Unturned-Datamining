namespace SDG.Framework.Landscapes;

public class LandscapeUtility
{
    public static void cleanHeightmapCoord(ref LandscapeCoord tileCoord, ref HeightmapCoord heightmapCoord)
    {
        if (heightmapCoord.x < 0)
        {
            tileCoord.y--;
            heightmapCoord.x += Landscape.HEIGHTMAP_RESOLUTION;
        }
        if (heightmapCoord.y < 0)
        {
            tileCoord.x--;
            heightmapCoord.y += Landscape.HEIGHTMAP_RESOLUTION;
        }
        if (heightmapCoord.x >= Landscape.HEIGHTMAP_RESOLUTION)
        {
            tileCoord.y++;
            heightmapCoord.x -= Landscape.HEIGHTMAP_RESOLUTION;
        }
        if (heightmapCoord.y >= Landscape.HEIGHTMAP_RESOLUTION)
        {
            tileCoord.x++;
            heightmapCoord.y -= Landscape.HEIGHTMAP_RESOLUTION;
        }
    }

    public static void cleanSplatmapCoord(ref LandscapeCoord tileCoord, ref SplatmapCoord splatmapCoord)
    {
        if (splatmapCoord.x < 0)
        {
            tileCoord.y--;
            splatmapCoord.x += Landscape.SPLATMAP_RESOLUTION;
        }
        if (splatmapCoord.y < 0)
        {
            tileCoord.x--;
            splatmapCoord.y += Landscape.SPLATMAP_RESOLUTION;
        }
        if (splatmapCoord.x >= Landscape.SPLATMAP_RESOLUTION)
        {
            tileCoord.y++;
            splatmapCoord.x -= Landscape.SPLATMAP_RESOLUTION;
        }
        if (splatmapCoord.y >= Landscape.SPLATMAP_RESOLUTION)
        {
            tileCoord.x++;
            splatmapCoord.y -= Landscape.SPLATMAP_RESOLUTION;
        }
    }
}
