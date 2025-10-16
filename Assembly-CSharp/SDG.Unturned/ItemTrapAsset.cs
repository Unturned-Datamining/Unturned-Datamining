using System;
using UnityEngine;

namespace SDG.Unturned;

public class ItemTrapAsset : ItemBarricadeAsset
{
    protected float _range2;

    public float playerDamage;

    public float zombieDamage;

    public float animalDamage;

    public float barricadeDamage;

    public float structureDamage;

    public float vehicleDamage;

    public float resourceDamage;

    public float objectDamage;

    /// <summary>
    /// Seconds after placement before damage can be dealt.
    /// </summary>
    public float trapSetupDelay;

    /// <summary>
    /// Seconds interval between damage dealt.
    /// i.e., will not cause damage if less than this amount of time passed since the last damage.
    /// </summary>
    public float trapCooldown;

    public float explosionLaunchSpeed;

    public Guid trapDetonationEffectGuid;

    private ushort _explosion2;

    protected bool _isBroken;

    protected bool _isExplosive;

    public bool damageTires;

    public bool requiresPower;

    public float range2 => _range2;

    public ushort explosion2 => _explosion2;

    public bool isBroken => _isBroken;

    public bool isExplosive => _isExplosive;

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (!builder.HasFlag(EItemDescriptionFlags.Uncategorized))
        {
            return;
        }
        if (isExplosive)
        {
            int sortOrder = 30000;
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBlastRadius", MeasurementTool.FormatLengthString(range2)), sortOrder++);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionPlayerDamage", Mathf.RoundToInt(playerDamage)), sortOrder);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionZombieDamage", Mathf.RoundToInt(zombieDamage)), sortOrder);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionAnimalDamage", Mathf.RoundToInt(animalDamage)), sortOrder);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBarricadeDamage", Mathf.RoundToInt(barricadeDamage)), sortOrder);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionStructureDamage", Mathf.RoundToInt(structureDamage)), sortOrder);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionVehicleDamage", Mathf.RoundToInt(vehicleDamage)), sortOrder);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionResourceDamage", Mathf.RoundToInt(resourceDamage)), sortOrder);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionObjectDamage", Mathf.RoundToInt(objectDamage)), sortOrder);
            return;
        }
        if (isBroken)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Trap_BreaksBones"), 10001);
        }
        if (damageTires)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Trap_DamagesTires"), 10001);
        }
        if (requiresPower)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Trap_RequiresPower"), 10001);
        }
        if (playerDamage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Trap_PlayerDamage", Mathf.RoundToInt(playerDamage)), 10002);
        }
        if (zombieDamage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Trap_ZombieDamage", Mathf.RoundToInt(zombieDamage)), 10002);
        }
        if (animalDamage > 0f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Trap_AnimalDamage", Mathf.RoundToInt(animalDamage)), 10002);
        }
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _range2 = p.data.ParseFloat("Range2");
        playerDamage = p.data.ParseFloat("Player_Damage");
        zombieDamage = p.data.ParseFloat("Zombie_Damage");
        animalDamage = p.data.ParseFloat("Animal_Damage");
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
        trapSetupDelay = p.data.ParseFloat("Trap_Setup_Delay", 0.25f);
        trapCooldown = p.data.ParseFloat("Trap_Cooldown");
        _explosion2 = p.data.ParseGuidOrLegacyId("Explosion2", out trapDetonationEffectGuid);
        explosionLaunchSpeed = p.data.ParseFloat("Explosion_Launch_Speed", playerDamage * 0.1f);
        _isBroken = p.data.ContainsKey("Broken");
        _isExplosive = p.data.ContainsKey("Explosive");
        damageTires = p.data.ContainsKey("Damage_Tires");
        requiresPower = p.data.ParseBool("Requires_Power");
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Trap");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Range2", range2);
        orAddDeclaration.Append("Player_Damage", playerDamage);
        orAddDeclaration.Append("Zombie_Damage", zombieDamage);
        orAddDeclaration.Append("Animal_Damage", animalDamage);
        orAddDeclaration.Append("Barricade_Damage", barricadeDamage);
        orAddDeclaration.Append("Structure_Damage", structureDamage);
        orAddDeclaration.Append("Vehicle_Damage", vehicleDamage);
        orAddDeclaration.Append("Resource_Damage", resourceDamage);
        orAddDeclaration.Append("Object_Damage", objectDamage);
        orAddDeclaration.Append("Trap_Setup_Delay", trapSetupDelay);
        orAddDeclaration.Append("Trap_Cooldown", trapCooldown);
        orAddDeclaration.Append("Explosion2", explosion2);
        orAddDeclaration.Append("Explosion_Launch_Speed", explosionLaunchSpeed);
        orAddDeclaration.Append("Broken", isBroken);
        orAddDeclaration.Append("Explosive", isExplosive);
        orAddDeclaration.Append("Damage_Tires", damageTires);
        orAddDeclaration.Append("Requires_Power", requiresPower);
    }
}
