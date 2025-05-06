using System;
using UnityEngine;

namespace SDG.Unturned;

public class ItemChargeAsset : ItemBarricadeAsset
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

    public float explosionLaunchSpeed;

    private Guid _detonationEffectGuid;

    private ushort _explosion2;

    public float range2 => _range2;

    public Guid DetonationEffectGuid => _detonationEffectGuid;

    public ushort explosion2
    {
        [Obsolete]
        get
        {
            return _explosion2;
        }
    }

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (!builder.shouldRestrictToLegacyContent)
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
        explosionLaunchSpeed = p.data.ParseFloat("Explosion_Launch_Speed", playerDamage * 0.1f);
        if (p.data.ContainsKey("Object_Damage"))
        {
            objectDamage = p.data.ParseFloat("Object_Damage");
        }
        else
        {
            objectDamage = resourceDamage;
        }
        _explosion2 = p.data.ParseGuidOrLegacyId("Explosion2", out _detonationEffectGuid);
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Charge");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Range2", range2);
        orAddDeclaration.Append("Player_Damage", playerDamage);
        orAddDeclaration.Append("Zombie_Damage", zombieDamage);
        orAddDeclaration.Append("Animal_Damage", animalDamage);
        orAddDeclaration.Append("Barricade_Damage", barricadeDamage);
        orAddDeclaration.Append("Structure_Damage", structureDamage);
        orAddDeclaration.Append("Vehicle_Damage", vehicleDamage);
        orAddDeclaration.Append("Resource_Damage", resourceDamage);
        orAddDeclaration.Append("Explosion_Launch_Speed", explosionLaunchSpeed);
        orAddDeclaration.Append("Object_Damage", objectDamage);
        orAddDeclaration.Append("Explosion2", explosion2);
    }
}
