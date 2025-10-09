using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Can be added to any GameObject with a Trigger collider.
/// Ensure that Layer will detect player overlaps. Trap is a good candidate.
/// </summary>
[AddComponentMenu("Unturned/Collision Damage")]
public class CollisionDamage : MonoBehaviour
{
    public enum EDirectionMode
    {
        Forward,
        Radial
    }

    public bool CanDamagePlayersInPvEMode;

    public EDeathCause DeathCause = EDeathCause.BURNING;

    public float PlayerDamage = 1f;

    public bool ApplyGlobalArmorMultiplier = true;

    public EDirectionMode DirectionMode;

    private void OnTriggerEnter(Collider other)
    {
        if (!Provider.isServer)
        {
            return;
        }
        GameObject gameObject = other?.gameObject;
        if (gameObject == null || !gameObject.CompareTag("Player") || (!Provider.isPvP && !CanDamagePlayersInPvEMode))
        {
            return;
        }
        Player player = DamageTool.getPlayer(other.transform);
        if (player != null)
        {
            CSteamID killer = CSteamID.Nil;
            if (DamageTool.TryFindOwnership(base.transform, out var ownerUser, out var _))
            {
                killer = new CSteamID(ownerUser);
            }
            EDirectionMode directionMode = DirectionMode;
            Vector3 vector = ((directionMode != 0 && directionMode == EDirectionMode.Radial) ? (player.GetCapsuleCenter() - base.transform.position).normalized : base.transform.forward);
            DamagePlayerParameters parameters = new DamagePlayerParameters(player);
            parameters.cause = DeathCause;
            parameters.limb = ELimb.SPINE;
            parameters.killer = killer;
            parameters.direction = vector;
            parameters.damage = PlayerDamage;
            parameters.applyGlobalArmorMultiplier = ApplyGlobalArmorMultiplier;
            DamageTool.damagePlayer(parameters, out var _);
            DamageTool.ServerSpawnLegacyImpact(player.GetCapsuleCenter(), -vector, "Flesh", null, Provider.GatherClientConnectionsWithinSphere(other.transform.position, EffectManager.SMALL));
        }
    }
}
