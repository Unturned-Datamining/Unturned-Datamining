using System;
using UnityEngine;
using Unturned.SystemEx;

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

    private ushort magazineID;

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

    public bool requiresNonZeroAttachmentCaliber;

    public bool hasSafety;

    public bool hasSemi;

    public bool hasAuto;

    public bool hasBurst;

    public bool isTurret;

    public int bursts;

    internal EFiremode firemode;

    public float spreadAim;

    [Obsolete("Replaced by baseSpreadAngleRadians")]
    public float spreadHip;

    public float spreadSprint;

    public float spreadCrouch;

    public float spreadProne;

    public float recoilMin_x;

    public float recoilMin_y;

    public float recoilMax_x;

    public float recoilMax_y;

    public float aimingRecoilMultiplier;

    public float recoilSprint;

    public float recoilCrouch;

    public float recoilProne;

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

    public float damageFalloffRange;

    public float damageFalloffMaxRange;

    public float damageFalloffMultiplier;

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

    public float aimingMovementSpeedMultiplier;

    public INPCReward[] shootQuestRewards;

    public AudioClip shoot => _shoot;

    public AudioClip reload => _reload;

    public AudioClip hammer => _hammer;

    public AudioClip aim => _aim;

    public AudioClip minigun => _minigun;

    public AudioClip chamberJammedSound => _chamberJammedSound;

    public AudioClip fireDelaySound { get; protected set; }

    public float gunshotRolloffDistance { get; protected set; }

    public GameObject projectile => _projectile;

    public override bool shouldFriendlySentryTargetUser => true;

    public float rangeRangefinder { get; protected set; }

    public bool instakillHeadshots { get; protected set; }

    public bool infiniteAmmo { get; protected set; }

    public byte ammoPerShot { get; protected set; }

    public int fireDelay { get; protected set; }

    public bool allowMagazineChange { get; protected set; }

    public bool canAimDuringSprint { get; protected set; }

    public float aimInDuration { get; protected set; }

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

    public ushort[] attachmentCalibers { get; private set; }

    public ushort[] magazineCalibers { get; private set; }

    public float baseSpreadAngleRadians { get; private set; }

    public float muzzleVelocity { get; protected set; }

    public float bulletGravityMultiplier { get; protected set; }

    public override bool showQuality => true;

    public bool canEverJam { get; protected set; }

    public float jamQualityThreshold { get; protected set; }

    public float jamMaxChance { get; protected set; }

    public string unjamChamberAnimName { get; protected set; }

    protected override bool doesItemTypeHaveSkins => true;

    public override string getContext(string desc, byte[] state)
    {
        ushort num = BitConverter.ToUInt16(state, 8);
        desc = ((!(Assets.find(EAssetType.ITEM, num) is ItemMagazineAsset itemMagazineAsset)) ? (desc + PlayerDashboardInventoryUI.localization.format("Ammo", PlayerDashboardInventoryUI.localization.format("None"), 0, 0)) : (desc + PlayerDashboardInventoryUI.localization.format("Ammo", "<color=" + Palette.hex(ItemTool.getRarityColorUI(itemMagazineAsset.rarity)) + ">" + itemMagazineAsset.itemName + "</color>", state[10], itemMagazineAsset.amount)));
        desc += "\n\n";
        return desc;
    }

    public override byte[] getState(EItemOrigin origin)
    {
        byte[] magazineState = getMagazineState(getMagazineID());
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

    public ushort getMagazineID()
    {
        if (Level.info != null && magazineReplacements != null)
        {
            MagazineReplacement[] array = magazineReplacements;
            for (int i = 0; i < array.Length; i++)
            {
                MagazineReplacement magazineReplacement = array[i];
                if (magazineReplacement.map == Level.info.name)
                {
                    return magazineReplacement.id;
                }
            }
        }
        return magazineID;
    }

    private byte[] getMagazineState(ushort id)
    {
        return BitConverter.GetBytes(id);
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
        if (shootQuestRewards != null)
        {
            INPCReward[] array = shootQuestRewards;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].grantReward(player, shouldSend: true);
            }
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _shoot = bundle.load<AudioClip>("Shoot");
        _reload = bundle.load<AudioClip>("Reload");
        _hammer = bundle.load<AudioClip>("Hammer");
        _aim = bundle.load<AudioClip>("Aim");
        _minigun = bundle.load<AudioClip>("Minigun");
        _chamberJammedSound = bundle.load<AudioClip>("ChamberJammed");
        fireDelaySound = bundle.load<AudioClip>("FireDelay");
        _projectile = bundle.load<GameObject>("Projectile");
        ammoMin = data.ParseUInt8("Ammo_Min", 0);
        ammoMax = data.ParseUInt8("Ammo_Max", 0);
        sightID = data.ParseUInt16("Sight", 0);
        tacticalID = data.ParseUInt16("Tactical", 0);
        gripID = data.ParseUInt16("Grip", 0);
        barrelID = data.ParseUInt16("Barrel", 0);
        magazineID = data.ParseUInt16("Magazine", 0);
        int num = data.ParseInt32("Magazine_Replacements");
        magazineReplacements = new MagazineReplacement[num];
        for (int i = 0; i < num; i++)
        {
            ushort num2 = data.ParseUInt16("Magazine_Replacement_" + i + "_ID", 0);
            string @string = data.GetString("Magazine_Replacement_" + i + "_Map");
            MagazineReplacement magazineReplacement = default(MagazineReplacement);
            magazineReplacement.id = num2;
            magazineReplacement.map = @string;
            magazineReplacements[i] = magazineReplacement;
        }
        unplace = data.ParseFloat("Unplace");
        replace = data.ParseFloat("Replace", 1f);
        hasSight = data.ContainsKey("Hook_Sight");
        hasTactical = data.ContainsKey("Hook_Tactical");
        hasGrip = data.ContainsKey("Hook_Grip");
        hasBarrel = data.ContainsKey("Hook_Barrel");
        int num3 = data.ParseInt32("Magazine_Calibers");
        if (num3 > 0)
        {
            magazineCalibers = new ushort[num3];
            for (int j = 0; j < num3; j++)
            {
                magazineCalibers[j] = data.ParseUInt16("Magazine_Caliber_" + j, 0);
            }
            int num4 = data.ParseInt32("Attachment_Calibers");
            if (num4 > 0)
            {
                attachmentCalibers = new ushort[num4];
                for (int k = 0; k < num4; k++)
                {
                    attachmentCalibers[k] = data.ParseUInt16("Attachment_Caliber_" + k, 0);
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
            magazineCalibers[0] = data.ParseUInt16("Caliber", 0);
            attachmentCalibers = magazineCalibers;
        }
        firerate = data.ParseUInt8("Firerate", 0);
        action = (EAction)Enum.Parse(typeof(EAction), data.GetString("Action"), ignoreCase: true);
        if (data.ContainsKey("Delete_Empty_Magazines"))
        {
            shouldDeleteEmptyMagazines = true;
        }
        else
        {
            bool defaultValue = action == EAction.Pump || action == EAction.Rail || action == EAction.String || action == EAction.Rocket || action == EAction.Break;
            shouldDeleteEmptyMagazines = data.ParseBool("Should_Delete_Empty_Magazines", defaultValue);
        }
        requiresNonZeroAttachmentCaliber = data.ParseBool("Requires_NonZero_Attachment_Caliber");
        bursts = data.ParseInt32("Bursts");
        hasSafety = data.ContainsKey("Safety");
        hasSemi = data.ContainsKey("Semi");
        hasAuto = data.ContainsKey("Auto");
        hasBurst = bursts > 0;
        isTurret = data.ContainsKey("Turret");
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
        spreadAim = data.ParseFloat("Spread_Aim");
        if (data.ContainsKey("Spread_Angle_Degrees"))
        {
            baseSpreadAngleRadians = (float)Math.PI / 180f * data.ParseFloat("Spread_Angle_Degrees");
            spreadHip = Mathf.Tan(baseSpreadAngleRadians);
        }
        else
        {
            spreadHip = data.ParseFloat("Spread_Hip");
            baseSpreadAngleRadians = Mathf.Atan(spreadHip);
            if ((bool)Assets.shouldValidateAssets)
            {
                UnturnedLog.info($"Converted \"{FriendlyName}\" Spread_Hip {spreadHip} to {baseSpreadAngleRadians * 57.29578f} degrees");
            }
        }
        spreadSprint = data.ParseFloat("Spread_Sprint", 1.25f);
        spreadCrouch = data.ParseFloat("Spread_Crouch", 0.85f);
        spreadProne = data.ParseFloat("Spread_Prone", 0.7f);
        recoilMin_x = data.ParseFloat("Recoil_Min_X");
        recoilMin_y = data.ParseFloat("Recoil_Min_Y");
        recoilMax_x = data.ParseFloat("Recoil_Max_X");
        recoilMax_y = data.ParseFloat("Recoil_Max_Y");
        aimingRecoilMultiplier = data.ParseFloat("Aiming_Recoil_Multiplier", 1f);
        recover_x = data.ParseFloat("Recover_X");
        recover_y = data.ParseFloat("Recover_Y");
        recoilSprint = data.ParseFloat("Recoil_Sprint", 1.25f);
        recoilCrouch = data.ParseFloat("Recoil_Crouch", 0.85f);
        recoilProne = data.ParseFloat("Recoil_Prone", 0.7f);
        shakeMin_x = data.ParseFloat("Shake_Min_X");
        shakeMin_y = data.ParseFloat("Shake_Min_Y");
        shakeMin_z = data.ParseFloat("Shake_Min_Z");
        shakeMax_x = data.ParseFloat("Shake_Max_X");
        shakeMax_y = data.ParseFloat("Shake_Max_Y");
        shakeMax_z = data.ParseFloat("Shake_Max_Z");
        ballisticSteps = data.ParseUInt8("Ballistic_Steps", 0);
        ballisticTravel = data.ParseFloat("Ballistic_Travel");
        bool flag = data.ContainsKey("Ballistic_Steps") && ballisticSteps > 0;
        bool flag2 = data.ContainsKey("Ballistic_Travel") && ballisticTravel > 0.1f;
        if (flag && flag2)
        {
            float num5 = Mathf.Abs((float)(int)ballisticSteps * ballisticTravel - range);
            if (num5 > 0.1f)
            {
                Assets.reportError(this, "range and manual ballistic range are mistmached by " + num5 + "m. Recommended to only have one or the other specified!");
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
        if (data.TryParseFloat("Ballistic_Drop", out var value) && value < 0.001f)
        {
            bulletGravityMultiplier = 0f;
        }
        else
        {
            bulletGravityMultiplier = data.ParseFloat("Bullet_Gravity_Multiplier", 4f);
        }
        if (data.ContainsKey("Ballistic_Force"))
        {
            ballisticForce = data.ParseFloat("Ballistic_Force");
        }
        else
        {
            ballisticForce = 0.002f;
        }
        damageFalloffRange = data.ParseFloat("Damage_Falloff_Range", 1f);
        damageFalloffMaxRange = data.ParseFloat("Damage_Falloff_Max_Range", 1f);
        damageFalloffMultiplier = data.ParseFloat("Damage_Falloff_Multiplier", 1f);
        projectileLifespan = data.ParseFloat("Projectile_Lifespan", 30f);
        projectilePenetrateBuildables = data.ContainsKey("Projectile_Penetrate_Buildables");
        projectileExplosionLaunchSpeed = data.ParseFloat("Projectile_Explosion_Launch_Speed", playerDamageMultiplier.damage * 0.1f);
        reloadTime = data.ParseFloat("Reload_Time");
        hammerTime = data.ParseFloat("Hammer_Time");
        muzzle = data.ParseGuidOrLegacyId("Muzzle", out muzzleGuid);
        explosion = data.ParseGuidOrLegacyId("Explosion", out projectileExplosionEffectGuid);
        if (data.ContainsKey("Shell"))
        {
            shell = data.ParseGuidOrLegacyId("Shell", out shellGuid);
        }
        else if (action == EAction.Pump || action == EAction.Break)
        {
            shellGuid = new Guid("0dc9bf936ce0409585fe9525287c7a7d");
        }
        else if (action != EAction.Rail)
        {
            shellGuid = new Guid("f380a6a6f41f422c9f5b9ac13e3b13e8");
        }
        if (data.ContainsKey("Alert_Radius"))
        {
            alertRadius = data.ParseFloat("Alert_Radius");
        }
        else
        {
            alertRadius = 48f;
        }
        if (data.ContainsKey("Range_Rangefinder"))
        {
            rangeRangefinder = data.ParseFloat("Range_Rangefinder");
        }
        else
        {
            rangeRangefinder = data.ParseFloat("Range");
        }
        instakillHeadshots = data.ParseBool("Instakill_Headshots");
        infiniteAmmo = data.ParseBool("Infinite_Ammo");
        ammoPerShot = data.ParseUInt8("Ammo_Per_Shot", 1);
        fireDelay = Mathf.RoundToInt(data.ParseFloat("Fire_Delay_Seconds") * (float)PlayerInput.TOCK_PER_SECOND);
        allowMagazineChange = data.ParseBool("Allow_Magazine_Change", defaultValue: true);
        canAimDuringSprint = data.ParseBool("Can_Aim_During_Sprint");
        aimingMovementSpeedMultiplier = data.ParseFloat("Aiming_Movement_Speed_Multiplier", canAimDuringSprint ? 1f : 0.75f);
        canEverJam = data.ContainsKey("Can_Ever_Jam");
        if (canEverJam)
        {
            jamQualityThreshold = data.ParseFloat("Jam_Quality_Threshold", 0.4f);
            jamMaxChance = data.ParseFloat("Jam_Max_Chance", 0.1f);
            unjamChamberAnimName = data.GetString("Unjam_Chamber_Anim", "UnjamChamber");
        }
        float defaultValue2 = ((action == EAction.String) ? 16f : ((action != EAction.Rocket) ? 512f : 64f));
        gunshotRolloffDistance = data.ParseFloat("Gunshot_Rolloff_Distance", defaultValue2);
        int num6 = data.ParseInt32("Shoot_Quest_Rewards");
        if (num6 > 0)
        {
            shootQuestRewards = new INPCReward[num6];
            NPCTool.readRewards(data, localization, "Shoot_Quest_Reward_", shootQuestRewards, this);
        }
        aimInDuration = data.ParseFloat("Aim_In_Duration", 0.2f);
        shouldScaleAimAnimations = data.ParseBool("Scale_Aim_Animation_Speed", defaultValue: true);
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
}
