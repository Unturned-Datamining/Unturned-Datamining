using UnityEngine;

namespace SDG.Unturned;

public class LocationNode : Node
{
    public string name;

    public LocationNode(Vector3 newPoint)
        : this(newPoint, "")
    {
    }

    public LocationNode(Vector3 newPoint, string newName)
    {
        _point = newPoint;
        name = newName;
        _type = ENodeType.LOCATION;
    }
}
