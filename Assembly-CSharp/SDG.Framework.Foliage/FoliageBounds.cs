using UnityEngine;

namespace SDG.Framework.Foliage;

public struct FoliageBounds
{
    public FoliageCoord min;

    public FoliageCoord max;

    public override string ToString()
    {
        return "[" + min.ToString() + ", " + max.ToString() + "]";
    }

    public FoliageBounds(FoliageCoord newMin, FoliageCoord newMax)
    {
        min = newMin;
        max = newMax;
    }

    public FoliageBounds(Bounds worldBounds)
    {
        min = new FoliageCoord(worldBounds.min);
        max = new FoliageCoord(worldBounds.max);
    }
}
