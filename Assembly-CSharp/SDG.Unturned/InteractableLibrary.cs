using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableLibrary : Interactable
{
    private CSteamID _owner;

    private CSteamID _group;

    private uint _amount;

    private uint _capacity;

    private byte _tax;

    private bool isLocked;

    private static readonly ClientInstanceMethod<uint> SendAmount = ClientInstanceMethod<uint>.Get(typeof(InteractableLibrary), "ReceiveAmount");

    private static readonly ServerInstanceMethod<byte, uint> SendTransferLibraryRequest = ServerInstanceMethod<byte, uint>.Get(typeof(InteractableLibrary), "ReceiveTransferLibraryRequest");

    public CSteamID owner => _owner;

    public CSteamID group => _group;

    public uint amount => _amount;

    public uint capacity => _capacity;

    public byte tax => _tax;

    public bool checkTransfer(CSteamID enemyPlayer, CSteamID enemyGroup)
    {
        if (Provider.isServer && !Dedicator.IsDedicatedServer)
        {
            return true;
        }
        if (isLocked && !(enemyPlayer == owner))
        {
            if (group != CSteamID.Nil)
            {
                return enemyGroup == group;
            }
            return false;
        }
        return true;
    }

    public void updateAmount(uint newAmount)
    {
        _amount = newAmount;
    }

    public override void updateState(Asset asset, byte[] state)
    {
        isLocked = ((ItemBarricadeAsset)asset).isLocked;
        _capacity = ((ItemLibraryAsset)asset).capacity;
        _tax = ((ItemLibraryAsset)asset).tax;
        _owner = new CSteamID(BitConverter.ToUInt64(state, 0));
        _group = new CSteamID(BitConverter.ToUInt64(state, 8));
        _amount = BitConverter.ToUInt32(state, 16);
    }

    public override bool checkUseable()
    {
        if (checkTransfer(Provider.client, Player.player.quests.groupID))
        {
            return !PlayerUI.window.showCursor;
        }
        return false;
    }

    public override void use()
    {
        PlayerBarricadeLibraryUI.open(this);
        PlayerLifeUI.close();
    }

    public override bool checkHint(out EPlayerMessage message, out string text, out Color color)
    {
        if (checkUseable())
        {
            message = EPlayerMessage.USE;
        }
        else
        {
            message = EPlayerMessage.LOCKED;
        }
        text = "";
        color = Color.white;
        return !PlayerUI.window.showCursor;
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, deferMode = ENetInvocationDeferMode.Queue)]
    public void ReceiveAmount(uint newAmount)
    {
        updateAmount(newAmount);
    }

    public void ClientTransfer(byte transaction, uint delta)
    {
        SendTransferLibraryRequest.Invoke(GetNetId(), ENetReliability.Unreliable, transaction, delta);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
    public void ReceiveTransferLibraryRequest(in ServerInvocationContext context, byte transaction, uint delta)
    {
        Player player = context.GetPlayer();
        if (player == null || player.life.isDead || (base.transform.position - player.transform.position).sqrMagnitude > 400f || !BarricadeManager.tryGetRegion(base.transform, out var x, out var y, out var plant, out var region) || !checkTransfer(player.channel.owner.playerID.steamID, player.quests.groupID))
        {
            return;
        }
        uint num3;
        if (transaction == 0)
        {
            uint num = (uint)Math.Ceiling((double)delta * ((double)(int)tax / 100.0));
            uint num2 = delta - num;
            if (delta > player.skills.experience || num2 + amount > capacity)
            {
                return;
            }
            num3 = amount + num2;
            player.skills.askSpend(delta);
        }
        else
        {
            if (delta > amount)
            {
                return;
            }
            num3 = amount - delta;
            player.skills.askAward(delta);
        }
        SendAmount.InvokeAndLoopback(GetNetId(), ENetReliability.Reliable, BarricadeManager.EnumerateClients_Remote(x, y, plant), num3);
        BarricadeDrop barricadeDrop = region.FindBarricadeByRootFast(base.transform);
        Buffer.BlockCopy(BitConverter.GetBytes(num3), 0, barricadeDrop.serversideData.barricade.state, 16, 4);
    }
}
