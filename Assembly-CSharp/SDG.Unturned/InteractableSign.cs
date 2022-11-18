using System;
using System.Collections.Generic;
using System.Text;
using SDG.NetTransport;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class InteractableSign : Interactable
{
    private CSteamID _owner;

    private CSteamID _group;

    private bool isLocked;

    private bool hasInitializedTextComponents;

    private Text label_0;

    private Text label_1;

    private List<TextMeshPro> tmpComponents;

    internal static readonly ClientInstanceMethod<string> SendChangeText = ClientInstanceMethod<string>.Get(typeof(InteractableSign), "ReceiveChangeText");

    private static readonly ServerInstanceMethod<string> SendChangeTextRequest = ServerInstanceMethod<string>.Get(typeof(InteractableSign), "ReceiveChangeTextRequest");

    public CSteamID owner => _owner;

    public CSteamID group => _group;

    public string text { get; private set; }

    public string DisplayText { get; private set; }

    public bool hasMesh
    {
        get
        {
            if (!(label_0 != null) && !(label_1 != null))
            {
                if (tmpComponents != null)
                {
                    return tmpComponents.Count > 0;
                }
                return false;
            }
            return true;
        }
    }

    public bool checkUpdate(CSteamID enemyPlayer, CSteamID enemyGroup)
    {
        _ = Provider.isServer;
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

    public string trimText(string text)
    {
        return text.Trim();
    }

    public bool isTextValid(string text)
    {
        if (Encoding.UTF8.GetByteCount(text) > 230)
        {
            return false;
        }
        if (hasMesh)
        {
            if (!RichTextUtil.isTextValidForSign(text))
            {
                return false;
            }
            if (text.CountNewlines() > 8)
            {
                return false;
            }
        }
        return true;
    }

    public void updateText(string newText)
    {
        text = newText;
    }

    public override void updateState(Asset asset, byte[] state)
    {
        isLocked = ((ItemBarricadeAsset)asset).isLocked;
        _owner = new CSteamID(BitConverter.ToUInt64(state, 0));
        _group = new CSteamID(BitConverter.ToUInt64(state, 8));
        byte b = state[16];
        if (b > 0)
        {
            string @string = Encoding.UTF8.GetString(state, 17, b);
            updateText(@string);
        }
        else
        {
            updateText(string.Empty);
        }
    }

    public override bool checkUseable()
    {
        if (checkUpdate(Provider.client, Player.player.quests.groupID))
        {
            return !PlayerUI.window.showCursor;
        }
        return false;
    }

    public override void use()
    {
        PlayerBarricadeSignUI.open(this);
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
    public void ReceiveChangeText(string newText)
    {
        updateText(newText);
    }

    public void ClientSetText(string newText)
    {
        SendChangeTextRequest.Invoke(GetNetId(), ENetReliability.Unreliable, newText);
    }

    [SteamCall(ESteamCallValidation.SERVERSIDE, ratelimitHz = 2)]
    public void ReceiveChangeTextRequest(in ServerInvocationContext context, string newText)
    {
        Player player = context.GetPlayer();
        if (player == null || player.life.isDead || (base.transform.position - player.transform.position).sqrMagnitude > 400f || !checkUpdate(player.channel.owner.playerID.steamID, player.quests.groupID))
        {
            return;
        }
        string trimmedText = trimText(newText);
        if (isTextValid(trimmedText))
        {
            bool shouldAllow = true;
            if (BarricadeManager.onModifySignRequested != null)
            {
                BarricadeManager.onModifySignRequested(player.channel.owner.playerID.steamID, this, ref trimmedText, ref shouldAllow);
            }
            if (shouldAllow && BarricadeManager.tryGetRegion(base.transform, out var x, out var y, out var plant, out var region))
            {
                BarricadeManager.ServerSetSignTextInternal(this, region, x, y, plant, trimmedText);
            }
        }
    }
}
