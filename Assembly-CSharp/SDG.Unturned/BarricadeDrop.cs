using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class BarricadeDrop
{
    public delegate void SalvageRequestHandler(BarricadeDrop barricade, SteamPlayer instigatorClient, ref bool shouldAllow);

    private Transform _model;

    private Interactable _interactable;

    internal static readonly ClientInstanceMethod<byte> SendHealth = ClientInstanceMethod<byte>.Get(typeof(BarricadeDrop), "ReceiveHealth");

    internal static readonly ClientInstanceMethod<byte, byte, ushort, Vector3, byte, byte, byte> SendTransform = ClientInstanceMethod<byte, byte, ushort, Vector3, byte, byte, byte>.Get(typeof(BarricadeDrop), "ReceiveTransform");

    internal static readonly ServerInstanceMethod<Vector3, byte, byte, byte> SendTransformRequest = ServerInstanceMethod<Vector3, byte, byte, byte>.Get(typeof(BarricadeDrop), "ReceiveTransformRequest");

    private static List<Interactable2SalvageBarricade> workingSalvageArray = new List<Interactable2SalvageBarricade>();

    internal static readonly ClientInstanceMethod<ulong, ulong> SendOwnerAndGroup = ClientInstanceMethod<ulong, ulong>.Get(typeof(BarricadeDrop), "ReceiveOwnerAndGroup");

    internal static readonly ClientInstanceMethod<byte[]> SendUpdateState = ClientInstanceMethod<byte[]>.Get(typeof(BarricadeDrop), "ReceiveUpdateState");

    internal static readonly ServerInstanceMethod SendSalvageRequest = ServerInstanceMethod.Get(typeof(BarricadeDrop), "ReceiveSalvageRequest");

    private NetId _netId;

    internal BarricadeData serversideData;

    public Transform model => _model;

    public Interactable interactable => _interactable;

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

    public ItemBarricadeAsset asset { get; protected set; }

    public static event SalvageRequestHandler OnSalvageRequested_Global;

    public BarricadeData GetServersideData()
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
        if (_interactable != null)
        {
            _interactable.AssignNetId(netId + 2u);
        }
    }

    internal void ReleaseNetId()
    {
        if (_interactable != null)
        {
            _interactable.ReleaseNetId();
        }
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
    public void ReceiveTransform(in ClientInvocationContext context, byte old_x, byte old_y, ushort oldPlant, Vector3 point, byte angle_x, byte angle_y, byte angle_z)
    {
        if (!BarricadeManager.tryGetRegion(old_x, old_y, oldPlant, out var region) || (!Provider.isServer && !region.isNetworked))
        {
            return;
        }
        model.position = point;
        model.rotation = Quaternion.Euler(angle_x * 2, angle_y * 2, angle_z * 2);
        if (oldPlant == ushort.MaxValue)
        {
            if (Regions.tryGetCoordinate(point, out var x, out var y) && (old_x != x || old_y != y))
            {
                BarricadeRegion barricadeRegion = BarricadeManager.regions[x, y];
                region.drops.Remove(this);
                if (barricadeRegion.isNetworked || Provider.isServer)
                {
                    barricadeRegion.drops.Add(this);
                }
                else if (!Provider.isServer)
                {
                    CustomDestroy();
                }
                if (Provider.isServer)
                {
                    region.barricades.Remove(serversideData);
                    barricadeRegion.barricades.Add(serversideData);
                }
            }
            if (Provider.isServer)
            {
                serversideData.point = point;
                serversideData.angle_x = angle_x;
                serversideData.angle_y = angle_y;
                serversideData.angle_z = angle_z;
            }
        }
        else if (Provider.isServer)
        {
            serversideData.point = model.localPosition;
            Vector3 eulerAngles = model.localRotation.eulerAngles;
            serversideData.angle_x = MeasurementTool.angleToByte(Mathf.RoundToInt(eulerAngles.x / 2f) * 2);
            serversideData.angle_y = MeasurementTool.angleToByte(Mathf.RoundToInt(eulerAngles.y / 2f) * 2);
            serversideData.angle_z = MeasurementTool.angleToByte(Mathf.RoundToInt(eulerAngles.z / 2f) * 2);
        }
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE)]
    public void ReceiveTransformRequest(in ServerInvocationContext context, Vector3 point, byte angle_x, byte angle_y, byte angle_z)
    {
        Player player = context.GetPlayer();
        if (!(player == null) && !player.life.isDead && player.look.canUseWorkzone && BarricadeManager.tryGetRegion(_model, out var x, out var y, out var plant, out var _))
        {
            bool shouldAllow = true;
            if (BarricadeManager.onTransformRequested != null)
            {
                BarricadeManager.onTransformRequested(player.channel.owner.playerID.steamID, x, y, plant, instanceID, ref point, ref angle_x, ref angle_y, ref angle_z, ref shouldAllow);
            }
            if (!shouldAllow)
            {
                point = model.position;
                Vector3 eulerAngles = model.rotation.eulerAngles;
                angle_x = MeasurementTool.angleToByte(Mathf.RoundToInt(eulerAngles.x / 2f) * 2);
                angle_y = MeasurementTool.angleToByte(Mathf.RoundToInt(eulerAngles.y / 2f) * 2);
                angle_z = MeasurementTool.angleToByte(Mathf.RoundToInt(eulerAngles.z / 2f) * 2);
            }
            BarricadeManager.InternalSetBarricadeTransform(x, y, plant, this, point, angle_x, angle_y, angle_z);
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveOwnerAndGroup(ulong newOwner, ulong newGroup)
    {
        workingSalvageArray.Clear();
        model.GetComponentsInChildren(workingSalvageArray);
        foreach (Interactable2SalvageBarricade item in workingSalvageArray)
        {
            item.owner = newOwner;
            item.group = newGroup;
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveUpdateState(byte[] newState)
    {
        if (asset == null || interactable == null)
        {
            UnturnedLog.warn("tellBarricadeUpdateState was missing asset/interactable");
        }
        else
        {
            interactable.updateState(asset, newState);
        }
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 10)]
    public void ReceiveSalvageRequest(in ServerInvocationContext context)
    {
        Player player = context.GetPlayer();
        if (player == null || player.life.isDead || (!asset.shouldBypassPickupOwnership && !OwnershipTool.checkToggle(player.channel.owner.playerID.steamID, serversideData.owner, player.quests.groupID, serversideData.group)) || !BarricadeManager.tryGetRegion(_model, out var x, out var y, out var plant, out var region))
        {
            return;
        }
        bool shouldAllow = true;
        if (BarricadeManager.onSalvageBarricadeRequested != null)
        {
            ushort index = (ushort)region.drops.IndexOf(this);
            BarricadeManager.onSalvageBarricadeRequested(player.channel.owner.playerID.steamID, x, y, plant, index, ref shouldAllow);
        }
        BarricadeDrop.OnSalvageRequested_Global?.Invoke(this, context.GetCallingPlayer(), ref shouldAllow);
        if (!shouldAllow || asset.isUnpickupable)
        {
            return;
        }
        if (serversideData.barricade.health >= asset.health)
        {
            player.inventory.forceAddItem(new Item(serversideData.barricade.asset.id, EItemOrigin.NATURE), auto: true);
        }
        else if (asset.isSalvageable)
        {
            ItemAsset itemAsset = asset.FindSalvageItemAsset();
            if (itemAsset != null)
            {
                player.inventory.forceAddItem(new Item(itemAsset, EItemOrigin.NATURE), auto: true);
            }
        }
        BarricadeManager.destroyBarricade(this, x, y, plant);
    }

    internal void CustomDestroy()
    {
        try
        {
            model.GetComponent<IManualOnDestroy>()?.ManualOnDestroy();
            ReleaseNetId();
            model.position = Vector3.zero;
            BarricadeManager.instance.DestroyOrReleaseBarricade(asset, model.gameObject);
        }
        catch (Exception e)
        {
            UnturnedLog.exception(e, "Exception destroying barricade {0}:", asset);
        }
    }

    internal static BarricadeDrop FindByRootFast(Transform rootTransform)
    {
        return rootTransform.GetComponent<BarricadeRefComponent>().tempNotSureIfBarricadeShouldBeAComponentYet;
    }

    internal static BarricadeDrop FindByTransformFastMaybeNull(Transform transform)
    {
        return transform?.root.GetComponent<BarricadeRefComponent>()?.tempNotSureIfBarricadeShouldBeAComponentYet;
    }

    internal BarricadeDrop(Transform newModel, Interactable newInteractable, ItemBarricadeAsset newAsset)
    {
        _model = newModel;
        _interactable = newInteractable;
        asset = newAsset;
    }

    [Obsolete]
    public BarricadeDrop(Transform newModel, Interactable newInteractable, uint newInstanceID, ItemBarricadeAsset newAsset)
        : this(newModel, newInteractable, newAsset)
    {
    }
}
