using System;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

public class UseableVehiclePaint : Useable
{
    private float startedUse;

    private float useTime;

    private bool isUsing;

    private bool isReplacing;

    private InteractableVehicle vehicle;

    private static readonly ClientInstanceMethod SendPlayReplace = ClientInstanceMethod.Get(typeof(UseableVehiclePaint), "ReceivePlayReplace");

    private bool wasSuccessfullyUsed;

    private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

    private bool isReplaceable => Time.realtimeSinceStartup - startedUse > useTime * 0.85f;

    public static event PaintVehicleRequestHandler OnPaintVehicleRequested;

    private void replace()
    {
        startedUse = Time.realtimeSinceStartup;
        isUsing = true;
        base.player.animator.play("Use", smooth: false);
        if (!Dedicator.IsDedicatedServer)
        {
            base.player.playSound(((ItemToolAsset)base.player.equipment.asset).use, 1f, 0.01f);
        }
        if (Provider.isServer)
        {
            AlertTool.alert(base.transform.position, 8f);
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public void ReceivePlayReplace()
    {
        if (base.player.equipment.IsEquipAnimationFinished)
        {
            replace();
        }
    }

    private bool IsVehicleAlreadySameColorAsPaint(InteractableVehicle vehicle)
    {
        if (base.player.equipment.asset is ItemVehiclePaintToolAsset { PaintColor: var paintColor })
        {
            paintColor.a = byte.MaxValue;
            return vehicle.PaintColor.Equals(paintColor);
        }
        return false;
    }

    private bool fire()
    {
        if (base.channel.IsLocalPlayer)
        {
            RaycastInfo raycastInfo = DamageTool.raycast(new Ray(base.player.look.aim.position, base.player.look.aim.forward), 3f, RayMasks.DAMAGE_CLIENT);
            if (raycastInfo.vehicle == null)
            {
                return false;
            }
            if (!raycastInfo.vehicle.asset.SupportsPaintColor)
            {
                PlayerUI.message(EPlayerMessage.NOT_PAINTABLE, string.Empty);
                return false;
            }
            if (!raycastInfo.vehicle.checkEnter(base.player))
            {
                return false;
            }
            if (IsVehicleAlreadySameColorAsPaint(raycastInfo.vehicle))
            {
                return false;
            }
            base.player.input.sendRaycast(raycastInfo, ERaycastInfoUsage.Paint);
        }
        if (Provider.isServer)
        {
            if (!base.player.input.hasInputs())
            {
                return false;
            }
            InputInfo input = base.player.input.getInput(doOcclusionCheck: true, ERaycastInfoUsage.Paint);
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
            if (input.vehicle == null || !input.vehicle.asset.SupportsPaintColor)
            {
                return false;
            }
            if (!input.vehicle.checkEnter(base.player))
            {
                return false;
            }
            if (IsVehicleAlreadySameColorAsPaint(input.vehicle))
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
            if (vehicle != null && vehicle.asset.SupportsPaintColor && vehicle.checkEnter(base.player))
            {
                Color32 desiredColor = new Color32(0, 0, 0, byte.MaxValue);
                if (base.player.equipment.asset is ItemVehiclePaintToolAsset itemVehiclePaintToolAsset)
                {
                    desiredColor = itemVehiclePaintToolAsset.PaintColor;
                    desiredColor.a = byte.MaxValue;
                }
                else
                {
                    desiredColor.r = (byte)UnityEngine.Random.Range(0, 256);
                    desiredColor.g = (byte)UnityEngine.Random.Range(0, 256);
                    desiredColor.b = (byte)UnityEngine.Random.Range(0, 256);
                }
                bool shouldAllow = true;
                try
                {
                    UseableVehiclePaint.OnPaintVehicleRequested?.Invoke(vehicle, base.player, ref shouldAllow, ref desiredColor);
                }
                catch (Exception e)
                {
                    UnturnedLog.exception(e, "Caught exception invoking OnPaintVehicleRequested:");
                }
                if (shouldAllow)
                {
                    vehicle.ServerSetPaintColor(desiredColor);
                    wasSuccessfullyUsed = true;
                }
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
