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
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBlastRadius", MeasurementTool.FormatLengthString(range2)), 20002);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionPlayerDamage", Mathf.RoundToInt(playerDamage)), 20003);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionZombieDamage", Mathf.RoundToInt(zombieDamage)), 20003);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionAnimalDamage", Mathf.RoundToInt(animalDamage)), 20003);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionBarricadeDamage", Mathf.RoundToInt(barricadeDamage)), 20003);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionStructureDamage", Mathf.RoundToInt(structureDamage)), 20003);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionVehicleDamage", Mathf.RoundToInt(vehicleDamage)), 20003);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionResourceDamage", Mathf.RoundToInt(resourceDamage)), 20003);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ExplosionObjectDamage", Mathf.RoundToInt(objectDamage)), 20003);
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _range2 = data.ParseFloat("Range2");
        playerDamage = data.ParseFloat("Player_Damage");
        zombieDamage = data.ParseFloat("Zombie_Damage");
        animalDamage = data.ParseFloat("Animal_Damage");
        barricadeDamage = data.ParseFloat("Barricade_Damage");
        structureDamage = data.ParseFloat("Structure_Damage");
        vehicleDamage = data.ParseFloat("Vehicle_Damage");
        resourceDamage = data.ParseFloat("Resource_Damage");
        explosionLaunchSpeed = data.ParseFloat("Explosion_Launch_Speed", playerDamage * 0.1f);
        if (data.ContainsKey("Object_Damage"))
        {
            objectDamage = data.ParseFloat("Object_Damage");
        }
        else
        {
            objectDamage = resourceDamage;
        }
        _explosion2 = data.ParseGuidOrLegacyId("Explosion2", out _detonationEffectGuid);
    }
}
