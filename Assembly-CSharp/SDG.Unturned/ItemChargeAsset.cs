using System;

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

    public ItemChargeAsset(Bundle bundle, Data data, Local localization, ushort id)
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
        explosionLaunchSpeed = data.readSingle("Explosion_Launch_Speed", playerDamage * 0.1f);
        if (data.has("Object_Damage"))
        {
            objectDamage = data.readSingle("Object_Damage");
        }
        else
        {
            objectDamage = resourceDamage;
        }
        _explosion2 = data.ReadGuidOrLegacyId("Explosion2", out _detonationEffectGuid);
    }
}
