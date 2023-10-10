using UnityEngine;

namespace SDG.Unturned;

public class ItemWeaponAsset : ItemAsset
{
    public float range;

    public PlayerDamageMultiplier playerDamageMultiplier;

    public ZombieDamageMultiplier zombieDamageMultiplier;

    public AnimalDamageMultiplier animalDamageMultiplier;

    public float barricadeDamage;

    public float structureDamage;

    public float vehicleDamage;

    public float resourceDamage;

    public float objectDamage;

    public float durability;

    public byte wear;

    public bool isInvulnerable;

    public byte[] bladeIDs { get; protected set; }

    public DamagePlayerParameters.Bleeding playerDamageBleeding { get; protected set; }

    public DamagePlayerParameters.Bones playerDamageBones { get; protected set; }

    public float playerDamageFood { get; protected set; }

    public float playerDamageWater { get; protected set; }

    public float playerDamageVirus { get; protected set; }

    public float playerDamageHallucination { get; protected set; }

    public EZombieStunOverride zombieStunOverride { get; protected set; }

    public IDamageMultiplier animalOrPlayerDamageMultiplier
    {
        get
        {
            if (!Provider.modeConfigData.Animals.Weapons_Use_Player_Damage)
            {
                return animalDamageMultiplier;
            }
            return playerDamageMultiplier;
        }
    }

    public IDamageMultiplier zombieOrPlayerDamageMultiplier
    {
        get
        {
            if (!Provider.modeConfigData.Zombies.Weapons_Use_Player_Damage)
            {
                return zombieDamageMultiplier;
            }
            return playerDamageMultiplier;
        }
    }

    public bool allowFleshFx { get; protected set; }

    public bool bypassAllowedToDamagePlayer { get; protected set; }

