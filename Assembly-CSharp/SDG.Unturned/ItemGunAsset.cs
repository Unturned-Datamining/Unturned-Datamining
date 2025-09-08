using System;
using UnityEngine;

namespace SDG.Unturned;

public class ItemGunAsset : ItemWeaponAsset
{
    protected AudioClip _shoot;

    protected AudioClip _reload;

    protected AudioClip _hammer;

    protected AudioClip _aim;

    protected AudioClip _minigun;

    protected AudioClip _chamberJammedSound;

    protected GameObject _projectile;

    public float alertRadius;

    public byte ammoMin;

    public byte ammoMax;

    private ushort _sightID;

    private byte[] sightState;

    private ushort _tacticalID;

    private byte[] tacticalState;

    private ushort _gripID;

    private byte[] gripState;

    private ushort _barrelID;

    private byte[] barrelState;

    private ushort defaultMagazineLegacyId;

    private Guid defaultMagazineGuid;

    private MagazineReplacement[] magazineReplacements;

    public float unplace;

    public float replace;

    public bool hasSight;

    public bool hasTactical;

    public bool hasGrip;

    public bool hasBarrel;

    public byte firerate;

    public EAction action;

    public bool shouldDeleteEmptyMagazines;

    /// <summary>
    /// Defaults to false. If true, attachments must specify at least one non-zero caliber.
    /// Requested by Great Hero J to block vanilla attachments in VGR.
    /// </summary>
    public bool requiresNonZeroAttachmentCaliber;

    public bool hasSafety;

    public bool hasSemi;

    public bool hasAuto;

    public bool hasBurst;

    public bool isTurret;

    internal EDriverTurretViewmodelMode driverTurretViewmodelMode;

    public int bursts;

    internal EFiremode firemode;

    public float spreadAim;

    [Obsolete("Replaced by baseSpreadAngleRadians")]
    public float spreadHip;

    /// <summary>
    /// Spread multiplier while sprinting.
    /// </summary>
    public float spreadSprint;

    /// <summary>
    /// Spread multiplier while crouched.
    /// </summary>
    public float spreadCrouch;

    /// <summary>
    /// Spread multiplier while prone.
    /// </summary>
    public float spreadProne;

    /// <summary>
    /// Spread multiplier while swimming.
    /// </summary>
    public float spreadSwimming;

    /// <summary>
    /// Spread multiplier while not grounded.
    /// </summary>
    public float spreadMidair;

    public float recoilMin_x;

    public float recoilMin_y;

    public float recoilMax_x;

    public float recoilMax_y;

    /// <summary>
    /// Recoil magnitude multiplier while the gun is aiming down sights.
    /// </summary>
    public float aimingRecoilMultiplier;

    /// <summary>
    /// Recoil magnitude while sprinting.
    /// </summary>
    public float recoilSprint;

    /// <summary>
    /// Recoil magnitude while crouched.
    /// </summary>
    public float recoilCrouch;

    /// <summary>
    /// Recoil magnitude while prone.
    /// </summary>
    public float recoilProne;

    /// <summary>
    /// Recoil magnitude while swimming.
    /// </summary>
    public float recoilSwimming;

    /// <summary>
    /// Recoil magnitude while not grounded.
    /// </summary>
    public float recoilMidair;

    public float recover_x;

    public float recover_y;

    public float shakeMin_x;

    public float shakeMin_y;

    public float shakeMin_z;

    public float shakeMax_x;

    public float shakeMax_y;

    public float shakeMax_z;

    public byte ballisticSteps;

    public float ballisticTravel;

    public float ballisticForce;

    /// <summary>
    /// [0, 1] percentage of maximum range where damage begins decreasing toward falloff multiplier.
    /// </summary>
    public float damageFalloffRange;

    /// <summary>
    /// [0, 1] percentage of maximum range where damage finishes decreasing toward falloff multiplier.
    /// </summary>
    public float damageFalloffMaxRange;

    /// <summary>
    /// [0, 1] percentage of damage to apply at damageFalloffMaxRange.
    /// </summary>
    public float damageFalloffMultiplier;

    /// <summary>
    /// Seconds before physics projectile is destroyed.
    /// </summary>
    public float projectileLifespan;

    public bool projectilePenetrateBuildables;

    public float projectileExplosionLaunchSpeed;

    public float reloadTime;

    public float hammerTime;

    public Guid muzzleGuid;

    [Obsolete]
    public ushort muzzle;

    public Guid shellGuid;

    [Obsolete]
    public ushort shell;

    public Guid projectileExplosionEffectGuid;

    public ushort explosion;

    /// <summary>
    /// Movement speed multiplier while the gun is aiming down sights.
    /// </summary>
    public float aimingMovementSpeedMultiplier;

    protected NPCRewardsList shootQuestRewards;

    private static CommandLineFlag shouldLogBallisticDropConversion = new CommandLineFlag(defaultValue: false, "-LogBallisticDropConversion");

    private static CommandLineFlag shouldLogSpreadConversion = new CommandLineFlag(defaultValue: false, "-LogGunSpreadConversion");

    public AudioClip shoot => _shoot;

    public AudioClip reload => _reload;

    public AudioClip hammer => _hammer;

    public AudioClip aim => _aim;

    public AudioClip minigun => _minigun;

    public AudioClip chamberJammedSound => _chamberJammedSound;

    /// <summary>
    /// Sound to play when input is pressed but weapon has a fire delay.
    /// </summary>
    public AudioClip fireDelaySound { get; protected set; }

    /// <summary>
    /// Maximum distance the gunshot can be heard.
    /// </summary>
    public float gunshotRolloffDistance { get; protected set; }

