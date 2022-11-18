using UnityEngine;

namespace SDG.Unturned;

public class Interactable2SalvageBarricade : Interactable2
{
    public Transform root;

    public Interactable2HP hp;

    public bool shouldBypassPickupOwnership;

    public override bool checkHint(out EPlayerMessage message, out float data)
    {
        message = EPlayerMessage.SALVAGE;
        if (hp != null)
        {
            data = (float)(int)hp.hp / 100f;
        }
        else
        {
            data = 0f;
        }
        if (!shouldBypassPickupOwnership && !base.hasOwnership)
        {
            return false;
        }
        return true;
    }

    public override void use()
    {
        BarricadeManager.salvageBarricade(root);
    }
}
