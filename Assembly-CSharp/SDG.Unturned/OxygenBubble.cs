using UnityEngine;

namespace SDG.Unturned;

public class OxygenBubble
{
    public Transform origin;

    public float sqrRadius;

    public OxygenBubble(Transform newOrigin, float newSqrRadius)
    {
        origin = newOrigin;
        sqrRadius = newSqrRadius;
    }
}