    public GameObject projectile => _projectile;

    public override bool shouldFriendlySentryTargetUser => true;

    /// <summary>
    /// Override Rangefinder attachment's maximum range.
    /// Defaults to range value.
    /// </summary>
    public float rangeRangefinder { get; protected set; }

    /// <summary>
    /// Can this weapon instantly kill players by headshots?
    /// Only valid when game config also enables this.
    /// </summary>
    public bool instakillHeadshots { get; protected set; }

    /// <summary>
    /// Can this weapon be fired without consuming ammo?
    /// Some mods use this for turrets.
    /// </summary>
    public bool infiniteAmmo { get; protected set; }

    /// <summary>
    /// Ammo quantity to consume per shot fired.
    /// </summary>
    public byte ammoPerShot { get; protected set; }

    /// <summary>
    /// Simulation steps to wait after input before firing.
    /// </summary>
    public int fireDelay { get; protected set; }

    /// <summary>
    /// Can magazine be changed by player?
    /// </summary>
    public bool allowMagazineChange { get; protected set; }

    /// <summary>
    /// Can player ADS while sprinting and vice versa?
    /// </summary>
    public bool canAimDuringSprint { get; protected set; }

    /// <summary>
    /// If true, the gun cannot shoot unless the player is aiming.
    /// Note: String action overrides this.
    /// Defaults to true for miniguns.
    /// </summary>
    public bool MustAimToShoot { get; protected set; }

    /// <summary>
    /// If true, the gun will stop aiming regardless of player input.
    /// </summary>
    public bool ShouldForceStopAimingAfterShooting { get; set; }

    /// <summary>
    /// Seconds from pressing "aim" to fully aiming down sights.
    /// </summary>
    public float aimInDuration { get; protected set; }

    /// <summary>
    /// If true, Aim_Start and Aim_Stop animations are scaled according to actual aim duration.
    /// </summary>
    public bool shouldScaleAimAnimations { get; protected set; }

    public ushort sightID
    {
        get
        {
            return _sightID;
        }
        set
        {
            _sightID = value;
            sightState = BitConverter.GetBytes(sightID);
        }
    }

    public ushort tacticalID
    {
        get
        {
            return _tacticalID;
        }
        set
        {
            _tacticalID = value;
            tacticalState = BitConverter.GetBytes(tacticalID);
        }
    }

    public ushort gripID
    {
        get
        {
            return _gripID;
        }
        set
        {
            _gripID = value;
            gripState = BitConverter.GetBytes(gripID);
        }
    }

    public ushort barrelID
    {
        get
        {
            return _barrelID;
        }
        set
        {
            _barrelID = value;
            barrelState = BitConverter.GetBytes(barrelID);
        }
    }

    /// <summary>
    /// How long in seconds after firing to rechamber the gun by playing the Hammer animation.
    /// Only applicable if RechamberAfterShotCount is &gt;0.
    /// Defaults to 0.25 seconds.
    /// </summary>
    public float RechamberAfterShotDelay { get; set; } = 0.25f;


    /// <summary>
    /// How long in seconds after hammering to eject a bullet casing.
    /// Defaults to 0.45 seconds.
    /// </summary>
    public float EjectAfterHammerDelay { get; set; } = 0.45f;


    /// <summary>
    /// How long in seconds after reloading to eject bullet casings.
    /// Only applicable if CasingEjectCountAfterReload is greater than zero.
    /// Defaults to 0.5 seconds.
    /// </summary>
    public float EjectAfterReloadDelay { get; set; } = 0.5f;


    public ushort[] attachmentCalibers { get; private set; }

    public ushort[] magazineCalibers { get; private set; }

    /// <summary>
    /// Determines whether "Hammer" animation plays after attaching a magazine.
    /// Note: this happens when a magazine replaces another OR fills previously empty slot.
    /// </summary>
    public ERechamberGunAfterReloadMode RechamberAfterMagazineAttached { get; set; }

    /// <summary>
    /// Determines whether "Hammer" animation plays after detached a magazine.
    /// Note: this happens when a magazine is removed from the gun without a replacement.
    /// </summary>
    public ERechamberGunAfterReloadMode RechamberAfterMagazineDetached { get; set; }

    public float baseSpreadAngleRadians { get; private set; }

    public float muzzleVelocity { get; protected set; }

    public float bulletGravityMultiplier { get; protected set; }

    public override bool showQuality => true;

    /// <summary>
    /// Is this gun setup to have a change of jamming?
    /// </summary>
    public bool canEverJam { get; protected set; }

    /// <summary>
    /// [0, 1] quality percentage that jamming will start happening.
    /// </summary>
    public float jamQualityThreshold { get; protected set; }

    /// <summary>
    /// [0, 1] percentage of the time that shots will jam the gun when at 0% quality.
    /// Chance of jamming is blended between 0% at jamQualityThreshold and jamMaxChance% at 0% quality.
    /// </summary>
    public float jamMaxChance { get; protected set; }

    /// <summary>
    /// Name of the animation to play when unjamming chamber.
    /// </summary>
    public string unjamChamberAnimName { get; protected set; }

    /// <summary>
    /// If &gt;0, hammer animation plays after shooting this many shots after RechamberAfterShotDelay seconds pass.
    /// Defaults to one for EAction.Pump and EAction.Bolt, zero otherwise.
    /// </summary>
    public int RechamberAfterShotCount { get; set; }

    /// <summary>
    /// If &gt;0, emit particles after hammer after EjectAfterHammerDelay seconds pass.
    /// Only applicable if RechamberAfterShotCount is &gt;0.
    /// Defaults to 1.
    /// </summary>
    public int CasingEjectCountAfterRechamberingAfterShooting { get; set; }

