using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableArrestStart : Useable
{
    private float startedUse;

    private float useTime;

    private bool isUsing;

    private Player enemy;

    private static readonly ClientInstanceMethod SendPlayArrest = ClientInstanceMethod.Get(typeof(UseableArrestStart), "ReceivePlayArrest");

    private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

    private void arrest()
    {
        base.player.animator.play("Use", smooth: false);
        if (!Dedicator.IsDedicatedServer)
        {
            base.player.playSound(((ItemArrestStartAsset)base.player.equipment.asset).use);
        }
    }

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

    public override bool startPrimary()
    {
        if (base.player.equipment.isBusy)
        {
            return false;
        }
        if (base.channel.isOwner)
        {
            RaycastInfo raycastInfo = DamageTool.raycast(new Ray(base.player.look.aim.position, base.player.look.aim.forward), 3f, RayMasks.DAMAGE_CLIENT);
            if (raycastInfo.player != null && raycastInfo.player.animator.gesture == EPlayerGesture.SURRENDER_START)
            {
                base.player.input.sendRaycast(raycastInfo, ERaycastInfoUsage.ArrestStart);
                if (!Provider.isServer)
                {
                    base.player.equipment.isBusy = true;
                    startedUse = Time.realtimeSinceStartup;
                    isUsing = true;
                    arrest();
                }
            }
        }
        if (Provider.isServer)
        {
            if (!base.player.input.hasInputs())
            {
                return false;
            }
            InputInfo input = base.player.input.getInput(doOcclusionCheck: true, ERaycastInfoUsage.ArrestStart);
            if (input == null)
            {
                return false;
            }
            if (input.type == ERaycastInfoType.PLAYER && input.player != null)
            {
                enemy = input.player;
                base.player.equipment.isBusy = true;
                startedUse = Time.realtimeSinceStartup;
                isUsing = true;
                arrest();
                SendPlayArrest.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GatherRemoteClientConnectionsExcludingOwner());
            }
        }
        return true;
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
            if (enemy != null && enemy.animator.gesture == EPlayerGesture.SURRENDER_START)
            {
                enemy.animator.captorID = base.channel.owner.playerID.steamID;
                enemy.animator.captorItem = base.player.equipment.itemID;
                enemy.animator.captorStrength = ((ItemArrestStartAsset)base.player.equipment.asset).strength;
                enemy.animator.sendGesture(EPlayerGesture.ARREST_START, all: true);
                base.player.equipment.use();
            }
            else
            {
                base.player.equipment.dequip();
            }
        }
    }
}
