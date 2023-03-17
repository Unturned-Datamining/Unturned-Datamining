using System;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class InteractableTrap : Interactable
{
    private float range2;

    private float playerDamage;

    private float zombieDamage;

    private float animalDamage;

    private float barricadeDamage;

    private float structureDamage;

    private float vehicleDamage;

    private float resourceDamage;

    private float objectDamage;

    private float setupDelay = 0.25f;

    private float cooldown;

    private float explosionLaunchSpeed;

    public Guid trapDetonationEffectGuid;

    private ushort explosion2;

    private bool isBroken;

    private bool isExplosive;

    private float lastActive;

    private float lastTriggered;

    public override void updateState(Asset asset, byte[] state)
    {
        ItemTrapAsset itemTrapAsset = (ItemTrapAsset)asset;
        range2 = itemTrapAsset.range2;
        playerDamage = itemTrapAsset.playerDamage;
        zombieDamage = itemTrapAsset.zombieDamage;
        animalDamage = itemTrapAsset.animalDamage;
        barricadeDamage = itemTrapAsset.barricadeDamage;
        structureDamage = itemTrapAsset.structureDamage;
        vehicleDamage = itemTrapAsset.vehicleDamage;
        resourceDamage = itemTrapAsset.resourceDamage;
        objectDamage = itemTrapAsset.objectDamage;
        setupDelay = itemTrapAsset.trapSetupDelay;
        cooldown = itemTrapAsset.trapCooldown;
        trapDetonationEffectGuid = itemTrapAsset.trapDetonationEffectGuid;
        explosion2 = itemTrapAsset.explosion2;
        explosionLaunchSpeed = itemTrapAsset.explosionLaunchSpeed;
        isBroken = itemTrapAsset.isBroken;
        isExplosive = itemTrapAsset.isExplosive;
        if (((ItemTrapAsset)asset).damageTires)
        {
            base.transform.parent.GetOrAddComponent<InteractableTrapDamageTires>();
        }
    }

    public override bool checkInteractable()
    {
        return false;
    }

    private void OnEnable()
    {
        lastActive = Time.realtimeSinceStartup;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || Time.realtimeSinceStartup - lastActive < setupDelay || (base.transform.parent != null && other.transform == base.transform.parent) || Time.realtimeSinceStartup - lastTriggered < cooldown)
        {
            return;
        }
        lastTriggered = Time.realtimeSinceStartup;
        if (!Provider.isServer)
        {
            return;
        }
        if (isExplosive)
        {
            if (!other.transform.CompareTag("Player") || (Provider.isPvP && (other.transform.parent == null || !other.transform.parent.CompareTag("Vehicle"))) || explosionLaunchSpeed > 0.01f)
            {
                Vector3 position = base.transform.position;
                ExplosionParameters parameters = new ExplosionParameters(position, range2, EDeathCause.LANDMINE, CSteamID.Nil);
                parameters.playerDamage = playerDamage;
                parameters.zombieDamage = zombieDamage;
                parameters.animalDamage = animalDamage;
                parameters.barricadeDamage = barricadeDamage;
                parameters.structureDamage = structureDamage;
                parameters.vehicleDamage = vehicleDamage;
                parameters.resourceDamage = resourceDamage;
                parameters.objectDamage = objectDamage;
                parameters.damageOrigin = EDamageOrigin.Trap_Explosion;
                parameters.launchSpeed = explosionLaunchSpeed;
                DamageTool.explode(parameters, out var _);
                EffectAsset effectAsset = Assets.FindEffectAssetByGuidOrLegacyId(trapDetonationEffectGuid, explosion2);
                if (effectAsset != null)
                {
                    TriggerEffectParameters parameters2 = new TriggerEffectParameters(effectAsset);
                    parameters2.position = position;
                    parameters2.relevantDistance = EffectManager.LARGE;
                    EffectManager.triggerEffect(parameters2);
                }
            }
        }
        else if (other.transform.CompareTag("Player"))
        {
            if (!Provider.isPvP || (!(other.transform.parent == null) && other.transform.parent.CompareTag("Vehicle")))
            {
                return;
            }
            Player player = DamageTool.getPlayer(other.transform);
            if (player != null)
            {
                DamageTool.damage(player, EDeathCause.SHRED, ELimb.SPINE, CSteamID.Nil, Vector3.up, playerDamage, 1f, out var _, applyGlobalArmorMultiplier: true, trackKill: true);
                if (isBroken)
                {
                    player.life.breakLegs();
                }
                DamageTool.ServerSpawnLegacyImpact(base.transform.position + Vector3.up, Vector3.down, "Flesh", null, Provider.GatherClientConnectionsWithinSphere(base.transform.position, EffectManager.SMALL));
                BarricadeManager.damage(base.transform.parent, 5f, 1f, armor: false, (player.channel?.owner?.playerID.steamID).GetValueOrDefault(), EDamageOrigin.Trap_Wear_And_Tear);
            }
        }
        else
        {
            if (!other.transform.CompareTag("Agent"))
            {
                return;
            }
            Zombie zombie = DamageTool.getZombie(other.transform);
            if (zombie != null)
            {
                DamageZombieParameters parameters3 = new DamageZombieParameters(zombie, base.transform.forward, zombieDamage);
                parameters3.instigator = this;
                DamageTool.damageZombie(parameters3, out var _, out var _);
                DamageTool.ServerSpawnLegacyImpact(base.transform.position + Vector3.up, Vector3.down, zombie.isRadioactive ? "Alien" : "Flesh", null, Provider.GatherClientConnectionsWithinSphere(base.transform.position, EffectManager.SMALL));
                BarricadeManager.damage(base.transform.parent, zombie.isHyper ? 10f : 5f, 1f, armor: false, default(CSteamID), EDamageOrigin.Trap_Wear_And_Tear);
                return;
            }
            Animal animal = DamageTool.getAnimal(other.transform);
            if (animal != null)
            {
                DamageAnimalParameters parameters4 = new DamageAnimalParameters(animal, base.transform.forward, animalDamage);
                parameters4.instigator = this;
                DamageTool.damageAnimal(parameters4, out var _, out var _);
                DamageTool.ServerSpawnLegacyImpact(base.transform.position + Vector3.up, Vector3.down, "Flesh", null, Provider.GatherClientConnectionsWithinSphere(base.transform.position, EffectManager.SMALL));
                BarricadeManager.damage(base.transform.parent, 5f, 1f, armor: false, default(CSteamID), EDamageOrigin.Trap_Wear_And_Tear);
            }
        }
    }
}
