using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableCarlockpick : Useable
{
    private float startedUse;

    private float useTime;

    private bool isUsing;

    private bool isUnlocking;

    private InteractableVehicle vehicle;

    private static readonly ClientInstanceMethod SendPlayJimmy = ClientInstanceMethod.Get(typeof(UseableCarlockpick), "ReceivePlayJimmy");

    private bool wasSuccessfullyUsed;

    private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

    private bool isUnlockable => Time.realtimeSinceStartup - startedUse > useTime * 0.75f;

    private void jimmy()
    {
        startedUse = Time.realtimeSinceStartup;
        isUsing = true;
        base.player.animator.play("Use", smooth: false);
        if (!Dedicator.IsDedicatedServer)
        {
            base.player.playSound(((ItemToolAsset)base.player.equipment.asset).use);
        }
        if (Provider.isServer)
        {
            AlertTool.alert(base.transform.position, 8f);
        }
    }

    [Obsolete]
    public void askJimmy(CSteamID steamID)
    {
        ReceivePlayJimmy();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askJimmy")]
    public void ReceivePlayJimmy()
    {
        if (base.player.equipment.isEquipped)
        {
            jimmy();
        }
    }

    private bool fire()
    {
        if (base.channel.isOwner)
        {
            RaycastInfo raycastInfo = DamageTool.raycast(new Ray(base.player.look.aim.position, base.player.look.aim.forward), 3f, RayMasks.DAMAGE_CLIENT);
            if (raycastInfo.vehicle == null || !raycastInfo.vehicle.isEmpty || !raycastInfo.vehicle.isLocked)
            {
                return false;
            }
            base.player.input.sendRaycast(raycastInfo, ERaycastInfoUsage.Carlockpick);
        }
        if (Provider.isServer)
        {
            if (!base.player.input.hasInputs())
            {
                return false;
            }
            InputInfo input = base.player.input.getInput(doOcclusionCheck: true, ERaycastInfoUsage.Carlockpick);
            if (input == null)
            {
                return false;
            }
            if ((input.point - base.player.look.aim.position).sqrMagnitude > 49f)
            {
                return false;
            }
            if (input.type != ERaycastInfoType.VEHICLE)
            {
                return false;
            }
            if (input.vehicle == null || !input.vehicle.isEmpty || !input.vehicle.isLocked)
            {
                return false;
            }
            isUnlocking = true;
            vehicle = input.vehicle;
        }
        return true;
    }

    public override bool startPrimary()
    {
        if (base.player.equipment.isBusy)
        {
            return false;
        }
        if (isUseable && fire())
        {
            base.player.equipment.isBusy = true;
            startedUse = Time.realtimeSinceStartup;
            isUsing = true;
            jimmy();
            if (Provider.isServer)
            {
                base.player.life.markAggressive(force: true);
                SendPlayJimmy.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GatherRemoteClientConnectionsExcludingOwner());
            }
            return true;
        }
        return false;
    }

    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
        useTime = base.player.animator.getAnimationLength("Use");
    }

    public override void simulate(uint simulation, bool inputSteady)
    {
        if (isUnlocking && isUnlockable)
        {
            isUnlocking = false;
            if (vehicle != null && vehicle.isEmpty && vehicle.isLocked)
            {
                VehicleManager.unlockVehicle(vehicle, base.player);
                wasSuccessfullyUsed = !vehicle.isLocked;
                vehicle = null;
            }
            if (Provider.isServer)
            {
                if (wasSuccessfullyUsed)
                {
                    base.player.equipment.useStepA();
                }
                else
                {
                    base.player.equipment.dequip();
                }
            }
        }
        if (isUsing && isUseable)
        {
            base.player.equipment.isBusy = false;
            isUsing = false;
            if (Provider.isServer && wasSuccessfullyUsed)
            {
                base.player.equipment.useStepB();
            }
        }
    }
}
