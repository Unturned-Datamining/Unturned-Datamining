using System;
using System.Collections.Generic;
using SDG.Framework.Foliage;
using UnityEngine;

namespace SDG.Unturned;

public class StructureDrop
{
    public delegate void SalvageRequestHandler(StructureDrop structure, SteamPlayer instigatorClient, ref bool shouldAllow);

    private Transform _model;

    internal static readonly ClientInstanceMethod<byte> SendHealth = ClientInstanceMethod<byte>.Get(typeof(StructureDrop), "ReceiveHealth");

    internal static readonly ClientInstanceMethod<byte, byte, Vector3, Quaternion> SendTransform = ClientInstanceMethod<byte, byte, Vector3, Quaternion>.Get(typeof(StructureDrop), "ReceiveTransform");

    internal static readonly ServerInstanceMethod<Vector3, Quaternion> SendTransformRequest = ServerInstanceMethod<Vector3, Quaternion>.Get(typeof(StructureDrop), "ReceiveTransformRequest");

    private static List<Interactable2SalvageStructure> workingSalvageArray = new List<Interactable2SalvageStructure>();

    internal static readonly ClientInstanceMethod<ulong, ulong> SendOwnerAndGroup = ClientInstanceMethod<ulong, ulong>.Get(typeof(StructureDrop), "ReceiveOwnerAndGroup");

    internal static readonly ServerInstanceMethod SendSalvageRequest = ServerInstanceMethod.Get(typeof(StructureDrop), "ReceiveSalvageRequest");

    private NetId _netId;

    internal StructureData serversideData;

    internal HousingConnectionData housingConnectionData;

    internal FoliageCut foliageCut;

    public Transform model => _model;

    public uint instanceID
    {
        get
        {
            if (serversideData == null)
            {
                return 0u;
            }
            return serversideData.instanceID;
        }
    }

    public ItemStructureAsset asset { get; protected set; }

    public static event SalvageRequestHandler OnSalvageRequested_Global;

    public StructureData GetServersideData()
    {
        return serversideData;
    }

    public NetId GetNetId()
    {
        return _netId;
    }

    internal void AssignNetId(NetId netId)
    {
        _netId = netId;
        NetIdRegistry.Assign(netId, this);
        NetIdRegistry.AssignTransform(netId + 1u, _model);
    }

