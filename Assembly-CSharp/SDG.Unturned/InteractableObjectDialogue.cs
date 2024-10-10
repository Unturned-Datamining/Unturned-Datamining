using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableObjectDialogue : InteractableObject, IDialogueTarget
{
    public Vector3 GetDialogueTargetWorldPosition()
    {
        return base.transform.position;
    }

    public NetId GetDialogueTargetNetId()
    {
        return NetIdRegistry.GetTransformNetId(base.transform);
    }

    public bool ShouldServerApproveDialogueRequest(Player withPlayer)
    {
        return base.objectAsset.areConditionsMet(withPlayer);
    }

    public DialogueAsset FindStartingDialogueAsset()
    {
        return base.objectAsset.FindInteractabilityDialogueAsset();
    }

    public string GetDialogueTargetDebugName()
    {
        return base.objectAsset.FriendlyName;
    }

    public string GetDialogueTargetNameShownToPlayer(Player player)
    {
        return base.objectAsset.FriendlyName;
    }

    public void SetFaceOverride(byte? faceOverride)
    {
    }

    public void SetIsTalkingWithLocalPlayer(bool isTalkingWithLocalPlayer)
    {
    }

    public override void use()
    {
        if (FindStartingDialogueAsset() == null)
        {
            UnturnedLog.warn("Failed to find interactable object dialogue: " + GetDialogueTargetDebugName());
        }
        else
        {
            ObjectManager.SendTalkWithNpcRequest.Invoke(ENetReliability.Reliable, GetDialogueTargetNetId());
        }
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
