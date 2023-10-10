using UnityEngine;

namespace SDG.Unturned;

public class InteractableObjectDropper : InteractableObject
{
    private float lastUsed = -9999f;

    private ushort[] interactabilityDrops;

    private ushort interactabilityRewardID;

    private AudioSource audioSourceComponent;

    private Transform dropTransform;

    public bool isUsable
    {
        get
        {
            if (Time.realtimeSinceStartup - lastUsed > base.objectAsset.interactabilityDelay)
            {
                if (base.objectAsset.interactabilityPower != 0)
                {
                    return base.isWired;
                }
                return true;
            }
            return false;
        }
    }

    private void initAudioSourceComponent()
    {
        audioSourceComponent = base.transform.GetComponent<AudioSource>();
    }

    private void updateAudioSourceComponent()
    {
        if (audioSourceComponent != null && !Dedicator.IsDedicatedServer)
        {
            audioSourceComponent.Play();
        }
    }

    private void initDropTransform()
    {
        dropTransform = base.transform.Find("Drop");
    }

    public override void updateState(Asset asset, byte[] state)
    {
        base.updateState(asset, state);
        interactabilityDrops = ((ObjectAsset)asset).interactabilityDrops;
        interactabilityRewardID = ((ObjectAsset)asset).interactabilityRewardID;
        initAudioSourceComponent();
        initDropTransform();
    }

    public void drop()
    {
        lastUsed = Time.realtimeSinceStartup;
        if (dropTransform == null || (base.objectAsset.holidayRestriction != 0 && !Provider.modeConfigData.Objects.Allow_Holiday_Drops))
        {
            return;
        }
        if (interactabilityRewardID != 0)
        {
            ushort num = SpawnTableTool.ResolveLegacyId(interactabilityRewardID, EAssetType.ITEM, OnGetDropSpawnTableErrorContext);
            if (num != 0)
            {
                ItemManager.dropItem(new Item(num, EItemOrigin.NATURE), dropTransform.position, playEffect: false, isDropped: true, wideSpread: false);
            }
        }
        else
        {
            ushort num2 = interactabilityDrops[Random.Range(0, interactabilityDrops.Length)];
            if (num2 != 0)
            {
                ItemManager.dropItem(new Item(num2, EItemOrigin.NATURE), dropTransform.position, playEffect: false, isDropped: true, wideSpread: false);
            }
        }
    }

    public override void use()
    {
        updateAudioSourceComponent();
        ObjectManager.useObjectDropper(base.transform);
    }

    public override bool checkUseable()
    {
        if (base.objectAsset.interactabilityPower == EObjectInteractabilityPower.NONE || base.isWired)
        {
            return base.objectAsset.areInteractabilityConditionsMet(Player.player);
        }
        return false;
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        for (int i = 0; i < base.objectAsset.interactabilityConditions.Length; i++)
        {
            INPCCondition iNPCCondition = base.objectAsset.interactabilityConditions[i];
            if (!iNPCCondition.isConditionMet(Player.player))
            {
                message = EPlayerMessage.CONDITION;
                text = iNPCCondition.formatCondition(Player.player);
                color = Color.white;
                return true;
            }
        }
        if (base.objectAsset.interactabilityPower != 0 && !base.isWired)
        {
            message = EPlayerMessage.POWER;
        }
        else
        {
            switch (base.objectAsset.interactabilityHint)
            {
            case EObjectInteractabilityHint.DOOR:
                message = EPlayerMessage.DOOR_OPEN;
                break;
            case EObjectInteractabilityHint.SWITCH:
                message = EPlayerMessage.SPOT_ON;
                break;
            case EObjectInteractabilityHint.FIRE:
                message = EPlayerMessage.FIRE_ON;
                break;
            case EObjectInteractabilityHint.GENERATOR:
                message = EPlayerMessage.GENERATOR_ON;
                break;
            case EObjectInteractabilityHint.USE:
                message = EPlayerMessage.USE;
                break;
            case EObjectInteractabilityHint.CUSTOM:
                message = EPlayerMessage.INTERACT;
                text = base.objectAsset.interactabilityText;
                color = Color.white;
                return true;
            default:
                message = EPlayerMessage.NONE;
                break;
            }
        }
        text = "";
        color = Color.white;
        return true;
    }

    private string OnGetDropSpawnTableErrorContext()
    {
        return base.objectAsset?.FriendlyName + " drop";
    }
}
