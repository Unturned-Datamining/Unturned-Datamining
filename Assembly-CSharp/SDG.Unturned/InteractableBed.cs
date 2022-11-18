using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableBed : Interactable
{
    private CSteamID _owner;

    private float claimed;

    internal static readonly ClientInstanceMethod<CSteamID> SendClaim = ClientInstanceMethod<CSteamID>.Get(typeof(InteractableBed), "ReceiveClaim");

    private static readonly ServerInstanceMethod SendClaimRequest = ServerInstanceMethod.Get(typeof(InteractableBed), "ReceiveClaimRequest");

    public CSteamID owner => _owner;

    public bool isClaimed => owner != CSteamID.Nil;

    public bool isClaimable => Time.realtimeSinceStartup - claimed > 0.75f;

    public bool checkClaim(CSteamID enemy)
    {
        _ = Provider.isServer;
        if (isClaimed)
        {
            return enemy == owner;
        }
        return true;
    }

    public void updateClaim(CSteamID newOwner)
    {
        claimed = Time.realtimeSinceStartup;
        _owner = newOwner;
    }

    public override void updateState(Asset asset, byte[] state)
    {
        _owner = new CSteamID(BitConverter.ToUInt64(state, 0));
    }

    public override bool checkUseable()
    {
        return checkClaim(Provider.client);
    }

    public override void use()
    {
        ClientClaim();
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        text = "";
        color = Color.white;
        if (checkUseable())
        {
            if (isClaimed)
            {
                message = EPlayerMessage.BED_OFF;
            }
            else
            {
                message = EPlayerMessage.BED_ON;
            }
        }
        else
        {
            message = EPlayerMessage.BED_CLAIMED;
        }
        return true;
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveClaim(CSteamID newOwner)
    {
        updateClaim(newOwner);
    }

    public void ClientClaim()
    {
        SendClaimRequest.Invoke(GetNetId(), ENetReliability.Unreliable);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 1)]
    public void ReceiveClaimRequest(in ServerInvocationContext context)
    {
        if (!BarricadeManager.tryGetRegion(base.transform, out var x, out var y, out var plant, out var region))
        {
            return;
        }
        Player player = context.GetPlayer();
        if (!(player == null) && !player.life.isDead && !((base.transform.position - player.transform.position).sqrMagnitude > 400f) && isClaimable && checkClaim(player.channel.owner.playerID.steamID))
        {
            if (isClaimed)
            {
                BarricadeManager.ServerSetBedOwnerInternal(this, x, y, plant, region, CSteamID.Nil);
                return;
            }
            BarricadeManager.unclaimBeds(player.channel.owner.playerID.steamID);
            BarricadeManager.ServerSetBedOwnerInternal(this, x, y, plant, region, player.channel.owner.playerID.steamID);
        }
    }
}
