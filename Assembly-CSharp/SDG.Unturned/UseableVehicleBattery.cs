using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableVehicleBattery : Useable
{
    private float startedUse;

    private float useTime;

    private bool isUsing;

    private bool isReplacing;

    private InteractableVehicle vehicle;

    private static readonly ClientInstanceMethod SendPlayReplace = ClientInstanceMethod.Get(typeof(UseableVehicleBattery), "ReceivePlayReplace");

    private bool wasSuccessfullyUsed;

    private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

    private bool isReplaceable => Time.realtimeSinceStartup - startedUse > useTime * 0.75f;

    private void replace()
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
    public void askReplace(CSteamID steamID)
    {
        ReceivePlayReplace();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askReplace")]
    public void ReceivePlayReplace()
    {
        if (base.player.equipment.IsEquipAnimationFinished)
        {
            replace();
        }
    }

    private bool fire()
    {
        if (base.channel.IsLocalPlayer)
        {
            RaycastInfo raycastInfo = DamageTool.raycast(new Ray(base.player.look.aim.position, base.player.look.aim.forward), 3f, RayMasks.DAMAGE_CLIENT);
            if (raycastInfo.vehicle == null || !raycastInfo.vehicle.isBatteryReplaceable)
            {
                return false;
            }
            base.player.input.sendRaycast(raycastInfo, ERaycastInfoUsage.Battery);
        }
        if (Provider.isServer)
        {
            if (!base.player.input.hasInputs())
            {
                return false;
            }
            InputInfo input = base.player.input.getInput(doOcclusionCheck: true, ERaycastInfoUsage.Battery);
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
            if (input.vehicle == null || !input.vehicle.isBatteryReplaceable)
            {
                return false;
            }
            isReplacing = true;
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
            replace();
            if (Provider.isServer)
            {
                SendPlayReplace.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GatherRemoteClientConnectionsExcludingOwner());
            }
            return true;
        }
        return false;
    }

    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
        useTime = base.player.animator.GetAnimationLength("Use");
    }

    public override void simulate(uint simulation, bool inputSteady)
    {
        if (isReplacing && isReplaceable)
        {
            isReplacing = false;
            if (vehicle != null && vehicle.isBatteryReplaceable)
            {
                vehicle.replaceBattery(base.player, base.player.equipment.quality, base.player.equipment.asset.GUID);
                wasSuccessfullyUsed = true;
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
