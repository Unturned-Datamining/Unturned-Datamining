using UnityEngine;

namespace SDG.Unturned;

public struct RegionBounds
{
    public RegionCoord min;

    public RegionCoord max;

    public override string ToString()
    {
        return "[" + min.ToString() + ", " + max.ToString() + "]";
    }

    public RegionBounds(RegionCoord newMin, RegionCoord newMax)
    {
        min = newMin;
        max = newMax;
    }

    public RegionBounds(Bounds worldBounds)
    {
        min = new RegionCoord(worldBounds.min);
        min.ClampIntoBounds();
        max = new RegionCoord(worldBounds.max);
        max.ClampIntoBounds();
    }
}
