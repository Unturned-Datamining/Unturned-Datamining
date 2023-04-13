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
