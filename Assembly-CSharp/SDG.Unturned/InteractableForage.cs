using UnityEngine;

namespace SDG.Unturned;

public class InteractableForage : Interactable
{
    internal ResourceAsset asset;

    public override void use()
    {
        ResourceManager.forage(base.transform.parent);
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        if (asset != null && !string.IsNullOrEmpty(asset.interactabilityText))
        {
            message = EPlayerMessage.INTERACT;
            text = asset.interactabilityText;
        }
        else
        {
            message = EPlayerMessage.FORAGE;
            text = string.Empty;
        }
        color = Color.white;
        return true;
    }
}
