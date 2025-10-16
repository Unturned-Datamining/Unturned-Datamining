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

    /// <summary>
    /// Added to player's food value.
    /// </summary>
    public float playerDamageFood { get; protected set; }

    /// <summary>
    /// Added to player's water value.
    /// </summary>
    public float playerDamageWater { get; protected set; }

    /// <summary>
    /// Added to player's virus value.
    /// </summary>
    public float playerDamageVirus { get; protected set; }

    /// <summary>
    /// Added to player's hallucination value.
    /// </summary>
    public float playerDamageHallucination { get; protected set; }

    public EZombieStunOverride zombieStunOverride { get; protected set; }

    public float ZombieRagdollForceMultiplier { get; set; }

    /// <summary>
    /// Get animal or player damage based on game mode config.
    /// </summary>
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

    /// <summary>
    /// Get zombie or player damage based on game mode config.
    /// </summary>
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

    /// <summary>
    /// Should player/animal/zombie surface be nulled on hit?
    /// Requested by spyjack for a chainsaw-style shield that was overboard with the blood.
    /// </summary>
    public bool allowFleshFx { get; protected set; }

    /// <summary>
    /// Should this weapon bypass the DamageTool.allowedToDamagePlayer test?
    /// Used by weapons that heal players in PvE.
    /// </summary>
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

    /// <summary>
    /// Please refer to ItemWeaponAsset.BuildDescription for an explanation of why this is necessary.
    /// </summary>
    protected void BuildExplosiveDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        int sortOrder = 30000;
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBlastRadius", MeasurementTool.FormatLengthString(range)), sortOrder++);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionPlayerDamage", Mathf.RoundToInt(playerDamageMultiplier.damage)), sortOrder);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionZombieDamage", Mathf.RoundToInt(zombieDamageMultiplier.damage)), sortOrder);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionAnimalDamage", Mathf.RoundToInt(animalDamageMultiplier.damage)), sortOrder);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBarricadeDamage", Mathf.RoundToInt(barricadeDamage)), sortOrder);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionStructureDamage", Mathf.RoundToInt(structureDamage)), sortOrder);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionVehicleDamage", Mathf.RoundToInt(vehicleDamage)), sortOrder);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionResourceDamage", Mathf.RoundToInt(resourceDamage)), sortOrder);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionObjectDamage", Mathf.RoundToInt(objectDamage)), sortOrder);
    }

    /// <summary>
    /// Please refer to ItemWeaponAsset.BuildDescription for an explanation of why this is necessary.
    /// </summary>
    protected void BuildNonExplosiveDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        if (range > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponRange", MeasurementTool.FormatLengthString(range)), 10000);
        }
        int sortOrder = 30000;
        if (playerDamageMultiplier.damage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Head", Mathf.FloorToInt(playerDamageMultiplier.damage * playerDamageMultiplier.skull)), sortOrder++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Body", Mathf.FloorToInt(playerDamageMultiplier.damage * playerDamageMultiplier.spine)), sortOrder++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Arm", Mathf.FloorToInt(playerDamageMultiplier.damage * playerDamageMultiplier.arm)), sortOrder++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_Leg", Mathf.FloorToInt(playerDamageMultiplier.damage * playerDamageMultiplier.leg)), sortOrder++);
        }
        int num = Mathf.RoundToInt(playerDamageFood);
        if (num > 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_FoodPositive", num.ToString()), sortOrder);
        }
        else if (num < 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_FoodNegative", (-num).ToString()), sortOrder);
        }
        int num2 = Mathf.RoundToInt(playerDamageWater);
        if (num2 > 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_WaterPositive", num2.ToString()), sortOrder);
        }
        else if (num2 < 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_WaterNegative", (-num2).ToString()), sortOrder);
        }
        int num3 = Mathf.RoundToInt(playerDamageVirus);
        if (num3 > 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_VirusPositive", num3.ToString()), sortOrder);
        }
        else if (num3 < 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_VirusNegative", (-num3).ToString()), sortOrder);
        }
        int num4 = Mathf.RoundToInt(playerDamageHallucination);
        if (num4 > 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_HallucinationPositive", $"{num4} s"), sortOrder);
        }
        else if (num4 < 0)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Player_HallucinationNegative", $"{-num4} s"), sortOrder);
        }
        switch (playerDamageBleeding)
        {
        case DamagePlayerParameters.Bleeding.Always:
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBleeding_Always"), sortOrder);
            break;
        case DamagePlayerParameters.Bleeding.Heal:
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBleeding_Heal"), sortOrder);
            break;
        }
        switch (playerDamageBones)
        {
        case DamagePlayerParameters.Bones.Always:
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBones_Always"), sortOrder);
            break;
        case DamagePlayerParameters.Bones.Heal:
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponBones_Heal"), sortOrder);
            break;
        }
        if (isInvulnerable)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Invulnerable"), 10000);
        }
        if (zombieDamageMultiplier.damage > 0f)
        {
            int sortOrder2 = 31000;
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Head", Mathf.FloorToInt(zombieDamageMultiplier.damage * zombieDamageMultiplier.skull)), sortOrder2++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Body", Mathf.FloorToInt(zombieDamageMultiplier.damage * zombieDamageMultiplier.spine)), sortOrder2++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Arm", Mathf.FloorToInt(zombieDamageMultiplier.damage * zombieDamageMultiplier.arm)), sortOrder2++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Zombie_Leg", Mathf.FloorToInt(zombieDamageMultiplier.damage * zombieDamageMultiplier.leg)), sortOrder2);
        }
        if (animalDamageMultiplier.damage > 0f)
        {
            int sortOrder3 = 32000;
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Animal_Head", Mathf.FloorToInt(animalDamageMultiplier.damage * animalDamageMultiplier.skull)), sortOrder3++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Animal_Body", Mathf.FloorToInt(animalDamageMultiplier.damage * animalDamageMultiplier.spine)), sortOrder3++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Animal_Limb", Mathf.FloorToInt(animalDamageMultiplier.damage * animalDamageMultiplier.leg)), sortOrder3);
        }
        if (barricadeDamage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Barricade", Mathf.FloorToInt(barricadeDamage)), 33000);
        }
        if (structureDamage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Structure", Mathf.FloorToInt(structureDamage)), 33000);
        }
        if (vehicleDamage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Vehicle", Mathf.FloorToInt(vehicleDamage)), 33000);
        }
        if (resourceDamage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Resource", Mathf.FloorToInt(resourceDamage)), 33000);
        }
        if (objectDamage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_WeaponDamage_Object", Mathf.FloorToInt(objectDamage)), 33000);
        }
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        builder.HasFlag(EItemDescriptionFlags.Uncategorized);
    }

    public ItemWeaponAsset()
    {
        playerDamageMultiplier = new PlayerDamageMultiplier(0f, 0f, 0f, 0f, 0f);
        zombieDamageMultiplier = new ZombieDamageMultiplier(0f, 0f, 0f, 0f, 0f);
        animalDamageMultiplier = new AnimalDamageMultiplier(0f, 0f, 0f, 0f);
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        int num = p.data.ParseInt32("BladeIDs");
        if (num > 0)
        {
            bladeIDs = new byte[num];
            for (int i = 0; i < num; i++)
            {
                bladeIDs[i] = p.data.ParseUInt8("BladeID_" + i, 0);
            }
        }
        else
        {
            bladeIDs = new byte[1];
            bladeIDs[0] = p.data.ParseUInt8("BladeID", 0);
        }
        range = p.data.ParseFloat("Range");
        playerDamageMultiplier = new PlayerDamageMultiplier(p.data.ParseFloat("Player_Damage"), p.data.ParseFloat("Player_Leg_Multiplier"), p.data.ParseFloat("Player_Arm_Multiplier"), p.data.ParseFloat("Player_Spine_Multiplier"), p.data.ParseFloat("Player_Skull_Multiplier"));
        playerDamageBleeding = p.data.ParseEnum("Player_Damage_Bleeding", DamagePlayerParameters.Bleeding.Default);
        playerDamageBones = p.data.ParseEnum("Player_Damage_Bones", DamagePlayerParameters.Bones.None);
        playerDamageFood = p.data.ParseFloat("Player_Damage_Food");
        playerDamageWater = p.data.ParseFloat("Player_Damage_Water");
        playerDamageVirus = p.data.ParseFloat("Player_Damage_Virus");
        playerDamageHallucination = p.data.ParseFloat("Player_Damage_Hallucination");
        zombieDamageMultiplier = new ZombieDamageMultiplier(p.data.ParseFloat("Zombie_Damage"), p.data.ParseFloat("Zombie_Leg_Multiplier"), p.data.ParseFloat("Zombie_Arm_Multiplier"), p.data.ParseFloat("Zombie_Spine_Multiplier"), p.data.ParseFloat("Zombie_Skull_Multiplier"));
        animalDamageMultiplier = new AnimalDamageMultiplier(p.data.ParseFloat("Animal_Damage"), p.data.ParseFloat("Animal_Leg_Multiplier"), p.data.ParseFloat("Animal_Spine_Multiplier"), p.data.ParseFloat("Animal_Skull_Multiplier"));
        barricadeDamage = p.data.ParseFloat("Barricade_Damage");
        structureDamage = p.data.ParseFloat("Structure_Damage");
        vehicleDamage = p.data.ParseFloat("Vehicle_Damage");
        resourceDamage = p.data.ParseFloat("Resource_Damage");
        if (p.data.ContainsKey("Object_Damage"))
        {
            objectDamage = p.data.ParseFloat("Object_Damage");
        }
        else
        {
            objectDamage = resourceDamage;
        }
        durability = p.data.ParseFloat("Durability");
        wear = p.data.ParseUInt8("Wear", 0);
        if (wear < 1)
        {
            wear = 1;
        }
        isInvulnerable = p.data.ContainsKey("Invulnerable");
        if (p.data.ContainsKey("Allow_Flesh_Fx"))
        {
            allowFleshFx = p.data.ParseBool("Allow_Flesh_Fx");
        }
        else
        {
            allowFleshFx = true;
        }
        if (p.data.ContainsKey("Stun_Zombie_Always"))
        {
            zombieStunOverride = EZombieStunOverride.Always;
        }
        else if (p.data.ContainsKey("Stun_Zombie_Never"))
        {
            zombieStunOverride = EZombieStunOverride.Never;
        }
        else
        {
            zombieStunOverride = EZombieStunOverride.None;
        }
        ZombieRagdollForceMultiplier = p.data.ParseFloat("Zombie_Ragdoll_Force_Multiplier", 1f);
        bypassAllowedToDamagePlayer = p.data.ParseBool("Bypass_Allowed_To_Damage_Player");
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Weapon");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Durability", durability);
        orAddDeclaration.Append("Range", range);
        orAddDeclaration.Append("Wear", wear);
        orAddDeclaration.Append("Player_Damage", playerDamageMultiplier.damage);
        orAddDeclaration.Append("Player_Leg_Multiplier", playerDamageMultiplier.leg);
        orAddDeclaration.Append("Player_Arm_Multiplier", playerDamageMultiplier.arm);
        orAddDeclaration.Append("Player_Spine_Multiplier", playerDamageMultiplier.spine);
        orAddDeclaration.Append("Player_Skull_Multiplier", playerDamageMultiplier.skull);
        orAddDeclaration.Append("Player_Damage_Bleeding", playerDamageBleeding);
        orAddDeclaration.Append("Player_Damage_Bones", playerDamageBones);
        orAddDeclaration.Append("Player_Damage_Food", playerDamageFood);
        orAddDeclaration.Append("Player_Damage_Water", playerDamageWater);
        orAddDeclaration.Append("Player_Damage_Virus", playerDamageVirus);
        orAddDeclaration.Append("Player_Damage_Hallucination", playerDamageHallucination);
        orAddDeclaration.Append("Zombie_Damage", zombieDamageMultiplier.damage);
        orAddDeclaration.Append("Zombie_Leg_Multiplier", zombieDamageMultiplier.leg);
        orAddDeclaration.Append("Zombie_Arm_Multiplier", zombieDamageMultiplier.arm);
        orAddDeclaration.Append("Zombie_Spine_Multiplier", zombieDamageMultiplier.spine);
        orAddDeclaration.Append("Zombie_Skull_Multiplier", zombieDamageMultiplier.skull);
        orAddDeclaration.Append("zombieStunOverride", zombieStunOverride);
        orAddDeclaration.Append("Animal_Damage", animalDamageMultiplier.damage);
        orAddDeclaration.Append("Animal_Leg_Multiplier", animalDamageMultiplier.leg);
        orAddDeclaration.Append("Animal_Spine_Multiplier", animalDamageMultiplier.spine);
        orAddDeclaration.Append("Animal_Skull_Multiplier", animalDamageMultiplier.skull);
        orAddDeclaration.Append("BladeIDs", bladeIDs.Length);
        orAddDeclaration.Append("Barricade_Damage", barricadeDamage);
        orAddDeclaration.Append("Structure_Damage", structureDamage);
        orAddDeclaration.Append("Vehicle_Damage", vehicleDamage);
        orAddDeclaration.Append("Resource_Damage", resourceDamage);
        orAddDeclaration.Append("Object_Damage", objectDamage);
        orAddDeclaration.Append("Invulnerable", isInvulnerable);
        for (byte b = 0; b < bladeIDs.Length; b++)
        {
            CargoDeclaration cargoDeclaration = builder.AddDeclaration("Weapon_BladeID");
            cargoDeclaration.Append("GUID", GUID);
            cargoDeclaration.Append("BladeID", (object)bladeIDs[b]);
        }
    }
}
