using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Initially these were structs so that they would be adjacent in memory and therefore faster to iterate lots of them,
/// but making them classes lets them reference each other which significantly simplifies finding adjactent housing parts.
/// </summary>
internal class HousingEdge
{
    public Vector3 position;

    public Vector3 direction;

    public float rotation;

    /// <summary>
    /// Item along positive direction.
    /// Can be multiple on existing saves or if players found an exploit.
    /// </summary>
    public List<StructureDrop> forwardFloors;

    /// <summary>
    /// Item along negative direction.
    /// Can be multiple on existing saves or if players found an exploit.
    /// </summary>
    public List<StructureDrop> backwardFloors;

    /// <summary>
    /// Item between floors.
    /// Can be multiple on existing saves or if players found an exploit.
    /// </summary>
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

    /// <summary>
    /// This check prevents placing roof onto the upper edge of a rampart because ramparts
    /// create an edge at full wall height even though they are short.
    ///
    /// Ideally in the future wall height will become configurable and remove
    /// the need for this check.
    ///
    /// See public issue #3590.
    /// </summary>
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

    /// <summary>
    /// Is there a wall in this slot, and is it full height (not rampart)?
    /// </summary>
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
