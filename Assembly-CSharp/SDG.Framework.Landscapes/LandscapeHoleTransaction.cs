using SDG.Framework.Devkit.Transactions;

namespace SDG.Framework.Landscapes;

public class LandscapeHoleTransaction : IDevkitTransaction
{
    protected LandscapeTile tile;

    protected bool[,] holesCopy;

    public bool delta => true;

    public void undo()
    {
        if (tile != null)
        {
            bool[,] holes = tile.holes;
            tile.holes = holesCopy;
            holesCopy = holes;
            tile.data.SetHoles(0, 0, tile.holes);
        }
    }

    public void redo()
    {
        undo();
    }

    public void begin()
    {
        holesCopy = LandscapeHoleCopyPool.claim();
        for (int i = 0; i < 256; i++)
        {
            for (int j = 0; j < 256; j++)
            {
                holesCopy[i, j] = tile.holes[i, j];
            }
        }
    }

    public void end()
    {
    }

    public void forget()
    {
        LandscapeHoleCopyPool.release(holesCopy);
    }

    public LandscapeHoleTransaction(LandscapeTile newTile)
    {
        tile = newTile;
    }
}