    /// <summary>
    /// If &gt;0, emit particles after reloading after EjectAfterReloadDelay seconds pass.
    /// Defaults to ammoMax for EAction.Break.
    /// </summary>
    public int CasingEjectCountAfterReload { get; set; }

    /// <summary>
    /// If true, emit particles when a shot is fired.
    /// Defaults to true for EAction.Trigger and EAction.Minigun.
    /// </summary>
    public bool ShouldEjectCasingAfterShooting { get; set; }

    protected override bool doesItemTypeHaveSkins => true;

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (itemInstance != null)
        {
            ushort num = BitConverter.ToUInt16(itemInstance.state, 8);
            if (Assets.find(EAssetType.ITEM, num) is ItemMagazineAsset itemMagazineAsset)
            {
                if (!string.IsNullOrEmpty(itemMagazineAsset.itemName))
                {
                    builder.Append(PlayerDashboardInventoryUI.localization.format("Ammo", "<color=" + Palette.hex(ItemTool.getRarityColorUI(itemMagazineAsset.rarity)) + ">" + itemMagazineAsset.itemName + "</color>", itemInstance.state[10], itemMagazineAsset.MaxAmount), 2000);
                }
                else
                {
                    builder.Append(PlayerDashboardInventoryUI.localization.format("Ammo", "<color=" + Palette.hex(ItemTool.getRarityColorUI(rarity)) + ">" + base.itemName + "</color>", itemInstance.state[10], itemMagazineAsset.MaxAmount), 2000);
                }
            }
            else
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("Ammo", PlayerDashboardInventoryUI.localization.format("None"), 0, 0), 2000);
            }
        }
        if (builder.shouldRestrictToLegacyContent)
        {
            return;
        }
        if (itemInstance != null)
        {
            ushort num2 = BitConverter.ToUInt16(itemInstance.state, 0);
            ushort num3 = BitConverter.ToUInt16(itemInstance.state, 2);
            ushort num4 = BitConverter.ToUInt16(itemInstance.state, 4);
            ushort num5 = BitConverter.ToUInt16(itemInstance.state, 6);
            ItemSightAsset itemSightAsset = Assets.find(EAssetType.ITEM, num2) as ItemSightAsset;
            ItemTacticalAsset itemTacticalAsset = Assets.find(EAssetType.ITEM, num3) as ItemTacticalAsset;
            ItemGripAsset itemGripAsset = Assets.find(EAssetType.ITEM, num4) as ItemGripAsset;
            ItemBarrelAsset itemBarrelAsset = Assets.find(EAssetType.ITEM, num5) as ItemBarrelAsset;
            if (itemSightAsset != null && (hasSight || num2 != sightID))
            {
                if (!string.IsNullOrEmpty(itemSightAsset.itemName))
                {
                    builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_SightAttachment", "<color=" + Palette.hex(ItemTool.getRarityColorUI(itemSightAsset.rarity)) + ">" + itemSightAsset.itemName + "</color>"), 2000);
                }
            }
            else if (hasSight)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_SightAttachment", PlayerDashboardInventoryUI.localization.format("None")), 2000);
            }
            if (itemTacticalAsset != null && (hasTactical || num3 != tacticalID))
            {
                if (!string.IsNullOrEmpty(itemTacticalAsset.itemName))
                {
                    builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_TacticalAttachment", "<color=" + Palette.hex(ItemTool.getRarityColorUI(itemTacticalAsset.rarity)) + ">" + itemTacticalAsset.itemName + "</color>"), 2000);
                }
            }
            else if (hasTactical)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_TacticalAttachment", PlayerDashboardInventoryUI.localization.format("None")), 2000);
            }
            if (itemGripAsset != null && (hasGrip || num4 != gripID))
            {
                if (!string.IsNullOrEmpty(itemGripAsset.itemName))
                {
                    builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_GripAttachment", "<color=" + Palette.hex(ItemTool.getRarityColorUI(itemGripAsset.rarity)) + ">" + itemGripAsset.itemName + "</color>"), 2000);
                }
            }
            else if (hasGrip)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_GripAttachment", PlayerDashboardInventoryUI.localization.format("None")), 2000);
            }
            if (itemBarrelAsset != null && (hasBarrel || num5 != barrelID))
            {
                if (!string.IsNullOrEmpty(itemBarrelAsset.itemName))
                {
                    builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_BarrelAttachment", "<color=" + Palette.hex(ItemTool.getRarityColorUI(itemBarrelAsset.rarity)) + ">" + itemBarrelAsset.itemName + "</color>"), 2000);
                }
            }
            else if (hasBarrel)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_BarrelAttachment", PlayerDashboardInventoryUI.localization.format("None")), 2000);
            }
        }
        float f = CalculateRoundsPerSecond() * 60f;
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Firerate", Mathf.RoundToInt(f)), 10000);
        builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Spread", $"{57.29578f * baseSpreadAngleRadians:N1}"), 10000);
        if (spreadAim != 1f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_Spread_Aim", $"{57.29578f * baseSpreadAngleRadians * spreadAim:N1}"), 10000);
        }
        if (aimingRecoilMultiplier != 1f)
        {
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_RecoilModifier_Aiming", PlayerDashboardInventoryUI.FormatStatModifier(aimingRecoilMultiplier, higherIsPositive: false, higherIsBeneficial: false)), 10000 + DescSort_LowerIsBeneficial(aimingRecoilMultiplier));
        }
        if (damageFalloffRange != 1f && damageFalloffMultiplier != 1f)
        {
            string arg = MeasurementTool.FormatLengthString(range * damageFalloffRange);
            string arg2 = MeasurementTool.FormatLengthString(range * damageFalloffMaxRange);
            builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_DamageFalloff", arg, arg2, $"{damageFalloffMultiplier:P}"), 10000);
        }
        if (_projectile != null)
        {
            BuildExplosiveDescription(builder, itemInstance);
        }
        else
        {
            BuildNonExplosiveDescription(builder, itemInstance);
        }
    }

    public override byte[] getState(EItemOrigin origin)
    {
        byte[] magazineState = getMagazineState(GetDefaultMagazineLegacyId());
        byte[] obj = new byte[18]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1, 100, 100, 100, 100, 100
        };
        obj[0] = sightState[0];
        obj[1] = sightState[1];
        obj[2] = tacticalState[0];
        obj[3] = tacticalState[1];
        obj[4] = gripState[0];
        obj[5] = gripState[1];
        obj[6] = barrelState[0];
        obj[7] = barrelState[1];
        obj[8] = magazineState[0];
        obj[9] = magazineState[1];
        obj[10] = ((origin != 0 || UnityEngine.Random.value < ((Provider.modeConfigData != null) ? Provider.modeConfigData.Items.Gun_Bullets_Full_Chance : 0.9f)) ? ammoMax : ((byte)Mathf.CeilToInt((float)UnityEngine.Random.Range(ammoMin, ammoMax + 1) * ((Provider.modeConfigData != null) ? Provider.modeConfigData.Items.Gun_Bullets_Multiplier : 1f))));
        obj[11] = (byte)firemode;
        return obj;
    }

    public byte[] getState(ushort sight, ushort tactical, ushort grip, ushort barrel, ushort magazine, byte ammo)
    {
        byte[] bytes = BitConverter.GetBytes(sight);
        byte[] bytes2 = BitConverter.GetBytes(tactical);
        byte[] bytes3 = BitConverter.GetBytes(grip);
        byte[] bytes4 = BitConverter.GetBytes(barrel);
        byte[] bytes5 = BitConverter.GetBytes(magazine);
        byte[] obj = new byte[18]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 1, 100, 100, 100, 100, 100
        };
        obj[0] = bytes[0];
        obj[1] = bytes[1];
        obj[2] = bytes2[0];
        obj[3] = bytes2[1];
        obj[4] = bytes3[0];
        obj[5] = bytes3[1];
        obj[6] = bytes4[0];
        obj[7] = bytes4[1];
        obj[8] = bytes5[0];
        obj[9] = bytes5[1];
        obj[10] = ammo;
        obj[11] = (byte)firemode;
        return obj;
    }

    /// <summary>
    /// Selects a default magazine, following magazine replacements and spawn table resolution.
    /// </summary>
    public ushort GetDefaultMagazineLegacyId()
    {
        return SelectDefaultMagazine()?.id ?? 0;
    }

    /// <summary>
    /// Selects a default magazine, following magazine replacements and spawn table resolution.
    /// </summary>
    public ItemMagazineAsset SelectDefaultMagazine()
    {
        bool flag = false;
        Asset asset = null;
        if (Level.info != null && magazineReplacements != null)
        {
            MagazineReplacement[] array = magazineReplacements;
            for (int i = 0; i < array.Length; i++)
            {
                MagazineReplacement magazineReplacement = array[i];
                if (magazineReplacement.map == Level.info.name)
                {
                    asset = Assets.FindByGuidOrLegacyId(magazineReplacement.guid, EAssetType.ITEM, magazineReplacement.legacyId);
                    flag = true;
                    break;
                }
            }
        }
        if (!flag)
        {
            asset = Assets.FindByGuidOrLegacyId(defaultMagazineGuid, EAssetType.ITEM, defaultMagazineLegacyId);
        }
        if (asset is SpawnAsset spawnAsset)
        {
            asset = SpawnTableTool.Resolve(spawnAsset, EAssetType.ITEM, OnGetDefaultMagazineSpawnTableErrorContext);
        }
        return asset as ItemMagazineAsset;
    }

    private string OnGetDefaultMagazineSpawnTableErrorContext()
    {
        return $"{GUID:N} default magazine";
    }

    private byte[] getMagazineState(ushort id)
    {
        return BitConverter.GetBytes(id);
    }

    internal float CalculateRoundsPerSecond()
    {
        return 50f / (float)Mathf.Max(1, firerate + 1);
    }

    public EffectAsset FindMuzzleEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(muzzleGuid, muzzle);
    }

    public EffectAsset FindShellEffectAsset()
    {
        return Assets.FindEffectAssetByGuidOrLegacyId(shellGuid, shell);
    }

    public void GrantShootQuestRewards(Player player)
    {
        shootQuestRewards.Grant(player);
    }

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _shoot = LoadRedirectableAsset<AudioClip>(p.bundle, "Shoot", p.data, "ShootAudioClip");
        _reload = LoadRedirectableAsset<AudioClip>(p.bundle, "Reload", p.data, "ReloadAudioClip");
        _hammer = LoadRedirectableAsset<AudioClip>(p.bundle, "Hammer", p.data, "HammerAudioClip");
        _aim = LoadRedirectableAsset<AudioClip>(p.bundle, "Aim", p.data, "AimAudioClip");
        _minigun = LoadRedirectableAsset<AudioClip>(p.bundle, "Minigun", p.data, "MinigunAudioClip");
        _chamberJammedSound = LoadRedirectableAsset<AudioClip>(p.bundle, "ChamberJammed", p.data, "ChamberJammedAudioClip");
        fireDelaySound = LoadRedirectableAsset<AudioClip>(p.bundle, "FireDelay", p.data, "FireDelayAudioClip");
        _projectile = p.bundle.load<GameObject>("Projectile");
        ammoMin = p.data.ParseUInt8("Ammo_Min", 0);
        ammoMax = p.data.ParseUInt8("Ammo_Max", 0);
        sightID = p.data.ParseUInt16("Sight", 0);
        tacticalID = p.data.ParseUInt16("Tactical", 0);
        gripID = p.data.ParseUInt16("Grip", 0);
        barrelID = p.data.ParseUInt16("Barrel", 0);
        defaultMagazineLegacyId = p.data.ParseGuidOrLegacyId("Magazine", out defaultMagazineGuid);
        int num = p.data.ParseInt32("Magazine_Replacements");
        magazineReplacements = new MagazineReplacement[num];
        for (int i = 0; i < num; i++)
        {
            Guid guid;
            ushort legacyId = p.data.ParseGuidOrLegacyId("Magazine_Replacement_" + i + "_ID", out guid);
            string @string = p.data.GetString("Magazine_Replacement_" + i + "_Map");
            MagazineReplacement magazineReplacement = default(MagazineReplacement);
            magazineReplacement.legacyId = legacyId;
            magazineReplacement.guid = guid;
            magazineReplacement.map = @string;
            magazineReplacements[i] = magazineReplacement;
        }
        unplace = p.data.ParseFloat("Unplace");
        replace = p.data.ParseFloat("Replace", 1f);
        RechamberAfterShotDelay = p.data.ParseFloat("RechamberAfterShotDelay", 0.25f);
        EjectAfterHammerDelay = p.data.ParseFloat("EjectAfterHammerDelay", 0.45f);
        EjectAfterReloadDelay = p.data.ParseFloat("EjectAfterReloadDelay", 0.5f);
        hasSight = p.data.ContainsKey("Hook_Sight");
        hasTactical = p.data.ContainsKey("Hook_Tactical");
        hasGrip = p.data.ContainsKey("Hook_Grip");
        hasBarrel = p.data.ContainsKey("Hook_Barrel");
        int num2 = p.data.ParseInt32("Magazine_Calibers");
        if (num2 > 0)
        {
            magazineCalibers = new ushort[num2];
            for (int j = 0; j < num2; j++)
            {
                magazineCalibers[j] = p.data.ParseUInt16("Magazine_Caliber_" + j, 0);
            }
            int num3 = p.data.ParseInt32("Attachment_Calibers");
            if (num3 > 0)
            {
                attachmentCalibers = new ushort[num3];
                for (int k = 0; k < num3; k++)
                {
                    attachmentCalibers[k] = p.data.ParseUInt16("Attachment_Caliber_" + k, 0);
                }
            }
            else
            {
                attachmentCalibers = magazineCalibers;
            }
        }
        else
        {
            magazineCalibers = new ushort[1];
            magazineCalibers[0] = p.data.ParseUInt16("Caliber", 0);
            attachmentCalibers = magazineCalibers;
        }
        firerate = p.data.ParseUInt8("Firerate", 0);
        action = (EAction)Enum.Parse(typeof(EAction), p.data.GetString("Action"), ignoreCase: true);
        if (p.data.ContainsKey("Delete_Empty_Magazines"))
        {
            shouldDeleteEmptyMagazines = true;
        }
        else
        {
            bool defaultValue = action == EAction.Pump || action == EAction.Rail || action == EAction.String || action == EAction.Rocket || action == EAction.Break;
            shouldDeleteEmptyMagazines = p.data.ParseBool("Should_Delete_Empty_Magazines", defaultValue);
        }
        requiresNonZeroAttachmentCaliber = p.data.ParseBool("Requires_NonZero_Attachment_Caliber");
        bursts = p.data.ParseInt32("Bursts");
        hasSafety = p.data.ContainsKey("Safety");
        hasSemi = p.data.ContainsKey("Semi");
        hasAuto = p.data.ContainsKey("Auto");
        hasBurst = bursts > 0;
        isTurret = p.data.ContainsKey("Turret");
        driverTurretViewmodelMode = p.data.ParseEnum("DriverTurretViewmodelMode", EDriverTurretViewmodelMode.OffscreenWhileAiming);
        if (hasAuto)
        {
            firemode = EFiremode.AUTO;
        }
        else if (hasSemi)
        {
            firemode = EFiremode.SEMI;
        }
        else if (hasBurst)
        {
            firemode = EFiremode.BURST;
        }
        else if (hasSafety)
        {
            firemode = EFiremode.SAFETY;
        }
        spreadAim = p.data.ParseFloat("Spread_Aim");
        if (p.data.ContainsKey("Spread_Angle_Degrees"))
        {
            baseSpreadAngleRadians = MathF.PI / 180f * p.data.ParseFloat("Spread_Angle_Degrees");
            spreadHip = Mathf.Tan(baseSpreadAngleRadians);
        }
        else
        {
            spreadHip = p.data.ParseFloat("Spread_Hip");
            baseSpreadAngleRadians = Mathf.Atan(spreadHip);
            if ((bool)shouldLogSpreadConversion)
            {
                UnturnedLog.info($"Converted \"{FriendlyName}\" Spread_Hip {spreadHip} to {baseSpreadAngleRadians * 57.29578f} degrees");
            }
        }
        spreadSprint = p.data.ParseFloat("Spread_Sprint", 1.25f);
        spreadCrouch = p.data.ParseFloat("Spread_Crouch", 0.85f);
        spreadProne = p.data.ParseFloat("Spread_Prone", 0.7f);
        spreadSwimming = p.data.ParseFloat("Spread_Swimming", 1.1f);
        spreadMidair = p.data.ParseFloat("Spread_Midair", 1.5f);
        recoilMin_x = p.data.ParseFloat("Recoil_Min_X");
        recoilMin_y = p.data.ParseFloat("Recoil_Min_Y");
        recoilMax_x = p.data.ParseFloat("Recoil_Max_X");
        recoilMax_y = p.data.ParseFloat("Recoil_Max_Y");
        aimingRecoilMultiplier = p.data.ParseFloat("Aiming_Recoil_Multiplier", 1f);
        recover_x = p.data.ParseFloat("Recover_X");
        recover_y = p.data.ParseFloat("Recover_Y");
        recoilSprint = p.data.ParseFloat("Recoil_Sprint", 1.25f);
        recoilCrouch = p.data.ParseFloat("Recoil_Crouch", 0.85f);
        recoilProne = p.data.ParseFloat("Recoil_Prone", 0.7f);
        recoilSwimming = p.data.ParseFloat("Recoil_Swimming", 1.1f);
        recoilMidair = p.data.ParseFloat("Recoil_Midair", 1f);
        shakeMin_x = p.data.ParseFloat("Shake_Min_X");
        shakeMin_y = p.data.ParseFloat("Shake_Min_Y");
        shakeMin_z = p.data.ParseFloat("Shake_Min_Z");
        shakeMax_x = p.data.ParseFloat("Shake_Max_X");
        shakeMax_y = p.data.ParseFloat("Shake_Max_Y");
        shakeMax_z = p.data.ParseFloat("Shake_Max_Z");
        ballisticSteps = p.data.ParseUInt8("Ballistic_Steps", 0);
        ballisticTravel = p.data.ParseFloat("Ballistic_Travel");
        bool flag = p.data.ContainsKey("Ballistic_Steps") && ballisticSteps > 0;
        bool flag2 = p.data.ContainsKey("Ballistic_Travel") && ballisticTravel > 0.1f;
        if (flag && flag2)
        {
            float num4 = Mathf.Abs((float)(int)ballisticSteps * ballisticTravel - range);
            if (num4 > 0.1f)
            {
                Assets.ReportError(this, "range and manual ballistic range are mismatched by " + num4 + "m. Recommended to only have one or the other specified!");
            }
        }
        else if (flag)
        {
            ballisticTravel = range / (float)(int)ballisticSteps;
        }
        else if (flag2)
        {
            ballisticSteps = (byte)Mathf.CeilToInt(range / ballisticTravel);
        }
        else
        {
            ballisticTravel = 10f;
            ballisticSteps = (byte)Mathf.CeilToInt(range / ballisticTravel);
        }
        muzzleVelocity = ballisticTravel * (float)PlayerInput.TOCK_PER_SECOND;
        if (p.data.TryParseFloat("Ballistic_Drop", out var value))
        {
            if (value < 1E-06f)
            {
                bulletGravityMultiplier = 0f;
            }
            else
            {
                float num5 = 0f;
                Vector2 right = Vector2.right;
                for (int l = 0; l < ballisticSteps; l++)
                {
                    num5 += right.y * ballisticTravel;
                    right.y -= value;
                    right.Normalize();
                }
                float num6 = (float)(int)ballisticSteps * 0.02f;
                float num7 = 2f * num5 / (num6 * num6);
                bulletGravityMultiplier = num7 / -9.81f;
                if ((bool)shouldLogBallisticDropConversion)
                {
                    UnturnedLog.info($"Converted \"{FriendlyName}\" Ballistic_Drop {value} to Bullet_Gravity_Multiplier {bulletGravityMultiplier}");
                }
            }
        }
        else
        {
            bulletGravityMultiplier = p.data.ParseFloat("Bullet_Gravity_Multiplier", 4f);
        }
        if (p.data.ContainsKey("Ballistic_Force"))
        {
            ballisticForce = p.data.ParseFloat("Ballistic_Force");
        }
        else
        {
            ballisticForce = 0.002f;
        }
        damageFalloffRange = p.data.ParseFloat("Damage_Falloff_Range", 1f);
        damageFalloffMaxRange = p.data.ParseFloat("Damage_Falloff_Max_Range", 1f);
        damageFalloffMultiplier = p.data.ParseFloat("Damage_Falloff_Multiplier", 1f);
        projectileLifespan = p.data.ParseFloat("Projectile_Lifespan", 30f);
        projectilePenetrateBuildables = p.data.ContainsKey("Projectile_Penetrate_Buildables");
        projectileExplosionLaunchSpeed = p.data.ParseFloat("Projectile_Explosion_Launch_Speed", playerDamageMultiplier.damage * 0.1f);
        reloadTime = p.data.ParseFloat("Reload_Time");
        hammerTime = p.data.ParseFloat("Hammer_Time");
        muzzle = p.data.ParseGuidOrLegacyId("Muzzle", out muzzleGuid);
        explosion = p.data.ParseGuidOrLegacyId("Explosion", out projectileExplosionEffectGuid);
        if (p.data.ContainsKey("Shell"))
        {
            shell = p.data.ParseGuidOrLegacyId("Shell", out shellGuid);
        }
        else if (action == EAction.Pump || action == EAction.Break)
        {
            shellGuid = new Guid("0dc9bf936ce0409585fe9525287c7a7d");
        }
        else if (action != EAction.Rail)
        {
            shellGuid = new Guid("f380a6a6f41f422c9f5b9ac13e3b13e8");
        }
        if (p.data.ContainsKey("Alert_Radius"))
        {
            alertRadius = p.data.ParseFloat("Alert_Radius");
        }
        else
        {
            alertRadius = 48f;
        }
        if (p.data.ContainsKey("Range_Rangefinder"))
        {
            rangeRangefinder = p.data.ParseFloat("Range_Rangefinder");
        }
        else
        {
            rangeRangefinder = p.data.ParseFloat("Range");
        }
        instakillHeadshots = p.data.ParseBool("Instakill_Headshots");
        infiniteAmmo = p.data.ParseBool("Infinite_Ammo");
        ammoPerShot = p.data.ParseUInt8("Ammo_Per_Shot", 1);
        fireDelay = Mathf.RoundToInt(p.data.ParseFloat("Fire_Delay_Seconds") * (float)PlayerInput.TOCK_PER_SECOND);
        allowMagazineChange = p.data.ParseBool("Allow_Magazine_Change", defaultValue: true);
        canAimDuringSprint = p.data.ParseBool("Can_Aim_During_Sprint");
        aimingMovementSpeedMultiplier = p.data.ParseFloat("Aiming_Movement_Speed_Multiplier", canAimDuringSprint ? 1f : 0.75f);
        MustAimToShoot = p.data.ParseBool("Must_Aim_To_Shoot", action == EAction.Minigun);
        ShouldForceStopAimingAfterShooting = p.data.ParseBool("Stop_Aiming_After_Shooting");
        canEverJam = p.data.ContainsKey("Can_Ever_Jam");
        if (canEverJam)
        {
            jamQualityThreshold = p.data.ParseFloat("Jam_Quality_Threshold", 0.4f);
            jamMaxChance = p.data.ParseFloat("Jam_Max_Chance", 0.1f);
            unjamChamberAnimName = p.data.GetString("Unjam_Chamber_Anim", "UnjamChamber");
        }
        gunshotRolloffDistance = DatDictionaryEx.ParseFloat(defaultValue: (action == EAction.String) ? 16f : ((action != EAction.Rocket) ? 512f : 64f), dictionary: p.data, key: "Gunshot_Rolloff_Distance");
        shootQuestRewards.Parse(p.data, p.localization, this, "Shoot_Quest_Rewards", "Shoot_Quest_Reward_");
        aimInDuration = p.data.ParseFloat("Aim_In_Duration", 0.2f);
        shouldScaleAimAnimations = p.data.ParseBool("Scale_Aim_Animation_Speed", defaultValue: true);
        int defaultValue3 = ((action == EAction.Bolt || action == EAction.Pump) ? 1 : 0);
        RechamberAfterShotCount = p.data.ParseInt32("RechamberAfterShotCount", defaultValue3);
        CasingEjectCountAfterRechamberingAfterShooting = p.data.ParseInt32("CasingEjectCountAfterRechamberingAfterShooting", 1);
        int defaultValue4 = ((action == EAction.Break) ? ammoMax : 0);
        CasingEjectCountAfterReload = p.data.ParseInt32("CasingEjectCountAfterReload", defaultValue4);
        bool defaultValue5 = action == EAction.Trigger || action == EAction.Minigun;
        ShouldEjectCasingAfterShooting = p.data.ParseBool("EjectCasingAfterShooting", defaultValue5);
        RechamberAfterMagazineAttached = p.data.ParseEnum("RechamberAfterMagazineAttached", ERechamberGunAfterReloadMode.IfAmmoWasEmpty);
        RechamberAfterMagazineDetached = p.data.ParseEnum("RechamberAfterMagazineDetached", ERechamberGunAfterReloadMode.Always);
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Gun");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Aim_In_Duration", aimInDuration);
        orAddDeclaration.Append("Aiming_Movement_Speed_Multiplier", aimingMovementSpeedMultiplier);
        orAddDeclaration.Append("Alert_Radius", alertRadius);
        orAddDeclaration.Append("Can_Aim_During_Sprint", canAimDuringSprint);
        orAddDeclaration.Append("Must_Aim_To_Shoot", MustAimToShoot);
        orAddDeclaration.Append("Range_Rangefinder", rangeRangefinder);
        orAddDeclaration.Append("Attachment_Calibers", attachmentCalibers.Length);
        for (byte b = 0; b < attachmentCalibers.Length; b++)
        {
            CargoDeclaration cargoDeclaration = builder.AddDeclaration("Gun_AttachmentCaliber");
            cargoDeclaration.Append("GUID", GUID);
            cargoDeclaration.Append("Caliber", attachmentCalibers[b]);
        }
        orAddDeclaration.Append("Magazine_Calibers", magazineCalibers.Length);
        for (byte b2 = 0; b2 < magazineCalibers.Length; b2++)
        {
            CargoDeclaration cargoDeclaration2 = builder.AddDeclaration("Gun_MagazineCaliber");
            cargoDeclaration2.Append("GUID", GUID);
            cargoDeclaration2.Append("Caliber", magazineCalibers[b2]);
        }
        orAddDeclaration.Append("Requires_NonZero_Attachment_Caliber", requiresNonZeroAttachmentCaliber);
        orAddDeclaration.Append("Damage_Falloff_Max_Range", damageFalloffMaxRange);
        orAddDeclaration.Append("Damage_Falloff_Multiplier", damageFalloffMultiplier);
        orAddDeclaration.Append("Damage_Falloff_Range", damageFalloffRange);
        orAddDeclaration.Append("Instakill_Headshots", instakillHeadshots);
        orAddDeclaration.Append("Action", action);
        orAddDeclaration.Append("Auto", hasAuto);
        orAddDeclaration.Append("hasBurst", hasBurst);
        orAddDeclaration.Append("Bursts", bursts);
        orAddDeclaration.Append("fireDelay", fireDelay);
        orAddDeclaration.Append("Firerate", firerate);
        orAddDeclaration.Append("Safety", hasSafety);
        orAddDeclaration.Append("Semi", hasSemi);
        orAddDeclaration.Append("Barrel", barrelID);
        orAddDeclaration.Append("Grip", gripID);
        orAddDeclaration.Append("Sight", sightID);
        orAddDeclaration.Append("Tactical", tacticalID);
        orAddDeclaration.Append("Hook_Barrel", hasBarrel);
        orAddDeclaration.Append("Hook_Grip", hasGrip);
        orAddDeclaration.Append("Hook_Sight", hasSight);
        orAddDeclaration.Append("Hook_Tactical", hasTactical);
        orAddDeclaration.Append("Can_Ever_Jam", canEverJam);
        orAddDeclaration.Append("Jam_Quality_Threshold", jamQualityThreshold);
        orAddDeclaration.Append("Jam_Max_Chance", jamMaxChance);
        orAddDeclaration.Append("Allow_Magazine_Change", allowMagazineChange);
        orAddDeclaration.Append("Ammo_Max", ammoMax);
        orAddDeclaration.Append("Ammo_Min", ammoMin);
        orAddDeclaration.Append("Ammo_Per_Shot", ammoPerShot);
        orAddDeclaration.Append("Hammer_Time", hammerTime);
        orAddDeclaration.Append("Infinite_Ammo", infiniteAmmo);
        orAddDeclaration.Append("Magazine", defaultMagazineLegacyId);
        orAddDeclaration.Append("MagazineGUID", defaultMagazineGuid);
        orAddDeclaration.Append("Magazine_Replacements", magazineReplacements.Length);
        for (int i = 0; i < magazineReplacements.Length; i++)
        {
            CargoDeclaration cargoDeclaration3 = builder.AddDeclaration("Gun_MagazineReplacement");
            cargoDeclaration3.Append("GUID", GUID);
            cargoDeclaration3.Append("magazineReplacementIndex", i);
            cargoDeclaration3.Append("ID", magazineReplacements[i].legacyId);
            cargoDeclaration3.Append("MagazineGUID", magazineReplacements[i].guid);
            cargoDeclaration3.Append("Map", magazineReplacements[i].map);
        }
        orAddDeclaration.Append("Reload_Time", reloadTime);
        orAddDeclaration.Append("Replace", replace);
        orAddDeclaration.Append("Should_Delete_Empty_Magazines", shouldDeleteEmptyMagazines);
        orAddDeclaration.Append("Unplace", unplace);
        orAddDeclaration.Append("Ballistic_Steps", ballisticSteps);
        orAddDeclaration.Append("Ballistic_Travel", ballisticTravel);
        orAddDeclaration.Append("Bullet_Gravity_Multiplier", bulletGravityMultiplier);
        orAddDeclaration.Append("Ballistic_Force", ballisticForce);
        orAddDeclaration.Append("Projectile_Explosion_Launch_Speed", projectileExplosionLaunchSpeed);
        orAddDeclaration.Append("Projectile_Lifespan", projectileLifespan);
        orAddDeclaration.Append("Projectile_Penetrate_Buildables", projectilePenetrateBuildables);
        orAddDeclaration.Append("Aiming_Recoil_Multiplier", aimingRecoilMultiplier);
        orAddDeclaration.Append("Recoil_Crouch", recoilCrouch);
        orAddDeclaration.Append("Recoil_Max_X", recoilMax_x);
        orAddDeclaration.Append("Recoil_Max_Y", recoilMax_y);
        orAddDeclaration.Append("Recoil_Min_X", recoilMin_x);
        orAddDeclaration.Append("Recoil_Min_Y", recoilMin_y);
        orAddDeclaration.Append("Recoil_Midair", recoilMidair);
        orAddDeclaration.Append("Recoil_Prone", recoilProne);
        orAddDeclaration.Append("Recoil_Sprint", recoilSprint);
        orAddDeclaration.Append("Recoil_Swimming", recoilSwimming);
        orAddDeclaration.Append("Recover_X", recover_x);
        orAddDeclaration.Append("Recover_Y", recover_y);
        orAddDeclaration.Append("Shake_Max_X", shakeMax_x);
        orAddDeclaration.Append("Shake_Min_X", shakeMin_x);
        orAddDeclaration.Append("Shake_Max_Y", shakeMax_y);
        orAddDeclaration.Append("Shake_Min_Y", shakeMin_y);
        orAddDeclaration.Append("Shake_Max_Z", shakeMax_z);
        orAddDeclaration.Append("Shake_Min_Z", shakeMin_z);
        orAddDeclaration.Append("spreadAim", baseSpreadAngleRadians * spreadAim);
        orAddDeclaration.Append("baseSpreadAngleRadians", baseSpreadAngleRadians);
        orAddDeclaration.Append("Spread_Crouch", spreadCrouch);
        orAddDeclaration.Append("Spread_Midair", spreadMidair);
        orAddDeclaration.Append("Spread_Prone", spreadProne);
        orAddDeclaration.Append("Spread_Sprint", spreadSprint);
        orAddDeclaration.Append("Spread_Swimming", spreadSwimming);
    }

    protected override AudioReference GetDefaultInventoryAudio()
    {
        if (name.Contains("Bow", StringComparison.InvariantCultureIgnoreCase))
        {
            return base.GetDefaultInventoryAudio();
        }
        if (size_x <= 2 && size_y <= 2)
        {
            return new AudioReference("core.masterbundle", "Sounds/Inventory/SmallGunAttachment.asset");
        }
        return new AudioReference("core.masterbundle", "Sounds/Inventory/LargeGunAttachment.asset");
    }

    [Obsolete("Replaced by GetDefaultMagazineLegacyId")]
    public ushort getMagazineID()
    {
        return GetDefaultMagazineLegacyId();
    }
}
