using SDG.Framework.Devkit.Transactions;

namespace SDG.Framework.Landscapes;

public class LandscapeHeightmapTransaction : IDevkitTransaction
{
    protected LandscapeTile tile;

    protected float[,] heightmapCopy;

    public bool delta => true;

    public void undo()
    {
        if (tile != null)
        {
            float[,] heightmap = tile.heightmap;
            tile.heightmap = heightmapCopy;
            heightmapCopy = heightmap;
            tile.SetHeightsDelayLOD();
            tile.SyncHeightmap();
        }
    }

    public void redo()
    {
        undo();
    }

    public void begin()
    {
        heightmapCopy = LandscapeHeightmapCopyPool.claim();
        for (int i = 0; i < Landscape.HEIGHTMAP_RESOLUTION; i++)
        {
            for (int j = 0; j < Landscape.HEIGHTMAP_RESOLUTION; j++)
            {
                heightmapCopy[i, j] = tile.heightmap[i, j];
            }
        }
    }

    public void end()
    {
    }

    public void forget()
    {
        LandscapeHeightmapCopyPool.release(heightmapCopy);
    }

    public LandscapeHeightmapTransaction(LandscapeTile newTile)
    {
        tile = newTile;
    }
}
