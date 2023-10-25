using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Payload for the DamageTool.damagePlayer function.
/// </summary>
public struct DamagePlayerParameters
{
    public enum Bleeding
    {
        Default,
        Always,
        Never,
        Heal
    }

    public enum Bones
    {
        None,
        Always,
        Heal
    }

    public Player player;

    public EDeathCause cause;

    public ELimb limb;

    public CSteamID killer;

    public Vector3 direction;

    public float damage;

    public float times;

    /// <summary>
    /// Should armor worn on matching limb be factored in?
    /// </summary>
    public bool respectArmor;

    /// <summary>
    /// Should game mode config damage multiplier be factored in?
    /// </summary>
    public bool applyGlobalArmorMultiplier;

    /// <summary>
    /// If player dies should it count towards quests?
    /// </summary>
    public bool trackKill;

    /// <summary>
    /// Effect to apply to ragdoll if dead.
    /// </summary>
    public ERagdollEffect ragdollEffect;

    public Bleeding bleedingModifier;

    public Bones bonesModifier;

    public float foodModifier;

    public float waterModifier;

    public float virusModifier;

    public float hallucinationModifier;

    public DamagePlayerParameters(Player player)
    {
        this.player = player;
        cause = EDeathCause.SUICIDE;
        limb = ELimb.SPINE;
        killer = CSteamID.Nil;
        direction = Vector3.up;
        damage = 0f;
        times = 1f;
        respectArmor = false;
        applyGlobalArmorMultiplier = true;
        trackKill = false;
        ragdollEffect = ERagdollEffect.NONE;
        bleedingModifier = Bleeding.Default;
        bonesModifier = Bones.None;
        foodModifier = 0f;
        waterModifier = 0f;
        virusModifier = 0f;
        hallucinationModifier = 0f;
    }

    public static DamagePlayerParameters make(Player player, EDeathCause cause, Vector3 direction, IDamageMultiplier multiplier, ELimb limb)
    {
        DamagePlayerParameters result = new DamagePlayerParameters(player);
        result.cause = cause;
        result.limb = limb;
        result.direction = direction;
        result.damage = multiplier.multiply(limb);
        return result;
    }
}
