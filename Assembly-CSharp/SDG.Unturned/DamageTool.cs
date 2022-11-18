using System;
using System.Collections.Generic;
using SDG.NetTransport;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class DamageTool
{
    public delegate void DamagePlayerHandler(ref DamagePlayerParameters parameters, ref bool shouldAllow);

    public delegate void DamageZombieHandler(ref DamageZombieParameters parameters, ref bool shouldAllow);

    public delegate void DamageAnimalHandler(ref DamageAnimalParameters parameters, ref bool shouldAllow);

    public delegate void PlayerAllowedToDamagePlayerHandler(Player instigator, Player victim, ref bool isAllowed);

    [Obsolete("Use damagePlayerRequested")]
    public static DamageToolPlayerDamagedHandler playerDamaged;

    [Obsolete("Use damageZombieRequested")]
    public static DamageToolZombieDamagedHandler zombieDamaged;

    [Obsolete("Use damageAnimalRequested")]
    public static DamageToolAnimalDamagedHandler animalDamaged;

    private static List<RegionCoordinate> regionsInRadius = new List<RegionCoordinate>(4);

    private static List<Player> playersInRadius = new List<Player>();

    private static List<Zombie> zombiesInRadius = new List<Zombie>();

    private static List<Animal> animalsInRadius = new List<Animal>();

    private static List<Transform> barricadesInRadius = new List<Transform>();

    private static List<Transform> structuresInRadius = new List<Transform>();

    private static List<InteractableVehicle> vehiclesInRadius = new List<InteractableVehicle>();

    private static List<Transform> resourcesInRadius = new List<Transform>();

    private static List<Transform> objectsInRadius = new List<Transform>();

    private static ExplosionRangeComparator explosionRangeComparator = new ExplosionRangeComparator();

    private static List<EPlayerKill> explosionKills = new List<EPlayerKill>();

    private static ClientStaticMethod<Vector3, Vector3, string, Transform> SendSpawnBulletImpact = ClientStaticMethod<Vector3, Vector3, string, Transform>.Get(ReceiveSpawnBulletImpact);

    private static ClientStaticMethod<Vector3, Vector3, string, Transform> SendSpawnLegacyImpact = ClientStaticMethod<Vector3, Vector3, string, Transform>.Get(ReceiveSpawnLegacyImpact);

    private static readonly AssetReference<EffectAsset> FleshDynamicRef = new AssetReference<EffectAsset>("cea791255ba74b43a20e511a52ebcbec");

    private static readonly AssetReference<EffectAsset> AlienDynamicRef = new AssetReference<EffectAsset>("67a4addd45174d7e9ca5c8ec24f8010f");

    public static event DamagePlayerHandler damagePlayerRequested;

    public static event DamageZombieHandler damageZombieRequested;

    public static event DamageAnimalHandler damageAnimalRequested;

    public static event PlayerAllowedToDamagePlayerHandler onPlayerAllowedToDamagePlayer;

    public static ELimb getLimb(Transform limb)
    {
        if (limb.CompareTag("Player") || limb.CompareTag("Enemy") || limb.CompareTag("Zombie") || limb.CompareTag("Animal"))
        {
            switch (limb.name)
            {
            case "Left_Foot":
                return ELimb.LEFT_FOOT;
            case "Left_Leg":
                return ELimb.LEFT_LEG;
            case "Right_Foot":
                return ELimb.RIGHT_FOOT;
            case "Right_Leg":
                return ELimb.RIGHT_LEG;
            case "Left_Hand":
                return ELimb.LEFT_HAND;
            case "Left_Arm":
                return ELimb.LEFT_ARM;
            case "Right_Hand":
                return ELimb.RIGHT_HAND;
            case "Right_Arm":
                return ELimb.RIGHT_ARM;
            case "Left_Back":
                return ELimb.LEFT_BACK;
            case "Right_Back":
                return ELimb.RIGHT_BACK;
            case "Left_Front":
                return ELimb.LEFT_FRONT;
            case "Right_Front":
                return ELimb.RIGHT_FRONT;
            case "Spine":
                return ELimb.SPINE;
            case "Skull":
                return ELimb.SKULL;
            }
        }
        return ELimb.SPINE;
    }

    public static Player getPlayer(Transform limb)
    {
        Player player = limb.GetComponentInParent<Player>();
        if (player != null && player.life.isDead)
        {
            player = null;
        }
        return player;
    }

    public static Zombie getZombie(Transform limb)
    {
        Zombie zombie = limb.GetComponentInParent<Zombie>();
        if (zombie != null && zombie.isDead)
        {
            zombie = null;
        }
        return zombie;
    }

    public static Animal getAnimal(Transform limb)
    {
        Animal animal = limb.GetComponentInParent<Animal>();
        if (animal != null && animal.isDead)
        {
            animal = null;
        }
        return animal;
    }

    public static InteractableVehicle getVehicle(Transform model)
    {
        if (model == null)
        {
            return null;
        }
        model = model.root;
        InteractableVehicle component = model.GetComponent<InteractableVehicle>();
        if (component != null)
        {
            return component;
        }
        VehicleRef component2 = model.GetComponent<VehicleRef>();
        if (component2 != null)
        {
            return component2.vehicle;
        }
        return null;
    }

    public static Transform getBarricadeRootTransform(Transform barricadeTransform)
    {
        Transform transform = barricadeTransform;
        while (true)
        {
            Transform parent = transform.parent;
            if (parent == null)
            {
                return transform;
            }
            if (parent.CompareTag("Vehicle"))
            {
                break;
            }
            transform = parent;
        }
        return transform;
    }

    public static Transform getStructureRootTransform(Transform structureTransform)
    {
        return structureTransform.root;
    }

    public static Transform getResourceRootTransform(Transform resourceTransform)
    {
        return resourceTransform.root;
    }

    public static void damagePlayer(DamagePlayerParameters parameters, out EPlayerKill kill)
    {
        if (parameters.player == null || parameters.player.life.isDead)
        {
            kill = EPlayerKill.NONE;
            return;
        }
        bool shouldAllow = true;
        if (DamageTool.damagePlayerRequested != null)
        {
            DamageTool.damagePlayerRequested(ref parameters, ref shouldAllow);
        }
        if (playerDamaged != null)
        {
            playerDamaged(parameters.player, ref parameters.cause, ref parameters.limb, ref parameters.killer, ref parameters.direction, ref parameters.damage, ref parameters.times, ref shouldAllow);
        }
        if (!shouldAllow)
        {
            kill = EPlayerKill.NONE;
            return;
        }
        if (parameters.respectArmor)
        {
            parameters.times *= getPlayerArmor(parameters.limb, parameters.player);
        }
        if (parameters.applyGlobalArmorMultiplier)
        {
            parameters.times *= Provider.modeConfigData.Players.Armor_Multiplier;
        }
        int num = Mathf.FloorToInt(parameters.damage * parameters.times);
        if (num == 0)
        {
            kill = EPlayerKill.NONE;
            return;
        }
        byte b = (byte)Mathf.Min(255, num);
        bool canCauseBleeding;
        switch (parameters.bleedingModifier)
        {
        default:
            canCauseBleeding = true;
            break;
        case DamagePlayerParameters.Bleeding.Always:
            canCauseBleeding = false;
            parameters.player.life.serverSetBleeding(newBleeding: true);
            break;
        case DamagePlayerParameters.Bleeding.Never:
            canCauseBleeding = false;
            break;
        case DamagePlayerParameters.Bleeding.Heal:
            canCauseBleeding = false;
            parameters.player.life.serverSetBleeding(newBleeding: false);
            break;
        }
        parameters.player.life.askDamage(b, parameters.direction * (int)b, parameters.cause, parameters.limb, parameters.killer, out kill, parameters.trackKill, parameters.ragdollEffect, canCauseBleeding);
        switch (parameters.bonesModifier)
        {
        case DamagePlayerParameters.Bones.Always:
            parameters.player.life.serverSetLegsBroken(newLegsBroken: true);
            break;
        case DamagePlayerParameters.Bones.Heal:
            parameters.player.life.serverSetLegsBroken(newLegsBroken: false);
            break;
        }
        parameters.player.life.serverModifyFood(parameters.foodModifier);
        parameters.player.life.serverModifyWater(parameters.waterModifier);
        parameters.player.life.serverModifyVirus(parameters.virusModifier);
        parameters.player.life.serverModifyHallucination(parameters.hallucinationModifier);
    }

    public static void damage(Player player, EDeathCause cause, ELimb limb, CSteamID killer, Vector3 direction, float damage, float times, out EPlayerKill kill, bool applyGlobalArmorMultiplier = true, bool trackKill = false, ERagdollEffect ragdollEffect = ERagdollEffect.NONE)
    {
        DamagePlayerParameters parameters = new DamagePlayerParameters(player);
        parameters.cause = cause;
        parameters.limb = limb;
        parameters.killer = killer;
        parameters.direction = direction;
        parameters.damage = damage;
        parameters.times = times;
        parameters.applyGlobalArmorMultiplier = applyGlobalArmorMultiplier;
        parameters.trackKill = trackKill;
        parameters.ragdollEffect = ragdollEffect;
        damagePlayer(parameters, out kill);
    }

    public static float getPlayerExplosionArmor(Player player)
    {
        if (player == null)
        {
            return 1f;
        }
        return (0f + (player.clothing.pantsAsset?.explosionArmor ?? 1f) + (player.clothing.shirtAsset?.explosionArmor ?? 1f) + (player.clothing.vestAsset?.explosionArmor ?? 1f) + (player.clothing.hatAsset?.explosionArmor ?? 1f)) / 4f;
    }

    public static float getPlayerArmor(ELimb limb, Player player)
    {
        switch (limb)
        {
        case ELimb.LEFT_FOOT:
        case ELimb.LEFT_LEG:
        case ELimb.RIGHT_FOOT:
        case ELimb.RIGHT_LEG:
        {
            ItemClothingAsset pantsAsset = player.clothing.pantsAsset;
            if (pantsAsset != null)
            {
                if (Provider.modeConfigData.Items.Has_Durability && player.clothing.pantsQuality > 0)
                {
                    player.clothing.pantsQuality--;
                    player.clothing.sendUpdatePantsQuality();
                }
                return pantsAsset.armor + (1f - pantsAsset.armor) * (1f - (float)(int)player.clothing.pantsQuality / 100f);
            }
            break;
        }
        case ELimb.LEFT_HAND:
        case ELimb.LEFT_ARM:
        case ELimb.RIGHT_HAND:
        case ELimb.RIGHT_ARM:
        {
            ItemClothingAsset shirtAsset2 = player.clothing.shirtAsset;
            if (shirtAsset2 != null)
            {
                if (Provider.modeConfigData.Items.Has_Durability && player.clothing.shirtQuality > 0)
                {
                    player.clothing.shirtQuality--;
                    player.clothing.sendUpdateShirtQuality();
                }
                return shirtAsset2.armor + (1f - shirtAsset2.armor) * (1f - (float)(int)player.clothing.shirtQuality / 100f);
            }
            break;
        }
        case ELimb.SPINE:
        {
            float num = 1f;
            if (player.clothing.vestAsset != null)
            {
                ItemClothingAsset vestAsset = player.clothing.vestAsset;
                if (Provider.modeConfigData.Items.Has_Durability && player.clothing.vestQuality > 0)
                {
                    player.clothing.vestQuality--;
                    player.clothing.sendUpdateVestQuality();
                }
                num *= vestAsset.armor + (1f - vestAsset.armor) * (1f - (float)(int)player.clothing.vestQuality / 100f);
            }
            if (player.clothing.shirtAsset != null)
            {
                ItemClothingAsset shirtAsset = player.clothing.shirtAsset;
                if (Provider.modeConfigData.Items.Has_Durability && player.clothing.shirtQuality > 0)
                {
                    player.clothing.shirtQuality--;
                    player.clothing.sendUpdateShirtQuality();
                }
                num *= shirtAsset.armor + (1f - shirtAsset.armor) * (1f - (float)(int)player.clothing.shirtQuality / 100f);
            }
            return num;
        }
        case ELimb.SKULL:
        {
            ItemClothingAsset hatAsset = player.clothing.hatAsset;
            if (hatAsset != null)
            {
                if (Provider.modeConfigData.Items.Has_Durability && player.clothing.hatQuality > 0)
                {
                    player.clothing.hatQuality--;
                    player.clothing.sendUpdateHatQuality();
                }
                return hatAsset.armor + (1f - hatAsset.armor) * (1f - (float)(int)player.clothing.hatQuality / 100f);
            }
            break;
        }
        }
        return 1f;
    }

    public static float GetZombieExplosionArmor(Zombie zombie)
    {
        if (zombie.type < LevelZombies.tables.Count)
        {
            float num = 0f;
            num = ((zombie.pants == byte.MaxValue || zombie.pants >= LevelZombies.tables[zombie.type].slots[1].table.Count) ? (num + 1f) : (num + ((Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[1].table[zombie.pants].item) as ItemClothingAsset)?.explosionArmor ?? 1f)));
            num = ((zombie.shirt == byte.MaxValue || zombie.shirt >= LevelZombies.tables[zombie.type].slots[0].table.Count) ? (num + 1f) : (num + ((Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[0].table[zombie.shirt].item) as ItemClothingAsset)?.explosionArmor ?? 1f)));
            num = ((zombie.gear == byte.MaxValue || zombie.gear >= LevelZombies.tables[zombie.type].slots[3].table.Count) ? (num + 1f) : (num + ((Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[3].table[zombie.gear].item) as ItemClothingAsset)?.explosionArmor ?? 1f)));
            num = ((zombie.hat == byte.MaxValue || zombie.hat >= LevelZombies.tables[zombie.type].slots[2].table.Count) ? (num + 1f) : (num + ((Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[2].table[zombie.hat].item) as ItemClothingAsset)?.explosionArmor ?? 1f)));
            return num / 4f;
        }
        return 1f;
    }

    public static float getZombieArmor(ELimb limb, Zombie zombie)
    {
        if (zombie.type < LevelZombies.tables.Count)
        {
            switch (limb)
            {
            case ELimb.LEFT_FOOT:
            case ELimb.LEFT_LEG:
            case ELimb.RIGHT_FOOT:
            case ELimb.RIGHT_LEG:
                if (zombie.pants != byte.MaxValue && zombie.pants < LevelZombies.tables[zombie.type].slots[1].table.Count && Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[1].table[zombie.pants].item) is ItemClothingAsset itemClothingAsset4)
                {
                    return itemClothingAsset4.armor;
                }
                break;
            case ELimb.LEFT_HAND:
            case ELimb.LEFT_ARM:
            case ELimb.RIGHT_HAND:
            case ELimb.RIGHT_ARM:
                if (zombie.shirt != byte.MaxValue && zombie.shirt < LevelZombies.tables[zombie.type].slots[0].table.Count && Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[0].table[zombie.shirt].item) is ItemClothingAsset itemClothingAsset3)
                {
                    return itemClothingAsset3.armor;
                }
                break;
            case ELimb.SPINE:
            {
                float num = 1f;
                if (zombie.gear != byte.MaxValue && zombie.gear < LevelZombies.tables[zombie.type].slots[3].table.Count && Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[3].table[zombie.gear].item) is ItemAsset itemAsset && itemAsset.type == EItemType.VEST)
                {
                    num *= ((ItemClothingAsset)itemAsset).armor;
                }
                if (zombie.shirt != byte.MaxValue && zombie.shirt < LevelZombies.tables[zombie.type].slots[0].table.Count && Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[0].table[zombie.shirt].item) is ItemClothingAsset itemClothingAsset2)
                {
                    num *= itemClothingAsset2.armor;
                }
                return num;
            }
            case ELimb.SKULL:
                if (zombie.hat != byte.MaxValue && zombie.hat < LevelZombies.tables[zombie.type].slots[2].table.Count && Assets.find(EAssetType.ITEM, LevelZombies.tables[zombie.type].slots[2].table[zombie.hat].item) is ItemClothingAsset itemClothingAsset)
                {
                    return itemClothingAsset.armor;
                }
                break;
            }
        }
        return 1f;
    }

    public static void damage(Player player, EDeathCause cause, ELimb limb, CSteamID killer, Vector3 direction, IDamageMultiplier multiplier, float times, bool armor, out EPlayerKill kill, bool trackKill = false, ERagdollEffect ragdollEffect = ERagdollEffect.NONE)
    {
        DamagePlayerParameters parameters = DamagePlayerParameters.make(player, cause, direction, multiplier, limb);
        parameters.killer = killer;
        parameters.times = times;
        parameters.respectArmor = armor;
        parameters.trackKill = trackKill;
        parameters.ragdollEffect = ragdollEffect;
        damagePlayer(parameters, out kill);
    }

    public static void damageZombie(DamageZombieParameters parameters, out EPlayerKill kill, out uint xp)
    {
        if (parameters.zombie == null || parameters.zombie.isDead)
        {
            kill = EPlayerKill.NONE;
            xp = 0u;
            return;
        }
        if (parameters.respectArmor)
        {
            parameters.times *= getZombieArmor(parameters.limb, parameters.zombie);
        }
        if (parameters.allowBackstab && (double)Vector3.Dot(parameters.zombie.transform.forward, parameters.direction) > 0.5)
        {
            parameters.times *= Provider.modeConfigData.Zombies.Backstab_Multiplier;
            if (Provider.modeConfigData.Zombies.Only_Critical_Stuns && parameters.zombieStunOverride == EZombieStunOverride.None)
            {
                parameters.zombieStunOverride = EZombieStunOverride.Always;
            }
        }
        bool shouldAllow = true;
        if (DamageTool.damageZombieRequested != null)
        {
            DamageTool.damageZombieRequested(ref parameters, ref shouldAllow);
        }
        if (zombieDamaged != null)
        {
            zombieDamaged(parameters.zombie, ref parameters.direction, ref parameters.damage, ref parameters.times, ref shouldAllow);
        }
        if (!shouldAllow)
        {
            kill = EPlayerKill.NONE;
            xp = 0u;
            return;
        }
        if (parameters.applyGlobalArmorMultiplier)
        {
            if (parameters.limb == ELimb.SKULL)
            {
                parameters.times *= Provider.modeConfigData.Zombies.Armor_Multiplier;
            }
            else
            {
                parameters.times *= Provider.modeConfigData.Zombies.NonHeadshot_Armor_Multiplier;
            }
        }
        int num = Mathf.FloorToInt(parameters.damage * parameters.times);
        if (num == 0)
        {
            kill = EPlayerKill.NONE;
            xp = 0u;
        }
        else
        {
            ushort num2 = (ushort)Mathf.Min(65535, num);
            parameters.zombie.askDamage(num2, parameters.direction * (int)num2, out kill, out xp, trackKill: true, dropLoot: true, parameters.zombieStunOverride, parameters.ragdollEffect);
        }
    }

    public static void damage(Zombie zombie, Vector3 direction, float damage, float times, out EPlayerKill kill, out uint xp, EZombieStunOverride zombieStunOverride = EZombieStunOverride.None, ERagdollEffect ragdollEffect = ERagdollEffect.NONE)
    {
        DamageZombieParameters parameters = new DamageZombieParameters(zombie, direction, damage);
        parameters.times = times;
        parameters.zombieStunOverride = zombieStunOverride;
        parameters.ragdollEffect = ragdollEffect;
        damageZombie(parameters, out kill, out xp);
    }

    public static void damage(Zombie zombie, ELimb limb, Vector3 direction, IDamageMultiplier multiplier, float times, bool armor, out EPlayerKill kill, out uint xp, EZombieStunOverride zombieStunOverride = EZombieStunOverride.None, ERagdollEffect ragdollEffect = ERagdollEffect.NONE)
    {
        DamageZombieParameters parameters = DamageZombieParameters.make(zombie, direction, multiplier, limb);
        parameters.legacyArmor = armor;
        parameters.times = times;
        parameters.zombieStunOverride = zombieStunOverride;
        parameters.ragdollEffect = ragdollEffect;
        damageZombie(parameters, out kill, out xp);
    }

    public static void damageAnimal(DamageAnimalParameters parameters, out EPlayerKill kill, out uint xp)
    {
        if (parameters.animal == null || parameters.animal.isDead)
        {
            kill = EPlayerKill.NONE;
            xp = 0u;
            return;
        }
        bool shouldAllow = true;
        if (DamageTool.damageAnimalRequested != null)
        {
            DamageTool.damageAnimalRequested(ref parameters, ref shouldAllow);
        }
        if (animalDamaged != null)
        {
            animalDamaged(parameters.animal, ref parameters.direction, ref parameters.damage, ref parameters.times, ref shouldAllow);
        }
        if (!shouldAllow)
        {
            kill = EPlayerKill.NONE;
            xp = 0u;
            return;
        }
        if (parameters.applyGlobalArmorMultiplier)
        {
            parameters.times *= Provider.modeConfigData.Animals.Armor_Multiplier;
        }
        int num = Mathf.FloorToInt(parameters.damage * parameters.times);
        if (num == 0)
        {
            kill = EPlayerKill.NONE;
            xp = 0u;
        }
        else
        {
            ushort num2 = (ushort)Mathf.Min(65535, num);
            parameters.animal.askDamage(num2, parameters.direction * (int)num2, out kill, out xp, trackKill: true, dropLoot: true, parameters.ragdollEffect);
        }
    }

    public static void damage(Animal animal, Vector3 direction, float damage, float times, out EPlayerKill kill, out uint xp, ERagdollEffect ragdollEffect = ERagdollEffect.NONE)
    {
        DamageAnimalParameters parameters = new DamageAnimalParameters(animal, direction, damage);
        parameters.times = times;
        parameters.ragdollEffect = ragdollEffect;
        damageAnimal(parameters, out kill, out xp);
    }

    public static void damage(Animal animal, ELimb limb, Vector3 direction, IDamageMultiplier multiplier, float times, out EPlayerKill kill, out uint xp, ERagdollEffect ragdollEffect = ERagdollEffect.NONE)
    {
        DamageAnimalParameters parameters = DamageAnimalParameters.make(animal, direction, multiplier, limb);
        parameters.times = times;
        parameters.ragdollEffect = ragdollEffect;
        damageAnimal(parameters, out kill, out xp);
    }

    public static void damage(InteractableVehicle vehicle, bool damageTires, Vector3 position, bool isRepairing, float vehicleDamage, float times, bool canRepair, out EPlayerKill kill, CSteamID instigatorSteamID = default(CSteamID), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
    {
        kill = EPlayerKill.NONE;
        if (vehicle == null)
        {
            return;
        }
        if (isRepairing)
        {
            if (!vehicle.isExploded && !vehicle.isRepaired)
            {
                VehicleManager.repair(vehicle, vehicleDamage, times, instigatorSteamID);
            }
            return;
        }
        if (!vehicle.isDead)
        {
            VehicleManager.damage(vehicle, vehicleDamage, times, canRepair, instigatorSteamID, damageOrigin);
        }
        if (damageTires && !vehicle.isExploded)
        {
            int hitTireIndex = vehicle.getHitTireIndex(position);
            if (hitTireIndex != -1)
            {
                VehicleManager.damageTire(vehicle, hitTireIndex, instigatorSteamID, damageOrigin);
            }
        }
    }

    public static void damage(Transform barricade, bool isRepairing, float barricadeDamage, float times, out EPlayerKill kill, CSteamID instigatorSteamID = default(CSteamID), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
    {
        kill = EPlayerKill.NONE;
        if (!(barricade == null))
        {
            if (isRepairing)
            {
                BarricadeManager.repair(barricade, barricadeDamage, times, instigatorSteamID);
            }
            else
            {
                BarricadeManager.damage(barricade, barricadeDamage, times, armor: true, instigatorSteamID, damageOrigin);
            }
        }
    }

    public static void damage(Transform structure, bool isRepairing, Vector3 direction, float structureDamage, float times, out EPlayerKill kill, CSteamID instigatorSteamID = default(CSteamID), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
    {
        kill = EPlayerKill.NONE;
        if (!(structure == null))
        {
            if (isRepairing)
            {
                StructureManager.repair(structure, structureDamage, times, instigatorSteamID);
            }
            else
            {
                StructureManager.damage(structure, direction, structureDamage, times, armor: true, instigatorSteamID, damageOrigin);
            }
        }
    }

    public static void damage(Transform resource, Vector3 direction, float resourceDamage, float times, float drops, out EPlayerKill kill, out uint xp, CSteamID instigatorSteamID = default(CSteamID), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
    {
        if (resource == null)
        {
            kill = EPlayerKill.NONE;
            xp = 0u;
        }
        else
        {
            ResourceManager.damage(resource, direction, resourceDamage, times, drops, out kill, out xp, instigatorSteamID, damageOrigin);
        }
    }

    public static void damage(Transform obj, Vector3 direction, byte section, float objectDamage, float times, out EPlayerKill kill, out uint xp, CSteamID instigatorSteamID = default(CSteamID), EDamageOrigin damageOrigin = EDamageOrigin.Unknown)
    {
        if (obj == null)
        {
            kill = EPlayerKill.NONE;
            xp = 0u;
        }
        else
        {
            ObjectManager.damage(obj, direction, section, objectDamage, times, out kill, out xp, instigatorSteamID, damageOrigin);
        }
    }

    public static void explode(Vector3 point, float damageRadius, EDeathCause cause, CSteamID killer, float playerDamage, float zombieDamage, float animalDamage, float barricadeDamage, float structureDamage, float vehicleDamage, float resourceDamage, float objectDamage, out List<EPlayerKill> kills, EExplosionDamageType damageType = EExplosionDamageType.CONVENTIONAL, float alertRadius = 32f, bool playImpactEffect = true, bool penetrateBuildables = false, EDamageOrigin damageOrigin = EDamageOrigin.Unknown, ERagdollEffect ragdollEffect = ERagdollEffect.NONE)
    {
        ExplosionParameters parameters = new ExplosionParameters(point, damageRadius, cause, killer);
        parameters.playerDamage = playerDamage;
        parameters.zombieDamage = zombieDamage;
        parameters.animalDamage = animalDamage;
        parameters.barricadeDamage = barricadeDamage;
        parameters.structureDamage = structureDamage;
        parameters.vehicleDamage = vehicleDamage;
        parameters.resourceDamage = resourceDamage;
        parameters.objectDamage = objectDamage;
        parameters.damageType = damageType;
        parameters.alertRadius = alertRadius;
        parameters.playImpactEffect = playImpactEffect;
        parameters.penetrateBuildables = penetrateBuildables;
        parameters.damageOrigin = damageOrigin;
        parameters.ragdollEffect = ragdollEffect;
        parameters.launchSpeed = playerDamage * 0.1f;
        explode(parameters, out kills);
    }

    public static void explode(ExplosionParameters parameters, out List<EPlayerKill> kills)
    {
        explosionKills.Clear();
        kills = explosionKills;
        explosionRangeComparator.point = parameters.point;
        float num = parameters.damageRadius * parameters.damageRadius;
        regionsInRadius.Clear();
        Regions.getRegionsInRadius(parameters.point, parameters.damageRadius, regionsInRadius);
        int layerMask = ((!parameters.penetrateBuildables) ? RayMasks.BLOCK_EXPLOSION : RayMasks.BLOCK_EXPLOSION_PENETRATE_BUILDABLES);
        RaycastHit hitInfo;
        if (parameters.structureDamage > 0.5f)
        {
            structuresInRadius.Clear();
            StructureManager.getStructuresInRadius(parameters.point, num, regionsInRadius, structuresInRadius);
            structuresInRadius.Sort(explosionRangeComparator);
            for (int i = 0; i < structuresInRadius.Count; i++)
            {
                Transform transform = structuresInRadius[i];
                if (transform == null)
                {
                    continue;
                }
                StructureDrop structureDrop = StructureDrop.FindByRootFast(transform);
                if (structureDrop == null)
                {
                    continue;
                }
                ItemStructureAsset asset = structureDrop.asset;
                if (asset == null || asset.proofExplosion)
                {
                    continue;
                }
                Vector3 vector = CollisionUtil.ClosestPoint(transform.gameObject, parameters.point, includeInactive: false) - parameters.point;
                float sqrMagnitude = vector.sqrMagnitude;
                if (!(sqrMagnitude < num))
                {
                    continue;
                }
                float num2 = Mathf.Sqrt(sqrMagnitude);
                Vector3 direction = vector / num2;
                if (num2 > 0.5f)
                {
                    Ray ray = new Ray(parameters.point, direction);
                    float maxDistance = num2 - 0.5f;
                    Physics.Raycast(ray, out hitInfo, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
                    if (hitInfo.transform != null && !hitInfo.transform.IsChildOf(transform))
                    {
                        continue;
                    }
                }
                StructureManager.damage(transform, vector.normalized, parameters.structureDamage, 1f - num2 / parameters.damageRadius, armor: true, parameters.killer, parameters.damageOrigin);
            }
        }
        EPlayerKill kill;
        uint xp;
        if (parameters.resourceDamage > 0.5f)
        {
            resourcesInRadius.Clear();
            ResourceManager.getResourcesInRadius(parameters.point, num, regionsInRadius, resourcesInRadius);
            resourcesInRadius.Sort(explosionRangeComparator);
            for (int j = 0; j < resourcesInRadius.Count; j++)
            {
                Transform transform2 = resourcesInRadius[j];
                if (transform2 == null)
                {
                    continue;
                }
                Vector3 vector2 = CollisionUtil.ClosestPoint(transform2.gameObject, parameters.point, includeInactive: false) - parameters.point;
                float sqrMagnitude2 = vector2.sqrMagnitude;
                if (!(sqrMagnitude2 < num))
                {
                    continue;
                }
                float num3 = Mathf.Sqrt(sqrMagnitude2);
                Vector3 direction2 = vector2 / num3;
                if (num3 > 0.5f)
                {
                    Ray ray2 = new Ray(parameters.point, direction2);
                    float maxDistance2 = num3 - 0.5f;
                    Physics.Raycast(ray2, out hitInfo, maxDistance2, layerMask, QueryTriggerInteraction.Ignore);
                    if (hitInfo.transform != null && !hitInfo.transform.IsChildOf(transform2))
                    {
                        continue;
                    }
                }
                ResourceManager.damage(transform2, vector2.normalized, parameters.resourceDamage, 1f - num3 / parameters.damageRadius, 1f, out kill, out xp, parameters.killer, parameters.damageOrigin);
                if (kill != 0)
                {
                    kills.Add(kill);
                }
            }
        }
        if (parameters.objectDamage > 0.5f)
        {
            objectsInRadius.Clear();
            ObjectManager.getObjectsInRadius(parameters.point, num, regionsInRadius, objectsInRadius);
            objectsInRadius.Sort(explosionRangeComparator);
            for (int k = 0; k < objectsInRadius.Count; k++)
            {
                Transform transform3 = objectsInRadius[k];
                if (transform3 == null)
                {
                    continue;
                }
                InteractableObjectRubble componentInParent = transform3.GetComponentInParent<InteractableObjectRubble>();
                if (componentInParent == null || componentInParent.asset.rubbleProofExplosion)
                {
                    continue;
                }
                for (byte b = 0; b < componentInParent.getSectionCount(); b = (byte)(b + 1))
                {
                    RubbleInfo sectionInfo = componentInParent.getSectionInfo(b);
                    if (sectionInfo.isDead)
                    {
                        continue;
                    }
                    Vector3 vector3 = sectionInfo.section.position;
                    if (sectionInfo.aliveGameObject != null)
                    {
                        vector3 = CollisionUtil.ClosestPoint(sectionInfo.section.gameObject, parameters.point, includeInactive: false);
                    }
                    Vector3 vector4 = vector3 - parameters.point;
                    float sqrMagnitude3 = vector4.sqrMagnitude;
                    if (!(sqrMagnitude3 < num))
                    {
                        continue;
                    }
                    float num4 = Mathf.Sqrt(sqrMagnitude3);
                    Vector3 direction3 = vector4 / num4;
                    if (num4 > 0.5f)
                    {
                        Ray ray3 = new Ray(parameters.point, direction3);
                        float maxDistance3 = num4 - 0.5f;
                        Physics.Raycast(ray3, out hitInfo, maxDistance3, layerMask, QueryTriggerInteraction.Ignore);
                        if (hitInfo.transform != null && !hitInfo.transform.IsChildOf(componentInParent.transform))
                        {
                            continue;
                        }
                    }
                    ObjectManager.damage(componentInParent.transform, vector4.normalized, b, parameters.objectDamage, 1f - num4 / parameters.damageRadius, out kill, out xp, parameters.killer, parameters.damageOrigin);
                    if (kill != 0)
                    {
                        kills.Add(kill);
                    }
                }
            }
        }
        if (parameters.barricadeDamage > 0.5f)
        {
            barricadesInRadius.Clear();
            BarricadeManager.getBarricadesInRadius(parameters.point, num, regionsInRadius, barricadesInRadius);
            BarricadeManager.getBarricadesInRadius(parameters.point, num, barricadesInRadius);
            barricadesInRadius.Sort(explosionRangeComparator);
            for (int l = 0; l < barricadesInRadius.Count; l++)
            {
                Transform transform4 = barricadesInRadius[l];
                if (transform4 == null)
                {
                    continue;
                }
                Vector3 vector5 = CollisionUtil.ClosestPoint(transform4.gameObject, parameters.point, includeInactive: false) - parameters.point;
                float sqrMagnitude4 = vector5.sqrMagnitude;
                if (!(sqrMagnitude4 < num))
                {
                    continue;
                }
                float num5 = Mathf.Sqrt(sqrMagnitude4);
                Vector3 direction4 = vector5 / num5;
                if (num5 > 0.5f)
                {
                    Ray ray4 = new Ray(parameters.point, direction4);
                    float maxDistance4 = num5 - 0.5f;
                    Physics.Raycast(ray4, out hitInfo, maxDistance4, layerMask, QueryTriggerInteraction.Ignore);
                    if (hitInfo.transform != null && !hitInfo.transform.IsChildOf(transform4))
                    {
                        continue;
                    }
                }
                BarricadeDrop barricadeDrop = BarricadeDrop.FindByRootFast(transform4);
                if (barricadeDrop != null)
                {
                    ItemBarricadeAsset asset2 = barricadeDrop.asset;
                    if (asset2 != null && !asset2.proofExplosion)
                    {
                        BarricadeManager.damage(transform4, parameters.barricadeDamage, 1f - num5 / parameters.damageRadius, armor: true, parameters.killer, parameters.damageOrigin);
                    }
                }
            }
        }
        bool flag = (Provider.isPvP || parameters.damageType == EExplosionDamageType.ZOMBIE_ACID || parameters.damageType == EExplosionDamageType.ZOMBIE_FIRE || parameters.damageType == EExplosionDamageType.ZOMBIE_ELECTRIC) && parameters.playerDamage > 0.5f;
        if (flag || parameters.launchSpeed > 0.01f)
        {
            playersInRadius.Clear();
            PlayerTool.getPlayersInRadius(parameters.point, num, playersInRadius);
            for (int m = 0; m < playersInRadius.Count; m++)
            {
                Player player = playersInRadius[m];
                if (player == null || player.life.isDead || (parameters.damageType == EExplosionDamageType.ZOMBIE_FIRE && player.clothing.shirtAsset != null && player.clothing.shirtAsset.proofFire && player.clothing.pantsAsset != null && player.clothing.pantsAsset.proofFire))
                {
                    continue;
                }
                Vector3 vector6 = CollisionUtil.ClosestPoint(player.gameObject, parameters.point, includeInactive: false) - parameters.point;
                float sqrMagnitude5 = vector6.sqrMagnitude;
                if (!(sqrMagnitude5 < num))
                {
                    continue;
                }
                float num6 = Mathf.Sqrt(sqrMagnitude5);
                Vector3 vector7 = vector6 / num6;
                if (num6 > 0.5f)
                {
                    Ray ray5 = new Ray(parameters.point, vector7);
                    float maxDistance5 = num6 - 0.5f;
                    Physics.Raycast(ray5, out hitInfo, maxDistance5, layerMask, QueryTriggerInteraction.Ignore);
                    if (hitInfo.transform != null && !hitInfo.transform.IsChildOf(player.transform))
                    {
                        continue;
                    }
                }
                if (flag)
                {
                    if (parameters.playImpactEffect)
                    {
                        EffectAsset effectAsset = FleshDynamicRef.Find();
                        if (effectAsset != null)
                        {
                            TriggerEffectParameters parameters2 = new TriggerEffectParameters(effectAsset);
                            parameters2.relevantDistance = EffectManager.SMALL;
                            parameters2.position = player.transform.position + Vector3.up;
                            EffectManager.triggerEffect(parameters2);
                            parameters2.direction = -vector7;
                            EffectManager.triggerEffect(parameters2);
                        }
                    }
                    float num7 = 1f - MathfEx.Square(num6 / parameters.damageRadius);
                    if (player.movement.getVehicle() != null && player.movement.getVehicle().asset != null)
                    {
                        num7 *= player.movement.getVehicle().asset.passengerExplosionArmor;
                    }
                    float playerExplosionArmor = getPlayerExplosionArmor(player);
                    num7 *= playerExplosionArmor;
                    damage(player, parameters.cause, ELimb.SPINE, parameters.killer, vector7, parameters.playerDamage, num7, out kill, applyGlobalArmorMultiplier: true, trackKill: true);
                    if (kill != 0)
                    {
                        kills.Add(kill);
                    }
                }
                if (parameters.launchSpeed > 0.01f)
                {
                    Vector3 normalized = (player.transform.position + Vector3.up - parameters.point).normalized;
                    float num8 = 1f - MathfEx.Square(num6 / parameters.damageRadius);
                    num8 *= Provider.modeConfigData.Gameplay.Explosion_Launch_Speed_Multiplier;
                    player.movement.pendingLaunchVelocity += normalized * parameters.launchSpeed * num8;
                }
            }
        }
        if (parameters.damageType == EExplosionDamageType.ZOMBIE_FIRE || parameters.zombieDamage > 0.5f)
        {
            zombiesInRadius.Clear();
            ZombieManager.getZombiesInRadius(parameters.point, num, zombiesInRadius);
            for (int n = 0; n < zombiesInRadius.Count; n++)
            {
                Zombie zombie = zombiesInRadius[n];
                if (zombie == null || zombie.isDead)
                {
                    continue;
                }
                if (parameters.damageType == EExplosionDamageType.ZOMBIE_FIRE)
                {
                    if (zombie.speciality == EZombieSpeciality.NORMAL)
                    {
                        ZombieManager.sendZombieSpeciality(zombie, EZombieSpeciality.BURNER);
                    }
                    continue;
                }
                Vector3 vector8 = CollisionUtil.ClosestPoint(zombie.gameObject, parameters.point, includeInactive: false) - parameters.point;
                float sqrMagnitude6 = vector8.sqrMagnitude;
                if (!(sqrMagnitude6 < num))
                {
                    continue;
                }
                float num9 = Mathf.Sqrt(sqrMagnitude6);
                Vector3 vector9 = vector8 / num9;
                if (num9 > 0.5f)
                {
                    Ray ray6 = new Ray(parameters.point, vector9);
                    float maxDistance6 = num9 - 0.5f;
                    Physics.Raycast(ray6, out hitInfo, maxDistance6, layerMask, QueryTriggerInteraction.Ignore);
                    if (hitInfo.transform != null && !hitInfo.transform.IsChildOf(zombie.transform))
                    {
                        continue;
                    }
                }
                if (parameters.playImpactEffect)
                {
                    EffectAsset effectAsset2 = (zombie.isRadioactive ? AlienDynamicRef.Find() : FleshDynamicRef.Find());
                    if (effectAsset2 != null)
                    {
                        TriggerEffectParameters parameters3 = new TriggerEffectParameters(effectAsset2);
                        parameters3.relevantDistance = EffectManager.SMALL;
                        parameters3.position = zombie.transform.position + Vector3.up;
                        EffectManager.triggerEffect(parameters3);
                        parameters3.direction = -vector9;
                        EffectManager.triggerEffect(parameters3);
                    }
                }
                float num10 = 1f - num9 / parameters.damageRadius;
                float zombieExplosionArmor = GetZombieExplosionArmor(zombie);
                num10 *= zombieExplosionArmor;
                damage(zombie, vector9, parameters.zombieDamage, num10, out kill, out xp, EZombieStunOverride.None, parameters.ragdollEffect);
                if (kill != 0)
                {
                    kills.Add(kill);
                }
            }
        }
        if (parameters.animalDamage > 0.5f)
        {
            animalsInRadius.Clear();
            AnimalManager.getAnimalsInRadius(parameters.point, num, animalsInRadius);
            for (int num11 = 0; num11 < animalsInRadius.Count; num11++)
            {
                Animal animal = animalsInRadius[num11];
                if (animal == null || animal.isDead)
                {
                    continue;
                }
                Vector3 vector10 = CollisionUtil.ClosestPoint(animal.gameObject, parameters.point, includeInactive: false) - parameters.point;
                float sqrMagnitude7 = vector10.sqrMagnitude;
                if (!(sqrMagnitude7 < num))
                {
                    continue;
                }
                float num12 = Mathf.Sqrt(sqrMagnitude7);
                Vector3 vector11 = vector10 / num12;
                if (num12 > 0.5f)
                {
                    Ray ray7 = new Ray(parameters.point, vector11);
                    float maxDistance7 = num12 - 0.5f;
                    Physics.Raycast(ray7, out hitInfo, maxDistance7, layerMask, QueryTriggerInteraction.Ignore);
                    if (hitInfo.transform != null && !hitInfo.transform.IsChildOf(animal.transform))
                    {
                        continue;
                    }
                }
                if (parameters.playImpactEffect)
                {
                    EffectAsset effectAsset3 = FleshDynamicRef.Find();
                    if (effectAsset3 != null)
                    {
                        TriggerEffectParameters parameters4 = new TriggerEffectParameters(effectAsset3);
                        parameters4.relevantDistance = EffectManager.SMALL;
                        parameters4.position = animal.transform.position + Vector3.up + Vector3.up;
                        EffectManager.triggerEffect(parameters4);
                        parameters4.direction = -vector11;
                        EffectManager.triggerEffect(parameters4);
                    }
                }
                damage(animal, vector11, parameters.animalDamage, 1f - num12 / parameters.damageRadius, out kill, out xp, parameters.ragdollEffect);
                if (kill != 0)
                {
                    kills.Add(kill);
                }
            }
        }
        if (parameters.vehicleDamage > 0.5f)
        {
            vehiclesInRadius.Clear();
            VehicleManager.getVehiclesInRadius(parameters.point, num, vehiclesInRadius);
            for (int num13 = 0; num13 < vehiclesInRadius.Count; num13++)
            {
                InteractableVehicle interactableVehicle = vehiclesInRadius[num13];
                if (interactableVehicle == null || interactableVehicle.isDead || interactableVehicle.asset == null || !interactableVehicle.asset.isVulnerableToExplosions)
                {
                    continue;
                }
                Vector3 vector12 = interactableVehicle.getClosestPointOnHull(parameters.point) - parameters.point;
                float sqrMagnitude8 = vector12.sqrMagnitude;
                if (!(sqrMagnitude8 < num))
                {
                    continue;
                }
                float num14 = Mathf.Sqrt(sqrMagnitude8);
                Vector3 direction5 = vector12 / num14;
                float num15 = 1f - num14 / parameters.damageRadius;
                if (num14 > 0.5f)
                {
                    Ray ray8 = new Ray(parameters.point, direction5);
                    float maxDistance8 = num14 - 0.5f;
                    Physics.Raycast(ray8, out hitInfo, maxDistance8, layerMask, QueryTriggerInteraction.Ignore);
                    if (hitInfo.transform != null)
                    {
                        if (!hitInfo.transform.IsChildOf(interactableVehicle.transform))
                        {
                            continue;
                        }
                        num15 *= interactableVehicle.asset.childExplosionArmorMultiplier;
                        num15 *= Provider.modeConfigData.Vehicles.Child_Explosion_Armor_Multiplier;
                    }
                }
                VehicleManager.damage(interactableVehicle, parameters.vehicleDamage, num15, canRepair: false, parameters.killer, parameters.damageOrigin);
            }
        }
        AlertTool.alert(parameters.point, parameters.alertRadius);
    }

    [Obsolete("Physics material enum replaced by string names")]
    public static EPhysicsMaterial getMaterial(Vector3 point, Transform transform, Collider collider)
    {
        return PhysicsTool.GetLegacyMaterialByName(PhysicsTool.GetMaterialName(point, transform, collider));
    }

    [Obsolete("Replaced by separate melee and bullet impact methods")]
    public static void impact(Vector3 point, Vector3 normal, EPhysicsMaterial material, bool forceDynamic)
    {
        impact(point, normal, material, forceDynamic, CSteamID.Nil, point);
    }

    [Obsolete("Replaced by separate melee and bullet impact methods")]
    public static void impact(Vector3 point, Vector3 normal, EPhysicsMaterial material, bool forceDynamic, CSteamID spectatorID, Vector3 spectatorPoint)
    {
        if (material != 0)
        {
            ushort id = 0;
            switch (material)
            {
            case EPhysicsMaterial.CLOTH_DYNAMIC:
            case EPhysicsMaterial.TILE_DYNAMIC:
            case EPhysicsMaterial.CONCRETE_DYNAMIC:
                id = 38;
                break;
            case EPhysicsMaterial.CLOTH_STATIC:
            case EPhysicsMaterial.TILE_STATIC:
            case EPhysicsMaterial.CONCRETE_STATIC:
                id = (ushort)(forceDynamic ? 38 : 13);
                break;
            case EPhysicsMaterial.FLESH_DYNAMIC:
                id = 5;
                break;
            case EPhysicsMaterial.GRAVEL_DYNAMIC:
                id = 44;
                break;
            case EPhysicsMaterial.GRAVEL_STATIC:
            case EPhysicsMaterial.SAND_STATIC:
                id = (ushort)(forceDynamic ? 44 : 14);
                break;
            case EPhysicsMaterial.METAL_DYNAMIC:
                id = 18;
                break;
            case EPhysicsMaterial.METAL_STATIC:
            case EPhysicsMaterial.METAL_SLIP:
                id = (ushort)(forceDynamic ? 18 : 12);
                break;
            case EPhysicsMaterial.WOOD_DYNAMIC:
                id = 17;
                break;
            case EPhysicsMaterial.WOOD_STATIC:
                id = (ushort)(forceDynamic ? 17 : 2);
                break;
            case EPhysicsMaterial.FOLIAGE_STATIC:
            case EPhysicsMaterial.FOLIAGE_DYNAMIC:
                id = 15;
                break;
            case EPhysicsMaterial.SNOW_STATIC:
            case EPhysicsMaterial.ICE_STATIC:
                id = 41;
                break;
            case EPhysicsMaterial.WATER_STATIC:
                id = 16;
                break;
            case EPhysicsMaterial.ALIEN_DYNAMIC:
                id = 95;
                break;
            }
            impact(point, normal, id, spectatorID, spectatorPoint);
        }
    }

    [Obsolete("Replaced by ServerTriggerImpactEffectForMagazinesV2")]
    public static void impact(Vector3 point, Vector3 normal, ushort id, CSteamID spectatorID, Vector3 spectatorPoint)
    {
        if (id != 0)
        {
            ServerTriggerImpactEffectForMagazinesV2(Assets.find(EAssetType.EFFECT, id) as EffectAsset, point, normal, PlayerTool.getSteamPlayer(spectatorID));
        }
    }

    public static void ServerTriggerImpactEffectForMagazinesV2(EffectAsset asset, Vector3 position, Vector3 normal, SteamPlayer instigatingClient)
    {
        if (asset != null)
        {
            position += normal * UnityEngine.Random.Range(0.04f, 0.06f);
            TriggerEffectParameters parameters = new TriggerEffectParameters(asset);
            parameters.position = position;
            parameters.direction = normal;
            parameters.relevantDistance = EffectManager.SMALL;
            if (instigatingClient != null && instigatingClient.player != null && instigatingClient.player.channel != null)
            {
                parameters.SetRelevantTransportConnections(instigatingClient.player.channel.EnumerateClients_WithinSphereOrOwner(position, EffectManager.SMALL));
            }
            EffectManager.triggerEffect(parameters);
        }
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveSpawnBulletImpact(Vector3 position, Vector3 normal, string materialName, Transform colliderTransform)
    {
    }

    internal static void ServerSpawnBulletImpact(Vector3 position, Vector3 normal, string materialName, Transform colliderTransform, IEnumerable<ITransportConnection> transportConnections)
    {
        position += normal * UnityEngine.Random.Range(0.04f, 0.06f);
        SendSpawnBulletImpact.Invoke(ENetReliability.Unreliable, transportConnections, position, normal, materialName, colliderTransform);
    }

    [SteamCall(ESteamCallValidation.ONLY_FROM_SERVER)]
    public static void ReceiveSpawnLegacyImpact(Vector3 position, Vector3 normal, string materialName, Transform colliderTransform)
    {
    }

    internal static void ServerSpawnLegacyImpact(Vector3 position, Vector3 normal, string materialName, Transform colliderTransform, IEnumerable<ITransportConnection> transportConnections)
    {
        position += normal * UnityEngine.Random.Range(0.04f, 0.06f);
        SendSpawnLegacyImpact.Invoke(ENetReliability.Unreliable, transportConnections, position, normal, materialName, colliderTransform);
    }

    public static RaycastInfo raycast(Ray ray, float range, int mask)
    {
        return raycast(ray, range, mask, null);
    }

    public static RaycastInfo raycast(Ray ray, float range, int mask, Player ignorePlayer = null)
    {
        Physics.Raycast(ray, out var hitInfo, range, mask);
        RaycastInfo raycastInfo = new RaycastInfo(hitInfo);
        raycastInfo.direction = ray.direction;
        raycastInfo.limb = ELimb.SPINE;
        if (raycastInfo.transform != null)
        {
            if (raycastInfo.transform.CompareTag("Barricade"))
            {
                raycastInfo.transform = getBarricadeRootTransform(raycastInfo.transform);
            }
            else if (raycastInfo.transform.CompareTag("Structure"))
            {
                raycastInfo.transform = getStructureRootTransform(raycastInfo.transform);
            }
            else if (raycastInfo.transform.CompareTag("Resource"))
            {
                raycastInfo.transform = getResourceRootTransform(raycastInfo.transform);
            }
            else if (raycastInfo.transform.CompareTag("Enemy"))
            {
                raycastInfo.player = getPlayer(raycastInfo.transform);
                if (raycastInfo.player == ignorePlayer)
                {
                    raycastInfo.player = null;
                }
                raycastInfo.limb = getLimb(raycastInfo.transform);
            }
            else if (raycastInfo.transform.CompareTag("Zombie"))
            {
                raycastInfo.zombie = getZombie(raycastInfo.transform);
                raycastInfo.limb = getLimb(raycastInfo.transform);
            }
            else if (raycastInfo.transform.CompareTag("Animal"))
            {
                raycastInfo.animal = getAnimal(raycastInfo.transform);
                raycastInfo.limb = getLimb(raycastInfo.transform);
            }
            else if (raycastInfo.transform.CompareTag("Vehicle"))
            {
                raycastInfo.vehicle = getVehicle(raycastInfo.transform);
            }
            if (raycastInfo.zombie != null && raycastInfo.zombie.isRadioactive)
            {
                raycastInfo.materialName = "Alien_Dynamic";
                raycastInfo.material = EPhysicsMaterial.ALIEN_DYNAMIC;
            }
            else
            {
                raycastInfo.materialName = PhysicsTool.GetMaterialName(hitInfo);
                raycastInfo.material = PhysicsTool.GetLegacyMaterialByName(raycastInfo.materialName);
            }
        }
        return raycastInfo;
    }

    public static bool isPlayerAllowedToDamagePlayer(Player instigator, Player victim)
    {
        bool isAllowed = Provider.isPvP && (Provider.modeConfigData.Gameplay.Friendly_Fire || !instigator.quests.isMemberOfSameGroupAs(victim));
        if (DamageTool.onPlayerAllowedToDamagePlayer != null)
        {
            try
            {
                DamageTool.onPlayerAllowedToDamagePlayer(instigator, victim, ref isAllowed);
                return isAllowed;
            }
            catch (Exception e)
            {
                UnturnedLog.warn("Plugin raised an exception from onPlayerAllowedToDamagePlayer:");
                UnturnedLog.exception(e);
                return isAllowed;
            }
        }
        return isAllowed;
    }
}
