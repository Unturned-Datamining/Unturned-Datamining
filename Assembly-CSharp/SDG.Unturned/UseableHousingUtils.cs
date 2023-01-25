using System;
using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

internal static class UseableHousingUtils
{
    public const float FOUNDATION_MOUSE_SCROLL_MULTIPLIER = 0.05f;

    public const float FOUNDATION_MIN_OFFSET = -1f;

    public const float FOUNDATION_MAX_OFFSET = 1f;

    public static Transform InstantiatePlacementPreview(ItemStructureAsset asset)
    {
        Transform transform = null;
        GameObject gameObject = asset.placementPreviewRef.loadAsset();
        if (gameObject != null)
        {
            transform = UnityEngine.Object.Instantiate(gameObject).transform;
        }
        if (transform == null)
        {
            transform = StructureTool.getStructure(asset.id, 0);
        }
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        Collider[] componentsInChildren = transform.GetComponentsInChildren<Collider>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            UnityEngine.Object.Destroy(componentsInChildren[i]);
        }
        HighlighterTool.help(transform, isValid: false);
        if (transform.Find("Clip") != null)
        {
            UnityEngine.Object.Destroy(transform.Find("Clip").gameObject);
        }
        if (transform.Find("Nav") != null)
        {
            UnityEngine.Object.Destroy(transform.Find("Nav").gameObject);
        }
        if (transform.Find("Cutter") != null)
        {
            UnityEngine.Object.Destroy(transform.Find("Cutter").gameObject);
        }
        if (transform.Find("Block") != null)
        {
            UnityEngine.Object.Destroy(transform.Find("Block").gameObject);
        }
        return transform;
    }

    public static EHousingPlacementResult ValidatePendingPlacement(ItemStructureAsset asset, ref Vector3 position, float yaw, ref string obstructionHint)
    {
        try
        {
            switch (asset.construct)
            {
            case EConstruct.FLOOR:
                return StructureManager.housingConnections.ValidateSquareFloorPlacement(asset.terrainTestHeight, ref position, yaw, ref obstructionHint);
            case EConstruct.WALL:
                return StructureManager.housingConnections.ValidateWallPlacement(ref position, 2.125f, asset.requiresPillars, requiresFullHeightPillars: true, ref obstructionHint);
            case EConstruct.RAMPART:
                return StructureManager.housingConnections.ValidateWallPlacement(ref position, 0.9f, asset.requiresPillars, requiresFullHeightPillars: false, ref obstructionHint);
            case EConstruct.ROOF:
                return StructureManager.housingConnections.ValidateSquareRoofPlacement(ref position, yaw, ref obstructionHint);
            case EConstruct.PILLAR:
                return StructureManager.housingConnections.ValidatePillarPlacement(ref position, 2.125f, ref obstructionHint);
            case EConstruct.POST:
                return StructureManager.housingConnections.ValidatePillarPlacement(ref position, 0.9f, ref obstructionHint);
            case EConstruct.FLOOR_POLY:
                return StructureManager.housingConnections.ValidateTriangleFloorPlacement(asset.terrainTestHeight, ref position, yaw, ref obstructionHint);
            case EConstruct.ROOF_POLY:
                return StructureManager.housingConnections.ValidateTriangleRoofPlacement(ref position, yaw, ref obstructionHint);
            default:
                UnturnedLog.error("Unhandled housing type: " + asset.construct);
                break;
            }
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception while validating housing placement:");
        }
        return EHousingPlacementResult.Success;
    }

    public static bool FindPlacement(ItemStructureAsset asset, Player player, float rotationOffset, float foundationOffset, out Vector3 pendingPlacementPosition, out float pendingPlacementYaw)
    {
        SteamChannel channel = player.channel;
        pendingPlacementPosition = default(Vector3);
        pendingPlacementYaw = player.look.yaw;
        EHousingPlacementResult eHousingPlacementResult = EHousingPlacementResult.Success;
        string obstructionHint = null;
        Ray ray = new Ray(player.look.aim.position, player.look.aim.forward);
        if (asset.construct == EConstruct.FLOOR || asset.construct == EConstruct.FLOOR_POLY)
        {
            if (!StructureManager.housingConnections.FindEmptyFloorSlot(ray, isRoof: false, out pendingPlacementPosition, out pendingPlacementYaw))
            {
                if (!Physics.SphereCast(ray, 0.1f, out var hitInfo, asset.range, RayMasks.STRUCTURE_INTERACT))
                {
                    pendingPlacementPosition = Vector3.zero;
                    return false;
                }
                pendingPlacementPosition = hitInfo.point;
                pendingPlacementYaw = player.look.yaw;
                if (asset.construct == EConstruct.FLOOR_POLY)
                {
                    pendingPlacementPosition += Quaternion.Euler(0f, pendingPlacementYaw, 0f) * new Vector3(0f, 0f, 1.2679492f);
                }
                pendingPlacementPosition += new Vector3(0f, foundationOffset, 0f);
                if (!StructureManager.housingConnections.DoesHitCountAsTerrain(hitInfo))
                {
                    eHousingPlacementResult = EHousingPlacementResult.MissingGround;
                }
            }
        }
        else if (asset.construct == EConstruct.WALL || asset.construct == EConstruct.RAMPART)
        {
            if (StructureManager.housingConnections.FindEmptyWallSlot(ray, out pendingPlacementPosition, out pendingPlacementYaw))
            {
                if (asset.construct == EConstruct.RAMPART)
                {
                    pendingPlacementPosition += Vector3.down * 1.225f;
                }
            }
            else
            {
                eHousingPlacementResult = EHousingPlacementResult.MissingSlot;
            }
        }
        else if (asset.construct == EConstruct.ROOF || asset.construct == EConstruct.ROOF_POLY)
        {
            if (!StructureManager.housingConnections.FindEmptyFloorSlot(ray, isRoof: true, out pendingPlacementPosition, out pendingPlacementYaw))
            {
                eHousingPlacementResult = EHousingPlacementResult.MissingSlot;
            }
        }
        else if (asset.construct == EConstruct.PILLAR || asset.construct == EConstruct.POST)
        {
            if (StructureManager.housingConnections.FindEmptyPillarSlot(ray, out pendingPlacementPosition, out pendingPlacementYaw))
            {
                if (asset.construct == EConstruct.POST)
                {
                    pendingPlacementPosition += Vector3.down * 1.225f;
                }
            }
            else
            {
                eHousingPlacementResult = EHousingPlacementResult.MissingSlot;
            }
        }
        if (eHousingPlacementResult == EHousingPlacementResult.Success)
        {
            eHousingPlacementResult = ValidatePendingPlacement(asset, ref pendingPlacementPosition, pendingPlacementYaw + rotationOffset, ref obstructionHint);
        }
        if (channel.isOwner)
        {
            switch (eHousingPlacementResult)
            {
            case EHousingPlacementResult.MissingSlot:
                switch (asset.construct)
                {
                case EConstruct.WALL:
                case EConstruct.RAMPART:
                    PlayerUI.hint(null, EPlayerMessage.WALL);
                    break;
                case EConstruct.ROOF:
                case EConstruct.ROOF_POLY:
                    PlayerUI.hint(null, EPlayerMessage.ROOF);
                    break;
                case EConstruct.PILLAR:
                case EConstruct.POST:
                    PlayerUI.hint(null, EPlayerMessage.CORNER);
                    break;
                }
                break;
            case EHousingPlacementResult.Obstructed:
                if (string.IsNullOrEmpty(obstructionHint))
                {
                    PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                }
                else
                {
                    PlayerUI.hint(null, EPlayerMessage.PLACEMENT_OBSTRUCTED_BY, obstructionHint, Color.white);
                }
                break;
            case EHousingPlacementResult.MissingPillar:
                switch (asset.construct)
                {
                case EConstruct.WALL:
                case EConstruct.ROOF:
                case EConstruct.ROOF_POLY:
                    PlayerUI.hint(null, EPlayerMessage.PILLAR);
                    break;
                case EConstruct.RAMPART:
                    PlayerUI.hint(null, EPlayerMessage.POST);
                    break;
                }
                break;
            case EHousingPlacementResult.MissingGround:
                PlayerUI.hint(null, EPlayerMessage.GROUND);
                break;
            }
        }
        return eHousingPlacementResult == EHousingPlacementResult.Success;
    }

    public static bool IsPendingPositionValid(Player player, Vector3 pendingPlacementPosition)
    {
        SteamChannel channel = player.channel;
        if (player.movement.isSafe && player.movement.isSafeInfo.noBuildables)
        {
            if (channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.SAFEZONE);
            }
            return false;
        }
        if (!Level.isPointWithinValidHeight(pendingPlacementPosition.y))
        {
            PlayerUI.hint(null, EPlayerMessage.BOUNDS);
            return false;
        }
        if (!ClaimManager.checkCanBuild(pendingPlacementPosition, channel.owner.playerID.steamID, player.quests.groupID, isClaim: false))
        {
            if (channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.CLAIM);
            }
            return false;
        }
        if (VolumeManager<PlayerClipVolume, PlayerClipVolumeManager>.Get().IsPositionInsideAnyVolume(pendingPlacementPosition))
        {
            if (channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.BOUNDS);
            }
            return false;
        }
        if (!LevelPlayers.checkCanBuild(pendingPlacementPosition))
        {
            if (channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.SPAWN);
            }
            return false;
        }
        return true;
    }
}
