using SDG.Framework.Devkit.Transactions;

namespace SDG.Framework.Landscapes;

public class LandscapeSplatmapTransaction : IDevkitTransaction
{
    protected LandscapeTile tile;

    protected float[,,] splatmapCopy;

    public bool delta => true;

    public void undo()
    {
        if (tile != null)
        {
            float[,,] splatmap = tile.splatmap;
            tile.splatmap = splatmapCopy;
            splatmapCopy = splatmap;
            tile.data.SetAlphamaps(0, 0, tile.splatmap);
        }
    }

    public void redo()
    {
        undo();
    }

    public void begin()
    {
        splatmapCopy = LandscapeSplatmapCopyPool.claim();
        for (int i = 0; i < Landscape.SPLATMAP_RESOLUTION; i++)
        {
            for (int j = 0; j < Landscape.SPLATMAP_RESOLUTION; j++)
            {
                for (int k = 0; k < Landscape.SPLATMAP_LAYERS; k++)
                {
                    splatmapCopy[i, j, k] = tile.splatmap[i, j, k];
                }
            }
        }
    }

    public void end()
    {
    }

    public void forget()
    {
        LandscapeSplatmapCopyPool.release(splatmapCopy);
    }

    public LandscapeSplatmapTransaction(LandscapeTile newTile)
    {
        tile = newTile;
    }
}
