using System;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class Grenade : MonoBehaviour, IExplodableThrowable
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

    public float fuseLength = 2.5f;

    public float explosionLaunchSpeed;

    public bool shouldDestroySelf = true;

    public void Explode()
    {
        ExplosionParameters parameters = new ExplosionParameters(base.transform.position, range, EDeathCause.GRENADE, killer);
        parameters.playerDamage = playerDamage;
        parameters.zombieDamage = zombieDamage;
        parameters.animalDamage = animalDamage;
        parameters.barricadeDamage = barricadeDamage;
        parameters.structureDamage = structureDamage;
        parameters.vehicleDamage = vehicleDamage;
        parameters.resourceDamage = resourceDamage;
        parameters.objectDamage = objectDamage;
        parameters.damageOrigin = EDamageOrigin.Grenade_Explosion;
        parameters.launchSpeed = explosionLaunchSpeed;
        DamageTool.explode(parameters, out var kills);
        EffectAsset effectAsset = Assets.FindEffectAssetByGuidOrLegacyId(explosionEffectGuid, explosion);
        if (effectAsset != null)
        {
            TriggerEffectParameters parameters2 = new TriggerEffectParameters(effectAsset);
            parameters2.position = base.transform.position;
            parameters2.relevantDistance = EffectManager.LARGE;
            parameters2.wasInstigatedByPlayer = true;
            EffectManager.triggerEffect(parameters2);
        }
        if (shouldDestroySelf)
        {
            UnityEngine.Object.Destroy(base.gameObject);
        }
        Player player = PlayerTool.getPlayer(killer);
        if (!(player != null))
        {
            return;
        }
        foreach (EPlayerKill item in kills)
        {
            player.sendStat(item);
        }
    }

    private void Start()
    {
        if (fuseLength >= 0f)
        {
            Invoke("Explode", fuseLength);
        }
    }
}
