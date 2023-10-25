using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal class HousingVertex
{
    /// <summary>
    /// Position at the base of the pillar.
    /// </summary>
    public Vector3 position;

    /// <summary>
    /// Yaw if placing pillar at this vertex.
    /// </summary>
    public float rotation;

    /// <summary>
    /// Pillar or post currently occupying this slot.
    /// Can be multiple on existing saves or if players found an exploit.
    /// </summary>
    public List<StructureDrop> pillars = new List<StructureDrop>(1);

    /// <summary>
    /// Can be zero if pillar is floating, or up to six in the center of a triangular circle.
    /// </summary>
    public List<StructureDrop> floors = new List<StructureDrop>(4);

    public List<HousingEdge> edges = new List<HousingEdge>(4);

    public HousingVertex upperVertex;

    public HousingVertex lowerVertex;

    public bool ShouldBeRemoved
    {
        get
        {
            if (pillars.Count < 1 && floors.Count < 1 && edges.Count < 1)
            {
                if (lowerVertex != null)
                {
                    return lowerVertex.pillars.IsEmpty();
                }
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Is there a pillar in this slot, and is it full height (not post)?
    /// </summary>
    public bool HasFullHeightPillar()
    {
        foreach (StructureDrop pillar in pillars)
        {
            if (pillar.asset.construct == EConstruct.PILLAR)
            {
                return true;
            }
        }
        return false;
    }
}
