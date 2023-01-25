using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

internal class HousingEdge
{
    public Vector3 position;

    public Vector3 direction;

    public float rotation;

    public List<StructureDrop> forwardFloors;

    public List<StructureDrop> backwardFloors;

    public List<StructureDrop> walls;

    public HousingVertex vertex0;

    public HousingVertex vertex1;

    public HousingEdge upperEdge;

    public HousingEdge lowerEdge;

    public bool ShouldBeRemoved
    {
        get
        {
            if (backwardFloors.IsEmpty() && forwardFloors.IsEmpty() && walls.IsEmpty())
            {
                if (lowerEdge != null)
                {
                    return lowerEdge.walls.IsEmpty();
                }
                return true;
            }
            return false;
        }
    }

    public bool CanAttachRoof
    {
        get
        {
            if (forwardFloors.Count + backwardFloors.Count + walls.Count > 0)
            {
                return true;
            }
            if (lowerEdge != null && lowerEdge.HasFullHeightWall())
            {
                return true;
            }
            if (vertex0 != null && vertex1 != null && vertex0.lowerVertex != null && vertex1.lowerVertex != null && vertex0.lowerVertex.HasFullHeightPillar() && vertex1.lowerVertex.HasFullHeightPillar())
            {
                return true;
            }
            return false;
        }
    }

    public bool HasFullHeightWall()
    {
        foreach (StructureDrop wall in walls)
        {
            if (wall.asset.construct == EConstruct.WALL)
            {
                return true;
            }
        }
        return false;
    }
}
