using UnityEngine;

namespace SDG.Unturned;

public class ClaimPlant : ClaimBase
{
    public Transform parent;

    public ClaimPlant(Transform newParent, ulong newOwner, ulong newGroup)
        : base(newOwner, newGroup)
    {
        parent = newParent;
    }
}
