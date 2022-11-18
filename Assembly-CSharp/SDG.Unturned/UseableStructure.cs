using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableStructure : Useable
{
    private static readonly ServerInstanceMethod<Vector3, float> SendBuildStructure = ServerInstanceMethod<Vector3, float>.Get(typeof(UseableStructure), "ReceiveBuildStructure");

    private static readonly ClientInstanceMethod SendPlayConstruct = ClientInstanceMethod.Get(typeof(UseableStructure), "ReceivePlayConstruct");

    private Transform placementPreviewTransform;

    private bool isPlacementPreviewValid;

    private double useAnimationStartTime;

    private float useAnimationDuration;

    private bool isWaitingForSoundTrigger;

    private bool isUseAnimationPlaying;

    private bool hasServerReceivedBuildRequest;

    private bool isServerBuildRequestInitiallyApproved;

    private Vector3 pendingPlacementPosition;

    private float pendingPlacementYaw;

    private float animatedRotationOffset;

    private float customRotationOffset;

    private float foundationPositionOffset;

    private Vector3 serverPlacementPosition;

    private float serverPlacementYaw;

    public ItemStructureAsset equippedStructureAsset => base.player.equipment.asset as ItemStructureAsset;

    private bool HasFinishedUseAnimation => Time.timeAsDouble - useAnimationStartTime > (double)useAnimationDuration;

    private bool HasReachedSoundTrigger => Time.timeAsDouble - useAnimationStartTime > (double)(useAnimationDuration * 0.8f);

    [Obsolete]
    public void askStructure(CSteamID steamID, Vector3 newPoint, float newAngle)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10)]
    public void ReceiveBuildStructure(in ServerInvocationContext context, Vector3 newPoint, float newAngle)
    {
        if (hasServerReceivedBuildRequest)
        {
            return;
        }
        hasServerReceivedBuildRequest = true;
        if (!((newPoint - base.player.look.aim.position).sqrMagnitude < 256f))
        {
            return;
        }
        serverPlacementPosition = newPoint;
        serverPlacementYaw = newAngle;
        if (!UseableHousingUtils.IsPendingPositionValid(base.player, serverPlacementPosition))
        {
            isServerBuildRequestInitiallyApproved = false;
            return;
        }
        string obstructionHint = null;
        if (UseableHousingUtils.ValidatePendingPlacement(equippedStructureAsset, ref serverPlacementPosition, serverPlacementYaw, ref obstructionHint) == EHousingPlacementResult.Success)
        {
            isServerBuildRequestInitiallyApproved = true;
        }
        else
        {
            isServerBuildRequestInitiallyApproved = false;
        }
    }

    [Obsolete]
    public void askConstruct(CSteamID steamID)
    {
        ReceivePlayConstruct();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askConstruct")]
    public void ReceivePlayConstruct()
    {
        if (base.player.equipment.isEquipped)
        {
            PlayUseAnimation();
        }
    }

    public override void startPrimary()
    {
        if (base.player.equipment.isBusy || base.player.movement.getVehicle() != null)
        {
            return;
        }
        if (isServerBuildRequestInitiallyApproved)
        {
            if (base.channel.isOwner)
            {
                SendBuildStructure.Invoke(GetNetId(), ENetReliability.Reliable, pendingPlacementPosition, pendingPlacementYaw + customRotationOffset);
            }
            base.player.equipment.isBusy = true;
            PlayUseAnimation();
            if (Provider.isServer)
            {
                SendPlayConstruct.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.EnumerateClients_RemoteNotOwner());
            }
        }
        else if (hasServerReceivedBuildRequest)
        {
            base.player.equipment.dequip();
        }
    }

    public override void startSecondary()
    {
        if (!base.player.equipment.isBusy && base.channel.isOwner && equippedStructureAsset.construct != EConstruct.FLOOR_POLY && equippedStructureAsset.construct != EConstruct.ROOF_POLY)
        {
            float num = ((equippedStructureAsset.construct != 0 && equippedStructureAsset.construct != EConstruct.ROOF) ? ((equippedStructureAsset.construct != EConstruct.RAMPART && equippedStructureAsset.construct != EConstruct.WALL) ? 30f : 180f) : 90f);
            if (InputEx.GetKey(KeyCode.LeftShift))
            {
                num *= -1f;
            }
            customRotationOffset += num;
        }
    }

    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
        useAnimationDuration = base.player.animator.getAnimationLength("Use");
        if (base.channel.isOwner)
        {
            isPlacementPreviewValid = false;
            placementPreviewTransform = UseableHousingUtils.InstantiatePlacementPreview(equippedStructureAsset);
        }
    }

    public override void dequip()
    {
        if (base.channel.isOwner && placementPreviewTransform != null)
        {
            UnityEngine.Object.Destroy(placementPreviewTransform.gameObject);
        }
    }

    public override void simulate(uint simulation, bool inputSteady)
    {
        if (!isUseAnimationPlaying || !HasFinishedUseAnimation)
        {
            return;
        }
        base.player.equipment.isBusy = false;
        if (!Provider.isServer)
        {
            return;
        }
        if (!UseableHousingUtils.IsPendingPositionValid(base.player, serverPlacementPosition))
        {
            base.player.equipment.dequip();
            return;
        }
        string obstructionHint = string.Empty;
        if (UseableHousingUtils.ValidatePendingPlacement(equippedStructureAsset, ref serverPlacementPosition, serverPlacementYaw, ref obstructionHint) != 0)
        {
            base.player.equipment.dequip();
            return;
        }
        ItemStructureAsset itemStructureAsset = equippedStructureAsset;
        bool flag = false;
        if (itemStructureAsset != null)
        {
            base.player.sendStat(EPlayerStat.FOUND_BUILDABLES);
            flag = StructureManager.dropStructure(new Structure(itemStructureAsset, itemStructureAsset.health), serverPlacementPosition, 0f, serverPlacementYaw, 0f, base.channel.owner.playerID.steamID.m_SteamID, base.player.quests.groupID.m_SteamID);
        }
        if (flag)
        {
            base.player.equipment.use();
        }
        else
        {
            base.player.equipment.dequip();
        }
    }

    public override void tick()
    {
        if (isWaitingForSoundTrigger && HasReachedSoundTrigger)
        {
            isWaitingForSoundTrigger = false;
            if (Provider.isServer)
            {
                AlertTool.alert(base.transform.position, 8f);
            }
        }
        if (!base.channel.isOwner || placementPreviewTransform == null)
        {
            return;
        }
        if (!isUseAnimationPlaying)
        {
            bool flag = UpdatePendingPlacement();
            if (isPlacementPreviewValid != flag)
            {
                isPlacementPreviewValid = flag;
                HighlighterTool.help(placementPreviewTransform, isPlacementPreviewValid);
            }
        }
        float num = (Glazier.Get().ShouldGameProcessInput ? Input.GetAxis("mouse_z") : 0f);
        foundationPositionOffset = Mathf.Clamp(foundationPositionOffset + num * 0.05f, -1f, 1f);
        animatedRotationOffset = Mathf.Lerp(animatedRotationOffset, customRotationOffset, 8f * Time.deltaTime);
        placementPreviewTransform.position = pendingPlacementPosition;
        placementPreviewTransform.rotation = Quaternion.Euler(-90f, pendingPlacementYaw + animatedRotationOffset, 0f);
    }

    private void PlayUseAnimation()
    {
        useAnimationStartTime = Time.timeAsDouble;
        isUseAnimationPlaying = true;
        isWaitingForSoundTrigger = true;
        base.player.animator.play("Use", smooth: false);
    }

    private bool UpdatePendingPlacement()
    {
        if (!UseableHousingUtils.FindPlacement(equippedStructureAsset, base.player, customRotationOffset, foundationPositionOffset, out pendingPlacementPosition, out pendingPlacementYaw))
        {
            return false;
        }
        if (!UseableHousingUtils.IsPendingPositionValid(base.player, pendingPlacementPosition))
        {
            return false;
        }
        return true;
    }
}
