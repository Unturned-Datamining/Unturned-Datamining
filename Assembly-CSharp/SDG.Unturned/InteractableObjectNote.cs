using UnityEngine;

namespace SDG.Unturned;

public class InteractableObjectNote : InteractableObjectTriggerableBase
{
    public override void use()
    {
        PlayerBarricadeSignUI.open(base.objectAsset.interactabilityText);
        PlayerLifeUI.close();
        ObjectManager.useObjectQuest(base.transform);
    }

    public override bool checkUseable()
    {
        return !PlayerUI.window.showCursor;
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        if (base.objectAsset.interactabilityHint == EObjectInteractabilityHint.USE)
        {
            message = EPlayerMessage.USE;
        }
        else
        {
            message = EPlayerMessage.NONE;
        }
        text = "";
        color = Color.white;
        return !PlayerUI.window.showCursor;
    }
}
