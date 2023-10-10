using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableClothing : Useable
{
    private float startedUse;

    private float useTime;

    private bool isUsing;

    private static readonly ClientInstanceMethod SendPlayWear = ClientInstanceMethod.Get(typeof(UseableClothing), "ReceivePlayWear");

    private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

    private void wear()
    {
        base.player.animator.play("Use", smooth: false);
        if (Provider.isServer)
        {
            AlertTool.alert(base.transform.position, 8f);
        }
    }

    [Obsolete]
    public void askWear(CSteamID steamID)
    {
        ReceivePlayWear();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askWear")]
    public void ReceivePlayWear()
    {
        if (base.player.equipment.IsEquipAnimationFinished)
        {
            wear();
        }
    }

    public override bool startPrimary()
    {
        if (base.player.equipment.isBusy)
        {
            return false;
        }
        base.player.equipment.isBusy = true;
        startedUse = Time.realtimeSinceStartup;
        isUsing = true;
        if (Provider.isServer)
        {
            SendPlayWear.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GatherRemoteClientConnectionsExcludingOwner());
        }
        wear();
        return true;
    }

    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
        useTime = base.player.animator.GetAnimationLength("Use");
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
            ItemAsset asset = base.player.equipment.asset;
            EItemType type = asset.type;
            byte quality = base.player.equipment.quality;
            byte[] state = base.player.equipment.state;
            base.player.equipment.use();
            switch (type)
            {
            case EItemType.HAT:
                base.player.clothing.askWearHat(asset as ItemHatAsset, quality, state, playEffect: true);
                break;
            case EItemType.SHIRT:
                base.player.clothing.askWearShirt(asset as ItemShirtAsset, quality, state, playEffect: true);
                break;
            case EItemType.PANTS:
                base.player.clothing.askWearPants(asset as ItemPantsAsset, quality, state, playEffect: true);
                break;
            case EItemType.BACKPACK:
                base.player.clothing.askWearBackpack(asset as ItemBackpackAsset, quality, state, playEffect: true);
                break;
            case EItemType.VEST:
                base.player.clothing.askWearVest(asset as ItemVestAsset, quality, state, playEffect: true);
                break;
            case EItemType.MASK:
                base.player.clothing.askWearMask(asset as ItemMaskAsset, quality, state, playEffect: true);
                break;
            case EItemType.GLASSES:
                base.player.clothing.askWearGlasses(asset as ItemGlassesAsset, quality, state, playEffect: true);
                break;
            }
        }
    }
}