    internal void ReleaseNetId()
    {
        NetIdRegistry.ReleaseTransform(_netId + 1u, _model);
        NetIdRegistry.Release(_netId);
        _netId.Clear();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveHealth(byte hp)
    {
        Interactable2HP component = model.GetComponent<Interactable2HP>();
        if (component != null)
        {
            component.hp = hp;
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveTransform(in ClientInvocationContext context, byte old_x, byte old_y, Vector3 point, Quaternion rotation)
    {
        if (!StructureManager.tryGetRegion(old_x, old_y, out var region) || (!Provider.isServer && !region.isNetworked))
        {
            return;
        }
        try
        {
            StructureManager.housingConnections.UnlinkConnections(this);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Caught exception while unlinking housing connections:");
        }
        bool flag = foliageCut != null;
        if (flag)
        {
            RemoveFoliageCut();
        }
        model.position = point;
        model.rotation = rotation;
        try
        {
            StructureManager.housingConnections.LinkConnections(this);
        }
        catch (Exception e2)
        {
            UnturnedLog.exception(e2, "Caught exception while linking housing connections:");
        }
        if (flag)
        {
            AddFoliageCut();
        }
        if (Regions.tryGetCoordinate(point, out var x, out var y) && (old_x != x || old_y != y))
        {
            StructureRegion structureRegion = StructureManager.regions[x, y];
            region.drops.Remove(this);
            if (structureRegion.isNetworked || Provider.isServer)
            {
                structureRegion.drops.Add(this);
            }
            else if (!Provider.isServer)
            {
                StructureManager.instance.DestroyOrReleaseStructure(this);
                ReleaseNetId();
            }
            if (Provider.isServer)
            {
                region.structures.Remove(serversideData);
                structureRegion.structures.Add(serversideData);
            }
        }
        if (Provider.isServer)
        {
            serversideData.point = point;
            serversideData.rotation = rotation;
        }
    }

    /// <summary>
    /// Not using rate limit attribute because this is potentially called for hundreds of structures at once,
    /// and only admins will actually be allowed to apply the transform.
    /// </summary>
    [SteamCall(ESteamCallValidation.SERVERSIDE)]
    public void ReceiveTransformRequest(in ServerInvocationContext context, Vector3 point, Quaternion rotation)
    {
        Player player = context.GetPlayer();
        if (player == null || player.life.isDead || !player.look.canUseWorkzone || !StructureManager.tryGetRegion(_model, out var x, out var y, out var _))
        {
            return;
        }
        if (StructureManager.onTransformRequested != null)
        {
            Vector3 eulerAngles = rotation.eulerAngles;
            byte b = MeasurementTool.angleToByte(eulerAngles.x);
            byte b2 = MeasurementTool.angleToByte(eulerAngles.y);
            byte b3 = MeasurementTool.angleToByte(eulerAngles.z);
            byte angle_x = b;
            byte angle_y = b2;
            byte angle_z = b3;
            bool shouldAllow = true;
            StructureManager.onTransformRequested(player.channel.owner.playerID.steamID, x, y, instanceID, ref point, ref angle_x, ref angle_y, ref angle_z, ref shouldAllow);
            if (angle_x != b || angle_y != b2 || angle_z != b3)
            {
                float x2 = MeasurementTool.byteToAngle(angle_x);
                float y2 = MeasurementTool.byteToAngle(angle_y);
                float z = MeasurementTool.byteToAngle(angle_z);
                rotation = Quaternion.Euler(x2, y2, z);
            }
            if (!shouldAllow)
            {
                point = serversideData.point;
                rotation = serversideData.rotation;
            }
        }
        StructureManager.InternalSetStructureTransform(x, y, this, point, rotation);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveOwnerAndGroup(ulong newOwner, ulong newGroup)
    {
        workingSalvageArray.Clear();
        _model.GetComponentsInChildren(workingSalvageArray);
        foreach (Interactable2SalvageStructure item in workingSalvageArray)
        {
            item.owner = newOwner;
            item.group = newGroup;
        }
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10)]
    public void ReceiveSalvageRequest(in ServerInvocationContext context)
    {
        if (!StructureManager.tryGetRegion(_model, out var x, out var y, out var region))
        {
            return;
        }
        Player player = context.GetPlayer();
        if (player == null || player.life.isDead || (_model.position - player.transform.position).sqrMagnitude > 400f || !OwnershipTool.checkToggle(player.channel.owner.playerID.steamID, serversideData.owner, player.quests.groupID, serversideData.group))
        {
            return;
        }
        bool shouldAllow = true;
        if (StructureManager.onSalvageStructureRequested != null)
        {
            ushort index = (ushort)region.drops.IndexOf(this);
            StructureManager.onSalvageStructureRequested(player.channel.owner.playerID.steamID, x, y, index, ref shouldAllow);
        }
        StructureDrop.OnSalvageRequested_Global?.Invoke(this, context.GetCallingPlayer(), ref shouldAllow);
        if (!shouldAllow)
        {
            return;
        }
        if (asset != null)
        {
            if (asset.isUnpickupable)
            {
                return;
            }
            if (serversideData.structure.health >= asset.health)
            {
                player.inventory.forceAddItem(new Item(asset.id, EItemOrigin.NATURE), auto: true);
            }
            else if (asset.isSalvageable)
            {
                ItemAsset itemAsset = asset.FindSalvageItemAsset();
                if (itemAsset != null)
                {
                    player.inventory.forceAddItem(new Item(itemAsset, EItemOrigin.NATURE), auto: true);
                }
            }
        }
        StructureManager.destroyStructure(this, x, y, (_model.position - player.transform.position).normalized * 100f, wasPickedUp: true);
    }

    /// <summary>
    /// See BarricadeRegion.FindBarricadeByRootFast comment.
    /// </summary>
    internal static StructureDrop FindByRootFast(Transform rootTransform)
    {
        return rootTransform.GetComponent<StructureRefComponent>().tempNotSureIfStructureShouldBeAComponentYet;
    }

    /// <summary>
    /// For code which does not know whether transform exists and/or even is part of a house.
    /// See BarricadeRegion.FindBarricadeByRootFast comment.
    /// </summary>
    internal static StructureDrop FindByTransformFastMaybeNull(Transform transform)
    {
        return transform?.root.GetComponent<StructureRefComponent>()?.tempNotSureIfStructureShouldBeAComponentYet;
    }

    internal StructureDrop(Transform newModel, ItemStructureAsset newAsset)
    {
        _model = newModel;
        asset = newAsset;
    }

    [Obsolete]
    public StructureDrop(Transform newModel, uint newInstanceID)
        : this(newModel, null)
    {
    }

    internal void AddFoliageCut()
    {
        if (!Dedicator.IsDedicatedServer && foliageCut == null && asset != null && (asset.construct == EConstruct.FLOOR || asset.construct == EConstruct.FLOOR_POLY) && asset.foliageCutRadius > 0.01f)
        {
            foliageCut = new FoliageCut(model.position, asset.foliageCutRadius, 8f);
            FoliageSystem.AddCut(foliageCut);
        }
    }

    internal void RemoveFoliageCut()
    {
        if (foliageCut != null)
        {
            FoliageSystem.RemoveCut(foliageCut);
            foliageCut = null;
        }
    }
}
