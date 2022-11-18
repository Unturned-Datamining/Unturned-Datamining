using System;
using SDG.Framework.IO.FormattedFiles;
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

    public float spreadHip;

    public float spreadSprint;

    public float spreadCrouch;

    public float spreadProne;

    public float recoilAim;

    public bool useRecoilAim;

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

    public float ballisticDrop;

    public float ballisticForce;

    public float damageFalloffRange;

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

    protected override void readAsset(IFormattedFileReader reader)
    {
        base.readAsset(reader);
        ammoMin = reader.readValue<byte>("Ammo_Min");
        ammoMax = reader.readValue<byte>("Ammo_Max");
        sightID = reader.readValue<ushort>("Sight_ID");
        tacticalID = reader.readValue<ushort>("Tactical_ID");
        gripID = reader.readValue<ushort>("Grip_ID");
        barrelID = reader.readValue<ushort>("Barrel_ID");
        magazineID = reader.readValue<ushort>("Magazine_ID");
        unplace = reader.readValue<float>("Unplace");
        replace = reader.readValue<float>("Replace");
        if ((double)replace < 0.01)
        {
            replace = 1f;
        }
        hasSight = reader.readValue<bool>("Hook_Sight");
        hasTactical = reader.readValue<bool>("Hook_Tactical");
        hasGrip = reader.readValue<bool>("Hook_Grip");
        hasBarrel = reader.readValue<bool>("Hook_Barrel");
        int num = reader.readArrayLength("Magazine_Calibers");
        if (num > 0)
        {
            magazineCalibers = new ushort[num];
            for (int i = 0; i < num; i++)
            {
                magazineCalibers[i] = reader.readValue<ushort>(i);
            }
            int num2 = reader.readArrayLength("Attachment_Calibers");
            if (num2 > 0)
            {
                attachmentCalibers = new ushort[num2];
                for (int j = 0; j < num2; j++)
                {
                    attachmentCalibers[j] = reader.readValue<ushort>(j);
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
            magazineCalibers[0] = reader.readValue<ushort>("Caliber");
            attachmentCalibers = magazineCalibers;
        }
        firerate = reader.readValue<byte>("Firerate");
        action = reader.readValue<EAction>("Action");
        shouldDeleteEmptyMagazines = reader.readValue<bool>("Delete_Empty_Magazines");
        bursts = reader.readValue<int>("Bursts");
        hasSafety = reader.readValue<bool>("Safety");
        hasSemi = reader.readValue<bool>("Semi");
        hasAuto = reader.readValue<bool>("Auto");
        hasBurst = bursts > 0;
        isTurret = reader.readValue<bool>("Turret");
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
        spreadAim = reader.readValue<float>("Spread_Aim");
        spreadHip = reader.readValue<float>("Spread_Hip");
        recoilAim = reader.readValue<float>("Recoil_Aim");
        useRecoilAim = reader.readValue<bool>("Use_Recoil_Aim");
        recoilMin_x = reader.readValue<float>("Recoil_Min_X");
        recoilMin_y = reader.readValue<float>("Recoil_Min_Y");
        recoilMax_x = reader.readValue<float>("Recoil_Max_X");
        recoilMax_y = reader.readValue<float>("Recoil_Max_Y");
        recover_x = reader.readValue<float>("Recover_X");
        recover_y = reader.readValue<float>("Recover_Y");
        shakeMin_x = reader.readValue<float>("Shake_Min_X");
        shakeMin_y = reader.readValue<float>("Shake_Min_Y");
        shakeMin_z = reader.readValue<float>("Shake_Min_Z");
        shakeMax_x = reader.readValue<float>("Shake_Max_X");
        shakeMax_y = reader.readValue<float>("Shake_Max_Y");
        shakeMax_z = reader.readValue<float>("Shake_Max_Z");
        ballisticSteps = reader.readValue<byte>("Ballistic_Steps");
        ballisticTravel = (int)reader.readValue<byte>("Ballistic_Travel");
        ballisticDrop = reader.readValue<float>("Ballistic_Drop");
        ballisticForce = reader.readValue<float>("Ballistic_Force");
        projectilePenetrateBuildables = reader.readValue<bool>("Projectile_Penetrate_Buildables");
        reloadTime = reader.readValue<float>("Reload_Time");
        hammerTime = reader.readValue<float>("Hammer_Time");
        if (reader.containsKey("Alert_Radius"))
        {
            alertRadius = reader.readValue<float>("Alert_Radius");
        }
        else
        {
            alertRadius = 48f;
        }
    }

    protected override void writeAsset(IFormattedFileWriter writer)
    {
        base.writeAsset(writer);
        writer.writeValue("Ammo_Min", ammoMin);
        writer.writeValue("Ammo_Max", ammoMax);
        writer.writeValue("Sight_ID", sightID);
        writer.writeValue("Tactical_ID", tacticalID);
        writer.writeValue("Grip_ID", gripID);
        writer.writeValue("Barrel_ID", barrelID);
        writer.writeValue("Magazine_ID", magazineID);
        writer.writeValue("Unplace", unplace);
        writer.writeValue("Replace", replace);
        writer.writeValue("Hook_Sight", hasSight);
        writer.writeValue("Hook_Tactical", hasTactical);
        writer.writeValue("Hook_Grip", hasGrip);
        writer.writeValue("Hook_Barrel", hasBarrel);
        writer.beginArray("Magazine_Calibers");
        ushort[] array = magazineCalibers;
        foreach (ushort value in array)
        {
            writer.writeValue(value);
        }
        writer.endArray();
        writer.beginArray("Attachment_Calibers");
        array = attachmentCalibers;
        foreach (ushort value2 in array)
        {
            writer.writeValue(value2);
        }
        writer.endArray();
        writer.writeValue("Firerate", firerate);
        writer.writeValue("Action", action);
        writer.writeValue("Delete_Empty_Magazines", shouldDeleteEmptyMagazines);
        writer.writeValue("Bursts", bursts);
        writer.writeValue("Safety", hasSafety);
        writer.writeValue("Semi", hasSemi);
        writer.writeValue("Auto", hasAuto);
        writer.writeValue("Turret", isTurret);
        writer.writeValue("Spread_Aim", spreadAim);
        writer.writeValue("Spread_Hip", spreadHip);
        writer.writeValue("Recoil_Aim", recoilAim);
        writer.writeValue("Use_Recoil_Aim", useRecoilAim);
        writer.writeValue("Recoil_Min_X", recoilMin_x);
        writer.writeValue("Recoil_Min_Y", recoilMin_y);
        writer.writeValue("Recoil_Max_X", recoilMax_x);
        writer.writeValue("Recoil_Max_Y", recoilMax_y);
        writer.writeValue("Recover_X", recover_x);
        writer.writeValue("Recover_Y", recover_y);
        writer.writeValue("Shake_Min_X", shakeMin_x);
        writer.writeValue("Shake_Min_Y", shakeMin_y);
        writer.writeValue("Shake_Min_Z", shakeMin_z);
        writer.writeValue("Shake_Max_X", shakeMax_x);
        writer.writeValue("Shake_Max_Y", shakeMax_y);
        writer.writeValue("Shake_Max_Z", shakeMax_z);
        writer.writeValue("Ballistic_Steps", ballisticSteps);
        writer.writeValue("Ballistic_Travel", ballisticTravel);
        writer.writeValue("Ballistic_Drop", ballisticDrop);
        writer.writeValue("Ballistic_Force", ballisticForce);
        writer.writeValue("Projectile_Penetrate_Buildables", projectilePenetrateBuildables);
        writer.writeValue("Reload_Time", reloadTime);
        writer.writeValue("Hammer_Time", hammerTime);
        writer.writeValue("Alert_Radius", alertRadius);
    }

    public ItemGunAsset()
    {
    }

    public ItemGunAsset(Bundle bundle, Local localization, byte[] hash)
        : base(bundle, localization, hash)
    {
        _shoot = bundle.load<AudioClip>("Shoot");
        _reload = bundle.load<AudioClip>("Reload");
        _hammer = bundle.load<AudioClip>("Hammer");
        _aim = bundle.load<AudioClip>("Aim");
        _minigun = bundle.load<AudioClip>("Minigun");
        _chamberJammedSound = bundle.load<AudioClip>("ChamberJammed");
        fireDelaySound = bundle.load<AudioClip>("FireDelay");
        _projectile = bundle.load<GameObject>("Projectile");
        allowMagazineChange = true;
        canAimDuringSprint = false;
    }

    public ItemGunAsset(Bundle bundle, Data data, Local localization, ushort id)
        : base(bundle, data, localization, id)
    {
        _shoot = bundle.load<AudioClip>("Shoot");
        _reload = bundle.load<AudioClip>("Reload");
        _hammer = bundle.load<AudioClip>("Hammer");
        _aim = bundle.load<AudioClip>("Aim");
        _minigun = bundle.load<AudioClip>("Minigun");
        _chamberJammedSound = bundle.load<AudioClip>("ChamberJammed");
        fireDelaySound = bundle.load<AudioClip>("FireDelay");
        _projectile = bundle.load<GameObject>("Projectile");
        ammoMin = data.readByte("Ammo_Min", 0);
        ammoMax = data.readByte("Ammo_Max", 0);
        sightID = data.readUInt16("Sight", 0);
        tacticalID = data.readUInt16("Tactical", 0);
        gripID = data.readUInt16("Grip", 0);
        barrelID = data.readUInt16("Barrel", 0);
        magazineID = data.readUInt16("Magazine", 0);
        int num = data.readInt32("Magazine_Replacements");
        magazineReplacements = new MagazineReplacement[num];
        for (int i = 0; i < num; i++)
        {
            ushort num2 = data.readUInt16("Magazine_Replacement_" + i + "_ID", 0);
            string map = data.readString("Magazine_Replacement_" + i + "_Map");
            MagazineReplacement magazineReplacement = new MagazineReplacement
            {
                id = num2,
                map = map
            };
            magazineReplacements[i] = magazineReplacement;
        }
        unplace = data.readSingle("Unplace");
        replace = data.readSingle("Replace", 1f);
        hasSight = data.has("Hook_Sight");
        hasTactical = data.has("Hook_Tactical");
        hasGrip = data.has("Hook_Grip");
        hasBarrel = data.has("Hook_Barrel");
        int num3 = data.readInt32("Magazine_Calibers");
        if (num3 > 0)
        {
            magazineCalibers = new ushort[num3];
            for (int j = 0; j < num3; j++)
            {
                magazineCalibers[j] = data.readUInt16("Magazine_Caliber_" + j, 0);
            }
            int num4 = data.readInt32("Attachment_Calibers");
            if (num4 > 0)
            {
                attachmentCalibers = new ushort[num4];
                for (int k = 0; k < num4; k++)
                {
                    attachmentCalibers[k] = data.readUInt16("Attachment_Caliber_" + k, 0);
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
            magazineCalibers[0] = data.readUInt16("Caliber", 0);
            attachmentCalibers = magazineCalibers;
        }
        firerate = data.readByte("Firerate", 0);
        action = (EAction)Enum.Parse(typeof(EAction), data.readString("Action"), ignoreCase: true);
        if (data.has("Delete_Empty_Magazines"))
        {
            shouldDeleteEmptyMagazines = true;
        }
        else
        {
            bool defaultValue = action == EAction.Pump || action == EAction.Rail || action == EAction.String || action == EAction.Rocket || action == EAction.Break;
            shouldDeleteEmptyMagazines = data.readBoolean("Should_Delete_Empty_Magazines", defaultValue);
        }
        requiresNonZeroAttachmentCaliber = data.readBoolean("Requires_NonZero_Attachment_Caliber");
        bursts = data.readInt32("Bursts");
        hasSafety = data.has("Safety");
        hasSemi = data.has("Semi");
        hasAuto = data.has("Auto");
        hasBurst = bursts > 0;
        isTurret = data.has("Turret");
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
        spreadAim = data.readSingle("Spread_Aim");
        spreadHip = data.readSingle("Spread_Hip");
        spreadSprint = data.readSingle("Spread_Sprint", 1.25f);
        spreadCrouch = data.readSingle("Spread_Crouch", 0.85f);
        spreadProne = data.readSingle("Spread_Prone", 0.7f);
        if (data.has("Recoil_Aim"))
        {
            recoilAim = data.readSingle("Recoil_Aim");
            useRecoilAim = true;
        }
        else
        {
            recoilAim = 1f;
            useRecoilAim = false;
        }
        recoilMin_x = data.readSingle("Recoil_Min_X");
        recoilMin_y = data.readSingle("Recoil_Min_Y");
        recoilMax_x = data.readSingle("Recoil_Max_X");
        recoilMax_y = data.readSingle("Recoil_Max_Y");
        aimingRecoilMultiplier = data.readSingle("Aiming_Recoil_Multiplier", 1f);
        recover_x = data.readSingle("Recover_X");
        recover_y = data.readSingle("Recover_Y");
        recoilSprint = data.readSingle("Recoil_Sprint", 1.25f);
        recoilCrouch = data.readSingle("Recoil_Crouch", 0.85f);
        recoilProne = data.readSingle("Recoil_Prone", 0.7f);
        shakeMin_x = data.readSingle("Shake_Min_X");
        shakeMin_y = data.readSingle("Shake_Min_Y");
        shakeMin_z = data.readSingle("Shake_Min_Z");
        shakeMax_x = data.readSingle("Shake_Max_X");
        shakeMax_y = data.readSingle("Shake_Max_Y");
        shakeMax_z = data.readSingle("Shake_Max_Z");
        ballisticSteps = data.readByte("Ballistic_Steps", 0);
        ballisticTravel = data.readSingle("Ballistic_Travel");
        bool flag = data.has("Ballistic_Steps") && ballisticSteps > 0;
        bool flag2 = data.has("Ballistic_Travel") && ballisticTravel > 0.1f;
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
        if (data.has("Ballistic_Drop"))
        {
            ballisticDrop = data.readSingle("Ballistic_Drop");
        }
        else
        {
            ballisticDrop = 0.002f;
        }
        if (data.has("Ballistic_Force"))
        {
            ballisticForce = data.readSingle("Ballistic_Force");
        }
        else
        {
            ballisticForce = 0.002f;
        }
        damageFalloffRange = data.readSingle("Damage_Falloff_Range", 1f);
        damageFalloffMultiplier = data.readSingle("Damage_Falloff_Multiplier", 1f);
        projectileLifespan = data.readSingle("Projectile_Lifespan", 30f);
        projectilePenetrateBuildables = data.has("Projectile_Penetrate_Buildables");
        projectileExplosionLaunchSpeed = data.readSingle("Projectile_Explosion_Launch_Speed", playerDamageMultiplier.damage * 0.1f);
        reloadTime = data.readSingle("Reload_Time");
        hammerTime = data.readSingle("Hammer_Time");
        muzzle = data.ReadGuidOrLegacyId("Muzzle", out muzzleGuid);
        explosion = data.ReadGuidOrLegacyId("Explosion", out projectileExplosionEffectGuid);
        if (data.has("Shell"))
        {
            shell = data.ReadGuidOrLegacyId("Shell", out shellGuid);
        }
        else if (action == EAction.Pump || action == EAction.Break)
        {
            shellGuid = new Guid("0dc9bf936ce0409585fe9525287c7a7d");
        }
        else if (action != EAction.Rail)
        {
            shellGuid = new Guid("f380a6a6f41f422c9f5b9ac13e3b13e8");
        }
        if (data.has("Alert_Radius"))
        {
            alertRadius = data.readSingle("Alert_Radius");
        }
        else
        {
            alertRadius = 48f;
        }
        if (data.has("Range_Rangefinder"))
        {
            rangeRangefinder = data.readSingle("Range_Rangefinder");
        }
        else
        {
            rangeRangefinder = data.readSingle("Range");
        }
        instakillHeadshots = data.readBoolean("Instakill_Headshots");
        infiniteAmmo = data.readBoolean("Infinite_Ammo");
        ammoPerShot = data.readByte("Ammo_Per_Shot", 1);
        fireDelay = Mathf.RoundToInt(data.readSingle("Fire_Delay_Seconds") * (float)PlayerInput.TOCK_PER_SECOND);
        allowMagazineChange = data.readBoolean("Allow_Magazine_Change", defaultValue: true);
        canAimDuringSprint = data.readBoolean("Can_Aim_During_Sprint");
        aimingMovementSpeedMultiplier = data.readSingle("Aiming_Movement_Speed_Multiplier", canAimDuringSprint ? 1f : 0.75f);
        canEverJam = data.has("Can_Ever_Jam");
        if (canEverJam)
        {
            jamQualityThreshold = data.readSingle("Jam_Quality_Threshold", 0.4f);
            jamMaxChance = data.readSingle("Jam_Max_Chance", 0.1f);
            unjamChamberAnimName = data.readString("Unjam_Chamber_Anim", "UnjamChamber");
        }
        gunshotRolloffDistance = data.readSingle("Gunshot_Rolloff_Distance", (action == EAction.String) ? 16f : ((action != EAction.Rocket) ? 512f : 64f));
        int num6 = data.readInt32("Shoot_Quest_Rewards");
        if (num6 > 0)
        {
            shootQuestRewards = new INPCReward[num6];
            NPCTool.readRewards(data, localization, "Shoot_Quest_Reward_", shootQuestRewards, this);
        }
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
