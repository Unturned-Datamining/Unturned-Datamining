using System;

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

    public float trapSetupDelay;

    public float trapCooldown;

    public float explosionLaunchSpeed;

    public Guid trapDetonationEffectGuid;

    private ushort _explosion2;

    protected bool _isBroken;

    protected bool _isExplosive;

    public bool damageTires;

    public float range2 => _range2;

    public ushort explosion2 => _explosion2;

    public bool isBroken => _isBroken;

    public bool isExplosive => _isExplosive;

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
        if (data.ContainsKey("Object_Damage"))
        {
            objectDamage = data.ParseFloat("Object_Damage");
        }
        else
        {
            objectDamage = resourceDamage;
        }
        trapSetupDelay = data.ParseFloat("Trap_Setup_Delay", 0.25f);
        trapCooldown = data.ParseFloat("Trap_Cooldown");
        _explosion2 = data.ParseGuidOrLegacyId("Explosion2", out trapDetonationEffectGuid);
        explosionLaunchSpeed = data.ParseFloat("Explosion_Launch_Speed", playerDamage * 0.1f);
        _isBroken = data.ContainsKey("Broken");
        _isExplosive = data.ContainsKey("Explosive");
        damageTires = data.ContainsKey("Damage_Tires");
    }
}
