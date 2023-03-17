using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableTire : Useable
{
    public delegate void ModifyTireRequestHandler(UseableTire useable, InteractableVehicle vehicle, int wheelIndex, ref bool shouldAllow);

    private float startedUse;

    private float useTime;

    private bool isUsing;

    private bool isAttaching;

    private InteractableVehicle vehicle;

    private int vehicleWheelIndex = -1;

    private static readonly ClientInstanceMethod SendPlayAttach = ClientInstanceMethod.Get(typeof(UseableTire), "ReceivePlayAttach");

    private bool wasSuccessfullyUsed;

    private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

    private bool isAttachable => Time.realtimeSinceStartup - startedUse > useTime * 0.75f;

    public static event ModifyTireRequestHandler onModifyTireRequested;

    private void attach()
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
    public void askAttach(CSteamID steamID)
    {
        ReceivePlayAttach();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askAttach")]
    public void ReceivePlayAttach()
    {
        if (base.player.equipment.isEquipped)
        {
            attach();
        }
    }

    private bool fire()
    {
        if (base.channel.isOwner)
        {
            RaycastInfo raycastInfo = DamageTool.raycast(new Ray(base.player.look.aim.position, base.player.look.aim.forward), 3f, RayMasks.DAMAGE_CLIENT);
            if (raycastInfo.vehicle == null || !raycastInfo.vehicle.isTireReplaceable)
            {
                return false;
            }
            if (((ItemTireAsset)base.player.equipment.asset).mode == EUseableTireMode.ADD && !raycastInfo.vehicle.isTireCompatible(base.player.equipment.itemID))
            {
                return false;
            }
            if (raycastInfo.vehicle.getClosestAliveTireIndex(raycastInfo.point, ((ItemTireAsset)base.player.equipment.asset).mode == EUseableTireMode.REMOVE) == -1)
            {
                return false;
            }
            base.player.input.sendRaycast(raycastInfo, ERaycastInfoUsage.Tire);
        }
        if (Provider.isServer)
        {
            if (!base.player.input.hasInputs())
            {
                return false;
            }
            InputInfo input = base.player.input.getInput(doOcclusionCheck: true, ERaycastInfoUsage.Tire);
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
            if (input.vehicle == null || !input.vehicle.isTireReplaceable)
            {
                return false;
            }
            if (((ItemTireAsset)base.player.equipment.asset).mode == EUseableTireMode.ADD && !input.vehicle.isTireCompatible(base.player.equipment.itemID))
            {
                return false;
            }
            int closestAliveTireIndex = input.vehicle.getClosestAliveTireIndex(input.point, ((ItemTireAsset)base.player.equipment.asset).mode == EUseableTireMode.REMOVE);
            if (closestAliveTireIndex == -1)
            {
                return false;
            }
            if (UseableTire.onModifyTireRequested != null)
            {
                bool shouldAllow = true;
                UseableTire.onModifyTireRequested(this, input.vehicle, closestAliveTireIndex, ref shouldAllow);
                if (!shouldAllow)
                {
                    return false;
                }
            }
            isAttaching = true;
            vehicle = input.vehicle;
            vehicleWheelIndex = closestAliveTireIndex;
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
            attach();
            if (Provider.isServer)
            {
                SendPlayAttach.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GatherRemoteClientConnectionsExcludingOwner());
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
        if (isAttaching && isAttachable)
        {
            isAttaching = false;
            if (vehicle != null && vehicle.isTireReplaceable && vehicleWheelIndex != -1)
            {
                if (((ItemTireAsset)base.player.equipment.asset).mode == EUseableTireMode.ADD)
                {
                    if (!vehicle.tires[vehicleWheelIndex].isAlive)
                    {
                        vehicle.askRepairTire(vehicleWheelIndex);
                        wasSuccessfullyUsed = vehicle.tires[vehicleWheelIndex].isAlive;
                    }
                }
                else if (vehicle.tires[vehicleWheelIndex].isAlive)
                {
                    vehicle.askDamageTire(vehicleWheelIndex);
                    if (!vehicle.tires[vehicleWheelIndex].isAlive)
                    {
                        base.player.inventory.forceAddItem(new Item(vehicle.asset.tireID, full: true), auto: false);
                    }
                }
                vehicle = null;
            }
            if (((ItemTireAsset)base.player.equipment.asset).mode == EUseableTireMode.ADD && Provider.isServer)
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
            if (((ItemTireAsset)base.player.equipment.asset).mode == EUseableTireMode.ADD && Provider.isServer && wasSuccessfullyUsed)
            {
                base.player.equipment.useStepB();
            }
        }
    }
}
