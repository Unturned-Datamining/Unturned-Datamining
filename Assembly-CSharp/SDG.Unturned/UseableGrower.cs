using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableGrower : Useable
{
    private float startedUse;

    private float useTime;

    private bool isUsing;

    private InteractableFarm farm;

    private static readonly ClientInstanceMethod SendPlayGrow = ClientInstanceMethod.Get(typeof(UseableGrower), "ReceivePlayGrow");

    private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

    private void grow()
    {
        startedUse = Time.realtimeSinceStartup;
        isUsing = true;
        base.player.animator.play("Use", smooth: false);
        if (Provider.isServer)
        {
            AlertTool.alert(base.transform.position, 8f);
        }
    }

    [Obsolete]
    public void askGrow(CSteamID steamID)
    {
        ReceivePlayGrow();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askGrow")]
    public void ReceivePlayGrow()
    {
        if (base.player.equipment.isEquipped)
        {
            grow();
        }
    }

    private bool fire()
    {
        if (base.channel.isOwner)
        {
            RaycastInfo raycastInfo = DamageTool.raycast(new Ray(base.player.look.aim.position, base.player.look.aim.forward), 3f, RayMasks.DAMAGE_CLIENT);
            if (raycastInfo.transform == null || !raycastInfo.transform.CompareTag("Barricade"))
            {
                return false;
            }
            InteractableFarm component = raycastInfo.transform.GetComponent<InteractableFarm>();
            if (component == null)
            {
                return false;
            }
            if (!component.canFertilize)
            {
                return false;
            }
            if (component.IsFullyGrown)
            {
                return false;
            }
            base.player.input.sendRaycast(raycastInfo, ERaycastInfoUsage.Grower);
        }
        if (Provider.isServer)
        {
            if (!base.player.input.hasInputs())
            {
                return false;
            }
            InputInfo input = base.player.input.getInput(doOcclusionCheck: true, ERaycastInfoUsage.Grower);
            if (input == null)
            {
                return false;
            }
            if ((input.point - base.player.look.aim.position).sqrMagnitude > 49f)
            {
                return false;
            }
            if (input.type != ERaycastInfoType.BARRICADE)
            {
                return false;
            }
            if (input.transform == null || !input.transform.CompareTag("Barricade"))
            {
                return false;
            }
            farm = input.transform.GetComponent<InteractableFarm>();
            if (farm == null)
            {
                return false;
            }
            if (!farm.canFertilize)
            {
                return false;
            }
            if (farm.IsFullyGrown)
            {
                return false;
            }
        }
        return true;
    }

    public override void startPrimary()
    {
        if (!base.player.equipment.isBusy && isUseable && fire())
        {
            base.player.equipment.isBusy = true;
            startedUse = Time.realtimeSinceStartup;
            isUsing = true;
            grow();
            if (Provider.isServer)
            {
                SendPlayGrow.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.EnumerateClients_RemoteNotOwner());
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
            if (farm != null && farm.canFertilize && !farm.IsFullyGrown)
            {
                BarricadeManager.updateFarm(farm.transform, 1u, shouldSend: true);
                base.player.equipment.use();
            }
            else
            {
                base.player.equipment.dequip();
            }
        }
    }
}
