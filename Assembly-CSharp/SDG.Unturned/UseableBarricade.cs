using System;
using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.Framework.Water;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableBarricade : Useable
{
    private static List<Collider> colliders = new List<Collider>();

    private static Collider[] checkColliders = new Collider[1];

    private Transform parent;

    private Transform help;

    private Transform guide;

    private Transform arrow;

    private InteractableVehicle parentVehicle;

    private bool boundsUse;

    private bool boundsDoubleDoor;

    private Vector3 boundsCenter;

    private Vector3 boundsExtents;

    private Vector3 boundsOverlap;

    private Quaternion boundsRotation;

    private float startedUse;

    private float useTime;

    private bool inputWantsToRotate;

    private bool isBuilding;

    private bool isUsing;

    private bool isValid;

    private bool wasAsked;

    private int pendingBuildHandle = -1;

    private RaycastHit hit;

    private Vector3 point;

    private float angle_x;

    private float angle_y;

    private float angle_z;

    private float rotate_x;

    private float rotate_y;

    private float rotate_z;

    private float input_x;

    private float input_y;

    private float input_z;

    private bool isPower;

    private Vector3 powerPoint;

    private List<InteractableClaim> claimsInRadius;

    private List<InteractableGenerator> generatorsInRadius;

    private List<InteractableSafezone> safezonesInRadius;

    private List<InteractableOxygenator> oxygenatorsInRadius;

    private static readonly ServerInstanceMethod<Vector3, float, float, float, NetId> SendBarricadeVehicle = ServerInstanceMethod<Vector3, float, float, float, NetId>.Get(typeof(UseableBarricade), "ReceiveBarricadeVehicle");

    private static readonly ServerInstanceMethod<Vector3, float, float, float> SendBarricadeNone = ServerInstanceMethod<Vector3, float, float, float>.Get(typeof(UseableBarricade), "ReceiveBarricadeNone");

    private static readonly ClientInstanceMethod SendPlayBuild = ClientInstanceMethod.Get(typeof(UseableBarricade), "ReceivePlayBuild");

    private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

    private bool isBuildable => Time.realtimeSinceStartup - startedUse > useTime * 0.8f;

    public ItemBarricadeAsset equippedBarricadeAsset => base.player.equipment.asset as ItemBarricadeAsset;

    private bool allowRotationInputOnAllAxes
    {
        get
        {
            if (equippedBarricadeAsset.build != EBuild.FREEFORM)
            {
                return equippedBarricadeAsset.build == EBuild.SENTRY_FREEFORM;
            }
            return true;
        }
    }

    private bool serverAllowAnyRotation
    {
        get
        {
            if (!allowRotationInputOnAllAxes && equippedBarricadeAsset.build != EBuild.CHARGE && equippedBarricadeAsset.build != EBuild.CLOCK)
            {
                return equippedBarricadeAsset.build == EBuild.NOTE;
            }
            return true;
        }
    }

    private bool useTrapRestrictions => equippedBarricadeAsset.type == EItemType.TRAP;

    private bool allowedToPlaceOnVehicle
    {
        get
        {
            if (!useTrapRestrictions)
            {
                return Provider.modeConfigData.Barricades.Allow_Item_Placement_On_Vehicle;
            }
            return Provider.modeConfigData.Barricades.Allow_Trap_Placement_On_Vehicle;
        }
    }

    private float maxDistanceFromHull
    {
        get
        {
            if (!useTrapRestrictions)
            {
                return Provider.modeConfigData.Barricades.Max_Item_Distance_From_Hull;
            }
            return Provider.modeConfigData.Barricades.Max_Trap_Distance_From_Hull;
        }
    }

    private bool useMaxDistanceFromHull => maxDistanceFromHull > -0.5f;

    private float sqrMaxDistanceFromHull => MathfEx.Square(maxDistanceFromHull);

    private bool isHighlightRecursive
    {
        get
        {
            if (equippedBarricadeAsset.build != EBuild.SENTRY)
            {
                return equippedBarricadeAsset.build == EBuild.SENTRY_FREEFORM;
            }
            return true;
        }
    }

    [Obsolete]
    public void askBarricadeVehicle(CSteamID steamID, Vector3 newPoint, float newAngle_X, float newAngle_Y, float newAngle_Z, ushort plant)
    {
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10)]
    public void ReceiveBarricadeVehicle(in ServerInvocationContext context, Vector3 newPoint, float newAngle_X, float newAngle_Y, float newAngle_Z, NetId regionNetId)
    {
        if (wasAsked)
        {
            return;
        }
        wasAsked = true;
        if (!allowedToPlaceOnVehicle)
        {
            return;
        }
        VehicleBarricadeRegion vehicleBarricadeRegion = NetIdRegistry.Get<VehicleBarricadeRegion>(regionNetId);
        if (vehicleBarricadeRegion == null)
        {
            return;
        }
        InteractableVehicle vehicle = vehicleBarricadeRegion.vehicle;
        if (vehicle == null)
        {
            return;
        }
        if (useMaxDistanceFromHull)
        {
            Vector3 position = vehicleBarricadeRegion.parent.TransformPoint(newPoint);
            if (vehicle.getSqrDistanceFromHull(position) > sqrMaxDistanceFromHull)
            {
                return;
            }
        }
        parent = vehicleBarricadeRegion.parent;
        parentVehicle = vehicle;
        point = newPoint;
        if (serverAllowAnyRotation)
        {
            angle_x = newAngle_X;
            angle_z = newAngle_Z;
        }
        else
        {
            angle_x = 0f;
            angle_z = 0f;
        }
        angle_y = newAngle_Y;
        rotate_x = 0f;
        rotate_y = 0f;
        rotate_z = 0f;
        isValid = checkClaims();
    }

    [Obsolete]
    public void askBarricadeNone(CSteamID steamID, Vector3 newPoint, float newAngle_X, float newAngle_Y, float newAngle_Z)
    {
        ReceiveBarricadeNone(newPoint, newAngle_X, newAngle_Y, newAngle_Z);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_OWNER, ratelimitHz = 10, legacyName = "askBarricadeNone")]
    public void ReceiveBarricadeNone(Vector3 newPoint, float newAngle_X, float newAngle_Y, float newAngle_Z)
    {
        if (wasAsked)
        {
            return;
        }
        wasAsked = true;
        if ((newPoint - base.player.look.aim.position).sqrMagnitude < 256f)
        {
            parent = null;
            parentVehicle = null;
            point = newPoint;
            if (serverAllowAnyRotation)
            {
                angle_x = newAngle_X;
                angle_z = newAngle_Z;
            }
            else
            {
                angle_x = 0f;
                angle_z = 0f;
            }
            angle_y = newAngle_Y;
            rotate_x = 0f;
            rotate_y = 0f;
            rotate_z = 0f;
            isValid = checkClaims();
            if (isValid)
            {
                pendingBuildHandle = BuildRequestManager.registerPendingBuild(point);
            }
        }
    }

    private bool check()
    {
        if (!checkSpace())
        {
            return false;
        }
        if (equippedBarricadeAsset.build == EBuild.VEHICLE)
        {
            parentVehicle = null;
            parent = null;
        }
        else
        {
            parentVehicle = DamageTool.getVehicle(hit.transform);
            parent = ((parentVehicle != null) ? hit.transform.root : null);
        }
        if (!checkClaims())
        {
            return false;
        }
        return true;
    }

    private Vector3 getPointInWorldSpace()
    {
        if (parent == null || (base.channel.isOwner && !wasAsked))
        {
            return point;
        }
        return parent.TransformPoint(point);
    }

    private bool checkClaims()
    {
        if (base.player.movement.isSafe && base.player.movement.isSafeInfo.noBuildables)
        {
            if (base.channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.SAFEZONE);
            }
            return false;
        }
        Vector3 pointInWorldSpace = getPointInWorldSpace();
        if (base.channel.isOwner && parentVehicle != null)
        {
            if (!allowedToPlaceOnVehicle)
            {
                PlayerUI.hint(null, EPlayerMessage.CANNOT_BUILD_ON_VEHICLE);
                return false;
            }
            if (useMaxDistanceFromHull && parentVehicle.getSqrDistanceFromHull(pointInWorldSpace) > sqrMaxDistanceFromHull)
            {
                PlayerUI.hint(null, EPlayerMessage.TOO_FAR_FROM_HULL);
                return false;
            }
        }
        if (equippedBarricadeAsset.build == EBuild.BEACON)
        {
            if (!LevelNavigation.checkSafeFakeNav(pointInWorldSpace) || parent != null)
            {
                if (base.channel.isOwner)
                {
                    PlayerUI.hint(null, EPlayerMessage.NAV);
                }
                return false;
            }
            if (LevelNavigation.tryGetBounds(pointInWorldSpace, out var bound))
            {
                ZombieDifficultyAsset difficultyInBound = ZombieManager.getDifficultyInBound(bound);
                if (difficultyInBound != null && !difficultyInBound.Allow_Horde_Beacon)
                {
                    if (base.channel.isOwner)
                    {
                        PlayerUI.hint(null, EPlayerMessage.NOT_ALLOWED_HERE);
                    }
                    return false;
                }
            }
        }
        if (equippedBarricadeAsset.build != EBuild.CHARGE && !equippedBarricadeAsset.bypassClaim)
        {
            if (parent != null && !ClaimManager.canBuildOnVehicle(parent, base.channel.owner.playerID.steamID, base.player.quests.groupID))
            {
                if (base.channel.isOwner)
                {
                    PlayerUI.hint(null, EPlayerMessage.CLAIM);
                }
                return false;
            }
            if (!ClaimManager.checkCanBuild(pointInWorldSpace, base.channel.owner.playerID.steamID, base.player.quests.groupID, equippedBarricadeAsset.build == EBuild.CLAIM))
            {
                if (base.channel.isOwner)
                {
                    PlayerUI.hint(null, EPlayerMessage.CLAIM);
                }
                return false;
            }
        }
        if (!equippedBarricadeAsset.AllowPlacementInsideClipVolumes && VolumeManager<PlayerClipVolume, PlayerClipVolumeManager>.Get().IsPositionInsideAnyVolume(pointInWorldSpace))
        {
            if (base.channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.BOUNDS);
            }
            return false;
        }
        if ((Level.info == null || Level.info.type == ELevelType.ARENA) && equippedBarricadeAsset.build == EBuild.BED)
        {
            return false;
        }
        if (!equippedBarricadeAsset.allowPlacementOnVehicle && !Provider.modeConfigData.Gameplay.Bypass_Buildable_Mobility && parent != null && parentVehicle != null && parentVehicle.asset != null && !parentVehicle.asset.supportsMobileBuildables)
        {
            if (base.channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.MOBILE);
            }
            return false;
        }
        if (parent != null && parentVehicle != null && parentVehicle.anySeatsOccupied)
        {
            if (base.channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.BUILD_ON_OCCUPIED_VEHICLE);
            }
            return false;
        }
        if (base.player.movement.getVehicle() != null)
        {
            if (base.channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.CANNOT_BUILD_WHILE_SEATED);
            }
            return false;
        }
        if ((Level.info == null || Level.info.type != ELevelType.ARENA) && !LevelPlayers.checkCanBuild(pointInWorldSpace))
        {
            if (base.channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.SPAWN);
            }
            return false;
        }
        if (!BuildRequestManager.canBuildAt(pointInWorldSpace, pendingBuildHandle))
        {
            return false;
        }
        if (WaterUtility.isPointUnderwater(pointInWorldSpace) && (equippedBarricadeAsset.build == EBuild.CAMPFIRE || equippedBarricadeAsset.build == EBuild.TORCH))
        {
            if (base.channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.UNDERWATER);
            }
            return false;
        }
        boundsRotation = BarricadeManager.getRotation((ItemBarricadeAsset)base.player.equipment.asset, angle_x + rotate_x, angle_y + rotate_y, angle_z + rotate_z);
        if (Physics.OverlapBoxNonAlloc(mask: (!(parent != null)) ? RayMasks.BLOCK_CHAR_BUILDABLE_OVERLAP_NOT_ON_VEHICLE : RayMasks.BLOCK_CHAR_BUILDABLE_OVERLAP, center: pointInWorldSpace + boundsRotation * boundsCenter, halfExtents: boundsOverlap, results: checkColliders, orientation: boundsRotation, queryTriggerInteraction: QueryTriggerInteraction.Collide) > 0)
        {
            if (base.channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.BLOCKED);
            }
            return false;
        }
        if (equippedBarricadeAsset.build == EBuild.BED)
        {
            Vector3 end = pointInWorldSpace + new Vector3(0f, 0.1f, 0f);
            if (Physics.Linecast(base.player.transform.position + Vector3.up * (base.player.look.heightLook * 0.5f), end, out var _, RayMasks.BLOCK_BED_LOS, QueryTriggerInteraction.Ignore))
            {
                if (base.channel.isOwner)
                {
                    PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                }
                return false;
            }
        }
        if (equippedBarricadeAsset.build == EBuild.DOOR || equippedBarricadeAsset.build == EBuild.GATE || equippedBarricadeAsset.build == EBuild.SHUTTER)
        {
            Vector3 halfExtents = boundsExtents;
            halfExtents.x -= 0.25f;
            halfExtents.y -= 0.5f;
            halfExtents.z += 0.6f;
            if (Physics.OverlapBoxNonAlloc(pointInWorldSpace + boundsRotation * boundsCenter, halfExtents, checkColliders, boundsRotation, RayMasks.BLOCK_DOOR_OPENING) > 0)
            {
                if (base.channel.isOwner)
                {
                    PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                }
                return false;
            }
            bool flag = false;
            bool flag2 = false;
            if (equippedBarricadeAsset.build == EBuild.DOOR)
            {
                flag = true;
                flag2 = boundsDoubleDoor;
            }
            else if (equippedBarricadeAsset.build == EBuild.GATE)
            {
                flag = boundsDoubleDoor;
                flag2 = boundsDoubleDoor;
            }
            else if (equippedBarricadeAsset.build == EBuild.SHUTTER)
            {
                flag = true;
                flag2 = true;
            }
            if (flag && Physics.OverlapSphereNonAlloc(pointInWorldSpace + boundsRotation * new Vector3(0f - boundsExtents.x, 0f, boundsExtents.x), 0.75f, checkColliders, RayMasks.BLOCK_DOOR_OPENING) > 0)
            {
                if (base.channel.isOwner)
                {
                    PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                }
                return false;
            }
            if (flag2 && Physics.OverlapSphereNonAlloc(pointInWorldSpace + boundsRotation * new Vector3(boundsExtents.x, 0f, boundsExtents.x), 0.75f, checkColliders, RayMasks.BLOCK_DOOR_OPENING) > 0)
            {
                if (base.channel.isOwner)
                {
                    PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                }
                return false;
            }
        }
        return true;
    }

    private bool checkSpace()
    {
        angle_y = base.player.look.yaw;
        if (equippedBarricadeAsset.build == EBuild.FORTIFICATION || equippedBarricadeAsset.build == EBuild.SHUTTER || equippedBarricadeAsset.build == EBuild.GLASS)
        {
            Physics.Raycast(base.player.look.aim.position, base.player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.SLOTS_INTERACT);
            if (hit.collider != null)
            {
                Transform transform = hit.collider.transform;
                if (transform.CompareTag("Logic") && transform.name == "Slot")
                {
                    point = hit.point - hit.normal * equippedBarricadeAsset.offset;
                    angle_y = transform.rotation.eulerAngles.y;
                    if (Mathf.Abs(Vector3.Dot(transform.right, Vector3.up)) > 0.5f)
                    {
                        if (Vector3.Dot(MainCamera.instance.transform.forward, transform.forward) < 0f)
                        {
                            angle_y += 180f;
                        }
                    }
                    else if (Vector3.Dot(MainCamera.instance.transform.forward, transform.up) > 0f)
                    {
                        angle_y += 180f;
                    }
                    if ((equippedBarricadeAsset.build == EBuild.SHUTTER || equippedBarricadeAsset.build == EBuild.GLASS) && (transform.parent.CompareTag("Barricade") || transform.parent.CompareTag("Structure")))
                    {
                        point = transform.position - hit.normal * equippedBarricadeAsset.offset;
                    }
                    if (!equippedBarricadeAsset.AllowPlacementInsideClipVolumes && !Level.checkSafeIncludingClipVolumes(point))
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.BOUNDS);
                        }
                        return false;
                    }
                    if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_WINDOW) > 0)
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                        }
                        return false;
                    }
                    return true;
                }
                point = Vector3.zero;
                if (base.channel.isOwner)
                {
                    PlayerUI.hint(null, EPlayerMessage.WINDOW);
                }
                return false;
            }
            point = Vector3.zero;
            if (base.channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.WINDOW);
            }
            return false;
        }
        if (equippedBarricadeAsset.build == EBuild.BARRICADE || equippedBarricadeAsset.build == EBuild.TANK || equippedBarricadeAsset.build == EBuild.LIBRARY || equippedBarricadeAsset.build == EBuild.BARREL_RAIN || equippedBarricadeAsset.build == EBuild.VEHICLE || equippedBarricadeAsset.build == EBuild.BED || equippedBarricadeAsset.build == EBuild.STORAGE || equippedBarricadeAsset.build == EBuild.MANNEQUIN || equippedBarricadeAsset.build == EBuild.SENTRY || equippedBarricadeAsset.build == EBuild.GENERATOR || equippedBarricadeAsset.build == EBuild.SPOT || equippedBarricadeAsset.build == EBuild.CAMPFIRE || equippedBarricadeAsset.build == EBuild.OVEN || equippedBarricadeAsset.build == EBuild.CLAIM || equippedBarricadeAsset.build == EBuild.SPIKE || equippedBarricadeAsset.build == EBuild.SAFEZONE || equippedBarricadeAsset.build == EBuild.OXYGENATOR || equippedBarricadeAsset.build == EBuild.BEACON || equippedBarricadeAsset.build == EBuild.SIGN || equippedBarricadeAsset.build == EBuild.STEREO)
        {
            if (equippedBarricadeAsset.build == EBuild.VEHICLE)
            {
                Physics.Raycast(base.player.look.aim.position, base.player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.BARRICADE_INTERACT);
            }
            else
            {
                Physics.SphereCast(base.player.look.aim.position, 0.1f, base.player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.BARRICADE_INTERACT);
            }
            if (hit.transform != null)
            {
                if (hit.normal.y < 0.01f)
                {
                    if (base.channel.isOwner)
                    {
                        PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                    }
                    return false;
                }
                if ((double)hit.normal.y > 0.75)
                {
                    point = hit.point + hit.normal * equippedBarricadeAsset.offset;
                }
                else
                {
                    point = hit.point + Vector3.up * equippedBarricadeAsset.offset;
                }
                if (equippedBarricadeAsset.build == EBuild.VEHICLE && Physics.Linecast(hit.point, point, out var _, RayMasks.BLOCK_BARRICADE))
                {
                    if (base.channel.isOwner)
                    {
                        PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                    }
                    return false;
                }
                if (!equippedBarricadeAsset.AllowPlacementInsideClipVolumes && !Level.checkSafeIncludingClipVolumes(point))
                {
                    if (base.channel.isOwner)
                    {
                        PlayerUI.hint(null, EPlayerMessage.BOUNDS);
                    }
                    return false;
                }
                if (equippedBarricadeAsset.build == EBuild.BED)
                {
                    if (Physics.OverlapSphereNonAlloc(point + Vector3.up, 0.95f + equippedBarricadeAsset.offset, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                        }
                        return false;
                    }
                }
                else if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
                {
                    if (base.channel.isOwner)
                    {
                        PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                    }
                    return false;
                }
                return true;
            }
            point = Vector3.zero;
            return false;
        }
        if (equippedBarricadeAsset.build == EBuild.WIRE)
        {
            Physics.SphereCast(base.player.look.aim.position, 0.1f, base.player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.BARRICADE_INTERACT);
            if (hit.transform != null)
            {
                point = hit.point + hit.normal * equippedBarricadeAsset.offset;
                if (!equippedBarricadeAsset.AllowPlacementInsideClipVolumes && !Level.checkSafeIncludingClipVolumes(point))
                {
                    if (base.channel.isOwner)
                    {
                        PlayerUI.hint(null, EPlayerMessage.BOUNDS);
                    }
                    return false;
                }
                if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
                {
                    if (base.channel.isOwner)
                    {
                        PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                    }
                    return false;
                }
                return true;
            }
            point = Vector3.zero;
            return false;
        }
        if (equippedBarricadeAsset.build == EBuild.FARM || equippedBarricadeAsset.build == EBuild.OIL)
        {
            Physics.SphereCast(base.player.look.aim.position, 0.1f, base.player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.BARRICADE_INTERACT);
            if (hit.transform != null)
            {
                if ((double)hit.normal.y > 0.75)
                {
                    point = hit.point + hit.normal * equippedBarricadeAsset.offset;
                }
                else
                {
                    point = hit.point + Vector3.up * equippedBarricadeAsset.offset;
                }
                string materialName = PhysicsTool.GetMaterialName(hit);
                if (hit.transform.CompareTag("Ground"))
                {
                    if (equippedBarricadeAsset.build == EBuild.FARM)
                    {
                        if (!(equippedBarricadeAsset as ItemFarmAsset).ignoreSoilRestrictions && !PhysicMaterialCustomData.IsArable(materialName))
                        {
                            if (base.channel.isOwner)
                            {
                                PlayerUI.hint(null, EPlayerMessage.SOIL);
                            }
                            return false;
                        }
                    }
                    else if (!PhysicMaterialCustomData.HasOil(materialName))
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.OIL);
                        }
                        return false;
                    }
                }
                else
                {
                    if (equippedBarricadeAsset.build != EBuild.FARM)
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.OIL);
                        }
                        return false;
                    }
                    if (!(equippedBarricadeAsset as ItemFarmAsset).ignoreSoilRestrictions && !PhysicMaterialCustomData.IsArable(materialName))
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.SOIL);
                        }
                        return false;
                    }
                }
                if (!equippedBarricadeAsset.AllowPlacementInsideClipVolumes && !Level.checkSafeIncludingClipVolumes(point))
                {
                    if (base.channel.isOwner)
                    {
                        PlayerUI.hint(null, EPlayerMessage.BOUNDS);
                    }
                    return false;
                }
                if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
                {
                    if (base.channel.isOwner)
                    {
                        PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                    }
                    return false;
                }
                return true;
            }
            point = Vector3.zero;
            return false;
        }
        if (equippedBarricadeAsset.build == EBuild.DOOR)
        {
            Physics.SphereCast(base.player.look.aim.position, 0.1f, base.player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.SLOTS_INTERACT);
            if (hit.collider != null)
            {
                Transform transform2 = hit.collider.transform;
                if (transform2.name == "Door")
                {
                    point = transform2.position;
                    angle_y = transform2.rotation.eulerAngles.y;
                    if (Vector3.Dot(MainCamera.instance.transform.forward, transform2.forward) < 0f)
                    {
                        angle_y += 180f;
                    }
                    if (!equippedBarricadeAsset.AllowPlacementInsideClipVolumes && !Level.checkSafeIncludingClipVolumes(point))
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.BOUNDS);
                        }
                        return false;
                    }
                    if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_FRAME) > 0)
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                        }
                        return false;
                    }
                    return true;
                }
                point = Vector3.zero;
                if (base.channel.isOwner)
                {
                    PlayerUI.hint(null, EPlayerMessage.DOORWAY);
                }
                return false;
            }
            point = Vector3.zero;
            if (base.channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.DOORWAY);
            }
            return false;
        }
        if (equippedBarricadeAsset.build == EBuild.HATCH)
        {
            Physics.SphereCast(base.player.look.aim.position, 0.1f, base.player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.SLOTS_INTERACT);
            if (hit.transform != null)
            {
                if (hit.transform.CompareTag("Logic") && hit.transform.name == "Hatch")
                {
                    point = hit.transform.position;
                    angle_y = hit.transform.rotation.eulerAngles.y;
                    float num = Vector3.Dot(MainCamera.instance.transform.forward, hit.transform.forward);
                    float num2 = Vector3.Dot(MainCamera.instance.transform.forward, hit.transform.right);
                    float num3 = Vector3.Dot(MainCamera.instance.transform.forward, -hit.transform.forward);
                    float num4 = Vector3.Dot(MainCamera.instance.transform.forward, -hit.transform.right);
                    float num5 = num;
                    if (num2 < num5)
                    {
                        num5 = num2;
                        angle_y = hit.transform.rotation.eulerAngles.y + 90f;
                    }
                    if (num3 < num5)
                    {
                        num5 = num3;
                        angle_y = hit.transform.rotation.eulerAngles.y + 180f;
                    }
                    if (num4 < num5)
                    {
                        angle_y = hit.transform.rotation.eulerAngles.y + 270f;
                    }
                    angle_y += 180f;
                    if (!equippedBarricadeAsset.AllowPlacementInsideClipVolumes && !Level.checkSafeIncludingClipVolumes(point))
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.BOUNDS);
                        }
                        return false;
                    }
                    if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_FRAME) > 0)
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                        }
                        return false;
                    }
                    return true;
                }
                point = Vector3.zero;
                if (base.channel.isOwner)
                {
                    PlayerUI.hint(null, EPlayerMessage.TRAPDOOR);
                }
                return false;
            }
            point = Vector3.zero;
            if (base.channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.TRAPDOOR);
            }
            return false;
        }
        if (equippedBarricadeAsset.build == EBuild.GATE)
        {
            Physics.SphereCast(base.player.look.aim.position, 0.1f, base.player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.SLOTS_INTERACT);
            if (hit.transform != null)
            {
                if (hit.transform.CompareTag("Logic") && hit.transform.name == "Gate")
                {
                    point = hit.transform.position;
                    angle_y = hit.transform.rotation.eulerAngles.y;
                    if (Mathf.Abs(Vector3.Dot(hit.transform.up, Vector3.up)) > 0.5f)
                    {
                        if (Vector3.Dot(MainCamera.instance.transform.forward, hit.transform.forward) < 0f)
                        {
                            angle_y += 180f;
                        }
                    }
                    else if (Vector3.Dot(MainCamera.instance.transform.forward, hit.transform.up) > 0f)
                    {
                        angle_y += 180f;
                    }
                    if (!equippedBarricadeAsset.AllowPlacementInsideClipVolumes && !Level.checkSafeIncludingClipVolumes(point))
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.BOUNDS);
                        }
                        return false;
                    }
                    if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_FRAME) > 0)
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                        }
                        return false;
                    }
                    if (Physics.OverlapSphereNonAlloc(point + hit.transform.forward * -1.5f + hit.transform.up * -2f, 0.25f, checkColliders, RayMasks.BLOCK_FRAME) > 0)
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                        }
                        return false;
                    }
                    return true;
                }
                point = Vector3.zero;
                if (base.channel.isOwner)
                {
                    PlayerUI.hint(null, EPlayerMessage.GARAGE);
                }
                return false;
            }
            point = Vector3.zero;
            if (base.channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.GARAGE);
            }
            return false;
        }
        if (equippedBarricadeAsset.build == EBuild.LADDER)
        {
            Physics.SphereCast(base.player.look.aim.position, 0.1f, base.player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.LADDERS_INTERACT);
            if (hit.transform != null)
            {
                if (hit.transform.CompareTag("Logic") && hit.transform.name == "Climb")
                {
                    point = hit.transform.position;
                    angle_y = hit.transform.rotation.eulerAngles.y;
                    if (Physics.OverlapSphereNonAlloc(point + hit.transform.up * 0.5f, 0.1f, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                        }
                        return false;
                    }
                    if (Physics.OverlapSphereNonAlloc(point + hit.transform.up * -0.5f, 0.1f, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                        }
                        return false;
                    }
                }
                else
                {
                    if (Mathf.Abs(hit.normal.y) < 0.1f)
                    {
                        point = hit.point + hit.normal * equippedBarricadeAsset.offset;
                        angle_y = Quaternion.LookRotation(hit.normal).eulerAngles.y;
                        if (Physics.OverlapSphereNonAlloc(point + Quaternion.Euler(0f, angle_y, 0f) * Vector3.right * 0.5f, 0.1f, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
                        {
                            if (base.channel.isOwner)
                            {
                                PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                            }
                            return false;
                        }
                        if (Physics.OverlapSphereNonAlloc(point + Quaternion.Euler(0f, angle_y, 0f) * Vector3.left * 0.5f, 0.1f, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
                        {
                            if (base.channel.isOwner)
                            {
                                PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                            }
                            return false;
                        }
                    }
                    else
                    {
                        if (hit.normal.y > 0.75f)
                        {
                            point = hit.point + hit.normal * StructureManager.HEIGHT;
                        }
                        else
                        {
                            point = hit.point + Vector3.up * StructureManager.HEIGHT;
                        }
                        if (Physics.OverlapSphereNonAlloc(point, 0.5f, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
                        {
                            if (base.channel.isOwner)
                            {
                                PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                            }
                            return false;
                        }
                    }
                    if (!equippedBarricadeAsset.AllowPlacementInsideClipVolumes && !Level.checkSafeIncludingClipVolumes(point))
                    {
                        if (base.channel.isOwner)
                        {
                            PlayerUI.hint(null, EPlayerMessage.BOUNDS);
                        }
                        return false;
                    }
                }
                return true;
            }
            point = Vector3.zero;
            return false;
        }
        if (equippedBarricadeAsset.build == EBuild.TORCH || equippedBarricadeAsset.build == EBuild.STORAGE_WALL || equippedBarricadeAsset.build == EBuild.SIGN_WALL || equippedBarricadeAsset.build == EBuild.CAGE)
        {
            Physics.SphereCast(base.player.look.aim.position, 0.1f, base.player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.BARRICADE_INTERACT);
            if (hit.transform != null && Mathf.Abs(hit.normal.y) < 0.1f)
            {
                point = hit.point + hit.normal * equippedBarricadeAsset.offset;
                angle_y = Quaternion.LookRotation(hit.normal).eulerAngles.y;
                if (Physics.OverlapSphereNonAlloc(point, 0.1f, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
                {
                    if (base.channel.isOwner)
                    {
                        PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                    }
                    return false;
                }
                if (!equippedBarricadeAsset.AllowPlacementInsideClipVolumes && !Level.checkSafeIncludingClipVolumes(point))
                {
                    if (base.channel.isOwner)
                    {
                        PlayerUI.hint(null, EPlayerMessage.BOUNDS);
                    }
                    return false;
                }
                return true;
            }
            if (base.channel.isOwner)
            {
                PlayerUI.hint(null, EPlayerMessage.WALL);
            }
            point = Vector3.zero;
            return false;
        }
        if (equippedBarricadeAsset.build == EBuild.FREEFORM || equippedBarricadeAsset.build == EBuild.SENTRY_FREEFORM)
        {
            Physics.SphereCast(base.player.look.aim.position, 0.1f, base.player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.BARRICADE_INTERACT);
            if (hit.transform != null)
            {
                Quaternion quaternion = Quaternion.Euler(0f, angle_y + rotate_y, 0f);
                quaternion *= Quaternion.Euler(-90f + angle_x + rotate_x, 0f, 0f);
                quaternion *= Quaternion.Euler(0f, angle_z + rotate_z, 0f);
                point = hit.point + hit.normal * -0.125f + quaternion * Vector3.forward * equippedBarricadeAsset.offset;
                if (!equippedBarricadeAsset.AllowPlacementInsideClipVolumes && !Level.checkSafeIncludingClipVolumes(point))
                {
                    if (base.channel.isOwner)
                    {
                        PlayerUI.hint(null, EPlayerMessage.BOUNDS);
                    }
                    return false;
                }
                if (Physics.OverlapSphereNonAlloc(point, equippedBarricadeAsset.radius, checkColliders, RayMasks.BLOCK_BARRICADE) > 0)
                {
                    if (base.channel.isOwner)
                    {
                        PlayerUI.hint(null, EPlayerMessage.BLOCKED);
                    }
                    return false;
                }
                return true;
            }
            point = Vector3.zero;
            return false;
        }
        if (equippedBarricadeAsset.build == EBuild.CHARGE || equippedBarricadeAsset.build == EBuild.CLOCK || equippedBarricadeAsset.build == EBuild.NOTE)
        {
            Physics.SphereCast(base.player.look.aim.position, 0.1f, base.player.look.aim.forward, out hit, equippedBarricadeAsset.range, RayMasks.BARRICADE_INTERACT);
            if (hit.transform != null)
            {
                Vector3 eulerAngles = Quaternion.LookRotation(hit.normal).eulerAngles;
                angle_x = eulerAngles.x;
                angle_y = eulerAngles.y;
                angle_z = eulerAngles.z;
                point = hit.point + hit.normal * equippedBarricadeAsset.offset;
                return true;
            }
            point = Vector3.zero;
            return false;
        }
        point = Vector3.zero;
        return false;
    }

    private void build()
    {
        startedUse = Time.realtimeSinceStartup;
        isUsing = true;
        isBuilding = true;
        base.player.animator.play("Use", smooth: false);
    }

    [Obsolete]
    public void askBuild(CSteamID steamID)
    {
        ReceivePlayBuild();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askBuild")]
    public void ReceivePlayBuild()
    {
        if (base.player.equipment.isEquipped)
        {
            build();
        }
    }

    public override void startPrimary()
    {
        if (base.player.equipment.isBusy)
        {
            return;
        }
        if (isValid)
        {
            if (base.channel.isOwner)
            {
                if (parent != null)
                {
                    VehicleBarricadeRegion vehicleBarricadeRegion = BarricadeManager.FindVehicleRegionByTransform(parent);
                    if (vehicleBarricadeRegion != null)
                    {
                        SendBarricadeVehicle.Invoke(GetNetId(), ENetReliability.Reliable, parent.InverseTransformPoint(point), angle_x + rotate_x, angle_y + rotate_y - parent.localRotation.eulerAngles.y, angle_z + rotate_z, vehicleBarricadeRegion._netId);
                    }
                }
                else
                {
                    SendBarricadeNone.Invoke(GetNetId(), ENetReliability.Reliable, point, angle_x + rotate_x, angle_y + rotate_y, angle_z + rotate_z);
                }
            }
            base.player.equipment.isBusy = true;
            build();
            if (Provider.isServer)
            {
                SendPlayBuild.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.EnumerateClients_RemoteNotOwner());
            }
        }
        else if (wasAsked)
        {
            base.player.equipment.dequip();
        }
    }

    public override void startSecondary()
    {
        if (!base.player.equipment.isBusy && equippedBarricadeAsset.build != EBuild.GLASS && equippedBarricadeAsset.build != EBuild.CHARGE && equippedBarricadeAsset.build != EBuild.CLOCK && equippedBarricadeAsset.build != EBuild.NOTE && equippedBarricadeAsset.build != 0 && equippedBarricadeAsset.build != EBuild.DOOR && equippedBarricadeAsset.build != EBuild.GATE && equippedBarricadeAsset.build != EBuild.SHUTTER && equippedBarricadeAsset.build != EBuild.HATCH && equippedBarricadeAsset.build != EBuild.TORCH && equippedBarricadeAsset.build != EBuild.CAGE && equippedBarricadeAsset.build != EBuild.STORAGE_WALL && equippedBarricadeAsset.build != EBuild.SIGN_WALL)
        {
            base.player.look.isIgnoringInput = true;
            inputWantsToRotate = true;
        }
    }

    public override void stopSecondary()
    {
        base.player.look.isIgnoringInput = false;
        inputWantsToRotate = false;
    }

    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
        useTime = base.player.animator.getAnimationLength("Use");
        if (equippedBarricadeAsset.build == EBuild.MANNEQUIN)
        {
            boundsUse = true;
            boundsCenter = new Vector3(0f, 0f, -0.05f);
            boundsExtents = new Vector3(1.175f, 0.2f, 1.05f);
        }
        else if (equippedBarricadeAsset.barricade != null)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(equippedBarricadeAsset.barricade, Vector3.zero, Quaternion.identity);
            gameObject.name = "Helper";
            Collider collider;
            if (equippedBarricadeAsset.build == EBuild.DOOR || equippedBarricadeAsset.build == EBuild.GATE || equippedBarricadeAsset.build == EBuild.SHUTTER)
            {
                collider = gameObject.transform.Find("Placeholder").GetComponent<Collider>();
                boundsDoubleDoor = gameObject.transform.Find("Skeleton").Find("Hinge") == null;
            }
            else
            {
                collider = gameObject.GetComponentInChildren<Collider>();
            }
            if (collider != null)
            {
                boundsUse = true;
                boundsCenter = gameObject.transform.InverseTransformPoint(collider.bounds.center);
                boundsExtents = collider.bounds.extents;
            }
            UnityEngine.Object.Destroy(gameObject);
        }
        boundsOverlap = boundsExtents + new Vector3(0.5f, 0.5f, 0.5f);
        if (!base.channel.isOwner)
        {
            return;
        }
        if (help == null)
        {
            help = BarricadeTool.getBarricade(null, 0, Vector3.zero, Quaternion.identity, base.player.equipment.itemID, base.player.equipment.state);
        }
        guide = help.Find("Root");
        if (guide == null)
        {
            guide = help;
        }
        HighlighterTool.help(guide, isValid, isHighlightRecursive);
        arrow = ((GameObject)UnityEngine.Object.Instantiate(Resources.Load("Build/Arrow"))).transform;
        arrow.name = "Arrow";
        arrow.parent = help;
        arrow.localPosition = Vector3.zero;
        if (equippedBarricadeAsset.build == EBuild.DOOR || equippedBarricadeAsset.build == EBuild.GATE || equippedBarricadeAsset.build == EBuild.SHUTTER || equippedBarricadeAsset.build == EBuild.HATCH)
        {
            arrow.localRotation = Quaternion.identity;
        }
        else
        {
            arrow.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }
        Collider collider2;
        if (equippedBarricadeAsset.build == EBuild.DOOR || equippedBarricadeAsset.build == EBuild.GATE || equippedBarricadeAsset.build == EBuild.SHUTTER)
        {
            collider2 = help.Find("Placeholder").GetComponent<Collider>();
            boundsDoubleDoor = help.Find("Skeleton").Find("Hinge") == null;
        }
        else
        {
            collider2 = help.GetComponentInChildren<Collider>();
        }
        if (equippedBarricadeAsset.build == EBuild.MANNEQUIN)
        {
            boundsUse = true;
            boundsCenter = new Vector3(0f, 0f, -0.05f);
            boundsExtents = new Vector3(1.175f, 0.2f, 1.05f);
            if (collider2 != null)
            {
                UnityEngine.Object.Destroy(collider2);
            }
        }
        else if (collider2 != null)
        {
            boundsUse = true;
            boundsCenter = help.InverseTransformPoint(collider2.bounds.center);
            boundsExtents = collider2.bounds.extents;
            UnityEngine.Object.Destroy(collider2);
        }
        boundsOverlap = boundsExtents + new Vector3(0.5f, 0.5f, 0.5f);
        if (equippedBarricadeAsset.build == EBuild.GLASS)
        {
            WaterHeightTransparentSort componentInChildren = help.GetComponentInChildren<WaterHeightTransparentSort>(includeInactive: true);
            if (componentInChildren != null)
            {
                UnityEngine.Object.Destroy(componentInChildren);
            }
        }
        HighlighterTool.help(arrow, isValid);
        if (help.Find("Radius") != null)
        {
            isPower = true;
            powerPoint = Vector3.zero;
            claimsInRadius = new List<InteractableClaim>();
            generatorsInRadius = new List<InteractableGenerator>();
            safezonesInRadius = new List<InteractableSafezone>();
            oxygenatorsInRadius = new List<InteractableOxygenator>();
            if (equippedBarricadeAsset.build == EBuild.CLAIM || equippedBarricadeAsset.build == EBuild.GENERATOR || equippedBarricadeAsset.build == EBuild.SAFEZONE || equippedBarricadeAsset.build == EBuild.OXYGENATOR)
            {
                help.Find("Radius").gameObject.SetActive(value: true);
            }
        }
        Interactable component = help.GetComponent<Interactable>();
        if (component != null)
        {
            UnityEngine.Object.Destroy(component);
        }
        if (equippedBarricadeAsset.build == EBuild.SPIKE || equippedBarricadeAsset.build == EBuild.WIRE)
        {
            UnityEngine.Object.Destroy(help.Find("Trap").GetComponent<InteractableTrap>());
        }
        if (equippedBarricadeAsset.build == EBuild.BEACON)
        {
            UnityEngine.Object.Destroy(help.GetComponent<InteractableBeacon>());
        }
        if (equippedBarricadeAsset.build == EBuild.DOOR || equippedBarricadeAsset.build == EBuild.GATE || equippedBarricadeAsset.build == EBuild.SHUTTER || equippedBarricadeAsset.build == EBuild.HATCH)
        {
            if (help.Find("Placeholder") != null)
            {
                UnityEngine.Object.Destroy(help.Find("Placeholder").gameObject);
            }
            List<InteractableDoorHinge> list = new List<InteractableDoorHinge>();
            help.GetComponentsInChildren(list);
            for (int i = 0; i < list.Count; i++)
            {
                InteractableDoorHinge interactableDoorHinge = list[i];
                if (interactableDoorHinge.transform.Find("Clip") != null)
                {
                    UnityEngine.Object.Destroy(interactableDoorHinge.transform.Find("Clip").gameObject);
                }
                if (interactableDoorHinge.transform.Find("Nav") != null)
                {
                    UnityEngine.Object.Destroy(interactableDoorHinge.transform.Find("Nav").gameObject);
                }
                UnityEngine.Object.Destroy(interactableDoorHinge.transform.GetComponent<Collider>());
                UnityEngine.Object.Destroy(interactableDoorHinge);
            }
        }
        else
        {
            if (help.Find("Clip") != null)
            {
                UnityEngine.Object.Destroy(help.Find("Clip").gameObject);
            }
            if (help.Find("Nav") != null)
            {
                UnityEngine.Object.Destroy(help.Find("Nav").gameObject);
            }
            if (help.Find("Ladder") != null)
            {
                UnityEngine.Object.Destroy(help.Find("Ladder").gameObject);
            }
            if (help.Find("Block") != null)
            {
                UnityEngine.Object.Destroy(help.Find("Block").gameObject);
            }
        }
        for (int j = 0; j < 2; j++)
        {
            if (!(help.Find("Climb") != null))
            {
                break;
            }
            UnityEngine.Object.Destroy(help.Find("Climb").gameObject);
        }
        help.GetComponentsInChildren(includeInactive: true, colliders);
        for (int k = 0; k < colliders.Count; k++)
        {
            UnityEngine.Object.Destroy(colliders[k]);
        }
    }

    public override void dequip()
    {
        base.player.look.isIgnoringInput = false;
        inputWantsToRotate = false;
        if (base.channel.isOwner)
        {
            if (help != null)
            {
                UnityEngine.Object.Destroy(help.gameObject);
            }
            if (isPower)
            {
                for (int i = 0; i < claimsInRadius.Count; i++)
                {
                    if (!(claimsInRadius[i] == null))
                    {
                        claimsInRadius[i].transform.Find("Radius")?.gameObject.SetActive(value: false);
                    }
                }
                claimsInRadius.Clear();
                for (int j = 0; j < generatorsInRadius.Count; j++)
                {
                    if (!(generatorsInRadius[j] == null))
                    {
                        generatorsInRadius[j].transform.Find("Radius")?.gameObject.SetActive(value: false);
                    }
                }
                generatorsInRadius.Clear();
                for (int k = 0; k < safezonesInRadius.Count; k++)
                {
                    if (!(safezonesInRadius[k] == null))
                    {
                        safezonesInRadius[k].transform.Find("Radius")?.gameObject.SetActive(value: false);
                    }
                }
                safezonesInRadius.Clear();
                for (int l = 0; l < oxygenatorsInRadius.Count; l++)
                {
                    if (!(oxygenatorsInRadius[l] == null))
                    {
                        oxygenatorsInRadius[l].transform.Find("Radius")?.gameObject.SetActive(value: false);
                    }
                }
                oxygenatorsInRadius.Clear();
            }
        }
        BuildRequestManager.finishPendingBuild(ref pendingBuildHandle);
    }

    public override void simulate(uint simulation, bool inputSteady)
    {
        if (!isUsing || !isUseable)
        {
            return;
        }
        base.player.equipment.isBusy = false;
        if (!Provider.isServer)
        {
            return;
        }
        int mask = ((!(parentVehicle != null)) ? RayMasks.BLOCK_CHAR_BUILDABLE_OVERLAP_NOT_ON_VEHICLE : RayMasks.BLOCK_CHAR_BUILDABLE_OVERLAP);
        if (boundsUse && Physics.OverlapBoxNonAlloc(getPointInWorldSpace() + boundsRotation * boundsCenter, boundsOverlap, checkColliders, boundsRotation, mask, QueryTriggerInteraction.Collide) > 0)
        {
            base.player.equipment.dequip();
            return;
        }
        if (parentVehicle != null && parentVehicle.isGoingToRespawn)
        {
            base.player.equipment.dequip();
            return;
        }
        if (parentVehicle != null && parentVehicle.isHooked)
        {
            base.player.equipment.dequip();
            return;
        }
        if (!checkClaims())
        {
            base.player.equipment.dequip();
            return;
        }
        ItemBarricadeAsset itemBarricadeAsset = (ItemBarricadeAsset)base.player.equipment.asset;
        bool flag = false;
        if (itemBarricadeAsset != null)
        {
            base.player.sendStat(EPlayerStat.FOUND_BUILDABLES);
            if (itemBarricadeAsset.build == EBuild.VEHICLE)
            {
                VehicleAsset vehicleAsset = itemBarricadeAsset.FindVehicleAsset();
                if (vehicleAsset != null)
                {
                    flag = VehicleManager.spawnVehicleV2(vehicleAsset.id, point, Quaternion.Euler(angle_x + rotate_x, angle_y + rotate_y, angle_z + rotate_z)) != null;
                }
            }
            else
            {
                Barricade barricade = new Barricade(itemBarricadeAsset);
                if (itemBarricadeAsset.build == EBuild.DOOR || itemBarricadeAsset.build == EBuild.GATE || itemBarricadeAsset.build == EBuild.SHUTTER || itemBarricadeAsset.build == EBuild.SIGN || itemBarricadeAsset.build == EBuild.SIGN_WALL || itemBarricadeAsset.build == EBuild.NOTE || itemBarricadeAsset.build == EBuild.HATCH)
                {
                    BitConverter.GetBytes(base.channel.owner.playerID.steamID.m_SteamID).CopyTo(barricade.state, 0);
                    BitConverter.GetBytes(base.player.quests.groupID.m_SteamID).CopyTo(barricade.state, 8);
                }
                else if (itemBarricadeAsset.build == EBuild.BED)
                {
                    BitConverter.GetBytes(CSteamID.Nil.m_SteamID).CopyTo(barricade.state, 0);
                }
                else if (itemBarricadeAsset.build == EBuild.STORAGE || itemBarricadeAsset.build == EBuild.STORAGE_WALL || itemBarricadeAsset.build == EBuild.MANNEQUIN || itemBarricadeAsset.build == EBuild.SENTRY || itemBarricadeAsset.build == EBuild.SENTRY_FREEFORM || itemBarricadeAsset.build == EBuild.LIBRARY || itemBarricadeAsset.build == EBuild.MANNEQUIN)
                {
                    BitConverter.GetBytes(base.channel.owner.playerID.steamID.m_SteamID).CopyTo(barricade.state, 0);
                    BitConverter.GetBytes(base.player.quests.groupID.m_SteamID).CopyTo(barricade.state, 8);
                }
                else if (itemBarricadeAsset.build == EBuild.FARM)
                {
                    BitConverter.GetBytes(Provider.time - (uint)((float)((ItemFarmAsset)base.player.equipment.asset).growth * (base.player.skills.mastery(2, 5) * 0.25f))).CopyTo(barricade.state, 0);
                }
                else if (itemBarricadeAsset.build == EBuild.TORCH || itemBarricadeAsset.build == EBuild.CAMPFIRE || itemBarricadeAsset.build == EBuild.OVEN || itemBarricadeAsset.build == EBuild.SPOT || itemBarricadeAsset.build == EBuild.SAFEZONE || itemBarricadeAsset.build == EBuild.OXYGENATOR || itemBarricadeAsset.build == EBuild.CAGE)
                {
                    barricade.state[0] = 1;
                }
                else if (itemBarricadeAsset.build == EBuild.GENERATOR)
                {
                    barricade.state[0] = 1;
                }
                else if (itemBarricadeAsset.build == EBuild.STEREO)
                {
                    barricade.state[16] = 100;
                }
                flag = BarricadeManager.dropBarricade(barricade, parent, point, angle_x + rotate_x, angle_y + rotate_y, angle_z + rotate_z, base.channel.owner.playerID.steamID.m_SteamID, base.player.quests.groupID.m_SteamID) != null;
            }
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

    private void processRotationInput()
    {
        if (allowRotationInputOnAllAxes)
        {
            if (ControlsSettings.invert)
            {
                input_x += ControlsSettings.mouseAimSensitivity * 2f * Input.GetAxis("mouse_y");
            }
            else
            {
                input_x -= ControlsSettings.mouseAimSensitivity * 2f * Input.GetAxis("mouse_y");
            }
        }
        input_y += ControlsSettings.mouseAimSensitivity * 2f * Input.GetAxis("mouse_x");
        if (allowRotationInputOnAllAxes)
        {
            input_z += ControlsSettings.mouseAimSensitivity * 30f * Input.GetAxis("mouse_z");
        }
        if (InputEx.GetKey(ControlsSettings.snap))
        {
            rotate_x = (float)(int)(input_x / 15f) * 15f;
            rotate_y = (float)(int)(input_y / 15f) * 15f;
            rotate_z = (float)(int)(input_z / 15f) * 15f;
        }
        else
        {
            rotate_x = input_x;
            rotate_y = input_y;
            rotate_z = input_z;
        }
    }

    public override void tick()
    {
        if (isBuilding && isBuildable)
        {
            isBuilding = false;
            if (Provider.isServer)
            {
                AlertTool.alert(base.transform.position, 8f);
            }
        }
        if (!base.channel.isOwner || help == null || isUsing)
        {
            return;
        }
        if (inputWantsToRotate)
        {
            processRotationInput();
        }
        if (check())
        {
            if (!isValid)
            {
                isValid = true;
                HighlighterTool.help(guide, isValid, isHighlightRecursive);
                if (arrow != null)
                {
                    HighlighterTool.help(arrow, isValid);
                }
            }
        }
        else if (isValid)
        {
            isValid = false;
            HighlighterTool.help(guide, isValid, isHighlightRecursive);
            if (arrow != null)
            {
                HighlighterTool.help(arrow, isValid);
            }
        }
        bool flag = help.parent != parent;
        if (flag)
        {
            help.parent = parent;
            help.gameObject.SetActive(value: false);
            help.gameObject.SetActive(value: true);
        }
        if (parent != null)
        {
            help.localPosition = parent.InverseTransformPoint(point);
            help.localRotation = Quaternion.Euler(0f, angle_y + rotate_y - parent.localRotation.eulerAngles.y, 0f);
            help.localRotation *= Quaternion.Euler((float)((equippedBarricadeAsset.build != EBuild.DOOR && equippedBarricadeAsset.build != EBuild.GATE && equippedBarricadeAsset.build != EBuild.SHUTTER && equippedBarricadeAsset.build != EBuild.HATCH) ? (-90) : 0) + angle_x + rotate_x, 0f, 0f);
            help.localRotation *= Quaternion.Euler(0f, angle_z + rotate_z, 0f);
        }
        else
        {
            help.position = point;
            help.rotation = Quaternion.Euler(0f, angle_y + rotate_y, 0f);
            help.rotation *= Quaternion.Euler((float)((equippedBarricadeAsset.build != EBuild.DOOR && equippedBarricadeAsset.build != EBuild.GATE && equippedBarricadeAsset.build != EBuild.SHUTTER && equippedBarricadeAsset.build != EBuild.HATCH) ? (-90) : 0) + angle_x + rotate_x, 0f, 0f);
            help.rotation *= Quaternion.Euler(0f, angle_z + rotate_z, 0f);
        }
        if (!isPower)
        {
            return;
        }
        bool flag2 = flag;
        if ((base.transform.position - powerPoint).sqrMagnitude > 1f)
        {
            powerPoint = base.transform.position;
            flag2 = true;
        }
        if (!flag2)
        {
            return;
        }
        for (int i = 0; i < claimsInRadius.Count; i++)
        {
            if (!(claimsInRadius[i] == null))
            {
                claimsInRadius[i].transform.Find("Radius")?.gameObject.SetActive(value: false);
            }
        }
        claimsInRadius.Clear();
        for (int j = 0; j < generatorsInRadius.Count; j++)
        {
            if (!(generatorsInRadius[j] == null))
            {
                generatorsInRadius[j].transform.Find("Radius")?.gameObject.SetActive(value: false);
            }
        }
        generatorsInRadius.Clear();
        for (int k = 0; k < safezonesInRadius.Count; k++)
        {
            if (!(safezonesInRadius[k] == null))
            {
                safezonesInRadius[k].transform.Find("Radius")?.gameObject.SetActive(value: false);
            }
        }
        safezonesInRadius.Clear();
        for (int l = 0; l < oxygenatorsInRadius.Count; l++)
        {
            if (!(oxygenatorsInRadius[l] == null))
            {
                oxygenatorsInRadius[l].transform.Find("Radius")?.gameObject.SetActive(value: false);
            }
        }
        oxygenatorsInRadius.Clear();
        BarricadeManager.tryGetPlant(parent, out var _, out var _, out var plant, out var _);
        if (equippedBarricadeAsset.build == EBuild.CLAIM)
        {
            PowerTool.checkInteractables(powerPoint, 64f, plant, claimsInRadius);
            for (int m = 0; m < claimsInRadius.Count; m++)
            {
                if (!(claimsInRadius[m] == null))
                {
                    claimsInRadius[m].transform.Find("Radius")?.gameObject.SetActive(value: true);
                }
            }
        }
        else
        {
            PowerTool.checkInteractables(powerPoint, 64f, plant, generatorsInRadius);
            for (int n = 0; n < generatorsInRadius.Count; n++)
            {
                if (!(generatorsInRadius[n] == null))
                {
                    generatorsInRadius[n].transform.Find("Radius")?.gameObject.SetActive(value: true);
                }
            }
        }
        if (equippedBarricadeAsset.build == EBuild.SAFEZONE)
        {
            PowerTool.checkInteractables(powerPoint, 64f, plant, safezonesInRadius);
            for (int num = 0; num < safezonesInRadius.Count; num++)
            {
                if (!(safezonesInRadius[num] == null))
                {
                    safezonesInRadius[num].transform.Find("Radius")?.gameObject.SetActive(value: true);
                }
            }
        }
        if (equippedBarricadeAsset.build != EBuild.OXYGENATOR)
        {
            return;
        }
        PowerTool.checkInteractables(powerPoint, 64f, plant, oxygenatorsInRadius);
        for (int num2 = 0; num2 < oxygenatorsInRadius.Count; num2++)
        {
            if (!(oxygenatorsInRadius[num2] == null))
            {
                oxygenatorsInRadius[num2].transform.Find("Radius")?.gameObject.SetActive(value: true);
            }
        }
    }

    protected void OnDestroy()
    {
        BuildRequestManager.finishPendingBuild(ref pendingBuildHandle);
    }
}
