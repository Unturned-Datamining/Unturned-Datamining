using SDG.Framework.IO.FormattedFiles;

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

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        range = reader.readValue<float>("Range");
        playerDamageMultiplier = new PlayerDamageMultiplier(reader.readValue<float>("Player_Damage"), reader.readValue<float>("Player_Leg_Multiplier"), reader.readValue<float>("Player_Arm_Multiplier"), reader.readValue<float>("Player_Spine_Multiplier"), reader.readValue<float>("Player_Skull_Multiplier"));
        zombieDamageMultiplier = new ZombieDamageMultiplier(reader.readValue<float>("Zombie_Damage"), reader.readValue<float>("Zombie_Leg_Multiplier"), reader.readValue<float>("Zombie_Arm_Multiplier"), reader.readValue<float>("Zombie_Spine_Multiplier"), reader.readValue<float>("Zombie_Skull_Multiplier"));
        animalDamageMultiplier = new AnimalDamageMultiplier(reader.readValue<float>("Animal_Damage"), reader.readValue<float>("Animal_Leg_Multiplier"), reader.readValue<float>("Animal_Spine_Multiplier"), reader.readValue<float>("Animal_Skull_Multiplier"));
        barricadeDamage = reader.readValue<float>("Barricade_Damage");
        structureDamage = reader.readValue<float>("Structure_Damage");
        vehicleDamage = reader.readValue<float>("Vehicle_Damage");
        resourceDamage = reader.readValue<float>("Resource_Damage");
        objectDamage = reader.readValue<float>("Object_Damage");
        durability = reader.readValue<float>("Durability");
        wear = reader.readValue<byte>("Wear");
        isInvulnerable = reader.readValue<bool>("Invulnerable");
    }

    protected override void writeAsset(IFormattedFileWriter writer)
    {
        base.writeAsset(writer);
        writer.writeValue("Range", range);
        writer.writeValue("Player_Damage", playerDamageMultiplier.damage);
        writer.writeValue("Player_Leg_Multiplier", playerDamageMultiplier.leg);
        writer.writeValue("Player_Arm_Multiplier", playerDamageMultiplier.arm);
        writer.writeValue("Player_Spine_Multiplier", playerDamageMultiplier.spine);
        writer.writeValue("Player_Skull_Multiplier", playerDamageMultiplier.skull);
        writer.writeValue("Zombie_Damage", zombieDamageMultiplier.damage);
        writer.writeValue("Zombie_Leg_Multiplier", zombieDamageMultiplier.leg);
        writer.writeValue("Zombie_Arm_Multiplier", zombieDamageMultiplier.arm);
        writer.writeValue("Zombie_Spine_Multiplier", zombieDamageMultiplier.spine);
        writer.writeValue("Zombie_Skull_Multiplier", zombieDamageMultiplier.skull);
        writer.writeValue("Animal_Damage", animalDamageMultiplier.damage);
        writer.writeValue("Animal_Leg_Multiplier", animalDamageMultiplier.leg);
        writer.writeValue("Animal_Spine_Multiplier", animalDamageMultiplier.spine);
        writer.writeValue("Animal_Skull_Multiplier", animalDamageMultiplier.skull);
        writer.writeValue("Barricade_Damage", barricadeDamage);
        writer.writeValue("Structure_Damage", structureDamage);
        writer.writeValue("Vehicle_Damage", vehicleDamage);
        writer.writeValue("Resource_Damage", resourceDamage);
        writer.writeValue("Object_Damage", objectDamage);
        writer.writeValue("Durability", durability);
        writer.writeValue("Wear", wear);
        writer.writeValue("Invulnerable", isInvulnerable);
    }

    public ItemWeaponAsset()
    {
        playerDamageMultiplier = new PlayerDamageMultiplier(0f, 0f, 0f, 0f, 0f);
        zombieDamageMultiplier = new ZombieDamageMultiplier(0f, 0f, 0f, 0f, 0f);
        animalDamageMultiplier = new AnimalDamageMultiplier(0f, 0f, 0f, 0f);
    }

    public ItemWeaponAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
    }

    public ItemWeaponAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        int num = data.readInt32("BladeIDs");
        if (num > 0)
        {
            bladeIDs = new byte[num];
            for (int i = 0; i < num; i++)
            {
                bladeIDs[i] = data.readByte("BladeID_" + i, 0);
            }
        }
        else
        {
            bladeIDs = new byte[1];
            bladeIDs[0] = data.readByte("BladeID", 0);
        }
        range = data.readSingle("Range");
        playerDamageMultiplier = new PlayerDamageMultiplier(data.readSingle("Player_Damage"), data.readSingle("Player_Leg_Multiplier"), data.readSingle("Player_Arm_Multiplier"), data.readSingle("Player_Spine_Multiplier"), data.readSingle("Player_Skull_Multiplier"));
        playerDamageBleeding = data.readEnum("Player_Damage_Bleeding", DamagePlayerParameters.Bleeding.Default);
        playerDamageBones = data.readEnum("Player_Damage_Bones", DamagePlayerParameters.Bones.None);
        playerDamageFood = data.readSingle("Player_Damage_Food");
        playerDamageWater = data.readSingle("Player_Damage_Water");
        playerDamageVirus = data.readSingle("Player_Damage_Virus");
        playerDamageHallucination = data.readSingle("Player_Damage_Hallucination");
        zombieDamageMultiplier = new ZombieDamageMultiplier(data.readSingle("Zombie_Damage"), data.readSingle("Zombie_Leg_Multiplier"), data.readSingle("Zombie_Arm_Multiplier"), data.readSingle("Zombie_Spine_Multiplier"), data.readSingle("Zombie_Skull_Multiplier"));
        animalDamageMultiplier = new AnimalDamageMultiplier(data.readSingle("Animal_Damage"), data.readSingle("Animal_Leg_Multiplier"), data.readSingle("Animal_Spine_Multiplier"), data.readSingle("Animal_Skull_Multiplier"));
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
        durability = data.readSingle("Durability");
        wear = data.readByte("Wear", 0);
        if (wear < 1)
        {
            wear = 1;
        }
        isInvulnerable = data.has("Invulnerable");
        if (data.has("Allow_Flesh_Fx"))
        {
            allowFleshFx = data.readBoolean("Allow_Flesh_Fx");
        }
        else
        {
            allowFleshFx = true;
        }
        if (data.has("Stun_Zombie_Always"))
        {
            zombieStunOverride = EZombieStunOverride.Always;
        }
        else if (data.has("Stun_Zombie_Never"))
        {
            zombieStunOverride = EZombieStunOverride.Never;
        }
        else
        {
            zombieStunOverride = EZombieStunOverride.None;
        }
        bypassAllowedToDamagePlayer = data.readBoolean("Bypass_Allowed_To_Damage_Player");
    }
}
