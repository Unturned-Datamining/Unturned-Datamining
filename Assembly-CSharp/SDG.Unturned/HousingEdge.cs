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
}
