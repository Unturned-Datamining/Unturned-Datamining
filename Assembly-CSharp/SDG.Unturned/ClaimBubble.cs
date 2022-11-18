using UnityEngine;

namespace SDG.Unturned;

public class ClaimBubble : ClaimBase
{
    public Vector3 origin;

    public float sqrRadius;

    public ClaimBubble(Vector3 newOrigin, float newSqrRadius, ulong newOwner, ulong newGroup)
        : base(newOwner, newGroup)
    {
        origin = newOrigin;
        sqrRadius = newSqrRadius;
    }
}
