using UnityEngine;

namespace SDG.Unturned;

public class ItemTacticalAssetMeleeProperties
{
    public float MeleeRange { get; set; }

    public PlayerDamageMultiplier MeleePlayerDamageMultiplier { get; set; }

    public DamagePlayerParameters.Bleeding MeleePlayerDamageBleeding { get; set; }

    public DamagePlayerParameters.Bones MeleePlayerDamageBones { get; set; }

    public ZombieDamageMultiplier MeleeZombieDamageMultiplier { get; set; }

    public EZombieStunOverride MeleeZombieStunOverride { get; set; }

    public AnimalDamageMultiplier MeleeAnimalDamageMultiplier { get; set; }

    /// <summary>
    /// Get animal or player damage based on game mode config.
    /// </summary>
    public IDamageMultiplier MeleeAnimalOrPlayerDamageMultiplier
    {
        get
        {
            if (!Provider.modeConfigData.Animals.Weapons_Use_Player_Damage)
            {
                return MeleeAnimalDamageMultiplier;
            }
            return MeleePlayerDamageMultiplier;
        }
    }

    /// <summary>
    /// Get zombie or player damage based on game mode config.
    /// </summary>
    public IDamageMultiplier MeleeZombieOrPlayerDamageMultiplier
    {
        get
        {
            if (!Provider.modeConfigData.Zombies.Weapons_Use_Player_Damage)
            {
                return MeleeZombieDamageMultiplier;
            }
            return MeleePlayerDamageMultiplier;
        }
    }

    public float MeleeZombieRagdollForceMultiplier { get; set; }

    public void InitPlayerDamageParameters(ref DamagePlayerParameters parameters)
    {
        parameters.bleedingModifier = MeleePlayerDamageBleeding;
        parameters.bonesModifier = MeleePlayerDamageBones;
    }

    internal void BuildDescription(ItemDescriptionBuilder builder)
    {
        if (MeleeRange > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponRange", MeasurementTool.FormatLengthString(MeleeRange)), 10000);
        }
        int sortOrder = 30000;
        if (MeleePlayerDamageMultiplier.damage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Head", Mathf.FloorToInt(MeleePlayerDamageMultiplier.damage * MeleePlayerDamageMultiplier.skull)), sortOrder++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Body", Mathf.FloorToInt(MeleePlayerDamageMultiplier.damage * MeleePlayerDamageMultiplier.spine)), sortOrder++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Arm", Mathf.FloorToInt(MeleePlayerDamageMultiplier.damage * MeleePlayerDamageMultiplier.arm)), sortOrder++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Leg", Mathf.FloorToInt(MeleePlayerDamageMultiplier.damage * MeleePlayerDamageMultiplier.leg)), sortOrder++);
        }
        switch (MeleePlayerDamageBleeding)
        {
        case DamagePlayerParameters.Bleeding.Always:
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBleeding_Always"), sortOrder);
            break;
        case DamagePlayerParameters.Bleeding.Heal:
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBleeding_Heal"), sortOrder);
            break;
        }
        switch (MeleePlayerDamageBones)
        {
        case DamagePlayerParameters.Bones.Always:
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBones_Always"), sortOrder);
            break;
        case DamagePlayerParameters.Bones.Heal:
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBones_Heal"), sortOrder);
            break;
        }
        if (MeleeZombieDamageMultiplier.damage > 0f)
        {
            int sortOrder2 = 31000;
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Head", Mathf.FloorToInt(MeleeZombieDamageMultiplier.damage * MeleeZombieDamageMultiplier.skull)), sortOrder2++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Body", Mathf.FloorToInt(MeleeZombieDamageMultiplier.damage * MeleeZombieDamageMultiplier.spine)), sortOrder2++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Arm", Mathf.FloorToInt(MeleeZombieDamageMultiplier.damage * MeleeZombieDamageMultiplier.arm)), sortOrder2++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Leg", Mathf.FloorToInt(MeleeZombieDamageMultiplier.damage * MeleeZombieDamageMultiplier.leg)), sortOrder2);
        }
        if (MeleeAnimalDamageMultiplier.damage > 0f)
        {
            int sortOrder3 = 32000;
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Animal_Head", Mathf.FloorToInt(MeleeAnimalDamageMultiplier.damage * MeleeAnimalDamageMultiplier.skull)), sortOrder3++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Animal_Body", Mathf.FloorToInt(MeleeAnimalDamageMultiplier.damage * MeleeAnimalDamageMultiplier.spine)), sortOrder3++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Animal_Limb", Mathf.FloorToInt(MeleeAnimalDamageMultiplier.damage * MeleeAnimalDamageMultiplier.leg)), sortOrder3);
        }
    }

    internal void PopulateAsset(in PopulateAssetParameters p)
    {
        MeleeRange = p.data.ParseFloat("Melee_Range", 2f);
        MeleePlayerDamageMultiplier = new PlayerDamageMultiplier(p.data.ParseFloat("Melee_Player_Damage", 40f), p.data.ParseFloat("Melee_Player_Leg_Multiplier", 0.6f), p.data.ParseFloat("Melee_Player_Arm_Multiplier", 0.6f), p.data.ParseFloat("Melee_Player_Spine_Multiplier", 0.8f), p.data.ParseFloat("Melee_Player_Skull_Multiplier", 1.1f));
        MeleePlayerDamageBleeding = p.data.ParseEnum("Melee_Player_Damage_Bleeding", DamagePlayerParameters.Bleeding.Default);
        MeleePlayerDamageBones = p.data.ParseEnum("Melee_Player_Damage_Bones", DamagePlayerParameters.Bones.None);
        MeleeZombieDamageMultiplier = new ZombieDamageMultiplier(p.data.ParseFloat("Melee_Zombie_Damage", 40f), p.data.ParseFloat("Melee_Zombie_Leg_Multiplier", 0.3f), p.data.ParseFloat("Melee_Zombie_Arm_Multiplier", 0.3f), p.data.ParseFloat("Melee_Zombie_Spine_Multiplier", 0.6f), p.data.ParseFloat("Melee_Zombie_Skull_Multiplier", 1.1f));
        MeleeAnimalDamageMultiplier = new AnimalDamageMultiplier(p.data.ParseFloat("Melee_Animal_Damage", 40f), p.data.ParseFloat("Melee_Animal_Leg_Multiplier", 0.3f), p.data.ParseFloat("Melee_Animal_Spine_Multiplier", 0.6f), p.data.ParseFloat("Melee_Animal_Skull_Multiplier", 1.1f));
        if (p.data.ContainsKey("Melee_Stun_Zombie_Always"))
        {
            MeleeZombieStunOverride = EZombieStunOverride.Always;
        }
        else if (p.data.ContainsKey("Melee_Stun_Zombie_Never"))
        {
            MeleeZombieStunOverride = EZombieStunOverride.Never;
        }
        else
        {
            MeleeZombieStunOverride = EZombieStunOverride.None;
        }
        MeleeZombieRagdollForceMultiplier = p.data.ParseFloat("Melee_Zombie_Ragdoll_Force_Multiplier", 1f);
    }
}