    public bool hasBladeID(byte bladeID)
    {
        if (bladeIDs != null)
        {
            for (int i = 0; i < bladeIDs.Length; i++)
            {
                if (bladeIDs[i] == bladeID)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void initPlayerDamageParameters(ref DamagePlayerParameters parameters)
    {
        parameters.bleedingModifier = playerDamageBleeding;
        parameters.bonesModifier = playerDamageBones;
        parameters.foodModifier = playerDamageFood;
        parameters.waterModifier = playerDamageWater;
        parameters.virusModifier = playerDamageVirus;
        parameters.hallucinationModifier = playerDamageHallucination;
    }

    protected void BuildExplosiveDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBlastRadius", MeasurementTool.FormatLengthString(range)), 20002);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionPlayerDamage", Mathf.RoundToInt(playerDamageMultiplier.damage)), 20003);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionZombieDamage", Mathf.RoundToInt(zombieDamageMultiplier.damage)), 20003);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionAnimalDamage", Mathf.RoundToInt(animalDamageMultiplier.damage)), 20003);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBarricadeDamage", Mathf.RoundToInt(barricadeDamage)), 20003);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionStructureDamage", Mathf.RoundToInt(structureDamage)), 20003);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionVehicleDamage", Mathf.RoundToInt(vehicleDamage)), 20003);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionResourceDamage", Mathf.RoundToInt(resourceDamage)), 20003);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionObjectDamage", Mathf.RoundToInt(objectDamage)), 20003);
    }

    protected void BuildNonExplosiveDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        if (range > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponRange", MeasurementTool.FormatLengthString(range)), 10000);
        }
        int priority = 21000;
        if (playerDamageMultiplier.damage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Head", Mathf.FloorToInt(playerDamageMultiplier.damage * playerDamageMultiplier.skull)), priority++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Body", Mathf.FloorToInt(playerDamageMultiplier.damage * playerDamageMultiplier.spine)), priority++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Arm", Mathf.FloorToInt(playerDamageMultiplier.damage * playerDamageMultiplier.arm)), priority++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Leg", Mathf.FloorToInt(playerDamageMultiplier.damage * playerDamageMultiplier.leg)), priority++);
        }
        int num = Mathf.RoundToInt(playerDamageFood);
        if (num > 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_FoodPositive", num.ToString()), priority);
        }
        else if (num < 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_FoodNegative", num.ToString()), priority);
        }
        int num2 = Mathf.RoundToInt(playerDamageWater);
        if (num2 > 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_WaterPositive", num2.ToString()), priority);
        }
        else if (num2 < 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_WaterNegative", num2.ToString()), priority);
        }
        int num3 = Mathf.RoundToInt(playerDamageVirus);
        if (num3 > 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_VirusPositive", num3.ToString()), priority);
        }
        else if (num3 < 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_VirusNegative", num3.ToString()), priority);
        }
        int num4 = Mathf.RoundToInt(playerDamageHallucination);
        if (num4 > 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_HallucinationPositive", $"{num4} s"), priority);
        }
        else if (num4 < 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_HallucinationNegative", $"{num4} s"), priority);
        }
        switch (playerDamageBleeding)
        {
        case DamagePlayerParameters.Bleeding.Always:
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBleeding_Always"), priority);
            break;
        case DamagePlayerParameters.Bleeding.Heal:
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBleeding_Heal"), priority);
            break;
        }
        switch (playerDamageBones)
        {
        case DamagePlayerParameters.Bones.Always:
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBones_Always"), priority);
            break;
        case DamagePlayerParameters.Bones.Heal:
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBones_Heal"), priority);
            break;
        }
        if (isInvulnerable)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Invulnerable"), 10000);
        }
        if (zombieDamageMultiplier.damage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Head", Mathf.FloorToInt(zombieDamageMultiplier.damage * zombieDamageMultiplier.skull)), 22000);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Body", Mathf.FloorToInt(zombieDamageMultiplier.damage * zombieDamageMultiplier.spine)), 22001);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Arm", Mathf.FloorToInt(zombieDamageMultiplier.damage * zombieDamageMultiplier.arm)), 22002);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Leg", Mathf.FloorToInt(zombieDamageMultiplier.damage * zombieDamageMultiplier.leg)), 22003);
        }
        if (animalDamageMultiplier.damage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Animal_Head", Mathf.FloorToInt(animalDamageMultiplier.damage * animalDamageMultiplier.skull)), 23000);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Animal_Body", Mathf.FloorToInt(animalDamageMultiplier.damage * animalDamageMultiplier.spine)), 23001);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Animal_Limb", Mathf.FloorToInt(animalDamageMultiplier.damage * animalDamageMultiplier.leg)), 23002);
        }
        if (barricadeDamage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Barricade", Mathf.FloorToInt(barricadeDamage)), 24000);
        }
        if (structureDamage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Structure", Mathf.FloorToInt(structureDamage)), 24000);
        }
        if (vehicleDamage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Vehicle", Mathf.FloorToInt(vehicleDamage)), 24000);
        }
        if (resourceDamage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Resource", Mathf.FloorToInt(resourceDamage)), 24000);
        }
        if (objectDamage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Object", Mathf.FloorToInt(objectDamage)), 24000);
        }
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        _ = builder.shouldRestrictToLegacyContent;
    }

    public ItemWeaponAsset()
    {
        playerDamageMultiplier = new PlayerDamageMultiplier(0f, 0f, 0f, 0f, 0f);
        zombieDamageMultiplier = new ZombieDamageMultiplier(0f, 0f, 0f, 0f, 0f);
        animalDamageMultiplier = new AnimalDamageMultiplier(0f, 0f, 0f, 0f);
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        int num = data.ParseInt32("BladeIDs");
        if (num > 0)
        {
            bladeIDs = new byte[num];
            for (int i = 0; i < num; i++)
            {
                bladeIDs[i] = data.ParseUInt8("BladeID_" + i, 0);
            }
        }
        else
        {
            bladeIDs = new byte[1];
            bladeIDs[0] = data.ParseUInt8("BladeID", 0);
        }
        range = data.ParseFloat("Range");
        playerDamageMultiplier = new PlayerDamageMultiplier(data.ParseFloat("Player_Damage"), data.ParseFloat("Player_Leg_Multiplier"), data.ParseFloat("Player_Arm_Multiplier"), data.ParseFloat("Player_Spine_Multiplier"), data.ParseFloat("Player_Skull_Multiplier"));
        playerDamageBleeding = data.ParseEnum("Player_Damage_Bleeding", DamagePlayerParameters.Bleeding.Default);
        playerDamageBones = data.ParseEnum("Player_Damage_Bones", DamagePlayerParameters.Bones.None);
        playerDamageFood = data.ParseFloat("Player_Damage_Food");
        playerDamageWater = data.ParseFloat("Player_Damage_Water");
        playerDamageVirus = data.ParseFloat("Player_Damage_Virus");
        playerDamageHallucination = data.ParseFloat("Player_Damage_Hallucination");
        zombieDamageMultiplier = new ZombieDamageMultiplier(data.ParseFloat("Zombie_Damage"), data.ParseFloat("Zombie_Leg_Multiplier"), data.ParseFloat("Zombie_Arm_Multiplier"), data.ParseFloat("Zombie_Spine_Multiplier"), data.ParseFloat("Zombie_Skull_Multiplier"));
        animalDamageMultiplier = new AnimalDamageMultiplier(data.ParseFloat("Animal_Damage"), data.ParseFloat("Animal_Leg_Multiplier"), data.ParseFloat("Animal_Spine_Multiplier"), data.ParseFloat("Animal_Skull_Multiplier"));
        barricadeDamage = data.ParseFloat("Barricade_Damage");
        structureDamage = data.ParseFloat("Structure_Damage");
        vehicleDamage = data.ParseFloat("Vehicle_Damage");
        resourceDamage = data.ParseFloat("Resource_Damage");
        if (data.ContainsKey("Object_Damage"))
        {
            objectDamage = data.ParseFloat("Object_Damage");
        }
        else
        {
            objectDamage = resourceDamage;
        }
        durability = data.ParseFloat("Durability");
        wear = data.ParseUInt8("Wear", 0);
        if (wear < 1)
        {
            wear = 1;
        }
        isInvulnerable = data.ContainsKey("Invulnerable");
        if (data.ContainsKey("Allow_Flesh_Fx"))
        {
            allowFleshFx = data.ParseBool("Allow_Flesh_Fx");
        }
        else
        {
            allowFleshFx = true;
        }
        if (data.ContainsKey("Stun_Zombie_Always"))
        {
            zombieStunOverride = EZombieStunOverride.Always;
        }
        else if (data.ContainsKey("Stun_Zombie_Never"))
        {
            zombieStunOverride = EZombieStunOverride.Never;
        }
        else
        {
            zombieStunOverride = EZombieStunOverride.None;
        }
        bypassAllowedToDamagePlayer = data.ParseBool("Bypass_Allowed_To_Damage_Player");
    }
}
