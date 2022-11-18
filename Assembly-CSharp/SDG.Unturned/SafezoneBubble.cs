using UnityEngine;

namespace SDG.Unturned;

public class SafezoneBubble
{
    public Vector3 origin;

    public float sqrRadius;

    public SafezoneBubble(Vector3 newOrigin, float newSqrRadius)
    {
        origin = newOrigin;
        sqrRadius = newSqrRadius;
    }
}
