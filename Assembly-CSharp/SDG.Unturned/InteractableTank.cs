using System;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableTank : Interactable
{
    private ETankSource _source;

    private ushort _amount;

    private ushort _capacity;

    private static readonly ClientInstanceMethod<ushort> SendAmount = ClientInstanceMethod<ushort>.Get(typeof(InteractableTank), "ReceiveAmount");

    public ETankSource source => _source;

    public ushort amount => _amount;

    public ushort capacity => _capacity;

    public bool isRefillable => amount < capacity;

    public bool isSiphonable => amount > 0;

    public void updateAmount(ushort newAmount)
    {
        _amount = newAmount;
    }

    public override void updateState(Asset asset, byte[] state)
    {
        _amount = BitConverter.ToUInt16(state, 0);
        _capacity = ((ItemTankAsset)asset).resource;
        _source = ((ItemTankAsset)asset).source;
    }

    public override bool checkUseable()
    {
        return amount > 0;
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        if (source == ETankSource.WATER)
        {
            message = EPlayerMessage.VOLUME_WATER;
            text = amount + "/" + capacity;
        }
        else
        {
            message = EPlayerMessage.VOLUME_FUEL;
            text = "";
        }
        color = Color.white;
        return true;
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveAmount(ushort newAmount)
    {
        updateAmount(newAmount);
    }

    public void ServerSetAmount(ushort newAmount)
    {
        if (BarricadeManager.tryGetRegion(base.transform, out var x, out var y, out var plant, out var region))
        {
            SendAmount.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, BarricadeManager.GatherRemoteClientConnections(x, y, plant), newAmount);
            BarricadeDrop barricadeDrop = region.FindBarricadeByRootFast(base.transform);
            byte[] bytes = BitConverter.GetBytes(newAmount);
            barricadeDrop.serversideData.barricade.state[0] = bytes[0];
            barricadeDrop.serversideData.barricade.state[1] = bytes[1];
        }
    }
}
