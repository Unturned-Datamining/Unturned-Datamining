using UnityEngine;

namespace SDG.Unturned;

public class RoadJoint
{
    public Vector3 vertex;

    private Vector3[] tangents;

    public ERoadMode mode;

    public float offset;

    public bool ignoreTerrain;

    public Vector3 getTangent(int index)
    {
        return tangents[index];
    }

    public void setTangent(int index, Vector3 tangent)
    {
        tangents[index] = tangent;
        if (mode == ERoadMode.MIRROR)
        {
            tangents[1 - index] = -tangent;
        }
        else if (mode == ERoadMode.ALIGNED)
        {
            tangents[1 - index] = -tangent.normalized * tangents[1 - index].magnitude;
        }
    }

    public RoadJoint(Vector3 vertex)
    {
        this.vertex = vertex;
        tangents = new Vector3[2];
        mode = ERoadMode.MIRROR;
        offset = 0f;
        ignoreTerrain = false;
    }

    public RoadJoint(Vector3 vertex, Vector3[] tangents, ERoadMode mode, float offset, bool ignoreTerrain)
    {
        this.vertex = vertex;
        this.tangents = tangents;
        this.mode = mode;
        this.offset = offset;
        this.ignoreTerrain = ignoreTerrain;
    }
}
