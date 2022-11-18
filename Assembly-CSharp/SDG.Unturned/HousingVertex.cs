using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal class HousingVertex
{
    public Vector3 position;

    public float rotation;

    public List<StructureDrop> pillars = new List<StructureDrop>(1);

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
