using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableArrestEnd : Useable
{
    private float startedUse;

    private float useTime;

    private bool isUsing;

    private Player enemy;

    private static readonly ClientInstanceMethod SendPlayArrest = ClientInstanceMethod.Get(typeof(UseableArrestEnd), "ReceivePlayArrest");

    private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

    private void arrest()
    {
        base.player.animator.play("Use", smooth: false);
    }

    [Obsolete]
    public void askArrest(CSteamID steamID)
    {
        ReceivePlayArrest();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askArrest")]
    public void ReceivePlayArrest()
    {
        if (base.player.equipment.isEquipped)
        {
            arrest();
        }
    }

    public override void startPrimary()
    {
        if (base.player.equipment.isBusy)
        {
            return;
        }
        if (base.channel.isOwner)
        {
            RaycastInfo raycastInfo = DamageTool.raycast(new Ray(base.player.look.aim.position, base.player.look.aim.forward), 3f, RayMasks.DAMAGE_CLIENT);
            if (raycastInfo.player != null && raycastInfo.player.animator.gesture == EPlayerGesture.ARREST_START)
            {
                base.player.input.sendRaycast(raycastInfo, ERaycastInfoUsage.ArrestEnd);
                if (!Provider.isServer)
                {
                    base.player.equipment.isBusy = true;
                    startedUse = Time.realtimeSinceStartup;
                    isUsing = true;
                    arrest();
                }
            }
        }
        if (Provider.isServer && base.player.input.hasInputs())
        {
            InputInfo input = base.player.input.getInput(doOcclusionCheck: true, ERaycastInfoUsage.ArrestEnd);
            if (input != null && input.type == ERaycastInfoType.PLAYER && input.player != null)
            {
                enemy = input.player;
                base.player.equipment.isBusy = true;
                startedUse = Time.realtimeSinceStartup;
                isUsing = true;
                arrest();
                SendPlayArrest.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.EnumerateClients_RemoteNotOwner());
            }
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
            if (enemy != null && enemy.animator.gesture == EPlayerGesture.ARREST_START && enemy.animator.captorID == base.channel.owner.playerID.steamID && enemy.animator.captorItem == ((ItemArrestEndAsset)base.player.equipment.asset).recover)
            {
                enemy.animator.captorID = CSteamID.Nil;
                enemy.animator.captorStrength = 0;
                enemy.animator.sendGesture(EPlayerGesture.ARREST_STOP, all: true);
                base.player.inventory.forceAddItem(new Item(((ItemArrestEndAsset)base.player.equipment.asset).recover, EItemOrigin.NATURE), auto: false);
            }
            base.player.equipment.dequip();
        }
    }
}
