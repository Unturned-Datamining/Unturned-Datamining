using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableFilter : Useable
{
    private float startedUse;

    private float useTime;

    private bool isUsing;

    private static readonly ClientInstanceMethod SendPlayFilter = ClientInstanceMethod.Get(typeof(UseableFilter), "ReceivePlayFilter");

    private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

    private void filter()
    {
        base.player.animator.play("Use", smooth: false);
        if (Provider.isServer)
        {
            AlertTool.alert(base.transform.position, 8f);
        }
    }

    [Obsolete]
    public void askFilter(CSteamID steamID)
    {
        ReceivePlayFilter();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askFilter")]
    public void ReceivePlayFilter()
    {
        if (base.player.equipment.isEquipped)
        {
            filter();
        }
    }

    public override void startPrimary()
    {
        if (!base.player.equipment.isBusy && base.player.clothing.maskAsset != null && base.player.clothing.maskAsset.proofRadiation && base.player.clothing.maskQuality != 100)
        {
            base.player.equipment.isBusy = true;
            startedUse = Time.realtimeSinceStartup;
            isUsing = true;
            if (Provider.isServer)
            {
                SendPlayFilter.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.EnumerateClients_RemoteNotOwner());
            }
            filter();
        }
    }

    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
        useTime = base.player.animator.getAnimationLength("Use");
    }

    public override void simulate(uint simulation, bool inputSteady)
    {
        if (!isUsing || !isUseable)
        {
            return;
        }
        base.player.equipment.isBusy = false;
        isUsing = false;
        if (Provider.isServer)
        {
            if (base.player.clothing.maskAsset != null && base.player.clothing.maskAsset.proofRadiation && base.player.clothing.maskQuality < 100)
            {
                base.player.equipment.use();
                base.player.clothing.maskQuality = 100;
                base.player.clothing.sendUpdateMaskQuality();
            }
            else
            {
                base.player.equipment.dequip();
            }
        }
    }
}
