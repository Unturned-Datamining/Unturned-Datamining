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

    public ItemTrapAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _range2 = data.readSingle("Range2");
        playerDamage = data.readSingle("Player_Damage");
        zombieDamage = data.readSingle("Zombie_Damage");
        animalDamage = data.readSingle("Animal_Damage");
        barricadeDamage = data.readSingle("Barricade_Damage");
        structureDamage = data.readSingle("Structure_Damage");
        vehicleDamage = data.readSingle("Vehicle_Damage");
        resourceDamage = data.readSingle("Resource_Damage");
        if (data.has("Object_Damage"))
        {
            objectDamage = data.readSingle("Object_Damage");
        }
        else
        {
            objectDamage = resourceDamage;
        }
        trapSetupDelay = data.readSingle("Trap_Setup_Delay", 0.25f);
        trapCooldown = data.readSingle("Trap_Cooldown");
        _explosion2 = data.ReadGuidOrLegacyId("Explosion2", out trapDetonationEffectGuid);
        explosionLaunchSpeed = data.readSingle("Explosion_Launch_Speed", playerDamage * 0.1f);
        _isBroken = data.has("Broken");
        _isExplosive = data.has("Explosive");
        damageTires = data.has("Damage_Tires");
    }
}
