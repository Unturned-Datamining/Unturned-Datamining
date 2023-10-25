using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableStructure : Useable
{
    private static readonly ServerInstanceMethod<Vector3, float> SendBuildStructure = ServerInstanceMethod<Vector3, float>.Get(typeof(UseableStructure), "ReceiveBuildStructure");

    private static readonly ClientInstanceMethod SendPlayConstruct = ClientInstanceMethod.Get(typeof(UseableStructure), "ReceivePlayConstruct");

    /// <summary>
    /// Stripped-down version of structure prefab for previewing where the structure will be spawned.
    /// </summary>
    private Transform placementPreviewTransform;

    /// <summary>
    /// Whether preview object is currently highlighted positively.
    /// </summary>
    private bool isPlacementPreviewValid;

    /// <summary>
    /// Time when "Use" animation clip started playing in seconds.
    /// </summary>
    private double useAnimationStartTime;

    /// <summary>
    /// Length of "Use" animation clip in seconds.
    /// </summary>
    private float useAnimationDuration;

    /// <summary>
    /// True when animation starts playing, false after placement sound is played.
    /// </summary>
    private bool isWaitingForSoundTrigger;

    /// <summary>
    /// Whether the "Use" animation clip started playing.
    /// </summary>
    private bool isUseAnimationPlaying;

    /// <summary>
    /// If running as server, whether ReceiveBuildStructure has been called yet.
    /// </summary>
    private bool hasServerReceivedBuildRequest;

    /// <summary>
    /// Whether basic range and claim checks passed.
    /// </summary>
    private bool isServerBuildRequestInitiallyApproved;

    /// <summary>
    /// Position the item should be spawned at.
    /// </summary>
    private Vector3 pendingPlacementPosition;

    /// <summary>
    /// Rotation the item should be spawned at.
    /// </summary>
    private float pendingPlacementYaw;

    /// <summary>
    /// Interpolated toward customRotationOffset.
    /// </summary>
    private float animatedRotationOffset;

    /// <summary>
    /// Allows players to flip walls.
    /// </summary>
    private float customRotationOffset;

    /// <summary>
    /// Vertical offset using scroll wheel.
    /// </summary>
    private float foundationPositionOffset;

    private Vector3 serverPlacementPosition;

    private float serverPlacementYaw;

    public ItemStructureAsset equippedStructureAsset => base.player.equipment.asset as ItemStructureAsset;

    /// <summary>
    /// Whether enough time has passed for "Use" animation to finish.
    /// </summary>
    private bool HasFinishedUseAnimation => Time.timeAsDouble - useAnimationStartTime > (double)useAnimationDuration;

    /// <summary>
    /// Whether animation has reached the time when placement sound should play.
    /// </summary>
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
        if (base.player.equipment.IsEquipAnimationFinished)
        {
            PlayUseAnimation();
        }
    }

    public override bool startPrimary()
    {
        if (base.player.equipment.isBusy)
        {
            return false;
        }
        if (base.player.movement.getVehicle() != null)
        {
            return false;
        }
        if (Dedicator.IsDedicatedServer ? isServerBuildRequestInitiallyApproved : UpdatePendingPlacement())
        {
            if (base.channel.IsLocalPlayer)
            {
                SendBuildStructure.Invoke(GetNetId(), ENetReliability.Reliable, pendingPlacementPosition, pendingPlacementYaw + customRotationOffset);
            }
            base.player.equipment.isBusy = true;
            PlayUseAnimation();
            if (Provider.isServer)
            {
                SendPlayConstruct.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GatherRemoteClientConnectionsExcludingOwner());
            }
        }
        else if (Dedicator.IsDedicatedServer && hasServerReceivedBuildRequest)
        {
            base.player.equipment.dequip();
        }
        return true;
    }

    public override bool startSecondary()
    {
        if (base.player.equipment.isBusy)
        {
            return false;
        }
        if (base.channel.IsLocalPlayer)
        {
            if (equippedStructureAsset.construct == EConstruct.FLOOR_POLY || equippedStructureAsset.construct == EConstruct.ROOF_POLY)
            {
                return false;
            }
            float num = ((equippedStructureAsset.construct != 0 && equippedStructureAsset.construct != EConstruct.ROOF) ? ((equippedStructureAsset.construct != EConstruct.RAMPART && equippedStructureAsset.construct != EConstruct.WALL) ? 30f : 180f) : 90f);
            if (InputEx.GetKey(KeyCode.LeftShift))
            {
                num *= -1f;
            }
            customRotationOffset += num;
        }
        return true;
    }

    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
        useAnimationDuration = base.player.animator.GetAnimationLength("Use");
        if (base.channel.IsLocalPlayer)
        {
            isPlacementPreviewValid = false;
            placementPreviewTransform = UseableHousingUtils.InstantiatePlacementPreview(equippedStructureAsset);
        }
    }

    public override void dequip()
    {
        if (base.channel.IsLocalPlayer && placementPreviewTransform != null)
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
            if (!Dedicator.IsDedicatedServer)
            {
                base.player.playSound(equippedStructureAsset.use);
            }
            if (Provider.isServer)
            {
                AlertTool.alert(base.transform.position, 8f);
            }
        }
        if (!base.channel.IsLocalPlayer || placementPreviewTransform == null)
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
