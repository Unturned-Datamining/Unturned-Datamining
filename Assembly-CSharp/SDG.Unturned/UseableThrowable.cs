using System;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class UseableThrowable : Useable
{
    public delegate void ThrowableSpawnedHandler(UseableThrowable useable, GameObject throwable);

    private float startedUse;

    private float useTime;

    private bool isUsing;

    private bool isSwinging;

    private ESwingMode swingMode;

    private static readonly ClientInstanceMethod<Vector3, Vector3> SendToss = ClientInstanceMethod<Vector3, Vector3>.Get(typeof(UseableThrowable), "ReceiveToss");

    private static readonly ClientInstanceMethod SendPlaySwing = ClientInstanceMethod.Get(typeof(UseableThrowable), "ReceivePlaySwing");

    public ItemThrowableAsset equippedThrowableAsset => base.player.equipment.asset as ItemThrowableAsset;

    private bool isUseable => Time.realtimeSinceStartup - startedUse > useTime;

    private bool isThrowable => Time.realtimeSinceStartup - startedUse > useTime * 0.6f;

    public static event ThrowableSpawnedHandler onThrowableSpawned;

    private void toss(Vector3 origin, Vector3 force)
    {
        Transform transform = UnityEngine.Object.Instantiate(equippedThrowableAsset.throwable).transform;
        transform.name = "Throwable";
        EffectManager.RegisterDebris(transform.gameObject);
        transform.position = origin;
        transform.rotation = Quaternion.LookRotation(force);
        Rigidbody component = transform.GetComponent<Rigidbody>();
        if (component != null)
        {
            component.AddForce(force);
            component.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        if (equippedThrowableAsset.isExplosive)
        {
            if (Provider.isServer)
            {
                Grenade grenade = transform.gameObject.AddComponent<Grenade>();
                grenade.killer = base.channel.owner.playerID.steamID;
                grenade.range = equippedThrowableAsset.range;
                grenade.playerDamage = equippedThrowableAsset.playerDamageMultiplier.damage;
                grenade.zombieDamage = equippedThrowableAsset.zombieDamageMultiplier.damage;
                grenade.animalDamage = equippedThrowableAsset.animalDamageMultiplier.damage;
                grenade.barricadeDamage = equippedThrowableAsset.barricadeDamage;
                grenade.structureDamage = equippedThrowableAsset.structureDamage;
                grenade.vehicleDamage = equippedThrowableAsset.vehicleDamage;
                grenade.resourceDamage = equippedThrowableAsset.resourceDamage;
                grenade.objectDamage = equippedThrowableAsset.objectDamage;
                grenade.explosionEffectGuid = equippedThrowableAsset.explosionEffectGuid;
                grenade.explosion = equippedThrowableAsset.explosion;
                grenade.fuseLength = equippedThrowableAsset.fuseLength;
                grenade.explosionLaunchSpeed = equippedThrowableAsset.explosionLaunchSpeed;
            }
            else
            {
                UnityEngine.Object.Destroy(transform.gameObject, equippedThrowableAsset.fuseLength);
            }
        }
        else if (equippedThrowableAsset.isFlash)
        {
            UnityEngine.Object.Destroy(transform.gameObject, equippedThrowableAsset.fuseLength);
        }
        else
        {
            transform.gameObject.AddComponent<Distraction>();
            UnityEngine.Object.Destroy(transform.gameObject, equippedThrowableAsset.fuseLength);
        }
        if (equippedThrowableAsset.isSticky)
        {
            transform.gameObject.AddComponent<StickyGrenade>().ignoreTransform = base.transform;
        }
        if (equippedThrowableAsset.explodeOnImpact && Provider.isServer)
        {
            transform.gameObject.SetLayerRecursively(30);
            ImpactGrenade impactGrenade = transform.gameObject.AddComponent<ImpactGrenade>();
            impactGrenade.explodable = transform.GetComponent<IExplodableThrowable>();
            impactGrenade.ignoreTransform = base.transform;
        }
        Transform transform2 = transform.Find("Smoke");
        if (transform2 != null)
        {
            UnityEngine.Object.Destroy(transform2.gameObject);
        }
        if (UseableThrowable.onThrowableSpawned != null)
        {
            UseableThrowable.onThrowableSpawned(this, transform.gameObject);
        }
    }

    private void swing()
    {
        isSwinging = true;
        base.player.animator.play("Use", smooth: false);
        if (Provider.isServer)
        {
            AlertTool.alert(base.transform.position, 8f);
        }
    }

    [Obsolete]
    public void askToss(CSteamID steamID, Vector3 origin, Vector3 force)
    {
        ReceiveToss(origin, force);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askToss")]
    public void ReceiveToss(Vector3 origin, Vector3 force)
    {
        if (base.player.equipment.isEquipped)
        {
            toss(origin, force);
        }
    }

    [Obsolete]
    public void askSwing(CSteamID steamID)
    {
        ReceivePlaySwing();
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER, legacyName = "askSwing")]
    public void ReceivePlaySwing()
    {
        if (base.player.equipment.isEquipped)
        {
            swing();
        }
    }

    protected void startAttack(ESwingMode newSwingMode)
    {
        if (base.player.equipment.isBusy)
        {
            return;
        }
        base.player.equipment.isBusy = true;
        startedUse = Time.realtimeSinceStartup;
        isUsing = true;
        swingMode = newSwingMode;
        swing();
        if (Provider.isServer)
        {
            if (equippedThrowableAsset.isExplosive)
            {
                base.player.life.markAggressive(force: false);
            }
            SendPlaySwing.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.EnumerateClients_RemoteNotOwner());
        }
    }

    public override void startPrimary()
    {
        startAttack(ESwingMode.STRONG);
    }

    public override void startSecondary()
    {
        startAttack(ESwingMode.WEAK);
    }

    public override void equip()
    {
        base.player.animator.play("Equip", smooth: true);
        useTime = base.player.animator.getAnimationLength("Use");
    }

    public override void tick()
    {
        if (!base.player.equipment.isEquipped || (!base.channel.isOwner && !Provider.isServer) || !isSwinging || !isThrowable)
        {
            return;
        }
        Vector3 position = base.player.look.aim.position;
        Vector3 forward = base.player.look.aim.forward;
        if (Physics.Raycast(new Ray(position, forward), out var hitInfo, 1.5f, RayMasks.DAMAGE_SERVER))
        {
            position += forward * (hitInfo.distance - 0.5f);
        }
        else
        {
            position += forward;
        }
        ESwingMode eSwingMode = swingMode;
        float num = ((eSwingMode == ESwingMode.WEAK || eSwingMode != ESwingMode.STRONG) ? equippedThrowableAsset.weakThrowForce : equippedThrowableAsset.strongThrowForce);
        if (base.player.skills.boost == EPlayerBoost.OLYMPIC)
        {
            num *= equippedThrowableAsset.boostForceMultiplier;
        }
        Vector3 vector = forward * num;
        toss(position, vector);
        if (base.channel.isOwner)
        {
            if (Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Throwables", out int data))
            {
                Provider.provider.statisticsService.userStatisticsService.setStatistic("Found_Throwables", data + 1);
            }
        }
        else
        {
            SendToss.Invoke(GetNetId(), ENetReliability.Unreliable, base.channel.EnumerateClients_RemoteNotOwner(), position, vector);
        }
        if (Provider.isServer)
        {
            base.player.equipment.useStepA();
        }
        isSwinging = false;
    }

    public override void simulate(uint simulation, bool inputSteady)
    {
        if (isUsing && isUseable)
        {
            base.player.equipment.isBusy = false;
            isUsing = false;
            if (Provider.isServer)
            {
                base.player.equipment.useStepB();
            }
        }
    }
}
