using UnityEngine;

namespace SDG.Unturned;

public class InteractableObjectQuest : InteractableObject
{
    private float lastEffect;

    public override void use()
    {
        EffectAsset effectAsset = base.objectAsset.FindInteractabilityEffectAsset();
        if (effectAsset != null && Time.realtimeSinceStartup - lastEffect > 1f)
        {
            lastEffect = Time.realtimeSinceStartup;
            Transform transform = base.transform.Find("Effect");
            if (transform != null)
            {
                EffectManager.effect(effectAsset, transform.position, transform.forward);
            }
            else
            {
                EffectManager.effect(effectAsset, base.transform.position, base.transform.forward);
            }
        }
        ObjectManager.useObjectQuest(base.transform);
    }

    public override bool checkUseable()
    {
        return base.objectAsset.areInteractabilityConditionsMet(Player.player);
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        for (int i = 0; i < base.objectAsset.interactabilityConditions.Length; i++)
        {
            INPCCondition iNPCCondition = base.objectAsset.interactabilityConditions[i];
            if (!iNPCCondition.isConditionMet(Player.player))
            {
                text = iNPCCondition.formatCondition(Player.player);
                color = Color.white;
                if (string.IsNullOrEmpty(text))
                {
                    message = EPlayerMessage.NONE;
                    return false;
                }
                message = EPlayerMessage.CONDITION;
                return true;
            }
        }
        message = EPlayerMessage.INTERACT;
        text = base.objectAsset.interactabilityText;
        color = Color.white;
        return true;
    }
}
