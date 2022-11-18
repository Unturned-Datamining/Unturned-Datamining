using UnityEngine;

namespace SDG.Unturned;

public class Interactable2 : MonoBehaviour
{
    public ulong owner;

    public ulong group;

    public float salvageDurationMultiplier = 1f;

    public bool hasOwnership => OwnershipTool.checkToggle(owner, group);

    public virtual bool checkHint(out EPlayerMessage message, out float data)
    {
        message = EPlayerMessage.NONE;
        data = 0f;
        return false;
    }

    public virtual void use()
    {
    }
}
