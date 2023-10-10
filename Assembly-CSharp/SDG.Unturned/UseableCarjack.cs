using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableCarjack : Useable
{
    private float startedUse;

    private float useTime;

    private bool isUsing;

    private bool isJacking;

    private InteractableVehicle vehicle;

    private static readonly ClientInstanceMethod SendPlayPull = ClientInstanceMethod.Get(typeof(UseableCarjack), "ReceivePlayPull");

    private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

    private bool isJackable => Time.realtimeSinceStartup - startedUse > useTime * 0.75f;

    private void pull()
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
    public void askPull(CSteamID steamID)
    {
        ReceivePlayPull();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askPull")]
    public void ReceivePlayPull()
    {
        if (base.player.equipment.IsEquipAnimationFinished)
        {
            pull();
        }
    }

    private bool isVehicleValid(InteractableVehicle testVehicle)
    {
        if (!testVehicle.isEmpty)
        {
            return false;
        }
        if (base.channel.owner.isAdmin)
        {
            return true;
        }
        if (base.player.movement.isSafe && base.player.movement.isSafeInfo != null && base.player.movement.isSafeInfo.noWeapons)
        {
            return testVehicle.checkEnter(base.player);
        }
        return true;
    }

    private bool fire()
    {
        if (base.channel.IsLocalPlayer)
        {
            RaycastInfo raycastInfo = DamageTool.raycast(new Ray(base.player.look.aim.position, base.player.look.aim.forward), 3f, RayMasks.DAMAGE_CLIENT);
            if (raycastInfo.vehicle == null || !isVehicleValid(raycastInfo.vehicle))
            {
                return false;
            }
            base.player.input.sendRaycast(raycastInfo, ERaycastInfoUsage.Carjack);
        }
        if (Provider.isServer)
        {
            if (!base.player.input.hasInputs())
            {
                return false;
            }
            InputInfo input = base.player.input.getInput(doOcclusionCheck: true, ERaycastInfoUsage.Carjack);
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
            if (input.vehicle == null || !isVehicleValid(input.vehicle))
            {
                return false;
            }
            isJacking = true;
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
            pull();
            if (Provider.isServer)
            {
                SendPlayPull.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.GatherRemoteClientConnectionsExcludingOwner());
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
        if (isJacking && isJackable)
        {
            isJacking = false;
            if (vehicle != null && isVehicleValid(vehicle))
            {
                Vector3 force = new Vector3(UnityEngine.Random.Range(-32f, 32f), UnityEngine.Random.Range(480f, 544f) * (float)((base.player.skills.boost != EPlayerBoost.FLIGHT) ? 1 : 4), UnityEngine.Random.Range(-32f, 32f));
                VehicleManager.carjackVehicle(torque: new Vector3(UnityEngine.Random.Range(-64f, 64f), UnityEngine.Random.Range(-64f, 64f), UnityEngine.Random.Range(-64f, 64f)), vehicle: vehicle, instigatingPlayer: base.player, force: force);
                vehicle = null;
            }
        }
        if (isUsing && isUseable)
        {
            base.player.equipment.isBusy = false;
            isUsing = false;
        }
    }
}
