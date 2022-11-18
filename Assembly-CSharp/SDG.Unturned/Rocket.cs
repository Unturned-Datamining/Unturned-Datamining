using System;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class Rocket : MonoBehaviour
{
    public CSteamID killer;

    public float range;

    public float playerDamage;

    public float zombieDamage;

    public float animalDamage;

    public float barricadeDamage;

    public float structureDamage;

    public float vehicleDamage;

    public float resourceDamage;

    public float objectDamage;

    public Guid explosionEffectGuid;

    public ushort explosion;

    public bool penetrateBuildables;

    public Transform ignoreTransform;

    public ERagdollEffect ragdollEffect;

    public float explosionLaunchSpeed;

    private bool isExploded;

    private Vector3 lastPos;

    private void OnTriggerEnter(Collider other)
    {
        if (isExploded || other.isTrigger || (ignoreTransform != null && (other.transform == ignoreTransform || other.transform.IsChildOf(ignoreTransform))))
        {
            return;
        }
        isExploded = true;
        if (Provider.isServer)
        {
            ExplosionParameters parameters = new ExplosionParameters(lastPos, range, EDeathCause.MISSILE, killer);
            parameters.playerDamage = playerDamage;
            parameters.zombieDamage = zombieDamage;
            parameters.animalDamage = animalDamage;
            parameters.barricadeDamage = barricadeDamage;
            parameters.structureDamage = structureDamage;
            parameters.vehicleDamage = vehicleDamage;
            parameters.resourceDamage = resourceDamage;
            parameters.objectDamage = objectDamage;
            parameters.damageOrigin = EDamageOrigin.Rocket_Explosion;
            parameters.penetrateBuildables = penetrateBuildables;
            parameters.ragdollEffect = ragdollEffect;
            parameters.launchSpeed = explosionLaunchSpeed;
            DamageTool.explode(parameters, out var kills);
            TriggerEffectParameters parameters2 = new TriggerEffectParameters(Assets.FindEffectAssetByGuidOrLegacyId(explosionEffectGuid, explosion));
            parameters2.position = lastPos;
            parameters2.relevantDistance = EffectManager.LARGE;
            parameters2.wasInstigatedByPlayer = true;
            EffectManager.triggerEffect(parameters2);
            Player player = PlayerTool.getPlayer(killer);
            if (player != null)
            {
                foreach (EPlayerKill item in kills)
                {
                    player.sendStat(item);
                }
            }
        }
        UnityEngine.Object.Destroy(base.gameObject);
    }

    private void FixedUpdate()
    {
        lastPos = base.transform.position;
    }

    private void Awake()
    {
        lastPos = base.transform.position;
    }
}
