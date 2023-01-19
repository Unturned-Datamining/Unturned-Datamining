using System;
using SDG.Framework.Water;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableRefill : Useable
{
    private float startedUse;

    private float useTime;

    private float refillTime;

    private bool isUsing;

    private ERefillMode refillMode;

    private ERefillWaterType refillWaterType;

    private static readonly ClientInstanceMethod SendPlayUse = ClientInstanceMethod.Get(typeof(UseableRefill), "ReceivePlayUse");

    private static readonly ClientInstanceMethod SendPlayRefill = ClientInstanceMethod.Get(typeof(UseableRefill), "ReceivePlayRefill");

    private bool isUseable
    {
        get
        {
            if (refillMode == ERefillMode.USE)
            {
                return Time.realtimeSinceStartup - startedUse > useTime;
            }
            if (refillMode == ERefillMode.REFILL)
            {
                return Time.realtimeSinceStartup - startedUse > refillTime;
            }
            return false;
        }
    }

    private ERefillWaterType waterType
    {
        get
        {
            if (base.player.equipment.state != null && base.player.equipment.state.Length != 0)
            {
                return (ERefillWaterType)base.player.equipment.state[0];
            }
            return ERefillWaterType.EMPTY;
        }
    }

    private void use()
    {
        startedUse = Time.realtimeSinceStartup;
        isUsing = true;
        base.player.animator.play("Use", smooth: false);
        if (!Dedicator.IsDedicatedServer)
        {
            base.player.playSound(((ItemRefillAsset)base.player.equipment.asset).use);
        }
        if (Provider.isServer)
        {
            AlertTool.alert(base.transform.position, 8f);
        }
    }

    [Obsolete]
    public void askUse(CSteamID steamID)
    {
        ReceivePlayUse();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askUse")]
    public void ReceivePlayUse()
    {
        if (base.player.equipment.isEquipped)
        {
            use();
        }
    }

    private void refill()
    {
        startedUse = Time.realtimeSinceStartup;
        isUsing = true;
        base.player.animator.play("Refill", smooth: false);
    }

    [Obsolete]
    public void askRefill(CSteamID steamID)
    {
        ReceivePlayRefill();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askRefill")]
    public void ReceivePlayRefill()
    {
        if (base.player.equipment.isEquipped)
        {
            refill();
        }
    }

    private bool fire(bool mode, out ERefillWaterType newWaterType)
    {
        newWaterType = ERefillWaterType.EMPTY;
        if (base.channel.isOwner)
        {
            RaycastInfo raycastInfo = DamageTool.raycast(new Ray(base.player.look.aim.position, base.player.look.aim.forward), 3f, RayMasks.DAMAGE_CLIENT);
            if (!(raycastInfo.transform != null))
            {
                return false;
            }
            InteractableRainBarrel component = raycastInfo.transform.GetComponent<InteractableRainBarrel>();
            InteractableTank component2 = raycastInfo.transform.GetComponent<InteractableTank>();
            InteractableObjectResource component3 = raycastInfo.transform.GetComponent<InteractableObjectResource>();
            if (WaterUtility.isPointUnderwater(raycastInfo.point, out var volume))
            {
                if (mode)
                {
                    return false;
                }
                if (waterType != 0)
                {
                    return false;
                }
                if (volume == null)
                {
                    newWaterType = ERefillWaterType.SALTY;
                }
                else
                {
                    newWaterType = volume.waterType;
                }
            }
            else if (component != null)
            {
                if (mode)
                {
                    if (waterType != ERefillWaterType.CLEAN)
                    {
                        return false;
                    }
                    if (component.isFull)
                    {
                        return false;
                    }
                    newWaterType = ERefillWaterType.EMPTY;
                }
                else
                {
                    if (waterType == ERefillWaterType.CLEAN)
                    {
                        return false;
                    }
                    if (!component.isFull)
                    {
                        return false;
                    }
                    newWaterType = ERefillWaterType.CLEAN;
                }
            }
            else if (component2 != null)
            {
                if (component2.source != ETankSource.WATER)
                {
                    return false;
                }
                if (mode)
                {
                    if (waterType != ERefillWaterType.CLEAN)
                    {
                        return false;
                    }
                    if (component2.amount == component2.capacity)
                    {
                        return false;
                    }
                    newWaterType = ERefillWaterType.EMPTY;
                }
                else
                {
                    if (waterType == ERefillWaterType.CLEAN)
                    {
                        return false;
                    }
                    if (component2.amount == 0)
                    {
                        return false;
                    }
                    newWaterType = ERefillWaterType.CLEAN;
                }
            }
            else
            {
                if (!(component3 != null))
                {
                    return false;
                }
                if (component3.objectAsset.interactability != EObjectInteractability.WATER)
                {
                    return false;
                }
                if (mode)
                {
                    if (waterType == ERefillWaterType.EMPTY)
                    {
                        return false;
                    }
                    if (component3.amount == component3.capacity)
                    {
                        return false;
                    }
                    newWaterType = ERefillWaterType.EMPTY;
                }
                else
                {
                    if (waterType == ERefillWaterType.CLEAN || waterType == ERefillWaterType.DIRTY)
                    {
                        return false;
                    }
                    if (component3.amount == 0)
                    {
                        return false;
                    }
                    newWaterType = ERefillWaterType.DIRTY;
                }
            }
            base.player.input.sendRaycast(raycastInfo, ERaycastInfoUsage.Refill);
        }
        if (Provider.isServer)
        {
            if (!base.player.input.hasInputs())
            {
                return false;
            }
            InputInfo input = base.player.input.getInput(doOcclusionCheck: true, ERaycastInfoUsage.Refill);
            if (input == null)
            {
                return false;
            }
            if ((input.point - base.player.look.aim.position).sqrMagnitude > 49f)
            {
                return false;
            }
            if (WaterUtility.isPointUnderwater(input.point, out var volume2))
            {
                if (mode)
                {
                    return false;
                }
                if (waterType != 0)
                {
                    return false;
                }
                if (volume2 == null)
                {
                    newWaterType = ERefillWaterType.SALTY;
                }
                else
                {
                    newWaterType = volume2.waterType;
                }
            }
            else if (input.type == ERaycastInfoType.BARRICADE)
            {
                if (input.transform == null || !input.transform.CompareTag("Barricade"))
                {
                    return false;
                }
                InteractableRainBarrel component4 = input.transform.GetComponent<InteractableRainBarrel>();
                InteractableTank component5 = input.transform.GetComponent<InteractableTank>();
                if (component4 != null)
                {
                    if (mode)
                    {
                        if (waterType != ERefillWaterType.CLEAN)
                        {
                            return false;
                        }
                        if (component4.isFull)
                        {
                            return false;
                        }
                        BarricadeManager.updateRainBarrel(component4.transform, isFull: true, shouldSend: true);
                        newWaterType = ERefillWaterType.EMPTY;
                    }
                    else
                    {
                        if (waterType == ERefillWaterType.CLEAN)
                        {
                            return false;
                        }
                        if (!component4.isFull)
                        {
                            return false;
                        }
                        BarricadeManager.updateRainBarrel(component4.transform, isFull: false, shouldSend: true);
                        newWaterType = ERefillWaterType.CLEAN;
                    }
                }
                else
                {
                    if (!(component5 != null))
                    {
                        return false;
                    }
                    if (component5.source != ETankSource.WATER)
                    {
                        return false;
                    }
                    if (mode)
                    {
                        if (waterType != ERefillWaterType.CLEAN)
                        {
                            return false;
                        }
                        if (component5.amount == component5.capacity)
                        {
                            return false;
                        }
                        component5.ServerSetAmount((ushort)(component5.amount + 1));
                        newWaterType = ERefillWaterType.EMPTY;
                    }
                    else
                    {
                        if (waterType == ERefillWaterType.CLEAN)
                        {
                            return false;
                        }
                        if (component5.amount == 0)
                        {
                            return false;
                        }
                        component5.ServerSetAmount((ushort)(component5.amount - 1));
                        newWaterType = ERefillWaterType.CLEAN;
                    }
                }
            }
            else if (input.type == ERaycastInfoType.OBJECT)
            {
                if (input.transform == null)
                {
                    return false;
                }
                InteractableObjectResource component6 = input.transform.GetComponent<InteractableObjectResource>();
                if (component6 == null || component6.objectAsset.interactability != EObjectInteractability.WATER)
                {
                    return false;
                }
                if (mode)
                {
                    if (waterType == ERefillWaterType.EMPTY)
                    {
                        return false;
                    }
                    if (component6.amount == component6.capacity)
                    {
                        return false;
                    }
                    ObjectManager.updateObjectResource(component6.transform, (byte)(component6.amount + 1), shouldSend: true);
                    newWaterType = ERefillWaterType.EMPTY;
                }
                else
                {
                    if (waterType == ERefillWaterType.CLEAN || waterType == ERefillWaterType.DIRTY)
                    {
                        return false;
                    }
                    if (component6.amount == 0)
                    {
                        return false;
                    }
                    ObjectManager.updateObjectResource(component6.transform, (byte)(component6.amount - 1), shouldSend: true);
                    newWaterType = ERefillWaterType.DIRTY;
                }
            }
        }
        return true;
    }

    private void msg()
    {
        PlayerUI.message(waterType switch
        {
            ERefillWaterType.EMPTY => EPlayerMessage.EMPTY, 
            ERefillWaterType.CLEAN => EPlayerMessage.CLEAN, 
            ERefillWaterType.SALTY => EPlayerMessage.SALTY, 
            ERefillWaterType.DIRTY => EPlayerMessage.DIRTY, 
            _ => EPlayerMessage.FULL, 
        }, "");
    }

    private void start(ERefillWaterType newWaterType)
    {
        base.player.equipment.isBusy = true;
        startedUse = Time.realtimeSinceStartup;
        isUsing = true;
        refillMode = ERefillMode.REFILL;
        refill();
        base.player.equipment.quality = (byte)((newWaterType != ERefillWaterType.DIRTY) ? 100u : 0u);
        base.player.equipment.updateQuality();
        base.player.equipment.state[0] = (byte)newWaterType;
        base.player.equipment.updateState();
        if (Provider.isServer)
        {
            SendPlayRefill.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.EnumerateClients_RemoteNotOwner());
        }
        if (base.channel.isOwner)
        {
            msg();
        }
    }

    public override void startPrimary()
    {
        if (base.player.equipment.isBusy || !isUseable)
        {
            return;
        }
        if (fire(mode: true, out var newWaterType))
        {
            start(newWaterType);
        }
        else if (waterType != 0)
        {
            base.player.equipment.isBusy = true;
            startedUse = Time.realtimeSinceStartup;
            isUsing = true;
            refillMode = ERefillMode.USE;
            refillWaterType = waterType;
            use();
            base.player.equipment.quality = (byte)((newWaterType != ERefillWaterType.DIRTY) ? 100u : 0u);
            base.player.equipment.updateQuality();
            base.player.equipment.state[0] = 0;
            base.player.equipment.updateState();
            if (Provider.isServer)
            {
                SendPlayUse.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.EnumerateClients_RemoteNotOwner());
            }
            if (base.channel.isOwner)
            {
                msg();
            }
        }
    }

    public override void startSecondary()
    {
        if (!base.player.equipment.isBusy && isUseable && fire(mode: false, out var newWaterType))
        {
            start(newWaterType);
        }
    }

    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
        useTime = base.player.animator.getAnimationLength("Use");
        refillTime = base.player.animator.getAnimationLength("Refill");
        if (base.channel.isOwner)
        {
            msg();
        }
    }

    public override void simulate(uint simulation, bool inputSteady)
    {
        if (!isUsing || !isUseable)
        {
            return;
        }
        base.player.equipment.isBusy = false;
        isUsing = false;
        if (refillMode != 0)
        {
            return;
        }
        ItemRefillAsset itemRefillAsset = base.player.equipment.asset as ItemRefillAsset;
        float delta;
        float delta2;
        switch (refillWaterType)
        {
        case ERefillWaterType.CLEAN:
            delta = itemRefillAsset.cleanStamina;
            delta2 = itemRefillAsset.cleanOxygen;
            break;
        case ERefillWaterType.SALTY:
            delta = itemRefillAsset.saltyStamina;
            delta2 = itemRefillAsset.saltyOxygen;
            break;
        case ERefillWaterType.DIRTY:
            delta = itemRefillAsset.dirtyStamina;
            delta2 = itemRefillAsset.dirtyOxygen;
            break;
        default:
            delta = 0f;
            delta2 = 0f;
            break;
        }
        base.player.life.simulatedModifyStamina(delta);
        base.player.life.simulatedModifyOxygen(delta2);
        if (Provider.isServer)
        {
            float delta3;
            float delta4;
            float delta5;
            float delta6;
            switch (refillWaterType)
            {
            case ERefillWaterType.CLEAN:
                delta3 = itemRefillAsset.cleanHealth;
                delta4 = itemRefillAsset.cleanFood;
                delta5 = itemRefillAsset.cleanWater;
                delta6 = itemRefillAsset.cleanVirus;
                break;
            case ERefillWaterType.SALTY:
                delta3 = itemRefillAsset.saltyHealth;
                delta4 = itemRefillAsset.saltyFood;
                delta5 = itemRefillAsset.saltyWater;
                delta6 = itemRefillAsset.saltyVirus;
                break;
            case ERefillWaterType.DIRTY:
                delta3 = itemRefillAsset.dirtyHealth;
                delta4 = itemRefillAsset.dirtyFood;
                delta5 = itemRefillAsset.dirtyWater;
                delta6 = itemRefillAsset.dirtyVirus;
                break;
            default:
                delta3 = 0f;
                delta4 = 0f;
                delta5 = 0f;
                delta6 = 0f;
                break;
            }
            base.player.life.serverModifyHealth(delta3);
            base.player.life.serverModifyFood(delta4);
            base.player.life.serverModifyWater(delta5);
            base.player.life.serverModifyVirus(delta6);
        }
    }
}
