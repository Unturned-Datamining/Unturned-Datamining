using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableFuel : Useable
{
    private enum EUseMode
    {
        /// <summary>
        /// Add fuel to target.
        /// </summary>
        Deposit,
        /// <summary>
        /// Remove fuel from target.
        /// </summary>
        Withdraw
    }

    private float startedUse;

    private float useTime;

    private bool isUsing;

    private bool shouldDeleteAfterUse;

    private ushort fuel;

    private static readonly ClientInstanceMethod SendPlayGlug = ClientInstanceMethod.Get(typeof(UseableFuel), "ReceivePlayGlug");

    private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

    private void glug()
    {
        startedUse = Time.realtimeSinceStartup;
        isUsing = true;
        base.player.animator.play("Use", smooth: false);
        if (!Dedicator.IsDedicatedServer)
        {
            base.player.playSound(((ItemFuelAsset)base.player.equipment.asset).use);
        }
        if (Provider.isServer)
        {
            AlertTool.alert(base.transform.position, 8f);
        }
    }

    [Obsolete]
    public void askGlug(CSteamID steamID)
    {
        ReceivePlayGlug();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askGlug")]
    public void ReceivePlayGlug()
    {
        if (base.player.equipment.IsEquipAnimationFinished)
        {
            glug();
        }
    }

    private bool fire(EUseMode mode)
    {
        if (base.channel.IsLocalPlayer)
        {
            RaycastInfo raycastInfo = DamageTool.raycast(new Ray(base.player.look.aim.position, base.player.look.aim.forward), 3f, RayMasks.DAMAGE_CLIENT);
            if (raycastInfo.vehicle != null)
            {
                if (!raycastInfo.vehicle.checkEnter(base.player))
                {
                    return false;
                }
                if (mode == EUseMode.Deposit)
                {
                    if (fuel == 0)
                    {
                        return false;
                    }
                    if (!raycastInfo.vehicle.isRefillable)
                    {
                        return false;
                    }
                }
                else
                {
                    if (fuel == ((ItemFuelAsset)base.player.equipment.asset).fuel)
                    {
                        return false;
                    }
                    if (!raycastInfo.vehicle.isSiphonable)
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (!(raycastInfo.transform != null))
                {
                    return false;
                }
                InteractableGenerator component = raycastInfo.transform.GetComponent<InteractableGenerator>();
                InteractableOil component2 = raycastInfo.transform.GetComponent<InteractableOil>();
                InteractableTank component3 = raycastInfo.transform.GetComponent<InteractableTank>();
                InteractableObjectResource component4 = raycastInfo.transform.GetComponent<InteractableObjectResource>();
                if (component != null)
                {
                    if (mode == EUseMode.Deposit)
                    {
                        if (fuel == 0)
                        {
                            return false;
                        }
                        if (!component.isRefillable)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (fuel == ((ItemFuelAsset)base.player.equipment.asset).fuel)
                        {
                            return false;
                        }
                        if (!component.isSiphonable)
                        {
                            return false;
                        }
                    }
                }
                else if (!(component2 != null))
                {
                    if (component3 != null)
                    {
                        if (component3.source != ETankSource.FUEL)
                        {
                            return false;
                        }
                        if (mode == EUseMode.Deposit)
                        {
                            if (fuel == 0)
                            {
                                return false;
                            }
                            if (!component3.isRefillable)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (fuel == ((ItemFuelAsset)base.player.equipment.asset).fuel)
                            {
                                return false;
                            }
                            if (!component3.isSiphonable)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (!(component4 != null))
                        {
                            return false;
                        }
                        if (component4.objectAsset.interactability != EObjectInteractability.FUEL)
                        {
                            return false;
                        }
                        if (mode == EUseMode.Deposit)
                        {
                            if (fuel == 0)
                            {
                                return false;
                            }
                            if (component4.amount == component4.capacity)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (fuel == ((ItemFuelAsset)base.player.equipment.asset).fuel)
                            {
                                return false;
                            }
                            if (component4.amount == 0)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            base.player.input.sendRaycast(raycastInfo, ERaycastInfoUsage.Fuel);
        }
        if (Provider.isServer)
        {
            if (!base.player.input.hasInputs())
            {
                return false;
            }
            InputInfo input = base.player.input.getInput(doOcclusionCheck: true, ERaycastInfoUsage.Fuel);
            if (input == null)
            {
                return false;
            }
            if ((input.point - base.player.look.aim.position).sqrMagnitude > 49f)
            {
                return false;
            }
            if (input.type == ERaycastInfoType.VEHICLE)
            {
                if (input.vehicle == null)
                {
                    return false;
                }
                if (!input.vehicle.checkEnter(base.player))
                {
                    return false;
                }
                if (mode == EUseMode.Deposit)
                {
                    if (fuel == 0)
                    {
                        return false;
                    }
                    if (!input.vehicle.isRefillable)
                    {
                        return false;
                    }
                    ushort num = (ushort)Mathf.Min(fuel, input.vehicle.asset.fuel - input.vehicle.fuel);
                    input.vehicle.askFillFuel(num);
                    fuel -= num;
                }
                else
                {
                    if (fuel == ((ItemFuelAsset)base.player.equipment.asset).fuel)
                    {
                        return false;
                    }
                    if (!input.vehicle.isSiphonable)
                    {
                        return false;
                    }
                    ushort desiredAmount = (ushort)(((ItemFuelAsset)base.player.equipment.asset).fuel - fuel);
                    fuel += VehicleManager.siphonFromVehicle(input.vehicle, base.player, desiredAmount);
                }
            }
            else if (input.type == ERaycastInfoType.BARRICADE)
            {
                if (input.transform == null || !input.transform.CompareTag("Barricade"))
                {
                    return false;
                }
                InteractableGenerator component5 = input.transform.GetComponent<InteractableGenerator>();
                InteractableOil component6 = input.transform.GetComponent<InteractableOil>();
                InteractableTank component7 = input.transform.GetComponent<InteractableTank>();
                if (component5 != null)
                {
                    if (mode == EUseMode.Deposit)
                    {
                        if (fuel == 0)
                        {
                            return false;
                        }
                        if (!component5.isRefillable)
                        {
                            return false;
                        }
                        ushort num2 = (ushort)Mathf.Min(fuel, component5.capacity - component5.fuel);
                        component5.askFill(num2);
                        BarricadeManager.sendFuel(input.transform, component5.fuel);
                        fuel -= num2;
                    }
                    else
                    {
                        if (fuel == ((ItemFuelAsset)base.player.equipment.asset).fuel)
                        {
                            return false;
                        }
                        if (!component5.isSiphonable)
                        {
                            return false;
                        }
                        ushort num3 = (ushort)Mathf.Min(component5.fuel, ((ItemFuelAsset)base.player.equipment.asset).fuel - fuel);
                        component5.askBurn(num3);
                        BarricadeManager.sendFuel(input.transform, component5.fuel);
                        fuel += num3;
                    }
                }
                else if (component6 != null)
                {
                    if (mode == EUseMode.Deposit)
                    {
                        if (fuel == 0)
                        {
                            return false;
                        }
                        if (!component6.isRefillable)
                        {
                            return false;
                        }
                        ushort num4 = (ushort)Mathf.Min(fuel, component6.capacity - component6.fuel);
                        component6.askFill(num4);
                        BarricadeManager.sendOil(input.transform, component6.fuel);
                        fuel -= num4;
                    }
                    else
                    {
                        if (fuel == ((ItemFuelAsset)base.player.equipment.asset).fuel)
                        {
                            return false;
                        }
                        if (!component6.isSiphonable)
                        {
                            return false;
                        }
                        ushort num5 = (ushort)Mathf.Min(component6.fuel, ((ItemFuelAsset)base.player.equipment.asset).fuel - fuel);
                        component6.askBurn(num5);
                        BarricadeManager.sendOil(input.transform, component6.fuel);
                        fuel += num5;
                    }
                }
                else
                {
                    if (!(component7 != null))
                    {
                        return false;
                    }
                    if (component7.source != ETankSource.FUEL)
                    {
                        return false;
                    }
                    if (mode == EUseMode.Deposit)
                    {
                        if (fuel == 0)
                        {
                            return false;
                        }
                        if (!component7.isRefillable)
                        {
                            return false;
                        }
                        ushort num6 = (ushort)Mathf.Min(fuel, component7.capacity - component7.amount);
                        component7.ServerSetAmount((ushort)(component7.amount + num6));
                        fuel -= num6;
                    }
                    else
                    {
                        if (fuel == ((ItemFuelAsset)base.player.equipment.asset).fuel)
                        {
                            return false;
                        }
                        if (!component7.isSiphonable)
                        {
                            return false;
                        }
                        ushort num7 = (ushort)Mathf.Min(component7.amount, ((ItemFuelAsset)base.player.equipment.asset).fuel - fuel);
                        component7.ServerSetAmount((ushort)(component7.amount - num7));
                        fuel += num7;
                    }
                }
            }
            else if (input.type == ERaycastInfoType.OBJECT)
            {
                if (input.transform == null)
                {
                    return false;
                }
                InteractableObjectResource component8 = input.transform.GetComponent<InteractableObjectResource>();
                if (component8 == null || component8.objectAsset.interactability != EObjectInteractability.FUEL)
                {
                    return false;
                }
                if (mode == EUseMode.Deposit)
                {
                    if (fuel == 0)
                    {
                        return false;
                    }
                    if (!component8.isRefillable)
                    {
                        return false;
                    }
                    ushort num8 = (ushort)Mathf.Min(fuel, component8.capacity - component8.amount);
                    ObjectManager.updateObjectResource(component8.transform, (ushort)(component8.amount + num8), shouldSend: true);
                    fuel -= num8;
                }
                else
                {
                    if (fuel == ((ItemFuelAsset)base.player.equipment.asset).fuel)
                    {
                        return false;
                    }
                    if (!component8.isSiphonable)
                    {
                        return false;
                    }
                    ushort num9 = (ushort)Mathf.Min(component8.amount, ((ItemFuelAsset)base.player.equipment.asset).fuel - fuel);
                    ObjectManager.updateObjectResource(component8.transform, (ushort)(component8.amount - num9), shouldSend: true);
                    fuel += num9;
                }
            }
        }
        return true;
    }

    private bool start(EUseMode mode)
    {
        if (base.player.equipment.isBusy)
        {
            return false;
        }
        if (isUseable && fire(mode))
        {
            if (Provider.isServer)
            {
                byte[] bytes = BitConverter.GetBytes(fuel);
                base.player.equipment.state[0] = bytes[0];
                base.player.equipment.state[1] = bytes[1];
                base.player.equipment.sendUpdateState();
            }
            base.player.equipment.isBusy = true;
            startedUse = Time.realtimeSinceStartup;
            isUsing = true;
            glug();
            if (Provider.isServer)
            {
                SendPlayGlug.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GatherRemoteClientConnectionsExcludingOwner());
                ItemFuelAsset itemFuelAsset = base.player.equipment.asset as ItemFuelAsset;
                if (mode == EUseMode.Deposit && itemFuelAsset != null && itemFuelAsset.shouldDeleteAfterFillingTarget)
                {
                    shouldDeleteAfterUse = true;
                }
            }
            return true;
        }
        return false;
    }

    public override bool startPrimary()
    {
        return start(EUseMode.Deposit);
    }

    public override bool startSecondary()
    {
        return start(EUseMode.Withdraw);
    }

    public override void updateState(byte[] newState)
    {
        if (base.channel.IsLocalPlayer)
        {
            fuel = BitConverter.ToUInt16(newState, 0);
            PlayerUI.message(EPlayerMessage.FUEL, ((int)((float)(int)fuel / (float)(int)((ItemFuelAsset)base.player.equipment.asset).fuel * 100f)).ToString());
        }
    }

    public override void equip()
    {
        if (base.channel.IsLocalPlayer || Provider.isServer)
        {
            if (base.player.equipment.state.Length < 2)
            {
                base.player.equipment.state = ((ItemFuelAsset)base.player.equipment.asset).getState(EItemOrigin.ADMIN);
                base.player.equipment.updateState();
            }
            fuel = BitConverter.ToUInt16(base.player.equipment.state, 0);
        }
        base.player.animator.play("Equip", smooth: true);
        useTime = base.player.animator.GetAnimationLength("Use");
        if (base.channel.IsLocalPlayer)
        {
            PlayerUI.message(EPlayerMessage.FUEL, ((int)((float)(int)fuel / (float)(int)((ItemFuelAsset)base.player.equipment.asset).fuel * 100f)).ToString());
        }
    }

    public override void simulate(uint simulation, bool inputSteady)
    {
        if (isUsing && isUseable)
        {
            base.player.equipment.isBusy = false;
            isUsing = false;
            if (Provider.isServer && shouldDeleteAfterUse)
            {
                base.player.equipment.use();
            }
        }
    }
}
