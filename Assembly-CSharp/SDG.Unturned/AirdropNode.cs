using UnityEngine;

namespace SDG.Unturned;

public class AirdropNode : Node
{
    public ushort id;

    public AirdropNode(Vector3 newPoint)
        : this(newPoint, 0)
    {
    }

    public AirdropNode(Vector3 newPoint, ushort newID)
    {
        _point = newPoint;
        id = newID;
        _type = ENodeType.AIRDROP;
    }
}
