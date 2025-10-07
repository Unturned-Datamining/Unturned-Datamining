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
        string interactabilityDialogueDisplayName = base.objectAsset.InteractabilityDialogueDisplayName;
        if (string.IsNullOrEmpty(interactabilityDialogueDisplayName))
        {
            return "object " + base.objectAsset.objectName;
        }
        return "object " + interactabilityDialogueDisplayName + " / " + base.objectAsset.objectName;
    }

    public string GetDialogueTargetNameShownToPlayer(Player player)
    {
        string text = base.objectAsset.InteractabilityDialogueDisplayName;
        if (string.IsNullOrEmpty(text))
        {
            text = base.objectAsset.objectName;
        }
        return text;
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
        return base.objectAsset.areInteractabilityConditionsMet(Player.LocalPlayer);
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        INPCCondition firstUnmetCondition = base.objectAsset.interactabilityConditionsList.GetFirstUnmetCondition(Player.LocalPlayer);
        if (firstUnmetCondition != null)
        {
            text = firstUnmetCondition.formatCondition(Player.LocalPlayer);
            color = Color.white;
            if (string.IsNullOrEmpty(text))
            {
                message = EPlayerMessage.NONE;
                return false;
            }
            message = EPlayerMessage.CONDITION;
            return true;
        }
        message = EPlayerMessage.INTERACT;
        text = base.objectAsset.interactabilityText;
        color = Color.white;
        return true;
    }
}
